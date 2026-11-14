using System;
using System.Collections.Generic;
using Godot;

public partial class StoryPanel : PanelContainer
{
    public event Action<string> ChoiceSelected;

    private Label _title;
    private Label _subtitle;
    private Label _day;
    private RichTextLabel _story;
    private VBoxContainer _choices;
    private Label _feedback;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _title = GetNode<Label>("Margin/Content/Header/Heading/Title");
        _subtitle = GetNode<Label>("Margin/Content/Header/Heading/Subtitle");
        _day = GetNode<Label>("Margin/Content/Header/Day");
        _story = GetNode<RichTextLabel>("Margin/Content/Story");
        _choices = GetNode<VBoxContainer>("Margin/Content/Choices");
        _feedback = GetNode<Label>("Margin/Content/Feedback");
    }

    public void ShowContent(string title, string subtitle, string text, IReadOnlyList<StoryChoice> choices, string dayLabel)
    {
        _title.Text = title;
        _subtitle.Text = subtitle;
        _day.Text = dayLabel;
        _story.Text = text;
        _feedback.Text = "Choose an option to continue.";
        ClearChoices();

        foreach (StoryChoice choice in choices)
        {
            StoryChoiceButton button = new StoryChoiceButton
            {
                Text = choice.Label,
                ChoiceId = choice.Id,
                CustomMinimumSize = new Vector2(0, 40),
                FocusMode = FocusModeEnum.All
            };
            UiTheme.StyleButton(button);
            button.ChoiceSelected += HandleChoiceSelected;
            _choices.AddChild(button);
        }
    }

    public void ShowFeedback(string message)
    {
        _feedback.Text = message;
    }

    private void HandleChoiceSelected(string choiceId)
    {
        ChoiceSelected?.Invoke(choiceId);
    }

    private void ClearChoices()
    {
        foreach (Node child in _choices.GetChildren())
        {
            _choices.RemoveChild(child);
            child.QueueFree();
        }
    }
}
