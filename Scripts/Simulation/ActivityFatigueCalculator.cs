using System;

public sealed class ActivityFatigueCalculator
{
    private const double RestingMet = 1.0;
    private const double MinutesPerFatigueUnit = 15.0;

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

        double relativeIntensity = Math.Max(0, activityMet - RestingMet);
        return relativeIntensity * actualDurationMinutes / MinutesPerFatigueUnit;
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
