using System;
using Godot;

/// <summary>Presents a Party member and allows selecting that member as the next acting character.</summary>
public partial class PartyMemberCard : Button
{
    /// <summary>Raised when an eligible Party member is selected.</summary>
    public event Action<string> Selected;

    private Label _nameLabel;
    private Label _healthLabel;
    private ProgressBar _healthBar;
    private Label _conditionLabel;
    private string _memberId = string.Empty;
    private string _displayName = string.Empty;
    private CombatCardVfx _vfx;
    private bool _isActive;
    private bool _isSelected;

    /// <summary>Gets the displayed member name.</summary>
    public string MemberName => _displayName;

    /// <summary>Gets the stable Party member identifier.</summary>
    public string MemberId => _memberId;

    /// <summary>Gets whether this member can act during the current Player Turn.</summary>
    public bool IsActive => _isActive;

    /// <summary>Initializes child controls and connects the pressed signal.</summary>
    public override void _Ready()
    {
        _nameLabel = GetNode<Label>("Margin/Content/Name");
        _healthLabel = GetNode<Label>("Margin/Content/Health");
        _healthBar = GetNode<ProgressBar>("Margin/Content/HealthBar");
        _conditionLabel = GetNode<Label>("Margin/Content/Condition");
        _vfx = GetNode<CombatCardVfx>("Vfx");

        Pressed += HandlePressed;
        FocusMode = FocusModeEnum.All;
        MouseDefaultCursorShape = CursorShape.PointingHand;
        UpdateStyle();
    }

    /// <summary>Updates the member status and whether the card can currently be selected.</summary>
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
        _isActive = member.IsActive;
        _isSelected = _isSelected && _isActive;
        Disabled = !_isActive;

        _nameLabel.Text = member.Name + (member.IsActive ? "  ·  READY" : string.Empty);
        _nameLabel.AddThemeColorOverride("font_color", member.IsActive ? UiTheme.Accent : UiTheme.Text);
        _healthLabel.Text = "HP  " + member.CurrentHealth + " / " + member.MaxHealth;
        _healthBar.MaxValue = member.MaxHealth;
        _healthBar.Value = member.CurrentHealth;
        _conditionLabel.Text = member.Condition;
        _conditionLabel.AddThemeColorOverride(
            "font_color",
            member.Condition == "Downed" ? new Color("#E07A70") : UiTheme.Muted);
        UpdateStyle();
    }

    /// <summary>Sets the selected state while preserving the member's current eligibility.</summary>
    public void SetSelected(bool selected)
    {
        _isSelected = selected && _isActive;
        UpdateStyle();
    }

    /// <summary>Plays the requested combat feedback cue.</summary>
    public void PlayFeedback(CombatFeedbackType feedbackType)
    {
        _vfx.PlayFeedback(feedbackType);
    }

    private void HandlePressed()
    {
        if (!_isActive || string.IsNullOrWhiteSpace(_memberId))
        {
            return;
        }

        _vfx.PlayTargetSelected();
        Selected?.Invoke(_memberId);
    }

    private void UpdateStyle()
    {
        Color background = _isSelected
            ? new Color("#514735")
            : _isActive
                ? new Color("#373e47")
                : UiTheme.PanelLight;
        Color border = _isSelected
            ? UiTheme.Accent
            : _isActive
                ? new Color("#8c7955")
                : new Color("#3a424e");
        int width = _isSelected ? 2 : 1;

        AddThemeStyleboxOverride("normal", CreateStyle(background, border, width));
        AddThemeStyleboxOverride("hover", CreateStyle(
            _isActive ? new Color("#46515e") : background,
            border,
            width));
        AddThemeStyleboxOverride("pressed", CreateStyle(
            _isActive ? new Color("#514735") : background,
            border,
            width));
        AddThemeStyleboxOverride("disabled", CreateStyle(UiTheme.PanelLight, new Color("#3a424e"), 1));
        AddThemeStyleboxOverride("focus", CreateStyle(background, UiTheme.Accent, 2));
        MouseDefaultCursorShape = _isActive ? CursorShape.PointingHand : CursorShape.Arrow;
    }

    private static StyleBoxFlat CreateStyle(Color background, Color border, int borderWidth)
    {
        StyleBoxFlat style = new StyleBoxFlat
        {
            BgColor = background,
            BorderColor = border,
            BorderWidthLeft = borderWidth,
            BorderWidthTop = borderWidth,
            BorderWidthRight = borderWidth,
            BorderWidthBottom = borderWidth,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6
        };

        style.ContentMarginLeft = 8;
        style.ContentMarginRight = 8;
        style.ContentMarginTop = 7;
        style.ContentMarginBottom = 7;
        return style;
    }
}