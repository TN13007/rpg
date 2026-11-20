using System;
using Godot;

public partial class CombatStatusPanel : PanelContainer
{
    private Label _round;
    private Label _turn;
    private TimelineTrack _timelineTrack;
    private PanelContainer _intentCard;
    private Label _intentHeading;
    private Label _intentDescription;
    private Label _intentConfidence;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);

        _round = GetNode<Label>("Margin/Content/Heading/Round");
        _turn = GetNode<Label>("Margin/Content/Heading/Turn");
        _timelineTrack = GetNode<TimelineTrack>("Margin/Content/TimelineTrack");
        _intentCard = GetNode<PanelContainer>("Margin/Content/IntentCard");
        _intentHeading = GetNode<Label>("Margin/Content/IntentCard/Margin/Content/Heading");
        _intentDescription = GetNode<Label>("Margin/Content/IntentCard/Margin/Content/Description");
        _intentConfidence = GetNode<Label>("Margin/Content/IntentCard/Margin/Content/Confidence");

        _intentCard.Visible = false;
    }

    public void SetTurn(int round, string turnLabel)
    {
        _round.Text = "ROUND " + round.ToString("00");
        _turn.Text = turnLabel.ToUpperInvariant();
    }

    public void SetTimeline(TimelineMarkerViewData[] entries)
    {
        _timelineTrack.SetCombatants(entries);
    }

    public void SetIntent(IntentObservation intent)
    {
        if (intent == null)
        {
            ClearIntent();
            return;
        }

        _intentCard.Visible = true;
        _intentHeading.Text = intent.EnemyName;
        _intentDescription.Text = intent.Description;
        _intentConfidence.Text = intent.Confidence == IntentConfidence.Recognized
            ? "RECOGNIZED"
            : "SUSPECTED";
    }

    private void ClearIntent()
    {
        _intentCard.Visible = false;
        _intentHeading.Text = string.Empty;
        _intentDescription.Text = string.Empty;
        _intentConfidence.Text = string.Empty;
    }
}
