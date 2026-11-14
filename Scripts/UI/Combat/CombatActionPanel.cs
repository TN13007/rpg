using System;
using Godot;

public partial class CombatActionPanel : PanelContainer
{
    public event Action<string> ActionSelected;

    private GridContainer _actionGrid;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _actionGrid = GetNode<GridContainer>("Margin/Content/ActionGrid");
    }

    public void SetActions(CombatActionOption[] actions)
    {
        ClearChildren();

        if (actions == null || actions.Length == 0)
        {
            Label emptyLabel = new Label { Text = "No actions available." };
            emptyLabel.AddThemeColorOverride("font_color", UiTheme.Muted);
            _actionGrid.AddChild(emptyLabel);
            return;
        }

        foreach (CombatActionOption action in actions)
        {
            _actionGrid.AddChild(CreateActionButton(action));
        }
    }

    private Button CreateActionButton(CombatActionOption action)
    {
        string cost = string.IsNullOrWhiteSpace(action.CostLabel) ? string.Empty : "  ·  " + action.CostLabel;
        string detail = string.IsNullOrWhiteSpace(action.Description) ? string.Empty : "\n" + action.Description;
        string unavailableReason = action.IsAvailable || string.IsNullOrWhiteSpace(action.UnavailableReason)
            ? string.Empty
            : "\n" + action.UnavailableReason;

        Button button = new Button
        {
            Text = action.Label + cost + detail + unavailableReason,
            TooltipText = action.UnavailableReason,
            CustomMinimumSize = new Vector2(0, 62),
            Disabled = !action.IsAvailable,
            Alignment = HorizontalAlignment.Left,
            FocusMode = FocusModeEnum.All
        };

        UiTheme.StyleButton(button);
        button.Pressed += () => ActionSelected?.Invoke(action.Id);
        return button;
    }

    private void ClearChildren()
    {
        foreach (Node child in _actionGrid.GetChildren())
        {
            _actionGrid.RemoveChild(child);
            child.QueueFree();
        }
    }
}
