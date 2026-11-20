using System;
using Xunit;

public sealed class ActivityTimeCalculatorTests
{
    private readonly ActivityTimeCalculator _calculator = new ActivityTimeCalculator();

    [Fact]
    public void Calculate_WithoutModifier_ReturnsBaseDuration()
    {
        ActionDefinition action = new ActionDefinition("practice", "Practice", 30);
        Character character = TestCharacterFactory.CreatePlayer();

        int duration = _calculator.Calculate(action, character);

        Assert.Equal(30, duration);
    }

    [Fact]
    public void Calculate_UsesConfiguredStatAndRoundsDurationUp()
    {
        ActionDefinition action = new ActionDefinition(
            "study",
            "Study",
            10,
            new ActivityTimeModifier(CharacterStat.Intelligence, 0.5));
        Character character = TestCharacterFactory.CreatePlayer(
            agility: 30,
            intelligence: 30);

        int duration = _calculator.Calculate(action, character);

        // 10 * (1 - 30 / 100 * 0.5) = 8.5, rounded up to 9 minutes.
        Assert.Equal(9, duration);
    }

    [Fact]
    public void Calculate_ZeroMaximumReduction_PreservesBaseDuration()
    {
        ActionDefinition action = new ActionDefinition(
            "walk",
            "Walk",
            45,
            new ActivityTimeModifier(CharacterStat.Agility, 0));
        Character character = TestCharacterFactory.CreatePlayer(agility: 100);

        int duration = _calculator.Calculate(action, character);

        Assert.Equal(45, duration);
    }

    [Fact]
    public void Calculate_FullStatAndMaximumReduction_UsesMinimumOneMinute()
    {
        ActionDefinition action = new ActionDefinition(
            "instant_practice",
            "Instant Practice",
            20,
            new ActivityTimeModifier(CharacterStat.Agility, 1));
        Character character = TestCharacterFactory.CreatePlayer(agility: 100);

        int duration = _calculator.Calculate(action, character);

        Assert.Equal(1, duration);
    }

    [Fact]
    public void Calculate_ZeroBaseDuration_RemainsZeroWithMaximumReduction()
    {
        ActionDefinition action = new ActionDefinition(
            "free_action",
            "Free Action",
            0,
            new ActivityTimeModifier(CharacterStat.Agility, 1));
        Character character = TestCharacterFactory.CreatePlayer(agility: 100);

        int duration = _calculator.Calculate(action, character);

        Assert.Equal(0, duration);
    }

    [Fact]
    public void Calculate_NullAction_ThrowsArgumentNullException()
    {
        Character character = TestCharacterFactory.CreatePlayer();

        Assert.Throws<ArgumentNullException>(
            () => _calculator.Calculate(null, character));
    }

    [Fact]
    public void Calculate_NullCharacter_ThrowsArgumentNullException()
    {
        ActionDefinition action = new ActionDefinition("practice", "Practice", 30);

        Assert.Throws<ArgumentNullException>(
            () => _calculator.Calculate(action, null));
    }
}
