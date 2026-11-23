using System;

/// <summary>Identifies the source that provides an available player interaction.</summary>
public enum InteractionSourceKind
{
    /// <summary>The interaction is an action owned by the player.</summary>
    Player,
    /// <summary>The interaction is provided by the current location.</summary>
    Location,
    /// <summary>The interaction is provided by an NPC.</summary>
    Npc,
    /// <summary>The interaction moves the player to another location.</summary>
    Travel,
    /// <summary>The interaction is a choice offered by a location event.</summary>
    Event,
    /// <summary>The interaction is provided by a selectable object within a Location.</summary>
    Interactable,
    /// <summary>The interaction selects a non-adjacent outdoor fast-travel destination.</summary>
    FastTravel
}

/// <summary>Provides available interaction functionality for the application.</summary>
public sealed class AvailableInteraction
{
    /// <summary>Gets the source kind.</summary>
    public InteractionSourceKind SourceKind { get; }
    /// <summary>Gets the source id.</summary>
    public string SourceId { get; }
    /// <summary>Gets the action id.</summary>
    public string ActionId { get; }
    /// <summary>Gets the target id.</summary>
    public string TargetId { get; }
    /// <summary>Gets the action.</summary>
    public ActionDefinition Action { get; }

    /// <summary>Initializes a new instance of <see cref="AvailableInteraction"/>.</summary>
    /// <param name="sourceKind">The source kind used to initialize the instance.</param>
    /// <param name="sourceId">The source id used to initialize the instance.</param>
    /// <param name="actionId">The action id used to initialize the instance.</param>
    /// <param name="targetId">The target id used to initialize the instance.</param>
    /// <param name="action">The action used to initialize the instance.</param>
    public AvailableInteraction(
        InteractionSourceKind sourceKind,
        string sourceId,
        string actionId,
        string targetId,
        ActionDefinition action)
    {
        if (!Enum.IsDefined(typeof(InteractionSourceKind), sourceKind))
            throw new ArgumentOutOfRangeException(nameof(sourceKind));
        if (string.IsNullOrWhiteSpace(sourceId)) throw new ArgumentException("Interaction source ID is required.", nameof(sourceId));
        if (string.IsNullOrWhiteSpace(actionId)) throw new ArgumentException("Interaction action ID is required.", nameof(actionId));
        SourceKind = sourceKind;
        SourceId = sourceId;
        ActionId = actionId;
        TargetId = targetId ?? string.Empty;
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }
}

/// <summary>Provides action selection request functionality for the application.</summary>
public sealed class ActionSelectionRequest
{
    /// <summary>Gets the source kind.</summary>
    public InteractionSourceKind SourceKind { get; }
    /// <summary>Gets the source id.</summary>
    public string SourceId { get; }
    /// <summary>Gets the action id.</summary>
    public string ActionId { get; }
    /// <summary>Gets the target id.</summary>
    public string TargetId { get; }

    /// <summary>Initializes a new instance of <see cref="ActionSelectionRequest"/>.</summary>
    /// <param name="sourceKind">The source kind used to initialize the instance.</param>
    /// <param name="sourceId">The source id used to initialize the instance.</param>
    /// <param name="actionId">The action id used to initialize the instance.</param>
    /// <param name="targetId">The target id used to initialize the instance.</param>
    public ActionSelectionRequest(
        InteractionSourceKind sourceKind,
        string sourceId,
        string actionId,
        string targetId = "")
    {
        if (!Enum.IsDefined(typeof(InteractionSourceKind), sourceKind))
            throw new ArgumentOutOfRangeException(nameof(sourceKind));
        if (string.IsNullOrWhiteSpace(sourceId)) throw new ArgumentException("Selection source ID is required.", nameof(sourceId));
        if (string.IsNullOrWhiteSpace(actionId)) throw new ArgumentException("Selection action ID is required.", nameof(actionId));
        SourceKind = sourceKind;
        SourceId = sourceId;
        ActionId = actionId;
        TargetId = targetId ?? string.Empty;
    }

    /// <summary>Encodes the selection request as a string suitable for UI transport.</summary>
    /// <returns>The resulting string.</returns>
    public string Encode()
    {
        return "choice|" + Uri.EscapeDataString(SourceKind.ToString())
            + "|" + Uri.EscapeDataString(SourceId)
            + "|" + Uri.EscapeDataString(ActionId)
            + "|" + Uri.EscapeDataString(TargetId);
    }

    /// <summary>Attempts to parse and reports whether the operation succeeded.</summary>
    /// <param name="value">The value used by this operation.</param>
    /// <param name="request">The request used by this operation.</param>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string value, out ActionSelectionRequest request)
    {
        request = null;
        if (string.IsNullOrWhiteSpace(value)) return false;
        string[] parts = value.Split('|');
        if (parts.Length != 5 || parts[0] != "choice") return false;
        if (!Enum.TryParse(Uri.UnescapeDataString(parts[1]), true, out InteractionSourceKind sourceKind)
            || !Enum.IsDefined(typeof(InteractionSourceKind), sourceKind)) return false;
        string sourceId = Uri.UnescapeDataString(parts[2]);
        string actionId = Uri.UnescapeDataString(parts[3]);
        string targetId = Uri.UnescapeDataString(parts[4]);
        if (string.IsNullOrWhiteSpace(sourceId) || string.IsNullOrWhiteSpace(actionId)) return false;
        request = new ActionSelectionRequest(sourceKind, sourceId, actionId, targetId);
        return true;
    }
}
