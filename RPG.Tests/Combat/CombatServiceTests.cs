using System.Linq;
using Xunit;

public sealed class CombatServiceTests
{
    [Fact]
    public void EncounterStartsAtFirstReadyPlayerDecision()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        Assert.True(combat.TryStartEncounter(out string reason));
        Assert.True(combat.IsAwaitingPlayerDecision);
        Assert.Equal(12.5, combat.CombatTime, 3);
        TimelineCombatantStatus playerEntry = combat.GetSnapshot().Timeline.Single(entry => entry.Id == player.Id);
        TimelineCombatantStatus enemyEntry = combat.GetSnapshot().Timeline.Single(entry => entry.Id == enemy.Id);
        Assert.Equal(0, playerEntry.RemainingProgress, 3);
        Assert.Equal(27.5, enemyEntry.RemainingProgress, 3);
    }

    [Fact]
    public void QuickStrikeResolvesAndEnemyActsOnItsOwnTimelineEvent()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        Assert.True(combat.TryStartEncounter(out string reason));
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("quick_strike", enemy.Id, player.Id));
        Assert.True(result.Success);
        Assert.Equal(32, enemy.Health);
        Assert.Contains(result.Events, item => item.Type == CombatEventType.EnemyAction);
        Assert.True(combat.IsAwaitingPlayerDecision);
        Assert.True(combat.CombatTime > 12.5);
    }

    [Fact]
    public void HeavyStrikeHasGreaterRecoveryThanQuickStrike()
    {
        CombatService quick = TestCharacterFactory.CreateCombat(out Character quickPlayer, out Character quickEnemy);
        CombatService heavy = TestCharacterFactory.CreateCombat(out Character heavyPlayer, out Character heavyEnemy);
        Assert.True(quick.TryStartEncounter(out string quickReason));
        Assert.True(heavy.TryStartEncounter(out string heavyReason));
        quick.ExecuteAction(new CombatActionRequest("quick_strike", quickEnemy.Id));
        heavy.ExecuteAction(new CombatActionRequest("heavy_strike", heavyEnemy.Id));
        Assert.True(heavy.CombatTime > quick.CombatTime);
    }

    [Fact]
    public void DisruptingStrikeAppliesDelayThroughEffectDispatcher()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        Assert.True(combat.TryStartEncounter(out string reason));
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("disrupting_strike", enemy.Id, player.Id));
        Assert.True(result.Success);
        Assert.Contains(result.Events, item => item.Detail.Contains("delayed"));
    }

    [Fact]
    public void InsufficientStaminaDoesNotConsumeDecisionWindow()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy, playerStamina: 1);
        Assert.True(combat.TryStartEncounter(out string reason));
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("evade", string.Empty, player.Id));
        Assert.False(result.Success);
        Assert.True(combat.IsAwaitingPlayerDecision);
        Assert.Equal(1, player.Stamina);
        Assert.Equal(40, enemy.Health);
    }

    [Fact]
    public void GuardSpendsStaminaAndReducesTheNextGoblinHit()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        Assert.True(combat.TryStartEncounter(out string reason));
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("guard", string.Empty, player.Id));
        Assert.True(result.Success);
        Assert.Equal(7, player.Stamina);
        Assert.Contains(result.Events, item => item.Type == CombatEventType.EnemyAction);
        Assert.InRange(player.Health, 12, 19);
    }

    [Fact]
    public void EvadeSpendsStaminaAndAvoidsTheNextGoblinHit()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        Assert.True(combat.TryStartEncounter(out string reason));
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("evade", string.Empty, player.Id));
        Assert.True(result.Success);
        Assert.Equal(6, player.Stamina);
        Assert.Contains(result.Events, item => item.Type == CombatEventType.Check);
        Assert.Equal(20, player.Health);
    }

    [Fact]
    public void CatchBreathRestoresStaminaAndTimelineContinues()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy, playerStamina: 1);
        Assert.True(combat.TryStartEncounter(out string reason));
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("catch_breath", string.Empty, player.Id));
        Assert.True(result.Success);
        Assert.Equal(4, player.Stamina);
        Assert.Contains(result.Events, item => item.Type == CombatEventType.EnemyAction);
    }

    [Fact]
    public void DefeatingGoblinEndsEncounterWithoutCounterattack()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy, enemyHealth: 8);
        Assert.True(combat.TryStartEncounter(out string reason));
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("quick_strike", enemy.Id, player.Id));
        Assert.True(result.Success);
        Assert.True(combat.IsEncounterOver);
        Assert.True(combat.IsVictory);
        Assert.Equal(0, enemy.Health);
        Assert.Equal(45, player.Gold);
        Assert.DoesNotContain(result.Events, item => item.Type == CombatEventType.EnemyAction);
    }

    [Fact]
    public void GoblinChoosesOnlyFromItsAuthoredActionSet()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy, randomSeed: 17);
        Assert.True(combat.TryStartEncounter(out string reason));
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("quick_strike", enemy.Id, player.Id));
        string[] allowed =
        {
            "The goblin snaps out a quick jab.",
            "The goblin commits to a heavy swing.",
            "The goblin lands a staggering blow."
        };
        Assert.Contains(result.Events, item => item.Type == CombatEventType.EnemyAction && allowed.Contains(item.Detail));
    }
}
