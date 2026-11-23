using Godot;

/// <summary>Provides shared colors and styling helpers for the game's Godot UI.</summary>
public static class UiTheme
{
    /// <summary>Gets the background.</summary>
    public static readonly Color Background = new("#171a20");
    /// <summary>Gets the panel.</summary>
    public static readonly Color Panel = new("#222731");
    /// <summary>Gets the panel light.</summary>
    public static readonly Color PanelLight = new("#2b323e");
    /// <summary>Gets the accent.</summary>
    public static readonly Color Accent = new("#c6a56b");
    /// <summary>Gets the text.</summary>
    public static readonly Color Text = new("#e8e2d6");
    /// <summary>Gets the muted.</summary>
    public static readonly Color Muted = new("#9ba3ad");

    /// <summary>Applies the shared panel styling to the supplied control.</summary>
    /// <param name="panel">The panel used by this operation.</param>
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

    /// <summary>Applies the shared button styling to the supplied control.</summary>
    /// <param name="button">The button used by this operation.</param>
    /// <param name="highlighted">The highlighted used by this operation.</param>
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
