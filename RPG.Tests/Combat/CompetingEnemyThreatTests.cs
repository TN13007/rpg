using System;
using Xunit;

public sealed class CompetingEnemyThreatTests
{
    [Fact]
    public void EncounterCombinesHeavyDamageAndStunThreatsAtTheSameTimelineTimestamp()
    {
        Character partyMember = CreateCharacter("party-tactician", "Tactician", agility: 8, luck: 20);
        Character heavyEnemy = CreateCharacter("enemy-heavy", "Heavy Brute", agility: 8, luck: 5);
        Character controlEnemy = CreateCharacter("enemy-controller", "Disruptor", agility: 8, luck: 5);

        CombatActionDefinition quickStrike = new CombatActionDefinition(
            "party_quick_strike",
            "Quick Strike",
            "No Stamina cost",
            "Deal a small amount of damage.",
            CombatActionBehavior.Attack,
            0,
            8,
            0,
            string.Empty,
            "The party member strikes.",
            recoveryCost: 70);

        CombatActionDefinition heavySwing = new CombatActionDefinition(
            "heavy_swing",
            "Heavy Swing",
            "Enemy action",
            "A slow, high-damage attack.",
            CombatActionBehavior.Attack,
            0,
            12,
            0,
            string.Empty,
            "The Heavy Brute commits to a heavy swing.",
            recoveryCost: 130);

        CombatActionDefinition staggeringBlow = new CombatActionDefinition(
            "staggering_blow",
            "Staggering Blow",
            "Enemy action",
            "A light hit that briefly stuns its target.",
            CombatActionBehavior.Stun,
            0,
            3,
            0,
            string.Empty,
            "The Disruptor lands a staggering blow.",
            recoveryCost: 115,
            stunDuration: 0.5);

        CombatService combat = new CombatService(
            new[]
            {
                new CombatantDefinition(partyMember, CombatantSide.Party, new[] { quickStrike }),
                new CombatantDefinition(heavyEnemy, CombatantSide.Enemy, new[] { heavySwing }),
                new CombatantDefinition(controlEnemy, CombatantSide.Enemy, new[] { staggeringBlow })
            },
            new Random(7));

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);
        combat.AdvanceCombatTime(10.0);

        Assert.True(combat.IsAwaitingPlayerDecision);
        Assert.Equal(partyMember.Id, combat.ActiveCombatantId);

        CombatResult partyAction = combat.ExecuteAction(
            new CombatActionRequest(quickStrike.Id, heavyEnemy.Id, partyMember.Id));

        Assert.True(partyAction.Success);
        Assert.False(combat.IsEncounterOver);

        CombatResult enemyBatch = combat.AdvanceCombatTime(0.0);

        Assert.Contains(enemyBatch.Events, item =>
            item.Type == CombatEventType.EnemyAction &&
            item.ActorName == "HEAVY BRUTE" &&
            item.Detail == heavySwing.ActionDetail);
        Assert.Contains(enemyBatch.Events, item =>
            item.Type == CombatEventType.EnemyAction &&
            item.ActorName == "DISRUPTOR" &&
            item.Detail == staggeringBlow.ActionDetail);
        Assert.Contains(enemyBatch.Events, item =>
            item.Type == CombatEventType.Damage &&
            item.Amount == 12);
        Assert.Contains(enemyBatch.Events, item =>
            item.Type == CombatEventType.StatusChanged &&
            item.Detail.Contains("stunned", StringComparison.OrdinalIgnoreCase));

        Assert.Equal(85, partyMember.Health);
        Assert.False(combat.IsEncounterOver);
    }

    private static Character CreateCharacter(string id, string name, int agility, int luck)
    {
        CharacterData data = new CharacterData(
            id,
            name,
            "Competing enemy threat test combatant",
            1,
            100,
            100,
            10,
            100,
            100,
            0,
            10,
            50,
            0,
            new CharacterStats(20, agility, 20, 20, 20, luck));

        return new Character(data);
    }
}
