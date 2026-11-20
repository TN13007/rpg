using System;

public sealed class CombatantTimelineState
{
    public string CombatantId { get; }
    public int Luck { get; }
    public double BaseSpeed { get; }
    public double RemainingProgress { get; private set; }
    public double? StunEndTime { get; private set; }

    public bool IsReady => RemainingProgress <= 0;
    public bool IsStunned(double combatTime) => StunEndTime.HasValue && combatTime < StunEndTime.Value;
    public bool CanAct(double combatTime) => IsReady && !IsStunned(combatTime);

    public CombatantTimelineState(
        string combatantId,
        int luck,
        double baseSpeed,
        double initialProgress)
    {
        if (string.IsNullOrWhiteSpace(combatantId))
            throw new ArgumentException("Combatant ID is required.", nameof(combatantId));
        if (luck < 0 || luck > 100)
            throw new ArgumentOutOfRangeException(nameof(luck));
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

        if (RemainingProgress <= 0)
            return;

        RemainingProgress = Math.Max(0, RemainingProgress - effectiveSpeed * elapsedCombatTime);
    }

    internal void SetRecoveryCost(double recoveryCost)
    {
        if (double.IsNaN(recoveryCost) || double.IsInfinity(recoveryCost) || recoveryCost < 0)
            throw new ArgumentOutOfRangeException(nameof(recoveryCost));
        RemainingProgress = recoveryCost;
    }

    internal void ApplyStun(double endTime)
    {
        if (double.IsNaN(endTime) || double.IsInfinity(endTime))
            throw new ArgumentOutOfRangeException(nameof(endTime));
        if (!StunEndTime.HasValue || endTime > StunEndTime.Value)
            StunEndTime = endTime;
    }
}
