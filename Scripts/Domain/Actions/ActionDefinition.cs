using System;

public enum ActionTimeMode
{
    Duration,
    NextDay
}

public enum ActionEffectType
{
    RestoreHealth,
    RestoreFatigue,
    RestoreStamina,
    ChangeMood,
    AddGold
}

public sealed class ActionEffectDefinition
{
    public ActionEffectType Type { get; }
    public int Amount { get; }

    public ActionEffectDefinition(ActionEffectType type, int amount)
    {
        if (!Enum.IsDefined(typeof(ActionEffectType), type))
        {
            throw new ArgumentOutOfRangeException(nameof(type));
        }

        if (type != ActionEffectType.ChangeMood && amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "This effect amount cannot be negative.");
        }

        Type = type;
        Amount = amount;
    }
}

public sealed class ActionDefinition
{
    public string Id { get; }
    public string Label { get; }
    public string Feedback { get; }
    public int DurationMinutes { get; }
    public double ActivityMet { get; }
    public ActivityTimeModifier TimeModifier { get; }
    public ActionTimeMode TimeMode { get; }
    public ActionEffectDefinition[] Effects { get; }
    public int? EstimatedDurationMinMinutes { get; }
    public int? EstimatedDurationMaxMinutes { get; }

    public ActionDefinition(
        string id,
        string label,
        int durationMinutes,
        ActivityTimeModifier timeModifier = null,
        string feedback = "",
        double activityMet = 1.0,
        ActionEffectDefinition[] effects = null,
        ActionTimeMode timeMode = ActionTimeMode.Duration,
        int? estimatedDurationMinMinutes = null,
        int? estimatedDurationMaxMinutes = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Action ID is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Action label is required.", nameof(label));
        if (durationMinutes < 0) throw new ArgumentOutOfRangeException(nameof(durationMinutes));
        if (double.IsNaN(activityMet) || double.IsInfinity(activityMet) || activityMet < 0 || activityMet > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(activityMet));
        }
        if (!Enum.IsDefined(typeof(ActionTimeMode), timeMode)) throw new ArgumentOutOfRangeException(nameof(timeMode));
        if (estimatedDurationMinMinutes.HasValue != estimatedDurationMaxMinutes.HasValue)
        {
            throw new ArgumentException("Both estimated duration bounds must be provided together.");
        }
        if (estimatedDurationMinMinutes.HasValue &&
            (estimatedDurationMinMinutes.Value < 0 ||
             estimatedDurationMaxMinutes.Value < estimatedDurationMinMinutes.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(estimatedDurationMinMinutes));
        }
        if (timeMode == ActionTimeMode.NextDay &&
            (estimatedDurationMinMinutes.HasValue || estimatedDurationMaxMinutes.HasValue))
        {
            throw new ArgumentException("NextDay actions cannot define a duration estimate.");
        }

        Id = id;
        Label = label;
        DurationMinutes = durationMinutes;
        ActivityMet = activityMet;
        TimeModifier = timeModifier;
        TimeMode = timeMode;
        Effects = effects ?? Array.Empty<ActionEffectDefinition>();
        Feedback = feedback ?? string.Empty;
        EstimatedDurationMinMinutes = estimatedDurationMinMinutes;
        EstimatedDurationMaxMinutes = estimatedDurationMaxMinutes;
    }
}
