using System;
using System.Collections.Generic;

public sealed class AvailableInteractionQuery
{
    public IReadOnlyList<AvailableInteraction> Query(
        Character player,
        PlayerActionDefinition[] playerActions,
        LocationContent location,
        Func<string, LocationContent> locationResolver,
        ActionDefinition travelAction)
    {
        if (player == null) throw new ArgumentNullException(nameof(player));
        if (playerActions == null) throw new ArgumentNullException(nameof(playerActions));
        if (location == null) throw new ArgumentNullException(nameof(location));
        if (locationResolver == null) throw new ArgumentNullException(nameof(locationResolver));
        if (travelAction == null) throw new ArgumentNullException(nameof(travelAction));

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
            LocationContent destination = locationResolver(exitId);
            ActionDefinition travel = travelAction;
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

    public IReadOnlyList<AvailableInteraction> Query(LocationEventDefinition locationEvent)
    {
        if (locationEvent == null) throw new ArgumentNullException(nameof(locationEvent));

        List<AvailableInteraction> interactions = new List<AvailableInteraction>();
        foreach (LocationEventChoiceDefinition choice in locationEvent.Choices)
        {
            interactions.Add(new AvailableInteraction(
                InteractionSourceKind.Event,
                locationEvent.Id,
                choice.Id,
                choice.EncounterActionId ?? string.Empty,
                choice.Action));
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
