using Godot;

/// <summary>Presents the Round, combat phase and shared Party Action Points.</summary>
public partial class CombatStatusPanel : PanelContainer
{
    private Label _round;
    private Label _turn;
    private Label _actionPoints;

    /// <summary>Initializes the status labels and panel style.</summary>
    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _round = GetNode<Label>("Margin/Content/Heading/Round");
        _turn = GetNode<Label>("Margin/Content/Heading/Turn");
        _actionPoints = GetNode<Label>("Margin/Content/Heading/ActionPoints");
    }

    /// <summary>Updates the current Round, phase label and shared AP display.</summary>
    public void SetTurn(int round, string turnLabel, bool isPlayerTurn, int currentActionPoints, int actionCapacity)
    {
        _round.Text = "ROUND " + round.ToString("00");
        _turn.Text = (turnLabel ?? string.Empty).ToUpperInvariant();
        _actionPoints.Text = isPlayerTurn
            ? "AP  " + currentActionPoints + " / " + actionCapacity
            : string.Empty;
    }
}