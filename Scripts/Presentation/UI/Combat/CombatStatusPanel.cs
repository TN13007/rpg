using Godot;

public partial class CombatStatusPanel : PanelContainer
{
    private Label _round;
    private Label _turn;
    private VBoxContainer _timelineList;
    private PanelContainer _intentCard;
    private Label _intentHeading;
    private Label _intentDescription;
    private Label _intentConfidence;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _round = GetNode<Label>("Margin/Content/Heading/Round");
        _turn = GetNode<Label>("Margin/Content/Heading/Turn");
        _timelineList = GetNode<VBoxContainer>("Margin/Content/TimelineList");
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

    public void SetTimeline(TimelineCombatantStatus[] entries)
    {
        ClearChildren(_timelineList);
        foreach (TimelineCombatantStatus entry in entries)
        {
            string state = entry.IsStunned ? "STUNNED" :
                entry.IsReady ? "READY" : "IN " + (entry.RemainingProgress / entry.Speed).ToString("0.0") + " CT";
            string side = entry.IsEnemy ? "ENEMY" : "PARTY";
            Label label = new Label
            {
                Text = side + " · " + entry.Name + " · " + state + " · " + entry.RemainingProgress.ToString("0") + " P",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            label.AddThemeFontSizeOverride("font_size", 11);
            label.AddThemeColorOverride("font_color", entry.IsReady ? UiTheme.Accent : UiTheme.Text);
            _timelineList.AddChild(label);
        }
    }

    public void SetIntent(IntentObservation intent)
    {
        if (intent == null) { ClearIntent(); return; }
        _intentCard.Visible = true;
        _intentHeading.Text = intent.EnemyName;
        _intentDescription.Text = intent.Description;
        _intentConfidence.Text = intent.Confidence == IntentConfidence.Recognized ? "RECOGNIZED" : "SUSPECTED";
    }

    private void ClearIntent()
    {
        _intentCard.Visible = false;
        _intentHeading.Text = string.Empty;
        _intentDescription.Text = string.Empty;
        _intentConfidence.Text = string.Empty;
    }

    private void ClearChildren(Node parent)
    {
        foreach (Node child in parent.GetChildren())
        {
            parent.RemoveChild(child);
            child.QueueFree();
        }
    }
}
