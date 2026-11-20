using Godot;

public partial class CombatStatusPanel : PanelContainer
{
    private Label _round;
    private Label _turn;
    private HBoxContainer _enemyList;
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
        _enemyList = GetNode<HBoxContainer>("Margin/Content/EnemyList");
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

    public void SetEnemies(EnemyCombatStatus[] enemies)
    {
        ClearChildren(_enemyList);
        foreach (EnemyCombatStatus enemy in enemies) _enemyList.AddChild(CreateEnemyCard(enemy));
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

    private Control CreateEnemyCard(EnemyCombatStatus enemy)
    {
        PanelContainer card = new PanelContainer();
        card.CustomMinimumSize = new Vector2(205, 0);
        StyleBoxFlat style = new StyleBoxFlat
        {
            BgColor = new Color("#2b333e"),
            CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6
        };
        style.ContentMarginLeft = 10;
        style.ContentMarginRight = 10;
        style.ContentMarginTop = 8;
        style.ContentMarginBottom = 8;
        card.AddThemeStyleboxOverride("panel", style);
        VBoxContainer content = new VBoxContainer();
        content.AddThemeConstantOverride("separation", 5);
        card.AddChild(content);
        HBoxContainer heading = new HBoxContainer();
        content.AddChild(heading);
        Label name = new Label { Text = enemy.Name, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        name.AddThemeFontSizeOverride("font_size", 13);
        name.AddThemeColorOverride("font_color", UiTheme.Text);
        heading.AddChild(name);
        Label health = new Label { Text = enemy.CurrentHealth + " / " + enemy.MaxHealth };
        health.AddThemeFontSizeOverride("font_size", 11);
        health.AddThemeColorOverride("font_color", UiTheme.Muted);
        heading.AddChild(health);
        ProgressBar healthBar = new ProgressBar
        {
            CustomMinimumSize = new Vector2(0, 8), MaxValue = enemy.MaxHealth,
            Value = enemy.CurrentHealth, ShowPercentage = false
        };
        content.AddChild(healthBar);
        Label condition = new Label { Text = enemy.Condition };
        condition.AddThemeFontSizeOverride("font_size", 10);
        condition.AddThemeColorOverride("font_color", UiTheme.Muted);
        content.AddChild(condition);
        return card;
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
