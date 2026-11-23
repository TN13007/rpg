using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public sealed class CombatServiceTests
{
    [Fact]
    public void EncounterStartPreservesPartyResourcesAndResetsEnemyResourcesToAuthoredValues()
    {
        Character player = CreatePartyMember("party-persistent", "Persistent Party Member");
        Character enemy = new Character(new CharacterData(
            "enemy-reset", "Reset Enemy", "Test Enemy", 1,
            40, 100, 10, 100,
            40, 5, 3, 60, 8));

        player.TakeDamage(20);
        player.AddFatigue(25);
        player.SpendStamina(3);
        player.ChangeMood(-10);
        player.AddGold(4);

        enemy.TakeDamage(10);
        enemy.AddFatigue(20);
        enemy.SpendStamina(2);
        enemy.ChangeMood(-20);
        enemy.AddGold(5);

        CombatService combat = CreateCombat(new[] { player }, new[] { enemy });
        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);

        Assert.Equal(80, player.Health);
        Assert.Equal(25, player.Fatigue);
        Assert.Equal(7, player.Stamina);
        Assert.Equal(40, player.Mood);
        Assert.Equal(4, player.Gold);

        Assert.Equal(40, enemy.Health);
        Assert.Equal(5, enemy.Fatigue);
        Assert.Equal(3, enemy.Stamina);
        Assert.Equal(60, enemy.Mood);
        Assert.Equal(8, enemy.Gold);
    }

    [Fact]
    public void EncounterStartsWithTwoSharedActionPointsAndOptionsForEveryEligiblePartyMember()
    {
        Character first = CreatePartyMember("party-first", "First");
        Character second = CreatePartyMember("party-second", "Second");
        Character enemy = TestCharacterFactory.CreateEnemy();
        CombatService combat = CreateCombat(new[] { first, second }, new[] { enemy });

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);

        CombatSnapshot snapshot = combat.GetSnapshot();
        Assert.Equal(1, snapshot.Round);
        Assert.Equal(CombatPhase.PlayerTurn, snapshot.Phase);
        Assert.Equal(2, snapshot.CurrentActionPoints);
        Assert.Equal(2, snapshot.ActionCapacity);
        Assert.All(snapshot.Party, member => Assert.True(member.IsActive));
        Assert.Contains(snapshot.AvailableActions, action => action.ActorId == first.Id);
        Assert.Contains(snapshot.AvailableActions, action => action.ActorId == second.Id);
        Assert.DoesNotContain(snapshot.AvailableActions, action => string.IsNullOrWhiteSpace(action.ActorId));
    }

    [Fact]
    public void AnyPartyMemberCanActOncePerRoundAndSharedPoolAutomaticallyStartsEnemyTurnAtZero()
    {
        Character first = CreatePartyMember("party-first", "First", agility: 2, luck: 1);
        Character second = CreatePartyMember("party-second", "Second", agility: 99, luck: 99);
        Character third = CreatePartyMember("party-third", "Third", agility: 40, luck: 60);
        Character enemy = TestCharacterFactory.CreateEnemy(100);
        CombatService combat = CreateCombat(new[] { first, second, third }, new[] { enemy });

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);

        CombatResult firstAction = combat.ExecuteAction(
            new CombatActionRequest("quick_strike", enemy.Id, second.Id));
        Assert.True(firstAction.Success, firstAction.FailureReason);
        Assert.Equal(1, firstAction.Snapshot.CurrentActionPoints);
        Assert.False(firstAction.Snapshot.Party.Single(member => member.Id == second.Id).IsActive);
        Assert.True(firstAction.Snapshot.Party.Single(member => member.Id == first.Id).IsActive);

        CombatResult repeatedAction = combat.ExecuteAction(
            new CombatActionRequest("quick_strike", enemy.Id, second.Id));
        Assert.False(repeatedAction.Success);
        Assert.Equal(1, repeatedAction.Snapshot.CurrentActionPoints);
        Assert.Contains(repeatedAction.FailureReason, new[]
        {
            "This Party member has already acted this Round.",
            "The Party has no Action Points remaining."
        });

        CombatResult secondAction = combat.ExecuteAction(
            new CombatActionRequest("quick_strike", enemy.Id, first.Id));

        Assert.True(secondAction.Success, secondAction.FailureReason);
        Assert.Contains(secondAction.Events, item =>
            item.Type == CombatEventType.EnemyAction &&
            item.ActorName == enemy.Name);
        Assert.Contains(secondAction.Events, item => item.Type == CombatEventType.RoundStarted);
        Assert.Equal(2, secondAction.Snapshot.Round);
        Assert.Equal(CombatPhase.PlayerTurn, secondAction.Snapshot.Phase);
        Assert.Equal(2, secondAction.Snapshot.CurrentActionPoints);
        Assert.All(secondAction.Snapshot.Party, member => Assert.True(member.IsActive));
    }

    [Fact]
    public void RejectedActionDoesNotSpendApOrLockTheActor()
    {
        Character player = TestCharacterFactory.CreatePlayer(stamina: 1);
        Character enemy = TestCharacterFactory.CreateEnemy();
        CombatService combat = CreateCombat(new[] { player }, new[] { enemy });
        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);

        CombatResult insufficientStamina = combat.ExecuteAction(
            new CombatActionRequest("evade", string.Empty, player.Id));

        Assert.False(insufficientStamina.Success);
        Assert.Equal(2, insufficientStamina.Snapshot.CurrentActionPoints);
        Assert.True(Assert.Single(insufficientStamina.Snapshot.Party).IsActive);
        Assert.Equal(1, player.Stamina);

        CombatResult invalidTarget = combat.ExecuteAction(
            new CombatActionRequest("quick_strike", "missing-enemy", player.Id));

        Assert.False(invalidTarget.Success);
        Assert.Equal(2, invalidTarget.Snapshot.CurrentActionPoints);
        Assert.True(Assert.Single(invalidTarget.Snapshot.Party).IsActive);

        CombatResult invalidActor = combat.ExecuteAction(
            new CombatActionRequest("quick_strike", enemy.Id, enemy.Id));

        Assert.False(invalidActor.Success);
        Assert.Equal(2, invalidActor.Snapshot.CurrentActionPoints);
        Assert.True(Assert.Single(invalidActor.Snapshot.Party).IsActive);
        Assert.Throws<ArgumentException>(
            () => new CombatActionRequest("quick_strike", enemy.Id, string.Empty));

        CombatResult validAction = combat.ExecuteAction(
            new CombatActionRequest("quick_strike", enemy.Id, player.Id));

        Assert.True(validAction.Success, validAction.FailureReason);
        Assert.Equal(1, validAction.Snapshot.CurrentActionPoints);
    }

    [Fact]
    public void EndPlayerTurnDiscardsUnusedApAndResolvesEnemiesInStableRosterOrder()
    {
        Character player = CreatePartyMember("party-first", "First");
        Character enemyA = TestCharacterFactory.CreateEnemy();
        Character enemyB = CreateEnemy("enemy-second", "Second Enemy");
        CombatActionDefinition attackA = CreateEnemyAttack("enemy_a_attack", 2);
        CombatActionDefinition attackB = CreateEnemyAttack("enemy_b_attack", 3);
        CombatService combat = CreateCombat(
            new[] { player },
            new[] { enemyA, enemyB },
            new[]
            {
                new EnemyPattern(new[] { attackA.Id }),
                new EnemyPattern(new[] { attackB.Id })
            },
            new[]
            {
                new[] { attackA },
                new[] { attackB }
            });

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);
        CombatResult result = combat.EndPlayerTurn();

        Assert.True(result.Success, result.FailureReason);
        Assert.Equal(
            new[] { attackA.Id, attackB.Id },
            result.Events
                .Where(item => item.Type == CombatEventType.EnemyAction)
                .Select(item => item.ActionId));
        Assert.Equal(2, result.Snapshot.Round);
        Assert.Equal(2, result.Snapshot.CurrentActionPoints);
        Assert.Equal(CombatPhase.PlayerTurn, result.Snapshot.Phase);
        Assert.Equal(95, player.Health);
    }

    [Fact]
    public void EnemyPatternAdvancesInAuthoredOrderAndRepeatsDeterministically()
    {
        Character player = CreatePartyMember("party-first", "First");
        Character enemy = TestCharacterFactory.CreateEnemy();
        CombatActionDefinition quick = CreateEnemyAttack("pattern-quick", 2);
        CombatActionDefinition heavy = CreateEnemyAttack("pattern-heavy", 6);
        CombatService combat = CreateCombat(
            new[] { player },
            new[] { enemy },
            new[] { new EnemyPattern(new[] { quick.Id, heavy.Id }) },
            new[] { new[] { quick, heavy } });

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);

        CombatResult firstTurn = combat.EndPlayerTurn();
        CombatResult secondTurn = combat.EndPlayerTurn();
        CombatResult thirdTurn = combat.EndPlayerTurn();

        Assert.Equal(quick.Id, Assert.Single(firstTurn.Events.Where(item => item.Type == CombatEventType.EnemyAction)).ActionId);
        Assert.Equal(heavy.Id, Assert.Single(secondTurn.Events.Where(item => item.Type == CombatEventType.EnemyAction)).ActionId);
        Assert.Equal(quick.Id, Assert.Single(thirdTurn.Events.Where(item => item.Type == CombatEventType.EnemyAction)).ActionId);
        Assert.Equal(90, player.Health);
    }

    [Fact]
    public void MultipleEnemyControlSourcesReduceTheNextPlayerTurnByOnlyOneAp()
    {
        Character player = CreatePartyMember("party-first", "First");
        Character enemyA = TestCharacterFactory.CreateEnemy();
        Character enemyB = CreateEnemy("enemy-second", "Second Enemy");
        CombatActionDefinition controlA = CreateEnemyControl("control-a", 1);
        CombatActionDefinition controlB = CreateEnemyControl("control-b", 1);
        CombatService combat = CreateCombat(
            new[] { player },
            new[] { enemyA, enemyB },
            new[]
            {
                new EnemyPattern(new[] { controlA.Id }),
                new EnemyPattern(new[] { controlB.Id })
            },
            new[]
            {
                new[] { controlA },
                new[] { controlB }
            },
            actionCapacity: 3);

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);
        CombatResult result = combat.EndPlayerTurn();

        Assert.Equal(2, result.Snapshot.CurrentActionPoints);
        Assert.Equal(3, result.Snapshot.ActionCapacity);
        Assert.Equal(
            2,
            result.Events.Count(item =>
                item.Type == CombatEventType.StatusChanged &&
                item.Detail.Contains("next Player Turn loses 1 AP", StringComparison.Ordinal)));
    }

    [Fact]
    public void ControlPenaltyNeverReducesOnePointCapacityBelowOne()
    {
        Character player = CreatePartyMember("party-first", "First");
        Character enemy = TestCharacterFactory.CreateEnemy();
        CombatActionDefinition control = CreateEnemyControl("control", 1);
        CombatService combat = CreateCombat(
            new[] { player },
            new[] { enemy },
            new[] { new EnemyPattern(new[] { control.Id }) },
            new[] { new[] { control } },
            actionCapacity: 1);

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);
        CombatResult result = combat.EndPlayerTurn();

        Assert.Equal(1, result.Snapshot.CurrentActionPoints);
        Assert.Equal(1, result.Snapshot.ActionCapacity);
    }

    [Fact]
    public void ReviveCostsFourStaminaAndOneApRestoresRoundedUpQuarterHealthAndCanBeUsedOnlyOnce()
    {
        Character medic = CreatePartyMember("party-medic", "Medic", maxHealth: 20, stamina: 6);
        Character downed = CreatePartyMember("party-downed", "Downed", maxHealth: 10);
        downed.TakeDamage(downed.Health);
        Character enemy = TestCharacterFactory.CreateEnemy();
        CombatActionDefinition enemyAttack = CreateEnemyAttack("enemy-attack", 3);
        CombatService combat = CreateCombat(
            new[] { downed, medic },
            new[] { enemy },
            new[] { new EnemyPattern(new[] { enemyAttack.Id }) },
            new[] { new[] { enemyAttack } });

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);
        CombatActionOption revive = Assert.Single(
            combat.GetSnapshot().AvailableActions,
            option => option.Id == "revive_ally" && option.ActorId == medic.Id);
        Assert.Contains(revive.Targets, target => target.Id == downed.Id);

        CombatResult result = combat.ExecuteAction(
            new CombatActionRequest("revive_ally", downed.Id, medic.Id));

        Assert.True(result.Success, result.FailureReason);
        Assert.Equal(3, downed.Health);
        Assert.Equal(2, medic.Stamina);
        Assert.Equal(1, result.Snapshot.CurrentActionPoints);
        Assert.False(result.Snapshot.Party.Single(member => member.Id == medic.Id).IsActive);

        CombatResult nextTurn = combat.EndPlayerTurn();
        Assert.Equal(2, nextTurn.Snapshot.Round);
        Assert.Equal(0, downed.Health);
        Assert.Equal("Downed", nextTurn.Snapshot.Party.Single(member => member.Id == downed.Id).Condition);

        Assert.DoesNotContain(
            nextTurn.Snapshot.AvailableActions,
            option => option.Id == "revive_ally" &&
                      option.Targets.Any(target => target.Id == downed.Id));
        Assert.DoesNotContain(
            combat.GetSnapshot().AvailableActions,
            option => option.Id == "revive_ally" &&
                      option.Targets.Any(target => target.Id == downed.Id));
    }

    [Fact]
    public void DefeatingTheLastLivingPartyMemberEndsEncounterDuringEnemyTurn()
    {
        Character downed = CreatePartyMember("party-downed", "Downed", maxHealth: 10);
        downed.TakeDamage(downed.Health);
        Character lastMember = CreatePartyMember("party-last", "Last", maxHealth: 10);
        lastMember.TakeDamage(7);
        Character enemy = TestCharacterFactory.CreateEnemy();
        CombatActionDefinition lethalAttack = CreateEnemyAttack("lethal-attack", 5);
        CombatService combat = CreateCombat(
            new[] { downed, lastMember },
            new[] { enemy },
            new[] { new EnemyPattern(new[] { lethalAttack.Id }) },
            new[] { new[] { lethalAttack } });

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);
        CombatResult result = combat.EndPlayerTurn();

        Assert.True(result.Success);
        Assert.True(combat.IsEncounterOver);
        Assert.False(combat.IsVictory);
        Assert.Equal(CombatPhase.Complete, result.Snapshot.Phase);
        Assert.Equal("Downed", result.Snapshot.Party.Single(member => member.Id == lastMember.Id).Condition);
        Assert.Empty(result.Snapshot.AvailableActions);
        Assert.DoesNotContain(result.Events, item => item.Type == CombatEventType.RoundStarted);
    }

    [Fact]
    public void DefeatingEnemyEndsEncounterWithoutCounterattackAndGrantsReward()
    {
        Character player = CreatePartyMember("party-first", "First");
        Character enemy = TestCharacterFactory.CreateEnemy(8);
        CombatService combat = CreateCombat(new[] { player }, new[] { enemy });

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);
        CombatResult result = combat.ExecuteAction(
            new CombatActionRequest("quick_strike", enemy.Id, player.Id));

        Assert.True(result.Success);
        Assert.True(combat.IsEncounterOver);
        Assert.True(combat.IsVictory);
        Assert.Equal(25, player.Gold);
        Assert.DoesNotContain(result.Events, item => item.Type == CombatEventType.EnemyAction);
        Assert.Equal(1, result.Snapshot.Round);
        Assert.Equal(CombatPhase.Complete, result.Snapshot.Phase);
    }

    [Fact]
    public void HiddenEnemyCannotBeChosenAsPlayerTargetButStillActsDuringEnemyTurn()
    {
        Character player = CreatePartyMember("party-first", "First", maxHealth: 100);
        Character hiddenEnemy = TestCharacterFactory.CreateEnemy();
        CombatActionDefinition enemyAttack = CreateEnemyAttack("hidden-attack", 4);
        CombatService combat = new CombatService(
            new[]
            {
                new CombatantDefinition(player, CombatantSide.Party, TestCharacterFactory.CreatePlayerCombatActions()),
                new CombatantDefinition(
                    hiddenEnemy,
                    CombatantSide.Enemy,
                    new[] { enemyAttack },
                    isVisible: false,
                    enemyPattern: new EnemyPattern(new[] { enemyAttack.Id }))
            });

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);
        Assert.DoesNotContain(
            combat.GetSnapshot().AvailableActions,
            option => option.Id == "quick_strike");

        CombatResult enemyTurn = combat.EndPlayerTurn();
        Assert.Contains(enemyTurn.Events, item =>
            item.Type == CombatEventType.EnemyAction &&
            item.ActionId == enemyAttack.Id);
        Assert.Equal(96, player.Health);
        Assert.False(Assert.Single(enemyTurn.Snapshot.Enemies).IsVisible);
    }

    private static CombatService CreateCombat(
        Character[] party,
        Character[] enemies,
        EnemyPattern[] patterns = null,
        CombatActionDefinition[][] enemyActionSets = null,
        int actionCapacity = 2)
    {
        List<CombatantDefinition> roster = new List<CombatantDefinition>();
        foreach (Character member in party)
        {
            roster.Add(new CombatantDefinition(
                member,
                CombatantSide.Party,
                TestCharacterFactory.CreatePlayerCombatActions()));
        }

        for (int index = 0; index < enemies.Length; index++)
        {
            CombatActionDefinition[] actions = enemyActionSets == null
                ? TestCharacterFactory.CreateEnemyCombatActions()
                : enemyActionSets[index];
            EnemyPattern pattern = patterns == null
                ? new EnemyPattern(new[] { actions[0].Id })
                : patterns[index];

            roster.Add(new CombatantDefinition(
                enemies[index],
                CombatantSide.Enemy,
                actions,
                enemyPattern: pattern));
        }

        return new CombatService(roster, actionCapacity);
    }

    private static Character CreatePartyMember(
        string id,
        string name,
        int maxHealth = 100,
        int stamina = 10,
        int agility = 20,
        int luck = 20)
    {
        return CreateCharacter(id, name, maxHealth, maxHealth, stamina, agility, luck);
    }

    private static Character CreateEnemy(string id, string name)
    {
        return CreateCharacter(id, name, 100, 100, 0, 5, 5);
    }

    private static Character CreateCharacter(
        string id,
        string name,
        int maxHealth,
        int health,
        int stamina,
        int agility,
        int luck)
    {
        CharacterData data = new CharacterData(
            id,
            name,
            "Combat service test combatant",
            1,
            maxHealth,
            100,
            stamina,
            100,
            health,
            0,
            stamina,
            50,
            0,
            new CharacterStats(20, agility, 20, 20, 20, luck));
        return new Character(data);
    }

    private static CombatActionDefinition CreateEnemyAttack(string id, int damage)
    {
        return new CombatActionDefinition(
            id,
            id,
            "Enemy Action",
            "A deterministic enemy attack.",
            CombatActionBehavior.Attack,
            0,
            damage,
            0,
            string.Empty,
            "The enemy uses " + id + ".");
    }

    private static CombatActionDefinition CreateEnemyControl(string id, int damage)
    {
        return new CombatActionDefinition(
            id,
            id,
            "Enemy Action",
            "A control attack that reduces the next Player Turn's AP.",
            CombatActionBehavior.Control,
            0,
            damage,
            0,
            string.Empty,
            "The enemy uses " + id + ".");
    }
}