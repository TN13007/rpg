using System;

/// <summary>Immutable validated snapshot of the GameClock position.</summary>
public sealed class GameClockState : IEquatable<GameClockState>
{
    private const int _minutesPerDay = 24 * 60;
    /// <summary>Gets the one-based in-game day.</summary>
    public int Day { get; }
    /// <summary>Gets minutes elapsed since midnight.</summary>
    public int MinutesOfDay { get; }

    /// <summary>Creates a validated immutable clock position.</summary>
    /// <param name="day">The one-based day.</param>
    /// <param name="minutesOfDay">Minutes since midnight, from zero through 1439.</param>
    public GameClockState(int day, int minutesOfDay)
    {
        if (day < 1) throw new ArgumentOutOfRangeException(nameof(day));
        if (minutesOfDay < 0 || minutesOfDay >= _minutesPerDay)
            throw new ArgumentOutOfRangeException(nameof(minutesOfDay));
        Day = day;
        MinutesOfDay = minutesOfDay;
    }

    /// <summary>Returns a new clock state advanced by the supplied non-negative duration.</summary>
    /// <summary>Returns a new clock state advanced by a non-negative duration.</summary>
    /// <param name="amount">The number of minutes to advance.</param>
    /// <returns>The advanced clock state.</returns>
    public GameClockState AdvanceMinutes(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        long totalMinutes = (long)MinutesOfDay + amount;
        long elapsedDays = totalMinutes / _minutesPerDay;
        if (elapsedDays > int.MaxValue - (long)Day)
            throw new OverflowException("The GameClock day counter cannot represent this advancement.");
        return new GameClockState(Day + (int)elapsedDays, (int)(totalMinutes % _minutesPerDay));
    }

    /// <summary>Determines whether another clock snapshot has the same position.</summary>
    public bool Equals(GameClockState other)
        => other != null && Day == other.Day && MinutesOfDay == other.MinutesOfDay;
    /// <summary>Determines whether another object is an equal clock snapshot.</summary>
    public override bool Equals(object obj) => Equals(obj as GameClockState);
    /// <summary>Returns a hash code for the clock position.</summary>
    public override int GetHashCode() => HashCode.Combine(Day, MinutesOfDay);
}
