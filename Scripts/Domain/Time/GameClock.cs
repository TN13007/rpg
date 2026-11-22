using System;

/// <summary>Tracks the current in-game day and minute of the day.</summary>
public sealed class GameClock
{
    private const int _minutesPerDay = 24 * 60;

    /// <summary>Gets the day.</summary>
    public int Day { get; private set; }
    /// <summary>Gets the minutes of day.</summary>
    public int MinutesOfDay { get; private set; }

    /// <summary>Initializes a new instance of <see cref="GameClock"/>.</summary>
    /// <param name="startingDay">The starting day used to initialize the instance.</param>
    /// <param name="startingHour">The starting hour used to initialize the instance.</param>
    /// <param name="startingMinute">The starting minute used to initialize the instance.</param>
    public GameClock(int startingDay = 1, int startingHour = 7, int startingMinute = 30)
    {
        if (startingDay < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(startingDay));
        }

        if (startingHour < 0 || startingHour > 23)
        {
            throw new ArgumentOutOfRangeException(nameof(startingHour));
        }

        if (startingMinute < 0 || startingMinute > 59)
        {
            throw new ArgumentOutOfRangeException(nameof(startingMinute));
        }

        Day = startingDay;
        MinutesOfDay = startingHour * 60 + startingMinute;
    }

    /// <summary>Checks whether the clock can advance without overflowing its day counter.</summary>
    /// <param name="amount">The non-negative number of minutes to advance.</param>
    /// <returns>True when the requested advancement can be represented; otherwise false.</returns>
    public bool CanAdvanceMinutes(int amount)
    {
        if (amount < 0)
        {
            return false;
        }

        long totalMinutes = (long)MinutesOfDay + amount;
        long elapsedDays = totalMinutes / _minutesPerDay;
        return elapsedDays <= int.MaxValue - (long)Day;
    }

    /// <summary>Advances minutes by the supplied amount.</summary>
    /// <param name="amount">The non-negative number of minutes to advance.</param>
    public void AdvanceMinutes(int amount)
    {
        if (!CanAdvanceMinutes(amount))
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "The GameClock cannot represent this advancement.");
        }

        long totalMinutes = (long)MinutesOfDay + amount;
        int elapsedDays = (int)(totalMinutes / _minutesPerDay);
        int nextDay = Day + elapsedDays;
        int nextMinutesOfDay = (int)(totalMinutes % _minutesPerDay);

        Day = nextDay;
        MinutesOfDay = nextMinutesOfDay;
    }
}
