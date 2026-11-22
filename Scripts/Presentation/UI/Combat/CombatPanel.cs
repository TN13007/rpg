using System;
using System.Linq;
using Godot;

/// <summary>Composes the Enemy, Party, portrait and immediate-action regions of the Combat HUD.</summary>
public partial class CombatPanel : PanelContainer
{
    /// <summary>Raised when a valid action is ready to be submitted to the combat flow.</summary>
    public event Action<CombatActionSelection> ActionSelected;
    /// <summary>Raised when the player ends the Player Turn.</summary>
    public event Action EndPlayerTurnRequested;
    /// <summary>Raised when the player returns to Exploration after an encounter.</summary>
    public event Action ContinueRequested;

    private Label _encounterTitle;
    private Button _continueButton;
    private Button _endTurnButton;
    private PartyRosterPanel _partyBar;
    private CombatStatusPanel _statusPanel;
    private CombatMessageLog _messageLog;
    private CombatActionPanel _actionPanel;
    private EnemyRegion _enemyRegion;
    private CharacterPortrait _characterPortrait;
    private CombatActionCardViewData[] _availableActions = Array.Empty<CombatActionCardViewData>();
    private PartyMemberCardViewData[] _partyMembers = Array.Empty<PartyMemberCardViewData>();

    /// <summary>Resolves child components and connects their presentation events.</summary>
    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _encounterTitle = GetNode<Label>("Margin/Content/Header/EncounterTitle");
        _continueButton = GetNode<Button>("Margin/Content/Header/ContinueButton");
        _endTurnButton = GetNode<Button>("Margin/Content/Header/EndTurnButton");
        _statusPanel = GetNode<CombatStatusPanel>("Margin/Content/Header/CombatStatusPanel");
        _partyBar = GetNode<PartyRosterPanel>("Margin/Content/PartyBar");
        _messageLog = GetNode<CombatMessageLog>("Margin/Content/CombatMessageLog");
        _actionPanel = GetNode<CombatActionPanel>("Margin/Content/CombatActionPanel");
        _enemyRegion = GetNode<EnemyRegion>("Margin/Content/BattlefieldRow/EnemyRegion");
        _characterPortrait = GetNode<CharacterPortrait>("Margin/Content/BattlefieldRow/CharacterPortrait");

        _actionPanel.ActionSelected += HandleActionSelected;
        _actionPanel.EnemyTargetSelectionRequested += BeginEnemyTargetSelection;
        _actionPanel.EnemyTargetSelectionCancelled += ClearEnemyTargetSelection;
        _enemyRegion.EnemySelected += HandleEnemySelected;
        _partyBar.MemberSelected += HandlePartyMemberSelected;
        _continueButton.Pressed += HandleContinuePressed;
        _endTurnButton.Pressed += HandleEndTurnPressed;

        _continueButton.Visible = false;
        _endTurnButton.Visible = false;
        UiTheme.StyleButton(_continueButton, true);
        UiTheme.StyleButton(_endTurnButton, true);
    }

    /// <summary>Initializes the visible state for a new encounter.</summary>
    public void BeginEncounter(string title)
    {
        _encounterTitle.Text = title;
        _partyMembers = Array.Empty<PartyMemberCardViewData>();
        _partyBar.SetMembers(_partyMembers);
        _enemyRegion.SetEnemies(Array.Empty<EnemyCardViewData>());
        _enemyRegion.ClearTargetSelection();
        _characterPortrait.Clear();
        _availableActions = Array.Empty<CombatActionCardViewData>();
        _statusPanel.SetTurn(1, "PLAYER TURN", true, 0, 0);
        _messageLog.Clear();
        _actionPanel.SetActions(Array.Empty<CombatActionCardViewData>());
        _continueButton.Visible = false;
        _endTurnButton.Visible = false;
    }

    /// <summary>Applies the latest Domain-derived view data to each independent region.</summary>
    public void ApplyViewData(CombatViewData viewData)
    {
        if (viewData == null)
        {
            GD.PushError("CombatPanel received null view data.");
            return;
        }

        _partyMembers = viewData.Party;
        _enemyRegion.SetEnemies(viewData.Enemies);
        _partyBar.SetMembers(_partyMembers);
        RefreshSelectedPortrait();

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
            _messageLog.Append(message);
    }

    /// <summary>Plays a visual feedback cue on the supplied Party member.</summary>
    public void PlayPartyFeedback(string characterName, CombatFeedbackType feedbackType)
    {
        _partyBar.PlayFeedback(characterName, feedbackType);
    }

    /// <summary>Plays a visual feedback cue on the supplied Enemy.</summary>
    public void PlayEnemyFeedback(string enemyName, CombatFeedbackType feedbackType)
    {
        _enemyRegion.PlayFeedback(enemyName, feedbackType);
    }

    /// <summary>Completes the pending Enemy-targeted action when the selected Enemy is valid.</summary>
    public bool SetSelectedEnemyTarget(string enemyId)
    {
        return _actionPanel.SetSelectedEnemyTarget(enemyId);
    }

    /// <summary>Clears target selection in both the action panel and Enemy region.</summary>
    public void ClearTargetSelection()
    {
        _actionPanel.ClearTargetSelection();
        _enemyRegion.ClearTargetSelection();
    }

    /// <summary>Appends one message to the retained combat history.</summary>
    public void AppendMessage(CombatMessage message)
    {
        _messageLog.Append(message);
    }

    /// <summary>Displays the encounter outcome and the return-to-Exploration action.</summary>
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
        string selectedActorId = _partyBar.SelectedMemberId;
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
        RefreshSelectedPortrait();
    }

    private void RefreshSelectedPortrait()
    {
        string selectedMemberId = _partyBar.SelectedMemberId;
        PartyMemberCardViewData selectedMember = Array.Find(
            _partyMembers,
            member => member.Id == selectedMemberId);

        if (selectedMember == null)
            _characterPortrait.Clear();
        else
            _characterPortrait.SetCharacter(selectedMember.Portrait);
    }

    private void BeginEnemyTargetSelection(string[] targetIds)
    {
        _enemyRegion.BeginTargetSelection(targetIds);
    }

    private void ClearEnemyTargetSelection()
    {
        _enemyRegion.ClearTargetSelection();
    }

    private void HandleEnemySelected(string enemyId)
    {
        _actionPanel.SetSelectedEnemyTarget(enemyId);
    }

    private void HandleActionSelected(CombatActionSelection selection)
    {
        ActionSelected?.Invoke(selection);
    }

    private void HandleContinuePressed()
    {
        ContinueRequested?.Invoke();
    }

    private void HandleEndTurnPressed()
    {
        ClearTargetSelection();
        EndPlayerTurnRequested?.Invoke();
    }
}
