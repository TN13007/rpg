using System;
using System.Collections.Generic;
using Godot;

/// <summary>Presents category-filtered actions, selected-action details and target selection.</summary>
public partial class CombatActionPanel : PanelContainer
{
    /// <summary>Raised when the player confirms an action selection and optional target.</summary>
    public event Action<CombatActionSelection> ActionSelected;
    /// <summary>Raised when a confirmed action requires the player to choose an Enemy.</summary>
    public event Action<string[]> EnemyTargetSelectionRequested;
    /// <summary>Raised when the current Enemy target-selection state is cancelled.</summary>
    public event Action EnemyTargetSelectionCancelled;

    private GridContainer _actionGrid;
    private HBoxContainer _categoryFilters;
    private Button _offenseButton;
    private Button _defenseButton;
    private Button _supportButton;
    private Button _otherButton;
    private VBoxContainer _selectedActionDetails;
    private Label _detailTitle;
    private Label _detailCost;
    private Label _detailDescription;
    private Label _unavailableReason;
    private Button _confirmActionButton;
    private VBoxContainer _targetSelection;
    private Label _targetHeading;
    private GridContainer _targetGrid;
    private Button _backButton;
    private PackedScene _actionCardScene;
    private CombatActionCardViewData[] _actions = Array.Empty<CombatActionCardViewData>();
    private readonly List<CombatActionCard> _actionCards = new List<CombatActionCard>();
    private CombatActionCardViewData _selectedAction;
    private CombatActionCardViewData _pendingAction;
    private CombatActionCategory _selectedCategory;
    private readonly HashSet<string> _validEnemyTargetIds =
        new HashSet<string>(StringComparer.Ordinal);

    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _categoryFilters = GetNode<HBoxContainer>("Margin/Content/CategoryFilters");
        _offenseButton = GetNode<Button>("Margin/Content/CategoryFilters/OffenseButton");
        _defenseButton = GetNode<Button>("Margin/Content/CategoryFilters/DefenseButton");
        _supportButton = GetNode<Button>("Margin/Content/CategoryFilters/SupportButton");
        _otherButton = GetNode<Button>("Margin/Content/CategoryFilters/OtherButton");
        _actionGrid = GetNode<GridContainer>("Margin/Content/ActionGrid");
        _selectedActionDetails = GetNode<VBoxContainer>("Margin/Content/SelectedActionDetails");
        _detailTitle = GetNode<Label>("Margin/Content/SelectedActionDetails/DetailTitle");
        _detailCost = GetNode<Label>("Margin/Content/SelectedActionDetails/DetailCost");
        _detailDescription = GetNode<Label>("Margin/Content/SelectedActionDetails/DetailDescription");
        _unavailableReason = GetNode<Label>("Margin/Content/SelectedActionDetails/UnavailableReason");
        _confirmActionButton = GetNode<Button>("Margin/Content/SelectedActionDetails/ConfirmActionButton");
        _targetSelection = GetNode<VBoxContainer>("Margin/Content/TargetSelection");
        _targetHeading = GetNode<Label>("Margin/Content/TargetSelection/TargetHeading");
        _targetGrid = GetNode<GridContainer>("Margin/Content/TargetSelection/TargetGrid");
        _backButton = GetNode<Button>("Margin/Content/TargetSelection/BackButton");
        _actionCardScene = GD.Load<PackedScene>("res://Scenes/UI/Combat/CombatActionCard.tscn");

        _offenseButton.Pressed += () => SelectCategory(CombatActionCategory.Offense);
        _defenseButton.Pressed += () => SelectCategory(CombatActionCategory.Defense);
        _supportButton.Pressed += () => SelectCategory(CombatActionCategory.Support);
        _otherButton.Pressed += () => SelectCategory(CombatActionCategory.Other);
        _confirmActionButton.Pressed += ConfirmSelectedAction;
        _backButton.Pressed += CancelTargetSelection;

        UiTheme.StyleButton(_offenseButton);
        UiTheme.StyleButton(_defenseButton);
        UiTheme.StyleButton(_supportButton);
        UiTheme.StyleButton(_otherButton);
        UiTheme.StyleButton(_backButton);
        UiTheme.StyleButton(_confirmActionButton, true);
        _selectedActionDetails.Visible = false;
        _targetSelection.Visible = false;
    }

    /// <summary>Replaces the actions displayed for the currently selected Party member.</summary>
    public void SetActions(CombatActionCardViewData[] actions)
    {
        bool wasSelectingEnemy = _pendingAction != null;
        ClearPendingTarget();
        ClearChildren(_targetGrid);
        _targetSelection.Visible = false;
        _actions = actions ?? Array.Empty<CombatActionCardViewData>();
        _selectedAction = null;
        _selectedActionDetails.Visible = false;
        _confirmActionButton.Visible = true;
        _actionGrid.Visible = true;

        if (wasSelectingEnemy)
        {
            EnemyTargetSelectionCancelled?.Invoke();
        }

        UpdateCategoryFilters();
        SelectFirstAvailableCategory();
    }

    /// <summary>Submits the target chosen from an Enemy card after validating it against the pending view data.</summary>
    public bool SetSelectedEnemyTarget(string enemyId)
    {
        if (_pendingAction == null ||
            string.IsNullOrWhiteSpace(enemyId) ||
            !_validEnemyTargetIds.Contains(enemyId))
        {
            return false;
        }

        CombatActionSelection selection = new CombatActionSelection(
            _pendingAction.Id,
            _pendingAction.ActorId,
            enemyId);

        ClearPendingTarget();
        _targetSelection.Visible = false;
        _actionGrid.Visible = true;
        ActionSelected?.Invoke(selection);
        return true;
    }

    /// <summary>Clears the current target state and restores the action list.</summary>
    public void ClearTargetSelection()
    {
        bool wasSelectingEnemy = _pendingAction != null;
        ClearPendingTarget();
        ClearChildren(_targetGrid);
        _targetSelection.Visible = false;
        _confirmActionButton.Visible = true;
        _actionGrid.Visible = true;
        if (wasSelectingEnemy)
        {
            EnemyTargetSelectionCancelled?.Invoke();
        }
    }

    private void UpdateCategoryFilters()
    {
        bool hasOffense = ContainsCategory(CombatActionCategory.Offense);
        bool hasDefense = ContainsCategory(CombatActionCategory.Defense);
        bool hasSupport = ContainsCategory(CombatActionCategory.Support);
        bool hasOther = ContainsCategory(CombatActionCategory.Other);

        _offenseButton.Visible = hasOffense;
        _defenseButton.Visible = hasDefense;
        _supportButton.Visible = hasSupport;
        _otherButton.Visible = hasOther;
        _categoryFilters.Visible = hasOffense || hasDefense || hasSupport || hasOther;
    }

    private bool ContainsCategory(CombatActionCategory category)
    {
        foreach (CombatActionCardViewData action in _actions)
        {
            if (action.Category == category)
            {
                return true;
            }
        }

        return false;
    }

    private void SelectFirstAvailableCategory()
    {
        if (ContainsCategory(CombatActionCategory.Offense))
        {
            SelectCategory(CombatActionCategory.Offense);
        }
        else if (ContainsCategory(CombatActionCategory.Defense))
        {
            SelectCategory(CombatActionCategory.Defense);
        }
        else if (ContainsCategory(CombatActionCategory.Support))
        {
            SelectCategory(CombatActionCategory.Support);
        }
        else if (ContainsCategory(CombatActionCategory.Other))
        {
            SelectCategory(CombatActionCategory.Other);
        }
        else
        {
            ClearChildren(_actionGrid);
            Label empty = new Label { Text = "No actions available." };
            empty.AddThemeColorOverride("font_color", UiTheme.Muted);
            _actionGrid.AddChild(empty);
        }
    }

    private void SelectCategory(CombatActionCategory category)
    {
        if (!ContainsCategory(category))
        {
            return;
        }

        _selectedCategory = category;
        _selectedAction = null;
        _selectedActionDetails.Visible = false;
        SetCategoryButtonState();

        ClearChildren(_actionGrid);
        _actionCards.Clear();
        foreach (CombatActionCardViewData action in _actions)
        {
            if (action.Category != _selectedCategory)
            {
                continue;
            }

            CombatActionCard card = CreateActionCard(action);
            if (card != null)
            {
                _actionGrid.AddChild(card);
                _actionCards.Add(card);
            }
        }
    }

    private void SetCategoryButtonState()
    {
        _offenseButton.ButtonPressed = _selectedCategory == CombatActionCategory.Offense;
        _defenseButton.ButtonPressed = _selectedCategory == CombatActionCategory.Defense;
        _supportButton.ButtonPressed = _selectedCategory == CombatActionCategory.Support;
        _otherButton.ButtonPressed = _selectedCategory == CombatActionCategory.Other;
    }

    private CombatActionCard CreateActionCard(CombatActionCardViewData action)
    {
        if (_actionCardScene == null)
        {
            GD.PushError("CombatActionCard.tscn could not be loaded.");
            return null;
        }

        CombatActionCard card = _actionCardScene.Instantiate<CombatActionCard>();
        card.SetAction(action);
        UiTheme.StyleButton(card);
        card.SetSelected(false);
        card.Selected += SelectAction;
        return card;
    }

    private void SelectAction(CombatActionCardViewData action)
    {
        _selectedAction = action;
        foreach (CombatActionCard card in _actionCards)
        {
            card.SetSelected(card.ActionId == action.Id);
        }

        ShowSelectedActionDetails(action);
    }

    private void ShowSelectedActionDetails(CombatActionCardViewData action)
    {
        _detailTitle.Text = action.Label;
        _detailCost.Text = action.CostLabel;
        _detailDescription.Text = action.Description;
        _unavailableReason.Text = action.UnavailableReason;
        _unavailableReason.Visible = !action.IsAvailable &&
            !string.IsNullOrWhiteSpace(action.UnavailableReason);
        _confirmActionButton.Disabled = !action.IsAvailable;
        _confirmActionButton.Text = action.TargetKind switch
        {
            CombatActionTargetKind.Enemy => "Choose Enemy",
            CombatActionTargetKind.Party => "Choose Ally",
            _ => "Use Action"
        };
        _selectedActionDetails.Visible = true;
    }

    private void ConfirmSelectedAction()
    {
        if (_selectedAction == null || !_selectedAction.IsAvailable)
        {
            return;
        }

        if (_selectedAction.TargetKind == CombatActionTargetKind.Enemy)
        {
            _pendingAction = _selectedAction;
            _validEnemyTargetIds.Clear();
            foreach (CombatActionTargetViewData target in _selectedAction.Targets)
            {
                _validEnemyTargetIds.Add(target.Id);
            }

            _targetHeading.Text = "Choose a highlighted Enemy. Click to execute the action.";
            _confirmActionButton.Visible = false;
            _targetGrid.Visible = false;
            _actionGrid.Visible = false;
            _targetSelection.Visible = true;
            EnemyTargetSelectionRequested?.Invoke(ToArray(_validEnemyTargetIds));
            return;
        }

        if (_selectedAction.TargetKind == CombatActionTargetKind.Party)
        {
            ShowPartyTargetButtons(_selectedAction);
            return;
        }

        ActionSelected?.Invoke(new CombatActionSelection(
            _selectedAction.Id,
            _selectedAction.ActorId,
            string.Empty));
    }

    private void ShowPartyTargetButtons(CombatActionCardViewData action)
    {
        ClearChildren(_targetGrid);
        _targetHeading.Text = "Choose an ally to revive.";
        foreach (CombatActionTargetViewData target in action.Targets)
        {
            CombatActionButton button = new CombatActionButton
            {
                Selection = new CombatActionSelection(action.Id, action.ActorId, target.Id),
                Text = target.Name,
                CustomMinimumSize = new Vector2(0, 42),
                Alignment = HorizontalAlignment.Left,
                FocusMode = FocusModeEnum.All,
                MouseDefaultCursorShape = CursorShape.PointingHand
            };
            UiTheme.StyleButton(button);
            button.ActionSelected += HandleActionSelected;
            _targetGrid.AddChild(button);
        }

        _confirmActionButton.Visible = false;
        _targetGrid.Visible = true;
        _actionGrid.Visible = false;
        _targetSelection.Visible = true;
    }

    private void CancelTargetSelection()
    {
        bool wasSelectingEnemy = _pendingAction != null;
        ClearPendingTarget();
        ClearChildren(_targetGrid);
        _targetSelection.Visible = false;
        _confirmActionButton.Visible = true;
        _actionGrid.Visible = true;
        if (wasSelectingEnemy)
        {
            EnemyTargetSelectionCancelled?.Invoke();
        }
    }

    private void HandleActionSelected(CombatActionSelection selection)
    {
        _targetSelection.Visible = false;
        _targetGrid.Visible = false;
        _actionGrid.Visible = true;
        ActionSelected?.Invoke(selection);
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
