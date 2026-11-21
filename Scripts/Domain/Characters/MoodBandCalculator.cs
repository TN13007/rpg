using System;

/// <summary>Represents a qualitative range of a character's current mood.</summary>
public enum MoodBand
{
    /// <summary>Mood is below 25 percent of its maximum.</summary>
    VeryLow,
    /// <summary>Mood is at least 25 percent and below 50 percent of its maximum.</summary>
    Low,
    /// <summary>Mood is at least 50 percent and below 75 percent of its maximum.</summary>
    Steady,
    /// <summary>Mood is at least 75 percent of its maximum.</summary>
    Positive
}

/// <summary>Calculates mood band values from gameplay data.</summary>
public static class MoodBandCalculator
{
    /// <summary>Calculates calculate from the supplied values.</summary>
    /// <param name="mood">The mood used by this operation.</param>
    /// <param name="maxMood">The max mood used by this operation.</param>
    /// <returns>The resulting mood band.</returns>
    public static MoodBand Calculate(int mood, int maxMood)
    {
        if (maxMood < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxMood));
        }

        if (mood < 0 || mood > maxMood)
        {
            throw new ArgumentOutOfRangeException(nameof(mood));
        }

        if (maxMood == 0)
        {
            return MoodBand.VeryLow;
        }

        long scaledMood = (long)mood * 100;
        long scaledMaximum = maxMood;

        if (scaledMood < scaledMaximum * 25)
        {
            return MoodBand.VeryLow;
        }

        if (scaledMood < scaledMaximum * 50)
        {
            return MoodBand.Low;
        }

        if (scaledMood < scaledMaximum * 75)
        {
            return MoodBand.Steady;
        }

        return MoodBand.Positive;
    }
}
