using Godot;

public partial class TimelineMarker : Panel
{
    private static readonly Color _ariColor = new Color("#55a7ff");
    private static readonly Color _partyColor = new Color("#e8bd55");
    private static readonly Color _enemyColor = new Color("#e45f68");
    private static readonly Color _borderColor = new Color("#20252d");
    private static readonly Color _stunnedColor = new Color("#b57be5");

    private TextureRect _icon;
    private Panel _statusIndicator;
    private StyleBoxFlat _markerStyle;
    private StyleBoxFlat _statusStyle;

    public override void _Ready()
    {
        _icon = GetNode<TextureRect>("Icon");
        _statusIndicator = GetNode<Panel>("StatusIndicator");

        _markerStyle = new StyleBoxFlat
        {
            BgColor = UiTheme.PanelLight,
            BorderColor = _borderColor,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8
        };
        AddThemeStyleboxOverride("panel", _markerStyle);

        _statusStyle = new StyleBoxFlat
        {
            BgColor = _stunnedColor,
            BorderColor = _borderColor,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 3,
            CornerRadiusTopRight = 3,
            CornerRadiusBottomLeft = 3,
            CornerRadiusBottomRight = 3
        };
        _statusIndicator.AddThemeStyleboxOverride("panel", _statusStyle);
    }

    public void SetCombatant(TimelineMarkerViewData combatant, double displayWindowSeconds)
    {
        if (combatant == null)
        {
            return;
        }

        _markerStyle.BgColor = GetMarkerColor(combatant);
        _statusIndicator.Visible = combatant.IsStunned;
        TooltipText = GetTooltip(combatant, displayWindowSeconds);
    }

    public void SetIcon(Texture2D icon)
    {
        _icon.Texture = icon;
        _icon.Visible = icon != null;
    }

    private static Color GetMarkerColor(TimelineMarkerViewData combatant)
    {
        if (combatant.IsEnemy)
        {
            return _enemyColor;
        }

        return combatant.IsPrimaryPartyMember ? _ariColor : _partyColor;
    }

    private static string GetTooltip(TimelineMarkerViewData combatant, double displayWindowSeconds)
    {
        string readyText = combatant.IsReady && !combatant.IsStunned
            ? "READY"
            : "READY IN " + combatant.TimeToReadySeconds.ToString("0.0") + "s";

        if (combatant.IsStunned)
        {
            readyText += " · STUNNED";
        }

        if (combatant.TimeToReadySeconds > displayWindowSeconds)
        {
            readyText += " · OUTSIDE DISPLAY WINDOW";
        }

        return combatant.Name + " · " + readyText;
    }
}
