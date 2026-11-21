using Godot;

/// <summary>Presents a combatant card and its current selection and feedback state.</summary>
public partial class PartyMemberCard : PanelContainer
{
    private Label _nameLabel;
    private Label _healthLabel;
    private ProgressBar _healthBar;
    private Label _conditionLabel;
    private string _memberId = string.Empty;
    private string _displayName = string.Empty;
    private CombatCardVfx _vfx;

    /// <summary>Gets the member name.</summary>
    public string MemberName => _displayName;

    /// <summary>Initializes the Godot control and connects its UI signals.</summary>
    public override void _Ready()
    {
        _nameLabel = GetNode<Label>("Margin/Content/Name");
        _healthLabel = GetNode<Label>("Margin/Content/Health");
        _healthBar = GetNode<ProgressBar>("Margin/Content/HealthBar");
        _conditionLabel = GetNode<Label>("Margin/Content/Condition");
        _vfx = GetNode<CombatCardVfx>("Vfx");
        UpdateStyle(false);
    }

    /// <summary>Sets member.</summary>
    /// <param name="member">The member used by this operation.</param>
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

    /// <summary>Plays feedback feedback for the selected combatant or action.</summary>
    /// <param name="feedbackType">The feedback type used by this operation.</param>
    public void PlayFeedback(CombatFeedbackType feedbackType)
    {
        _vfx.PlayFeedback(feedbackType);
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
