using System;
using Xunit;

public sealed class CombatSessionServiceTests
{
    [Fact]
    public void CompletingEncounterAdvancesWorldTimeAndAddsFatigueExactlyOnce()
    {
        Character player = TestCharacterFactory.CreatePlayer();
        Character enemy = TestCharacterFactory.CreateEnemy(8);
        CombatService combatService = new CombatService(
            new[]
            {
                new CombatantDefinition(
                    player,
                    CombatantSide.Party,
                    TestCharacterFactory.CreatePlayerCombatActions()),
                new CombatantDefinition(
                    enemy,
                    CombatantSide.Enemy,
                    TestCharacterFactory.CreateEnemyCombatActions(),
                    enemyPattern: new EnemyPattern(new[] { "presence_quick_jab" }))
            });
        GameClock gameClock = new GameClock();
        ActivityTimeCalculator timeCalculator = new ActivityTimeCalculator();
        ActivityFatigueCalculator fatigueCalculator = new ActivityFatigueCalculator();
        ActionExecutionService actionExecutionService = new ActionExecutionService(
            timeCalculator,
            new ActionEffectExecutor(),
            fatigueCalculator);
        ActionDefinition entryAction = new ActionDefinition("encounter_entry", "Enter encounter", 0);
        ActionDefinition combatActivity = new ActionDefinition(
            "combat_encounter",
            "Combat encounter",
            30,
            activityMet: 2.8);
        CombatSessionService session = new CombatSessionService(
            player,
            combatService,
            gameClock,
            timeCalculator,
            fatigueCalculator,
            actionExecutionService,
            combatActivity);

        Assert.True(session.TryStartEncounter(entryAction, out string failureReason));
        Assert.Empty(failureReason);
        Assert.Equal(2, session.GetSnapshot().CurrentActionPoints);
        Assert.Equal(450, gameClock.MinutesOfDay);
        Assert.Equal(0, player.Fatigue);

        CombatResult result = session.ExecuteAction(
            new CombatActionRequest("quick_strike", enemy.Id, player.Id));

        Assert.NotNull(result);
        Assert.True(result.Success, result.FailureReason);
        Assert.True(session.IsEncounterOver);
        Assert.Equal(480, gameClock.MinutesOfDay);
        Assert.Equal(3.6, player.Fatigue, 2);
        Assert.Null(session.ExecuteAction(
            new CombatActionRequest("quick_strike", enemy.Id, player.Id)));
        Assert.Equal(3.6, player.Fatigue, 2);

        Assert.True(session.TryConsumeTimeAdvance(
            out DateOnly startingDate,
            out int startDay,
            out int startMinutes,
            out int endDay,
            out int endMinutes));
        Assert.Equal(new DateOnly(2025, 9, 1), startingDate);
        Assert.Equal(1, startDay);
        Assert.Equal(450, startMinutes);
        Assert.Equal(1, endDay);
        Assert.Equal(480, endMinutes);
        Assert.False(session.TryConsumeTimeAdvance(
            out _, out _, out _, out _, out _));
    }

    [Fact]
    public void EndingPlayerTurnDoesNotAdvanceWorldClockUntilEncounterCompletes()
    {
        Character player = TestCharacterFactory.CreatePlayer();
        Character enemy = TestCharacterFactory.CreateEnemy(100);
        CombatService combatService = new CombatService(
            new[]
            {
                new CombatantDefinition(
                    player,
                    CombatantSide.Party,
                    TestCharacterFactory.CreatePlayerCombatActions()),
                new CombatantDefinition(
                    enemy,
                    CombatantSide.Enemy,
                    TestCharacterFactory.CreateEnemyCombatActions(),
                    enemyPattern: new EnemyPattern(new[] { "presence_quick_jab" }))
            });
        GameClock gameClock = new GameClock();
        ActivityTimeCalculator timeCalculator = new ActivityTimeCalculator();
        ActivityFatigueCalculator fatigueCalculator = new ActivityFatigueCalculator();
        ActionExecutionService actionExecutionService = new ActionExecutionService(
            timeCalculator,
            new ActionEffectExecutor(),
            fatigueCalculator);
        ActionDefinition entryAction = new ActionDefinition("encounter_entry", "Enter encounter", 0);
        ActionDefinition combatActivity = new ActionDefinition("combat_encounter", "Combat encounter", 30);
        CombatSessionService session = new CombatSessionService(
            player,
            combatService,
            gameClock,
            timeCalculator,
            fatigueCalculator,
            actionExecutionService,
            combatActivity);

        Assert.True(session.TryStartEncounter(entryAction, out string failureReason), failureReason);
        CombatResult turnResult = session.EndPlayerTurn();

        Assert.True(turnResult.Success);
        Assert.False(session.IsEncounterOver);
        Assert.Equal(450, gameClock.MinutesOfDay);
        Assert.Equal(0, player.Fatigue);
    }
}