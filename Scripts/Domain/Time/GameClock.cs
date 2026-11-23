using System;

/// <summary>Tracks campaign time and its derived Gregorian calendar date.</summary>
public sealed class GameClock
{
    private const int _minutesPerDay = 24 * 60;
    private static readonly DateOnly _defaultStartingDate = new DateOnly(2025, 9, 1);
    private readonly DateOnly _startingDate;

    /// <summary>Gets the one-based campaign day.</summary>
    public int Day { get; private set; }
    /// <summary>Gets the minutes elapsed since midnight.</summary>
    public int MinutesOfDay { get; private set; }
    /// <summary>Gets the authored Gregorian date assigned to campaign Day 1.</summary>
    public DateOnly StartingDate => _startingDate;
    /// <summary>Gets the Gregorian date derived from the campaign day.</summary>
    public DateOnly CurrentDate => _startingDate.AddDays(Day - 1);
    /// <summary>Gets the weekday assigned to campaign Day 1.</summary>
    public DayOfWeek StartingWeekday => _startingDate.DayOfWeek;
    /// <summary>Gets the weekday derived from the current Gregorian date.</summary>
    public DayOfWeek Weekday => CurrentDate.DayOfWeek;

    /// <summary>Initializes a new instance of <see cref="GameClock"/>.</summary>
    /// <param name="startingDay">The one-based campaign day used to initialize the instance.</param>
    /// <param name="startingHour">The starting hour used to initialize the instance.</param>
    /// <param name="startingMinute">The starting minute used to initialize the instance.</param>
    /// <param name="startingDate">The Gregorian date assigned to campaign Day 1; defaults to Monday, 1 September 2025.</param>
    public GameClock(
        int startingDay = 1,
        int startingHour = 7,
        int startingMinute = 30,
        DateOnly? startingDate = null)
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

        _startingDate = startingDate ?? _defaultStartingDate;
        if (!IsDateRepresentable(startingDay))
        {
            throw new ArgumentOutOfRangeException(
                nameof(startingDay),
                "The campaign day exceeds the supported Gregorian calendar range.");
        }

        Day = startingDay;
        MinutesOfDay = startingHour * 60 + startingMinute;
    }

    /// <summary>Checks whether the clock can advance by the supplied non-negative duration.</summary>
    /// <param name="amount">The number of minutes to advance.</param>
    /// <returns>True when the requested advancement can be represented; otherwise false.</returns>
    public bool CanAdvanceMinutes(int amount)
    {
        return TryComputeAdvance(Day, MinutesOfDay, amount, out _, out _);
    }

    /// <summary>Captures the current clock position as an immutable value.</summary>
    internal GameClockState CaptureState()
    {
        return new GameClockState(Day, MinutesOfDay);
    }

    /// <summary>Checks whether a prepared clock state is valid for this owner and calendar.</summary>
    internal bool CanApplyState(GameClockState state)
    {
        return state != null
            && state.Day >= 1
            && state.MinutesOfDay >= 0
            && state.MinutesOfDay < _minutesPerDay
            && IsDateRepresentable(state.Day);
    }

    /// <summary>Commits a prepared clock position through the GameClock state owner.</summary>
    internal void ApplyState(GameClockState state)
    {
        if (!CanApplyState(state))
        {
            throw new InvalidOperationException("The prepared clock state is invalid.");
        }

        Day = state.Day;
        MinutesOfDay = state.MinutesOfDay;
    }

    /// <summary>Advances elapsed minutes and commits the new clock position.</summary>
    /// <param name="amount">The non-negative number of minutes to advance.</param>
    public void AdvanceMinutes(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        if (!TryComputeAdvance(Day, MinutesOfDay, amount, out int nextDay, out int nextMinutesOfDay))
        {
            throw new OverflowException("The GameClock cannot advance beyond its supported day or calendar range.");
        }

        Day = nextDay;
        MinutesOfDay = nextMinutesOfDay;
    }

    /// <summary>Attempts to advance the clock; returns false when the requested position is not representable.</summary>
    /// <param name="amount">The non-negative number of minutes to advance.</param>
    /// <returns>True when the clock advanced; otherwise false without changing its state.</returns>
    public bool TryAdvanceMinutes(int amount)
    {
        if (!TryComputeAdvance(Day, MinutesOfDay, amount, out int nextDay, out int nextMinutesOfDay))
        {
            return false;
        }

        Day = nextDay;
        MinutesOfDay = nextMinutesOfDay;
        return true;
    }

    private bool TryComputeAdvance(
        int currentDay,
        int currentMinutes,
        int amount,
        out int nextDay,
        out int nextMinutesOfDay)
    {
        nextDay = 0;
        nextMinutesOfDay = 0;

        if (amount < 0)
        {
            return false;
        }

        long totalMinutes = (long)currentMinutes + amount;
        long elapsedDays = totalMinutes / _minutesPerDay;
        long candidateDay = currentDay + elapsedDays;

        if (candidateDay > int.MaxValue || !IsDateRepresentable((int)candidateDay))
        {
            return false;
        }

        nextDay = (int)candidateDay;
        nextMinutesOfDay = (int)(totalMinutes % _minutesPerDay);
        return true;
    }

    private bool IsDateRepresentable(int day)
    {
        long dateDayNumber = (long)_startingDate.DayNumber + day - 1;
        return dateDayNumber <= DateOnly.MaxValue.DayNumber;
    }
}
