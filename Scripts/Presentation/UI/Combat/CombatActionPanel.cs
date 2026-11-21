using System;
using System.Collections.Generic;
using Godot;

/// <summary>Presents combat action information and exposes its UI interactions.</summary>
public partial class CombatActionPanel : PanelContainer
{
    /// <summary>Raised when action selected occurs.</summary>
    public event Action<CombatActionRequest> ActionSelected;
    /// <summary>Raised when enemy target selection requested occurs.</summary>
    public event Action<string[]> EnemyTargetSelectionRequested;
    /// <summary>Raised when enemy target selection cancelled occurs.</summary>
    public event Action EnemyTargetSelectionCancelled;

    private GridContainer _actionGrid;
    private VBoxContainer _targetSelection;
    private Label _targetHeading;
    private GridContainer _targetGrid;
    private Button _backButton;
    private PackedScene _actionCardScene;
    private CombatActionOption _pendingAction;
    private readonly HashSet<string> _validEnemyTargetIds =
        new HashSet<string>(StringComparer.Ordinal);

    /// <summary>Initializes the Godot control and connects its UI signals.</summary>
    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _actionGrid = GetNode<GridContainer>("Margin/Content/ActionGrid");
        _targetSelection = GetNode<VBoxContainer>("Margin/Content/TargetSelection");
        _targetHeading = GetNode<Label>("Margin/Content/TargetSelection/TargetHeading");
        _targetGrid = GetNode<GridContainer>("Margin/Content/TargetSelection/TargetGrid");
        _backButton = GetNode<Button>("Margin/Content/TargetSelection/BackButton");
        _actionCardScene = GD.Load<PackedScene>("res://Scenes/UI/Combat/CombatActionCard.tscn");
        _backButton.Pressed += CancelTargetSelection;
        UiTheme.StyleButton(_backButton);
        _targetGrid.Visible = false;
    }

    /// <summary>Sets actions.</summary>
    /// <param name="actions">The actions used by this operation.</param>
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
            CombatActionCard card = CreateActionCard(action);
            if (card != null)
            {
                _actionGrid.AddChild(card);
            }
        }
    }

    /// <summary>Sets selected enemy target.</summary>
    /// <param name="enemyId">The enemy id used by this operation.</param>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
    public bool SetSelectedEnemyTarget(string enemyId)
    {
        if (_pendingAction == null ||
            string.IsNullOrWhiteSpace(enemyId) ||
            !_validEnemyTargetIds.Contains(enemyId))
        {
            return false;
        }

        CombatActionRequest request = new CombatActionRequest(
            _pendingAction.Id,
            enemyId,
            _pendingAction.ActorId);

        ClearPendingTarget();
        _targetSelection.Visible = false;
        _actionGrid.Visible = true;
        ActionSelected?.Invoke(request);
        return true;
    }

    /// <summary>Clears target selection from the current UI state.</summary>
    public void ClearTargetSelection()
    {
        ClearPendingTarget();
        _targetSelection.Visible = false;
        _actionGrid.Visible = true;
    }

    private CombatActionCard CreateActionCard(CombatActionOption action)
    {
        if (_actionCardScene == null)
        {
            GD.PushError("CombatActionCard.tscn could not be loaded.");
            return null;
        }

        CombatActionCard card = _actionCardScene.Instantiate<CombatActionCard>();
        card.SetAction(action);
        UiTheme.StyleButton(card);
        card.Selected += SelectAction;
        return card;
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
        _validEnemyTargetIds.Clear();
        foreach (CombatActionTargetOption target in action.Targets)
        {
            _validEnemyTargetIds.Add(target.Id);
        }

        _targetHeading.Text = "Choose a highlighted enemy from the sidebar. Selecting one executes the action.";
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
        _actionGrid.Visible = false;
        _targetSelection.Visible = true;
    }


    private void CancelTargetSelection()
    {
        bool wasSelectingEnemy = _pendingAction != null;
        ClearPendingTarget();
        _targetSelection.Visible = false;
        _actionGrid.Visible = true;
        _targetGrid.Visible = false;
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
        ActionSelected?.Invoke(request);
    }

    private void ClearPendingTarget()
    {
        _pendingAction = null;
        _validEnemyTargetIds.Clear();
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
