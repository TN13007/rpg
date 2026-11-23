using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Coordinates category, action, detail and target selection for immediate combat actions.</summary>
public partial class CombatActionPanel : PanelContainer
{
    /// <summary>Raised when an action is ready to execute, including any required target.</summary>
    public event Action<CombatActionSelection> ActionSelected;
    /// <summary>Raised when valid Enemy targets should be highlighted in the Enemy region.</summary>
    public event Action<string[]> EnemyTargetSelectionRequested;
    /// <summary>Raised when Enemy target highlighting should be cleared.</summary>
    public event Action EnemyTargetSelectionCancelled;

    private Button _attackButton;
    private Button _defenceButton;
    private Button _allureButton;
    private Button _inventoryButton;
    private Button _runButton;
    private Button _otherButton;
    private VBoxContainer _skillList;
    private ScrollContainer _skillScroll;
    private VBoxContainer _partyTargets;
    private Label _listHeading;
    private Label _detailTitle;
    private Label _detailDescription;
    private Label _effectSummary;
    private Label _detailCost;
    private Label _targetRule;
    private Label _unavailableReason;
    private Label _targetSelectionHint;
    private Button _cancelTargetButton;
    private PackedScene _actionCardScene;
    private CombatActionCardViewData[] _actions = Array.Empty<CombatActionCardViewData>();
    private readonly List<CombatActionCard> _actionCards = new List<CombatActionCard>();
    private readonly HashSet<string> _validEnemyTargetIds =
        new HashSet<string>(StringComparer.Ordinal);
    private CombatActionCardViewData _selectedAction;
    private CombatActionCardViewData _pendingAction;
    private CombatActionCategory _selectedCategory = CombatActionCategory.Attack;

    /// <summary>Initializes the three child scenes and connects all category controls.</summary>
    public override void _Ready()
    {
        UiTheme.StylePanel(this);

        _attackButton = GetNode<Button>("Margin/Content/CategoryList/Margin/Content/Categories/AttackButton");
        _defenceButton = GetNode<Button>("Margin/Content/CategoryList/Margin/Content/Categories/DefenceButton");
        _allureButton = GetNode<Button>("Margin/Content/CategoryList/Margin/Content/Categories/AllureButton");
        _inventoryButton = GetNode<Button>("Margin/Content/CategoryList/Margin/Content/Categories/InventoryButton");
        _runButton = GetNode<Button>("Margin/Content/CategoryList/Margin/Content/Categories/RunButton");
        _otherButton = GetNode<Button>("Margin/Content/CategoryList/Margin/Content/Categories/OtherButton");

        _listHeading = GetNode<Label>("Margin/Content/ActionList/Margin/Content/Heading");
        _skillScroll = GetNode<ScrollContainer>("Margin/Content/ActionList/Margin/Content/SkillScroll");
        _skillList = GetNode<VBoxContainer>("Margin/Content/ActionList/Margin/Content/SkillScroll/SkillList");
        _partyTargets = GetNode<VBoxContainer>("Margin/Content/ActionList/Margin/Content/PartyTargets");

        _detailTitle = GetNode<Label>("Margin/Content/DetailsPanel/Margin/Content/Title");
        _detailDescription = GetNode<Label>("Margin/Content/DetailsPanel/Margin/Content/Description");
        _effectSummary = GetNode<Label>("Margin/Content/DetailsPanel/Margin/Content/EffectSummary");
        _detailCost = GetNode<Label>("Margin/Content/DetailsPanel/Margin/Content/Cost");
        _targetRule = GetNode<Label>("Margin/Content/DetailsPanel/Margin/Content/TargetRule");
        _unavailableReason = GetNode<Label>("Margin/Content/DetailsPanel/Margin/Content/UnavailableReason");
        _targetSelectionHint = GetNode<Label>("Margin/Content/DetailsPanel/Margin/Content/TargetSelectionHint");
        _cancelTargetButton = GetNode<Button>("Margin/Content/DetailsPanel/Margin/Content/CancelTargetButton");

        _actionCardScene = GD.Load<PackedScene>("res://Scenes/UI/Combat/CombatActionCard.tscn");

        _attackButton.Pressed += () => SelectCategory(CombatActionCategory.Attack);
        _defenceButton.Pressed += () => SelectCategory(CombatActionCategory.Defence);
        _allureButton.Pressed += () => SelectCategory(CombatActionCategory.Allure);
        _inventoryButton.Pressed += () => SelectCategory(CombatActionCategory.Inventory);
        _runButton.Pressed += () => SelectCategory(CombatActionCategory.Run);
        _otherButton.Pressed += () => SelectCategory(CombatActionCategory.Other);
        _cancelTargetButton.Pressed += ClearTargetSelection;

        UiTheme.StyleButton(_attackButton);
        UiTheme.StyleButton(_defenceButton);
        UiTheme.StyleButton(_allureButton);
        UiTheme.StyleButton(_inventoryButton);
        UiTheme.StyleButton(_runButton);
        UiTheme.StyleButton(_otherButton);
        UiTheme.StyleButton(_cancelTargetButton);

        _otherButton.Visible = false;
        ClearDetails();
    }

    /// <summary>Replaces the actions belonging to the currently selected Party member.</summary>
    public void SetActions(CombatActionCardViewData[] actions)
    {
        bool wasSelectingTarget = _pendingAction != null;
        ClearPendingTarget();
        if (wasSelectingTarget)
            EnemyTargetSelectionCancelled?.Invoke();

        _actions = actions ?? Array.Empty<CombatActionCardViewData>();
        _selectedAction = null;
        _partyTargets.Visible = false;
        _skillScroll.Visible = true;
        ClearChildren(_partyTargets);
        ClearDetails();

        _otherButton.Visible = _actions.Any(action => action.Category == CombatActionCategory.Other);
        SelectCategory(GetInitialCategory());
    }

    /// <summary>Completes a pending Enemy-targeted action after validating the selected Enemy ID.</summary>
    public bool SetSelectedEnemyTarget(string enemyId)
    {
        if (_pendingAction == null ||
            _pendingAction.TargetKind != CombatActionTargetKind.Enemy ||
            string.IsNullOrWhiteSpace(enemyId) ||
            !_validEnemyTargetIds.Contains(enemyId))
            return false;

        SubmitPendingAction(enemyId);
        return true;
    }

    /// <summary>Cancels target selection and restores the current skill list.</summary>
    public void ClearTargetSelection()
    {
        bool wasSelectingTarget = _pendingAction != null;
        ClearPendingTarget();
        _partyTargets.Visible = false;
        _skillScroll.Visible = true;
        ClearChildren(_partyTargets);

        if (wasSelectingTarget)
            EnemyTargetSelectionCancelled?.Invoke();

        RenderActions();
        if (_selectedAction != null)
        {
            foreach (CombatActionCard card in _actionCards)
                card.SetSelected(card.ActionId == _selectedAction.Id);
            ShowSelectedAction(_selectedAction);
        }
    }

    private CombatActionCategory GetInitialCategory()
    {
        if (_actions.Any(action => action.Category == CombatActionCategory.Attack))
            return CombatActionCategory.Attack;
        if (_actions.Any(action => action.Category == CombatActionCategory.Defence))
            return CombatActionCategory.Defence;
        if (_actions.Any(action => action.Category == CombatActionCategory.Allure))
            return CombatActionCategory.Allure;
        if (_actions.Any(action => action.Category == CombatActionCategory.Inventory))
            return CombatActionCategory.Inventory;
        if (_actions.Any(action => action.Category == CombatActionCategory.Run))
            return CombatActionCategory.Run;
        return CombatActionCategory.Attack;
    }

    private void SelectCategory(CombatActionCategory category)
    {
        if (_pendingAction != null)
            ClearTargetSelection();

        _selectedCategory = category;
        _selectedAction = null;
        _partyTargets.Visible = false;
        _skillScroll.Visible = true;
        ClearChildren(_partyTargets);
        UpdateCategoryButtonState();
        ClearDetails();
        RenderActions();
    }

    private void UpdateCategoryButtonState()
    {
        _attackButton.ButtonPressed = _selectedCategory == CombatActionCategory.Attack;
        _defenceButton.ButtonPressed = _selectedCategory == CombatActionCategory.Defence;
        _allureButton.ButtonPressed = _selectedCategory == CombatActionCategory.Allure;
        _inventoryButton.ButtonPressed = _selectedCategory == CombatActionCategory.Inventory;
        _runButton.ButtonPressed = _selectedCategory == CombatActionCategory.Run;
        _otherButton.ButtonPressed = _selectedCategory == CombatActionCategory.Other;
    }

    private void RenderActions()
    {
        ClearChildren(_skillList);
        _actionCards.Clear();
        ClearChildren(_partyTargets);
        _listHeading.Text = GetCategoryLabel(_selectedCategory).ToUpperInvariant();

        CombatActionCardViewData[] filtered = _actions
            .Where(action => action.Category == _selectedCategory)
            .ToArray();

        if (filtered.Length == 0)
        {
            Label empty = new Label
            {
                Text = "No actions available in this category.",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            empty.AddThemeColorOverride("font_color", UiTheme.Muted);
            _skillList.AddChild(empty);
            return;
        }

        foreach (CombatActionCardViewData action in filtered)
        {
            if (_actionCardScene == null)
            {
                GD.PushError("CombatActionCard.tscn could not be loaded.");
                return;
            }

            CombatActionCard card = _actionCardScene.Instantiate<CombatActionCard>();
            card.SetAction(action);
            card.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            UiTheme.StyleButton(card);
            card.SetSelected(false);
            card.Selected += SelectAction;
            _skillList.AddChild(card);
            _actionCards.Add(card);
        }
    }

    private void SelectAction(CombatActionCardViewData action)
    {
        _selectedAction = action;
        foreach (CombatActionCard card in _actionCards)
            card.SetSelected(card.ActionId == action.Id);

        ShowSelectedAction(action);
        if (!action.IsAvailable)
            return;

        switch (action.TargetKind)
        {
            case CombatActionTargetKind.Enemy:
                _pendingAction = action;
                _validEnemyTargetIds.Clear();
                foreach (CombatActionTargetViewData target in action.Targets)
                    _validEnemyTargetIds.Add(target.Id);

                _targetSelectionHint.Text = "Choose a highlighted Enemy. The action executes when you select it.";
                _targetSelectionHint.Visible = true;
                _cancelTargetButton.Visible = true;
                EnemyTargetSelectionRequested?.Invoke(ToArray(_validEnemyTargetIds));
                break;

            case CombatActionTargetKind.Party:
                _pendingAction = action;
                ShowPartyTargets(action);
                _targetSelectionHint.Text = "Choose an ally to perform the action.";
                _targetSelectionHint.Visible = true;
                _cancelTargetButton.Visible = true;
                break;

            default:
                ActionSelected?.Invoke(new CombatActionSelection(action.Id, action.ActorId, string.Empty));
                break;
        }
    }

    private void ShowSelectedAction(CombatActionCardViewData action)
    {
        _detailTitle.Text = action.Label;
        _detailDescription.Text = action.Description;
        _detailCost.Text = string.IsNullOrWhiteSpace(action.CostLabel)
            ? string.Empty
            : "Cost  ·  " + action.CostLabel;
        _effectSummary.Text = action.EffectSummary;
        _effectSummary.Visible = !string.IsNullOrWhiteSpace(action.EffectSummary);
        _targetRule.Text = action.TargetKind switch
        {
            CombatActionTargetKind.Enemy => "Target  ·  Single enemy",
            CombatActionTargetKind.Party => "Target  ·  One ally",
            _ => "Target  ·  Self / no target"
        };
        _targetRule.Visible = true;
        _unavailableReason.Text = action.UnavailableReason;
        _unavailableReason.Visible = !action.IsAvailable &&
            !string.IsNullOrWhiteSpace(action.UnavailableReason);
        _targetSelectionHint.Visible = false;
        _cancelTargetButton.Visible = false;
    }

    private void ShowPartyTargets(CombatActionCardViewData action)
    {
        _skillScroll.Visible = false;
        _partyTargets.Visible = true;
        ClearChildren(_partyTargets);

        Label heading = new Label
        {
            Text = "SELECT ALLY",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        heading.AddThemeColorOverride("font_color", UiTheme.Accent);
        _partyTargets.AddChild(heading);

        foreach (CombatActionTargetViewData target in action.Targets)
        {
            Button targetButton = new Button
            {
                Text = target.Name,
                CustomMinimumSize = new Vector2(0, 42),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                Alignment = HorizontalAlignment.Left,
                FocusMode = FocusModeEnum.All,
                MouseDefaultCursorShape = CursorShape.PointingHand
            };
            UiTheme.StyleButton(targetButton);
            targetButton.Pressed += () => SubmitPendingAction(target.Id);
            _partyTargets.AddChild(targetButton);
        }
    }

    private void SubmitPendingAction(string targetId)
    {
        if (_pendingAction == null)
            return;

        CombatActionSelection selection = new CombatActionSelection(
            _pendingAction.Id,
            _pendingAction.ActorId,
            targetId);

        ClearPendingTarget();
        _partyTargets.Visible = false;
        _skillScroll.Visible = true;
        ClearChildren(_partyTargets);
        _targetSelectionHint.Visible = false;
        _cancelTargetButton.Visible = false;
        EnemyTargetSelectionCancelled?.Invoke();
        ActionSelected?.Invoke(selection);
    }

    private void ClearPendingTarget()
    {
        _pendingAction = null;
        _validEnemyTargetIds.Clear();
    }

    private void ClearDetails()
    {
        _detailTitle.Text = "Select a skill";
        _detailDescription.Text = "Choose a command, then select a skill from the list.";
        _detailCost.Text = string.Empty;
        _effectSummary.Text = string.Empty;
        _effectSummary.Visible = false;
        _targetRule.Text = string.Empty;
        _targetRule.Visible = false;
        _unavailableReason.Text = string.Empty;
        _unavailableReason.Visible = false;
        _targetSelectionHint.Text = string.Empty;
        _targetSelectionHint.Visible = false;
        _cancelTargetButton.Visible = false;
    }

    private static string GetCategoryLabel(CombatActionCategory category)
    {
        return category switch
        {
            CombatActionCategory.Attack => "Attack",
            CombatActionCategory.Defence => "Defence",
            CombatActionCategory.Allure => "Allure",
            CombatActionCategory.Inventory => "Inventory",
            CombatActionCategory.Run => "Run",
            _ => "Other"
        };
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
