using System;
using System.Globalization;

/// <summary>Formats game clock values for the player-facing time label.</summary>
public static class GameClockDisplayFormatter
{
    /// <summary>Formats the current campaign date and time.</summary>
    /// <param name="clock">The clock to format.</param>
    /// <returns>The formatted calendar and time label.</returns>
    public static string Format(GameClock clock)
    {
        if (clock == null)
        {
            throw new ArgumentNullException(nameof(clock));
        }

        return Format(clock.StartingDate, clock.Day, clock.MinutesOfDay);
    }

    /// <summary>Formats a campaign day and time against its authored Gregorian start date.</summary>
    /// <param name="startingDate">The Gregorian date assigned to campaign Day 1.</param>
    /// <param name="day">The one-based campaign day.</param>
    /// <param name="minutesOfDay">Minutes elapsed since midnight.</param>
    /// <returns>The formatted calendar and time label.</returns>
    public static string Format(DateOnly startingDate, int day, int minutesOfDay)
    {
        ValidateClockValues(day, minutesOfDay);

        long currentDayNumber = (long)startingDate.DayNumber + day - 1;
        if (currentDayNumber > DateOnly.MaxValue.DayNumber)
        {
            throw new ArgumentOutOfRangeException(nameof(day));
        }

        DateOnly currentDate = DateOnly.FromDayNumber((int)currentDayNumber);
        string weekdayAndDate = currentDate
            .ToString("ddd, dd MMM yyyy", CultureInfo.InvariantCulture)
            .ToUpperInvariant();

        return "DAY " + day.ToString("00", CultureInfo.InvariantCulture)
            + "  ·  " + weekdayAndDate
            + "  ·  " + GetTimeOfDay(minutesOfDay)
            + "  ·  " + FormatTime(minutesOfDay);
    }

    /// <summary>Formats a campaign day and time without calendar-date context.</summary>
    /// <param name="day">The one-based campaign day.</param>
    /// <param name="minutesOfDay">Minutes elapsed since midnight.</param>
    /// <returns>The formatted day and time label.</returns>
    public static string Format(int day, int minutesOfDay)
    {
        ValidateClockValues(day, minutesOfDay);

        return "DAY " + day.ToString("00", CultureInfo.InvariantCulture)
            + "  ·  " + GetTimeOfDay(minutesOfDay)
            + "  ·  " + FormatTime(minutesOfDay);
    }

    private static void ValidateClockValues(int day, int minutesOfDay)
    {
        if (day < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(day));
        }

        if (minutesOfDay < 0 || minutesOfDay >= 24 * 60)
        {
            throw new ArgumentOutOfRangeException(nameof(minutesOfDay));
        }
    }

    private static string FormatTime(int minutesOfDay)
    {
        int hour = minutesOfDay / 60;
        int minute = minutesOfDay % 60;
        return hour.ToString("00", CultureInfo.InvariantCulture)
            + ":" + minute.ToString("00", CultureInfo.InvariantCulture);
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
