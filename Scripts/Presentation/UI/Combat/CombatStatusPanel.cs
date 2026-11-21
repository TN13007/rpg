using System;
using Godot;

/// <summary>Presents combat status information and exposes its UI interactions.</summary>
public partial class CombatStatusPanel : PanelContainer
{
    private Label _round;
    private Label _turn;
    private TimelineTrack _timelineTrack;
    private PanelContainer _intentCard;
    private Label _intentHeading;
    private Label _intentDescription;
    private Label _intentConfidence;

    /// <summary>Initializes the Godot control and connects its UI signals.</summary>
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

    /// <summary>Sets turn.</summary>
    /// <param name="round">The round used by this operation.</param>
    /// <param name="turnLabel">The turn label used by this operation.</param>
    public void SetTurn(int round, string turnLabel)
    {
        _round.Text = "ROUND " + round.ToString("00");
        _turn.Text = turnLabel.ToUpperInvariant();
    }

    /// <summary>Sets timeline.</summary>
    /// <param name="entries">The entries used by this operation.</param>
    public void SetTimeline(TimelineMarkerViewData[] entries)
    {
        _timelineTrack.SetCombatants(entries);
    }

    /// <summary>Sets intent.</summary>
    /// <param name="intent">The intent used by this operation.</param>
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
