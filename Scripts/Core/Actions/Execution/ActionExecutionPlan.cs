using System;

/// <summary>Contains a validated Action execution and its immutable proposed state change.</summary>
public sealed class ActionExecutionPlan
{
    internal Character _actor { get; }
    internal GameClock _clock { get; }
    internal ActionDefinition _action { get; }
    internal CharacterRuntimeState _preparedCharacterState { get; }
    internal GameClockState _preparedClockState { get; }
    internal bool _isApplied { get; private set; }
    internal ActionStateChange _stateChange { get; }

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
        CharacterRuntimeState preparedCharacterState,
        GameClockState preparedClockState,
        ActionStateChange stateChange)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
        _actor = actor ?? throw new ArgumentNullException(nameof(actor));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _preparedCharacterState = preparedCharacterState ?? throw new ArgumentNullException(nameof(preparedCharacterState));
        _preparedClockState = preparedClockState ?? throw new ArgumentNullException(nameof(preparedClockState));
        _stateChange = stateChange ?? throw new ArgumentNullException(nameof(stateChange));
        ActualDurationMinutes = actualDurationMinutes;
        FatigueChange = fatigueChange;
    }

    /// <summary>Reports whether the runtime owners still match the state used to prepare this plan.</summary>
    internal bool MatchesPreparedState()
    {
        return _clock.CaptureState().Equals(_preparedClockState)
            && _actor.CaptureRuntimeState().Equals(_preparedCharacterState);
    }

    /// <summary>Marks this plan as committed so it cannot be applied more than once.</summary>
    internal void MarkApplied()
    {
        _isApplied = true;
    }
}
