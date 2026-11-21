using System;
using Xunit;

public sealed class CombatSameTimestampTests
{
    [Fact]
    public void StunPreventsAnEnemyAlreadyReadyAtTheSameTimestampFromActing()
    {
        Character partyMember = CreateCharacter("party-stunner", "Stunner", luck: 20);
        Character enemy = CreateCharacter("enemy-same-time", "Enemy", luck: 5);

        CombatActionDefinition stun = new CombatActionDefinition(
            "test_stun",
            "Stun",
            "1 Stamina",
            "Stun the enemy that is ready at the same timestamp.",
            CombatActionBehavior.Stun,
            1,
            0,
            0,
            "Not enough Stamina.",
            "The party member stuns the enemy.",
            recoveryCost: 100,
            stunDuration: 1.0);

        CombatActionDefinition enemyAttack = new CombatActionDefinition(
            "test_enemy_attack",
            "Enemy Attack",
            "Enemy action",
            "A basic enemy attack.",
            CombatActionBehavior.Attack,
            0,
            5,
            0,
            string.Empty,
            "The enemy attacks.",
            recoveryCost: 100);

        CombatService combat = new CombatService(
            new[]
            {
                new CombatantDefinition(partyMember, CombatantSide.Party, new[] { stun }),
                new CombatantDefinition(enemy, CombatantSide.Enemy, new[] { enemyAttack })
            },
            new Random(1));

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);
        combat.AdvanceCombatTime(10.0);

        Assert.True(combat.IsAwaitingPlayerDecision);
        Assert.Equal(partyMember.Id, combat.ActiveCombatantId);
        Assert.Equal(0, combat.GetSnapshot().Timeline[0].RemainingProgress, 6);

        CombatResult stunResult = combat.ExecuteAction(
            new CombatActionRequest(stun.Id, enemy.Id, partyMember.Id));

        Assert.True(stunResult.Success);
        Assert.Contains(stunResult.Events, item =>
            item.Type == CombatEventType.StatusChanged &&
            item.Detail.Contains("stunned", StringComparison.OrdinalIgnoreCase));

        CombatResult sameTimestampResult = combat.AdvanceCombatTime(0.0);

        Assert.Empty(sameTimestampResult.Events);
        Assert.Equal(100, enemy.Health);
        Assert.False(combat.IsEncounterOver);

        CombatResult expiryResult = combat.AdvanceCombatTime(1.0);

        Assert.Contains(expiryResult.Events, item => item.Type == CombatEventType.EnemyAction);
        Assert.Equal(95, partyMember.Health);
    }

    [Fact]
    public void DefeatingAnEnemyStopsTheRemainingReadyActionsAtTheSameTimestamp()
    {
        Character partyMember = CreateCharacter("party-finisher", "Finisher", luck: 20);
        Character enemy = CreateCharacter("enemy-finishing-blow", "Enemy", luck: 5, health: 5);

        CombatActionDefinition attack = new CombatActionDefinition(
            "test_finisher",
            "Finishing Blow",
            "No Stamina cost",
            "Defeat the enemy before its simultaneous action.",
            CombatActionBehavior.Attack,
            0,
            10,
            0,
            string.Empty,
            "The party member defeats the enemy.",
            recoveryCost: 100);

        CombatActionDefinition enemyAttack = new CombatActionDefinition(
            "test_enemy_attack",
            "Enemy Attack",
            "Enemy action",
            "A basic enemy attack.",
            CombatActionBehavior.Attack,
            0,
            5,
            0,
            string.Empty,
            "The enemy attacks.",
            recoveryCost: 100);

        CombatService combat = new CombatService(
            new[]
            {
                new CombatantDefinition(partyMember, CombatantSide.Party, new[] { attack }),
                new CombatantDefinition(enemy, CombatantSide.Enemy, new[] { enemyAttack })
            },
            new Random(1));

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);
        combat.AdvanceCombatTime(10.0);

        Assert.True(combat.IsAwaitingPlayerDecision);
        Assert.Equal(partyMember.Id, combat.ActiveCombatantId);

        CombatResult attackResult = combat.ExecuteAction(
            new CombatActionRequest(attack.Id, enemy.Id, partyMember.Id));

        Assert.True(attackResult.Success);
        Assert.True(combat.IsEncounterOver);
        Assert.True(combat.IsVictory);
        Assert.Equal(0, enemy.Health);
        Assert.DoesNotContain(attackResult.Events, item => item.Type == CombatEventType.EnemyAction);

        CombatResult afterVictory = combat.AdvanceCombatTime(0.0);

        Assert.Empty(afterVictory.Events);
        Assert.Equal(100, partyMember.Health);
    }

    private static Character CreateCharacter(
        string id,
        string name,
        int luck,
        int health = 100)
    {
        CharacterData data = new CharacterData(
            id,
            name,
            "Same-timestamp regression test combatant",
            1,
            health,
            100,
            10,
            100,
            health,
            0,
            10,
            50,
            0,
            new CharacterStats(20, 30, 20, 20, 20, luck));

        return new Character(data);
    }
}
