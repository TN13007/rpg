using System;

/// <summary>Commits a fully prepared Action state through the authoritative state owners.</summary>
public sealed class ActionStateChangeApplier
{
    /// <summary>Checks both owners before writing either prepared state.</summary>
    /// <summary>Checks whether both authoritative owners can accept the proposed state.</summary>
    /// <param name="change">The proposed state change.</param>
    /// <param name="actor">The Character state owner.</param>
    /// <param name="clock">The GameClock state owner.</param>
    /// <returns>True when both snapshots satisfy their owners' invariants.</returns>
    public bool CanApply(ActionStateChange change, Character actor, GameClock clock)
    {
        if (change == null) throw new ArgumentNullException(nameof(change));
        if (actor == null) throw new ArgumentNullException(nameof(actor));
        if (clock == null) throw new ArgumentNullException(nameof(clock));
        return actor.CanApplyRuntimeState(change.CharacterState)
            && clock.CanApplyState(change.ClockState);
    }

    /// <summary>Writes a previously validated change to Character and GameClock.</summary>
    /// <summary>Commits a validated state change through both authoritative owners.</summary>
    /// <param name="change">The proposed state change.</param>
    /// <param name="actor">The Character state owner.</param>
    /// <param name="clock">The GameClock state owner.</param>
    public void Apply(ActionStateChange change, Character actor, GameClock clock)
    {
        if (!CanApply(change, actor, clock))
            throw new InvalidOperationException("The prepared Action state is not valid for its owners.");
        actor.ApplyRuntimeState(change.CharacterState);
        clock.ApplyState(change.ClockState);
    }
}
