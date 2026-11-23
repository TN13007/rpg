using System;

/// <summary>Coordinates Action preparation and commits the prepared runtime changes.</summary>
public sealed class ActionExecutionService
{
    private readonly ActivityTimeCalculator _timeCalculator;
    private readonly ActivityFatigueCalculator _fatigueCalculator;
    private readonly ActionEffectExecutor _effectExecutor;

    /// <summary>Creates the Action execution service with its focused calculators and effect handler.</summary>
    /// <param name="timeCalculator">Calculates the actual duration of an authored Action.</param>
    /// <param name="effectExecutor">Prepares and applies authored Action effects.</param>
    /// <param name="fatigueCalculator">Calculates Fatigue gained from an activity; a default is created when omitted.</param>
    public ActionExecutionService(
        ActivityTimeCalculator timeCalculator,
        ActionEffectExecutor effectExecutor,
        ActivityFatigueCalculator fatigueCalculator = null)
    {
        _timeCalculator = timeCalculator ?? throw new ArgumentNullException(nameof(timeCalculator));
        _effectExecutor = effectExecutor ?? throw new ArgumentNullException(nameof(effectExecutor));
        _fatigueCalculator = fatigueCalculator ?? new ActivityFatigueCalculator();
    }

    /// <summary>Checks whether the actor is allowed to execute the Action in the actor's current state.</summary>
    /// <param name="action">The authored Action to validate.</param>
    /// <param name="actor">The Character attempting to perform the Action.</param>
    /// <param name="failureReason">Receives an explanation when the actor cannot perform the Action.</param>
    /// <returns>True when the actor may execute the Action; otherwise false.</returns>
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

    /// <summary>Prepares an Action without mutating the Character or GameClock.</summary>
    /// <param name="action">The authored Action to prepare.</param>
    /// <param name="actor">The Character attempting to perform the Action.</param>
    /// <param name="gameClock">The shared world clock that would be advanced on commit.</param>
    /// <param name="plan">Receives the immutable execution plan when preparation succeeds.</param>
    /// <param name="failureReason">Receives the reason preparation was rejected.</param>
    /// <returns>True when a complete execution plan was prepared; otherwise false.</returns>
    public bool TryPrepare(
        ActionDefinition action,
        Character actor,
        GameClock gameClock,
        out ActionExecutionPlan plan,
        out string failureReason)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (actor == null)
        {
            throw new ArgumentNullException(nameof(actor));
        }

        if (gameClock == null)
        {
            throw new ArgumentNullException(nameof(gameClock));
        }

        plan = null;
        if (!CanExecute(action, actor, out failureReason))
        {
            return false;
        }

        int actualDuration = _timeCalculator.Calculate(action, actor);
        if (!gameClock.CanAdvanceMinutes(actualDuration))
        {
            failureReason = "The GameClock cannot advance by the calculated Action duration.";
            return false;
        }

        double fatigueChange = _fatigueCalculator.Calculate(action, actualDuration);
        ActionEffectPlan effectPlan;
        try
        {
            effectPlan = _effectExecutor.Prepare(action);
        }
        catch (InvalidOperationException exception)
        {
            failureReason = exception.Message;
            return false;
        }

        plan = new ActionExecutionPlan(
            action,
            actor,
            gameClock,
            actualDuration,
            fatigueChange,
            effectPlan);
        failureReason = string.Empty;
        return true;
    }

    /// <summary>Commits a prepared Action when its source state is still unchanged.</summary>
    /// <param name="plan">The execution plan produced by <see cref="TryPrepare"/>.</param>
    /// <param name="failureReason">Receives the reason the plan could not be committed.</param>
    /// <returns>True when the plan was applied; otherwise false without changing runtime state.</returns>
    public bool TryApply(ActionExecutionPlan plan, out string failureReason)
    {
        if (plan == null)
        {
            throw new ArgumentNullException(nameof(plan));
        }

        if (plan._isApplied)
        {
            failureReason = "This Action execution plan has already been applied.";
            return false;
        }

        if (!plan.MatchesPreparedState())
        {
            failureReason = "The Character or GameClock changed after the Action was prepared.";
            return false;
        }

        if (!CanExecute(plan._action, plan._actor, out failureReason))
        {
            return false;
        }

        if (!plan._clock.CanAdvanceMinutes(plan.ActualDurationMinutes))
        {
            failureReason = "The GameClock can no longer advance by the prepared Action duration.";
            return false;
        }

        // All checks and effect validation are complete. These synchronous domain mutations
        // use already-validated values and have no expected failure path.
        plan._clock.AdvanceMinutes(plan.ActualDurationMinutes);
        plan._actor.AddFatigue(plan.FatigueChange);
        _effectExecutor.Apply(plan._effectPlan, plan._actor);
        plan.MarkApplied();

        failureReason = string.Empty;
        return true;
    }

    /// <summary>Prepares and commits an Action, preserving the existing execution entry point.</summary>
    /// <param name="action">The authored Action to execute.</param>
    /// <param name="actor">The Character attempting to perform the Action.</param>
    /// <param name="gameClock">The shared world clock to advance.</param>
    /// <param name="failureReason">Receives an explanation when execution is rejected.</param>
    /// <param name="actualDuration">Receives the committed duration, or zero when execution fails.</param>
    /// <returns>True when the Action was executed; otherwise false.</returns>
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

        if (!TryPrepare(action, actor, gameClock, out ActionExecutionPlan plan, out failureReason))
        {
            actualDuration = 0;
            return false;
        }

        if (!TryApply(plan, out failureReason))
        {
            actualDuration = 0;
            return false;
        }

        actualDuration = plan.ActualDurationMinutes;
        return true;
    }

    private static bool HasReviveEffect(ActionDefinition action)
    {
        foreach (ActionEffectDefinition effect in action.Effects)
        {
            if (effect != null && effect.Type == ActionEffectType.Revive)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsRecoveryAction(ActionDefinition action)
    {
        return action.Effects.Count == 1
            && action.Effects[0] != null
            && action.Effects[0].Type == ActionEffectType.Revive
            && action.Effects[0].Amount > 0;
    }
}
