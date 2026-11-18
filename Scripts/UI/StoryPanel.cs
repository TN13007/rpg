using System;
using System.Collections.Generic;
using Godot;

public partial class StoryPanel : PanelContainer
{
    public event Action<string> ChoiceSelected;
    public event Action ReturnToLocationRequested;

    private Label _title;
    private Label _subtitle;
    private Label _day;
    private Label _timeFeedback;
    private RichTextLabel _story;
    private VBoxContainer _choices;
    private Label _choicesTitle;
    private Label _feedback;
    private Button _returnButton;
    private Tween _clockTween;
    private Tween _feedbackTween;
    private int _clockStartTotalMinutes;
    private int _clockEndTotalMinutes;
    private bool _isClockAnimating;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _title = GetNode<Label>("Margin/Content/Header/Heading/Title");
        _subtitle = GetNode<Label>("Margin/Content/Header/Heading/Subtitle");
        _day = GetNode<Label>("Margin/Content/Header/Day");
        _timeFeedback = GetNode<Label>("Margin/Content/Header/TimeFeedback");
        _timeFeedback.Visible = false;
        _story = GetNode<RichTextLabel>("Margin/Content/Story");
        _choices = GetNode<VBoxContainer>("Margin/Content/Choices");
        _choicesTitle = GetNode<Label>("Margin/Content/ChoicesTitle");
        _feedback = GetNode<Label>("Margin/Content/Feedback");
        _returnButton = GetNode<Button>("Margin/Content/Header/ReturnButton");
        _returnButton.Pressed += HandleReturnPressed;
        _returnButton.Visible = false;
        UiTheme.StyleButton(_returnButton);
    }

    public void ShowContent(string title, string subtitle, string text, IReadOnlyList<ActionChoiceViewData> choices, string dayLabel)
    {
        _title.Text = title;
        _subtitle.Text = subtitle;
        if (!_isClockAnimating)
        {
            _day.Text = dayLabel;
        }

        _story.Text = text;
        _feedback.Text = "Choose an option to continue.";
        _returnButton.Visible = false;
        ClearChoices();
        string currentGroup = null;

        foreach (ActionChoiceViewData choice in choices)
        {
            if (!string.IsNullOrWhiteSpace(choice.GroupTitle) && choice.GroupTitle != currentGroup)
            {
                Label groupHeading = new Label
                {
                    Text = choice.GroupTitle,
                    Modulate = new Color(0.72f, 0.78f, 0.88f, 1.0f)
                };
                groupHeading.AddThemeFontSizeOverride("font_size", 12);
                groupHeading.AddThemeColorOverride("font_color", new Color(0.72f, 0.78f, 0.88f, 1.0f));
                _choices.AddChild(groupHeading);
                currentGroup = choice.GroupTitle;
            }

            StoryChoiceButton button = new StoryChoiceButton
            {
                Text = string.IsNullOrEmpty(choice.DurationLabel)
                    ? choice.Label
                    : choice.Label + "  (" + choice.DurationLabel + ")",
                ChoiceId = choice.Id,
                CustomMinimumSize = new Vector2(0, 40),
                FocusMode = FocusModeEnum.All
            };
            UiTheme.StyleButton(button);
            button.ChoiceSelected += HandleChoiceSelected;
            _choices.AddChild(button);
        }
    }

    public void ShowTimeAdvanceFeedback(int startDay, int startMinutes, int endDay, int endMinutes)
    {
        StopClockAnimations();

        _clockStartTotalMinutes = startDay * 1440 + startMinutes;
        _clockEndTotalMinutes = endDay * 1440 + endMinutes;

        int elapsedMinutes = _clockEndTotalMinutes - _clockStartTotalMinutes;
        if (elapsedMinutes <= 0)
        {
            _day.Text = FormatClockDisplay(endDay, endMinutes);
            return;
        }

        _isClockAnimating = true;
        _day.Text = FormatClockDisplay(startDay, startMinutes);
        _timeFeedback.Text = "+" + FormatElapsedTime(elapsedMinutes);
        _timeFeedback.Modulate = Colors.White;
        _timeFeedback.Visible = true;

        float animationDuration = Mathf.Clamp(0.5f + elapsedMinutes * 0.015f, 0.6f, 1.8f);
        _clockTween = CreateTween();
        _clockTween.TweenMethod(
            Callable.From<float>(UpdateClockProgress),
            0.0f,
            1.0f,
            animationDuration);
        _clockTween.TweenCallback(Callable.From(FinishClockAnimation));

        _feedbackTween = CreateTween();
        _feedbackTween.TweenInterval(4.8);
        _feedbackTween.TweenProperty(_timeFeedback, "modulate:a", 0.0f, 0.7);
        _feedbackTween.TweenCallback(Callable.From(HideTimeFeedback));
    }

    public void ShowFeedback(string message)
    {
        _feedback.Text = message;
    }

    public void ShowLocationButton(bool visible)
    {
        _returnButton.Visible = visible;
    }

    private void UpdateClockProgress(float progress)
    {
        int totalMinutes = _clockStartTotalMinutes
            + Mathf.RoundToInt((_clockEndTotalMinutes - _clockStartTotalMinutes) * progress);
        int day = totalMinutes / 1440;
        int minutesOfDay = totalMinutes % 1440;
        _day.Text = FormatClockDisplay(day, minutesOfDay);
    }

    private void FinishClockAnimation()
    {
        _day.Text = FormatClockDisplay(
            _clockEndTotalMinutes / 1440,
            _clockEndTotalMinutes % 1440);
        _isClockAnimating = false;
    }

    private void StopClockAnimations()
    {
        if (_clockTween != null && _clockTween.IsRunning())
        {
            _clockTween.Kill();
        }

        if (_feedbackTween != null && _feedbackTween.IsRunning())
        {
            _feedbackTween.Kill();
        }

        _isClockAnimating = false;
        HideTimeFeedback();
    }

    private void HideTimeFeedback()
    {
        _timeFeedback.Visible = false;
        _timeFeedback.Text = string.Empty;
        _timeFeedback.Modulate = Colors.White;
    }

    private string FormatClockDisplay(int day, int minutesOfDay)
    {
        int hour = minutesOfDay / 60;
        int minute = minutesOfDay % 60;
        string timeOfDay = "NIGHT";

        if (minutesOfDay < 12 * 60)
        {
            timeOfDay = "MORNING";
        }
        else if (minutesOfDay < 17 * 60)
        {
            timeOfDay = "AFTERNOON";
        }
        else if (minutesOfDay < 21 * 60)
        {
            timeOfDay = "EVENING";
        }

        return "DAY " + day.ToString("00")
            + "  ·  " + timeOfDay
            + "  ·  " + hour.ToString("00")
            + ":" + minute.ToString("00");
    }

    private string FormatElapsedTime(int totalMinutes)
    {
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;
        return hours.ToString() + ":" + minutes.ToString("00");
    }

    private void HandleChoiceSelected(string choiceId)
    {
        ChoiceSelected?.Invoke(choiceId);
    }

    private void HandleReturnPressed()
    {
        ReturnToLocationRequested?.Invoke();
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
