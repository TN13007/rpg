using System;

/// <summary>Tracks the mutable combatant timeline state used during combat.</summary>
public sealed class CombatantTimelineState
{
    /// <summary>Gets the combatant id.</summary>
    public string CombatantId { get; }
    /// <summary>Gets the luck.</summary>
    public int Luck { get; }
    /// <summary>Gets the base speed.</summary>
    public double BaseSpeed { get; }
    /// <summary>Gets the remaining progress.</summary>
    public double RemainingProgress { get; private set; }
    /// <summary>Gets the stun end time.</summary>
    public double? StunEndTime { get; private set; }

    /// <summary>Gets a value indicating whether ready.</summary>
    public bool IsReady => RemainingProgress <= 0;
    /// <summary>Determines whether stunned.</summary>
    /// <param name="combatTime">The combat time used by this operation.</param>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
    public bool IsStunned(double combatTime) => StunEndTime.HasValue && combatTime < StunEndTime.Value;
    /// <summary>Determines whether act.</summary>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
    public bool CanAct(double combatTime) => IsReady && !IsStunned(combatTime);

    /// <summary>Initializes a new instance of <see cref="CombatantTimelineState"/>.</summary>
    /// <param name="combatantId">The combatant id used to initialize the instance.</param>
    /// <param name="luck">The luck used to initialize the instance.</param>
    /// <param name="baseSpeed">The base speed used to initialize the instance.</param>
    /// <param name="initialProgress">The initial progress used to initialize the instance.</param>
    public CombatantTimelineState(string combatantId, int luck, double baseSpeed, double initialProgress)
    {
        if (string.IsNullOrWhiteSpace(combatantId)) throw new ArgumentException("Combatant ID is required.", nameof(combatantId));
        if (luck < 0 || luck > 100) throw new ArgumentOutOfRangeException(nameof(luck));
        if (double.IsNaN(baseSpeed) || double.IsInfinity(baseSpeed) || baseSpeed <= 0)
            throw new ArgumentOutOfRangeException(nameof(baseSpeed), "Base Speed must be finite and greater than zero.");
        if (double.IsNaN(initialProgress) || double.IsInfinity(initialProgress) || initialProgress < 0)
            throw new ArgumentOutOfRangeException(nameof(initialProgress));
        CombatantId = combatantId;
        Luck = luck;
        BaseSpeed = baseSpeed;
        RemainingProgress = initialProgress;
    }

    internal void AdvanceProgress(double elapsedCombatTime, double effectiveSpeed)
    {
        if (elapsedCombatTime < 0 || double.IsNaN(elapsedCombatTime) || double.IsInfinity(elapsedCombatTime))
            throw new ArgumentOutOfRangeException(nameof(elapsedCombatTime));
        if (effectiveSpeed <= 0 || double.IsNaN(effectiveSpeed) || double.IsInfinity(effectiveSpeed))
            throw new ArgumentOutOfRangeException(nameof(effectiveSpeed));
        if (RemainingProgress <= 0) return;
        RemainingProgress = Math.Max(0, RemainingProgress - effectiveSpeed * elapsedCombatTime);
    }

    internal void SetRecoveryCost(double recoveryCost)
    {
        if (double.IsNaN(recoveryCost) || double.IsInfinity(recoveryCost) || recoveryCost < 0)
            throw new ArgumentOutOfRangeException(nameof(recoveryCost));
        RemainingProgress = recoveryCost;
    }

    internal void AddDelay(double amount)
    {
        if (double.IsNaN(amount) || double.IsInfinity(amount) || amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        double nextProgress = RemainingProgress + amount;
        if (double.IsInfinity(nextProgress)) throw new ArgumentOutOfRangeException(nameof(amount));
        RemainingProgress = nextProgress;
    }

    internal void ApplyStun(double endTime)
    {
        if (double.IsNaN(endTime) || double.IsInfinity(endTime)) throw new ArgumentOutOfRangeException(nameof(endTime));
        if (!StunEndTime.HasValue || endTime > StunEndTime.Value) StunEndTime = endTime;
    }
}
