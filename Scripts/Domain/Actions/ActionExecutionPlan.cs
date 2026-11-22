using System;

/// <summary>Contains a validated Action execution prepared for a single synchronous commit.</summary>
public sealed class ActionExecutionPlan
{
    internal Character Actor { get; }
    internal GameClock Clock { get; }
    internal ActionDefinition Action { get; }
    internal ActionEffectPlan EffectPlan { get; }
    internal int PreparedDay { get; }
    internal int PreparedMinutesOfDay { get; }
    internal int PreparedHealth { get; }
    internal double PreparedFatigue { get; }
    internal int PreparedStamina { get; }
    internal int PreparedMood { get; }
    internal int PreparedGold { get; }
    internal bool IsApplied { get; private set; }

    /// <summary>Gets the duration that will be applied when this plan is committed.</summary>
    public int ActualDurationMinutes { get; }

    /// <summary>Gets the Fatigue change calculated for this execution.</summary>
    public double FatigueChange { get; }

    internal ActionExecutionPlan(
        ActionDefinition action,
        Character actor,
        GameClock clock,
        int actualDurationMinutes,
        double fatigueChange,
        ActionEffectPlan effectPlan)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
        Actor = actor ?? throw new ArgumentNullException(nameof(actor));
        Clock = clock ?? throw new ArgumentNullException(nameof(clock));
        EffectPlan = effectPlan ?? throw new ArgumentNullException(nameof(effectPlan));
        ActualDurationMinutes = actualDurationMinutes;
        FatigueChange = fatigueChange;

        PreparedDay = clock.Day;
        PreparedMinutesOfDay = clock.MinutesOfDay;
        PreparedHealth = actor.Health;
        PreparedFatigue = actor.Fatigue;
        PreparedStamina = actor.Stamina;
        PreparedMood = actor.Mood;
        PreparedGold = actor.Gold;
    }

    /// <summary>Reports whether the runtime objects still match the state used to prepare this plan.</summary>
    internal bool MatchesPreparedState()
    {
        return Clock.Day == PreparedDay
            && Clock.MinutesOfDay == PreparedMinutesOfDay
            && Actor.Health == PreparedHealth
            && Actor.Fatigue.Equals(PreparedFatigue)
            && Actor.Stamina == PreparedStamina
            && Actor.Mood == PreparedMood
            && Actor.Gold == PreparedGold;
    }

    /// <summary>Marks this plan as committed so it cannot be applied more than once.</summary>
    internal void MarkApplied()
    {
        IsApplied = true;
    }
}
