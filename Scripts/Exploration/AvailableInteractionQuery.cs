using System;
using System.Collections.Generic;

public enum InteractionSourceKind
{
    Player,
    Location,
    Travel
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

public sealed class AvailableInteractionQuery
{
    public IReadOnlyList<AvailableInteraction> Query(
        Character player,
        PlayerActionDefinition[] playerActions,
        LocationContent location)
    {
        if (player == null) throw new ArgumentNullException(nameof(player));
        if (playerActions == null) throw new ArgumentNullException(nameof(playerActions));
        if (location == null) throw new ArgumentNullException(nameof(location));

        List<AvailableInteraction> interactions = new List<AvailableInteraction>();

        foreach (ActionDefinition action in location.Actions)
        {
            if (!IsEventEncounterAction(location, action.Id))
            {
                interactions.Add(new AvailableInteraction(
                    InteractionSourceKind.Location, location.Id, action.Id, string.Empty, action));
            }
        }

        if (player.CanAct)
        {
            foreach (PlayerActionDefinition playerAction in playerActions)
            {
                if (playerAction != null && playerAction.IsAvailableAt(location.Id))
                {
                    interactions.Add(new AvailableInteraction(
                        InteractionSourceKind.Player,
                        player.Id,
                        playerAction.Action.Id,
                        string.Empty,
                        playerAction.Action));
                }
            }
        }

        foreach (string exitId in location.Exits)
        {
            LocationContent destination = AcademyContent.Get(exitId);
            ActionDefinition travel = AcademyContent.Travel;
            ActionDefinition travelChoice = new ActionDefinition(
                travel.Id,
                "Travel to " + destination.Name,
                travel.DurationMinutes,
                travel.TimeModifier,
                travel.Feedback,
                travel.ActivityMet,
                travel.Effects,
                travel.TimeMode,
                travel.EstimatedDurationMinMinutes,
                travel.EstimatedDurationMaxMinutes);
            interactions.Add(new AvailableInteraction(
                InteractionSourceKind.Travel, location.Id, travel.Id, exitId, travelChoice));
        }

        return interactions;
    }

    private static bool IsEventEncounterAction(LocationContent location, string actionId)
    {
        foreach (LocationEventDefinition locationEvent in location.Events)
        {
            foreach (LocationEventChoiceDefinition choice in locationEvent.Choices)
            {
                if (choice.EncounterActionId == actionId) return true;
            }
        }
        return false;
    }
}

public sealed class ActionChoiceMapper
{
    private readonly ActionDurationEstimateMapper _durationMapper;

    public ActionChoiceMapper(ActionDurationEstimateMapper durationMapper)
    {
        _durationMapper = durationMapper ?? throw new ArgumentNullException(nameof(durationMapper));
    }

    public List<ActionChoiceViewData> Map(IReadOnlyList<AvailableInteraction> interactions, Character player)
    {
        if (interactions == null) throw new ArgumentNullException(nameof(interactions));
        if (player == null) throw new ArgumentNullException(nameof(player));

        List<ActionChoiceViewData> choices = new List<ActionChoiceViewData>();
        foreach (AvailableInteraction interaction in interactions)
        {
            ActionSelectionRequest request = new ActionSelectionRequest(
                interaction.SourceKind,
                interaction.SourceId,
                interaction.ActionId,
                interaction.TargetId);
            string groupTitle = interaction.SourceKind switch
            {
                InteractionSourceKind.Player => "PERSONAL",
                InteractionSourceKind.Location => "AT THIS LOCATION",
                InteractionSourceKind.Travel => "TRAVEL",
                _ => throw new ArgumentOutOfRangeException()
            };
            choices.Add(new ActionChoiceViewData(
                request.Encode(),
                interaction.Action.Label,
                _durationMapper.Map(interaction.Action, player),
                groupTitle));
        }
        return choices;
    }
}
