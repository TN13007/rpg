using System;
using System.IO;
using Xunit;

public sealed class ActionExecutionTests
{
    private readonly AcademyContent _academy = new AcademyContent();
    private readonly ActionExecutionService _service =
        new ActionExecutionService(new ActivityTimeCalculator(), new ActionEffectExecutor());

    [Fact]
    public void ActivityAddsFatigueFromMetAndCalculatedDuration()
    {
        Character actor = TestCharacterFactory.CreatePlayer(agility: 30);
        GameClock clock = new GameClock();
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
        GameClock clock = new GameClock();
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
        GameClock clock = new GameClock();

        bool success = _service.TryExecute(FindAction("rest"), actor, clock, out string reason, out int duration);

        Assert.True(success, reason);
        Assert.Equal(30, duration);
        Assert.Equal(15, actor.Fatigue);
        Assert.Equal(7, actor.Stamina);
        Assert.Equal(42, actor.Mood);
        Assert.Equal(7, actor.Gold);
        Assert.Equal(480, clock.MinutesOfDay);
    }

    [Fact]
    public void TalkToMiraAppliesAuthoredMoodIncreaseExactlyOnce()
    {
        LoadAcademyContent();
        Character actor = TestCharacterFactory.CreatePlayer(mood: 40);
        GameClock clock = new GameClock();
        ActionDefinition action = FindAction("talk_mira");

        ActionEffectDefinition moodEffect = Assert.Single(action.Effects);
        Assert.Equal(ActionEffectType.ChangeMood, moodEffect.Type);
        Assert.Equal(3, moodEffect.Amount);

        bool success = _service.TryExecute(
            action, actor, clock, out string reason, out int duration);

        Assert.True(success, reason);
        Assert.Equal(15, duration);
        Assert.Equal(43, actor.Mood);
        Assert.Equal(465, clock.MinutesOfDay);
    }

    [Fact]
    public void RestAndSleepRecoveryAreClamped()
    {
        LoadAcademyContent();
        Character actor = TestCharacterFactory.CreatePlayer(fatigue: 0, stamina: 10);
        GameClock clock = new GameClock();

        Assert.True(_service.TryExecute(FindAction("rest"), actor, clock, out string reason, out _), reason);
        Assert.Equal(0, actor.Fatigue);
        Assert.Equal(10, actor.Stamina);

        actor.AddFatigue(90);
        actor.SpendStamina(5);
        Assert.True(_service.TryExecute(FindAction("sleep"), actor, clock, out reason, out _), reason);
        Assert.Equal(0, actor.Fatigue);
        Assert.Equal(10, actor.Stamina);
    }

    [Theory]
    [InlineData(22, 0, 2, 360)]
    [InlineData(2, 0, 1, 600)]
    [InlineData(23, 0, 2, 420)]
    [InlineData(7, 29, 1, 929)]
    public void SleepAdvancesEightHoursAndAppliesRecovery(
        int startingHour,
        int startingMinute,
        int expectedDay,
        int expectedMinutesOfDay)
    {
        LoadAcademyContent();
        Character actor = TestCharacterFactory.CreatePlayer(
            health: 5, fatigue: 55, stamina: 2, mood: 50);
        GameClock clock = new GameClock(1, startingHour, startingMinute);

        bool success = _service.TryExecute(FindAction("sleep"), actor, clock, out string reason, out int duration);

        Assert.True(success, reason);
        Assert.Equal(480, duration);
        Assert.Equal(expectedDay, clock.Day);
        Assert.Equal(expectedMinutesOfDay, clock.MinutesOfDay);
        Assert.Equal(20, actor.Health);
        Assert.Equal(0, actor.Fatigue);
        Assert.Equal(10, actor.Stamina);
        Assert.Equal(60, actor.Mood);
    }

    [Fact]
    public void DeadActorCannotSleepToRestoreResourcesOrAdvanceTime()
    {
        Character actor = TestCharacterFactory.CreatePlayer(
            health: 0, fatigue: 55, stamina: 2, mood: 50);
        GameClock clock = new GameClock(1, 22, 0);

        bool success = _service.TryExecute(
            FindAction("sleep"), actor, clock, out string reason, out int duration);

        Assert.False(success);
        Assert.Contains("dead", reason);
        Assert.Equal(0, duration);
        Assert.Equal(1, clock.Day);
        Assert.Equal(1320, clock.MinutesOfDay);
        Assert.Equal(0, actor.Health);
        Assert.Equal(55, actor.Fatigue);
        Assert.Equal(2, actor.Stamina);
        Assert.Equal(50, actor.Mood);
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
        GameClock clock = new GameClock();

        bool success = _service.TryExecute(
            _academy.Travel, actor, clock, out string reason, out int duration);

        Assert.True(success, reason);
        Assert.Equal(2.8, _academy.Travel.ActivityMet, 6);
        Assert.Equal(9, duration);
        Assert.Equal(1.08, actor.Fatigue, 6);
        Assert.Equal(8, actor.Stamina);
        Assert.Equal(459, clock.MinutesOfDay);
    }

    [Fact]
    public void CombatEncounterHasSeparateDurationAndActivityIntensity()
    {
        LoadAcademyContent();

        Assert.Equal(30, _academy.CombatEncounter.DurationMinutes);
        Assert.Equal(5.0, _academy.CombatEncounter.ActivityMet, 6);
        Assert.Equal(8.0, new ActivityFatigueCalculator().Calculate(
            _academy.CombatEncounter, _academy.CombatEncounter.DurationMinutes), 6);
    }

    [Fact]
    public void EveryConfiguredActivityHasValidIntensityAndDuration()
    {
        LoadAcademyContent();
        int actionCount = 0;

        foreach (LocationContent location in _academy.All)
        {
            foreach (ActionDefinition action in location.Actions)
            {
                actionCount++;
                Assert.InRange(action.ActivityMet, 0, 20);
                Assert.True(action.DurationMinutes >= 0);
            }
        }

        Assert.Equal(19, actionCount);
        Assert.Equal(3, LoadPlayerData().PlayerActions.Length);
    }

    [Fact]
    public void AcademyRoutineActivitiesAdvanceTimeApplyFatigueAndRestoreMealStamina()
    {
        LoadAcademyContent();
        Character actor = TestCharacterFactory.CreatePlayer(
            health: 20, fatigue: 0, stamina: 4, mood: 40, gold: 7);
        GameClock clock = new GameClock(1, 7, 30);

        LocationContent courtyard = _academy.Get("courtyard");
        LocationContent classroom = _academy.Get("classroom");
        LocationContent canteen = _academy.Get("canteen");

        Assert.Contains("canteen", courtyard.Exits);
        Assert.Contains("courtyard", canteen.Exits);
        Assert.Contains(courtyard.Actions, action => action.Id == "practice");
        Assert.Contains(classroom.Actions, action => action.Id == "attend_lesson");
        Assert.Contains(canteen.Actions, action => action.Id == "eat_meal");
        Assert.DoesNotContain(classroom.Actions, action => action.Id == "practice");
        Assert.DoesNotContain(courtyard.Actions, action => action.Id == "attend_lesson");
        Assert.DoesNotContain(courtyard.Actions, action => action.Id == "eat_meal");

        ActionDefinition meal = FindAction("eat_meal");
        ActionDefinition lesson = FindAction("attend_lesson");
        ActionDefinition practice = FindAction("practice");

        Assert.Equal(20, meal.DurationMinutes);
        Assert.Equal(1.5, meal.ActivityMet, 6);
        Assert.Equal(60, lesson.DurationMinutes);
        Assert.Equal(2.0, lesson.ActivityMet, 6);
        Assert.Equal(45, practice.DurationMinutes);
        Assert.Equal(3.0, practice.ActivityMet, 6);

        Assert.True(_service.TryExecute(meal, actor, clock, out string reason, out int mealDuration), reason);
        Assert.Equal(20, mealDuration);
        Assert.Equal(0.666667, actor.Fatigue, 5);
        Assert.Equal(42, actor.Mood);
        Assert.Equal(7, actor.Stamina);

        Assert.True(_service.TryExecute(lesson, actor, clock, out reason, out int lessonDuration), reason);
        Assert.Equal(60, lessonDuration);
        Assert.Equal(4.666667, actor.Fatigue, 5);
        Assert.Equal(7, actor.Stamina);

        Assert.True(_service.TryExecute(practice, actor, clock, out reason, out int practiceDuration), reason);
        Assert.Equal(45, practiceDuration);
        Assert.Equal(10.666667, actor.Fatigue, 5);
        Assert.Equal(7, actor.Stamina);
        Assert.Equal(20, actor.Health);
        Assert.Equal(7, actor.Gold);
        Assert.Equal(42, actor.Mood);
        Assert.Equal(1, clock.Day);
        Assert.Equal(575, clock.MinutesOfDay);
    }

    [Fact]
    public void InspectRequestsAppliesAuthoredNegativeMoodEffect()
    {
        LoadAcademyContent();
        Character actor = TestCharacterFactory.CreatePlayer(mood: 76);
        GameClock clock = new GameClock();
        ActionDefinition action = FindAction("inspect_requests");

        ActionEffectDefinition moodEffect = Assert.Single(action.Effects);
        Assert.Equal(ActionEffectType.ChangeMood, moodEffect.Type);
        Assert.Equal(-2, moodEffect.Amount);

        bool success = _service.TryExecute(
            action, actor, clock, out string reason, out int duration);

        Assert.True(success, reason);
        Assert.Equal(10, duration);
        Assert.Equal(74, actor.Mood);
        Assert.Equal(460, clock.MinutesOfDay);
    }

    [Fact]
    public void InfirmaryTreatmentRestoresConfiguredHealthAndAppliesItsCostsOnce()
    {
        LoadAcademyContent();
        LocationContent infirmary = _academy.Get("infirmary");
        ActionDefinition treatment = FindAction("infirmary_treatment");

        Assert.Contains("infirmary", _academy.Get("courtyard").Exits);
        Assert.Contains("courtyard", infirmary.Exits);
        Assert.Equal(60, treatment.DurationMinutes);
        Assert.Equal(1.3, treatment.ActivityMet, 6);
        ActionEffectDefinition effect = Assert.Single(treatment.Effects);
        Assert.Equal(ActionEffectType.RestoreHealth, effect.Type);
        Assert.Equal(5, effect.Amount);

        Character actor = TestCharacterFactory.CreatePlayer(
            health: 12, fatigue: 10, stamina: 3, mood: 40, gold: 7);
        GameClock clock = new GameClock(1, 8, 0);

        bool success = _service.TryExecute(
            treatment, actor, clock, out string reason, out int duration);

        Assert.True(success, reason);
        Assert.Equal(60, duration);
        Assert.Equal(17, actor.Health);
        Assert.Equal(11.2, actor.Fatigue, 6);
        Assert.Equal(3, actor.Stamina);
        Assert.Equal(40, actor.Mood);
        Assert.Equal(7, actor.Gold);
        Assert.Equal(1, clock.Day);
        Assert.Equal(540, clock.MinutesOfDay);
    }

    [Fact]
    public void InfirmaryTreatmentAtFullHealthDoesNotExceedMaximum()
    {
        LoadAcademyContent();
        Character actor = TestCharacterFactory.CreatePlayer(
            health: 20, fatigue: 10, stamina: 3, mood: 40, gold: 7);
        GameClock clock = new GameClock(1, 10, 0);

        bool success = _service.TryExecute(
            FindAction("infirmary_treatment"),
            actor,
            clock,
            out string reason,
            out int duration);

        Assert.True(success, reason);
        Assert.Equal(60, duration);
        Assert.Equal(20, actor.Health);
        Assert.Equal(11.2, actor.Fatigue, 6);
        Assert.Equal(3, actor.Stamina);
        Assert.Equal(40, actor.Mood);
        Assert.Equal(7, actor.Gold);
        Assert.Equal(660, clock.MinutesOfDay);
    }

    [Fact]
    public void DeadActorCannotReceiveInfirmaryTreatmentOrChangeAnyState()
    {
        LoadAcademyContent();
        Character actor = TestCharacterFactory.CreatePlayer(
            health: 0, fatigue: 35, stamina: 2, mood: 45, gold: 9);
        GameClock clock = new GameClock(2, 9, 30);

        bool success = _service.TryExecute(
            FindAction("infirmary_treatment"),
            actor,
            clock,
            out string reason,
            out int duration);

        Assert.False(success);
        Assert.Contains("dead", reason);
        Assert.Equal(0, duration);
        Assert.Equal(0, actor.Health);
        Assert.Equal(35, actor.Fatigue);
        Assert.Equal(2, actor.Stamina);
        Assert.Equal(45, actor.Mood);
        Assert.Equal(9, actor.Gold);
        Assert.Equal(2, clock.Day);
        Assert.Equal(570, clock.MinutesOfDay);
    }

    private ActionDefinition FindAction(string actionId)
    {
        foreach (LocationContent location in _academy.All)
        {
            foreach (ActionDefinition action in location.Actions)
            {
                if (action.Id == actionId)
                {
                    return action;
                }
            }
        }

        foreach (PlayerActionDefinition playerAction in LoadPlayerData().PlayerActions)
        {
            if (playerAction.Action.Id == actionId)
            {
                return playerAction.Action;
            }
        }

        throw new InvalidOperationException("Action not found: " + actionId);
    }

    private static CharacterData LoadPlayerData()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "Player.json");
        return CharacterDataLoader.LoadJson(File.ReadAllText(path));
    }

    private void LoadAcademyContent()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "AcademyLocations.json");
        _academy.LoadJson(File.ReadAllText(path));
    }
}
