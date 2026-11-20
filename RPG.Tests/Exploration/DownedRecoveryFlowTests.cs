using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

public sealed class DownedRecoveryFlowTests
{
    [Fact]
    public void PartyVictoryWithAriDownedOffersRescueAndRestoresExplorationActions()
    {
        GameContentCatalog content = LoadContent();
        CharacterData ariData = content.GetCharacter("player-ari");
        Character ari = new Character(ariData);
        Character enemy = CreateFastTestEnemy();
        CombatantDefinition[] party = StartingParty.CreateCombatants(content, ari);
        List<CombatantDefinition> encounter = new List<CombatantDefinition>(party)
        {
            new CombatantDefinition(
                enemy,
                CombatantSide.Enemy,
                new[] { CreateLethalEnemyAttack() })
        };

        CombatService combat = new CombatService(encounter, new FirstIndexRandom());
        GameClock clock = new GameClock(1, 7, 30);
        ActivityTimeCalculator timeCalculator = new ActivityTimeCalculator();
        ActivityFatigueCalculator fatigueCalculator = new ActivityFatigueCalculator();
        ActionExecutionService actionExecution = new ActionExecutionService(
            timeCalculator,
            new ActionEffectExecutor(),
            fatigueCalculator);
        CombatSessionService session = new CombatSessionService(
            ari,
            combat,
            clock,
            timeCalculator,
            fatigueCalculator,
            actionExecution,
            new ActionDefinition("combat_encounter", "Combat encounter", 30, activityMet: 5.0));

        Assert.True(session.TryStartEncounter(
            new ActionDefinition("enter_encounter", "Enter encounter", 0),
            out string startFailure), startFailure);

        CombatResult openingEvents = session.AdvanceCombatTime(4.0);

        Assert.Contains(
            openingEvents.Events,
            item => item.Type == CombatEventType.EnemyAction && item.TargetName == "Ari");
        Assert.Equal(0, ari.Health);
        Assert.Equal("sera", combat.ActiveCombatantId);

        CombatResult victory = session.ExecuteAction(
            new CombatActionRequest("quick_strike", enemy.Id, "sera"));

        Assert.True(victory.Success, victory.FailureReason);
        Assert.True(session.IsEncounterOver);
        Assert.True(session.IsVictory);
        Assert.Equal(0, ari.Health);

        AcademyContent academy = LoadAcademy();
        LocationContent courtyard = academy.Get("courtyard");
        AvailableInteractionQuery interactionQuery = new AvailableInteractionQuery();
        IReadOnlyList<AvailableInteraction> incapacitatedChoices = interactionQuery.Query(
            ari,
            ariData.PlayerActions,
            courtyard,
            academy.Get,
            academy.Travel);

        AvailableInteraction rescueChoice = Assert.Single(incapacitatedChoices);
        Assert.Equal(InteractionSourceKind.Player, rescueChoice.SourceKind);
        Assert.Equal("request_rescue", rescueChoice.ActionId);
        Assert.DoesNotContain(incapacitatedChoices, item =>
            item.SourceKind == InteractionSourceKind.Location ||
            item.SourceKind == InteractionSourceKind.Travel);

        ActionDefinition rescueAction = ariData.PlayerActions
            .Single(item => item.Action.Id == "request_rescue")
            .Action;
        int minutesBeforeRescue = clock.MinutesOfDay;

        Assert.True(actionExecution.TryExecute(
            rescueAction,
            ari,
            clock,
            out string rescueFailure,
            out int rescueDuration), rescueFailure);

        Assert.Equal(180, rescueDuration);
        Assert.Equal(5, ari.Health);
        Assert.Equal(minutesBeforeRescue + 180, clock.MinutesOfDay);

        IReadOnlyList<AvailableInteraction> recoveredChoices = interactionQuery.Query(
            ari,
            ariData.PlayerActions,
            courtyard,
            academy.Get,
            academy.Travel);

        Assert.Contains(recoveredChoices, item =>
            item.SourceKind == InteractionSourceKind.Player && item.ActionId == "rest");
        Assert.Contains(recoveredChoices, item => item.SourceKind == InteractionSourceKind.Travel);
        Assert.DoesNotContain(recoveredChoices, item => item.ActionId == "request_rescue");
        Assert.True(actionExecution.CanExecute(academy.Travel, ari, out string travelFailure), travelFailure);
    }

    [Fact]
    public void RescueIsUnavailableToHealthyCharactersAndOrdinaryActionsRemainBlockedWhenDowned()
    {
        GameContentCatalog content = LoadContent();
        CharacterData ariData = content.GetCharacter("player-ari");
        Character ari = new Character(ariData);
        AcademyContent academy = LoadAcademy();
        ActionExecutionService actionExecution = new ActionExecutionService(
            new ActivityTimeCalculator(),
            new ActionEffectExecutor());
        AvailableInteractionQuery interactionQuery = new AvailableInteractionQuery();
        LocationContent courtyard = academy.Get("courtyard");
        ActionDefinition rescueAction = ariData.PlayerActions
            .Single(item => item.Action.Id == "request_rescue")
            .Action;
        GameClock clock = new GameClock();

        IReadOnlyList<AvailableInteraction> healthyChoices = interactionQuery.Query(
            ari,
            ariData.PlayerActions,
            courtyard,
            academy.Get,
            academy.Travel);

        Assert.DoesNotContain(healthyChoices, item => item.ActionId == "request_rescue");
        Assert.False(actionExecution.CanExecute(rescueAction, ari, out string healthyFailure));
        Assert.Contains("incapacitated", healthyFailure);

        ari.TakeDamage(ari.Health);

        IReadOnlyList<AvailableInteraction> downedChoices = interactionQuery.Query(
            ari,
            ariData.PlayerActions,
            courtyard,
            academy.Get,
            academy.Travel);

        Assert.Single(downedChoices);
        Assert.Contains(downedChoices, item => item.ActionId == "request_rescue");
        Assert.False(actionExecution.CanExecute(academy.Travel, ari, out string travelFailure));
        Assert.Contains("dead", travelFailure);
        Assert.False(actionExecution.TryExecute(
            academy.Travel,
            ari,
            clock,
            out string executeFailure,
            out int duration));
        Assert.Equal(0, duration);
        Assert.Equal(0, ari.Health);
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
            LoadAcademy(),
            new CharacterCatalog(characters),
            new CombatActionCatalog(TestCharacterFactory.CreateCombatActions()));
    }

    private static AcademyContent LoadAcademy()
    {
        AcademyContent academy = new AcademyContent();
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "AcademyLocations.json");
        academy.LoadJson(File.ReadAllText(path));
        return academy;
    }

    private static Character CreateFastTestEnemy()
    {
        CharacterData data = new CharacterData(
            "recovery-test-enemy",
            "Test Goblin",
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
            "The enemy strikes Ari.",
            1000);
    }

    private sealed class FirstIndexRandom : Random
    {
        public override int Next(int maxValue)
        {
            return 0;
        }
    }
}
