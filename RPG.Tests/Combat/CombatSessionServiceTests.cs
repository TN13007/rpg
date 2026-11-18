using Xunit;

public sealed class CombatSessionServiceTests
{
    [Fact]
    public void CompletingEncounterAdvancesTimeAndAddsFatigueExactlyOnce()
    {
        Character player = TestCharacterFactory.CreatePlayer();
        Character enemy = TestCharacterFactory.CreateEnemy(20);
        CombatService combatService = new CombatService(player, enemy);
        DayClock dayClock = new DayClock();
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
            dayClock,
            timeCalculator,
            fatigueCalculator,
            actionExecutionService,
            combatActivity);

        Assert.True(session.TryStartEncounter(entryAction, out string failureReason));
        Assert.Null(failureReason);

        CombatResult result = session.ExecuteAction(new CombatActionRequest("attack", enemy.Id));

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.True(session.IsEncounterOver);
        Assert.Equal(480, dayClock.MinutesOfDay);
        Assert.Equal(3.6, player.Fatigue, 2);

        Assert.Null(session.ExecuteAction(new CombatActionRequest("attack", enemy.Id)));
        Assert.Equal(3.6, player.Fatigue, 2);

        Assert.True(session.TryConsumeTimeAdvance(
            out int startDay,
            out int startMinutes,
            out int endDay,
            out int endMinutes));
        Assert.Equal(1, startDay);
        Assert.Equal(450, startMinutes);
        Assert.Equal(1, endDay);
        Assert.Equal(480, endMinutes);
        Assert.False(session.TryConsumeTimeAdvance(out _, out _, out _, out _));
    }
}
