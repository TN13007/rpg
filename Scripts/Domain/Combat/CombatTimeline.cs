using System;
using System.Collections.Generic;

/// <summary>Tracks combat time and the readiness of participating combatants.</summary>
public sealed class CombatTimeline
{
    private readonly Dictionary<string, CombatantTimelineState> _combatants =
        new Dictionary<string, CombatantTimelineState>(StringComparer.Ordinal);
    private readonly double _progressScale;

    /// <summary>Gets the combat time.</summary>
    public double CombatTime { get; private set; }

    /// <summary>Initializes a new instance of <see cref="CombatTimeline"/>.</summary>
    /// <param name="progressScale">The progress scale used to initialize the instance.</param>
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

    /// <summary>Adds combatant to the current state.</summary>
    /// <param name="combatantId">The combatant id used by this operation.</param>
    /// <param name="luck">The luck used by this operation.</param>
    /// <param name="baseSpeed">The base speed used by this operation.</param>
    /// <param name="initialProgress">The initial progress used by this operation.</param>
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

    /// <summary>Removes combatant from the current state.</summary>
    /// <param name="combatantId">The combatant id used by this operation.</param>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
    public bool RemoveCombatant(string combatantId)
    {
        if (string.IsNullOrWhiteSpace(combatantId))
        {
            throw new ArgumentException("Combatant ID is required.", nameof(combatantId));
        }

        return _combatants.Remove(combatantId);
    }

    /// <summary>Performs the contains combatant operation for combat timeline.</summary>
    /// <param name="combatantId">The combatant id used by this operation.</param>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
    public bool ContainsCombatant(string combatantId)
    {
        return !string.IsNullOrWhiteSpace(combatantId) && _combatants.ContainsKey(combatantId);
    }

    /// <summary>Returns combatant.</summary>
    /// <param name="combatantId">The combatant id used by this operation.</param>
    /// <returns>The resulting combatant timeline state.</returns>
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

    /// <summary>Applies stun to the target combatant.</summary>
    /// <param name="combatantId">The combatant id used by this operation.</param>
    /// <param name="duration">The duration used by this operation.</param>
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

    /// <summary>Applies delay to the target combatant.</summary>
    /// <param name="combatantId">The combatant id used by this operation.</param>
    /// <param name="amount">The amount used by this operation.</param>
    public void ApplyDelay(string combatantId, double amount)
    {
        if (double.IsNaN(amount) || double.IsInfinity(amount) || amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        GetCombatant(combatantId).AddDelay(amount * _progressScale);
    }

    /// <summary>Applies recovery to the target combatant.</summary>
    /// <param name="combatantId">The combatant id used by this operation.</param>
    /// <param name="recoveryCost">The recovery cost used by this operation.</param>
    public void ApplyRecovery(string combatantId, double recoveryCost)
    {
        GetCombatant(combatantId).SetRecoveryCost(recoveryCost * _progressScale);
    }

    /// <summary>Returns next ready combatant id.</summary>
    /// <returns>The resulting string.</returns>
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

    /// <summary>Returns time until next event.</summary>
    /// <returns>The resulting double.</returns>
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

    /// <summary>Advances to next event by the supplied amount.</summary>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
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

    /// <summary>Advances time by the supplied amount.</summary>
    /// <param name="elapsedCombatTime">The elapsed combat time used by this operation.</param>
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
