using System;

public sealed class GameClock
{
    private const int _minutesPerDay = 24 * 60;

    public int Day { get; private set; }
    public int MinutesOfDay { get; private set; }

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

    public void AdvanceMinutes(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        long totalMinutes = (long)MinutesOfDay + amount;
        int elapsedDays = (int)(totalMinutes / _minutesPerDay);
        int nextDay = checked(Day + elapsedDays);
        int nextMinutesOfDay = (int)(totalMinutes % _minutesPerDay);

        Day = nextDay;
        MinutesOfDay = nextMinutesOfDay;
    }

}
