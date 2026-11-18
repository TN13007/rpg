using Xunit;

public sealed class CombatServiceTests
{
    [Fact]
    public void AttackDealsDamageAndEnemyRespondsWithoutSpendingStamina()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        Assert.True(combat.TryStartEncounter(out string reason));

        CombatResult result = combat.ExecuteAction(new CombatActionRequest("attack", enemy.Id));

        Assert.True(result.Success);
        Assert.Equal(20, enemy.Health);
        Assert.Equal(16, player.Health);
        Assert.Equal(8, player.Stamina);
        Assert.False(combat.IsEncounterOver);
    }

    [Fact]
    public void AttackAtZeroStaminaRemainsAvailable()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(
            out Character player, out Character enemy, playerStamina: 0);
        Assert.True(combat.TryStartEncounter(out string reason));

        CombatResult result = combat.ExecuteAction(new CombatActionRequest("attack", enemy.Id));

        Assert.True(result.Success);
        Assert.Equal(20, enemy.Health);
        Assert.Equal(0, player.Stamina);
        Assert.True(HasEvent(result, CombatEventType.EnemyAction));
        Assert.False(combat.IsEncounterOver);
    }

    [Fact]
    public void DefendCostsOneStaminaAndReducesNextEnemyHitToOne()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        Assert.True(combat.TryStartEncounter(out string reason));

        CombatResult result = combat.ExecuteAction(new CombatActionRequest("defend"));

        Assert.True(result.Success);
        Assert.Equal(7, player.Stamina);
        Assert.Equal(19, player.Health);
        Assert.False(combat.IsEncounterOver);
    }

    [Fact]
    public void DodgeCostsTwoStaminaAndAvoidsNextEnemyHit()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        Assert.True(combat.TryStartEncounter(out string reason));

        CombatResult result = combat.ExecuteAction(new CombatActionRequest("dodge"));

        Assert.True(result.Success);
        Assert.Equal(6, player.Stamina);
        Assert.Equal(20, player.Health);
        Assert.False(combat.IsEncounterOver);
    }

    [Fact]
    public void InsufficientStaminaDoesNotConsumeTurnOrEndEncounter()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(
            out Character player, out Character enemy, playerStamina: 1);
        Assert.True(combat.TryStartEncounter(out string reason));

        CombatResult result = combat.ExecuteAction(new CombatActionRequest("dodge"));

        Assert.False(result.Success);
        Assert.Contains("Stamina", result.FailureReason);
        Assert.Equal(1, player.Stamina);
        Assert.Equal(20, player.Health);
        Assert.Equal(40, enemy.Health);
        Assert.False(combat.IsEncounterOver);
    }

    [Fact]
    public void SpendingLastStaminaDoesNotEndEncounter()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(
            out Character player, out Character enemy, playerStamina: 1);
        Assert.True(combat.TryStartEncounter(out string reason));

        CombatResult result = combat.ExecuteAction(new CombatActionRequest("defend"));

        Assert.True(result.Success);
        Assert.Equal(0, player.Stamina);
        Assert.Equal(19, player.Health);
        Assert.False(combat.IsEncounterOver);
        Assert.True(HasEvent(result, CombatEventType.EnemyAction));
    }

    [Fact]
    public void CatchBreathRestoresThreeStaminaAndEnemyStillResponds()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(
            out Character player, out Character enemy, playerStamina: 1);
        Assert.True(combat.TryStartEncounter(out string reason));

        CombatResult result = combat.ExecuteAction(new CombatActionRequest("catch_breath"));

        Assert.True(result.Success);
        Assert.Equal(4, player.Stamina);
        Assert.Equal(16, player.Health);
        Assert.True(HasEvent(result, CombatEventType.EnemyAction));
        Assert.False(combat.IsEncounterOver);
    }

    [Fact]
    public void CatchBreathAtFullStaminaIsRejectedWithoutConsumingTurn()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        Assert.True(combat.TryStartEncounter(out string reason));

        CombatResult result = combat.ExecuteAction(new CombatActionRequest("catch_breath"));

        Assert.False(result.Success);
        Assert.Equal(8, player.Stamina);
        Assert.Equal(20, player.Health);
        Assert.Equal(40, enemy.Health);
        Assert.False(combat.IsEncounterOver);
    }

    [Fact]
    public void CharacterWithZeroStaminaCanStartEncounterAndRecover()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(
            out Character player, out Character enemy, playerStamina: 0);

        Assert.True(combat.TryStartEncounter(out string reason));
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("catch_breath"));

        Assert.True(result.Success);
        Assert.Equal(3, player.Stamina);
        Assert.Equal(16, player.Health);
    }

    [Fact]
    public void AttackThatDefeatsEnemyEndsEncounterAndAwardsGoldWithoutCounterattack()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(
            out Character player, out Character enemy, enemyHealth: 20);
        Assert.True(combat.TryStartEncounter(out string reason));

        CombatResult result = combat.ExecuteAction(new CombatActionRequest("attack", enemy.Id));

        Assert.True(result.Success);
        Assert.True(combat.IsEncounterOver);
        Assert.True(combat.IsVictory);
        Assert.Equal(0, enemy.Health);
        Assert.Equal(20, player.Health);
        Assert.Equal(45, player.Gold);
        Assert.Equal(25, combat.RewardGold);
    }

    [Fact]
    public void DeadPlayerCannotStartEncounterOrAct()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(
            out Character player, out Character enemy, playerHealth: 0);
        Assert.False(combat.TryStartEncounter(out string reason));
        Assert.Contains("dead", reason);

        CombatResult result = combat.ExecuteAction(new CombatActionRequest("attack", enemy.Id));

        Assert.False(result.Success);
        Assert.True(player.IsDead);
        Assert.Equal(40, enemy.Health);
    }

    [Fact]
    public void EnemyAttackCanKillPlayerAndDeathEndsEncounter()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(
            out Character player, out Character enemy, playerHealth: 4);
        Assert.True(combat.TryStartEncounter(out string reason));

        CombatResult result = combat.ExecuteAction(new CombatActionRequest("attack", enemy.Id));

        Assert.True(result.Success);
        Assert.True(combat.IsEncounterOver);
        Assert.False(combat.IsVictory);
        Assert.True(player.IsDead);
    }

    [Fact]
    public void InvalidTargetDoesNotChangeCombatState()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        Assert.True(combat.TryStartEncounter(out string reason));

        CombatResult result = combat.ExecuteAction(new CombatActionRequest("attack", "wrong-target"));

        Assert.False(result.Success);
        Assert.Equal(20, player.Health);
        Assert.Equal(8, player.Stamina);
        Assert.Equal(40, enemy.Health);
        Assert.False(combat.IsEncounterOver);
    }

    [Fact]
    public void EveryStaminaValueRespectsDefendAndDodgeCostsWithoutForcedDefeat()
    {
        for (int stamina = 0; stamina <= 10; stamina++)
        {
            AssertStaminaAction(stamina, "defend", 1);
            AssertStaminaAction(stamina, "dodge", 2);
        }
    }

    private static void AssertStaminaAction(int stamina, string actionId, int cost)
    {
        CombatService combat = TestCharacterFactory.CreateCombat(
            out Character player, out Character enemy, playerStamina: stamina);
        Assert.True(combat.TryStartEncounter(out string reason));

        CombatResult result = combat.ExecuteAction(new CombatActionRequest(actionId));

        if (stamina < cost)
        {
            Assert.False(result.Success);
            Assert.Equal(stamina, player.Stamina);
            Assert.Equal(20, player.Health);
            Assert.Equal(40, enemy.Health);
            Assert.False(combat.IsEncounterOver);
            return;
        }

        Assert.True(result.Success);
        Assert.Equal(stamina - cost, player.Stamina);
        Assert.False(combat.IsEncounterOver);
        Assert.True(HasEvent(result, CombatEventType.EnemyAction));
    }

    private static bool HasEvent(CombatResult result, CombatEventType type)
    {
        foreach (CombatEvent combatEvent in result.Events)
        {
            if (combatEvent.Type == type) return true;
        }

        return false;
    }
}
