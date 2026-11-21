using System;

/// <summary>Configures how activity time values are adjusted.</summary>
public sealed class ActivityTimeModifier
{
    /// <summary>Gets the stat.</summary>
    public CharacterStat Stat { get; }
    /// <summary>Gets the max reduction.</summary>
    public double MaxReduction { get; }

    /// <summary>Initializes a new instance of <see cref="ActivityTimeModifier"/>.</summary>
    /// <param name="stat">The stat used to initialize the instance.</param>
    /// <param name="maxReduction">The max reduction used to initialize the instance.</param>
    public ActivityTimeModifier(CharacterStat stat, double maxReduction)
    {
        if (!Enum.IsDefined(typeof(CharacterStat), stat))
        {
            throw new ArgumentOutOfRangeException(nameof(stat));
        }

        if (maxReduction < 0 || maxReduction > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxReduction), "Maximum time reduction must be between 0 and 1.");
        }

        Stat = stat;
        MaxReduction = maxReduction;
    }
}
