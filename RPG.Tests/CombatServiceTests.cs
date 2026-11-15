using Xunit;

public sealed class CombatServiceTests
{
    [Fact]
    public void Attack_DealsDamageAndEnemyResponds()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        Assert.True(combat.TryStartEncounter(out string reason));
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("attack", enemy.Id));
        Assert.True(result.Success);
        Assert.Equal(20, enemy.Health);
        Assert.Equal(16, player.Health);
        Assert.Equal(8, player.Energy);
        Assert.False(combat.IsEncounterOver);
    }

    [Fact]
    public void AttackThatDefeatsEnemy_EndsEncounterAndAwardsGoldWithoutCounterattack()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy, enemyHealth: 20);
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
    public void Defend_ReducesNextEnemyHitToOne()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        Assert.True(combat.TryStartEncounter(out string reason));
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("defend"));
        Assert.True(result.Success);
        Assert.Equal(7, player.Energy);
        Assert.Equal(19, player.Health);
        Assert.False(combat.IsEncounterOver);
    }

    [Fact]
    public void Dodge_CostsTwoEnergyAndAvoidsNextEnemyHit()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        Assert.True(combat.TryStartEncounter(out string reason));
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("dodge"));
        Assert.True(result.Success);
        Assert.Equal(6, player.Energy);
        Assert.Equal(20, player.Health);
        Assert.False(combat.IsEncounterOver);
    }

    [Fact]
    public void DefendSpendingLastEnergy_EndsEncounterBeforeEnemyCanAttack()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy, playerEnergy: 1);
        Assert.True(combat.TryStartEncounter(out string reason));
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("defend"));
        Assert.True(result.Success);
        Assert.True(combat.IsEncounterOver);
        Assert.False(combat.IsVictory);
        Assert.True(player.IsUnconscious);
        Assert.Equal(20, player.Health);
        Assert.Equal(40, enemy.Health);
        Assert.Equal(0, player.Energy);
        Assert.False(HasEvent(result, CombatEventType.EnemyAction));
        Assert.False(HasAction(result.Snapshot, "attack"));
    }

    [Fact]
    public void DodgeSpendingLastEnergy_EndsEncounterBeforeEnemyCanAttack()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy, playerEnergy: 2);
        Assert.True(combat.TryStartEncounter(out string reason));
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("dodge"));
        Assert.True(result.Success);
        Assert.True(combat.IsEncounterOver);
        Assert.True(player.IsUnconscious);
        Assert.Equal(20, player.Health);
        Assert.Equal(40, enemy.Health);
        Assert.False(HasEvent(result, CombatEventType.EnemyAction));
    }

    [Fact]
    public void ZeroEnergy_CannotAttackEvenThoughAttackHasNoEnergyCost()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy, playerEnergy: 0);
        Assert.False(combat.TryStartEncounter(out string reason));
        Assert.True(player.IsUnconscious);
        Assert.True(combat.IsEncounterOver);
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("attack", enemy.Id));
        Assert.False(result.Success);
        Assert.Equal(40, enemy.Health);
        Assert.Equal(20, player.Health);
        Assert.False(HasEvent(result, CombatEventType.EnemyAction));
    }

    [Fact]
    public void InsufficientEnergyForDefendOrDodge_DoesNotConsumeTurn()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy, playerEnergy: 1);
        Assert.True(combat.TryStartEncounter(out string reason));
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("dodge"));
        Assert.False(result.Success);
        Assert.Equal(1, player.Energy);
        Assert.Equal(20, player.Health);
        Assert.Equal(40, enemy.Health);
        Assert.False(combat.IsEncounterOver);
    }

    [Fact]
    public void EnemyAttackCanKillPlayer_AndDeathEndsEncounter()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy, playerHealth: 4);
        Assert.True(combat.TryStartEncounter(out string reason));
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("attack", enemy.Id));
        Assert.True(result.Success);
        Assert.True(combat.IsEncounterOver);
        Assert.False(combat.IsVictory);
        Assert.True(player.IsDead);
        Assert.False(player.IsUnconscious);
        Assert.False(HasAction(result.Snapshot, "attack"));
    }

    [Fact]
    public void DeadPlayer_CannotStartEncounterOrAct()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy, playerHealth: 0, playerEnergy: 0);
        Assert.False(combat.TryStartEncounter(out string reason));
        Assert.Contains("dead", reason);
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("attack", enemy.Id));
        Assert.False(result.Success);
        Assert.True(player.IsDead);
        Assert.False(player.IsUnconscious);
        Assert.Equal(40, enemy.Health);
    }

    [Fact]
    public void HealthAndEnergyBothZero_DeathHasPriority()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy, playerHealth: 0, playerEnergy: 0);
        Assert.False(combat.TryStartEncounter(out string reason));
        Assert.Contains("dead", reason);
        Assert.True(player.IsDead);
        Assert.False(player.IsUnconscious);
        Assert.Equal(40, enemy.Health);
    }

    [Fact]
    public void InvalidTarget_DoesNotChangeCombatState()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        Assert.True(combat.TryStartEncounter(out string reason));
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("attack", "wrong-target"));
        Assert.False(result.Success);
        Assert.Equal(20, player.Health);
        Assert.Equal(8, player.Energy);
        Assert.Equal(40, enemy.Health);
        Assert.False(combat.IsEncounterOver);
    }

    [Fact]
    public void ActionAfterEncounterEnds_IsRejected()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy, enemyHealth: 20);
        Assert.True(combat.TryStartEncounter(out string reason));
        CombatResult first = combat.ExecuteAction(new CombatActionRequest("attack", enemy.Id));
        CombatResult second = combat.ExecuteAction(new CombatActionRequest("attack", enemy.Id));
        Assert.True(first.Success);
        Assert.False(second.Success);
        Assert.Equal(0, enemy.Health);
        Assert.Equal(45, player.Gold);
    }

    [Fact]
    public void EveryHealthAndEnergyCombination_UsesDeathBeforeUnconsciousness()
    {
        for (int health = 0; health <= 20; health++)
        {
            for (int energy = 0; energy <= 10; energy++)
            {
                CombatService combat = TestCharacterFactory.CreateCombat(
                    out Character player,
                    out Character enemy,
                    playerHealth: health,
                    playerEnergy: energy);

                bool started = combat.TryStartEncounter(out string reason);
                bool shouldStart = health > 0 && energy > 0;

                Assert.Equal(shouldStart, started);
                Assert.Equal(health == 0, player.IsDead);
                Assert.Equal(health > 0 && energy == 0, player.IsUnconscious);

                if (!shouldStart)
                {
                    Assert.True(combat.IsEncounterOver);
                    Assert.Equal(40, enemy.Health);
                }
            }
        }
    }

    [Fact]
    public void EveryEnergyValue_DefendAndDodgeRespectCostAndExhaustion()
    {
        for (int energy = 0; energy <= 10; energy++)
        {
            AssertCombatEnergyAction(energy, "defend", 1);
            AssertCombatEnergyAction(energy, "dodge", 2);
        }
    }

    [Fact]
    public void EveryHealthValue_EnemyResponseResolvesDeathAtZero()
    {
        for (int health = 1; health <= 20; health++)
        {
            CombatService combat = TestCharacterFactory.CreateCombat(
                out Character player,
                out Character enemy,
                playerHealth: health,
                playerEnergy: 8);

            Assert.True(combat.TryStartEncounter(out string reason));
            CombatResult result = combat.ExecuteAction(new CombatActionRequest("attack", enemy.Id));

            Assert.True(result.Success);
            Assert.Equal(health <= 4, player.IsDead);
            Assert.Equal(health <= 4, combat.IsEncounterOver);
            Assert.Equal(health <= 4 ? 0 : health - 4, player.Health);
        }
    }

    private static void AssertCombatEnergyAction(int energy, string actionId, int cost)
    {
        CombatService combat = TestCharacterFactory.CreateCombat(
            out Character player,
            out Character enemy,
            playerEnergy: energy);

        if (energy == 0)
        {
            Assert.False(combat.TryStartEncounter(out string startReason));
            return;
        }

        Assert.True(combat.TryStartEncounter(out string reason));
        CombatResult result = combat.ExecuteAction(new CombatActionRequest(actionId));

        if (energy < cost)
        {
            Assert.False(result.Success);
            Assert.Equal(energy, player.Energy);
            Assert.Equal(20, player.Health);
            Assert.Equal(40, enemy.Health);
            Assert.False(combat.IsEncounterOver);
            return;
        }

        Assert.True(result.Success);
        Assert.Equal(energy - cost, player.Energy);

        if (energy == cost)
        {
            Assert.True(player.IsUnconscious);
            Assert.True(combat.IsEncounterOver);
            Assert.Equal(20, player.Health);
            Assert.Equal(40, enemy.Health);
            Assert.False(HasEvent(result, CombatEventType.EnemyAction));
            return;
        }

        Assert.False(player.IsUnconscious);
        Assert.False(combat.IsEncounterOver);
        Assert.True(HasEvent(result, CombatEventType.EnemyAction));

        if (actionId == "defend")
        {
            Assert.Equal(19, player.Health);
        }
        else
        {
            Assert.Equal(20, player.Health);
        }
    }

    [Fact]
    public void EncounterStart_ResetsEnemyAndCombatFlags()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        Assert.True(combat.TryStartEncounter(out string reason));
        combat.ExecuteAction(new CombatActionRequest("attack", enemy.Id));
        Assert.Equal(20, enemy.Health);
        Assert.True(combat.TryStartEncounter(out reason));
        Assert.Equal(40, enemy.Health);
        Assert.False(combat.IsEncounterOver);
        Assert.False(combat.IsVictory);
    }

    private static bool HasEvent(CombatResult result, CombatEventType type)
    {
        foreach (CombatEvent combatEvent in result.Events)
        {
            if (combatEvent.Type == type) return true;
        }
        return false;
    }

    private static bool HasAction(CombatSnapshot snapshot, string actionId)
    {
        foreach (CombatActionOption action in snapshot.AvailableActions)
        {
            if (action.Id == actionId) return true;
        }
        return false;
    }
}
