using Xunit;

public sealed class ActionPreviewMapperTests
{
    private readonly ActivityTimeCalculator _timeCalculator = new ActivityTimeCalculator();
    private readonly ActionExecutionService _executionService =
        new ActionExecutionService(new ActivityTimeCalculator(), new ActionEffectExecutor());

    [Fact]
    public void Map_UsesStatAdjustedDurationAndExpectedTimeWithoutMutatingState()
    {
        Character actor = TestCharacterFactory.CreatePlayer(energy: 8, intelligence: 25);
        DayClock clock = new DayClock();
        ActionDefinition action = new ActionDefinition("study", "Study the field maps", 20,
            new ActivityTimeModifier(CharacterStat.Intelligence, 0.5), energyCost: 2);

        ActionPreviewData preview = CreateMapper().Map(action, actor, clock);

        Assert.Equal("Study the field maps", preview.Title);
        Assert.Equal("18 minutes", preview.Duration);
        Assert.Equal("2 Energy", preview.EnergyCost);
        Assert.Equal("DAY 01 · 07:48", preview.ExpectedTime);
        Assert.True(preview.CanConfirm);
        Assert.Equal(8, actor.Energy);
        Assert.Equal(450, clock.MinutesOfDay);
    }

    [Fact]
    public void Map_ShowsAuthoredEffectsAsReadableLabels()
    {
        Character actor = TestCharacterFactory.CreatePlayer();
        DayClock clock = new DayClock();
        ActionDefinition action = new ActionDefinition("rest", "Take a short rest", 30,
            effects: new[]
            {
                new ActionEffectDefinition(ActionEffectType.RestoreEnergy, 3),
                new ActionEffectDefinition(ActionEffectType.ChangeMood, 2),
                new ActionEffectDefinition(ActionEffectType.RestoreHealth, 4),
                new ActionEffectDefinition(ActionEffectType.AddGold, 5)
            });

        ActionPreviewData preview = CreateMapper().Map(action, actor, clock);

        Assert.Equal("Restore 3 Energy\nMood +2\nRestore 4 Health\nGain 5 Gold", preview.Effects);
        Assert.Equal("DAY 01 · 08:00", preview.ExpectedTime);
    }

    [Fact]
    public void Map_ShowsNegativeMoodEffect()
    {
        Character actor = TestCharacterFactory.CreatePlayer();
        DayClock clock = new DayClock();
        ActionDefinition action = new ActionDefinition("bad-news", "Hear bad news", 0,
            effects: new[] { new ActionEffectDefinition(ActionEffectType.ChangeMood, -5) });

        ActionPreviewData preview = CreateMapper().Map(action, actor, clock);

        Assert.Equal("Mood -5", preview.Effects);
        Assert.Equal("0 minutes", preview.Duration);
    }

    [Fact]
    public void Map_HandlesMidnightRollover()
    {
        Character actor = TestCharacterFactory.CreatePlayer();
        DayClock clock = new DayClock(1, 23, 59);
        ActionDefinition action = new ActionDefinition("instant", "Instant action", 2);

        ActionPreviewData preview = CreateMapper().Map(action, actor, clock);

        Assert.Equal("2 minutes", preview.Duration);
        Assert.Equal("DAY 02 · 00:01", preview.ExpectedTime);
    }

    [Fact]
    public void Map_RepresentsNextDayAsWakeUpInsteadOfZeroMinutes()
    {
        Character actor = TestCharacterFactory.CreatePlayer();
        DayClock clock = new DayClock(3, 22, 0);
        ActionDefinition action = new ActionDefinition("sleep", "Sleep until tomorrow", 0,
            timeMode: ActionTimeMode.NextDay);

        ActionPreviewData preview = CreateMapper().Map(action, actor, clock);

        Assert.Equal("Until the next morning", preview.Duration);
        Assert.Equal("DAY 04 · 07:30", preview.ExpectedTime);
        Assert.Equal("DAY 03  ·  NIGHT  ·  22:00", preview.CurrentTime);
        Assert.True(preview.CanConfirm);
        Assert.Equal(3, clock.Day);
        Assert.Equal(1320, clock.MinutesOfDay);
    }

    [Fact]
    public void Map_NextDayBeforeWakeUpKeepsCurrentDayNumber()
    {
        Character actor = TestCharacterFactory.CreatePlayer();
        DayClock clock = new DayClock(3, 2, 0);
        ActionDefinition action = new ActionDefinition("sleep", "Sleep until tomorrow", 0,
            timeMode: ActionTimeMode.NextDay);

        ActionPreviewData preview = CreateMapper().Map(action, actor, clock);

        Assert.Equal("DAY 03 · 07:30", preview.ExpectedTime);
    }

    [Fact]
    public void Map_DisablesConfirmationForInsufficientEnergyWithoutMutation()
    {
        Character actor = TestCharacterFactory.CreatePlayer(energy: 1);
        DayClock clock = new DayClock();
        ActionDefinition action = new ActionDefinition("study", "Study", 20, energyCost: 2);

        ActionPreviewData preview = CreateMapper().Map(action, actor, clock);

        Assert.False(preview.CanConfirm);
        Assert.Contains("enough Energy", preview.RejectionReason);
        Assert.Equal(1, actor.Energy);
        Assert.Equal(450, clock.MinutesOfDay);
    }

    [Fact]
    public void Map_DisablesConfirmationForUnconsciousAndDeadCharacters()
    {
        Character unconscious = TestCharacterFactory.CreatePlayer(health: 20, energy: 0);
        Character dead = TestCharacterFactory.CreatePlayer(health: 0, energy: 8);
        DayClock clock = new DayClock();
        ActionDefinition action = new ActionDefinition("rest", "Rest", 30);

        ActionPreviewData unconsciousPreview = CreateMapper().Map(action, unconscious, clock);
        ActionPreviewData deadPreview = CreateMapper().Map(action, dead, clock);

        Assert.False(unconsciousPreview.CanConfirm);
        Assert.Contains("unconscious", unconsciousPreview.RejectionReason);
        Assert.False(deadPreview.CanConfirm);
        Assert.Contains("dead", deadPreview.RejectionReason);
    }

    [Fact]
    public void RepeatedPreviewsDoNotChangeCharacterOrClock()
    {
        Character actor = TestCharacterFactory.CreatePlayer(health: 12, energy: 5, mood: 40, gold: 17);
        DayClock clock = new DayClock(2, 11, 15);
        ActionDefinition action = new ActionDefinition("rest", "Rest", 30,
            effects: new[]
            {
                new ActionEffectDefinition(ActionEffectType.RestoreHealth, 3),
                new ActionEffectDefinition(ActionEffectType.RestoreEnergy, 3),
                new ActionEffectDefinition(ActionEffectType.ChangeMood, 2),
                new ActionEffectDefinition(ActionEffectType.AddGold, 5)
            });

        ActionPreviewMapper mapper = CreateMapper();
        for (int index = 0; index < 10; index++)
        {
            ActionPreviewData preview = mapper.Map(action, actor, clock);
            Assert.True(preview.CanConfirm);
        }

        Assert.Equal(12, actor.Health);
        Assert.Equal(5, actor.Energy);
        Assert.Equal(40, actor.Mood);
        Assert.Equal(17, actor.Gold);
        Assert.Equal(2, clock.Day);
        Assert.Equal(675, clock.MinutesOfDay);
    }

    [Fact]
    public void Map_HandlesEnergyEqualToCost()
    {
        Character actor = TestCharacterFactory.CreatePlayer(energy: 2);
        DayClock clock = new DayClock();
        ActionDefinition action = new ActionDefinition("costly", "Costly", 10, energyCost: 2);

        ActionPreviewData preview = CreateMapper().Map(action, actor, clock);

        Assert.True(preview.CanConfirm);
        Assert.Equal("2 Energy", preview.EnergyCost);
    }

    [Fact]
    public void PreviewDurationMatchesTheDurationUsedByExecution()
    {
        Character actor = TestCharacterFactory.CreatePlayer(energy: 8, intelligence: 25);
        DayClock clock = new DayClock();
        ActionDefinition action = new ActionDefinition(
            "study",
            "Study the field maps",
            20,
            new ActivityTimeModifier(CharacterStat.Intelligence, 0.5),
            energyCost: 2,
            effects: new[] { new ActionEffectDefinition(ActionEffectType.ChangeMood, 3) });

        ActionPreviewData preview = CreateMapper().Map(action, actor, clock);
        bool success = _executionService.TryExecute(
            action,
            actor,
            clock,
            out string failureReason,
            out int actualDuration);

        Assert.True(success, failureReason);
        Assert.Equal("18 minutes", preview.Duration);
        Assert.Equal(18, actualDuration);
        Assert.Equal("DAY 01 · 07:48", preview.ExpectedTime);
        Assert.Equal(468, clock.MinutesOfDay);
        Assert.Equal(6, actor.Energy);
        Assert.Equal(73, actor.Mood);
    }

    [Fact]
    public void Map_RejectsNullInputs()
    {
        ActionPreviewMapper mapper = CreateMapper();
        Character actor = TestCharacterFactory.CreatePlayer();
        DayClock clock = new DayClock();
        ActionDefinition action = new ActionDefinition("rest", "Rest", 30);

        Assert.Throws<System.ArgumentNullException>(MapNullAction);
        Assert.Throws<System.ArgumentNullException>(MapNullActor);
        Assert.Throws<System.ArgumentNullException>(MapNullClock);
    }

    private void MapNullAction()
    {
        CreateMapper().Map(null, TestCharacterFactory.CreatePlayer(), new DayClock());
    }

    private void MapNullActor()
    {
        CreateMapper().Map(new ActionDefinition("rest", "Rest", 30), null, new DayClock());
    }

    private void MapNullClock()
    {
        CreateMapper().Map(new ActionDefinition("rest", "Rest", 30), TestCharacterFactory.CreatePlayer(), null);
    }

    private ActionPreviewMapper CreateMapper()
    {
        return new ActionPreviewMapper(_timeCalculator, _executionService);
    }
}
