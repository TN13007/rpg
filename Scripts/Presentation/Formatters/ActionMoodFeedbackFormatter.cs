using System;

/// <summary>Formats action mood feedback information for display.</summary>
public static class ActionMoodFeedbackFormatter
{
    /// <summary>Formats the action feedback using the action definition and actual mood change.</summary>
    /// <param name="action">The action used by this operation.</param>
    /// <param name="actualMoodChange">The actual mood change used by this operation.</param>
    /// <returns>The resulting string.</returns>
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
