using System;
using System.Collections.Generic;

/// <summary>Provides indexed access to the configured game content definitions.</summary>
public sealed class GameContentCatalog
{
    private readonly LocationCatalog _locations;

    /// <summary>Gets the characters.</summary>
    public CharacterCatalog Characters { get; }
    /// <summary>Gets the combat actions.</summary>
    public CombatActionCatalog CombatActions { get; }
    /// <summary>Gets the npc schedules.</summary>
    public NpcScheduleCatalog NpcSchedules { get; }
    /// <summary>Gets the npc interactions.</summary>
    public NpcInteractionCatalog NpcInteractions { get; }

    /// <summary>Gets the locations.</summary>
    public IEnumerable<LocationContent> Locations => _locations.All;
    /// <summary>Gets the travel.</summary>
    public ActionDefinition Travel => _locations.Travel;
    /// <summary>Gets the combat encounter.</summary>
    public ActionDefinition CombatEncounter => _locations.CombatEncounter;

    /// <summary>Initializes a new instance of <see cref="GameContentCatalog"/>.</summary>
    /// <param name="locations">The locations used to initialize the instance.</param>
    /// <param name="characters">The characters used to initialize the instance.</param>
    /// <param name="combatActions">The combat actions used to initialize the instance.</param>
    /// <param name="npcSchedules">The npc schedules used to initialize the instance.</param>
    /// <param name="npcInteractions">The npc interactions used to initialize the instance.</param>
    public GameContentCatalog(
        LocationCatalog locations,
        CharacterCatalog characters,
        CombatActionCatalog combatActions,
        NpcScheduleCatalog npcSchedules,
        NpcInteractionCatalog npcInteractions = null)
    {
        _locations = locations ?? throw new ArgumentNullException(nameof(locations));
        Characters = characters ?? throw new ArgumentNullException(nameof(characters));
        CombatActions = combatActions ?? throw new ArgumentNullException(nameof(combatActions));
        NpcSchedules = npcSchedules ?? throw new ArgumentNullException(nameof(npcSchedules));
        NpcInteractions = npcInteractions ?? new NpcInteractionCatalog(Array.Empty<NpcInteractionDefinition>());

        foreach (CharacterData character in Characters.All)
        {
            CombatActions.Resolve(character.CombatActionIds);
        }

        ValidateNpcScheduleReferences();
        ValidateNpcInteractionReferences();
        ValidateRequiredNpcReferences();
        ValidateInteractablePlayerActionReferences();
    }

    /// <summary>Returns location.</summary>
    /// <param name="id">The id used by this operation.</param>
    /// <returns>The resulting location content.</returns>
    public LocationContent GetLocation(string id)
    {
        return _locations.Get(id);
    }

    /// <summary>Returns character.</summary>
    /// <param name="id">The id used by this operation.</param>
    /// <returns>The resulting character data.</returns>
    public CharacterData GetCharacter(string id)
    {
        return Characters.Get(id);
    }

    /// <summary>Returns combat actions for character.</summary>
    /// <param name="characterId">The character id used by this operation.</param>
    /// <returns>The resulting combat action definition.</returns>
    public CombatActionDefinition[] GetCombatActionsForCharacter(string characterId)
    {
        CharacterData character = Characters.Get(characterId);
        return CombatActions.Resolve(character.CombatActionIds);
    }

    private void ValidateInteractablePlayerActionReferences()
    {
        bool hasPlayerActionReferences = false;
        foreach (LocationContent location in Locations)
        {
            foreach (LocationInteractableDefinition interactable in location.Interactables)
            {
                if (interactable.PlayerActionIds.Length > 0)
                {
                    hasPlayerActionReferences = true;
                    break;
                }
            }

            if (hasPlayerActionReferences)
            {
                break;
            }
        }

        if (!hasPlayerActionReferences)
        {
            return;
        }

        CharacterData player = Characters.Get("player_student");
        foreach (LocationContent location in Locations)
        {
            foreach (LocationInteractableDefinition interactable in location.Interactables)
            {
                foreach (string playerActionId in interactable.PlayerActionIds)
                {
                    bool actionFound = false;
                    foreach (PlayerActionDefinition playerAction in player.PlayerActions)
                    {
                        if (playerAction.Action.Id == playerActionId &&
                            playerAction.IsAvailableAt(location.Id))
                        {
                            actionFound = true;
                            break;
                        }
                    }

                    if (!actionFound)
                    {
                        throw new InvalidOperationException(
                            "Interactable '" + interactable.Id + "' in location '" + location.Id
                            + "' references unavailable Player Action '" + playerActionId + "'.");
                    }
                }
            }
        }
    }

    private void ValidateNpcScheduleReferences()
    {
        foreach (NpcScheduleDefinition schedule in NpcSchedules.All)
        {
            Characters.Get(schedule.CharacterId);
            foreach (NpcScheduleEntryDefinition entry in schedule.Entries)
            {
                _locations.Get(entry.LocationId);
            }
        }
    }

    private void ValidateNpcInteractionReferences()
    {
        foreach (NpcInteractionDefinition interaction in NpcInteractions.All)
        {
            Characters.Get(interaction.CharacterId);
            if (!NpcSchedules.TryGet(interaction.CharacterId, out _))
            {
                throw new InvalidOperationException(
                    "NPC interaction references Character '" + interaction.CharacterId
                    + "', but that Character has no daily schedule.");
            }

            foreach (string locationId in interaction.AvailableLocationIds)
            {
                _locations.Get(locationId);
            }
        }
    }

    private void ValidateRequiredNpcReferences()
    {
        foreach (LocationContent location in Locations)
        {
            foreach (ActionDefinition action in location.Actions)
            {
                ValidateRequiredNpcReference(action);
            }

            foreach (LocationInteractableDefinition interactable in location.Interactables)
            {
                foreach (ActionDefinition action in interactable.Actions)
                {
                    ValidateRequiredNpcReference(action);
                }
            }

            foreach (LocationEventDefinition locationEvent in location.Events)
            {
                ValidateRequiredNpcReference(locationEvent.RequiredNpcId, "Event '" + locationEvent.Id + "'");
                foreach (LocationEventChoiceDefinition choice in locationEvent.Choices)
                {
                    ValidateRequiredNpcReference(choice.Action);
                }
            }
        }
    }

    private void ValidateRequiredNpcReference(ActionDefinition action)
    {
        if (string.IsNullOrWhiteSpace(action.RequiredNpcId))
        {
            return;
        }

        ValidateRequiredNpcReference(action.RequiredNpcId, "Action '" + action.Id + "'");
    }

    private void ValidateRequiredNpcReference(string requiredNpcId, string sourceDescription)
    {
        if (string.IsNullOrWhiteSpace(requiredNpcId))
        {
            return;
        }

        if (!NpcSchedules.TryGet(requiredNpcId, out _))
        {
            throw new InvalidOperationException(
                sourceDescription + " requires NPC '" + requiredNpcId
                + "', but no schedule is defined for that Character ID.");
        }
    }
}
