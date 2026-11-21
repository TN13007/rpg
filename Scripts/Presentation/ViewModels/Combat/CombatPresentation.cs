using System;

public enum CombatMessageType { Player, Enemy, System }
public enum CombatMessageCategory { None, Damage, Check, Turn, Environment, Inference, Emotion, StatusChange, Round }
public enum IntentConfidence { Suspected, Recognized }
public enum CombatEventType { PlayerAction, EnemyAction, Damage, Check, TurnChanged, EnvironmentChanged, Inference, Emotion, StatusChanged, RoundStarted }

public sealed class CombatActionRequest
{
    public string ActionId { get; }
    public string TargetId { get; }
    public string ActorId { get; }

    public CombatActionRequest(string actionId, string targetId = "", string actorId = "")
    {
        if (string.IsNullOrWhiteSpace(actionId)) throw new ArgumentException("Action ID is required.", nameof(actionId));
        ActionId = actionId;
        TargetId = targetId ?? string.Empty;
        ActorId = actorId ?? string.Empty;
    }
}

public sealed class CombatEvent
{
    public string Id { get; }
    public int Round { get; }
    public CombatEventType Type { get; }
    public string ActorName { get; }
    public string TargetName { get; }
    public int Amount { get; }
    public string Detail { get; }

    public CombatEvent(CombatEventType type, int round, string actorName = "", string targetName = "", int amount = 0, string detail = "", string id = "")
    {
        Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id;
        Round = round;
        Type = type;
        ActorName = actorName ?? string.Empty;
        TargetName = targetName ?? string.Empty;
        Amount = amount;
        Detail = detail ?? string.Empty;
    }
}

public sealed class CombatMessage
{
    public string Id { get; }
    public int Round { get; }
    public CombatMessageType Type { get; }
    public CombatMessageCategory Category { get; }
    public string Speaker { get; }
    public string Text { get; }

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

public sealed class EnemyCombatStatus
{
    public string Id { get; }
    public string Name { get; }
    public int CurrentHealth { get; }
    public int MaxHealth { get; }
    public string Condition { get; }
    public bool IsVisible { get; }

    public EnemyCombatStatus(string id, string name, int currentHealth, int maxHealth, string condition, bool isVisible = true)
    {
        if (maxHealth < 1) throw new ArgumentOutOfRangeException(nameof(maxHealth));
        if (currentHealth < 0 || currentHealth > maxHealth) throw new ArgumentOutOfRangeException(nameof(currentHealth));
        Id = id ?? string.Empty;
        Name = name ?? string.Empty;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        Condition = condition ?? string.Empty;
        IsVisible = isVisible;
    }
}

public sealed class PartyCombatStatus
{
    public string Id { get; }
    public string Name { get; }
    public int CurrentHealth { get; }
    public int MaxHealth { get; }
    public string Condition { get; }
    public bool IsActive { get; }

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

public sealed class TimelineCombatantStatus
{
    public string Id { get; }
    public string Name { get; }
    public bool IsEnemy { get; }
    public double RemainingProgress { get; }
    public double Speed { get; }
    public int Luck { get; }
    public bool IsReady { get; }
    public bool IsStunned { get; }
    public bool IsVisible { get; }
    public bool IsPrimaryPartyMember { get; }
    public double RemainingStunTime { get; }
    public string IntentDescription { get; }

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

public sealed class IntentObservation
{
    public string EnemyId { get; }
    public string EnemyName { get; }
    public string Description { get; }
    public IntentConfidence Confidence { get; }

    public IntentObservation(string enemyId, string enemyName, string description, IntentConfidence confidence)
    {
        EnemyId = enemyId ?? string.Empty;
        EnemyName = enemyName ?? string.Empty;
        Description = description ?? string.Empty;
        Confidence = confidence;
    }
}

public sealed class CombatActionTargetOption
{
    public string Id { get; }
    public string Name { get; }

    public CombatActionTargetOption(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Target ID is required.", nameof(id));

        Id = id;
        Name = name ?? string.Empty;
    }
}

public sealed class CombatActionOption
{
    public string Id { get; }
    public string ActorId { get; }
    public string Label { get; }
    public string CostLabel { get; }
    public string Description { get; }
    public bool IsAvailable { get; }
    public string UnavailableReason { get; }
    public CombatActionTargetOption[] Targets { get; }

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

public sealed class CombatSnapshot
{
    public int Round { get; }
    public string ActiveTurnLabel { get; }
    public EnemyCombatStatus[] Enemies { get; }
    public PartyCombatStatus[] Party { get; }
    public IntentObservation Intent { get; }
    public CombatActionOption[] AvailableActions { get; }
    public TimelineCombatantStatus[] Timeline { get; }
    public string ActiveCombatantId { get; }

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

public sealed class CombatResult
{
    public bool Success { get; }
    public string FailureReason { get; }
    public CombatSnapshot Snapshot { get; }
    public CombatEvent[] Events { get; }

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

public interface ICombatService
{
    CombatSnapshot GetSnapshot();
    CombatResult ExecuteAction(CombatActionRequest request);
}

public sealed class TimelineMarkerViewData
{
    public string Id { get; }
    public string Name { get; }
    public bool IsEnemy { get; }
    public bool IsPrimaryPartyMember { get; }
    public bool IsVisible { get; }
    public bool IsReady { get; }
    public int Luck { get; }
    public bool IsStunned { get; }
    public double TimeToReadySeconds { get; }
    public string IntentDescription { get; }

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

public sealed class CombatViewData
{
    public int Round { get; }
    public string ActiveTurnLabel { get; }
    public EnemyCombatStatus[] Enemies { get; }
    public PartyCombatStatus[] Party { get; }
    public IntentObservation Intent { get; }
    public CombatActionOption[] AvailableActions { get; }
    public CombatMessage[] NewMessages { get; }
    public TimelineMarkerViewData[] Timeline { get; }
    public string ActiveCombatantId { get; }

    public CombatViewData(
        int round,
        string activeTurnLabel,
        EnemyCombatStatus[] enemies,
        IntentObservation intent,
        CombatActionOption[] availableActions,
        CombatMessage[] newMessages,
        TimelineMarkerViewData[] timeline = null,
        PartyCombatStatus[] party = null,
        string activeCombatantId = "")
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
    }
}
