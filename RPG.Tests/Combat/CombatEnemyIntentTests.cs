using System;
using System.Linq;
using Xunit;

public sealed class CombatEnemyIntentTests
{
    [Fact]
    public void EnemyCommitsActionAndTargetAtEncounterStart()
    {
        Character player = TestCharacterFactory.CreatePlayer();
        Character enemy = TestCharacterFactory.CreateEnemy();
        CombatActionDefinition heavySwing = CreateEnemyAttack("heavy_swing", "Heavy Swing", 12);

        CombatService combat = CreateCombat(player, enemy, heavySwing);
        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);

        CombatSnapshot snapshot = combat.GetSnapshot();
        TimelineCombatantStatus enemyTimeline = Assert.Single(
            snapshot.Timeline.Where(entry => entry.Id == enemy.Id));

        Assert.Equal("Heavy Swing → " + player.Name, enemyTimeline.IntentDescription);
        Assert.NotNull(snapshot.Intent);
        Assert.Equal(enemy.Id, snapshot.Intent.EnemyId);
        Assert.Equal("Heavy Swing → " + player.Name, snapshot.Intent.Description);
        Assert.Equal(IntentConfidence.Recognized, snapshot.Intent.Confidence);
    }

    [Fact]
    public void StunDoesNotReplaceAnEnemyCommittedIntent()
    {
        Character player = TestCharacterFactory.CreatePlayer();
        Character enemy = TestCharacterFactory.CreateEnemy();
        CombatActionDefinition stun = new CombatActionDefinition(
            "test_stun",
            "Stunning Strike",
            "No Stamina cost",
            "Briefly stuns the target.",
            CombatActionBehavior.Stun,
            0,
            0,
            0,
            string.Empty,
            "The player stuns the enemy.",
            recoveryCost: 70,
            stunDuration: 1.5);
        CombatActionDefinition heavySwing = CreateEnemyAttack("heavy_swing", "Heavy Swing", 12);

        CombatService combat = CreateCombat(player, enemy, heavySwing, stun);
        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);
        combat.AdvanceCombatTime(10.0);
        Assert.True(combat.IsAwaitingPlayerDecision);

        CombatResult result = combat.ExecuteAction(
            new CombatActionRequest(stun.Id, enemy.Id, player.Id));

        Assert.True(result.Success);
        TimelineCombatantStatus enemyTimeline = Assert.Single(
            result.Snapshot.Timeline.Where(entry => entry.Id == enemy.Id));

        Assert.True(enemyTimeline.IsStunned);
        Assert.Equal("Heavy Swing → " + player.Name, enemyTimeline.IntentDescription);
        Assert.Equal("Heavy Swing → " + player.Name, result.Snapshot.Intent.Description);
    }

    [Fact]
    public void EachVisibleEnemyHasItsOwnCommittedIntentAndTarget()
    {
        Character player = TestCharacterFactory.CreatePlayer();
        Character enemyA = TestCharacterFactory.CreateEnemy();
        Character enemyB = CreateEnemy("enemy-disruptor", "Disruptor");
        CombatActionDefinition heavySwing = CreateEnemyAttack("heavy_swing", "Heavy Swing", 12);
        CombatActionDefinition staggeringBlow = new CombatActionDefinition(
            "staggering_blow",
            "Staggering Blow",
            "Enemy action",
            "Briefly stuns the target.",
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
                new CombatantDefinition(player, CombatantSide.Party, TestCharacterFactory.CreatePlayerCombatActions()),
                new CombatantDefinition(enemyA, CombatantSide.Enemy, new[] { heavySwing }),
                new CombatantDefinition(enemyB, CombatantSide.Enemy, new[] { staggeringBlow })
            },
            new Random(23));

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);

        TimelineCombatantStatus heavyIntent = Assert.Single(
            combat.GetSnapshot().Timeline.Where(entry => entry.Id == enemyA.Id));
        TimelineCombatantStatus disruptorIntent = Assert.Single(
            combat.GetSnapshot().Timeline.Where(entry => entry.Id == enemyB.Id));

        Assert.Equal("Heavy Swing → " + player.Name, heavyIntent.IntentDescription);
        Assert.Equal("Staggering Blow → " + player.Name, disruptorIntent.IntentDescription);
    }

    private static CombatService CreateCombat(
        Character player,
        Character enemy,
        CombatActionDefinition enemyAction,
        CombatActionDefinition playerAction = null)
    {
        CombatActionDefinition[] playerActions = playerAction == null
            ? TestCharacterFactory.CreatePlayerCombatActions()
            : new[] { playerAction };

        return new CombatService(
            new[]
            {
                new CombatantDefinition(player, CombatantSide.Party, playerActions),
                new CombatantDefinition(enemy, CombatantSide.Enemy, new[] { enemyAction })
            },
            new Random(7));
    }

    private static CombatActionDefinition CreateEnemyAttack(string id, string label, int damage)
    {
        return new CombatActionDefinition(
            id,
            label,
            "Enemy action",
            "A committed enemy attack.",
            CombatActionBehavior.Attack,
            0,
            damage,
            0,
            string.Empty,
            "The enemy performs " + label + ".",
            recoveryCost: 130);
    }

    private static Character CreateEnemy(string id, string name)
    {
        CharacterData data = new CharacterData(
            id,
            name,
            "Enemy intent test combatant",
            1,
            40,
            0,
            0,
            0,
            40,
            0,
            0,
            0,
            0,
            new CharacterStats(12, 8, 3, 10, 3, 5));

        return new Character(data);
    }
}
