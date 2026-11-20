using System;
using System.Collections.Generic;

public sealed class CombatTimeline
{
    private readonly Dictionary<string, CombatantTimelineState> _combatants =
        new Dictionary<string, CombatantTimelineState>(StringComparer.Ordinal);

    public double CombatTime { get; private set; }

    public void AddCombatant(
        string combatantId,
        int luck,
        double baseSpeed,
        double initialProgress)
    {
        CombatantTimelineState state = new CombatantTimelineState(
            combatantId,
            luck,
            baseSpeed,
            initialProgress);

        if (!_combatants.TryAdd(combatantId, state))
            throw new ArgumentException("Duplicate combatant ID: " + combatantId, nameof(combatantId));
    }

    public CombatantTimelineState GetCombatant(string combatantId)
    {
        if (string.IsNullOrWhiteSpace(combatantId))
            throw new ArgumentException("Combatant ID is required.", nameof(combatantId));
        if (!_combatants.TryGetValue(combatantId, out CombatantTimelineState state))
            throw new KeyNotFoundException("Combatant was not found: " + combatantId);
        return state;
    }

    public void ApplyStun(string combatantId, double duration)
    {
        if (double.IsNaN(duration) || double.IsInfinity(duration) || duration <= 0)
            throw new ArgumentOutOfRangeException(nameof(duration));

        CombatantTimelineState state = GetCombatant(combatantId);
        state.ApplyStun(CombatTime + duration);
    }

    public void ApplyRecovery(string combatantId, double recoveryCost)
    {
        GetCombatant(combatantId).SetRecoveryCost(recoveryCost);
    }

    public string GetNextReadyCombatantId()
    {
        CombatantTimelineState selected = null;
        foreach (CombatantTimelineState candidate in _combatants.Values)
        {
            if (!candidate.CanAct(CombatTime))
                continue;

            if (selected == null ||
                candidate.Luck > selected.Luck ||
                (candidate.Luck == selected.Luck &&
                 string.CompareOrdinal(candidate.CombatantId, selected.CombatantId) < 0))
            {
                selected = candidate;
            }
        }

        return selected == null ? string.Empty : selected.CombatantId;
    }

    public double? GetTimeUntilNextEvent()
    {
        double? nextEventTime = null;

        foreach (CombatantTimelineState state in _combatants.Values)
        {
            if (state.IsStunned(CombatTime))
            {
                double stunRemaining = state.StunEndTime.Value - CombatTime;
                if (!nextEventTime.HasValue || stunRemaining < nextEventTime.Value)
                    nextEventTime = stunRemaining;
                continue;
            }

            if (state.RemainingProgress <= 0)
                continue;

            double timeToReady = state.RemainingProgress / state.BaseSpeed;
            if (!nextEventTime.HasValue || timeToReady < nextEventTime.Value)
                nextEventTime = timeToReady;
        }

        return nextEventTime;
    }

    public bool AdvanceToNextEvent()
    {
        double? elapsed = GetTimeUntilNextEvent();
        if (!elapsed.HasValue)
            return false;

        AdvanceTime(elapsed.Value);
        return true;
    }

    public void AdvanceTime(double elapsedCombatTime)
    {
        if (double.IsNaN(elapsedCombatTime) ||
            double.IsInfinity(elapsedCombatTime) ||
            elapsedCombatTime < 0)
            throw new ArgumentOutOfRangeException(nameof(elapsedCombatTime));

        foreach (CombatantTimelineState state in _combatants.Values)
        {
            double activeElapsed = elapsedCombatTime;
            if (state.IsStunned(CombatTime))
            {
                double stunnedElapsed = state.StunEndTime.Value - CombatTime;
                activeElapsed = Math.Max(0, elapsedCombatTime - stunnedElapsed);
            }

            if (activeElapsed > 0)
                state.AdvanceProgress(activeElapsed, state.BaseSpeed);
        }

        CombatTime += elapsedCombatTime;
    }
}
