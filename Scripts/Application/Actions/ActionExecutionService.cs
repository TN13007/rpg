using System;

public sealed class ActionExecutionService
{
    private readonly ActivityTimeCalculator _timeCalculator;
    private readonly ActivityFatigueCalculator _fatigueCalculator;
    private readonly ActionEffectExecutor _effectExecutor;

    public ActionExecutionService(
        ActivityTimeCalculator timeCalculator,
        ActionEffectExecutor effectExecutor,
        ActivityFatigueCalculator fatigueCalculator = null)
    {
        _timeCalculator = timeCalculator ?? throw new ArgumentNullException(nameof(timeCalculator));
        _effectExecutor = effectExecutor ?? throw new ArgumentNullException(nameof(effectExecutor));
        _fatigueCalculator = fatigueCalculator ?? new ActivityFatigueCalculator();
    }

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

        if (actor.IsDead)
        {
            failureReason = "You are dead and cannot perform actions.";
            return false;
        }

        failureReason = string.Empty;
        return true;
    }

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
