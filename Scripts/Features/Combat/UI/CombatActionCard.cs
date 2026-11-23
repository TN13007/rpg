using System;
using Godot;

/// <summary>Presents one action option from Presentation-owned view data.</summary>
public partial class CombatActionCard : Button
{
    /// <summary>Raised when the player selects this action to inspect its details.</summary>
    public event Action<CombatActionCardViewData> Selected;

    private CombatActionCardViewData _action;
    private bool _isSelected;

    /// <summary>Gets the stable action ID represented by this card.</summary>
    public string ActionId => _action?.Id ?? string.Empty;

    public override void _Ready()
    {
        FocusMode = FocusModeEnum.All;
        Pressed += HandlePressed;
        UpdateStyle();
    }

    /// <summary>Supplies the values displayed by this reusable action card.</summary>
    public void SetAction(CombatActionCardViewData action)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
        string cost = string.IsNullOrWhiteSpace(action.CostLabel)
            ? string.Empty
            : "  ·  " + action.CostLabel;

        Text = action.Label + cost;
        Disabled = false;
        Modulate = action.IsAvailable ? Colors.White : new Color(0.72f, 0.72f, 0.72f, 1.0f);
        MouseDefaultCursorShape = CursorShape.PointingHand;
        UpdateStyle();
    }

    /// <summary>Highlights the action currently shown in the details area.</summary>
    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateStyle();
    }

    private void HandlePressed()
    {
        if (_action != null)
        {
            Selected?.Invoke(_action);
        }
    }

    private void UpdateStyle()
    {
        Color background = _isSelected
            ? new Color("#514735")
            : UiTheme.PanelLight;
        Color border = _isSelected
            ? UiTheme.Accent
            : new Color("#3a424e");
        int width = _isSelected ? 2 : 1;

        AddThemeStyleboxOverride("normal", CreateStyle(background, border, width));
        AddThemeStyleboxOverride("hover", CreateStyle(
            _isSelected ? background : new Color("#3b4552"),
            _isSelected ? border : new Color("#8c7955"),
            width));
        AddThemeStyleboxOverride("pressed", CreateStyle(new Color("#514735"), UiTheme.Accent, 2));
        AddThemeStyleboxOverride("focus", CreateStyle(background, UiTheme.Accent, 2));
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
        style.ContentMarginTop = 7;
        style.ContentMarginBottom = 7;
        return style;
    }
}
