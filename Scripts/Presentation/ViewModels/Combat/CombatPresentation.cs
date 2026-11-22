using System;

public enum CombatMessageType
{
    Player,
    Enemy,
    System
}

public enum CombatMessageCategory
{
    None,
    Damage,
    Check,
    Turn,
    Environment,
    StatusChange,
    Round
}

/// <summary>Identifies a presentation-only group for combat actions.</summary>
public enum CombatActionCategory
{
    Attack,
    Defence,
    Allure,
    Inventory,
    Run,
    Other
}

/// <summary>Identifies which side can be selected by a targeted action.</summary>
public enum CombatActionTargetKind
{
    None,
    Enemy,
    Party
}

/// <summary>Identifies a visual feedback cue for a combatant.</summary>
public enum CombatFeedbackType
{
    QuickAttack,
    HeavyAttack,
    DisruptingAttack,
    Attack,
    Guard,
    Evade,
    Recover,
    Revive,
    Hit
}

/// <summary>Presentation-only state for one Enemy card. Contains no Domain objects.</summary>
public sealed class EnemyCardViewData
{
    public string Id { get; }
    public string Name { get; }
    public int CurrentHealth { get; }
    public int MaxHealth { get; }
    public string Condition { get; }
    public int? Arousal { get; }
    /// <summary>Gets Lust when the Arousal runtime provides an authoritative value.</summary>
    public int? Lust => Arousal;

    public EnemyCardViewData(
        string id,
        string name,
        int currentHealth,
        int maxHealth,
        string condition,
        int? arousal = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Enemy card ID is required.", nameof(id));
        if (maxHealth < 1 || currentHealth < 0 || currentHealth > maxHealth)
            throw new ArgumentOutOfRangeException(nameof(currentHealth));
        if (arousal.HasValue && (arousal.Value < 0 || arousal.Value > 100))
            throw new ArgumentOutOfRangeException(nameof(arousal));

        Id = id;
        Name = name ?? string.Empty;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        Condition = condition ?? string.Empty;
        Arousal = arousal;
    }
}

/// <summary>Presentation-only state for one Party roster card. Contains no Domain objects.</summary>
public sealed class PartyMemberCardViewData
{
    public string Id { get; }
    public string Name { get; }
    public int CurrentHealth { get; }
    public int MaxHealth { get; }
    public int CurrentStamina { get; }
    public int MaxStamina { get; }
    public string Condition { get; }
    public bool CanAct { get; }
    public int? Arousal { get; }
    /// <summary>Gets Lust when the Arousal runtime provides an authoritative value.</summary>
    public int? Lust => Arousal;
    /// <summary>Gets the current MP when the character runtime provides it.</summary>
    public int? CurrentMp { get; }
    /// <summary>Gets maximum MP when the character runtime provides it.</summary>
    public int? MaxMp { get; }
    /// <summary>Gets the data needed to compose this member's portrait.</summary>
    public CharacterPortraitViewData Portrait { get; }

    public PartyMemberCardViewData(
        string id,
        string name,
        int currentHealth,
        int maxHealth,
        string condition,
        bool canAct,
        int currentStamina,
        int maxStamina,
        int? arousal = null,
        int? currentMp = null,
        int? maxMp = null,
        CharacterPortraitViewData portrait = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Party member card ID is required.", nameof(id));
        if (maxHealth < 1 || currentHealth < 0 || currentHealth > maxHealth)
            throw new ArgumentOutOfRangeException(nameof(currentHealth));
        if (maxStamina < 0 || currentStamina < 0 || currentStamina > maxStamina)
            throw new ArgumentOutOfRangeException(nameof(currentStamina));
        if (arousal.HasValue && (arousal.Value < 0 || arousal.Value > 100))
            throw new ArgumentOutOfRangeException(nameof(arousal));
        if (currentMp.HasValue != maxMp.HasValue)
            throw new ArgumentException("Current and maximum MP must both be supplied or both be omitted.");
        if (currentMp.HasValue && (maxMp.Value < 0 || currentMp.Value < 0 || currentMp.Value > maxMp.Value))
            throw new ArgumentOutOfRangeException(nameof(currentMp));

        Id = id;
        Name = name ?? string.Empty;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        Condition = condition ?? string.Empty;
        CanAct = canAct;
        CurrentStamina = currentStamina;
        MaxStamina = maxStamina;
        Arousal = arousal;
        CurrentMp = currentMp;
        MaxMp = maxMp;
        Portrait = portrait ?? new CharacterPortraitViewData(
            id,
            name,
            "res://Assets/CharacterPortrait.svg",
            string.Equals(condition, "Downed", StringComparison.OrdinalIgnoreCase) ? "downed" : "idle",
            string.Empty,
            string.Empty,
            condition);
    }
}

/// <summary>Presentation-only visual layers used to compose a Party member portrait.</summary>
public sealed class CharacterPortraitViewData
{
    public string CharacterId { get; }
    public string Name { get; }
    public string BasePosePath { get; }
    public string OutfitLayerPath { get; }
    public string ExpressionLayerPath { get; }
    public string StatusOverlayPath { get; }
    public string StanceId { get; }
    public string ExpressionId { get; }
    public string OutfitId { get; }
    public string Status { get; }

    public CharacterPortraitViewData(
        string characterId,
        string name,
        string basePosePath,
        string stanceId,
        string expressionId,
        string outfitId,
        string status,
        string outfitLayerPath = "",
        string expressionLayerPath = "",
        string statusOverlayPath = "")
    {
        if (string.IsNullOrWhiteSpace(characterId))
            throw new ArgumentException("Portrait character ID is required.", nameof(characterId));

        CharacterId = characterId;
        Name = name ?? string.Empty;
        BasePosePath = basePosePath ?? string.Empty;
        StanceId = stanceId ?? string.Empty;
        ExpressionId = expressionId ?? string.Empty;
        OutfitId = outfitId ?? string.Empty;
        Status = status ?? string.Empty;
        OutfitLayerPath = outfitLayerPath ?? string.Empty;
        ExpressionLayerPath = expressionLayerPath ?? string.Empty;
        StatusOverlayPath = statusOverlayPath ?? string.Empty;
    }
}

/// <summary>Presentation-only target information shown by a combat action.</summary>
public sealed class CombatActionTargetViewData
{
    public string Id { get; }
    public string Name { get; }

    public CombatActionTargetViewData(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Combat target ID is required.", nameof(id));

        Id = id;
        Name = name ?? string.Empty;
    }
}

/// <summary>Presentation-only state for one action card. It is safe to pass directly to a Godot control.</summary>
public sealed class CombatActionCardViewData
{
    public string Id { get; }
    public string ActorId { get; }
    public string Label { get; }
    public string CostLabel { get; }
    public string Description { get; }
    public string EffectSummary { get; }
    public bool IsAvailable { get; }
    public string UnavailableReason { get; }
    public CombatActionCategory Category { get; }
    public CombatActionTargetKind TargetKind { get; }
    public CombatActionTargetViewData[] Targets { get; }

    public CombatActionCardViewData(
        string id,
        string actorId,
        string label,
        string costLabel,
        string description,
        bool isAvailable,
        string unavailableReason,
        CombatActionCategory category,
        CombatActionTargetKind targetKind,
        CombatActionTargetViewData[] targets,
        string effectSummary = "")
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Combat action ID is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(actorId))
            throw new ArgumentException("Combat action actor ID is required.", nameof(actorId));

        Id = id;
        ActorId = actorId;
        Label = label ?? string.Empty;
        CostLabel = costLabel ?? string.Empty;
        Description = description ?? string.Empty;
        EffectSummary = effectSummary ?? string.Empty;
        IsAvailable = isAvailable;
        UnavailableReason = unavailableReason ?? string.Empty;
        Category = category;
        TargetKind = targetKind;
        Targets = targets ?? Array.Empty<CombatActionTargetViewData>();
    }
}

/// <summary>A UI selection intent. The composition/flow boundary converts it into a Domain request.</summary>
public sealed class CombatActionSelection
{
    public string ActionId { get; }
    public string ActorId { get; }
    public string TargetId { get; }

    public CombatActionSelection(string actionId, string actorId, string targetId)
    {
        if (string.IsNullOrWhiteSpace(actionId))
            throw new ArgumentException("Combat action ID is required.", nameof(actionId));
        if (string.IsNullOrWhiteSpace(actorId))
            throw new ArgumentException("Combat action actor ID is required.", nameof(actorId));

        ActionId = actionId;
        ActorId = actorId;
        TargetId = targetId ?? string.Empty;
    }
}

/// <summary>Contains presentation-only feedback information for a combat action.</summary>
public sealed class CombatFeedbackViewData
{
    public string ActorName { get; }
    public string TargetName { get; }
    public CombatFeedbackType Type { get; }

    public CombatFeedbackViewData(string actorName, string targetName, CombatFeedbackType type)
    {
        ActorName = actorName ?? string.Empty;
        TargetName = targetName ?? string.Empty;
        Type = type;
    }
}

/// <summary>Contains one formatted message for the combat history UI.</summary>
public sealed class CombatMessage
{
    public string Id { get; }
    public int Round { get; }
    public CombatMessageType Type { get; }
    public CombatMessageCategory Category { get; }
    public string Speaker { get; }
    public string Text { get; }

    public CombatMessage(
        string id,
        int round,
        CombatMessageType type,
        CombatMessageCategory category,
        string speaker,
        string text)
    {
        Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id;
        Round = round;
        Type = type;
        Category = category;
        Speaker = speaker ?? string.Empty;
        Text = text ?? string.Empty;
    }
}

/// <summary>Contains only Presentation-owned data required to render the combat interface.</summary>
public sealed class CombatViewData
{
    public int Round { get; }
    public bool IsPlayerTurn { get; }
    public int CurrentActionPoints { get; }
    public int ActionCapacity { get; }
    public string ActiveTurnLabel { get; }
    public EnemyCardViewData[] Enemies { get; }
    public PartyMemberCardViewData[] Party { get; }
    public CombatActionCardViewData[] AvailableActions { get; }
    public CombatMessage[] NewMessages { get; }
    public CombatFeedbackViewData[] Feedback { get; }

    public CombatViewData(
        int round,
        bool isPlayerTurn,
        int currentActionPoints,
        int actionCapacity,
        string activeTurnLabel,
        EnemyCardViewData[] enemies,
        PartyMemberCardViewData[] party,
        CombatActionCardViewData[] availableActions,
        CombatMessage[] newMessages,
        CombatFeedbackViewData[] feedback = null)
    {
        Round = round;
        IsPlayerTurn = isPlayerTurn;
        CurrentActionPoints = currentActionPoints;
        ActionCapacity = actionCapacity;
        ActiveTurnLabel = activeTurnLabel ?? string.Empty;
        Enemies = enemies ?? Array.Empty<EnemyCardViewData>();
        Party = party ?? Array.Empty<PartyMemberCardViewData>();
        AvailableActions = availableActions ?? Array.Empty<CombatActionCardViewData>();
        NewMessages = newMessages ?? Array.Empty<CombatMessage>();
        Feedback = feedback ?? Array.Empty<CombatFeedbackViewData>();
    }
}
