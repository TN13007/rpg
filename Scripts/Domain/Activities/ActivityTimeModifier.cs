using System;

public sealed class ActivityTimeModifier
{
    public CharacterStat Stat { get; }
    public double MaxReduction { get; }

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
