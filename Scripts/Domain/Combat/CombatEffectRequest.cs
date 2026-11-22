/// <summary>Identifies an effect produced by a combat action.</summary>
public enum CombatEffectType
{
    /// <summary>Applies damage to a target combatant.</summary>
    Damage,
    /// <summary>Enables the actor's guard state.</summary>
    Guard,
    /// <summary>Enables the actor's evade state.</summary>
    Evade,
    /// <summary>Restores Stamina to the actor.</summary>
    RestoreStamina,
    /// <summary>Applies the bounded Party Action Point control penalty.</summary>
    Control,
    /// <summary>Revives a Downed Party member.</summary>
    Revive
}

/// <summary>Describes one effect to be applied to a combatant.</summary>
public sealed class CombatEffectRequest
{
    /// <summary>Gets the effect type.</summary>
    public CombatEffectType Type { get; }
    /// <summary>Gets the target identifier.</summary>
    public string TargetId { get; }
    /// <summary>Gets the numeric effect amount.</summary>
    public int Amount { get; }

    /// <summary>Creates an effect request.</summary>
    public CombatEffectRequest(CombatEffectType type, string targetId, int amount = 0)
    {
        Type = type;
        TargetId = targetId ?? string.Empty;
        Amount = amount;
    }
}