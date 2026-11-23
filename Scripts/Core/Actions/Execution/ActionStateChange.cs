using System;

/// <summary>Immutable proposed state for one successful Action execution.</summary>
public sealed class ActionStateChange
{
    /// <summary>Gets the proposed Character resources.</summary>
    public CharacterRuntimeState CharacterState { get; }
    /// <summary>Gets the proposed world-clock position.</summary>
    public GameClockState ClockState { get; }

    /// <summary>Creates an immutable proposed state for both Action-owned runtime resources.</summary>
    /// <param name="characterState">The proposed Character resources.</param>
    /// <param name="clockState">The proposed clock position.</param>
    public ActionStateChange(CharacterRuntimeState characterState, GameClockState clockState)
    {
        CharacterState = characterState ?? throw new ArgumentNullException(nameof(characterState));
        ClockState = clockState ?? throw new ArgumentNullException(nameof(clockState));
    }
}
