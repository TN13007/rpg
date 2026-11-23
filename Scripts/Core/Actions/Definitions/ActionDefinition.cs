using System;
using System.Collections.Generic;

/// <summary>Identifies the kind of effect an action applies to a character.</summary>
public enum ActionEffectType
{
    /// <summary>Restores the target character's Health.</summary>
    RestoreHealth,
    /// <summary>Reduces the target character's Health.</summary>
    ReduceHealth,
    /// <summary>Increases the target character's Fatigue.</summary>
    IncreaseFatigue,
    /// <summary>Decreases the target character's Fatigue.</summary>
    DecreaseFatigue,
    /// <summary>Restores the target character's Stamina.</summary>
    RestoreStamina,
    /// <summary>Reduces the target character's Stamina.</summary>
    ReduceStamina,
    /// <summary>Restores the target character's Mood.</summary>
    RestoreMood,
    /// <summary>Reduces the target character's Mood.</summary>
    ReduceMood,
    /// <summary>Adds Gold to the target character.</summary>
    AddGold,
    /// <summary>Reduces Gold from the target character.</summary>
    ReduceGold,
    /// <summary>Revives a defeated character with the configured Health amount.</summary>
    Revive
}

/// <summary>Defines immutable authored action-effect data and its validation rules.</summary>
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

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Effect amount must be greater than zero.");
        }

        Type = type;
        Amount = amount;
    }
}

/// <summary>Defines immutable authored action data and its validation rules.</summary>
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
    public IReadOnlyList<ActionEffectDefinition> Effects { get; }
    /// <summary>Gets the estimated duration min minutes.</summary>
    public int? EstimatedDurationMinMinutes { get; }
    /// <summary>Gets the estimated duration max minutes.</summary>
    public int? EstimatedDurationMaxMinutes { get; }
    /// <summary>Gets the required npc id.</summary>
    public string RequiredNpcId { get; }
    /// <summary>Gets the first minute of the day when the action is available, inclusive.</summary>
    public int? AvailableStartMinute { get; }
    /// <summary>Gets the minute of the day when the action stops being available, exclusive.</summary>
    public int? AvailableEndMinute { get; }

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
    /// <param name="availableStartMinute">The inclusive start minute of an optional daily availability window.</param>
    /// <param name="availableEndMinute">The exclusive end minute of an optional daily availability window.</param>
    public ActionDefinition(
        string id,
        string label,
        int durationMinutes,
        ActivityTimeModifier timeModifier = null,
        string feedback = "",
        double activityMet = 1.0,
        IEnumerable<ActionEffectDefinition> effects = null,
        int? estimatedDurationMinMinutes = null,
        int? estimatedDurationMaxMinutes = null,
        string requiredNpcId = null,
        int? availableStartMinute = null,
        int? availableEndMinute = null)
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

        if (availableStartMinute.HasValue != availableEndMinute.HasValue)
        {
            throw new ArgumentException("Both action availability window bounds must be provided together.");
        }
        if (availableStartMinute.HasValue &&
            (availableStartMinute.Value < 0 ||
             availableStartMinute.Value >= 1440 ||
             availableEndMinute.Value <= availableStartMinute.Value ||
             availableEndMinute.Value > 1440))
        {
            throw new ArgumentOutOfRangeException(nameof(availableStartMinute));
        }

        Id = id;
        Label = label;
        DurationMinutes = durationMinutes;
        ActivityMet = activityMet;
        TimeModifier = timeModifier;
        Effects = Array.AsReadOnly(effects == null ? Array.Empty<ActionEffectDefinition>() : new List<ActionEffectDefinition>(effects).ToArray());
        Feedback = feedback ?? string.Empty;
        EstimatedDurationMinMinutes = estimatedDurationMinMinutes;
        EstimatedDurationMaxMinutes = estimatedDurationMaxMinutes;
        RequiredNpcId = string.IsNullOrWhiteSpace(requiredNpcId) ? null : requiredNpcId;
        AvailableStartMinute = availableStartMinute;
        AvailableEndMinute = availableEndMinute;
    }

    /// <summary>Determines whether the action is available at the supplied minute of the day.</summary>
    /// <param name="minuteOfDay">The current minute after midnight, or null when the current time is unknown.</param>
    /// <returns>True when no window is authored or the supplied time is inside the authored window.</returns>
    public bool IsAvailableAt(int? minuteOfDay)
    {
        if (!AvailableStartMinute.HasValue)
        {
            return true;
        }

        if (!minuteOfDay.HasValue)
        {
            return false;
        }

        return minuteOfDay.Value >= AvailableStartMinute.Value
            && minuteOfDay.Value < AvailableEndMinute.Value;
    }
}
