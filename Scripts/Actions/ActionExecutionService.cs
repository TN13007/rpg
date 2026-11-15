using System;

public sealed class ActionExecutionService
{
    private readonly ActivityTimeCalculator _timeCalculator;
    private readonly ActionEffectExecutor _effectExecutor;

    public ActionExecutionService(
        ActivityTimeCalculator timeCalculator,
        ActionEffectExecutor effectExecutor)
    {
        _timeCalculator = timeCalculator ?? throw new ArgumentNullException(nameof(timeCalculator));
        _effectExecutor = effectExecutor ?? throw new ArgumentNullException(nameof(effectExecutor));
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

        if (!actor.IsAlive)
        {
            failureReason = "You are dead and cannot perform actions.";
            return false;
        }

        if (actor.Energy < action.EnergyCost)
        {
            failureReason = "You do not have enough Energy to perform this action.";
            return false;
        }

        failureReason = string.Empty;
        return true;
    }

    public bool TryExecute(
        ActionDefinition action,
        Character actor,
        DayClock dayClock,
        out string failureReason,
        out int actualDuration)
    {
        if (dayClock == null)
        {
            throw new ArgumentNullException(nameof(dayClock));
        }

        if (!CanExecute(action, actor, out failureReason))
        {
            actualDuration = 0;
            return false;
        }

        actualDuration = action.TimeMode == ActionTimeMode.Duration
            ? _timeCalculator.Calculate(action, actor)
            : 0;

        if (!actor.SpendEnergy(action.EnergyCost))
        {
            failureReason = "You do not have enough Energy to perform this action.";
            actualDuration = 0;
            return false;
        }

        if (action.TimeMode == ActionTimeMode.NextDay)
        {
            dayClock.SleepUntilNextDay();
        }
        else
        {
            dayClock.AdvanceMinutes(actualDuration);
        }

        _effectExecutor.Apply(action, actor);
        failureReason = string.Empty;
        return true;
    }
}
