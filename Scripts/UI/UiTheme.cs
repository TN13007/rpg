using Godot;

public static class UiTheme
{
    public static readonly Color Background = new("#171a20");
    public static readonly Color Panel = new("#222731");
    public static readonly Color PanelLight = new("#2b323e");
    public static readonly Color Accent = new("#c6a56b");
    public static readonly Color Text = new("#e8e2d6");
    public static readonly Color Muted = new("#9ba3ad");

    public static void StylePanel(PanelContainer panel)
    {
        var style = new StyleBoxFlat
        {
            BgColor = Panel,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8
        };

        panel.AddThemeStyleboxOverride("panel", style);
    }

    public static void StyleButton(Button button, bool highlighted = false)
    {
        Color normal = highlighted ? new Color("#514735") : PanelLight;
        button.AddThemeFontSizeOverride("font_size", 13);
        button.AddThemeColorOverride("font_color", Text);
        button.AddThemeStyleboxOverride("normal", ButtonBox(normal));
        button.AddThemeStyleboxOverride("hover", ButtonBox(new Color("#3b4552")));
        button.AddThemeStyleboxOverride("pressed", ButtonBox(new Color("#514735")));
    }

    private static StyleBoxFlat ButtonBox(Color color)
    {
        var style = new StyleBoxFlat
        {
            BgColor = color,
            CornerRadiusTopLeft = 5,
            CornerRadiusTopRight = 5,
            CornerRadiusBottomLeft = 5,
            CornerRadiusBottomRight = 5
        };

        style.ContentMarginLeft = 10;
        style.ContentMarginRight = 10;
        style.ContentMarginTop = 8;
        style.ContentMarginBottom = 8;
        return style;
    }
}
