using System;

public enum InteractionSourceKind
{
    Player,
    Location,
    Npc,
    Travel,
    Event
}

public sealed class AvailableInteraction
{
    public InteractionSourceKind SourceKind { get; }
    public string SourceId { get; }
    public string ActionId { get; }
    public string TargetId { get; }
    public ActionDefinition Action { get; }

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

public sealed class ActionSelectionRequest
{
    public InteractionSourceKind SourceKind { get; }
    public string SourceId { get; }
    public string ActionId { get; }
    public string TargetId { get; }

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

    public string Encode()
    {
        return "choice|" + Uri.EscapeDataString(SourceKind.ToString())
            + "|" + Uri.EscapeDataString(SourceId)
            + "|" + Uri.EscapeDataString(ActionId)
            + "|" + Uri.EscapeDataString(TargetId);
    }

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
