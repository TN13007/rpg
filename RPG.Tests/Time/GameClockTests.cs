using System;
using Xunit;

public sealed class GameClockTests
{
    [Fact]
    public void Constructor_DefaultsToMondaySeptemberFirst2025()
    {
        GameClock clock = new GameClock();

        Assert.Equal(new DateOnly(2025, 9, 1), clock.StartingDate);
        Assert.Equal(new DateOnly(2025, 9, 1), clock.CurrentDate);
        Assert.Equal(DayOfWeek.Monday, clock.StartingWeekday);
        Assert.Equal(DayOfWeek.Monday, clock.Weekday);
        Assert.Equal(1, clock.Day);
        Assert.Equal(450, clock.MinutesOfDay);
    }

    [Fact]
    public void AdvanceMinutes_WithinSameDay_UpdatesTimeWithoutChangingDate()
    {
        GameClock clock = new GameClock();

        clock.AdvanceMinutes(30);

        Assert.Equal(1, clock.Day);
        Assert.Equal(480, clock.MinutesOfDay);
        Assert.Equal(new DateOnly(2025, 9, 1), clock.CurrentDate);
    }

    [Fact]
    public void AdvanceMinutes_CrossingMidnight_AdvancesCampaignDayDateAndWeekday()
    {
        GameClock clock = new GameClock(startingHour: 23);

        clock.AdvanceMinutes(60);

        Assert.Equal(2, clock.Day);
        Assert.Equal(0, clock.MinutesOfDay);
        Assert.Equal(new DateOnly(2025, 9, 2), clock.CurrentDate);
        Assert.Equal(DayOfWeek.Tuesday, clock.Weekday);
    }

    [Fact]
    public void AdvanceMinutes_AtExactMidnight_AdvancesDateAndResetsTimeToZero()
    {
        GameClock clock = new GameClock(startingHour: 23);

        clock.AdvanceMinutes(60);

        Assert.Equal(new DateOnly(2025, 9, 2), clock.CurrentDate);
        Assert.Equal(0, clock.MinutesOfDay);
    }

    [Fact]
    public void AdvanceMinutes_CrossingMultipleMidnights_AdvancesEveryElapsedDay()
    {
        GameClock clock = new GameClock(startingHour: 23);

        clock.AdvanceMinutes(3000);

        Assert.Equal(4, clock.Day);
        Assert.Equal(60, clock.MinutesOfDay);
        Assert.Equal(new DateOnly(2025, 9, 4), clock.CurrentDate);
        Assert.Equal(DayOfWeek.Thursday, clock.Weekday);
    }

    [Fact]
    public void AdvanceMinutes_SleepDurationCrossesIntoNextMorning()
    {
        GameClock clock = new GameClock(startingHour: 23);

        clock.AdvanceMinutes(480);

        Assert.Equal(2, clock.Day);
        Assert.Equal(new DateOnly(2025, 9, 2), clock.CurrentDate);
        Assert.Equal(420, clock.MinutesOfDay);
        Assert.Equal(DayOfWeek.Tuesday, clock.Weekday);
    }

    [Fact]
    public void AdvanceMinutes_CrossingMonthBoundary_AdvancesToFirstDayOfNextMonth()
    {
        GameClock clock = new GameClock(
            startingHour: 23,
            startingMinute: 59,
            startingDate: new DateOnly(2025, 9, 30));

        clock.AdvanceMinutes(1);

        Assert.Equal(new DateOnly(2025, 10, 1), clock.CurrentDate);
        Assert.Equal(DayOfWeek.Wednesday, clock.Weekday);
    }

    [Fact]
    public void AdvanceMinutes_CrossingYearBoundary_AdvancesToNextYear()
    {
        GameClock clock = new GameClock(
            startingHour: 23,
            startingMinute: 59,
            startingDate: new DateOnly(2025, 12, 31));

        clock.AdvanceMinutes(1);

        Assert.Equal(new DateOnly(2026, 1, 1), clock.CurrentDate);
        Assert.Equal(DayOfWeek.Thursday, clock.Weekday);
    }

    [Fact]
    public void AdvanceMinutes_LeapYearIncludesFebruaryTwentyNinth()
    {
        GameClock clock = new GameClock(
            startingHour: 23,
            startingMinute: 59,
            startingDate: new DateOnly(2028, 2, 28));

        clock.AdvanceMinutes(1);

        Assert.Equal(new DateOnly(2028, 2, 29), clock.CurrentDate);
        Assert.Equal(DayOfWeek.Tuesday, clock.Weekday);

        clock.AdvanceMinutes(24 * 60);

        Assert.Equal(new DateOnly(2028, 3, 1), clock.CurrentDate);
        Assert.Equal(DayOfWeek.Wednesday, clock.Weekday);
    }

    [Fact]
    public void AdvanceMinutes_NonLeapYearSkipsFebruaryTwentyNinth()
    {
        GameClock clock = new GameClock(
            startingHour: 23,
            startingMinute: 59,
            startingDate: new DateOnly(2025, 2, 28));

        clock.AdvanceMinutes(1);

        Assert.Equal(new DateOnly(2025, 3, 1), clock.CurrentDate);
        Assert.Equal(DayOfWeek.Saturday, clock.Weekday);
    }

    [Fact]
    public void AdvanceMinutes_NegativeAmount_ThrowsAndPreservesClock()
    {
        GameClock clock = new GameClock(3, 12, 15);

        Assert.Throws<ArgumentOutOfRangeException>(() => clock.AdvanceMinutes(-1));

        Assert.Equal(3, clock.Day);
        Assert.Equal(735, clock.MinutesOfDay);
        Assert.Equal(new DateOnly(2025, 9, 3), clock.CurrentDate);
    }

    [Fact]
    public void AdvanceMinutes_CalendarOverflow_ThrowsAndPreservesClock()
    {
        GameClock clock = new GameClock(
            startingHour: 23,
            startingMinute: 59,
            startingDate: DateOnly.MaxValue);

        Assert.False(clock.CanAdvanceMinutes(1));
        Assert.Throws<OverflowException>(() => clock.AdvanceMinutes(1));

        Assert.Equal(1, clock.Day);
        Assert.Equal(1439, clock.MinutesOfDay);
        Assert.Equal(DateOnly.MaxValue, clock.CurrentDate);
    }

    [Fact]
    public void Constructor_StartingDayOutsideCalendarRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new GameClock(startingDay: 2, startingDate: DateOnly.MaxValue));
    }

    [Fact]
    public void CanApplyState_RejectsDateOutsideCalendarRange()
    {
        GameClock clock = new GameClock(startingDate: DateOnly.MaxValue);
        GameClockState invalidState = new GameClockState(2, 0);

        Assert.False(clock.CanApplyState(invalidState));
    }

    [Fact]
    public void Weekday_DerivesFromGregorianDateAcrossWeekWrap()
    {
        GameClock clock = new GameClock();

        clock.AdvanceMinutes(8 * 24 * 60);

        Assert.Equal(9, clock.Day);
        Assert.Equal(new DateOnly(2025, 9, 9), clock.CurrentDate);
        Assert.Equal(DayOfWeek.Tuesday, clock.Weekday);
    }
}
