using System;

/// <summary>Identifies the speaker of a presentation combat message.</summary>
public enum CombatMessageType
{
    /// <summary>A Party member performed the action.</summary>
    Player,
    /// <summary>An Enemy performed the action.</summary>
    Enemy,
    /// <summary>The message describes a system event.</summary>
    System
}

/// <summary>Identifies the category of a presentation combat message.</summary>
public enum CombatMessageCategory
{
    /// <summary>No specialized category.</summary>
    None,
    /// <summary>Damage was applied.</summary>
    Damage,
    /// <summary>A gameplay check was resolved.</summary>
    Check,
    /// <summary>A turn changed.</summary>
    Turn,
    /// <summary>The environment changed.</summary>
    Environment,
    /// <summary>A status changed.</summary>
    StatusChange,
    /// <summary>A new Round began.</summary>
    Round
}

/// <summary>Identifies a visual feedback cue for a combatant.</summary>
public enum CombatFeedbackType
{
    /// <summary>An attack was performed.</summary>
    Attack,
    /// <summary>The combatant guarded.</summary>
    Guard,
    /// <summary>The combatant evaded.</summary>
    Evade,
    /// <summary>The combatant recovered Stamina.</summary>
    Recover,
    /// <summary>The combatant revived an ally.</summary>
    Revive,
    /// <summary>The combatant was hit.</summary>
    Hit
}

/// <summary>Contains presentation-only feedback information for a combat action.</summary>
public sealed class CombatFeedbackViewData
{
    /// <summary>Gets the acting combatant's display name.</summary>
    public string ActorName { get; }
    /// <summary>Gets the target combatant's display name.</summary>
    public string TargetName { get; }
    /// <summary>Gets the feedback cue.</summary>
    public CombatFeedbackType Type { get; }

    /// <summary>Creates combat feedback view data.</summary>
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
    /// <summary>Gets the message identifier.</summary>
    public string Id { get; }
    /// <summary>Gets the Round number associated with the message.</summary>
    public int Round { get; }
    /// <summary>Gets the speaker type.</summary>
    public CombatMessageType Type { get; }
    /// <summary>Gets the message category.</summary>
    public CombatMessageCategory Category { get; }
    /// <summary>Gets the speaker display label.</summary>
    public string Speaker { get; }
    /// <summary>Gets the formatted message text.</summary>
    public string Text { get; }

    /// <summary>Creates a combat history message.</summary>
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

/// <summary>Contains all data required to render the current combat interface.</summary>
public sealed class CombatViewData
{
    /// <summary>Gets the current Round number.</summary>
    public int Round { get; }
    /// <summary>Gets the current combat phase.</summary>
    public CombatPhase Phase { get; }
    /// <summary>Gets the current shared Party AP.</summary>
    public int CurrentActionPoints { get; }
    /// <summary>Gets the Party AP capacity.</summary>
    public int ActionCapacity { get; }
    /// <summary>Gets the short HUD label.</summary>
    public string ActiveTurnLabel { get; }
    /// <summary>Gets the Enemy status data.</summary>
    public EnemyCombatStatus[] Enemies { get; }
    /// <summary>Gets the Party status data.</summary>
    public PartyCombatStatus[] Party { get; }
    /// <summary>Gets the action cards available to eligible Party members.</summary>
    public CombatActionOption[] AvailableActions { get; }
    /// <summary>Gets new messages to append to the history.</summary>
    public CombatMessage[] NewMessages { get; }
    /// <summary>Gets the combatant ID used for the persistent character panel, when one is selected.</summary>
    public string ActiveCombatantId { get; }
    /// <summary>Gets visual feedback cues emitted by the latest operation.</summary>
    public CombatFeedbackViewData[] Feedback { get; }

    /// <summary>Creates the presentation model from Domain snapshot data and mapped events.</summary>
    public CombatViewData(
        int round,
        CombatPhase phase,
        int currentActionPoints,
        int actionCapacity,
        string activeTurnLabel,
        EnemyCombatStatus[] enemies,
        PartyCombatStatus[] party,
        CombatActionOption[] availableActions,
        CombatMessage[] newMessages,
        string activeCombatantId = "",
        CombatFeedbackViewData[] feedback = null)
    {
        Round = round;
        Phase = phase;
        CurrentActionPoints = currentActionPoints;
        ActionCapacity = actionCapacity;
        ActiveTurnLabel = activeTurnLabel ?? string.Empty;
        Enemies = enemies ?? Array.Empty<EnemyCombatStatus>();
        Party = party ?? Array.Empty<PartyCombatStatus>();
        AvailableActions = availableActions ?? Array.Empty<CombatActionOption>();
        NewMessages = newMessages ?? Array.Empty<CombatMessage>();
        ActiveCombatantId = activeCombatantId ?? string.Empty;
        Feedback = feedback ?? Array.Empty<CombatFeedbackViewData>();
    }
}