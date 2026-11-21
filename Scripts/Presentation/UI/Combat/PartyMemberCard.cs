using Godot;

public partial class PartyMemberCard : PanelContainer
{
    private Label _nameLabel;
    private Label _healthLabel;
    private ProgressBar _healthBar;
    private Label _conditionLabel;
    private string _memberId = string.Empty;
    private string _displayName = string.Empty;

    public string MemberName => _displayName;

    public override void _Ready()
    {
        _nameLabel = GetNode<Label>("Margin/Content/Name");
        _healthLabel = GetNode<Label>("Margin/Content/Health");
        _healthBar = GetNode<ProgressBar>("Margin/Content/HealthBar");
        _conditionLabel = GetNode<Label>("Margin/Content/Condition");
        UpdateStyle(false);
    }

    public void SetMember(PartyCombatStatus member)
    {
        if (member == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(_memberId) && _memberId != member.Id)
        {
            GD.PushError("A PartyMemberCard cannot be reused for a different party member.");
            return;
        }

        _memberId = member.Id;
        _displayName = member.Name;
        _nameLabel.Text = member.Name + (member.IsActive ? "  ·  READY" : string.Empty);
        _nameLabel.AddThemeColorOverride("font_color", member.IsActive ? UiTheme.Accent : UiTheme.Text);
        _healthLabel.Text = "HP  " + member.CurrentHealth + " / " + member.MaxHealth;
        _healthBar.MaxValue = member.MaxHealth;
        _healthBar.Value = member.CurrentHealth;
        _conditionLabel.Text = member.Condition;
        _conditionLabel.AddThemeColorOverride(
            "font_color",
            member.Condition == "Downed" ? new Color("#E07A70") : UiTheme.Muted);
        UpdateStyle(member.IsActive);
    }

    public void PlayFeedback(CombatFeedbackType feedbackType)
    {
        Color feedbackColor = feedbackType switch
        {
            CombatFeedbackType.Attack => new Color("#e7a45d"),
            CombatFeedbackType.Guard => new Color("#78a9d2"),
            CombatFeedbackType.Evade => new Color("#79c9d2"),
            CombatFeedbackType.Recover => new Color("#8bc78d"),
            CombatFeedbackType.Revive => new Color("#a5d68d"),
            CombatFeedbackType.Hit => new Color("#e07870"),
            _ => Colors.White
        };

        Modulate = feedbackColor;
        CreateTween()
            .TweenProperty(this, "modulate", Colors.White, 0.28)
            .SetDelay(0.12);
    }

    private void UpdateStyle(bool isActive)
    {
        StyleBoxFlat style = new StyleBoxFlat
        {
            BgColor = isActive ? new Color("#514735") : UiTheme.PanelLight,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6
        };
        AddThemeStyleboxOverride("panel", style);
    }
}
