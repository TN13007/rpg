using System;
using System.Collections.Generic;

public sealed class GameContentCatalog
{
    private readonly AcademyContent _academy;

    public CharacterCatalog Characters { get; }
    public CombatActionCatalog CombatActions { get; }

    public IEnumerable<LocationContent> Locations => _academy.All;
    public ActionDefinition Travel => _academy.Travel;
    public ActionDefinition CombatEncounter => _academy.CombatEncounter;

    public GameContentCatalog(
        AcademyContent academy,
        CharacterCatalog characters,
        CombatActionCatalog combatActions)
    {
        _academy = academy ?? throw new ArgumentNullException(nameof(academy));
        Characters = characters ?? throw new ArgumentNullException(nameof(characters));
        CombatActions = combatActions ?? throw new ArgumentNullException(nameof(combatActions));

        // Resolve every character reference during loading, before gameplay starts.
        foreach (CharacterData character in Characters.All)
        {
            CombatActions.Resolve(character.CombatActionIds);
        }
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
}
