using System;
using System.Linq;
using Godot;

/// <summary>Presents combat data and exposes action, End Turn and encounter-exit interactions.</summary>
public partial class CombatPanel : PanelContainer
{
    /// <summary>Raised when the player selects an action.</summary>
    public event Action<CombatActionSelection> ActionSelected;
    /// <summary>Raised when the player chooses to end the Player Turn.</summary>
    public event Action EndPlayerTurnRequested;
    /// <summary>Raised when the roster's selected acting member changes.</summary>
    public event Action<string> PartyMemberSelected;
    /// <summary>Raised when the player acknowledges completion and returns to Exploration.</summary>
    public event Action ContinueRequested;
    /// <summary>Raised when the player begins target selection for an action.</summary>
    public event Action<string[]> EnemyTargetSelectionRequested;
    /// <summary>Raised when the current target selection is cancelled.</summary>
    public event Action EnemyTargetSelectionCancelled;

    private Label _encounterTitle;
    private Label _mode;
    private Button _continueButton;
    private Button _endTurnButton;
    private PartyRosterPanel _partyRosterPanel;
    private CombatStatusPanel _statusPanel;
    private CombatMessageLog _messageLog;
    private CombatActionPanel _actionPanel;
    private CombatActionCardViewData[] _availableActions = Array.Empty<CombatActionCardViewData>();

    /// <summary>Resolves child controls, connects UI signals and applies the panel style.</summary>
    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _encounterTitle = GetNode<Label>("Margin/Content/Header/EncounterTitle");
        _mode = GetNode<Label>("Margin/Content/Header/Mode");
        _continueButton = GetNode<Button>("Margin/Content/ContinueButton");
        _endTurnButton = GetNode<Button>("Margin/Content/EndTurnButton");
        _partyRosterPanel = GetNode<PartyRosterPanel>("Margin/Content/PartyRosterPanel");
        _statusPanel = GetNode<CombatStatusPanel>("Margin/Content/CombatStatusPanel");
        _messageLog = GetNode<CombatMessageLog>("Margin/Content/CombatMessageLog");
        _actionPanel = GetNode<CombatActionPanel>("Margin/Content/CombatActionPanel");

        _actionPanel.ActionSelected += HandleActionSelected;
        _actionPanel.EnemyTargetSelectionRequested += HandleEnemyTargetSelectionRequested;
        _actionPanel.EnemyTargetSelectionCancelled += HandleEnemyTargetSelectionCancelled;
        _partyRosterPanel.MemberSelected += HandlePartyMemberSelected;
        _continueButton.Pressed += HandleContinuePressed;
        _endTurnButton.Pressed += HandleEndTurnPressed;
        _continueButton.Visible = false;
        _endTurnButton.Visible = false;
        _mode.Text = "TURN-BASED COMBAT";
        UiTheme.StyleButton(_continueButton, true);
        UiTheme.StyleButton(_endTurnButton, true);
    }

    /// <summary>Initializes the visible state for a new encounter.</summary>
    public void BeginEncounter(string title)
    {
        _encounterTitle.Text = title;
        _partyRosterPanel.SetMembers(Array.Empty<PartyMemberCardViewData>());
        _availableActions = Array.Empty<CombatActionCardViewData>();
        _statusPanel.SetTurn(1, "PLAYER TURN", true, 0, 0);
        _messageLog.Clear();
        _actionPanel.SetActions(Array.Empty<CombatActionCardViewData>());
        _continueButton.Visible = false;
        _endTurnButton.Visible = false;
    }

    /// <summary>Applies the latest Domain-derived view data and appends its new messages.</summary>
    public void ApplyViewData(CombatViewData viewData)
    {
        if (viewData == null)
        {
            GD.PushError("CombatPanel received null view data.");
            return;
        }

        _partyRosterPanel.SetMembers(viewData.Party);
        PartyMemberSelected?.Invoke(_partyRosterPanel.SelectedMemberId);
        _availableActions = viewData.AvailableActions;
        UpdateActionOptions();
        _statusPanel.SetTurn(
            viewData.Round,
            viewData.ActiveTurnLabel,
            viewData.IsPlayerTurn,
            viewData.CurrentActionPoints,
            viewData.ActionCapacity);
        _endTurnButton.Visible = viewData.IsPlayerTurn;
        _continueButton.Visible = false;

        foreach (CombatMessage message in viewData.NewMessages)
        {
            _messageLog.Append(message);
        }
    }

    /// <summary>Plays the Party member's action feedback cue.</summary>
    public void PlayPartyFeedback(string characterName, CombatFeedbackType feedbackType)
    {
        _partyRosterPanel.PlayFeedback(characterName, feedbackType);
    }

    /// <summary>Selects an enemy target in the action panel.</summary>
    public bool SetSelectedEnemyTarget(string enemyId)
    {
        return _actionPanel.SetSelectedEnemyTarget(enemyId);
    }

    /// <summary>Clears the current action panel target selection.</summary>
    public void ClearTargetSelection()
    {
        _actionPanel.ClearTargetSelection();
    }

    /// <summary>Appends one message to the combat history.</summary>
    public void AppendMessage(CombatMessage message)
    {
        _messageLog.Append(message);
    }

    /// <summary>Displays the final encounter outcome and the Exploration return action.</summary>
    public void ShowEncounterComplete(string resultText)
    {
        _availableActions = Array.Empty<CombatActionCardViewData>();
        _actionPanel.SetActions(Array.Empty<CombatActionCardViewData>());
        _endTurnButton.Visible = false;
        _continueButton.Text = "Return to Exploration";
        _continueButton.Visible = true;
        _encounterTitle.Text = resultText;
    }

    private void UpdateActionOptions()
    {
        string selectedActorId = _partyRosterPanel.SelectedMemberId;
        CombatActionCardViewData[] selectedActorActions = string.IsNullOrWhiteSpace(selectedActorId)
            ? Array.Empty<CombatActionCardViewData>()
            : _availableActions
                .Where(action => action.ActorId == selectedActorId)
                .ToArray();

        _actionPanel.SetActions(selectedActorActions);
    }

    private void HandlePartyMemberSelected(string memberId)
    {
        UpdateActionOptions();
        PartyMemberSelected?.Invoke(memberId);
    }

    private void HandleActionSelected(CombatActionSelection selection)
    {
        ActionSelected?.Invoke(selection);
    }

    private void HandleEnemyTargetSelectionRequested(string[] targetIds)
    {
        EnemyTargetSelectionRequested?.Invoke(targetIds);
    }

    private void HandleEnemyTargetSelectionCancelled()
    {
        EnemyTargetSelectionCancelled?.Invoke();
    }

    private void HandleContinuePressed()
    {
        ContinueRequested?.Invoke();
    }

    private void HandleEndTurnPressed()
    {
        EndPlayerTurnRequested?.Invoke();
    }
}