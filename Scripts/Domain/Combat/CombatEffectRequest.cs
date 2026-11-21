/// <summary>Identifies the kind of effect produced by a combat action.</summary>
public enum CombatEffectType
{
    Damage,
    Guard,
    Evade,
    RestoreStamina,
    Delay,
    Stun,
    Revive
}

/// <summary>Provides combat effect request functionality for combat.</summary>
public sealed class CombatEffectRequest
{
    /// <summary>Gets the type.</summary>
    public CombatEffectType Type { get; }
    /// <summary>Gets the target id.</summary>
    public string TargetId { get; }
    /// <summary>Gets the amount.</summary>
    public int Amount { get; }
    /// <summary>Gets the duration.</summary>
    public double Duration { get; }

    /// <summary>Initializes a new instance of <see cref="CombatEffectRequest"/>.</summary>
    /// <param name="type">The type used to initialize the instance.</param>
    /// <param name="targetId">The target id used to initialize the instance.</param>
    /// <param name="amount">The amount used to initialize the instance.</param>
    /// <param name="duration">The duration used to initialize the instance.</param>
    public CombatEffectRequest(CombatEffectType type, string targetId, int amount = 0, double duration = 0)
    {
        Type = type;
        TargetId = targetId ?? string.Empty;
        Amount = amount;
        Duration = duration;
    }
}
