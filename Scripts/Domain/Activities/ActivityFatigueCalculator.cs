using System;

public sealed class ActivityFatigueCalculator
{
    private const double _restingMet = 1.0;
    private const double _minutesPerFatigueUnit = 15.0;

    public double Calculate(double activityMet, int actualDurationMinutes)
    {
        if (double.IsNaN(activityMet) || double.IsInfinity(activityMet) || activityMet < 0 || activityMet > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(activityMet), "Activity MET must be between 0 and 20.");
        }

        if (actualDurationMinutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(actualDurationMinutes));
        }

        double relativeIntensity = Math.Max(0, activityMet - _restingMet);
        return relativeIntensity * actualDurationMinutes / _minutesPerFatigueUnit;
    }

    public double Calculate(ActionDefinition action, int actualDurationMinutes)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return Calculate(action.ActivityMet, actualDurationMinutes);
    }
}
