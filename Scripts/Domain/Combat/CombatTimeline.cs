using System;
using System.Collections.Generic;

public sealed class CombatTimeline
{
    private readonly Dictionary<string, CombatantTimelineState> _combatants =
        new Dictionary<string, CombatantTimelineState>(StringComparer.Ordinal);
    private readonly double _progressScale;

    public double CombatTime { get; private set; }

    public CombatTimeline(double progressScale = 1.0)
    {
        if (double.IsNaN(progressScale) ||
            double.IsInfinity(progressScale) ||
            progressScale <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(progressScale));
        }

        _progressScale = progressScale;
    }

    public void AddCombatant(string combatantId, int luck, double baseSpeed, double initialProgress)
    {
        CombatantTimelineState state = new CombatantTimelineState(
            combatantId,
            luck,
            baseSpeed,
            initialProgress * _progressScale);
        if (!_combatants.TryAdd(combatantId, state))
        {
            throw new ArgumentException("Duplicate combatant ID: " + combatantId, nameof(combatantId));
        }
    }

    public bool RemoveCombatant(string combatantId)
    {
        if (string.IsNullOrWhiteSpace(combatantId))
        {
            throw new ArgumentException("Combatant ID is required.", nameof(combatantId));
        }

        return _combatants.Remove(combatantId);
    }

    public bool ContainsCombatant(string combatantId)
    {
        return !string.IsNullOrWhiteSpace(combatantId) && _combatants.ContainsKey(combatantId);
    }

    public CombatantTimelineState GetCombatant(string combatantId)
    {
        if (string.IsNullOrWhiteSpace(combatantId))
        {
            throw new ArgumentException("Combatant ID is required.", nameof(combatantId));
        }

        if (!_combatants.TryGetValue(combatantId, out CombatantTimelineState state))
        {
            throw new KeyNotFoundException("Combatant was not found: " + combatantId);
        }

        return state;
    }

    public void ApplyStun(string combatantId, double duration)
    {
        ValidatePositiveFinite(duration, nameof(duration));
        double endTime = CombatTime + duration;
        if (double.IsInfinity(endTime))
        {
            throw new ArgumentOutOfRangeException(nameof(duration));
        }

        GetCombatant(combatantId).ApplyStun(endTime);
    }

    public void ApplyDelay(string combatantId, double amount)
    {
        if (double.IsNaN(amount) || double.IsInfinity(amount) || amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        GetCombatant(combatantId).AddDelay(amount * _progressScale);
    }

    public void ApplyRecovery(string combatantId, double recoveryCost)
    {
        GetCombatant(combatantId).SetRecoveryCost(recoveryCost * _progressScale);
    }

    public string GetNextReadyCombatantId()
    {
        CombatantTimelineState selected = null;
        foreach (CombatantTimelineState candidate in _combatants.Values)
        {
            if (!candidate.CanAct(CombatTime))
            {
                continue;
            }

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
                double remainingStun = state.StunEndTime.Value - CombatTime;
                if (!nextEventTime.HasValue || remainingStun < nextEventTime.Value)
                {
                    nextEventTime = remainingStun;
                }

                continue;
            }

            if (state.RemainingProgress <= 0)
            {
                continue;
            }

            double timeToReady = state.RemainingProgress / state.BaseSpeed;
            if (!nextEventTime.HasValue || timeToReady < nextEventTime.Value)
            {
                nextEventTime = timeToReady;
            }
        }

        return nextEventTime;
    }

    public bool AdvanceToNextEvent()
    {
        double? elapsed = GetTimeUntilNextEvent();
        if (!elapsed.HasValue)
        {
            return false;
        }

        AdvanceTime(elapsed.Value);
        return true;
    }

    public void AdvanceTime(double elapsedCombatTime)
    {
        if (double.IsNaN(elapsedCombatTime) || double.IsInfinity(elapsedCombatTime) || elapsedCombatTime < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedCombatTime));
        }

        double nextTime = CombatTime + elapsedCombatTime;
        if (double.IsInfinity(nextTime))
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedCombatTime));
        }

        foreach (CombatantTimelineState state in _combatants.Values)
        {
            double activeElapsed = elapsedCombatTime;
            if (state.IsStunned(CombatTime))
            {
                double stunnedElapsed = state.StunEndTime.Value - CombatTime;
                activeElapsed = Math.Max(0, elapsedCombatTime - stunnedElapsed);
            }

            if (activeElapsed > 0)
            {
                state.AdvanceProgress(activeElapsed, state.BaseSpeed);
            }
        }

        CombatTime = nextTime;
    }

    private static void ValidatePositiveFinite(double value, string name)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(name);
        }
    }
}
