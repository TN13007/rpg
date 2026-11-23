using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;

/// <summary>Presents story content and available player choices.</summary>
public partial class StoryPanel : PanelContainer
{
    /// <summary>Raised when choice selected occurs.</summary>
    public event Action<string> ChoiceSelected;
    /// <summary>Raised when return to location requested occurs.</summary>
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
    private long _clockStartTotalMinutes;
    private long _clockEndTotalMinutes;
    private DateOnly _clockStartingDate;
    private bool _isClockAnimating;

    /// <summary>Initializes the Godot control and connects its UI signals.</summary>
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

    /// <summary>Displays content in the story interface.</summary>
    /// <param name="title">The title used by this operation.</param>
    /// <param name="subtitle">The subtitle used by this operation.</param>
    /// <param name="text">The text used by this operation.</param>
    /// <param name="choices">The choices used by this operation.</param>
    /// <param name="dayLabel">The day label used by this operation.</param>
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

    /// <summary>Displays time advance feedback in the story interface.</summary>
    /// <param name="startDay">The start day used by this operation.</param>
    /// <param name="startMinutes">The start minutes used by this operation.</param>
    /// <param name="endDay">The end day used by this operation.</param>
    /// <param name="endMinutes">The end minutes used by this operation.</param>
    /// <param name="startingDate">The Gregorian date assigned to campaign Day 1.</param>
    public void ShowTimeAdvanceFeedback(
        int startDay,
        int startMinutes,
        int endDay,
        int endMinutes,
        DateOnly startingDate)
    {
        StopClockAnimations();

        _clockStartingDate = startingDate;
        _clockStartTotalMinutes = (long)startDay * 1440 + startMinutes;
        _clockEndTotalMinutes = (long)endDay * 1440 + endMinutes;

        long elapsedMinutes = _clockEndTotalMinutes - _clockStartTotalMinutes;
        if (elapsedMinutes <= 0)
        {
            _day.Text = GameClockDisplayFormatter.Format(startingDate, endDay, endMinutes);
            return;
        }

        _isClockAnimating = true;
        _day.Text = GameClockDisplayFormatter.Format(startingDate, startDay, startMinutes);
        _timeFeedback.Text = "+" + FormatElapsedTime(elapsedMinutes);
        _timeFeedback.Modulate = Colors.White;
        _timeFeedback.Visible = true;

        float animationDuration = Mathf.Clamp(0.5f + (float)elapsedMinutes * 0.015f, 0.6f, 1.8f);
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

    /// <summary>Displays feedback in the story interface.</summary>
    /// <param name="message">The message used by this operation.</param>
    public void ShowFeedback(string message)
    {
        _feedback.Text = message;
    }

    /// <summary>Displays location button in the story interface.</summary>
    /// <param name="visible">The visible used by this operation.</param>
    public void ShowLocationButton(bool visible)
    {
        _returnButton.Visible = visible;
    }

    private void UpdateClockProgress(float progress)
    {
        long elapsedMinutes = _clockEndTotalMinutes - _clockStartTotalMinutes;
        long totalMinutes = _clockStartTotalMinutes
            + (long)Math.Round(elapsedMinutes * progress);
        int day = (int)(totalMinutes / 1440);
        int minutesOfDay = (int)(totalMinutes % 1440);
        _day.Text = GameClockDisplayFormatter.Format(_clockStartingDate, day, minutesOfDay);
    }

    private void FinishClockAnimation()
    {
        _day.Text = GameClockDisplayFormatter.Format(
            _clockStartingDate,
            (int)(_clockEndTotalMinutes / 1440),
            (int)(_clockEndTotalMinutes % 1440));
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

    private string FormatElapsedTime(long totalMinutes)
    {
        long hours = totalMinutes / 60;
        long minutes = totalMinutes % 60;
        return hours.ToString(CultureInfo.InvariantCulture)
            + ":" + minutes.ToString("00", CultureInfo.InvariantCulture);
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
