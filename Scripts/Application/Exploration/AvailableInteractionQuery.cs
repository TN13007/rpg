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

        if (!player.CanAct)
        {
            AddRecoveryActions(interactions, player, playerActions, location.Id);
            return interactions;
        }

        AddLocationActions(interactions, location);
        AddPlayerActions(interactions, player, playerActions, location.Id);
        AddTravelActions(interactions, location, locationResolver, travelAction);

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

    private static void AddLocationActions(
        List<AvailableInteraction> interactions,
        LocationContent location)
    {
        foreach (ActionDefinition action in location.Actions)
        {
            if (!IsEventEncounterAction(location, action.Id))
            {
                interactions.Add(new AvailableInteraction(
                    InteractionSourceKind.Location, location.Id, action.Id, string.Empty, action));
            }
        }
    }

    private static void AddPlayerActions(
        List<AvailableInteraction> interactions,
        Character player,
        PlayerActionDefinition[] playerActions,
        string locationId)
    {
        foreach (PlayerActionDefinition playerAction in playerActions)
        {
            if (playerAction != null &&
                playerAction.IsAvailableAt(locationId) &&
                !HasReviveEffect(playerAction.Action))
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

    private static void AddRecoveryActions(
        List<AvailableInteraction> interactions,
        Character player,
        PlayerActionDefinition[] playerActions,
        string locationId)
    {
        foreach (PlayerActionDefinition playerAction in playerActions)
        {
            if (playerAction != null &&
                playerAction.IsAvailableAt(locationId) &&
                IsRecoveryAction(playerAction.Action))
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

    private static void AddTravelActions(
        List<AvailableInteraction> interactions,
        LocationContent location,
        Func<string, LocationContent> locationResolver,
        ActionDefinition travelAction)
    {
        foreach (string exitId in location.Exits)
        {
            LocationContent destination = locationResolver(exitId);
            ActionDefinition travelChoice = new ActionDefinition(
                travelAction.Id,
                "Travel to " + destination.Name,
                travelAction.DurationMinutes,
                travelAction.TimeModifier,
                travelAction.Feedback,
                travelAction.ActivityMet,
                travelAction.Effects,
                travelAction.EstimatedDurationMinMinutes,
                travelAction.EstimatedDurationMaxMinutes);
            interactions.Add(new AvailableInteraction(
                InteractionSourceKind.Travel, location.Id, travelAction.Id, exitId, travelChoice));
        }
    }

    private static bool HasReviveEffect(ActionDefinition action)
    {
        foreach (ActionEffectDefinition effect in action.Effects)
        {
            if (effect.Type == ActionEffectType.Revive)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsRecoveryAction(ActionDefinition action)
    {
        return action.Effects.Length == 1
            && action.Effects[0].Type == ActionEffectType.Revive
            && action.Effects[0].Amount > 0;
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
