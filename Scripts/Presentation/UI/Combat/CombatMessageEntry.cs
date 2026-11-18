using Godot;

public partial class CombatMessageEntry : HBoxContainer
{
    private PanelContainer _bubble;
    private Label _heading;
    private Label _body;

    public override void _Ready()
    {
        _bubble = GetNode<PanelContainer>("Bubble");
        _heading = GetNode<Label>("Bubble/Margin/Content/Heading");
        _body = GetNode<Label>("Bubble/Margin/Content/Body");
    }

    public void SetMessage(CombatMessage message)
    {
        string category = message.Category == CombatMessageCategory.None
            ? string.Empty
            : " · " + message.Category.ToString().ToUpperInvariant();

        _heading.Text = message.Speaker.ToUpperInvariant() + category;
        _body.Text = message.Text;
        _body.AutowrapMode = TextServer.AutowrapMode.WordSmart;

        Color background = new Color("#303740");
        Color headingColor = UiTheme.Muted;

        switch (message.Type)
        {
            case CombatMessageType.Player:
                Alignment = BoxContainer.AlignmentMode.End;
                background = new Color("#34495e");
                headingColor = new Color("#c5d9eb");
                break;
            case CombatMessageType.Enemy:
                Alignment = BoxContainer.AlignmentMode.Begin;
                background = new Color("#34383d");
                headingColor = new Color("#d7b08b");
                break;
            case CombatMessageType.System:
                Alignment = BoxContainer.AlignmentMode.Center;
                background = new Color("#2c343d");
                headingColor = UiTheme.Accent;
                _body.HorizontalAlignment = HorizontalAlignment.Center;
                break;
        }

        StyleBoxFlat style = new StyleBoxFlat
        {
            BgColor = background,
            CornerRadiusTopLeft = 9,
            CornerRadiusTopRight = 9,
            CornerRadiusBottomLeft = 9,
            CornerRadiusBottomRight = 9
        };
        _bubble.AddThemeStyleboxOverride("panel", style);
        _heading.AddThemeColorOverride("font_color", headingColor);
    }
}
