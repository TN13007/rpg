using System;
using Xunit;

public sealed class GameClockTests
{
    [Fact]
    public void AdvanceMinutes_WithinSameDay_UpdatesTimeWithoutChangingDay()
    {
        GameClock clock = new GameClock(1, 7, 30);

        clock.AdvanceMinutes(30);

        Assert.Equal(1, clock.Day);
        Assert.Equal(480, clock.MinutesOfDay);
    }

    [Fact]
    public void AdvanceMinutes_CrossingMidnight_AdvancesDayAndPreservesRemainingTime()
    {
        GameClock clock = new GameClock(1, 23, 0);

        clock.AdvanceMinutes(480);

        Assert.Equal(2, clock.Day);
        Assert.Equal(420, clock.MinutesOfDay);
    }

    [Fact]
    public void AdvanceMinutes_AtExactMidnight_AdvancesDayAndResetsTimeToZero()
    {
        GameClock clock = new GameClock(1, 23, 0);

        clock.AdvanceMinutes(60);

        Assert.Equal(2, clock.Day);
        Assert.Equal(0, clock.MinutesOfDay);
    }

    [Fact]
    public void AdvanceMinutes_CrossingMultipleMidnights_AdvancesEveryElapsedDay()
    {
        GameClock clock = new GameClock(1, 23, 0);

        clock.AdvanceMinutes(3000);

        Assert.Equal(4, clock.Day);
        Assert.Equal(60, clock.MinutesOfDay);
    }

    [Fact]
    public void AdvanceMinutes_NegativeAmount_ThrowsAndPreservesClock()
    {
        GameClock clock = new GameClock(3, 12, 15);

        Assert.Throws<ArgumentOutOfRangeException>(() => clock.AdvanceMinutes(-1));

        Assert.Equal(3, clock.Day);
        Assert.Equal(735, clock.MinutesOfDay);
    }

    [Fact]
    public void AdvanceMinutes_DayOverflow_ThrowsAndPreservesClock()
    {
        GameClock clock = new GameClock(int.MaxValue, 23, 59);

        Assert.Throws<OverflowException>(() => clock.AdvanceMinutes(1));

        Assert.Equal(int.MaxValue, clock.Day);
        Assert.Equal(1439, clock.MinutesOfDay);
    }

    [Fact]
    public void Weekday_DayOneDefaultsToMonday()
    {
        GameClock clock = new GameClock();

        Assert.Equal(DayOfWeek.Monday, clock.Weekday);
        Assert.Equal(DayOfWeek.Monday, clock.StartingWeekday);
    }

    [Theory]
    [InlineData(1, DayOfWeek.Monday)]
    [InlineData(2, DayOfWeek.Tuesday)]
    [InlineData(3, DayOfWeek.Wednesday)]
    [InlineData(4, DayOfWeek.Thursday)]
    [InlineData(5, DayOfWeek.Friday)]
    [InlineData(6, DayOfWeek.Saturday)]
    [InlineData(7, DayOfWeek.Sunday)]
    [InlineData(8, DayOfWeek.Monday)]
    public void Weekday_DerivesFromDayIndex(int day, DayOfWeek expected)
    {
        GameClock clock = new GameClock(startingDay: day);

        Assert.Equal(expected, clock.Weekday);
    }

    [Fact]
    public void Weekday_UsesAuthoredStartingWeekday()
    {
        GameClock clock = new GameClock(
            startingWeekday: DayOfWeek.Friday);

        Assert.Equal(DayOfWeek.Friday, clock.Weekday);

        clock.AdvanceMinutes(24 * 60);

        Assert.Equal(DayOfWeek.Saturday, clock.Weekday);
    }

    [Fact]
    public void Weekday_CrossingMidnightAdvancesWeekday()
    {
        GameClock clock = new GameClock(1, 23, 0);

        clock.AdvanceMinutes(60);

        Assert.Equal(2, clock.Day);
        Assert.Equal(DayOfWeek.Tuesday, clock.Weekday);
    }

    [Fact]
    public void Weekday_AdvancingMultipleDaysWrapsAroundWeek()
    {
        GameClock clock = new GameClock(
            startingWeekday: DayOfWeek.Friday);

        clock.AdvanceMinutes(8 * 24 * 60);

        Assert.Equal(DayOfWeek.Saturday, clock.Weekday);
    }

    [Fact]
    public void Constructor_InvalidStartingWeekday_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new GameClock(startingWeekday: (DayOfWeek)7));
    }

}
