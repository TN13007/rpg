using System;
using Godot;

/// <summary>Displays one Enemy's identity and the two combat meters used by the interface.</summary>
public partial class EnemyCard : Button
{
    /// <summary>Raised when the player selects this valid Enemy target.</summary>
    public event Action<string> Selected;

    private Label _nameLabel;
    private Label _healthLabel;
    private ProgressBar _healthBar;
    private Label _lustLabel;
    private ProgressBar _lustBar;
    private CombatCardVfx _vfx;
    private string _enemyId = string.Empty;
    private bool _selectable;
    private bool _selected;
    private bool _targetSelectionActive;
    private bool _defeated;

    /// <summary>Gets the displayed Enemy name.</summary>
    public string EnemyName => _nameLabel?.Text ?? string.Empty;

    /// <summary>Initializes controls and wires the target-selection interaction.</summary>
    public override void _Ready()
    {
        _nameLabel = GetNode<Label>("Margin/Content/Name");
        _healthLabel = GetNode<Label>("Margin/Content/HealthLabel");
        _healthBar = GetNode<ProgressBar>("Margin/Content/HealthBar");
        _lustLabel = GetNode<Label>("Margin/Content/LustLabel");
        _lustBar = GetNode<ProgressBar>("Margin/Content/LustBar");
        _vfx = GetNode<CombatCardVfx>("Vfx");
        _lustLabel.Visible = false;
        _lustBar.Visible = false;
        Pressed += HandlePressed;
        FocusMode = FocusModeEnum.All;
        UpdateVisualState();
    }

    /// <summary>Initializes this reusable card with one Enemy's presentation data.</summary>
    public void Setup(EnemyCardViewData enemy)
    {
        if (enemy == null)
            throw new ArgumentNullException(nameof(enemy));

        _enemyId = enemy.Id;
        UpdateDisplay(enemy);
    }

    /// <summary>Updates the Enemy's name and HP/Lust values without accessing gameplay objects.</summary>
    public void UpdateDisplay(EnemyCardViewData enemy)
    {
        if (enemy == null)
            throw new ArgumentNullException(nameof(enemy));

        if (!string.IsNullOrEmpty(_enemyId) && _enemyId != enemy.Id)
            throw new InvalidOperationException("An EnemyCard cannot be reused for a different enemy.");

        _enemyId = enemy.Id;
        _nameLabel.Text = enemy.Name;
        _healthLabel.Text = "HP  " + enemy.CurrentHealth + " / " + enemy.MaxHealth;
        _healthBar.MaxValue = enemy.MaxHealth;
        _healthBar.Value = enemy.CurrentHealth;

        _defeated = string.Equals(enemy.Condition, "Defeated", StringComparison.OrdinalIgnoreCase);
        _lustLabel.Visible = enemy.Lust.HasValue;
        _lustBar.Visible = enemy.Lust.HasValue;
        if (enemy.Lust.HasValue)
        {
            _lustLabel.Text = "LUST  " + enemy.Lust.Value + " / 100";
            _lustBar.MaxValue = 100;
            _lustBar.Value = enemy.Lust.Value;
        }

        if (_defeated)
        {
            _selectable = false;
            _selected = false;
        }

        UpdateVisualState();
    }

    /// <summary>Sets the valid, invalid and selected target states supplied by the owning region.</summary>
    public void SetTargetSelectionState(bool selectionActive, bool selectable, bool selected)
    {
        _targetSelectionActive = selectionActive;
        _selectable = selectionActive && selectable && !_defeated;
        _selected = _selectable && selected;
        UpdateVisualState();
    }

    /// <summary>Plays one presentation feedback cue.</summary>
    public void PlayFeedback(CombatFeedbackType feedbackType)
    {
        _vfx.PlayFeedback(feedbackType);
    }

    private void HandlePressed()
    {
        if (!_selectable || string.IsNullOrWhiteSpace(_enemyId))
            return;

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
            ? new Color(0.58f, 0.58f, 0.63f, 0.72f)
            : _defeated
                ? new Color(0.62f, 0.62f, 0.66f, 0.78f)
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
        style.ContentMarginLeft = 9;
        style.ContentMarginRight = 9;
        style.ContentMarginTop = 9;
        style.ContentMarginBottom = 9;
        return style;
    }
}
