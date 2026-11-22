using System;

/// <summary>Identifies the current phase of a discrete combat round.</summary>
public enum CombatPhase
{
    /// <summary>No encounter has been started.</summary>
    NotStarted,
    /// <summary>The player may spend shared Party Action Points.</summary>
    PlayerTurn,
    /// <summary>Living enemies are resolving their authored actions.</summary>
    EnemyTurn,
    /// <summary>The encounter has reached victory or defeat.</summary>
    Complete
}

/// <summary>Carries a player-selected combat action and its required acting character.</summary>
public sealed class CombatActionRequest
{
    /// <summary>Gets the action identifier.</summary>
    public string ActionId { get; }

    /// <summary>Gets the selected target identifier, or an empty string for an action without a target.</summary>
    public string TargetId { get; }

    /// <summary>Gets the required Party actor identifier.</summary>
    public string ActorId { get; }

    /// <summary>Initializes a combat action request.</summary>
    /// <param name="actionId">The authored action identifier.</param>
    /// <param name="targetId">The selected target identifier, or an empty string for self/no-target actions.</param>
    /// <param name="actorId">The Party member who must perform the action.</param>
    public CombatActionRequest(string actionId, string targetId, string actorId)
    {
        if (string.IsNullOrWhiteSpace(actionId))
        {
            throw new ArgumentException("Action ID is required.", nameof(actionId));
        }

        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor ID is required.", nameof(actorId));
        }

        ActionId = actionId;
        TargetId = targetId ?? string.Empty;
        ActorId = actorId;
    }
}

/// <summary>Identifies a domain event produced while resolving combat.</summary>
public enum CombatEventType
{
    /// <summary>A Party member performed an action.</summary>
    PlayerAction,
    /// <summary>An enemy performed an authored action.</summary>
    EnemyAction,
    /// <summary>A combatant took damage.</summary>
    Damage,
    /// <summary>A gameplay effect or check was resolved.</summary>
    Check,
    /// <summary>The combat phase changed.</summary>
    TurnChanged,
    /// <summary>A combatant or encounter condition changed.</summary>
    StatusChanged,
    /// <summary>A new Round began.</summary>
    RoundStarted
}

/// <summary>Records one resolved domain event in the order it occurred.</summary>
public sealed class CombatEvent
{
    /// <summary>Gets the stable event identifier.</summary>
    public string Id { get; }
    /// <summary>Gets the Round in which the event occurred.</summary>
    public int Round { get; }
    /// <summary>Gets the event type.</summary>
    public CombatEventType Type { get; }
    /// <summary>Gets the actor display name.</summary>
    public string ActorName { get; }
    /// <summary>Gets the target display name.</summary>
    public string TargetName { get; }
    /// <summary>Gets the amount associated with the event.</summary>
    public int Amount { get; }
    /// <summary>Gets the event detail.</summary>
    public string Detail { get; }
    /// <summary>Gets the authored action identifier.</summary>
    public string ActionId { get; }

    /// <summary>Creates an ordered combat event.</summary>
    public CombatEvent(
        CombatEventType type,
        int round,
        string actorName = "",
        string targetName = "",
        int amount = 0,
        string detail = "",
        string id = "",
        string actionId = "")
    {
        Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id;
        Round = round;
        Type = type;
        ActorName = actorName ?? string.Empty;
        TargetName = targetName ?? string.Empty;
        Amount = amount;
        Detail = detail ?? string.Empty;
        ActionId = actionId ?? string.Empty;
    }
}

/// <summary>Contains the current presentation-safe state of one enemy.</summary>
public sealed class EnemyCombatStatus
{
    /// <summary>Gets the stable enemy identifier.</summary>
    public string Id { get; }
    /// <summary>Gets the enemy display name.</summary>
    public string Name { get; }
    /// <summary>Gets current Health.</summary>
    public int CurrentHealth { get; }
    /// <summary>Gets maximum Health.</summary>
    public int MaxHealth { get; }
    /// <summary>Gets the current condition label.</summary>
    public string Condition { get; }
    /// <summary>Gets whether this enemy should be shown to the player.</summary>
    public bool IsVisible { get; }

    /// <summary>Creates an enemy status value.</summary>
    public EnemyCombatStatus(
        string id,
        string name,
        int currentHealth,
        int maxHealth,
        string condition,
        bool isVisible = true)
    {
        if (maxHealth < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxHealth));
        }

        if (currentHealth < 0 || currentHealth > maxHealth)
        {
            throw new ArgumentOutOfRangeException(nameof(currentHealth));
        }

        Id = id ?? string.Empty;
        Name = name ?? string.Empty;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        Condition = condition ?? string.Empty;
        IsVisible = isVisible;
    }
}

/// <summary>Contains the current presentation-safe state of one Party member.</summary>
public sealed class PartyCombatStatus
{
    /// <summary>Gets the stable Party member identifier.</summary>
    public string Id { get; }
    /// <summary>Gets the Party member display name.</summary>
    public string Name { get; }
    /// <summary>Gets current Health.</summary>
    public int CurrentHealth { get; }
    /// <summary>Gets maximum Health.</summary>
    public int MaxHealth { get; }
    /// <summary>Gets the current condition label.</summary>
    public string Condition { get; }
    /// <summary>Gets whether the member is eligible to act during the current Player Turn.</summary>
    public bool IsActive { get; }

    /// <summary>Creates a Party status value.</summary>
    public PartyCombatStatus(
        string id,
        string name,
        int currentHealth,
        int maxHealth,
        string condition,
        bool isActive)
    {
        if (maxHealth < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxHealth));
        }

        if (currentHealth < 0 || currentHealth > maxHealth)
        {
            throw new ArgumentOutOfRangeException(nameof(currentHealth));
        }

        Id = id ?? string.Empty;
        Name = name ?? string.Empty;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        Condition = condition ?? string.Empty;
        IsActive = isActive;
    }
}

/// <summary>Describes one valid target offered for a combat action.</summary>
public sealed class CombatActionTargetOption
{
    /// <summary>Gets the target identifier.</summary>
    public string Id { get; }
    /// <summary>Gets the target display name.</summary>
    public string Name { get; }

    /// <summary>Creates a target option.</summary>
    public CombatActionTargetOption(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Target ID is required.", nameof(id));
        }

        Id = id;
        Name = name ?? string.Empty;
    }
}

/// <summary>Describes one action a specific Party member can attempt.</summary>
public sealed class CombatActionOption
{
    /// <summary>Gets the authored action identifier.</summary>
    public string Id { get; }
    /// <summary>Gets the required actor identifier.</summary>
    public string ActorId { get; }
    /// <summary>Gets the action label, including its acting Party member.</summary>
    public string Label { get; }
    /// <summary>Gets the displayed resource cost.</summary>
    public string CostLabel { get; }
    /// <summary>Gets the action description.</summary>
    public string Description { get; }
    /// <summary>Gets whether the action can currently be performed.</summary>
    public bool IsAvailable { get; }
    /// <summary>Gets why the action is unavailable.</summary>
    public string UnavailableReason { get; }
    /// <summary>Gets the valid target options.</summary>
    public CombatActionTargetOption[] Targets { get; }

    /// <summary>Creates an action option.</summary>
    public CombatActionOption(
        string id,
        string actorId,
        string label,
        string costLabel,
        string description,
        bool isAvailable = true,
        string unavailableReason = "",
        CombatActionTargetOption[] targets = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Action ID is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor ID is required.", nameof(actorId));
        }

        Id = id;
        ActorId = actorId;
        Label = label ?? string.Empty;
        CostLabel = costLabel ?? string.Empty;
        Description = description ?? string.Empty;
        IsAvailable = isAvailable;
        UnavailableReason = unavailableReason ?? string.Empty;
        Targets = targets ?? Array.Empty<CombatActionTargetOption>();
    }
}

/// <summary>Provides a read-only view of the current discrete combat state.</summary>
public sealed class CombatSnapshot
{
    /// <summary>Gets the current Round number.</summary>
    public int Round { get; }
    /// <summary>Gets the current combat phase.</summary>
    public CombatPhase Phase { get; }
    /// <summary>Gets the Party's currently available shared Action Points.</summary>
    public int CurrentActionPoints { get; }
    /// <summary>Gets the Party's maximum Action Points per Player Turn.</summary>
    public int ActionCapacity { get; }
    /// <summary>Gets the short phase label intended for the combat HUD.</summary>
    public string ActiveTurnLabel { get; }
    /// <summary>Gets the current enemy statuses.</summary>
    public EnemyCombatStatus[] Enemies { get; }
    /// <summary>Gets the current Party statuses.</summary>
    public PartyCombatStatus[] Party { get; }
    /// <summary>Gets the action options for all eligible Party members.</summary>
    public CombatActionOption[] AvailableActions { get; }

    /// <summary>Creates a combat snapshot.</summary>
    public CombatSnapshot(
        int round,
        CombatPhase phase,
        int currentActionPoints,
        int actionCapacity,
        string activeTurnLabel,
        EnemyCombatStatus[] enemies,
        PartyCombatStatus[] party,
        CombatActionOption[] availableActions)
    {
        if (round < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(round));
        }

        if (currentActionPoints < 0 || actionCapacity < 1 || currentActionPoints > actionCapacity)
        {
            throw new ArgumentOutOfRangeException(nameof(currentActionPoints));
        }

        if (!Enum.IsDefined(typeof(CombatPhase), phase))
        {
            throw new ArgumentOutOfRangeException(nameof(phase));
        }

        Round = round;
        Phase = phase;
        CurrentActionPoints = currentActionPoints;
        ActionCapacity = actionCapacity;
        ActiveTurnLabel = activeTurnLabel ?? string.Empty;
        Enemies = enemies ?? Array.Empty<EnemyCombatStatus>();
        Party = party ?? Array.Empty<PartyCombatStatus>();
        AvailableActions = availableActions ?? Array.Empty<CombatActionOption>();
    }
}

/// <summary>Contains the result and ordered events produced by a combat operation.</summary>
public sealed class CombatResult
{
    /// <summary>Gets whether the requested operation succeeded.</summary>
    public bool Success { get; }
    /// <summary>Gets the reason for a rejected operation.</summary>
    public string FailureReason { get; }
    /// <summary>Gets the latest combat snapshot.</summary>
    public CombatSnapshot Snapshot { get; }
    /// <summary>Gets events in the exact order they were resolved.</summary>
    public CombatEvent[] Events { get; }

    /// <summary>Creates a combat result.</summary>
    public CombatResult(bool success, CombatSnapshot snapshot, CombatEvent[] events, string failureReason = "")
    {
        Success = success;
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        Events = events ?? Array.Empty<CombatEvent>();
        FailureReason = failureReason ?? string.Empty;

        if (!success && string.IsNullOrWhiteSpace(FailureReason))
        {
            throw new ArgumentException("A failed combat result must include a failure reason.", nameof(failureReason));
        }
    }
}

/// <summary>Defines the operations exposed by the combat Domain service.</summary>
public interface ICombatService
{
    /// <summary>Returns a read-only snapshot of the current encounter.</summary>
    CombatSnapshot GetSnapshot();

    /// <summary>Executes one Party action.</summary>
    CombatResult ExecuteAction(CombatActionRequest request);

    /// <summary>Ends the current Player Turn and resolves the complete Enemy Turn.</summary>
    CombatResult EndPlayerTurn();
}
