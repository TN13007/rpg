using System;

/// <summary>Coordinates action execution behavior and applies its domain rules.</summary>
public sealed class ActionExecutionService
{
    private readonly ActivityTimeCalculator _timeCalculator;
    private readonly ActivityFatigueCalculator _fatigueCalculator;
    private readonly ActionEffectExecutor _effectExecutor;

    /// <summary>Initializes a new instance of <see cref="ActionExecutionService"/>.</summary>
    /// <param name="timeCalculator">The time calculator used to initialize the instance.</param>
    /// <param name="effectExecutor">The effect executor used to initialize the instance.</param>
    /// <param name="fatigueCalculator">The fatigue calculator used to initialize the instance.</param>
    public ActionExecutionService(
        ActivityTimeCalculator timeCalculator,
        ActionEffectExecutor effectExecutor,
        ActivityFatigueCalculator fatigueCalculator = null)
    {
        _timeCalculator = timeCalculator ?? throw new ArgumentNullException(nameof(timeCalculator));
        _effectExecutor = effectExecutor ?? throw new ArgumentNullException(nameof(effectExecutor));
        _fatigueCalculator = fatigueCalculator ?? new ActivityFatigueCalculator();
    }

    /// <summary>Determines whether execute.</summary>
    /// <param name="action">The action used by this operation.</param>
    /// <param name="actor">The actor used by this operation.</param>
    /// <param name="failureReason">The failure reason used by this operation.</param>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
    public bool CanExecute(ActionDefinition action, Character actor, out string failureReason)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (actor == null)
        {
            throw new ArgumentNullException(nameof(actor));
        }

        bool hasReviveEffect = HasReviveEffect(action);
        bool isRecoveryAction = IsRecoveryAction(action);
        if (actor.IsDead && !isRecoveryAction)
        {
            failureReason = "You are dead and cannot perform actions.";
            return false;
        }

        if (actor.IsAlive && hasReviveEffect)
        {
            failureReason = "This recovery action is only available while incapacitated.";
            return false;
        }

        failureReason = string.Empty;
        return true;
    }

    private static bool HasReviveEffect(ActionDefinition action)
    {
        foreach (ActionEffectDefinition effect in action.Effects)
        {
            if (effect.Type == ActionEffectType.Revive)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsRecoveryAction(ActionDefinition action)
    {
        return action.Effects.Length == 1
            && action.Effects[0].Type == ActionEffectType.Revive
            && action.Effects[0].Amount > 0;
    }

    /// <summary>Attempts to execute and reports whether the operation succeeded.</summary>
    /// <param name="action">The action used by this operation.</param>
    /// <param name="actor">The actor used by this operation.</param>
    /// <param name="gameClock">The game clock used by this operation.</param>
    /// <param name="failureReason">The failure reason used by this operation.</param>
    /// <param name="actualDuration">The actual duration used by this operation.</param>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
    public bool TryExecute(
        ActionDefinition action,
        Character actor,
        GameClock gameClock,
        out string failureReason,
        out int actualDuration)
    {
        if (gameClock == null)
        {
            throw new ArgumentNullException(nameof(gameClock));
        }

        if (!CanExecute(action, actor, out failureReason))
        {
            actualDuration = 0;
            return false;
        }

        actualDuration = _timeCalculator.Calculate(action, actor);
        gameClock.AdvanceMinutes(actualDuration);

        actor.AddFatigue(_fatigueCalculator.Calculate(action, actualDuration));
        _effectExecutor.Apply(action, actor);
        failureReason = string.Empty;
        return true;
    }
}
