using System;

public sealed class DayClock
{
    private const int _minutesPerDay = 24 * 60;
    private const int _wakeUpMinutes = 7 * 60 + 30;

    public int Day { get; private set; }
    public int MinutesOfDay { get; private set; }

    public DayClock(int startingDay = 1, int startingHour = 7, int startingMinute = 30)
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

        Day = checked(Day + elapsedDays);
        MinutesOfDay = (int)(totalMinutes % _minutesPerDay);
    }

    public void SleepUntilNextDay()
    {
        if (MinutesOfDay >= _wakeUpMinutes)
        {
            Day = checked(Day + 1);
        }

        MinutesOfDay = _wakeUpMinutes;
    }

    public string GetTimeOfDay()
    {
        if (MinutesOfDay < 12 * 60)
        {
            return "MORNING";
        }

        if (MinutesOfDay < 17 * 60)
        {
            return "AFTERNOON";
        }

        if (MinutesOfDay < 21 * 60)
        {
            return "EVENING";
        }

        return "NIGHT";
    }

    public string GetDisplayText()
    {
        int hour = MinutesOfDay / 60;
        int minute = MinutesOfDay % 60;
        return "DAY " + Day.ToString("00") + "  ·  " + GetTimeOfDay() + "  ·  " + hour.ToString("00") + ":" + minute.ToString("00");
    }
}
