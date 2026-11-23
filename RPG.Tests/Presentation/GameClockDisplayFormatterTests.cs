using System;
using Xunit;

public sealed class GameClockDisplayFormatterTests
{
    [Theory]
    [InlineData(1, 7, 30, "DAY 01  ·  MON, 01 SEP 2025  ·  MORNING  ·  07:30")]
    [InlineData(2, 12, 0, "DAY 02  ·  TUE, 02 SEP 2025  ·  AFTERNOON  ·  12:00")]
    [InlineData(3, 17, 0, "DAY 03  ·  WED, 03 SEP 2025  ·  EVENING  ·  17:00")]
    [InlineData(4, 21, 0, "DAY 04  ·  THU, 04 SEP 2025  ·  NIGHT  ·  21:00")]
    public void Format_GameClock_ReturnsCampaignDateAndTimeLabel(
        int day,
        int hour,
        int minute,
        string expected)
    {
        GameClock clock = new GameClock(day, hour, minute);

        string result = GameClockDisplayFormatter.Format(clock);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Format_CustomStartDate_UsesGregorianWeekdayAndDate()
    {
        GameClock clock = new GameClock(
            startingDate: new DateOnly(2028, 2, 29));

        Assert.Equal(
            "DAY 01  ·  TUE, 29 FEB 2028  ·  MORNING  ·  07:30",
            GameClockDisplayFormatter.Format(clock));
    }

    [Fact]
    public void Format_NullClock_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => GameClockDisplayFormatter.Format(null));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, -1)]
    [InlineData(1, 1440)]
    public void Format_InvalidClockValues_Throws(int day, int minutesOfDay)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => GameClockDisplayFormatter.Format(day, minutesOfDay));
    }

    [Fact]
    public void Format_DateBeyondCalendarRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => GameClockDisplayFormatter.Format(DateOnly.MaxValue, 2, 0));
    }
}
