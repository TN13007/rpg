using System;

/// <summary>Formats game clock display values for display.</summary>
public static class GameClockDisplayFormatter
{
    /// <summary>Formats format for display.</summary>
    /// <param name="clock">The clock used by this operation.</param>
    /// <returns>The resulting string.</returns>
    public static string Format(GameClock clock)
    {
        if (clock == null)
        {
            throw new ArgumentNullException(nameof(clock));
        }

        return Format(clock.Day, clock.MinutesOfDay);
    }

    /// <summary>Formats format for display.</summary>
    /// <param name="day">The day used by this operation.</param>
    /// <param name="minutesOfDay">The minutes of day used by this operation.</param>
    /// <returns>The resulting string.</returns>
    public static string Format(int day, int minutesOfDay)
    {
        if (day < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(day));
        }

        if (minutesOfDay < 0 || minutesOfDay >= 24 * 60)
        {
            throw new ArgumentOutOfRangeException(nameof(minutesOfDay));
        }

        int hour = minutesOfDay / 60;
        int minute = minutesOfDay % 60;
        string timeOfDay = GetTimeOfDay(minutesOfDay);

        return "DAY " + day.ToString("00")
            + "  ·  " + timeOfDay
            + "  ·  " + hour.ToString("00")
            + ":" + minute.ToString("00");
    }

    private static string GetTimeOfDay(int minutesOfDay)
    {
        if (minutesOfDay < 12 * 60)
        {
            return "MORNING";
        }

        if (minutesOfDay < 17 * 60)
        {
            return "AFTERNOON";
        }

        if (minutesOfDay < 21 * 60)
        {
            return "EVENING";
        }

        return "NIGHT";
    }
}
