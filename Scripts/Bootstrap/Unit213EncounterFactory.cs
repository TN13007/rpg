using System;
using System.Collections.Generic;

/// <summary>Builds the playable Unit 213 encounter roster from authored game content.</summary>
public static class Unit213EncounterFactory
{
    /// <summary>Creates the four-member starting party and two Unit 213 manifestations with distinct threat profiles.</summary>
    /// <param name="content">The validated game content catalog containing the party and manifestation definitions.</param>
    /// <param name="primaryCharacter">The already-created primary party character whose runtime state must be preserved.</param>
    /// <returns>The complete encounter roster in party-first, enemy-second order.</returns>
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
            behaviorProfile: EnemyBehaviorProfiles.CreateHeavyPressurePresence()));

        CharacterData echoData = content.GetCharacter("hostile_echo");
        combatants.Add(new CombatantDefinition(
            new Character(echoData),
            CombatantSide.Enemy,
            content.GetCombatActionsForCharacter(echoData.Id),
            behaviorProfile: EnemyBehaviorProfiles.CreateDisruptiveEcho()));

        return combatants.ToArray();
    }
}
