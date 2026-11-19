using System;
using Xunit;

public sealed class MoodBandCalculatorTests
{
    [Theory]
    [InlineData(0, MoodBand.VeryLow)]
    [InlineData(24, MoodBand.VeryLow)]
    [InlineData(25, MoodBand.Low)]
    [InlineData(49, MoodBand.Low)]
    [InlineData(50, MoodBand.Steady)]
    [InlineData(74, MoodBand.Steady)]
    [InlineData(75, MoodBand.Positive)]
    [InlineData(100, MoodBand.Positive)]
    public void CalculateClassifiesEveryMoodBandBoundary(int mood, MoodBand expected)
    {
        Assert.Equal(expected, MoodBandCalculator.Calculate(mood, 100));
    }

    [Theory]
    [InlineData(48, MoodBand.VeryLow)]
    [InlineData(50, MoodBand.Low)]
    [InlineData(98, MoodBand.Low)]
    [InlineData(100, MoodBand.Steady)]
    [InlineData(148, MoodBand.Steady)]
    [InlineData(150, MoodBand.Positive)]
    [InlineData(200, MoodBand.Positive)]
    public void CalculateUsesConfiguredMaximum(int mood, MoodBand expected)
    {
        Assert.Equal(expected, MoodBandCalculator.Calculate(mood, 200));
    }

    [Fact]
    public void CalculateClassifiesZeroMaximumAsVeryLow()
    {
        Assert.Equal(MoodBand.VeryLow, MoodBandCalculator.Calculate(0, 0));
    }

    [Theory]
    [InlineData(-1, 100)]
    [InlineData(101, 100)]
    public void CalculateRejectsMoodOutsideConfiguredRange(int mood, int maxMood)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MoodBandCalculator.Calculate(mood, maxMood));
    }

    [Fact]
    public void CalculateRejectsNegativeMaximum()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MoodBandCalculator.Calculate(0, -1));
    }

    [Fact]
    public void CalculateDoesNotMutateCharacterMood()
    {
        Character character = TestCharacterFactory.CreatePlayer(mood: 74);

        MoodBand band = MoodBandCalculator.Calculate(character.Mood, character.MaxMood);

        Assert.Equal(MoodBand.Steady, band);
        Assert.Equal(74, character.Mood);
    }
}
