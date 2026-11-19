using System;

public enum MoodBand
{
    VeryLow,
    Low,
    Steady,
    Positive
}

public static class MoodBandCalculator
{
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
