using System;
using Xunit;

public sealed class ActivityFatigueCalculatorTests
{
    private readonly ActivityFatigueCalculator _calculator = new ActivityFatigueCalculator();

    [Theory]
    [InlineData(2.8, 5, 0.6)]
    [InlineData(2.8, 30, 3.6)]
    [InlineData(1.3, 60, 1.2)]
    [InlineData(1.0, 30, 0.0)]
    [InlineData(0.5, 30, 0.0)]
    [InlineData(5.0, 30, 8.0)]
    public void CalculateUsesRelativeIntensityAndActualDuration(double met, int minutes, double expected)
    {
        Assert.Equal(expected, _calculator.Calculate(met, minutes), 6);
    }

    [Fact]
    public void ZeroDurationAddsNoFatigue()
    {
        Assert.Equal(0, _calculator.Calculate(5.0, 0));
    }

    [Fact]
    public void InvalidMetAndDurationAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.Calculate(-1, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.Calculate(double.NaN, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.Calculate(2, -1));
    }

    [Fact]
    public void CalculateReadsActivityMetFromActionDefinition()
    {
        ActionDefinition action = new ActionDefinition(
            "walk", "Walk", 5, activityMet: 2.8);

        Assert.Equal(0.6, _calculator.Calculate(action, 5), 6);
    }
}
