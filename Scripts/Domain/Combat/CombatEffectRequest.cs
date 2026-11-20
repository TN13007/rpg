public enum CombatEffectType
{
    Damage,
    Guard,
    Evade,
    RestoreStamina,
    Delay,
    Stun
}

public sealed class CombatEffectRequest
{
    public CombatEffectType Type { get; }
    public string TargetId { get; }
    public int Amount { get; }
    public double Duration { get; }

    public CombatEffectRequest(CombatEffectType type, string targetId, int amount = 0, double duration = 0)
    {
        Type = type;
        TargetId = targetId ?? string.Empty;
        Amount = amount;
        Duration = duration;
    }
}
