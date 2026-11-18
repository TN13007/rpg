using System;

public static class GameClockDisplayFormatter
{
    public static string Format(GameClock clock)
    {
        if (clock == null)
        {
            throw new ArgumentNullException(nameof(clock));
        }

        return Format(clock.Day, clock.MinutesOfDay);
    }

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
