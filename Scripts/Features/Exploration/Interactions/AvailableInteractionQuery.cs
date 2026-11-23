using System;
using System.Collections.Generic;

/// <summary>Retrieves available interaction information from the current game state.</summary>
public sealed class AvailableInteractionQuery
{
    /// <summary>Builds the currently available Player, Location and Travel choices for the supplied Exploration context.</summary>
    /// <param name="player">The controlled Character whose current state determines which actions are available.</param>
    /// <param name="playerActions">The authored Player Actions to consider for the current Location.</param>
    /// <param name="location">The current Location, including its Actions and exits.</param>
    /// <param name="locationResolver">Resolves each exit ID to the destination Location used to label a Travel choice.</param>
    /// <param name="travelAction">The shared Action definition that supplies Travel duration and effects.</param>
    /// <param name="presentNpcIds">NPC Character IDs currently present; actions with unmet NPC requirements are omitted.</param>
    /// <param name="currentMinuteOfDay">The current minute after midnight used to filter time-gated actions.</param>
    /// <param name="selectedInteractableId">Optional object ID; when supplied, only that object's actions are returned.</param>
    /// <returns>A read-only list of available interactions. When the Player cannot act, only eligible recovery actions are returned.</returns>
    public IReadOnlyList<AvailableInteraction> Query(
        Character player,
        IReadOnlyList<PlayerActionDefinition> playerActions,
        LocationContent location,
        Func<string, LocationContent> locationResolver,
        ActionDefinition travelAction,
        ISet<string> presentNpcIds = null,
        int? currentMinuteOfDay = null,
        string selectedInteractableId = null)
    {
        if (player == null) throw new ArgumentNullException(nameof(player));
        if (playerActions == null) throw new ArgumentNullException(nameof(playerActions));
        if (location == null) throw new ArgumentNullException(nameof(location));
        if (locationResolver == null) throw new ArgumentNullException(nameof(locationResolver));
        if (travelAction == null) throw new ArgumentNullException(nameof(travelAction));

        List<AvailableInteraction> interactions = new List<AvailableInteraction>();

        if (!player.CanAct)
        {
            AddRecoveryActions(interactions, player, playerActions, location.Id, currentMinuteOfDay);
            return interactions.AsReadOnly();
        }

        if (!string.IsNullOrWhiteSpace(selectedInteractableId))
        {
            AddInteractableActions(
                interactions, location, selectedInteractableId, playerActions, presentNpcIds, currentMinuteOfDay);
            return interactions.AsReadOnly();
        }

        AddLocationActions(interactions, location, presentNpcIds, currentMinuteOfDay);
        AddPlayerActions(interactions, player, playerActions, location, currentMinuteOfDay);
        AddInteractableTargets(interactions, location);
        AddTravelActions(interactions, location, locationResolver, travelAction);

        return interactions.AsReadOnly();
    }

    /// <summary>Builds fast-travel choices for eligible outdoor arrival Locations.</summary>
    /// <param name="locations">All authored Locations.</param>
    /// <param name="currentLocationId">The Location the player is leaving.</param>
    /// <param name="travelAction">The shared Travel action used for duration and resource estimates.</param>
    /// <returns>Eligible destinations only; interior Locations are excluded by their authored flag.</returns>
    public IReadOnlyList<AvailableInteraction> QueryFastTravelDestinations(
        IEnumerable<LocationContent> locations,
        string currentLocationId,
        ActionDefinition travelAction)
    {
        if (locations == null) throw new ArgumentNullException(nameof(locations));
        if (string.IsNullOrWhiteSpace(currentLocationId)) throw new ArgumentException("Current Location ID is required.", nameof(currentLocationId));
        if (travelAction == null) throw new ArgumentNullException(nameof(travelAction));

        List<AvailableInteraction> interactions = new List<AvailableInteraction>();
        foreach (LocationContent destination in locations)
        {
            if (destination == null || !destination.IsFastTravelDestination ||
                string.Equals(destination.Id, currentLocationId, StringComparison.Ordinal))
            {
                continue;
            }

            ActionDefinition choice = new ActionDefinition(
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
                InteractionSourceKind.FastTravel,
                currentLocationId,
                travelAction.Id,
                destination.Id,
                choice));
        }

        return interactions.AsReadOnly();
    }

    /// <summary>Returns the interactions or presence records matching the supplied criteria.</summary>
    /// <param name="locationEvent">The Event whose choices should be filtered by their NPC requirements.</param>
    /// <param name="presentNpcIds">The present npc ids used by this operation.</param>
    /// <returns>A read-only list containing the Event choices whose NPC requirements are currently satisfied.</returns>
    public IReadOnlyList<AvailableInteraction> Query(
        LocationEventDefinition locationEvent,
        ISet<string> presentNpcIds = null)
    {
        if (locationEvent == null) throw new ArgumentNullException(nameof(locationEvent));

        List<AvailableInteraction> interactions = new List<AvailableInteraction>();
        if (!IsNpcRequirementSatisfied(locationEvent.RequiredNpcId, presentNpcIds))
        {
            return interactions.AsReadOnly();
        }

        foreach (LocationEventChoiceDefinition choice in locationEvent.Choices)
        {
            if (!IsNpcRequirementSatisfied(choice.Action, presentNpcIds))
            {
                continue;
            }

            interactions.Add(new AvailableInteraction(
                InteractionSourceKind.Event,
                locationEvent.Id,
                choice.Id,
                choice.EncounterActionId ?? string.Empty,
                choice.Action));
        }
        return interactions.AsReadOnly();
    }

    private static void AddLocationActions(
        List<AvailableInteraction> interactions,
        LocationContent location,
        ISet<string> presentNpcIds,
        int? currentMinuteOfDay)
    {
        foreach (ActionDefinition action in location.Actions)
        {
            if (!IsEventEncounterAction(location, action.Id) &&
                IsNpcRequirementSatisfied(action, presentNpcIds) &&
                action.IsAvailableAt(currentMinuteOfDay))
            {
                interactions.Add(new AvailableInteraction(
                    InteractionSourceKind.Location, location.Id, action.Id, string.Empty, action));
            }
        }
    }

    private static void AddInteractableTargets(
        List<AvailableInteraction> interactions,
        LocationContent location)
    {
        foreach (LocationInteractableDefinition interactable in location.Interactables)
        {
            ActionDefinition selection = new ActionDefinition(
                "select",
                interactable.Name,
                0,
                feedback: interactable.Description,
                activityMet: 0);
            interactions.Add(new AvailableInteraction(
                InteractionSourceKind.Interactable,
                location.Id,
                "select",
                interactable.Id,
                selection));
        }
    }

    private static void AddInteractableActions(
        List<AvailableInteraction> interactions,
        LocationContent location,
        string interactableId,
        IReadOnlyList<PlayerActionDefinition> playerActions,
        ISet<string> presentNpcIds,
        int? currentMinuteOfDay)
    {
        LocationInteractableDefinition selected = null;
        foreach (LocationInteractableDefinition interactable in location.Interactables)
        {
            if (interactable.Id == interactableId)
            {
                selected = interactable;
                break;
            }
        }

        if (selected == null)
        {
            return;
        }

        foreach (string playerActionId in selected.PlayerActionIds)
        {
            foreach (PlayerActionDefinition playerAction in playerActions)
            {
                if (playerAction != null &&
                    playerAction.Action.Id == playerActionId &&
                    playerAction.IsAvailableAt(location.Id) &&
                    playerAction.Action.IsAvailableAt(currentMinuteOfDay) &&
                    !HasReviveEffect(playerAction.Action))
                {
                    interactions.Add(new AvailableInteraction(
                        InteractionSourceKind.Interactable,
                        location.Id,
                        playerAction.Action.Id,
                        selected.Id,
                        playerAction.Action));
                    break;
                }
            }
        }

        foreach (ActionDefinition action in selected.Actions)
        {
            if (action.IsAvailableAt(currentMinuteOfDay) &&
                IsNpcRequirementSatisfied(action, presentNpcIds))
            {
                interactions.Add(new AvailableInteraction(
                    InteractionSourceKind.Interactable,
                    location.Id,
                    action.Id,
                    selected.Id,
                    action));
            }
        }
    }

    private static void AddPlayerActions(
        List<AvailableInteraction> interactions,
        Character player,
        IReadOnlyList<PlayerActionDefinition> playerActions,
        LocationContent location,
        int? currentMinuteOfDay)
    {
        foreach (PlayerActionDefinition playerAction in playerActions)
        {
            if (playerAction != null &&
                playerAction.IsAvailableAt(location.Id) &&
                playerAction.Action.IsAvailableAt(currentMinuteOfDay) &&
                !IsExposedByInteractable(location, playerAction.Action.Id) &&
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

    private static bool IsExposedByInteractable(LocationContent location, string actionId)
    {
        foreach (LocationInteractableDefinition interactable in location.Interactables)
        {
            foreach (string playerActionId in interactable.PlayerActionIds)
            {
                if (playerActionId == actionId)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void AddRecoveryActions(
        List<AvailableInteraction> interactions,
        Character player,
        IReadOnlyList<PlayerActionDefinition> playerActions,
        string locationId,
        int? currentMinuteOfDay)
    {
        foreach (PlayerActionDefinition playerAction in playerActions)
        {
            if (playerAction != null &&
                playerAction.IsAvailableAt(locationId) &&
                playerAction.Action.IsAvailableAt(currentMinuteOfDay) &&
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
            string travelLabel = location.Id == "dormitory_room" && exitId == "dormitory_lobby"
                ? "Leave"
                : location.Id == "dormitory_room" && exitId == "dormitory_bathroom"
                    ? "Go to Bathroom"
                    : location.Id == "dormitory_lobby" && exitId == "university_campus"
                        ? "Go outside"
                        : "Travel to " + destination.Name;
            ActionDefinition travelChoice = new ActionDefinition(
                travelAction.Id,
                travelLabel,
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

    private static bool IsNpcRequirementSatisfied(ActionDefinition action, ISet<string> presentNpcIds)
    {
        return IsNpcRequirementSatisfied(action.RequiredNpcId, presentNpcIds);
    }

    private static bool IsNpcRequirementSatisfied(string requiredNpcId, ISet<string> presentNpcIds)
    {
        return string.IsNullOrWhiteSpace(requiredNpcId)
            || (presentNpcIds != null && presentNpcIds.Contains(requiredNpcId));
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
        return action.Effects.Count == 1
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
