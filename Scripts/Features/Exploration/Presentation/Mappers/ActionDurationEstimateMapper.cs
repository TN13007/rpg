using System;

/// <summary>Maps application data into action duration estimate presentation models.</summary>
public sealed class ActionDurationEstimateMapper
{
    private readonly ActivityTimeCalculator _timeCalculator;

    /// <summary>Initializes a new instance of <see cref="ActionDurationEstimateMapper"/>.</summary>
    /// <param name="timeCalculator">The time calculator used to initialize the instance.</param>
    public ActionDurationEstimateMapper(ActivityTimeCalculator timeCalculator)
    {
        _timeCalculator = timeCalculator ?? throw new ArgumentNullException(nameof(timeCalculator));
    }

    /// <summary>Maps the supplied map data into presentation data.</summary>
    /// <param name="action">The action used by this operation.</param>
    /// <param name="actor">The actor used by this operation.</param>
    /// <returns>The resulting string.</returns>
    public string Map(ActionDefinition action, Character actor)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (actor == null) throw new ArgumentNullException(nameof(actor));

        if (action.DurationMinutes == 0)
        {
            return "Immediate";
        }

        if (action.EstimatedDurationMinMinutes.HasValue)
        {
            int minimum = CalculateDuration(action, actor, action.EstimatedDurationMinMinutes.Value);
            int maximum = CalculateDuration(action, actor, action.EstimatedDurationMaxMinutes.Value);

            if (minimum == maximum)
            {
                return FormatDuration(minimum);
            }

            return FormatDuration(minimum) + "–" + FormatDuration(maximum);
        }

        return FormatDuration(_timeCalculator.Calculate(action, actor));
    }

    private int CalculateDuration(ActionDefinition action, Character actor, int baseDuration)
    {
        ActionDefinition estimate = new ActionDefinition(
            "duration_estimate",
            "Duration estimate",
            baseDuration,
            action.TimeModifier);

        return _timeCalculator.Calculate(estimate, actor);
    }

    private string FormatDuration(int minutes)
    {
        int hours = minutes / 60;
        int remainingMinutes = minutes % 60;
        return hours.ToString("0") + ":" + remainingMinutes.ToString("00");
    }
}
