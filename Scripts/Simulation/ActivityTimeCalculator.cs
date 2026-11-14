using System;

public sealed class ActivityTimeCalculator
{
    public int Calculate(StoryChoice activity, Character character)
    {
        if (activity == null)
        {
            throw new ArgumentNullException(nameof(activity));
        }

        return Calculate(activity.DurationMinutes, character, activity.TimeModifier);
    }

    public int Calculate(ActivityDefinition activity, Character character)
    {
        if (activity == null)
        {
            throw new ArgumentNullException(nameof(activity));
        }

        return Calculate(activity.DurationMinutes, character, activity.TimeModifier);
    }

    private int Calculate(int baseDuration, Character character, ActivityTimeModifier modifier)
    {
        if (character == null)
        {
            throw new ArgumentNullException(nameof(character));
        }

        if (baseDuration == 0 || modifier == null)
        {
            return baseDuration;
        }

        int statValue = character.Stats.GetValue(modifier.Stat);
        double reduction = statValue / 100.0 * modifier.MaxReduction;
        double adjustedDuration = baseDuration * (1.0 - reduction);

        return Math.Max(1, (int)Math.Ceiling(adjustedDuration));
    }
}
