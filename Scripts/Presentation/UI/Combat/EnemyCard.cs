using System;
using Godot;

/// <summary>Presents one Enemy from Presentation-owned view data and exposes target-selection events.</summary>
public partial class EnemyCard : Button
{
    /// <summary>Raised when the player selects this Enemy during target selection.</summary>
    public event Action<string> Selected;

    private Label _nameLabel;
    private Label _healthLabel;
    private ProgressBar _healthBar;
    private Label _conditionLabel;
    private Label _arousalLabel;
    private ProgressBar _arousalBar;
    private CombatCardVfx _vfx;
    private string _enemyId = string.Empty;
    private bool _selectable;
    private bool _selected;
    private bool _targetSelectionActive;

    /// <summary>Gets the displayed enemy name.</summary>
    public string EnemyName => _nameLabel?.Text ?? string.Empty;

    /// <summary>Initializes child controls and connects the pressed signal.</summary>
    public override void _Ready()
    {
        _nameLabel = GetNode<Label>("Margin/Content/Header/Name");
        _healthLabel = GetNode<Label>("Margin/Content/HealthLabel");
        _healthBar = GetNode<ProgressBar>("Margin/Content/HealthBar");
        _conditionLabel = GetNode<Label>("Margin/Content/Condition");
        _arousalLabel = GetNode<Label>("Margin/Content/ArousalLabel");
        _arousalBar = GetNode<ProgressBar>("Margin/Content/ArousalBar");
        _vfx = GetNode<CombatCardVfx>("Vfx");
        _arousalLabel.Visible = false;
        _arousalBar.Visible = false;

        Pressed += HandlePressed;
        FocusMode = FocusModeEnum.All;
        UpdateVisualState();
    }

    /// <summary>Initializes this card with Presentation-owned data for one Enemy.</summary>
    public void Setup(EnemyCardViewData enemy)
    {
        if (enemy == null)
        {
            throw new ArgumentNullException(nameof(enemy));
        }

        _enemyId = enemy.Id;
        UpdateDisplay(enemy);
    }

    /// <summary>Updates visible Enemy values without accessing Domain objects or combat services.</summary>
    public void UpdateDisplay(EnemyCardViewData enemy)
    {
        if (enemy == null)
        {
            throw new ArgumentNullException(nameof(enemy));
        }

        if (!string.IsNullOrEmpty(_enemyId) && _enemyId != enemy.Id)
        {
            throw new InvalidOperationException("An EnemyCard cannot be reused for a different enemy.");
        }

        _enemyId = enemy.Id;
        _nameLabel.Text = enemy.Name;
        _healthLabel.Text = "HP  " + enemy.CurrentHealth + " / " + enemy.MaxHealth;
        _healthBar.MaxValue = enemy.MaxHealth;
        _healthBar.Value = enemy.CurrentHealth;
        _conditionLabel.Text = enemy.Condition;
        _arousalLabel.Visible = enemy.Arousal.HasValue;
        _arousalBar.Visible = enemy.Arousal.HasValue;
        if (enemy.Arousal.HasValue)
        {
            _arousalLabel.Text = "AROUSAL  " + enemy.Arousal.Value + " / 100";
            _arousalBar.MaxValue = 100;
            _arousalBar.Value = enemy.Arousal.Value;
        }

        if (string.Equals(enemy.Condition, "Defeated", StringComparison.OrdinalIgnoreCase))
        {
            _selectable = false;
            _selected = false;
        }

        UpdateVisualState();
    }

    /// <summary>Sets whether this Enemy is a valid target and whether it is currently selected.</summary>
    public void SetTargetSelectionState(bool selectionActive, bool selectable, bool selected)
    {
        _targetSelectionActive = selectionActive;
        _selectable = selectionActive && selectable;
        _selected = _selectable && selected;
        UpdateVisualState();
    }

    /// <summary>Plays the requested combat feedback cue.</summary>
    public void PlayFeedback(CombatFeedbackType feedbackType)
    {
        _vfx.PlayFeedback(feedbackType);
    }

    private void HandlePressed()
    {
        if (!_selectable || string.IsNullOrWhiteSpace(_enemyId))
        {
            return;
        }

        _selected = true;
        UpdateVisualState();
        _vfx.PlayTargetSelected();
        Selected?.Invoke(_enemyId);
    }

    private void UpdateVisualState()
    {
        Color background = _selected
            ? new Color("#514735")
            : _targetSelectionActive && !_selectable
                ? new Color("#242a33")
                : UiTheme.PanelLight;
        Color border = _selected
            ? UiTheme.Accent
            : _selectable
                ? new Color("#8c7955")
                : new Color("#3a424e");
        int borderWidth = _selected || _selectable ? 2 : 1;

        AddThemeStyleboxOverride("normal", CreateStyle(background, border, borderWidth));
        AddThemeStyleboxOverride("hover", CreateStyle(
            _selectable ? new Color("#3b4552") : background,
            border,
            borderWidth));
        AddThemeStyleboxOverride("pressed", CreateStyle(
            _selectable ? new Color("#514735") : background,
            border,
            borderWidth));
        AddThemeStyleboxOverride("focus", CreateStyle(background, UiTheme.Accent, 2));

        Modulate = _targetSelectionActive && !_selectable
            ? new Color(0.72f, 0.72f, 0.72f, 1.0f)
            : Colors.White;
        MouseDefaultCursorShape = _selectable ? CursorShape.PointingHand : CursorShape.Arrow;
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
