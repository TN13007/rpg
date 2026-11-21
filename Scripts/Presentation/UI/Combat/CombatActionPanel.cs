using System;
using System.Collections.Generic;
using Godot;

public partial class CombatActionPanel : PanelContainer
{
    public event Action<CombatActionRequest> ActionSelected;
    public event Action<string[]> EnemyTargetSelectionRequested;
    public event Action EnemyTargetSelectionCancelled;

    private GridContainer _actionGrid;
    private VBoxContainer _targetSelection;
    private Label _targetHeading;
    private GridContainer _targetGrid;
    private Button _backButton;
    private Button _confirmButton;
    private CombatActionOption _pendingAction;
    private readonly HashSet<string> _validEnemyTargetIds =
        new HashSet<string>(StringComparer.Ordinal);
    private string _selectedTargetId = string.Empty;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _actionGrid = GetNode<GridContainer>("Margin/Content/ActionGrid");
        _targetSelection = GetNode<VBoxContainer>("Margin/Content/TargetSelection");
        _targetHeading = GetNode<Label>("Margin/Content/TargetSelection/TargetHeading");
        _targetGrid = GetNode<GridContainer>("Margin/Content/TargetSelection/TargetGrid");
        _backButton = GetNode<Button>("Margin/Content/TargetSelection/BackButton");
        _confirmButton = GetNode<Button>("Margin/Content/TargetSelection/ConfirmButton");
        _backButton.Pressed += CancelTargetSelection;
        _confirmButton.Pressed += ConfirmTargetSelection;
        UiTheme.StyleButton(_backButton);
        UiTheme.StyleButton(_confirmButton, true);
        _confirmButton.Disabled = true;
        _targetGrid.Visible = false;
    }

    public void SetActions(CombatActionOption[] actions)
    {
        ClearChildren(_actionGrid);
        ClearChildren(_targetGrid);
        ClearPendingTarget();
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

    public bool SetSelectedEnemyTarget(string enemyId)
    {
        if (_pendingAction == null ||
            string.IsNullOrWhiteSpace(enemyId) ||
            !_validEnemyTargetIds.Contains(enemyId))
        {
            return false;
        }

        _selectedTargetId = enemyId;
        _targetHeading.Text = "Target selected. Confirm " + _pendingAction.Label + ".";
        _confirmButton.Disabled = false;
        return true;
    }

    public void ClearTargetSelection()
    {
        ClearPendingTarget();
        _targetSelection.Visible = false;
        _actionGrid.Visible = true;
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
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand
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

        // Revive targets are party members, not EnemyCards. Keep their dedicated
        // target list until party roster cards become selectable components.
        if (action.Id == "revive_ally")
        {
            ShowPartyTargetButtons(action);
            return;
        }

        _pendingAction = action;
        _selectedTargetId = string.Empty;
        _validEnemyTargetIds.Clear();
        foreach (CombatActionTargetOption target in action.Targets)
        {
            _validEnemyTargetIds.Add(target.Id);
        }

        _targetHeading.Text = "Choose a highlighted enemy from the sidebar.";
        _confirmButton.Disabled = true;
        _targetGrid.Visible = false;
        _actionGrid.Visible = false;
        _targetSelection.Visible = true;
        EnemyTargetSelectionRequested?.Invoke(ToArray(_validEnemyTargetIds));
    }

    private void ShowPartyTargetButtons(CombatActionOption action)
    {
        ClearChildren(_targetGrid);
        _targetHeading.Text = "Choose an ally to revive.";
        foreach (CombatActionTargetOption target in action.Targets)
        {
            CombatActionButton button = new CombatActionButton
            {
                Request = new CombatActionRequest(action.Id, target.Id, action.ActorId),
                Text = target.Name,
                CustomMinimumSize = new Vector2(0, 42),
                Disabled = !action.IsAvailable,
                Alignment = HorizontalAlignment.Left,
                FocusMode = FocusModeEnum.All,
                MouseDefaultCursorShape = CursorShape.PointingHand
            };
            UiTheme.StyleButton(button);
            button.ActionSelected += HandleActionSelected;
            _targetGrid.AddChild(button);
        }

        _pendingAction = null;
        _targetGrid.Visible = true;
        _confirmButton.Visible = false;
        _actionGrid.Visible = false;
        _targetSelection.Visible = true;
    }

    private void ConfirmTargetSelection()
    {
        if (_pendingAction == null || string.IsNullOrWhiteSpace(_selectedTargetId))
        {
            return;
        }

        CombatActionRequest request = new CombatActionRequest(
            _pendingAction.Id,
            _selectedTargetId,
            _pendingAction.ActorId);
        ClearPendingTarget();
        _targetSelection.Visible = false;
        _actionGrid.Visible = true;
        ActionSelected?.Invoke(request);
    }

    private void CancelTargetSelection()
    {
        bool wasSelectingEnemy = _pendingAction != null;
        ClearPendingTarget();
        _targetSelection.Visible = false;
        _actionGrid.Visible = true;
        _targetGrid.Visible = false;
        _confirmButton.Visible = true;
        if (wasSelectingEnemy)
        {
            EnemyTargetSelectionCancelled?.Invoke();
        }
    }

    private void HandleActionSelected(CombatActionRequest request)
    {
        _targetSelection.Visible = false;
        _actionGrid.Visible = true;
        _targetGrid.Visible = false;
        _confirmButton.Visible = true;
        ActionSelected?.Invoke(request);
    }

    private void ClearPendingTarget()
    {
        _pendingAction = null;
        _selectedTargetId = string.Empty;
        _validEnemyTargetIds.Clear();
        _confirmButton.Disabled = true;
        _confirmButton.Visible = true;
    }

    private static string[] ToArray(HashSet<string> values)
    {
        string[] result = new string[values.Count];
        values.CopyTo(result);
        return result;
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
