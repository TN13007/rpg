using System;
using System.IO;
using Xunit;

public sealed class ActionExecutionTests
{
    private readonly ActionExecutionService _service =
        new ActionExecutionService(new ActivityTimeCalculator(), new ActionEffectExecutor());

    [Fact]
    public void ActivityAddsFatigueFromMetAndCalculatedDuration()
    {
        Character actor = TestCharacterFactory.CreatePlayer(agility: 30);
        DayClock clock = new DayClock();
        ActionDefinition walk = new ActionDefinition(
            "walk", "Walk", 10,
            new ActivityTimeModifier(CharacterStat.Agility, 0.5),
            activityMet: 2.8);

        bool success = _service.TryExecute(walk, actor, clock, out string reason, out int duration);

        Assert.True(success, reason);
        Assert.Equal(9, duration);
        Assert.Equal(1.08, actor.Fatigue, 6);
        Assert.Equal(8, actor.Stamina);
        Assert.Equal(459, clock.MinutesOfDay);
    }

    [Fact]
    public void HighFatigueDoesNotBlockOrdinaryActions()
    {
        Character actor = TestCharacterFactory.CreatePlayer(fatigue: 100, stamina: 0);
        ActionDefinition action = new ActionDefinition("read", "Read", 10, activityMet: 1.3);

        Assert.True(_service.CanExecute(action, actor, out string reason));
        Assert.Empty(reason);
    }

    [Fact]
    public void DeadActorCannotPerformActionOrAdvanceTime()
    {
        Character actor = TestCharacterFactory.CreatePlayer(health: 0, fatigue: 50);
        DayClock clock = new DayClock();
        ActionDefinition action = new ActionDefinition("read", "Read", 10);

        bool success = _service.TryExecute(action, actor, clock, out string reason, out int duration);

        Assert.False(success);
        Assert.Contains("dead", reason);
        Assert.Equal(0, duration);
        Assert.Equal(50, actor.Fatigue);
        Assert.Equal(450, clock.MinutesOfDay);
    }

    [Fact]
    public void RestRecoversFatigueStaminaAndMood()
    {
        LoadAcademyContent();
        Character actor = TestCharacterFactory.CreatePlayer(fatigue: 20, stamina: 1, mood: 40, gold: 7);
        DayClock clock = new DayClock();

        bool success = _service.TryExecute(FindAction("rest"), actor, clock, out string reason, out int duration);

        Assert.True(success, reason);
        Assert.Equal(30, duration);
        Assert.Equal(15, actor.Fatigue);
        Assert.Equal(4, actor.Stamina);
        Assert.Equal(42, actor.Mood);
        Assert.Equal(7, actor.Gold);
        Assert.Equal(480, clock.MinutesOfDay);
    }

    [Fact]
    public void RestAndSleepRecoveryAreClamped()
    {
        LoadAcademyContent();
        Character actor = TestCharacterFactory.CreatePlayer(fatigue: 0, stamina: 10);
        DayClock clock = new DayClock();

        Assert.True(_service.TryExecute(FindAction("rest"), actor, clock, out string reason, out _), reason);
        Assert.Equal(0, actor.Fatigue);
        Assert.Equal(10, actor.Stamina);

        actor.AddFatigue(90);
        actor.SpendStamina(5);
        Assert.True(_service.TryExecute(FindAction("sleep"), actor, clock, out reason, out _), reason);
        Assert.Equal(0, actor.Fatigue);
        Assert.Equal(10, actor.Stamina);
    }

    [Fact]
    public void SleepAdvancesOneDayAndPreservesHealthAndMoodRecovery()
    {
        LoadAcademyContent();
        Character actor = TestCharacterFactory.CreatePlayer(
            health: 5, fatigue: 55, stamina: 2, mood: 50);
        DayClock clock = new DayClock(1, 22, 0);

        bool success = _service.TryExecute(FindAction("sleep"), actor, clock, out string reason, out int duration);

        Assert.True(success, reason);
        Assert.Equal(0, duration);
        Assert.Equal(2, clock.Day);
        Assert.Equal(450, clock.MinutesOfDay);
        Assert.Equal(20, actor.Health);
        Assert.Equal(0, actor.Fatigue);
        Assert.Equal(10, actor.Stamina);
        Assert.Equal(60, actor.Mood);
    }

    [Fact]
    public void NegativeFatigueAndStaminaRestoreEffectsAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ActionEffectDefinition(ActionEffectType.RestoreFatigue, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ActionEffectDefinition(ActionEffectType.RestoreStamina, -1));
    }

    [Fact]
    public void EffectExecutorAppliesAndClampsCharacterResources()
    {
        Character actor = TestCharacterFactory.CreatePlayer(
            health: 18, fatigue: 98, stamina: 9, mood: 98, gold: 20);
        ActionDefinition action = new ActionDefinition(
            "all-effects", "All effects", 1,
            effects: new[]
            {
                new ActionEffectDefinition(ActionEffectType.RestoreHealth, 10),
                new ActionEffectDefinition(ActionEffectType.RestoreFatigue, 10),
                new ActionEffectDefinition(ActionEffectType.RestoreStamina, 10),
                new ActionEffectDefinition(ActionEffectType.ChangeMood, 10),
                new ActionEffectDefinition(ActionEffectType.AddGold, 25)
            });

        new ActionEffectExecutor().Apply(action, actor);

        Assert.Equal(20, actor.Health);
        Assert.Equal(88, actor.Fatigue);
        Assert.Equal(10, actor.Stamina);
        Assert.Equal(100, actor.Mood);
        Assert.Equal(45, actor.Gold);
    }

    [Fact]
    public void TravelUsesActivityMetAndKeepsAgilityDurationModifier()
    {
        LoadAcademyContent();
        Character actor = TestCharacterFactory.CreatePlayer(agility: 30);
        DayClock clock = new DayClock();

        bool success = _service.TryExecute(
            AcademyContent.Travel, actor, clock, out string reason, out int duration);

        Assert.True(success, reason);
        Assert.Equal(2.8, AcademyContent.Travel.ActivityMet, 6);
        Assert.Equal(9, duration);
        Assert.Equal(1.08, actor.Fatigue, 6);
        Assert.Equal(8, actor.Stamina);
        Assert.Equal(459, clock.MinutesOfDay);
    }

    [Fact]
    public void CombatEncounterHasSeparateDurationAndActivityIntensity()
    {
        LoadAcademyContent();

        Assert.Equal(30, AcademyContent.CombatEncounter.DurationMinutes);
        Assert.Equal(5.0, AcademyContent.CombatEncounter.ActivityMet, 6);
        Assert.Equal(8.0, new ActivityFatigueCalculator().Calculate(
            AcademyContent.CombatEncounter, AcademyContent.CombatEncounter.DurationMinutes), 6);
    }

    [Fact]
    public void EveryConfiguredActivityHasValidIntensityAndDuration()
    {
        LoadAcademyContent();
        int actionCount = 0;

        foreach (LocationContent location in AcademyContent.All)
        {
            foreach (ActionDefinition action in location.Actions)
            {
                actionCount++;
                Assert.InRange(action.ActivityMet, 0, 20);
                Assert.True(action.DurationMinutes >= 0);
            }
        }

        Assert.Equal(9, actionCount);
        Assert.Single(AcademyContent.SelfActions);
    }

    private static ActionDefinition FindAction(string actionId)
    {
        foreach (LocationContent location in AcademyContent.All)
        {
            foreach (ActionDefinition action in location.Actions)
            {
                if (action.Id == actionId)
                {
                    return action;
                }
            }
        }

        foreach (ActionDefinition action in AcademyContent.SelfActions)
        {
            if (action.Id == actionId)
            {
                return action;
            }
        }

        throw new InvalidOperationException("Action not found: " + actionId);
    }

    private static void LoadAcademyContent()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "AcademyLocations.json");
        AcademyContent.LoadJson(File.ReadAllText(path));
    }
}
