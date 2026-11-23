using System;
using System.Collections.Generic;

/// <summary>Builds the Unit 213 encounter roster from validated content data.</summary>
public static class Unit213EncounterFactory
{
    /// <summary>Creates the starting Party and the two manifestations with fixed authored action patterns.</summary>
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

        List<CombatantDefinition> combatants =
            new List<CombatantDefinition>(StartingParty.CreateCombatants(content, primaryCharacter));

        CharacterData presenceData = content.GetCharacter("hostile_presence");
        combatants.Add(new CombatantDefinition(
            new Character(presenceData),
            CombatantSide.Enemy,
            content.GetCombatActionsForCharacter(presenceData.Id),
            enemyPattern: new EnemyPattern(new[] { "presence_heavy_swing" })));

        CharacterData echoData = content.GetCharacter("hostile_echo");
        combatants.Add(new CombatantDefinition(
            new Character(echoData),
            CombatantSide.Enemy,
            content.GetCombatActionsForCharacter(echoData.Id),
            enemyPattern: new EnemyPattern(new[] { "presence_staggering_blow" })));

        return combatants.ToArray();
    }
}