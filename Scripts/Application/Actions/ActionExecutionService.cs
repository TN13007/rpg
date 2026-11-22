using System;

/// <summary>Coordinates action execution behavior and applies its domain rules.</summary>
public sealed class ActionExecutionService
{
    private readonly ActivityTimeCalculator _timeCalculator;
    private readonly ActivityFatigueCalculator _fatigueCalculator;
    private readonly ActionEffectExecutor _effectExecutor;

    /// <summary>Creates the action executor with the calculators and effect handler used by Exploration actions.</summary>
    /// <param name="timeCalculator">Calculates the actual duration of an authored action for its actor.</param>
    /// <param name="effectExecutor">Applies the action's configured effects to the actor.</param>
    /// <param name="fatigueCalculator">Calculates Fatigue gained from the executed action; a default calculator is created when omitted.</param>
    public ActionExecutionService(
        ActivityTimeCalculator timeCalculator,
        ActionEffectExecutor effectExecutor,
        ActivityFatigueCalculator fatigueCalculator = null)
    {
        _timeCalculator = timeCalculator ?? throw new ArgumentNullException(nameof(timeCalculator));
        _effectExecutor = effectExecutor ?? throw new ArgumentNullException(nameof(effectExecutor));
        _fatigueCalculator = fatigueCalculator ?? new ActivityFatigueCalculator();
    }

    /// <summary>Checks whether the actor is allowed to execute the action in the actor's current state.</summary>
    /// <param name="action">The authored action to validate.</param>
    /// <param name="actor">The Character attempting to perform the action.</param>
    /// <param name="failureReason">Receives an explanation when the actor cannot perform the action; empty when the action is allowed.</param>
    /// <returns><see langword="true"/> when the actor may execute the action; otherwise, <see langword="false"/>.</returns>
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
        return action.Effects.Count == 1
            && action.Effects[0].Type == ActionEffectType.Revive
            && action.Effects[0].Amount > 0;
    }

    /// <summary>Validates and executes an action, advancing world time and applying its Fatigue and effects.</summary>
    /// <param name="action">The action used by this operation.</param>
    /// <param name="actor">The actor used by this operation.</param>
    /// <param name="gameClock">The shared world clock to advance by the action's calculated duration.</param>
    /// <param name="failureReason">The failure reason used by this operation.</param>
    /// <param name="actualDuration">Receives the calculated duration in game minutes, or zero when validation fails.</param>
    /// <returns><see langword="true"/> when the action is executed; otherwise, <see langword="false"/>.</returns>
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

        // Availability and earlier preflight checks are advisory; actor state may have changed.
        // Revalidate immediately before calculating time or mutating any runtime state.
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
