using System;

/// <summary>Provides combat message type presentation data.</summary>
public enum CombatMessageType {
    /// <summary>A message describing a player or party action.</summary>
    Player,
    /// <summary>A message describing an enemy action.</summary>
    Enemy,
    /// <summary>A system-generated combat message.</summary>
    System
}
/// <summary>Provides combat message category presentation data.</summary>
public enum CombatMessageCategory {
    /// <summary>The message has no specialized category.</summary>
    None,
    /// <summary>The message reports damage.</summary>
    Damage,
    /// <summary>The message reports a gameplay check.</summary>
    Check,
    /// <summary>The message reports a turn transition.</summary>
    Turn,
    /// <summary>The message describes an environmental event.</summary>
    Environment,
    /// <summary>The message communicates an inferred enemy behavior.</summary>
    Inference,
    /// <summary>The message communicates an emotional change.</summary>
    Emotion,
    /// <summary>The message reports a status change.</summary>
    StatusChange,
    /// <summary>The message reports a round transition.</summary>
    Round
}
/// <summary>Provides intent confidence presentation data.</summary>
public enum IntentConfidence {
    /// <summary>The enemy intent is an informed suspicion.</summary>
    Suspected,
    /// <summary>The enemy intent has been recognized with higher confidence.</summary>
    Recognized
}
/// <summary>Provides combat event type presentation data.</summary>
public enum CombatEventType {
    /// <summary>A party member performed an action.</summary>
    PlayerAction,
    /// <summary>An enemy performed an action.</summary>
    EnemyAction,
    /// <summary>A combatant took damage.</summary>
    Damage,
    /// <summary>A gameplay check was resolved.</summary>
    Check,
    /// <summary>The active combatant changed.</summary>
    TurnChanged,
    /// <summary>The combat environment changed.</summary>
    EnvironmentChanged,
    /// <summary>An enemy intent inference was produced.</summary>
    Inference,
    /// <summary>A character emotion event occurred.</summary>
    Emotion,
    /// <summary>A combatant or encounter status changed.</summary>
    StatusChanged,
    /// <summary>A new combat round started.</summary>
    RoundStarted
}
/// <summary>Provides combat feedback type presentation data.</summary>
public enum CombatFeedbackType {
    /// <summary>Attack feedback.</summary>
    Attack,
    /// <summary>Guard feedback.</summary>
    Guard,
    /// <summary>Evade feedback.</summary>
    Evade,
    /// <summary>Stamina recovery feedback.</summary>
    Recover,
    /// <summary>Revival feedback.</summary>
    Revive,
    /// <summary>Incoming hit feedback.</summary>
    Hit
}

/// <summary>Carries combat feedback information to the presentation layer.</summary>
public sealed class CombatFeedbackViewData
{
    /// <summary>Gets the actor name.</summary>
    public string ActorName { get; }
    /// <summary>Gets the target name.</summary>
    public string TargetName { get; }
    /// <summary>Gets the type.</summary>
    public CombatFeedbackType Type { get; }

    /// <summary>Initializes a new instance of <see cref="CombatFeedbackViewData"/>.</summary>
    /// <param name="actorName">The actor name used to initialize the instance.</param>
    /// <param name="targetName">The target name used to initialize the instance.</param>
    /// <param name="type">The type used to initialize the instance.</param>
    public CombatFeedbackViewData(string actorName, string targetName, CombatFeedbackType type)
    {
        ActorName = actorName ?? string.Empty;
        TargetName = targetName ?? string.Empty;
        Type = type;
    }
}

/// <summary>Provides combat action request presentation data.</summary>
public sealed class CombatActionRequest
{
    /// <summary>Gets the action id.</summary>
    public string ActionId { get; }
    /// <summary>Gets the target id.</summary>
    public string TargetId { get; }
    /// <summary>Gets the actor id.</summary>
    public string ActorId { get; }

    /// <summary>Initializes a new instance of <see cref="CombatActionRequest"/>.</summary>
    /// <param name="actionId">The action id used to initialize the instance.</param>
    /// <param name="targetId">The target id used to initialize the instance.</param>
    /// <param name="actorId">The actor id used to initialize the instance.</param>
    public CombatActionRequest(string actionId, string targetId = "", string actorId = "")
    {
        if (string.IsNullOrWhiteSpace(actionId)) throw new ArgumentException("Action ID is required.", nameof(actionId));
        ActionId = actionId;
        TargetId = targetId ?? string.Empty;
        ActorId = actorId ?? string.Empty;
    }
}

/// <summary>Provides combat event presentation data.</summary>
public sealed class CombatEvent
{
    /// <summary>Gets the id.</summary>
    public string Id { get; }
    /// <summary>Gets the round.</summary>
    public int Round { get; }
    /// <summary>Gets the type.</summary>
    public CombatEventType Type { get; }
    /// <summary>Gets the actor name.</summary>
    public string ActorName { get; }
    /// <summary>Gets the target name.</summary>
    public string TargetName { get; }
    /// <summary>Gets the amount.</summary>
    public int Amount { get; }
    /// <summary>Gets the detail.</summary>
    public string Detail { get; }
    /// <summary>Gets the action id.</summary>
    public string ActionId { get; }

    /// <summary>Initializes a new instance of <see cref="CombatEvent"/>.</summary>
    /// <param name="type">The type used to initialize the instance.</param>
    /// <param name="round">The round used to initialize the instance.</param>
    /// <param name="actorName">The actor name used to initialize the instance.</param>
    /// <param name="targetName">The target name used to initialize the instance.</param>
    /// <param name="amount">The amount used to initialize the instance.</param>
    /// <param name="detail">The detail used to initialize the instance.</param>
    /// <param name="id">The id used to initialize the instance.</param>
    /// <param name="actionId">The action id used to initialize the instance.</param>
    public CombatEvent(CombatEventType type, int round, string actorName = "", string targetName = "", int amount = 0, string detail = "", string id = "", string actionId = "")
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

/// <summary>Provides combat message presentation data.</summary>
public sealed class CombatMessage
{
    /// <summary>Gets the id.</summary>
    public string Id { get; }
    /// <summary>Gets the round.</summary>
    public int Round { get; }
    /// <summary>Gets the type.</summary>
    public CombatMessageType Type { get; }
    /// <summary>Gets the category.</summary>
    public CombatMessageCategory Category { get; }
    /// <summary>Gets the speaker.</summary>
    public string Speaker { get; }
    /// <summary>Gets the text.</summary>
    public string Text { get; }

    /// <summary>Initializes a new instance of <see cref="CombatMessage"/>.</summary>
    /// <param name="id">The id used to initialize the instance.</param>
    /// <param name="round">The round used to initialize the instance.</param>
    /// <param name="type">The type used to initialize the instance.</param>
    /// <param name="category">The category used to initialize the instance.</param>
    /// <param name="speaker">The speaker used to initialize the instance.</param>
    /// <param name="text">The text used to initialize the instance.</param>
    public CombatMessage(string id, int round, CombatMessageType type, CombatMessageCategory category, string speaker, string text)
    {
        Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id;
        Round = round;
        Type = type;
        Category = category;
        Speaker = speaker ?? string.Empty;
        Text = text ?? string.Empty;
    }
}

/// <summary>Provides enemy combat status presentation data.</summary>
public sealed class EnemyCombatStatus
{
    /// <summary>Gets the id.</summary>
    public string Id { get; }
    /// <summary>Gets the name.</summary>
    public string Name { get; }
    /// <summary>Gets the current health.</summary>
    public int CurrentHealth { get; }
    /// <summary>Gets the max health.</summary>
    public int MaxHealth { get; }
    /// <summary>Gets the condition.</summary>
    public string Condition { get; }
    /// <summary>Gets a value indicating whether visible.</summary>
    public bool IsVisible { get; }
    /// <summary>Gets the intent description.</summary>
    public string IntentDescription { get; }

    /// <summary>Initializes a new instance of <see cref="EnemyCombatStatus"/>.</summary>
    /// <param name="id">The id used to initialize the instance.</param>
    /// <param name="name">The name used to initialize the instance.</param>
    /// <param name="currentHealth">The current health used to initialize the instance.</param>
    /// <param name="maxHealth">The max health used to initialize the instance.</param>
    /// <param name="condition">The condition used to initialize the instance.</param>
    /// <param name="isVisible">The is visible used to initialize the instance.</param>
    /// <param name="intentDescription">The intent description used to initialize the instance.</param>
    public EnemyCombatStatus(
        string id,
        string name,
        int currentHealth,
        int maxHealth,
        string condition,
        bool isVisible = true,
        string intentDescription = "")
    {
        if (maxHealth < 1) throw new ArgumentOutOfRangeException(nameof(maxHealth));
        if (currentHealth < 0 || currentHealth > maxHealth) throw new ArgumentOutOfRangeException(nameof(currentHealth));
        Id = id ?? string.Empty;
        Name = name ?? string.Empty;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        Condition = condition ?? string.Empty;
        IsVisible = isVisible;
        IntentDescription = intentDescription ?? string.Empty;
    }
}

/// <summary>Provides party combat status presentation data.</summary>
public sealed class PartyCombatStatus
{
    /// <summary>Gets the id.</summary>
    public string Id { get; }
    /// <summary>Gets the name.</summary>
    public string Name { get; }
    /// <summary>Gets the current health.</summary>
    public int CurrentHealth { get; }
    /// <summary>Gets the max health.</summary>
    public int MaxHealth { get; }
    /// <summary>Gets the condition.</summary>
    public string Condition { get; }
    /// <summary>Gets a value indicating whether active.</summary>
    public bool IsActive { get; }

    /// <summary>Initializes a new instance of <see cref="PartyCombatStatus"/>.</summary>
    /// <param name="id">The id used to initialize the instance.</param>
    /// <param name="name">The name used to initialize the instance.</param>
    /// <param name="currentHealth">The current health used to initialize the instance.</param>
    /// <param name="maxHealth">The max health used to initialize the instance.</param>
    /// <param name="condition">The condition used to initialize the instance.</param>
    /// <param name="isActive">The is active used to initialize the instance.</param>
    public PartyCombatStatus(string id, string name, int currentHealth, int maxHealth, string condition, bool isActive)
    {
        if (maxHealth < 1) throw new ArgumentOutOfRangeException(nameof(maxHealth));
        if (currentHealth < 0 || currentHealth > maxHealth) throw new ArgumentOutOfRangeException(nameof(currentHealth));
        Id = id ?? string.Empty;
        Name = name ?? string.Empty;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        Condition = condition ?? string.Empty;
        IsActive = isActive;
    }
}

/// <summary>Provides timeline combatant status presentation data.</summary>
public sealed class TimelineCombatantStatus
{
    /// <summary>Gets the id.</summary>
    public string Id { get; }
    /// <summary>Gets the name.</summary>
    public string Name { get; }
    /// <summary>Gets a value indicating whether enemy.</summary>
    public bool IsEnemy { get; }
    /// <summary>Gets the remaining progress.</summary>
    public double RemainingProgress { get; }
    /// <summary>Gets the speed.</summary>
    public double Speed { get; }
    /// <summary>Gets the luck.</summary>
    public int Luck { get; }
    /// <summary>Gets a value indicating whether ready.</summary>
    public bool IsReady { get; }
    /// <summary>Gets a value indicating whether stunned.</summary>
    public bool IsStunned { get; }
    /// <summary>Gets a value indicating whether visible.</summary>
    public bool IsVisible { get; }
    /// <summary>Gets a value indicating whether primary party member.</summary>
    public bool IsPrimaryPartyMember { get; }
    /// <summary>Gets the remaining stun time.</summary>
    public double RemainingStunTime { get; }
    /// <summary>Gets the intent description.</summary>
    public string IntentDescription { get; }

    /// <summary>Initializes a new instance of <see cref="TimelineCombatantStatus"/>.</summary>
    /// <param name="id">The id used to initialize the instance.</param>
    /// <param name="name">The name used to initialize the instance.</param>
    /// <param name="isEnemy">The is enemy used to initialize the instance.</param>
    /// <param name="remainingProgress">The remaining progress used to initialize the instance.</param>
    /// <param name="speed">The speed used to initialize the instance.</param>
    /// <param name="luck">The luck used to initialize the instance.</param>
    /// <param name="isReady">The is ready used to initialize the instance.</param>
    /// <param name="isStunned">The is stunned used to initialize the instance.</param>
    /// <param name="isVisible">The is visible used to initialize the instance.</param>
    /// <param name="isPrimaryPartyMember">The is primary party member used to initialize the instance.</param>
    /// <param name="remainingStunTime">The remaining stun time used to initialize the instance.</param>
    /// <param name="intentDescription">The intent description used to initialize the instance.</param>
    public TimelineCombatantStatus(
        string id,
        string name,
        bool isEnemy,
        double remainingProgress,
        double speed,
        int luck,
        bool isReady,
        bool isStunned,
        bool isVisible = true,
        bool isPrimaryPartyMember = false,
        double remainingStunTime = 0.0,
        string intentDescription = "")
    {
        if (double.IsNaN(remainingStunTime) ||
            double.IsInfinity(remainingStunTime) ||
            remainingStunTime < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(remainingStunTime));
        }

        Id = id ?? string.Empty;
        Name = name ?? string.Empty;
        IsEnemy = isEnemy;
        RemainingProgress = remainingProgress;
        Speed = speed;
        Luck = luck;
        IsReady = isReady;
        IsStunned = isStunned;
        IsVisible = isVisible;
        IsPrimaryPartyMember = !isEnemy && isPrimaryPartyMember;
        RemainingStunTime = remainingStunTime;
        IntentDescription = intentDescription ?? string.Empty;
    }
}

/// <summary>Provides intent observation presentation data.</summary>
public sealed class IntentObservation
{
    /// <summary>Gets the enemy id.</summary>
    public string EnemyId { get; }
    /// <summary>Gets the enemy name.</summary>
    public string EnemyName { get; }
    /// <summary>Gets the description.</summary>
    public string Description { get; }
    /// <summary>Gets the confidence.</summary>
    public IntentConfidence Confidence { get; }

    /// <summary>Initializes a new instance of <see cref="IntentObservation"/>.</summary>
    /// <param name="enemyId">The enemy id used to initialize the instance.</param>
    /// <param name="enemyName">The enemy name used to initialize the instance.</param>
    /// <param name="description">The description used to initialize the instance.</param>
    /// <param name="confidence">The confidence used to initialize the instance.</param>
    public IntentObservation(string enemyId, string enemyName, string description, IntentConfidence confidence)
    {
        EnemyId = enemyId ?? string.Empty;
        EnemyName = enemyName ?? string.Empty;
        Description = description ?? string.Empty;
        Confidence = confidence;
    }
}

/// <summary>Provides combat action target option presentation data.</summary>
public sealed class CombatActionTargetOption
{
    /// <summary>Gets the id.</summary>
    public string Id { get; }
    /// <summary>Gets the name.</summary>
    public string Name { get; }

    /// <summary>Initializes a new instance of <see cref="CombatActionTargetOption"/>.</summary>
    /// <param name="id">The id used to initialize the instance.</param>
    /// <param name="name">The name used to initialize the instance.</param>
    public CombatActionTargetOption(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Target ID is required.", nameof(id));

        Id = id;
        Name = name ?? string.Empty;
    }
}

/// <summary>Provides combat action option presentation data.</summary>
public sealed class CombatActionOption
{
    /// <summary>Gets the id.</summary>
    public string Id { get; }
    /// <summary>Gets the actor id.</summary>
    public string ActorId { get; }
    /// <summary>Gets the label.</summary>
    public string Label { get; }
    /// <summary>Gets the cost label.</summary>
    public string CostLabel { get; }
    /// <summary>Gets the description.</summary>
    public string Description { get; }
    /// <summary>Gets a value indicating whether available.</summary>
    public bool IsAvailable { get; }
    /// <summary>Gets the unavailable reason.</summary>
    public string UnavailableReason { get; }
    /// <summary>Gets the targets.</summary>
    public CombatActionTargetOption[] Targets { get; }

    /// <summary>Initializes a new instance of <see cref="CombatActionOption"/>.</summary>
    /// <param name="id">The id used to initialize the instance.</param>
    /// <param name="label">The label used to initialize the instance.</param>
    /// <param name="costLabel">The cost label used to initialize the instance.</param>
    /// <param name="description">The description used to initialize the instance.</param>
    /// <param name="isAvailable">The is available used to initialize the instance.</param>
    /// <param name="unavailableReason">The unavailable reason used to initialize the instance.</param>
    /// <param name="actorId">The actor id used to initialize the instance.</param>
    /// <param name="targets">The targets used to initialize the instance.</param>
    public CombatActionOption(
        string id,
        string label,
        string costLabel,
        string description,
        bool isAvailable = true,
        string unavailableReason = "",
        string actorId = "",
        CombatActionTargetOption[] targets = null)
    {
        Id = id ?? string.Empty;
        ActorId = actorId ?? string.Empty;
        Label = label ?? string.Empty;
        CostLabel = costLabel ?? string.Empty;
        Description = description ?? string.Empty;
        IsAvailable = isAvailable;
        UnavailableReason = unavailableReason ?? string.Empty;
        Targets = targets ?? Array.Empty<CombatActionTargetOption>();
    }
}

/// <summary>Provides combat snapshot presentation data.</summary>
public sealed class CombatSnapshot
{
    /// <summary>Gets the round.</summary>
    public int Round { get; }
    /// <summary>Gets the active turn label.</summary>
    public string ActiveTurnLabel { get; }
    /// <summary>Gets the enemies.</summary>
    public EnemyCombatStatus[] Enemies { get; }
    /// <summary>Gets the party.</summary>
    public PartyCombatStatus[] Party { get; }
    /// <summary>Gets the intent.</summary>
    public IntentObservation Intent { get; }
    /// <summary>Gets the available actions.</summary>
    public CombatActionOption[] AvailableActions { get; }
    /// <summary>Gets the timeline.</summary>
    public TimelineCombatantStatus[] Timeline { get; }
    /// <summary>Gets the active combatant id.</summary>
    public string ActiveCombatantId { get; }

    /// <summary>Initializes a new instance of <see cref="CombatSnapshot"/>.</summary>
    /// <param name="round">The round used to initialize the instance.</param>
    /// <param name="activeTurnLabel">The active turn label used to initialize the instance.</param>
    /// <param name="enemies">The enemies used to initialize the instance.</param>
    /// <param name="intent">The intent used to initialize the instance.</param>
    /// <param name="availableActions">The available actions used to initialize the instance.</param>
    /// <param name="timeline">The timeline used to initialize the instance.</param>
    /// <param name="party">The party used to initialize the instance.</param>
    /// <param name="activeCombatantId">The active combatant id used to initialize the instance.</param>
    public CombatSnapshot(
        int round,
        string activeTurnLabel,
        EnemyCombatStatus[] enemies,
        IntentObservation intent,
        CombatActionOption[] availableActions,
        TimelineCombatantStatus[] timeline = null,
        PartyCombatStatus[] party = null,
        string activeCombatantId = "")
    {
        Round = round;
        ActiveTurnLabel = activeTurnLabel ?? string.Empty;
        Enemies = enemies ?? Array.Empty<EnemyCombatStatus>();
        Party = party ?? Array.Empty<PartyCombatStatus>();
        Intent = intent;
        AvailableActions = availableActions ?? Array.Empty<CombatActionOption>();
        Timeline = timeline ?? Array.Empty<TimelineCombatantStatus>();
        ActiveCombatantId = activeCombatantId ?? string.Empty;
    }
}

/// <summary>Provides combat result presentation data.</summary>
public sealed class CombatResult
{
    /// <summary>Gets the success.</summary>
    public bool Success { get; }
    /// <summary>Gets the failure reason.</summary>
    public string FailureReason { get; }
    /// <summary>Gets the snapshot.</summary>
    public CombatSnapshot Snapshot { get; }
    /// <summary>Gets the events.</summary>
    public CombatEvent[] Events { get; }

    /// <summary>Initializes a new instance of <see cref="CombatResult"/>.</summary>
    /// <param name="success">The success used to initialize the instance.</param>
    /// <param name="snapshot">The snapshot used to initialize the instance.</param>
    /// <param name="events">The events used to initialize the instance.</param>
    /// <param name="failureReason">The failure reason used to initialize the instance.</param>
    public CombatResult(bool success, CombatSnapshot snapshot, CombatEvent[] events, string failureReason = "")
    {
        Success = success;
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        Events = events ?? Array.Empty<CombatEvent>();
        FailureReason = failureReason ?? string.Empty;
        if (!success && string.IsNullOrWhiteSpace(FailureReason))
            throw new ArgumentException("A failed combat result must include a failure reason.", nameof(failureReason));
    }
}

/// <summary>Provides i combat service presentation data.</summary>
public interface ICombatService
{
    CombatSnapshot GetSnapshot();
    CombatResult ExecuteAction(CombatActionRequest request);
}

/// <summary>Carries timeline marker information to the presentation layer.</summary>
public sealed class TimelineMarkerViewData
{
    /// <summary>Gets the id.</summary>
    public string Id { get; }
    /// <summary>Gets the name.</summary>
    public string Name { get; }
    /// <summary>Gets a value indicating whether enemy.</summary>
    public bool IsEnemy { get; }
    /// <summary>Gets a value indicating whether primary party member.</summary>
    public bool IsPrimaryPartyMember { get; }
    /// <summary>Gets a value indicating whether visible.</summary>
    public bool IsVisible { get; }
    /// <summary>Gets a value indicating whether ready.</summary>
    public bool IsReady { get; }
    /// <summary>Gets the luck.</summary>
    public int Luck { get; }
    /// <summary>Gets a value indicating whether stunned.</summary>
    public bool IsStunned { get; }
    /// <summary>Gets the time to ready seconds.</summary>
    public double TimeToReadySeconds { get; }
    /// <summary>Gets the intent description.</summary>
    public string IntentDescription { get; }

    /// <summary>Initializes a new instance of <see cref="TimelineMarkerViewData"/>.</summary>
    /// <param name="id">The id used to initialize the instance.</param>
    /// <param name="name">The name used to initialize the instance.</param>
    /// <param name="isEnemy">The is enemy used to initialize the instance.</param>
    /// <param name="isPrimaryPartyMember">The is primary party member used to initialize the instance.</param>
    /// <param name="isVisible">The is visible used to initialize the instance.</param>
    /// <param name="isReady">The is ready used to initialize the instance.</param>
    /// <param name="isStunned">The is stunned used to initialize the instance.</param>
    /// <param name="luck">The luck used to initialize the instance.</param>
    /// <param name="timeToReadySeconds">The time to ready seconds used to initialize the instance.</param>
    /// <param name="intentDescription">The intent description used to initialize the instance.</param>
    public TimelineMarkerViewData(
        string id,
        string name,
        bool isEnemy,
        bool isPrimaryPartyMember,
        bool isVisible,
        bool isReady,
        bool isStunned,
        int luck,
        double timeToReadySeconds,
        string intentDescription = "")
    {
        if (double.IsNaN(timeToReadySeconds) ||
            double.IsInfinity(timeToReadySeconds) ||
            timeToReadySeconds < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(timeToReadySeconds));
        }

        Id = id ?? string.Empty;
        Name = name ?? string.Empty;
        IsEnemy = isEnemy;
        IsPrimaryPartyMember = !isEnemy && isPrimaryPartyMember;
        IsVisible = isVisible;
        IsReady = isReady;
        IsStunned = isStunned;
        Luck = luck;
        TimeToReadySeconds = timeToReadySeconds;
        IntentDescription = intentDescription ?? string.Empty;
    }
}

/// <summary>Carries combat information to the presentation layer.</summary>
public sealed class CombatViewData
{
    /// <summary>Gets the round.</summary>
    public int Round { get; }
    /// <summary>Gets the active turn label.</summary>
    public string ActiveTurnLabel { get; }
    /// <summary>Gets the enemies.</summary>
    public EnemyCombatStatus[] Enemies { get; }
    /// <summary>Gets the party.</summary>
    public PartyCombatStatus[] Party { get; }
    /// <summary>Gets the intent.</summary>
    public IntentObservation Intent { get; }
    /// <summary>Gets the available actions.</summary>
    public CombatActionOption[] AvailableActions { get; }
    /// <summary>Gets the new messages.</summary>
    public CombatMessage[] NewMessages { get; }
    /// <summary>Gets the timeline.</summary>
    public TimelineMarkerViewData[] Timeline { get; }
    /// <summary>Gets the active combatant id.</summary>
    public string ActiveCombatantId { get; }
    /// <summary>Gets the feedback.</summary>
    public CombatFeedbackViewData[] Feedback { get; }

    /// <summary>Initializes a new instance of <see cref="CombatViewData"/>.</summary>
    /// <param name="round">The round used to initialize the instance.</param>
    /// <param name="activeTurnLabel">The active turn label used to initialize the instance.</param>
    /// <param name="enemies">The enemies used to initialize the instance.</param>
    /// <param name="intent">The intent used to initialize the instance.</param>
    /// <param name="availableActions">The available actions used to initialize the instance.</param>
    /// <param name="newMessages">The new messages used to initialize the instance.</param>
    /// <param name="timeline">The timeline used to initialize the instance.</param>
    /// <param name="party">The party used to initialize the instance.</param>
    /// <param name="activeCombatantId">The active combatant id used to initialize the instance.</param>
    /// <param name="feedback">The feedback used to initialize the instance.</param>
    public CombatViewData(
        int round,
        string activeTurnLabel,
        EnemyCombatStatus[] enemies,
        IntentObservation intent,
        CombatActionOption[] availableActions,
        CombatMessage[] newMessages,
        TimelineMarkerViewData[] timeline = null,
        PartyCombatStatus[] party = null,
        string activeCombatantId = "",
        CombatFeedbackViewData[] feedback = null)
    {
        Round = round;
        ActiveTurnLabel = activeTurnLabel ?? string.Empty;
        Enemies = enemies ?? Array.Empty<EnemyCombatStatus>();
        Party = party ?? Array.Empty<PartyCombatStatus>();
        Intent = intent;
        AvailableActions = availableActions ?? Array.Empty<CombatActionOption>();
        NewMessages = newMessages ?? Array.Empty<CombatMessage>();
        Timeline = timeline ?? Array.Empty<TimelineMarkerViewData>();
        ActiveCombatantId = activeCombatantId ?? string.Empty;
        Feedback = feedback ?? Array.Empty<CombatFeedbackViewData>();
    }
}
