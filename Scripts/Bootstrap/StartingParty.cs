using System;
using System.Collections.Generic;

public static class StartingParty
{
    private static readonly string[] _characterIds =
    {
        "player_student",
        "noah",
        "mira",
        "sera"
    };

    public static CombatantDefinition[] CreateCombatants(
        GameContentCatalog content,
        Character primaryCharacter)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (primaryCharacter == null)
        {
            throw new ArgumentNullException(nameof(primaryCharacter));
        }

        if (Array.IndexOf(_characterIds, primaryCharacter.Id) < 0)
        {
            throw new ArgumentException(
                "The primary character must belong to the starting party.",
                nameof(primaryCharacter));
        }

        List<CombatantDefinition> combatants =
            new List<CombatantDefinition>(_characterIds.Length);

        foreach (string characterId in _characterIds)
        {
            CharacterData characterData = content.GetCharacter(characterId);
            Character character = characterId == primaryCharacter.Id
                ? primaryCharacter
                : new Character(characterData);

            combatants.Add(new CombatantDefinition(
                character,
                CombatantSide.Party,
                content.GetCombatActionsForCharacter(characterId)));
        }

        return combatants.ToArray();
    }
}
