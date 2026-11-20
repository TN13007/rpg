using Godot;

public partial class PartyRosterPanel : PanelContainer
{
    private HBoxContainer _roster;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _roster = GetNode<HBoxContainer>("Margin/Content/RosterScroll/Roster");
    }

    public void SetMembers(PartyCombatStatus[] members)
    {
        ClearChildren();

        if (members == null || members.Length == 0)
        {
            Label emptyLabel = new Label
            {
                Text = "No party members",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            emptyLabel.AddThemeFontSizeOverride("font_size", 10);
            emptyLabel.AddThemeColorOverride("font_color", UiTheme.Muted);
            _roster.AddChild(emptyLabel);
            return;
        }

        foreach (PartyCombatStatus member in members)
        {
            _roster.AddChild(CreateMemberCard(member));
        }
    }

    private Control CreateMemberCard(PartyCombatStatus member)
    {
        PanelContainer card = new PanelContainer
        {
            CustomMinimumSize = new Vector2(125, 0),
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin
        };

        StyleBoxFlat cardStyle = new StyleBoxFlat
        {
            BgColor = member.IsActive ? new Color("#514735") : UiTheme.PanelLight,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6
        };
        cardStyle.ContentMarginLeft = 8;
        cardStyle.ContentMarginRight = 8;
        cardStyle.ContentMarginTop = 7;
        cardStyle.ContentMarginBottom = 7;
        card.AddThemeStyleboxOverride("panel", cardStyle);

        VBoxContainer content = new VBoxContainer();
        content.AddThemeConstantOverride("separation", 4);
        card.AddChild(content);

        Label name = new Label
        {
            Text = member.Name + (member.IsActive ? "  ·  READY" : string.Empty),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            ClipText = true
        };
        name.AddThemeFontSizeOverride("font_size", 11);
        name.AddThemeColorOverride("font_color", member.IsActive ? UiTheme.Accent : UiTheme.Text);
        content.AddChild(name);

        Label health = new Label
        {
            Text = "HP  " + member.CurrentHealth + " / " + member.MaxHealth,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        health.AddThemeFontSizeOverride("font_size", 10);
        health.AddThemeColorOverride("font_color", UiTheme.Muted);
        content.AddChild(health);

        ProgressBar healthBar = new ProgressBar
        {
            CustomMinimumSize = new Vector2(0, 7),
            MaxValue = member.MaxHealth,
            Value = member.CurrentHealth,
            ShowPercentage = false,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        content.AddChild(healthBar);

        Label condition = new Label
        {
            Text = member.Condition,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        condition.AddThemeFontSizeOverride("font_size", 9);
        condition.AddThemeColorOverride("font_color", UiTheme.Muted);
        content.AddChild(condition);

        return card;
    }

    private void ClearChildren()
    {
        foreach (Node child in _roster.GetChildren())
        {
            _roster.RemoveChild(child);
            child.QueueFree();
        }
    }
}
