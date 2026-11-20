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

    public EnemyCombatStatus(string id, string name, int currentHealth, int maxHealth, string condition)
    {
        if (maxHealth < 1) throw new ArgumentOutOfRangeException(nameof(maxHealth));
        if (currentHealth < 0 || currentHealth > maxHealth) throw new ArgumentOutOfRangeException(nameof(currentHealth));
        Id = id ?? string.Empty;
        Name = name ?? string.Empty;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        Condition = condition ?? string.Empty;
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

    public TimelineCombatantStatus(string id, string name, bool isEnemy, double remainingProgress, double speed, int luck, bool isReady, bool isStunned)
    {
        Id = id ?? string.Empty;
        Name = name ?? string.Empty;
        IsEnemy = isEnemy;
        RemainingProgress = remainingProgress;
        Speed = speed;
        Luck = luck;
        IsReady = isReady;
        IsStunned = isStunned;
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

public sealed class CombatActionOption
{
    public string Id { get; }
    public string TargetId { get; }
    public string ActorId { get; }
    public string Label { get; }
    public string CostLabel { get; }
    public string Description { get; }
    public bool IsAvailable { get; }
    public string UnavailableReason { get; }

    public CombatActionOption(string id, string label, string costLabel, string description, bool isAvailable = true, string unavailableReason = "", string targetId = "", string actorId = "")
    {
        Id = id ?? string.Empty;
        TargetId = targetId ?? string.Empty;
        ActorId = actorId ?? string.Empty;
        Label = label ?? string.Empty;
        CostLabel = costLabel ?? string.Empty;
        Description = description ?? string.Empty;
        IsAvailable = isAvailable;
        UnavailableReason = unavailableReason ?? string.Empty;
    }
}

public sealed class CombatSnapshot
{
    public int Round { get; }
    public string ActiveTurnLabel { get; }
    public EnemyCombatStatus[] Enemies { get; }
    public IntentObservation Intent { get; }
    public CombatActionOption[] AvailableActions { get; }
    public TimelineCombatantStatus[] Timeline { get; }

    public CombatSnapshot(int round, string activeTurnLabel, EnemyCombatStatus[] enemies, IntentObservation intent, CombatActionOption[] availableActions, TimelineCombatantStatus[] timeline = null)
    {
        Round = round;
        ActiveTurnLabel = activeTurnLabel ?? string.Empty;
        Enemies = enemies ?? Array.Empty<EnemyCombatStatus>();
        Intent = intent;
        AvailableActions = availableActions ?? Array.Empty<CombatActionOption>();
        Timeline = timeline ?? Array.Empty<TimelineCombatantStatus>();
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

public sealed class CombatViewData
{
    public int Round { get; }
    public string ActiveTurnLabel { get; }
    public EnemyCombatStatus[] Enemies { get; }
    public IntentObservation Intent { get; }
    public CombatActionOption[] AvailableActions { get; }
    public CombatMessage[] NewMessages { get; }
    public TimelineCombatantStatus[] Timeline { get; }

    public CombatViewData(int round, string activeTurnLabel, EnemyCombatStatus[] enemies, IntentObservation intent, CombatActionOption[] availableActions, CombatMessage[] newMessages, TimelineCombatantStatus[] timeline = null)
    {
        Round = round;
        ActiveTurnLabel = activeTurnLabel ?? string.Empty;
        Enemies = enemies ?? Array.Empty<EnemyCombatStatus>();
        Intent = intent;
        AvailableActions = availableActions ?? Array.Empty<CombatActionOption>();
        NewMessages = newMessages ?? Array.Empty<CombatMessage>();
        Timeline = timeline ?? Array.Empty<TimelineCombatantStatus>();
    }
}
