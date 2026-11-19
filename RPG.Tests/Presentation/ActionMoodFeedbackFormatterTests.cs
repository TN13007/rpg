using System;
using Xunit;

public sealed class ActionMoodFeedbackFormatterTests
{
    [Fact]
    public void FormatAppendsActualPositiveMoodChange()
    {
        ActionDefinition action = CreateMoodAction(3, "Mira smiles.");

        string feedback = ActionMoodFeedbackFormatter.Format(action, 3);

        Assert.Equal("Mira smiles. Mood +3.", feedback);
    }

    [Fact]
    public void FormatAppendsActualNegativeMoodChange()
    {
        ActionDefinition action = CreateMoodAction(-2, "The available requests are disappointing.");

        string feedback = ActionMoodFeedbackFormatter.Format(action, -2);

        Assert.Equal("The available requests are disappointing. Mood -2.", feedback);
    }

    [Fact]
    public void FormatReportsWhenMoodEffectWasClampedToNoChange()
    {
        ActionDefinition action = CreateMoodAction(3, "Mira smiles.");

        string feedback = ActionMoodFeedbackFormatter.Format(action, 0);

        Assert.Equal("Mira smiles. Mood unchanged.", feedback);
    }

    [Fact]
    public void FormatDoesNotAppendMoodWhenActionHasNoMoodEffect()
    {
        ActionDefinition action = new ActionDefinition(
            "read", "Read", 10, feedback: "You read the note.");

        string feedback = ActionMoodFeedbackFormatter.Format(action, 3);

        Assert.Equal("You read the note.", feedback);
    }

    [Fact]
    public void FormatHandlesMoodActionWithoutAuthoredFeedback()
    {
        ActionDefinition action = CreateMoodAction(2, string.Empty);

        string feedback = ActionMoodFeedbackFormatter.Format(action, 2);

        Assert.Equal("Mood +2.", feedback);
    }

    [Fact]
    public void FormatRejectsNullAction()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ActionMoodFeedbackFormatter.Format(null, 0));
    }

    private static ActionDefinition CreateMoodAction(int amount, string feedback)
    {
        return new ActionDefinition(
            "mood-action",
            "Mood action",
            10,
            feedback: feedback,
            effects: new[]
            {
                new ActionEffectDefinition(ActionEffectType.ChangeMood, amount)
            });
    }
}
