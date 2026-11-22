using System;

/// <summary>Identifies the kind of effect an action applies to a character.</summary>
public enum ActionEffectType
{
    /// <summary>Restores the target character's Health.</summary>
    RestoreHealth,
    /// <summary>Reduces the target character's Fatigue.</summary>
    RestoreFatigue,
    /// <summary>Restores the target character's Stamina.</summary>
    RestoreStamina,
    /// <summary>Changes the target character's Mood.</summary>
    ChangeMood,
    /// <summary>Adds Gold to the target character.</summary>
    AddGold,
    /// <summary>Revives a defeated character with the configured Health amount.</summary>
    Revive
}

/// <summary>Defines the action effect data and its validation rules.</summary>
public sealed class ActionEffectDefinition
{
    /// <summary>Gets the type.</summary>
    public ActionEffectType Type { get; }
    /// <summary>Gets the amount.</summary>
    public int Amount { get; }

    /// <summary>Initializes a new instance of <see cref="ActionEffectDefinition"/>.</summary>
    /// <param name="type">The type used to initialize the instance.</param>
    /// <param name="amount">The amount used to initialize the instance.</param>
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

/// <summary>Defines the action data and its validation rules.</summary>
public sealed class ActionDefinition
{
    /// <summary>Gets the id.</summary>
    public string Id { get; }
    /// <summary>Gets the label.</summary>
    public string Label { get; }
    /// <summary>Gets the feedback.</summary>
    public string Feedback { get; }
    /// <summary>Gets the duration minutes.</summary>
    public int DurationMinutes { get; }
    /// <summary>Gets the activity met.</summary>
    public double ActivityMet { get; }
    /// <summary>Gets the time modifier.</summary>
    public ActivityTimeModifier TimeModifier { get; }
    /// <summary>Gets the effects.</summary>
    public ActionEffectDefinition[] Effects { get; }
    /// <summary>Gets the estimated duration min minutes.</summary>
    public int? EstimatedDurationMinMinutes { get; }
    /// <summary>Gets the estimated duration max minutes.</summary>
    public int? EstimatedDurationMaxMinutes { get; }
    /// <summary>Gets the required npc id.</summary>
    public string RequiredNpcId { get; }

    /// <summary>Initializes a new instance of <see cref="ActionDefinition"/>.</summary>
    /// <param name="id">The id used to initialize the instance.</param>
    /// <param name="label">The label used to initialize the instance.</param>
    /// <param name="durationMinutes">The duration minutes used to initialize the instance.</param>
    /// <param name="timeModifier">The time modifier used to initialize the instance.</param>
    /// <param name="feedback">The feedback used to initialize the instance.</param>
    /// <param name="activityMet">The activity met used to initialize the instance.</param>
    /// <param name="effects">The effects used to initialize the instance.</param>
    /// <param name="estimatedDurationMinMinutes">The estimated duration min minutes used to initialize the instance.</param>
    /// <param name="estimatedDurationMaxMinutes">The estimated duration max minutes used to initialize the instance.</param>
    /// <param name="requiredNpcId">The required npc id used to initialize the instance.</param>
    public ActionDefinition(
        string id,
        string label,
        int durationMinutes,
        ActivityTimeModifier timeModifier = null,
        string feedback = "",
        double activityMet = 1.0,
        ActionEffectDefinition[] effects = null,
        int? estimatedDurationMinMinutes = null,
        int? estimatedDurationMaxMinutes = null,
        string requiredNpcId = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Action ID is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Action label is required.", nameof(label));
        if (durationMinutes < 0) throw new ArgumentOutOfRangeException(nameof(durationMinutes));
        if (double.IsNaN(activityMet) || double.IsInfinity(activityMet) || activityMet < 0 || activityMet > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(activityMet));
        }
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

        Id = id;
        Label = label;
        DurationMinutes = durationMinutes;
        ActivityMet = activityMet;
        TimeModifier = timeModifier;
        Effects = effects ?? Array.Empty<ActionEffectDefinition>();
        Feedback = feedback ?? string.Empty;
        EstimatedDurationMinMinutes = estimatedDurationMinMinutes;
        EstimatedDurationMaxMinutes = estimatedDurationMaxMinutes;
        RequiredNpcId = string.IsNullOrWhiteSpace(requiredNpcId) ? null : requiredNpcId;
    }
}
