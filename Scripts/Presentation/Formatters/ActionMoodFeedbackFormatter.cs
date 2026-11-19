using System;

public static class ActionMoodFeedbackFormatter
{
    public static string Format(ActionDefinition action, int actualMoodChange)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        bool hasMoodEffect = false;
        foreach (ActionEffectDefinition effect in action.Effects)
        {
            if (effect.Type == ActionEffectType.ChangeMood)
            {
                hasMoodEffect = true;
                break;
            }
        }

        if (!hasMoodEffect)
        {
            return action.Feedback;
        }

        string moodResult;
        if (actualMoodChange > 0)
        {
            moodResult = "Mood +" + actualMoodChange;
        }
        else if (actualMoodChange < 0)
        {
            moodResult = "Mood " + actualMoodChange;
        }
        else
        {
            moodResult = "Mood unchanged";
        }

        if (string.IsNullOrWhiteSpace(action.Feedback))
        {
            return moodResult + ".";
        }

        return action.Feedback.TrimEnd() + " " + moodResult + ".";
    }
}
