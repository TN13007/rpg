using System;
using System.Collections.Generic;

public sealed class GameContentCatalog
{
    private readonly AcademyContent _academy;

    public CharacterCatalog Characters { get; }
    public CombatActionCatalog CombatActions { get; }
    public NpcScheduleCatalog NpcSchedules { get; }
    public NpcInteractionCatalog NpcInteractions { get; }

    public IEnumerable<LocationContent> Locations => _academy.All;
    public ActionDefinition Travel => _academy.Travel;
    public ActionDefinition CombatEncounter => _academy.CombatEncounter;

    public GameContentCatalog(
        AcademyContent academy,
        CharacterCatalog characters,
        CombatActionCatalog combatActions,
        NpcScheduleCatalog npcSchedules,
        NpcInteractionCatalog npcInteractions = null)
    {
        _academy = academy ?? throw new ArgumentNullException(nameof(academy));
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
    }

    public LocationContent GetLocation(string id)
    {
        return _academy.Get(id);
    }

    public CharacterData GetCharacter(string id)
    {
        return Characters.Get(id);
    }

    public CombatActionDefinition[] GetCombatActionsForCharacter(string characterId)
    {
        CharacterData character = Characters.Get(characterId);
        return CombatActions.Resolve(character.CombatActionIds);
    }

    private void ValidateNpcScheduleReferences()
    {
        foreach (NpcScheduleDefinition schedule in NpcSchedules.All)
        {
            Characters.Get(schedule.CharacterId);
            foreach (NpcScheduleEntryDefinition entry in schedule.Entries)
            {
                _academy.Get(entry.LocationId);
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
                _academy.Get(locationId);
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
