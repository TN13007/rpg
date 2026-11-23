using Xunit;

public sealed class ActionEffectExecutorTests
{
    [Fact]
    public void ApplyReturnsNewCharacterStateWithoutMutatingTheSourceSnapshot()
    {
        Character actor = TestCharacterFactory.CreatePlayer(
            health: 10, fatigue: 20, stamina: 2, mood: 40, gold: 7);
        CharacterRuntimeState original = CharacterRuntimeState.Capture(actor);
        ActionDefinition action = new ActionDefinition(
            "snapshot_effects",
            "Snapshot Effects",
            0,
            effects: new[]
            {
                new ActionEffectDefinition(ActionEffectType.RestoreHealth, 5),
                new ActionEffectDefinition(ActionEffectType.RestoreFatigue, 8),
                new ActionEffectDefinition(ActionEffectType.RestoreStamina, 3),
                new ActionEffectDefinition(ActionEffectType.ReduceMood, 6),
                new ActionEffectDefinition(ActionEffectType.AddGold, 12)
            });
        ActionEffectExecutor executor = new ActionEffectExecutor();
        ActionEffectPlan plan = executor.Prepare(action);

        CharacterRuntimeState result = executor.Apply(plan, original);

        Assert.NotSame(original, result);
        Assert.Equal(10, original.Health);
        Assert.Equal(20, original.Fatigue);
        Assert.Equal(2, original.Stamina);
        Assert.Equal(40, original.Mood);
        Assert.Equal(7, original.Gold);
        Assert.Equal(15, result.Health);
        Assert.Equal(12, result.Fatigue);
        Assert.Equal(5, result.Stamina);
        Assert.Equal(34, result.Mood);
        Assert.Equal(19, result.Gold);
        Assert.Equal(10, actor.Health);
        Assert.Equal(20, actor.Fatigue);
        Assert.Equal(2, actor.Stamina);
        Assert.Equal(40, actor.Mood);
        Assert.Equal(7, actor.Gold);
    }

    [Fact]
    public void StateChangeApplierCommitsPreparedCharacterAndClockStates()
    {
        Character actor = TestCharacterFactory.CreatePlayer(fatigue: 10, stamina: 2, mood: 40, gold: 7);
        GameClock clock = new GameClock(1, 9, 0);
        CharacterRuntimeState nextCharacter = CharacterRuntimeState.Capture(actor)
            .AddFatigue(2)
            .Apply(new ActionEffectDefinition(ActionEffectType.RestoreStamina, 3));
        GameClockState nextClock = new GameClockState(clock.Day, clock.MinutesOfDay).AdvanceMinutes(30);
        ActionStateChange change = new ActionStateChange(nextCharacter, nextClock);
        ActionStateChangeApplier applier = new ActionStateChangeApplier();

        Assert.True(applier.CanApply(change, actor, clock));
        applier.Apply(change, actor, clock);

        Assert.Equal(12, actor.Fatigue);
        Assert.Equal(5, actor.Stamina);
        Assert.Equal(1, clock.Day);
        Assert.Equal(570, clock.MinutesOfDay);
    }

    [Fact]
    public void ApplySupportsDirectionalEffectsAndGoldDebt()
    {
        Character actor = TestCharacterFactory.CreatePlayer(
            health: 10, fatigue: 50, stamina: 5, mood: 50, gold: 10);
        CharacterRuntimeState state = CharacterRuntimeState.Capture(actor);

        state = state.Apply(new ActionEffectDefinition(ActionEffectType.ReduceHealth, 15));
        state = state.Apply(new ActionEffectDefinition(ActionEffectType.ReduceFatigue, 20));
        state = state.Apply(new ActionEffectDefinition(ActionEffectType.ReduceStamina, 8));
        state = state.Apply(new ActionEffectDefinition(ActionEffectType.ReduceMood, 60));
        state = state.Apply(new ActionEffectDefinition(ActionEffectType.ReduceGold, 30));

        Assert.Equal(0, state.Health);
        Assert.Equal(70, state.Fatigue);
        Assert.Equal(0, state.Stamina);
        Assert.Equal(0, state.Mood);
        Assert.Equal(-20, state.Gold);

        state = state.Apply(new ActionEffectDefinition(ActionEffectType.Revive, 5));
        state = state.Apply(new ActionEffectDefinition(ActionEffectType.RestoreFatigue, 100));
        state = state.Apply(new ActionEffectDefinition(ActionEffectType.RestoreStamina, 10));
        state = state.Apply(new ActionEffectDefinition(ActionEffectType.RestoreMood, 20));
        state = state.Apply(new ActionEffectDefinition(ActionEffectType.AddGold, 5));

        Assert.Equal(5, state.Health);
        Assert.Equal(0, state.Fatigue);
        Assert.Equal(10, state.Stamina);
        Assert.Equal(20, state.Mood);
        Assert.Equal(-15, state.Gold);
        Assert.Equal(10, actor.Health);
        Assert.Equal(50, actor.Fatigue);
        Assert.Equal(5, actor.Stamina);
        Assert.Equal(50, actor.Mood);
        Assert.Equal(10, actor.Gold);
    }

    [Fact]
    public void ApplyAllowsCalculatedZeroChangeAtResourceBoundaries()
    {
        Character actor = TestCharacterFactory.CreatePlayer(
            health: 20, fatigue: 0, stamina: 10, mood: 100, gold: int.MaxValue);
        CharacterRuntimeState state = CharacterRuntimeState.Capture(actor);
        ActionDefinition action = new ActionDefinition(
            "clamped_effects",
            "Clamped Effects",
            0,
            effects: new[]
            {
                new ActionEffectDefinition(ActionEffectType.RestoreHealth, 5),
                new ActionEffectDefinition(ActionEffectType.ReduceFatigue, 5),
                new ActionEffectDefinition(ActionEffectType.RestoreStamina, 5),
                new ActionEffectDefinition(ActionEffectType.RestoreMood, 5),
                new ActionEffectDefinition(ActionEffectType.AddGold, 5)
            });

        CharacterRuntimeState result = new ActionEffectExecutor().Apply(
            new ActionEffectExecutor().Prepare(action), state);

        Assert.Equal(20, result.Health);
        Assert.Equal(0, result.Fatigue);
        Assert.Equal(10, result.Stamina);
        Assert.Equal(100, result.Mood);
        Assert.Equal(int.MaxValue, result.Gold);
    }
}
