using System;
using Godot;

public partial class CombatActionPanel : PanelContainer
{
    public event Action<CombatActionRequest> ActionSelected;

    private GridContainer _actionGrid;
    private VBoxContainer _targetSelection;
    private Label _targetHeading;
    private GridContainer _targetGrid;
    private Button _backButton;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _actionGrid = GetNode<GridContainer>("Margin/Content/ActionGrid");
        _targetSelection = GetNode<VBoxContainer>("Margin/Content/TargetSelection");
        _targetHeading = GetNode<Label>("Margin/Content/TargetSelection/TargetHeading");
        _targetGrid = GetNode<GridContainer>("Margin/Content/TargetSelection/TargetGrid");
        _backButton = GetNode<Button>("Margin/Content/TargetSelection/BackButton");
        _backButton.Pressed += ShowActions;
        UiTheme.StyleButton(_backButton);
    }

    public void SetActions(CombatActionOption[] actions)
    {
        ClearChildren(_actionGrid);
        ClearChildren(_targetGrid);
        _actionGrid.Visible = true;
        _targetSelection.Visible = false;

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
        string cost = string.IsNullOrWhiteSpace(action.CostLabel)
            ? string.Empty
            : "  ·  " + action.CostLabel;
        string detail = string.IsNullOrWhiteSpace(action.Description)
            ? string.Empty
            : "\n" + action.Description;
        string unavailableReason = action.IsAvailable || string.IsNullOrWhiteSpace(action.UnavailableReason)
            ? string.Empty
            : "\n" + action.UnavailableReason;

        Button button = new Button
        {
            Text = action.Label + cost + detail + unavailableReason,
            CustomMinimumSize = new Vector2(0, 62),
            Disabled = !action.IsAvailable,
            Alignment = HorizontalAlignment.Left,
            FocusMode = FocusModeEnum.All
        };
        UiTheme.StyleButton(button);
        button.Pressed += () => SelectAction(action);
        return button;
    }

    private void SelectAction(CombatActionOption action)
    {
        if (action.Targets.Length == 0)
        {
            ActionSelected?.Invoke(new CombatActionRequest(
                action.Id,
                string.Empty,
                action.ActorId));
            return;
        }

        ClearChildren(_targetGrid);
        _targetHeading.Text = "Choose a target for " + action.Label;
        foreach (CombatActionTargetOption target in action.Targets)
        {
            CombatActionButton button = new CombatActionButton
            {
                Request = new CombatActionRequest(action.Id, target.Id, action.ActorId),
                Text = target.Name,
                CustomMinimumSize = new Vector2(0, 42),
                Disabled = !action.IsAvailable,
                Alignment = HorizontalAlignment.Left,
                FocusMode = FocusModeEnum.All
            };
            UiTheme.StyleButton(button);
            button.ActionSelected += HandleActionSelected;
            _targetGrid.AddChild(button);
        }

        _actionGrid.Visible = false;
        _targetSelection.Visible = true;
    }

    private void HandleActionSelected(CombatActionRequest request)
    {
        ActionSelected?.Invoke(request);
    }

    private void ShowActions()
    {
        _targetSelection.Visible = false;
        _actionGrid.Visible = true;
    }

    private static void ClearChildren(Node container)
    {
        foreach (Node child in container.GetChildren())
        {
            container.RemoveChild(child);
            child.QueueFree();
        }
    }
}
