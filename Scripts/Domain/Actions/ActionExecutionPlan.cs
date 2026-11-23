using System;

/// <summary>Contains a validated Action execution prepared for a single synchronous commit.</summary>
public sealed class ActionExecutionPlan
{
    internal Character _actor { get; }
    internal GameClock _clock { get; }
    internal ActionDefinition _action { get; }
    internal ActionEffectPlan _effectPlan { get; }
    internal int _preparedDay { get; }
    internal int _preparedMinutesOfDay { get; }
    internal int _preparedHealth { get; }
    internal double _preparedFatigue { get; }
    internal int _preparedStamina { get; }
    internal int _preparedMood { get; }
    internal int _preparedGold { get; }
    internal bool _isApplied { get; private set; }

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
        _action = action ?? throw new ArgumentNullException(nameof(action));
        _actor = actor ?? throw new ArgumentNullException(nameof(actor));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _effectPlan = effectPlan ?? throw new ArgumentNullException(nameof(effectPlan));
        ActualDurationMinutes = actualDurationMinutes;
        FatigueChange = fatigueChange;

        _preparedDay = clock.Day;
        _preparedMinutesOfDay = clock.MinutesOfDay;
        _preparedHealth = actor.Health;
        _preparedFatigue = actor.Fatigue;
        _preparedStamina = actor.Stamina;
        _preparedMood = actor.Mood;
        _preparedGold = actor.Gold;
    }

    /// <summary>Reports whether the runtime objects still match the state used to prepare this plan.</summary>
    internal bool MatchesPreparedState()
    {
        return _clock.Day == _preparedDay
            && _clock.MinutesOfDay == _preparedMinutesOfDay
            && _actor.Health == _preparedHealth
            && _actor.Fatigue.Equals(_preparedFatigue)
            && _actor.Stamina == _preparedStamina
            && _actor.Mood == _preparedMood
            && _actor.Gold == _preparedGold;
    }

    /// <summary>Marks this plan as committed so it cannot be applied more than once.</summary>
    internal void MarkApplied()
    {
        _isApplied = true;
    }
}
