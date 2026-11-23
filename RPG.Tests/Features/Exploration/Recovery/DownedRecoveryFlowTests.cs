using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

public sealed class DownedRecoveryFlowTests
{
    [Fact]
    public void PartyVictoryWithAlexDownedOffersRescueAndRestoresExplorationActions()
    {
        GameContentCatalog content = LoadContent();
        CharacterData playerData = content.GetCharacter("player_student");
        Character player = new Character(playerData);
        Character enemy = CreateFastTestEnemy();
        CombatantDefinition[] party = StartingParty.CreateCombatants(content, player);
        CombatActionDefinition enemyAttack = CreateLethalEnemyAttack();

        List<CombatantDefinition> encounter = new List<CombatantDefinition>(party)
        {
            new CombatantDefinition(
                enemy,
                CombatantSide.Enemy,
                new[] { enemyAttack },
                enemyPattern: new EnemyPattern(new[] { enemyAttack.Id }))
        };

        CombatService combat = new CombatService(encounter);
        GameClock clock = new GameClock(1, 7, 30);
        ActivityTimeCalculator timeCalculator = new ActivityTimeCalculator();
        ActivityFatigueCalculator fatigueCalculator = new ActivityFatigueCalculator();
        ActionExecutionService actionExecution = new ActionExecutionService(
            timeCalculator,
            new ActionEffectExecutor(),
            fatigueCalculator);
        CombatSessionService session = new CombatSessionService(
            player,
            combat,
            clock,
            timeCalculator,
            fatigueCalculator,
            actionExecution,
            new ActionDefinition("combat_encounter", "Combat encounter", 30, activityMet: 5.0));

        Assert.True(session.TryStartEncounter(
            new ActionDefinition("enter_encounter", "Enter encounter", 0),
            out string startFailure), startFailure);

        CombatResult enemyTurn = session.EndPlayerTurn();

        Assert.Contains(
            enemyTurn.Events,
            item => item.Type == CombatEventType.EnemyAction && item.TargetName == "Alex");
        Assert.Equal(0, player.Health);
        Assert.Equal(
            "Downed",
            enemyTurn.Snapshot.Party.Single(member => member.Id == player.Id).Condition);
        Assert.True(enemyTurn.Snapshot.Party.Single(member => member.Id == "sera").IsActive);

        CombatResult victory = session.ExecuteAction(
            new CombatActionRequest("quick_strike", enemy.Id, "sera"));

        Assert.True(victory.Success, victory.FailureReason);
        Assert.True(session.IsEncounterOver);
        Assert.True(session.IsVictory);
        Assert.Equal(0, player.Health);

        LocationCatalog locations = LoadLocations();
        LocationContent universityCampus = locations.Get("university_campus");
        AvailableInteractionQuery interactionQuery = new AvailableInteractionQuery();
        IReadOnlyList<AvailableInteraction> incapacitatedChoices = interactionQuery.Query(
            player,
            playerData.PlayerActions,
            universityCampus,
            locations.Get,
            locations.Travel);

        AvailableInteraction rescueChoice = Assert.Single(incapacitatedChoices);
        Assert.Equal(InteractionSourceKind.Player, rescueChoice.SourceKind);
        Assert.Equal("request_rescue", rescueChoice.ActionId);
        Assert.DoesNotContain(incapacitatedChoices, item =>
            item.SourceKind == InteractionSourceKind.Location ||
            item.SourceKind == InteractionSourceKind.Travel);

        ActionDefinition rescueAction = playerData.PlayerActions
            .Single(item => item.Action.Id == "request_rescue")
            .Action;
        int minutesBeforeRescue = clock.MinutesOfDay;

        Assert.True(actionExecution.TryExecute(
            rescueAction,
            player,
            clock,
            out string rescueFailure,
            out int rescueDuration), rescueFailure);

        Assert.Equal(180, rescueDuration);
        Assert.Equal(5, player.Health);
        Assert.Equal(minutesBeforeRescue + 180, clock.MinutesOfDay);

        IReadOnlyList<AvailableInteraction> recoveredChoices = interactionQuery.Query(
            player,
            playerData.PlayerActions,
            universityCampus,
            locations.Get,
            locations.Travel);

        Assert.Contains(recoveredChoices, item =>
            item.SourceKind == InteractionSourceKind.Player && item.ActionId == "rest");
        Assert.Contains(recoveredChoices, item => item.SourceKind == InteractionSourceKind.Travel);
        Assert.DoesNotContain(recoveredChoices, item => item.ActionId == "request_rescue");
        Assert.True(actionExecution.CanExecute(locations.Travel, player, out string travelFailure), travelFailure);
    }

    [Fact]
    public void RescueIsUnavailableToHealthyCharactersAndOrdinaryActionsRemainBlockedWhenDowned()
    {
        GameContentCatalog content = LoadContent();
        CharacterData playerData = content.GetCharacter("player_student");
        Character player = new Character(playerData);
        LocationCatalog locations = LoadLocations();
        ActionExecutionService actionExecution = new ActionExecutionService(
            new ActivityTimeCalculator(),
            new ActionEffectExecutor());
        AvailableInteractionQuery interactionQuery = new AvailableInteractionQuery();
        LocationContent universityCampus = locations.Get("university_campus");
        ActionDefinition rescueAction = playerData.PlayerActions
            .Single(item => item.Action.Id == "request_rescue")
            .Action;
        GameClock clock = new GameClock();

        IReadOnlyList<AvailableInteraction> healthyChoices = interactionQuery.Query(
            player,
            playerData.PlayerActions,
            universityCampus,
            locations.Get,
            locations.Travel);

        Assert.DoesNotContain(healthyChoices, item => item.ActionId == "request_rescue");
        Assert.False(actionExecution.CanExecute(rescueAction, player, out string healthyFailure));
        Assert.Contains("incapacitated", healthyFailure);

        player.TakeDamage(player.Health);

        IReadOnlyList<AvailableInteraction> downedChoices = interactionQuery.Query(
            player,
            playerData.PlayerActions,
            universityCampus,
            locations.Get,
            locations.Travel);

        Assert.Single(downedChoices);
        Assert.Contains(downedChoices, item => item.ActionId == "request_rescue");
        Assert.False(actionExecution.CanExecute(locations.Travel, player, out string travelFailure));
        Assert.Contains("dead", travelFailure);
        Assert.False(actionExecution.TryExecute(
            locations.Travel,
            player,
            clock,
            out string executeFailure,
            out int duration));
        Assert.Equal(0, duration);
        Assert.Equal(0, player.Health);
        Assert.Equal(450, clock.MinutesOfDay);
    }

    private static GameContentCatalog LoadContent()
    {
        CharacterData[] characters = Directory.GetFiles(
                Path.Combine(AppContext.BaseDirectory, "Data", "Characters"),
                "*.json")
            .Select(path => CharacterDataLoader.LoadJson(File.ReadAllText(path), path))
            .ToArray();

        return new GameContentCatalog(
            LoadLocations(),
            new CharacterCatalog(characters),
            new CombatActionCatalog(CombatActionJsonReader.Read(File.ReadAllText(
                Path.Combine(AppContext.BaseDirectory, "Data", "Combat", "CombatActions.json")))),
            NpcScheduleDataLoader.LoadJson(File.ReadAllText(
                Path.Combine(AppContext.BaseDirectory, "Data", "NPC", "NpcSchedules.json"))));
    }

    private static LocationCatalog LoadLocations()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "Locations", "Locations.json");
        return LocationDataLoader.LoadJson(File.ReadAllText(path));
    }

    private static Character CreateFastTestEnemy()
    {
        CharacterData data = new CharacterData(
            "recovery-test-enemy",
            "Test Presence",
            "Deterministic recovery test enemy",
            1,
            8,
            0,
            0,
            0,
            8,
            0,
            0,
            0,
            0,
            new CharacterStats(12, 100, 3, 10, 3, 1));

        return new Character(data);
    }

    private static CombatActionDefinition CreateLethalEnemyAttack()
    {
        return new CombatActionDefinition(
            "recovery_test_lethal_attack",
            "Lethal Attack",
            "Enemy Action",
            "A deterministic attack that downs the primary character.",
            CombatActionBehavior.Attack,
            0,
            20,
            0,
            string.Empty,
            "The enemy strikes Alex.");
    }
}