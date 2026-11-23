using System;
using Godot;

/// <summary>Displays one Party member's identity, HP, Lust and MP, and emits member selection.</summary>
public partial class PartyMemberCard : Button
{
    /// <summary>Raised when the player selects a living Party member.</summary>
    public event Action<string> Selected;

    private Label _nameLabel;
    private Label _healthLabel;
    private ProgressBar _healthBar;
    private Label _lustLabel;
    private ProgressBar _lustBar;
    private Label _mpLabel;
    private ProgressBar _mpBar;
    private string _memberId = string.Empty;
    private string _displayName = string.Empty;
    private CombatCardVfx _vfx;
    private bool _canAct;
    private bool _isDowned;
    private bool _isSelected;

    /// <summary>Gets the displayed member name.</summary>
    public string MemberName => _displayName;
    /// <summary>Gets the stable Party member identifier.</summary>
    public string MemberId => _memberId;
    /// <summary>Gets whether the member can perform an action now.</summary>
    public bool CanAct => _canAct;
    /// <summary>Gets whether the member is Downed.</summary>
    public bool IsDowned => _isDowned;

    /// <summary>Initializes child controls and selection styling.</summary>
    public override void _Ready()
    {
        _nameLabel = GetNode<Label>("Margin/Content/Name");
        _healthLabel = GetNode<Label>("Margin/Content/Health");
        _healthBar = GetNode<ProgressBar>("Margin/Content/HealthBar");
        _lustLabel = GetNode<Label>("Margin/Content/Lust");
        _lustBar = GetNode<ProgressBar>("Margin/Content/LustBar");
        _mpLabel = GetNode<Label>("Margin/Content/Mp");
        _mpBar = GetNode<ProgressBar>("Margin/Content/MpBar");
        _vfx = GetNode<CombatCardVfx>("Vfx");

        Pressed += HandlePressed;
        FocusMode = FocusModeEnum.All;
        UpdateStyle();
    }

    /// <summary>Updates this card from Presentation-owned Party view data.</summary>
    public void SetMember(PartyMemberCardViewData member)
    {
        if (member == null)
            return;

        if (!string.IsNullOrEmpty(_memberId) && _memberId != member.Id)
        {
            GD.PushError("A PartyMemberCard cannot be reused for a different party member.");
            return;
        }

        _memberId = member.Id;
        _displayName = member.Name;
        _canAct = member.CanAct;
        _isDowned = string.Equals(member.Condition, "Downed", StringComparison.OrdinalIgnoreCase);
        _isSelected = _isSelected && !_isDowned;
        Disabled = _isDowned || !_canAct;

        _nameLabel.Text = member.Name;
        _nameLabel.AddThemeColorOverride("font_color", _isDowned ? new Color("#E07A70") : UiTheme.Text);
        _healthLabel.Text = "HP  " + member.CurrentHealth + " / " + member.MaxHealth;
        _healthBar.MaxValue = member.MaxHealth;
        _healthBar.Value = member.CurrentHealth;

        _lustLabel.Visible = member.Lust.HasValue;
        _lustBar.Visible = member.Lust.HasValue;
        if (member.Lust.HasValue)
        {
            _lustLabel.Text = "LUST  " + member.Lust.Value + " / 100";
            _lustBar.MaxValue = 100;
            _lustBar.Value = member.Lust.Value;
        }

        _mpLabel.Visible = member.CurrentMp.HasValue && member.MaxMp.HasValue;
        _mpBar.Visible = _mpLabel.Visible;
        if (_mpLabel.Visible)
        {
            _mpLabel.Text = "MP  " + member.CurrentMp.Value + " / " + member.MaxMp.Value;
            _mpBar.MaxValue = member.MaxMp.Value;
            _mpBar.Value = member.CurrentMp.Value;
        }

        UpdateStyle();
    }

    /// <summary>Sets the selected state while allowing the current actor to remain focused during Enemy Turn.</summary>
    public void SetSelected(bool selected)
    {
        _isSelected = selected && !_isDowned;
        UpdateStyle();
    }

    /// <summary>Plays the requested combat feedback cue.</summary>
    public void PlayFeedback(CombatFeedbackType feedbackType)
    {
        _vfx.PlayFeedback(feedbackType);
    }

    private void HandlePressed()
    {
        if (_isDowned || !_canAct || string.IsNullOrWhiteSpace(_memberId))
            return;

        _vfx.PlayTargetSelected();
        Selected?.Invoke(_memberId);
    }

    private void UpdateStyle()
    {
        Color background = _isSelected
            ? new Color("#514735")
            : _canAct
                ? new Color("#373e47")
                : UiTheme.PanelLight;
        Color border = _isSelected
            ? UiTheme.Accent
            : _canAct
                ? new Color("#8c7955")
                : new Color("#3a424e");
        int width = _isSelected ? 2 : 1;

        AddThemeStyleboxOverride("normal", CreateStyle(background, border, width));
        AddThemeStyleboxOverride("hover", CreateStyle(
            _canAct && !_isDowned ? new Color("#46515e") : background,
            border,
            width));
        AddThemeStyleboxOverride("pressed", CreateStyle(
            _canAct && !_isDowned ? new Color("#514735") : background,
            border,
            width));
        AddThemeStyleboxOverride("disabled", CreateStyle(
            _isSelected ? background : UiTheme.PanelLight,
            _isSelected ? UiTheme.Accent : new Color("#3a424e"),
            _isSelected ? 2 : 1));
        AddThemeStyleboxOverride("focus", CreateStyle(background, UiTheme.Accent, 2));
        MouseDefaultCursorShape = _canAct && !_isDowned ? CursorShape.PointingHand : CursorShape.Arrow;
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
        style.ContentMarginLeft = 9;
        style.ContentMarginRight = 9;
        style.ContentMarginTop = 8;
        style.ContentMarginBottom = 8;
        return style;
    }
}
