using System;
using Xunit;

public sealed class ActionDurationEstimateMapperTests
{
    private readonly ActionDurationEstimateMapper _mapper =
        new ActionDurationEstimateMapper(new ActivityTimeCalculator());

    [Fact]
    public void Map_ShowsAuthoredEstimateRange()
    {
        Character actor = TestCharacterFactory.CreatePlayer();
        ActionDefinition action = new ActionDefinition(
            "read",
            "Read a book",
            15,
            estimatedDurationMinMinutes: 10,
            estimatedDurationMaxMinutes: 20);

        string result = _mapper.Map(action, actor);

        Assert.Equal("0:10–0:20", result);
    }

    [Fact]
    public void Map_AppliesStatModifierToBothEstimateBounds()
    {
        Character actor = TestCharacterFactory.CreatePlayer(intelligence: 25);
        ActionDefinition action = new ActionDefinition(
            "read",
            "Read a book",
            15,
            new ActivityTimeModifier(CharacterStat.Intelligence, 0.5),
            estimatedDurationMinMinutes: 10,
            estimatedDurationMaxMinutes: 20);

        string result = _mapper.Map(action, actor);

        Assert.Equal("0:09–0:18", result);
    }

    [Fact]
    public void Map_UsesCalculatedDurationWhenNoEstimateRangeExists()
    {
        Character actor = TestCharacterFactory.CreatePlayer(intelligence: 25);
        ActionDefinition action = new ActionDefinition(
            "study",
            "Study maps",
            20,
            new ActivityTimeModifier(CharacterStat.Intelligence, 0.5));

        string result = _mapper.Map(action, actor);

        Assert.Equal("0:18", result);
    }

    [Fact]
    public void Map_LabelsNextDayActions()
    {
        Character actor = TestCharacterFactory.CreatePlayer();
        ActionDefinition action = new ActionDefinition(
            "sleep",
            "Sleep",
            0,
            timeMode: ActionTimeMode.NextDay);

        string result = _mapper.Map(action, actor);

        Assert.Equal("Until next morning", result);
    }

    [Fact]
    public void Map_LabelsZeroDurationActionsAsImmediate()
    {
        Character actor = TestCharacterFactory.CreatePlayer();
        ActionDefinition action = new ActionDefinition("instant", "Instant", 0);

        string result = _mapper.Map(action, actor);

        Assert.Equal("Immediate", result);
    }

    [Fact]
    public void Map_RejectsNullInputs()
    {
        Character actor = TestCharacterFactory.CreatePlayer();
        ActionDefinition action = new ActionDefinition("read", "Read", 10);

        Assert.Throws<ArgumentNullException>(MapNullAction);
        Assert.Throws<ArgumentNullException>(MapNullActor);
    }

    [Fact]
    public void ActionDefinition_RejectsIncompleteEstimateBounds()
    {
        Assert.Throws<ArgumentException>(CreateActionWithIncompleteEstimate);
    }

    [Fact]
    public void ActionDefinition_RejectsNegativeEstimateBounds()
    {
        Assert.Throws<ArgumentOutOfRangeException>(CreateActionWithNegativeEstimate);
    }

    [Fact]
    public void ActionDefinition_RejectsMaximumBelowMinimum()
    {
        Assert.Throws<ArgumentOutOfRangeException>(CreateActionWithReversedEstimate);
    }

    [Fact]
    public void ActionDefinition_RejectsEstimateBoundsForNextDay()
    {
        Assert.Throws<ArgumentException>(CreateNextDayActionWithEstimate);
    }

    private void MapNullAction()
    {
        _mapper.Map(null, TestCharacterFactory.CreatePlayer());
    }

    private void MapNullActor()
    {
        _mapper.Map(new ActionDefinition("read", "Read", 10), null);
    }

    private void CreateActionWithIncompleteEstimate()
    {
        new ActionDefinition("read", "Read", 10, estimatedDurationMinMinutes: 5);
    }

    private void CreateActionWithNegativeEstimate()
    {
        new ActionDefinition("read", "Read", 10,
            estimatedDurationMinMinutes: -1,
            estimatedDurationMaxMinutes: 10);
    }

    private void CreateActionWithReversedEstimate()
    {
        new ActionDefinition("read", "Read", 10,
            estimatedDurationMinMinutes: 20,
            estimatedDurationMaxMinutes: 10);
    }

    private void CreateNextDayActionWithEstimate()
    {
        new ActionDefinition("sleep", "Sleep", 0,
            timeMode: ActionTimeMode.NextDay,
            estimatedDurationMinMinutes: 10,
            estimatedDurationMaxMinutes: 20);
    }
}
