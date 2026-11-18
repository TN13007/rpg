using System;
using Xunit;

public sealed class GameClockDisplayFormatterTests
{
    [Theory]
    [InlineData(1, 7, 30, "DAY 01  ·  MORNING  ·  07:30")]
    [InlineData(2, 12, 0, "DAY 02  ·  AFTERNOON  ·  12:00")]
    [InlineData(3, 17, 0, "DAY 03  ·  EVENING  ·  17:00")]
    [InlineData(4, 21, 0, "DAY 04  ·  NIGHT  ·  21:00")]
    public void Format_GameClock_ReturnsPresentationLabel(
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
}
