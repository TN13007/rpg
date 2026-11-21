using System;
using Godot;

public partial class EnemyCard : Button
{
    public event Action<string> Selected;

    private Label _nameLabel;
    private Label _healthLabel;
    private ProgressBar _healthBar;
    private Label _conditionLabel;
    private Label _intentLabel;

    private string _enemyId = string.Empty;
    private bool _selectable;
    private bool _selected;
    private bool _targetSelectionActive;

    public string EnemyName => _nameLabel?.Text ?? string.Empty;

    public override void _Ready()
    {
        _nameLabel = GetNode<Label>("Margin/Content/Header/Name");
        _healthLabel = GetNode<Label>("Margin/Content/HealthLabel");
        _healthBar = GetNode<ProgressBar>("Margin/Content/HealthBar");
        _conditionLabel = GetNode<Label>("Margin/Content/Condition");
        _intentLabel = GetNode<Label>("Margin/Content/Intent");

        Pressed += HandlePressed;
        FocusMode = FocusModeEnum.All;
        MouseDefaultCursorShape = CursorShape.PointingHand;
        UpdateVisualState();
    }

    public void Setup(EnemyCombatStatus enemy)
    {
        if (enemy == null)
        {
            throw new ArgumentNullException(nameof(enemy));
        }

        _enemyId = enemy.Id;
        UpdateDisplay(enemy);
    }

    public void UpdateDisplay(EnemyCombatStatus enemy)
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

        bool hasIntent = !string.IsNullOrWhiteSpace(enemy.IntentDescription);
        _intentLabel.Visible = hasIntent;
        _intentLabel.Text = hasIntent ? "Intent: " + enemy.IntentDescription : string.Empty;

        if (string.Equals(enemy.Condition, "Defeated", StringComparison.OrdinalIgnoreCase))
        {
            _selectable = false;
            _selected = false;
        }

        UpdateVisualState();
    }

    public void SetTargetSelectionState(bool selectionActive, bool selectable, bool selected)
    {
        _targetSelectionActive = selectionActive;
        _selectable = selectionActive && selectable;
        _selected = _selectable && selected;
        UpdateVisualState();
    }

    public void PlayFeedback(CombatFeedbackType feedbackType)
    {
        Color feedbackColor = feedbackType switch
        {
            CombatFeedbackType.Hit => new Color("#e07870"),
            CombatFeedbackType.Attack => new Color("#e7a45d"),
            _ => Colors.White
        };

        Modulate = feedbackColor;
        CreateTween()
            .TweenProperty(this, "modulate", Colors.White, 0.28)
            .SetDelay(0.12);
    }

    private void HandlePressed()
    {
        if (!_selectable || string.IsNullOrWhiteSpace(_enemyId))
        {
            return;
        }

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
