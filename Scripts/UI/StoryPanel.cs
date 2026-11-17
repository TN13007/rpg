using System;
using System.Collections.Generic;
using Godot;

public partial class StoryPanel : PanelContainer
{
    public event Action<string> ChoiceSelected;
    public event Action ReturnToLocationRequested;
    public event Action ActionPreviewConfirmed;
    public event Action ActionPreviewCancelled;

    private Label _title;
    private Label _subtitle;
    private Label _day;
    private RichTextLabel _story;
    private VBoxContainer _choices;
    private Label _choicesTitle;
    private Label _feedback;
    private Button _returnButton;
    private VBoxContainer _actionPreview;
    private Label _previewTitle;
    private RichTextLabel _previewDetails;
    private Button _confirmButton;
    private Button _cancelButton;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _title = GetNode<Label>("Margin/Content/Header/Heading/Title");
        _subtitle = GetNode<Label>("Margin/Content/Header/Heading/Subtitle");
        _day = GetNode<Label>("Margin/Content/Header/Day");
        _story = GetNode<RichTextLabel>("Margin/Content/Story");
        _choices = GetNode<VBoxContainer>("Margin/Content/Choices");
        _choicesTitle = GetNode<Label>("Margin/Content/ChoicesTitle");
        _feedback = GetNode<Label>("Margin/Content/Feedback");
        _returnButton = GetNode<Button>("Margin/Content/Header/ReturnButton");
        _actionPreview = GetNode<VBoxContainer>("Margin/Content/ActionPreview");
        _previewTitle = GetNode<Label>("Margin/Content/ActionPreview/PreviewTitle");
        _previewDetails = GetNode<RichTextLabel>("Margin/Content/ActionPreview/PreviewDetails");
        _confirmButton = GetNode<Button>("Margin/Content/ActionPreview/Buttons/ConfirmButton");
        _cancelButton = GetNode<Button>("Margin/Content/ActionPreview/Buttons/CancelButton");
        _returnButton.Pressed += HandleReturnPressed;
        _confirmButton.Pressed += HandleActionPreviewConfirmed;
        _cancelButton.Pressed += HandleActionPreviewCancelled;
        _returnButton.Visible = false;
        UiTheme.StyleButton(_returnButton);
        UiTheme.StyleButton(_confirmButton, true);
        UiTheme.StyleButton(_cancelButton);
        _actionPreview.Visible = false;
    }

    public void ShowContent(string title, string subtitle, string text, IReadOnlyList<ActionDefinition> choices, string dayLabel)
    {
        _title.Text = title;
        _subtitle.Text = subtitle;
        _day.Text = dayLabel;
        _story.Text = text;
        _feedback.Text = "Choose an option to continue.";
        _returnButton.Visible = false;
        HideActionPreview();
        ClearChoices();

        foreach (ActionDefinition choice in choices)
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

    public void ShowActionPreview(ActionPreviewData preview)
    {
        if (preview == null)
        {
            throw new ArgumentNullException(nameof(preview));
        }

        _previewTitle.Text = preview.Title;
        _previewDetails.Text =
            "Current time: " + preview.CurrentTime
            + "\nDuration: " + preview.Duration
            + "\nEnergy cost: " + preview.EnergyCost
            + "\nExpected time: " + preview.ExpectedTime
            + "\n\nEffects:\n" + preview.Effects;

        if (!preview.CanConfirm)
        {
            _previewDetails.Text += "\n\nUnavailable: " + preview.RejectionReason;
        }

        _choices.Visible = false;
        _choicesTitle.Visible = false;
        _actionPreview.Visible = true;
        _confirmButton.Disabled = !preview.CanConfirm;
    }

    public void HideActionPreview()
    {
        if (_actionPreview == null || _choices == null)
        {
            return;
        }

        _actionPreview.Visible = false;
        _choices.Visible = true;
        _choicesTitle.Visible = true;
        _confirmButton.Disabled = false;
    }

    public void ShowFeedback(string message)
    {
        _feedback.Text = message;
    }

    public void ShowLocationButton(bool visible)
    {
        _returnButton.Visible = visible;
    }

    private void HandleChoiceSelected(string choiceId)
    {
        ChoiceSelected?.Invoke(choiceId);
    }

    private void HandleActionPreviewConfirmed()
    {
        ActionPreviewConfirmed?.Invoke();
    }

    private void HandleActionPreviewCancelled()
    {
        HideActionPreview();
        ActionPreviewCancelled?.Invoke();
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
