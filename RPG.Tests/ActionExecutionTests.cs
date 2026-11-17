using Xunit;

public sealed class ActionExecutionTests
{
    private readonly ActionExecutionService _service =
        new ActionExecutionService(new ActivityTimeCalculator(), new ActionEffectExecutor());

    [Fact]
    public void ZeroEnergy_CannotPerformEvenZeroCostAction()
    {
        Character actor = TestCharacterFactory.CreatePlayer(health: 20, energy: 0);
        DayClock clock = new DayClock();
        ActionDefinition freeAction = new ActionDefinition("free", "Free", 10);

        bool success = _service.TryExecute(freeAction, actor, clock, out string reason, out int duration);

        Assert.False(success);
        Assert.Contains("unconscious", reason);
        Assert.Equal(0, duration);
        Assert.Equal(0, actor.Energy);
        Assert.Equal(1, clock.Day);
        Assert.Equal(450, clock.MinutesOfDay);
    }

    [Fact]
    public void InsufficientEnergy_DoesNotSpendResourcesOrAdvanceTime()
    {
        Character actor = TestCharacterFactory.CreatePlayer(energy: 2);
        DayClock clock = new DayClock();
        ActionDefinition action = new ActionDefinition("costly", "Costly", 30, energyCost: 3);

        bool success = _service.TryExecute(action, actor, clock, out string reason, out int duration);

        Assert.False(success);
        Assert.NotEmpty(reason);
        Assert.Equal(0, duration);
        Assert.Equal(2, actor.Energy);
        Assert.Equal(20, actor.Health);
        Assert.Equal(70, actor.Mood);
        Assert.Equal(20, actor.Gold);
        Assert.Equal(450, clock.MinutesOfDay);
    }

    [Fact]
    public void SpendingLastEnergy_CompletesCurrentActionThenLeavesActorUnconscious()
    {
        Character actor = TestCharacterFactory.CreatePlayer(energy: 1);
        DayClock clock = new DayClock();
        ActionDefinition action = new ActionDefinition("last-action", "Last action", 15, energyCost: 1);

        bool success = _service.TryExecute(action, actor, clock, out string reason, out int duration);

        Assert.True(success);
        Assert.Empty(reason);
        Assert.Equal(15, duration);
        Assert.Equal(0, actor.Energy);
        Assert.True(actor.IsUnconscious);
        Assert.Equal(465, clock.MinutesOfDay);
    }

    [Fact]
    public void DeadActor_CannotUseRestToRecover()
    {
        LoadAcademyContent();
        Character actor = TestCharacterFactory.CreatePlayer(health: 0, energy: 0);
        DayClock clock = new DayClock();
        ActionDefinition rest = FindAction("rest");

        bool success = _service.TryExecute(rest, actor, clock, out string reason, out int duration);

        Assert.False(success);
        Assert.Contains("dead", reason);
        Assert.Equal(0, actor.Health);
        Assert.Equal(0, actor.Energy);
        Assert.Equal(1, clock.Day);
        Assert.Equal(450, clock.MinutesOfDay);
        Assert.Equal(0, duration);
    }

    [Fact]
    public void RestFromZeroEnergy_RecoversAndAllowsTheCharacterToAct()
    {
        LoadAcademyContent();
        Character actor = TestCharacterFactory.CreatePlayer(health: 12, energy: 0, mood: 40, gold: 7);
        DayClock clock = new DayClock();
        ActionDefinition rest = FindAction("rest");

        bool success = _service.TryExecute(rest, actor, clock, out string reason, out int duration);

        Assert.True(success, reason);
        Assert.Empty(reason);
        Assert.Equal(30, duration);
        Assert.Equal(12, actor.Health);
        Assert.Equal(3, actor.Energy);
        Assert.Equal(42, actor.Mood);
        Assert.Equal(7, actor.Gold);
        Assert.False(actor.IsUnconscious);
        Assert.True(actor.CanAct);
        Assert.Equal(1, clock.Day);
        Assert.Equal(480, clock.MinutesOfDay);
    }

    [Fact]
    public void RestAtMaximumEnergy_DoesNotExceedMaximum()
    {
        LoadAcademyContent();
        Character actor = TestCharacterFactory.CreatePlayer(energy: 10);
        DayClock clock = new DayClock();
        ActionDefinition rest = FindAction("rest");

        bool success = _service.TryExecute(rest, actor, clock, out string reason, out int duration);

        Assert.True(success, reason);
        Assert.Equal(10, actor.Energy);
        Assert.Equal(30, duration);
        Assert.Equal(480, clock.MinutesOfDay);
    }

    [Fact]
    public void OnlyExplicitlyConfiguredActions_CanExecuteWhileUnconscious()
    {
        LoadAcademyContent();
        ActionDefinition rest = FindAction("rest");
        ActionDefinition ordinaryAction = FindAction("inspect_letter");
        Character actor = TestCharacterFactory.CreatePlayer(energy: 0);

        Assert.True(rest.CanExecuteWhenUnconscious);
        Assert.False(ordinaryAction.CanExecuteWhenUnconscious);
        Assert.False(AcademyContent.Travel.CanExecuteWhenUnconscious);
        Assert.True(_service.CanExecute(rest, actor, out string restFailure));
        Assert.Empty(restFailure);
        Assert.False(_service.CanExecute(ordinaryAction, actor, out string ordinaryFailure));
        Assert.Contains("unconscious", ordinaryFailure);
        Assert.False(_service.CanExecute(AcademyContent.Travel, actor, out string travelFailure));
        Assert.Contains("unconscious", travelFailure);
    }

    [Fact]
    public void SelfRestAction_IsAvailableFromEveryLocationThroughContent()
    {
        LoadAcademyContent();
        Assert.Single(AcademyContent.SelfActions);
        Assert.Equal("rest", AcademyContent.SelfActions[0].Id);
        Assert.True(AcademyContent.SelfActions[0].CanExecuteWhenUnconscious);

        int locationCount = 0;
        foreach (LocationContent location in AcademyContent.All)
        {
            locationCount++;
        }

        Assert.Equal(4, locationCount);
    }

    [Fact]
    public void Effects_ApplyAndClampAllCharacterResources()
    {
        Character actor = TestCharacterFactory.CreatePlayer(health: 18, energy: 9, mood: 98, gold: 20);
        ActionDefinition action = new ActionDefinition(
            "all-effects", "All effects", 1,
            effects: new[]
            {
                new ActionEffectDefinition(ActionEffectType.RestoreHealth, 10),
                new ActionEffectDefinition(ActionEffectType.RestoreEnergy, 10),
                new ActionEffectDefinition(ActionEffectType.ChangeMood, 10),
                new ActionEffectDefinition(ActionEffectType.AddGold, 25)
            });

        new ActionEffectExecutor().Apply(action, actor);

        Assert.Equal(20, actor.Health);
        Assert.Equal(10, actor.Energy);
        Assert.Equal(100, actor.Mood);
        Assert.Equal(45, actor.Gold);
    }

    [Fact]
    public void NegativeMoodEffect_CanReduceMoodButCannotGoBelowZero()
    {
        Character actor = TestCharacterFactory.CreatePlayer(mood: 5);
        ActionDefinition action = new ActionDefinition(
            "bad-news", "Bad news", 0,
            effects: new[] { new ActionEffectDefinition(ActionEffectType.ChangeMood, -20) });

        new ActionEffectExecutor().Apply(action, actor);
        Assert.Equal(0, actor.Mood);
    }

    [Fact]
    public void TimeModifier_UsesMinimumOneMinuteAndStatMaximum()
    {
        Character lowStat = TestCharacterFactory.CreatePlayer(intelligence: 0);
        Character highStat = TestCharacterFactory.CreatePlayer(intelligence: 100);
        ActionDefinition action = new ActionDefinition(
            "study", "Study", 20,
            new ActivityTimeModifier(CharacterStat.Intelligence, 0.5));

        Assert.Equal(20, new ActivityTimeCalculator().Calculate(action, lowStat));
        Assert.Equal(10, new ActivityTimeCalculator().Calculate(action, highStat));
    }

    [Fact]
    public void EveryCharacterStatValue_ProducesExpectedTimeModifier()
    {
        for (int statIndex = 0; statIndex < 5; statIndex++)
        {
            CharacterStat stat = (CharacterStat)statIndex;

            for (int value = 0; value <= 100; value++)
            {
                CharacterStats stats = new CharacterStats(
                    stat == CharacterStat.Strength ? value : 0,
                    stat == CharacterStat.Agility ? value : 0,
                    stat == CharacterStat.Intelligence ? value : 0,
                    stat == CharacterStat.Endurance ? value : 0,
                    stat == CharacterStat.Wisdom ? value : 0);

                CharacterData data = new CharacterData(
                    "stat-test", "Stat Test", "Test", 1,
                    20, 10, 100, 20, 10, 50, 0, stats);
                Character actor = new Character(data);
                ActionDefinition action = new ActionDefinition(
                    "stat-time", "Stat Time", 20,
                    new ActivityTimeModifier(stat, 0.5));

                int expectedDuration = (int)System.Math.Ceiling(20 * (1.0 - value / 100.0 * 0.5));
                Assert.Equal(expectedDuration, new ActivityTimeCalculator().Calculate(action, actor));
            }
        }
    }

    [Fact]
    public void NextDayAction_AdvancesToWakeUpAndAppliesEffects()
    {
        Character actor = TestCharacterFactory.CreatePlayer(health: 5, energy: 2, mood: 50);
        DayClock clock = new DayClock(1, 22, 0);
        ActionDefinition sleep = new ActionDefinition(
            "sleep", "Sleep", 0,
            effects: new[]
            {
                new ActionEffectDefinition(ActionEffectType.RestoreHealth, 20),
                new ActionEffectDefinition(ActionEffectType.RestoreEnergy, 10),
                new ActionEffectDefinition(ActionEffectType.ChangeMood, 10)
            },
            timeMode: ActionTimeMode.NextDay);

        bool success = _service.TryExecute(sleep, actor, clock, out string reason, out int duration);

        Assert.True(success);
        Assert.Empty(reason);
        Assert.Equal(0, duration);
        Assert.Equal(2, clock.Day);
        Assert.Equal(450, clock.MinutesOfDay);
        Assert.Equal(20, actor.Health);
        Assert.Equal(10, actor.Energy);
        Assert.Equal(60, actor.Mood);
    }

    [Fact]
    public void EveryConfiguredLocationAction_CanExecuteAndHasValidCostsAndEffects()
    {
        LoadAcademyContent();
        int actionCount = 0;

        foreach (LocationContent location in AcademyContent.All)
        {
            foreach (ActionDefinition action in location.Actions)
            {
                actionCount++;
                Assert.NotNull(action.Effects);
                Assert.True(action.EnergyCost >= 0);
                Assert.True(action.DurationMinutes >= 0);

                Character actor = TestCharacterFactory.CreatePlayer(health: 5, energy: 8, mood: 50, gold: 10);
                DayClock clock = new DayClock();
                bool success = _service.TryExecute(action, actor, clock, out string reason, out int duration);

                Assert.True(success, location.Id + "/" + action.Id + ": " + reason);
                Assert.InRange(actor.Health, 0, actor.MaxHealth);
                Assert.InRange(actor.Energy, 0, actor.MaxEnergy);
                Assert.InRange(actor.Mood, 0, actor.MaxMood);
                Assert.True(duration >= 0);
            }
        }

        Assert.Equal(9, actionCount);
        Assert.Single(AcademyContent.SelfActions);
    }

    [Fact]
    public void SelfAction_RestHandlesEveryHealthAndEnergyCombination()
    {
        LoadAcademyContent();
        ActionDefinition rest = FindAction("rest");

        for (int health = 0; health <= 20; health++)
        {
            for (int energy = 0; energy <= 10; energy++)
            {
                Character actor = TestCharacterFactory.CreatePlayer(
                    health: health,
                    energy: energy,
                    mood: 50,
                    gold: 10);
                DayClock clock = new DayClock();

                bool success = _service.TryExecute(rest, actor, clock, out string reason, out int duration);
                bool expectedSuccess = health > 0;

                Assert.Equal(expectedSuccess, success);

                if (expectedSuccess)
                {
                    Assert.Empty(reason);
                    Assert.Equal(System.Math.Min(10, energy + 3), actor.Energy);
                    Assert.Equal(52, actor.Mood);
                    Assert.False(actor.IsUnconscious);
                    Assert.Equal(480, clock.MinutesOfDay);
                    Assert.Equal(30, duration);
                    continue;
                }

                Assert.Contains("dead", reason);
                Assert.Equal(0, actor.Health);
                Assert.Equal(energy, actor.Energy);
                Assert.Equal(50, actor.Mood);
                Assert.Equal(10, actor.Gold);
                Assert.Equal(450, clock.MinutesOfDay);
                Assert.Equal(0, duration);
            }
        }
    }

    [Fact]
    public void EveryConfiguredAction_CoversEveryHealthAndEnergyCombination()
    {
        LoadAcademyContent();

        foreach (LocationContent location in AcademyContent.All)
        {
            foreach (ActionDefinition action in location.Actions)
            {
                for (int health = 0; health <= 20; health++)
                {
                    for (int energy = 0; energy <= 10; energy++)
                    {
                        Character actor = TestCharacterFactory.CreatePlayer(
                            health: health,
                            energy: energy,
                            mood: 50);
                        DayClock clock = new DayClock();

                        bool success = _service.TryExecute(
                            action,
                            actor,
                            clock,
                            out string reason,
                            out int duration);

                        bool expectedSuccess = health > 0 && energy > 0 && energy >= action.EnergyCost;
                        Assert.Equal(expectedSuccess, success);

                        if (!expectedSuccess)
                        {
                            Assert.Equal(health, actor.Health);
                            Assert.Equal(energy, actor.Energy);
                            Assert.Equal(450, clock.MinutesOfDay);
                            Assert.Equal(0, duration);
                            continue;
                        }

                        int expectedHealth = health;
                        int expectedEnergy = energy - action.EnergyCost;
                        int expectedMood = 50;

                        foreach (ActionEffectDefinition effect in action.Effects)
                        {
                            if (effect.Type == ActionEffectType.RestoreHealth)
                            {
                                expectedHealth = System.Math.Min(20, expectedHealth + effect.Amount);
                            }
                            else if (effect.Type == ActionEffectType.RestoreEnergy)
                            {
                                expectedEnergy = System.Math.Min(10, expectedEnergy + effect.Amount);
                            }
                            else if (effect.Type == ActionEffectType.ChangeMood)
                            {
                                expectedMood = (int)System.Math.Clamp((long)expectedMood + effect.Amount, 0L, 100L);
                            }
                        }

                        Assert.Equal(expectedHealth, actor.Health);
                        Assert.Equal(expectedEnergy, actor.Energy);
                        Assert.Equal(expectedMood, actor.Mood);
                    }
                }
            }
        }
    }

    [Fact]
    public void EveryConfiguredAction_CoversEveryMoodValue()
    {
        LoadAcademyContent();

        foreach (LocationContent location in AcademyContent.All)
        {
            foreach (ActionDefinition action in location.Actions)
            {
                for (int mood = 0; mood <= 100; mood++)
                {
                    Character actor = TestCharacterFactory.CreatePlayer(
                        health: 20,
                        energy: 8,
                        mood: mood);
                    DayClock clock = new DayClock();

                    bool success = _service.TryExecute(
                        action,
                        actor,
                        clock,
                        out string reason,
                        out int duration);

                    Assert.True(success, location.Id + "/" + action.Id + ": " + reason);

                    int expectedMood = mood;
                    foreach (ActionEffectDefinition effect in action.Effects)
                    {
                        if (effect.Type == ActionEffectType.ChangeMood)
                        {
                            expectedMood = (int)System.Math.Clamp((long)expectedMood + effect.Amount, 0L, 100L);
                        }
                    }

                    Assert.Equal(expectedMood, actor.Mood);
                }
            }
        }
    }

    [Fact]
    public void TravelDefinition_AppliesEnergyCostAndAgilityTimeModifier()
    {
        LoadAcademyContent();
        Character actor = TestCharacterFactory.CreatePlayer(energy: 8, agility: 30);
        DayClock clock = new DayClock();

        bool success = _service.TryExecute(
            AcademyContent.Travel,
            actor,
            clock,
            out string reason,
            out int duration);

        Assert.True(success, reason);
        Assert.Equal(1, AcademyContent.Travel.EnergyCost);
        Assert.Equal(9, duration);
        Assert.Equal(7, actor.Energy);
        Assert.Equal(459, clock.MinutesOfDay);
    }

    [Fact]
    public void CombatEncounterDuration_IsConfiguredSeparatelyFromLocationActions()
    {
        LoadAcademyContent();
        Assert.Equal(30, AcademyContent.CombatEncounter.DurationMinutes);
        Assert.Equal(0, AcademyContent.CombatEncounter.EnergyCost);
    }

    [Fact]
    public void ConfiguredActions_ApplyExpectedEffectsAndCosts()
    {
        LoadAcademyContent();
        AssertActionResult("rest", 0, 5, 10, 52, 10, 30);
        AssertActionResult("inspect_letter", 1, 5, 7, 50, 10, 10);
        AssertActionResult("sleep", 0, 20, 10, 60, 10, 0);
        AssertActionResult("talk_mira", 1, 5, 7, 53, 10, 15);
        AssertActionResult("inspect_board", 1, 5, 7, 50, 10, 10);
        AssertActionResult("encounter_goblin", 2, 5, 6, 50, 10, 0);
        AssertActionResult("talk_instructor", 1, 5, 7, 50, 10, 20);
        AssertActionResult("inspect_maps", 2, 5, 6, 50, 10, 18);
        AssertActionResult("talk_clerk", 1, 5, 7, 50, 10, 15);
        AssertActionResult("inspect_requests", 1, 5, 7, 50, 10, 10);
    }

    private void AssertActionResult(string actionId, int expectedCost, int expectedHealth, int expectedEnergy, int expectedMood, int expectedGold, int expectedDuration)
    {
        ActionDefinition action = FindAction(actionId);
        Character actor = TestCharacterFactory.CreatePlayer(health: 5, energy: 8, mood: 50, gold: 10);
        DayClock clock = new DayClock();

        bool success = _service.TryExecute(action, actor, clock, out string reason, out int duration);

        Assert.True(success, actionId + ": " + reason);
        Assert.Equal(expectedCost, action.EnergyCost);
        Assert.Equal(expectedHealth, actor.Health);
        Assert.Equal(expectedEnergy, actor.Energy);
        Assert.Equal(expectedMood, actor.Mood);
        Assert.Equal(expectedGold, actor.Gold);
        Assert.Equal(expectedDuration, duration);
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

        throw new System.InvalidOperationException("Action not found: " + actionId);
    }

    private static void LoadAcademyContent()
    {
        string path = System.IO.Path.Combine(System.AppContext.BaseDirectory, "Data", "academy_locations.json");
        AcademyContent.LoadJson(System.IO.File.ReadAllText(path));
    }
}
