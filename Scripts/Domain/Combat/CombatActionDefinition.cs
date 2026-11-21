using System;

/// <summary>Identifies the behavior performed when a combat action is executed.</summary>
public enum CombatActionBehavior
{
    /// <summary>Deals damage to a selected target.</summary>
    Attack,
    /// <summary>Guards against incoming damage.</summary>
    Defend,
    /// <summary>Evades the next incoming attack.</summary>
    Dodge,
    /// <summary>Restores the acting combatant's Stamina.</summary>
    RecoverStamina,
    /// <summary>Adds progress delay to a selected target's timeline.</summary>
    Delay,
    /// <summary>Prevents a selected target from acting for a duration.</summary>
    Stun,
    /// <summary>Revives a downed party member.</summary>
    Revive
}

/// <summary>Defines the combat action data and its validation rules.</summary>
public sealed class CombatActionDefinition
{
    /// <summary>Gets the id.</summary>
    public string Id { get; }
    /// <summary>Gets the label.</summary>
    public string Label { get; }
    /// <summary>Gets the cost label.</summary>
    public string CostLabel { get; }
    /// <summary>Gets the description.</summary>
    public string Description { get; }
    /// <summary>Gets the behavior.</summary>
    public CombatActionBehavior Behavior { get; }
    /// <summary>Gets the stamina cost.</summary>
    public int StaminaCost { get; }
    /// <summary>Gets the damage.</summary>
    public int Damage { get; }
    /// <summary>Gets the recovery amount.</summary>
    public int RecoveryAmount { get; }
    /// <summary>Gets the recovery cost.</summary>
    public double RecoveryCost { get; }
    /// <summary>Gets the delay amount.</summary>
    public double DelayAmount { get; }
    /// <summary>Gets the stun duration.</summary>
    public double StunDuration { get; }
    /// <summary>Gets the unavailable reason.</summary>
    public string UnavailableReason { get; }
    /// <summary>Gets the action detail.</summary>
    public string ActionDetail { get; }

    /// <summary>Initializes a new instance of <see cref="CombatActionDefinition"/>.</summary>
    /// <param name="id">The id used to initialize the instance.</param>
    /// <param name="label">The label used to initialize the instance.</param>
    /// <param name="costLabel">The cost label used to initialize the instance.</param>
    /// <param name="description">The description used to initialize the instance.</param>
    /// <param name="behavior">The behavior used to initialize the instance.</param>
    /// <param name="staminaCost">The stamina cost used to initialize the instance.</param>
    /// <param name="damage">The damage used to initialize the instance.</param>
    /// <param name="recoveryAmount">The recovery amount used to initialize the instance.</param>
    /// <param name="unavailableReason">The unavailable reason used to initialize the instance.</param>
    /// <param name="actionDetail">The action detail used to initialize the instance.</param>
    /// <param name="recoveryCost">The recovery cost used to initialize the instance.</param>
    /// <param name="delayAmount">The delay amount used to initialize the instance.</param>
    /// <param name="stunDuration">The stun duration used to initialize the instance.</param>
    public CombatActionDefinition(
        string id, string label, string costLabel, string description,
        CombatActionBehavior behavior, int staminaCost, int damage, int recoveryAmount,
        string unavailableReason, string actionDetail,
        double recoveryCost = 100, double delayAmount = 0, double stunDuration = 0)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Combat action ID is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Combat action label is required.", nameof(label));
        if (!Enum.IsDefined(typeof(CombatActionBehavior), behavior)) throw new ArgumentOutOfRangeException(nameof(behavior));
        if (staminaCost < 0 || damage < 0 || recoveryAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(staminaCost), "Combat action values cannot be negative.");
        ValidateNonNegativeFinite(recoveryCost, nameof(recoveryCost));
        ValidateNonNegativeFinite(delayAmount, nameof(delayAmount));
        ValidateNonNegativeFinite(stunDuration, nameof(stunDuration));

        switch (behavior)
        {
            case CombatActionBehavior.Attack when damage < 1:
                throw new ArgumentOutOfRangeException(nameof(damage), "Attack damage must be positive.");
            case CombatActionBehavior.Defend:
            case CombatActionBehavior.Dodge:
                if (staminaCost < 1) throw new ArgumentOutOfRangeException(nameof(staminaCost), "Defend and Dodge must cost Stamina.");
                break;
            case CombatActionBehavior.RecoverStamina when recoveryAmount < 1:
                throw new ArgumentOutOfRangeException(nameof(recoveryAmount), "Stamina recovery must be positive.");
            case CombatActionBehavior.Delay when delayAmount <= 0:
                throw new ArgumentOutOfRangeException(nameof(delayAmount), "Delay actions must add positive progress.");
            case CombatActionBehavior.Stun when stunDuration <= 0:
                throw new ArgumentOutOfRangeException(nameof(stunDuration), "Stun actions must have a positive duration.");
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
        RecoveryCost = recoveryCost;
        DelayAmount = delayAmount;
        StunDuration = stunDuration;
        UnavailableReason = unavailableReason ?? string.Empty;
        ActionDetail = actionDetail ?? string.Empty;
    }

    private static void ValidateNonNegativeFinite(double value, string name)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
            throw new ArgumentOutOfRangeException(name);
    }
}
