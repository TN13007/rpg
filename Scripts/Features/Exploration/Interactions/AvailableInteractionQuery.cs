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
    /// <returns>A read-only list of available interactions. When the Player cannot act, only eligible recovery actions are returned.</returns>
    public IReadOnlyList<AvailableInteraction> Query(
        Character player,
        IReadOnlyList<PlayerActionDefinition> playerActions,
        LocationContent location,
        Func<string, LocationContent> locationResolver,
        ActionDefinition travelAction,
        ISet<string> presentNpcIds = null)
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
            return interactions.AsReadOnly();
        }

        AddLocationActions(interactions, location, presentNpcIds);
        AddPlayerActions(interactions, player, playerActions, location.Id);
        AddTravelActions(interactions, location, locationResolver, travelAction);

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
        ISet<string> presentNpcIds)
    {
        foreach (ActionDefinition action in location.Actions)
        {
            if (!IsEventEncounterAction(location, action.Id) && IsNpcRequirementSatisfied(action, presentNpcIds))
            {
                interactions.Add(new AvailableInteraction(
                    InteractionSourceKind.Location, location.Id, action.Id, string.Empty, action));
            }
        }
    }

    private static void AddPlayerActions(
        List<AvailableInteraction> interactions,
        Character player,
        IReadOnlyList<PlayerActionDefinition> playerActions,
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
        IReadOnlyList<PlayerActionDefinition> playerActions,
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
