using System;

/// <summary>Identifies the behavior performed when a combat action is executed.</summary>
public enum CombatActionBehavior
{
    /// <summary>Deals damage to a selected target.</summary>
    Attack,
    /// <summary>Guards against the next incoming damage instance.</summary>
    Defend,
    /// <summary>Evades the next incoming damage instance.</summary>
    Dodge,
    /// <summary>Restores the acting combatant's Stamina.</summary>
    RecoverStamina,
    /// <summary>Deals damage and reduces the next Party Turn's shared AP when used by an enemy.</summary>
    Control,
    /// <summary>Revives a Downed Party member.</summary>
    Revive
}

/// <summary>Defines an authored combat action without any frame-time or recovery scheduling data.</summary>
public sealed class CombatActionDefinition
{
    /// <summary>Gets the stable action identifier.</summary>
    public string Id { get; }
    /// <summary>Gets the action label.</summary>
    public string Label { get; }
    /// <summary>Gets the displayed resource cost.</summary>
    public string CostLabel { get; }
    /// <summary>Gets the action description.</summary>
    public string Description { get; }
    /// <summary>Gets the action behavior.</summary>
    public CombatActionBehavior Behavior { get; }
    /// <summary>Gets the Stamina cost.</summary>
    public int StaminaCost { get; }
    /// <summary>Gets the damage amount.</summary>
    public int Damage { get; }
    /// <summary>Gets the Stamina recovery amount.</summary>
    public int RecoveryAmount { get; }
    /// <summary>Gets the reason displayed when the action is unavailable.</summary>
    public string UnavailableReason { get; }
    /// <summary>Gets the action result text.</summary>
    public string ActionDetail { get; }

    /// <summary>Creates an authored combat action.</summary>
    public CombatActionDefinition(
        string id,
        string label,
        string costLabel,
        string description,
        CombatActionBehavior behavior,
        int staminaCost,
        int damage,
        int recoveryAmount,
        string unavailableReason,
        string actionDetail)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Combat action ID is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Combat action label is required.", nameof(label));
        }

        if (!Enum.IsDefined(typeof(CombatActionBehavior), behavior))
        {
            throw new ArgumentOutOfRangeException(nameof(behavior));
        }

        if (staminaCost < 0 || damage < 0 || recoveryAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(staminaCost), "Combat action values cannot be negative.");
        }

        switch (behavior)
        {
            case CombatActionBehavior.Attack when damage < 1:
                throw new ArgumentOutOfRangeException(nameof(damage), "Attack damage must be positive.");
            case CombatActionBehavior.Defend:
            case CombatActionBehavior.Dodge:
                if (staminaCost < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(staminaCost), "Defend and Dodge must cost Stamina.");
                }
                break;
            case CombatActionBehavior.RecoverStamina when recoveryAmount < 1:
                throw new ArgumentOutOfRangeException(nameof(recoveryAmount), "Stamina recovery must be positive.");
            case CombatActionBehavior.Control when damage < 0:
                throw new ArgumentOutOfRangeException(nameof(damage));
            case CombatActionBehavior.Revive when staminaCost < 1:
                throw new ArgumentOutOfRangeException(nameof(staminaCost), "Revive actions must cost Stamina.");
        }

        Id = id;
        Label = label;
        CostLabel = costLabel ?? string.Empty;
        Description = description ?? string.Empty;
        Behavior = behavior;
        StaminaCost = staminaCost;
        Damage = damage;
        RecoveryAmount = recoveryAmount;
        UnavailableReason = unavailableReason ?? string.Empty;
        ActionDetail = actionDetail ?? string.Empty;
    }
}