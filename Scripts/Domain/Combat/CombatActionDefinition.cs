using System;

public enum CombatActionBehavior
{
    Attack,
    Defend,
    Dodge,
    RecoverStamina
}

public sealed class CombatActionDefinition
{
    public string Id { get; }
    public string Label { get; }
    public string CostLabel { get; }
    public string Description { get; }
    public CombatActionBehavior Behavior { get; }
    public int StaminaCost { get; }
    public int Damage { get; }
    public int RecoveryAmount { get; }
    public string UnavailableReason { get; }
    public string ActionDetail { get; }

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
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Combat action ID is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Combat action label is required.", nameof(label));
        if (!Enum.IsDefined(typeof(CombatActionBehavior), behavior)) throw new ArgumentOutOfRangeException(nameof(behavior));
        if (staminaCost < 0 || damage < 0 || recoveryAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(staminaCost), "Combat action values cannot be negative.");

        switch (behavior)
        {
            case CombatActionBehavior.Attack when damage < 1:
                throw new ArgumentOutOfRangeException(nameof(damage), "Attack damage must be positive.");
            case CombatActionBehavior.Defend:
            case CombatActionBehavior.Dodge:
                if (staminaCost < 1)
                    throw new ArgumentOutOfRangeException(nameof(staminaCost), "Defend and Dodge must cost Stamina.");
                break;
            case CombatActionBehavior.RecoverStamina when recoveryAmount < 1:
                throw new ArgumentOutOfRangeException(nameof(recoveryAmount), "Stamina recovery must be positive.");
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
