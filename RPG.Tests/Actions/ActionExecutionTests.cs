using System;
using System.IO;
using Xunit;

public sealed class ActionExecutionTests
{
    private LocationCatalog _locations = new LocationCatalog();
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
    public void TryExecute_RevalidatesActorStateAfterPreflightAndDoesNotApplyEffects()
    {
        Character actor = TestCharacterFactory.CreatePlayer(
            health: 5, fatigue: 20, stamina: 3, mood: 40, gold: 7);
        GameClock clock = new GameClock(2, 9, 30);
        ActionDefinition action = new ActionDefinition(
            "grant_gold",
            "Grant Gold",
            30,
            effects: new[]
            {
                new ActionEffectDefinition(ActionEffectType.AddGold, 50)
            });

        Assert.True(_service.CanExecute(action, actor, out string preflightReason), preflightReason);

        actor.TakeDamage(5);

        bool success = _service.TryExecute(
            action, actor, clock, out string failureReason, out int duration);

        Assert.False(success);
        Assert.Contains("dead", failureReason);
        Assert.Equal(0, duration);
        Assert.Equal(0, actor.Health);
        Assert.Equal(20, actor.Fatigue);
        Assert.Equal(3, actor.Stamina);
        Assert.Equal(40, actor.Mood);
        Assert.Equal(7, actor.Gold);
        Assert.Equal(2, clock.Day);
        Assert.Equal(570, clock.MinutesOfDay);
    }

    [Fact]
    public void RestRecoversFatigueStaminaAndMood()
    {
        LoadLocationCatalog();
        Character actor = TestCharacterFactory.CreatePlayer(fatigue: 20, stamina: 1, mood: 40, gold: 7);
        GameClock clock = new GameClock();

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
    public void TalkToMiraAppliesAuthoredMoodIncreaseExactlyOnce()
    {
        LoadLocationCatalog();
        Character actor = TestCharacterFactory.CreatePlayer(mood: 40);
        GameClock clock = new GameClock();
        ActionDefinition action = FindNpcInteraction("mira", "talk");

        ActionEffectDefinition moodEffect = Assert.Single(action.Effects);
        Assert.Equal(ActionEffectType.ChangeMood, moodEffect.Type);
        Assert.Equal(2, moodEffect.Amount);

        bool success = _service.TryExecute(
            action, actor, clock, out string reason, out int duration);

        Assert.True(success, reason);
        Assert.Equal(10, duration);
        Assert.Equal(42, actor.Mood);
        Assert.Equal(460, clock.MinutesOfDay);
    }

    [Fact]
    public void RestAndSleepRecoveryAreClamped()
    {
        LoadLocationCatalog();
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
        LoadLocationCatalog();
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
        LoadLocationCatalog();
        Character actor = TestCharacterFactory.CreatePlayer(agility: 30);
        GameClock clock = new GameClock();

        bool success = _service.TryExecute(
            _locations.Travel, actor, clock, out string reason, out int duration);

        Assert.True(success, reason);
        Assert.Equal(2.8, _locations.Travel.ActivityMet, 6);
        Assert.Equal(9, duration);
        Assert.Equal(1.08, actor.Fatigue, 6);
        Assert.Equal(8, actor.Stamina);
        Assert.Equal(459, clock.MinutesOfDay);
    }

    [Fact]
    public void CombatEncounterHasSeparateDurationAndActivityIntensity()
    {
        LoadLocationCatalog();

        Assert.Equal(30, _locations.CombatEncounter.DurationMinutes);
        Assert.Equal(5.0, _locations.CombatEncounter.ActivityMet, 6);
        Assert.Equal(8.0, new ActivityFatigueCalculator().Calculate(
            _locations.CombatEncounter, _locations.CombatEncounter.DurationMinutes), 6);
    }

    [Fact]
    public void EveryConfiguredActivityHasValidIntensityAndDuration()
    {
        LoadLocationCatalog();
        int actionCount = 0;

        foreach (LocationContent location in _locations.All)
        {
            foreach (ActionDefinition action in location.Actions)
            {
                actionCount++;
                Assert.InRange(action.ActivityMet, 0, 20);
                Assert.True(action.DurationMinutes >= 0);
            }
        }

        foreach (NpcInteractionDefinition interaction in LoadNpcInteractions().All)
        {
            actionCount++;
            Assert.InRange(interaction.Action.ActivityMet, 0, 20);
            Assert.True(interaction.Action.DurationMinutes >= 0);
        }

        Assert.Equal(19, actionCount);
        Assert.Equal(3, LoadPlayerData().PlayerActions.Count);
    }

    [Fact]
    public void UniversityRoutineActivitiesAdvanceTimeApplyFatigueAndRestoreMealStamina()
    {
        LoadLocationCatalog();
        Character actor = TestCharacterFactory.CreatePlayer(
            health: 20, fatigue: 0, stamina: 4, mood: 40, gold: 7);
        GameClock clock = new GameClock(1, 7, 30);

        LocationContent campus = _locations.Get("university_campus");
        LocationContent lectureHall = _locations.Get("lecture_hall");
        LocationContent cafe = _locations.Get("campus_cafe");
        LocationContent sportsField = _locations.Get("sports_field");

        Assert.Contains("campus_cafe", campus.Exits);
        Assert.Contains("university_campus", cafe.Exits);
        Assert.Contains(sportsField.Actions, action => action.Id == "practice");
        Assert.Contains(lectureHall.Actions, action => action.Id == "attend_lesson");
        Assert.Contains(cafe.Actions, action => action.Id == "eat_meal");
        Assert.DoesNotContain(lectureHall.Actions, action => action.Id == "practice");
        Assert.DoesNotContain(campus.Actions, action => action.Id == "attend_lesson");
        Assert.DoesNotContain(campus.Actions, action => action.Id == "eat_meal");

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
        Assert.Equal(43, actor.Mood);
        Assert.Equal(1, clock.Day);
        Assert.Equal(575, clock.MinutesOfDay);
    }

    [Fact]
    public void InspectRequestsAppliesAuthoredNegativeMoodEffect()
    {
        LoadLocationCatalog();
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
        LoadLocationCatalog();
        LocationContent clinic = _locations.Get("campus_clinic");
        ActionDefinition treatment = FindAction("infirmary_treatment");

        Assert.Contains("campus_clinic", _locations.Get("east_district").Exits);
        Assert.Contains("east_district", clinic.Exits);
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
        LoadLocationCatalog();
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
        LoadLocationCatalog();
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


    [Fact]
    public void TryPrepareCalculatesCompletePlanWithoutMutatingRuntimeState()
    {
        Character actor = TestCharacterFactory.CreatePlayer(
            health: 12, fatigue: 10, stamina: 2, mood: 40, gold: 7, agility: 30);
        GameClock clock = new GameClock(3, 9, 15);
        ActionDefinition action = new ActionDefinition(
            "prepared_walk",
            "Prepared Walk",
            20,
            new ActivityTimeModifier(CharacterStat.Agility, 0.5),
            activityMet: 2.5,
            effects: new[]
            {
                new ActionEffectDefinition(ActionEffectType.RestoreStamina, 2),
                new ActionEffectDefinition(ActionEffectType.ChangeMood, 3)
            });

        bool prepared = _service.TryPrepare(
            action, actor, clock, out ActionExecutionPlan plan, out string reason);

        Assert.True(prepared, reason);
        Assert.Equal(17, plan.ActualDurationMinutes);
        Assert.Equal(1.7, plan.FatigueChange, 6);
        Assert.Equal(12, actor.Health);
        Assert.Equal(10, actor.Fatigue);
        Assert.Equal(2, actor.Stamina);
        Assert.Equal(40, actor.Mood);
        Assert.Equal(7, actor.Gold);
        Assert.Equal(3, clock.Day);
        Assert.Equal(555, clock.MinutesOfDay);
    }

    [Fact]
    public void TryApplyCommitsPreparedStateExactlyOnce()
    {
        Character actor = TestCharacterFactory.CreatePlayer(fatigue: 10, stamina: 2, mood: 40, gold: 7);
        GameClock clock = new GameClock(1, 9, 0);
        ActionDefinition action = new ActionDefinition(
            "prepared_rest",
            "Prepared Rest",
            30,
            effects: new[]
            {
                new ActionEffectDefinition(ActionEffectType.RestoreStamina, 2),
                new ActionEffectDefinition(ActionEffectType.ChangeMood, 3)
            });

        Assert.True(_service.TryPrepare(action, actor, clock, out ActionExecutionPlan plan, out string reason), reason);
        Assert.True(_service.TryApply(plan, out reason), reason);

        Assert.Equal(10, actor.Fatigue);
        Assert.Equal(4, actor.Stamina);
        Assert.Equal(43, actor.Mood);
        Assert.Equal(570, clock.MinutesOfDay);

        Assert.False(_service.TryApply(plan, out reason));
        Assert.Contains("already been applied", reason);
        Assert.Equal(10, actor.Fatigue);
        Assert.Equal(4, actor.Stamina);
        Assert.Equal(43, actor.Mood);
        Assert.Equal(480, clock.MinutesOfDay);
    }

    [Fact]
    public void TryApplyRejectsStalePlanWithoutApplyingPreparedChanges()
    {
        Character actor = TestCharacterFactory.CreatePlayer(fatigue: 10, stamina: 2, mood: 40, gold: 7);
        GameClock clock = new GameClock(1, 9, 0);
        ActionDefinition action = new ActionDefinition(
            "prepared_action",
            "Prepared Action",
            30,
            effects: new[]
            {
                new ActionEffectDefinition(ActionEffectType.AddGold, 50)
            });

        Assert.True(_service.TryPrepare(action, actor, clock, out ActionExecutionPlan plan, out string reason), reason);
        actor.ChangeMood(1);

        Assert.False(_service.TryApply(plan, out reason));
        Assert.Contains("changed after", reason);
        Assert.Equal(10, actor.Fatigue);
        Assert.Equal(2, actor.Stamina);
        Assert.Equal(41, actor.Mood);
        Assert.Equal(7, actor.Gold);
        Assert.Equal(1, clock.Day);
        Assert.Equal(540, clock.MinutesOfDay);
    }

    [Fact]
    public void TryPrepareRejectsClockOverflowWithoutMutatingCharacterOrClock()
    {
        Character actor = TestCharacterFactory.CreatePlayer(fatigue: 10, stamina: 2, mood: 40, gold: 7);
        GameClock clock = new GameClock(int.MaxValue, 23, 59);
        ActionDefinition action = new ActionDefinition("next_minute", "Next Minute", 1);

        Assert.False(_service.TryPrepare(action, actor, clock, out ActionExecutionPlan plan, out string reason));
        Assert.Null(plan);
        Assert.Contains("cannot advance", reason);
        Assert.Equal(10, actor.Fatigue);
        Assert.Equal(2, actor.Stamina);
        Assert.Equal(40, actor.Mood);
        Assert.Equal(7, actor.Gold);
        Assert.Equal(int.MaxValue, clock.Day);
        Assert.Equal(1439, clock.MinutesOfDay);
    }

    [Fact]
    public void TryPrepareRejectsMalformedEffectListBeforeAnyMutation()
    {
        Character actor = TestCharacterFactory.CreatePlayer(fatigue: 10, stamina: 2, mood: 40, gold: 7);
        GameClock clock = new GameClock(1, 9, 0);
        ActionDefinition action = new ActionDefinition(
            "malformed_effects",
            "Malformed Effects",
            30,
            effects: new ActionEffectDefinition[] { null });

        Assert.False(_service.TryPrepare(action, actor, clock, out ActionExecutionPlan plan, out string reason));
        Assert.Null(plan);
        Assert.Contains("null effect", reason);
        Assert.Equal(10, actor.Fatigue);
        Assert.Equal(2, actor.Stamina);
        Assert.Equal(40, actor.Mood);
        Assert.Equal(7, actor.Gold);
        Assert.Equal(1, clock.Day);
        Assert.Equal(540, clock.MinutesOfDay);
    }

    private ActionDefinition FindAction(string actionId)
    {
        foreach (LocationContent location in _locations.All)
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

    private static ActionDefinition FindNpcInteraction(string characterId, string actionId)
    {
        foreach (NpcInteractionDefinition interaction in LoadNpcInteractions().GetForCharacter(characterId))
        {
            if (interaction.Action.Id == actionId)
            {
                return interaction.Action;
            }
        }

        throw new InvalidOperationException(
            "NPC interaction not found: " + characterId + "/" + actionId);
    }

    private static NpcInteractionCatalog LoadNpcInteractions()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "NpcInteractions.json");
        return NpcInteractionDataLoader.LoadJson(File.ReadAllText(path));
    }

    private static CharacterData LoadPlayerData()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "Player.json");
        return CharacterDataLoader.LoadJson(File.ReadAllText(path));
    }

    private void LoadLocationCatalog()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "Locations.json");
        _locations = LocationDataLoader.LoadJson(File.ReadAllText(path));
    }
}
