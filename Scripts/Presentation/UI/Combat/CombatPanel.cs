using System;
using Godot;

/// <summary>Presents combat information and exposes its UI interactions.</summary>
public partial class CombatPanel : PanelContainer
{
    /// <summary>Raised when action selected occurs.</summary>
    public event Action<CombatActionRequest> ActionSelected;
    /// <summary>Raised when continue requested occurs.</summary>
    public event Action ContinueRequested;
    /// <summary>Raised when enemy target selection requested occurs.</summary>
    public event Action<string[]> EnemyTargetSelectionRequested;
    /// <summary>Raised when enemy target selection cancelled occurs.</summary>
    public event Action EnemyTargetSelectionCancelled;

    private Label _encounterTitle;
    private Label _mode;
    private Button _continueButton;
    private PartyRosterPanel _partyRosterPanel;
    private CombatStatusPanel _statusPanel;
    private CombatMessageLog _messageLog;
    private CombatActionPanel _actionPanel;

    /// <summary>Initializes the Godot control and connects its UI signals.</summary>
    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _encounterTitle = GetNode<Label>("Margin/Content/Header/EncounterTitle");
        _mode = GetNode<Label>("Margin/Content/Header/Mode");
        _continueButton = GetNode<Button>("Margin/Content/ContinueButton");
        _partyRosterPanel = GetNode<PartyRosterPanel>("Margin/Content/PartyRosterPanel");
        _statusPanel = GetNode<CombatStatusPanel>("Margin/Content/CombatStatusPanel");
        _messageLog = GetNode<CombatMessageLog>("Margin/Content/CombatMessageLog");
        _actionPanel = GetNode<CombatActionPanel>("Margin/Content/CombatActionPanel");
        _actionPanel.ActionSelected += HandleActionSelected;
        _actionPanel.EnemyTargetSelectionRequested += HandleEnemyTargetSelectionRequested;
        _actionPanel.EnemyTargetSelectionCancelled += HandleEnemyTargetSelectionCancelled;
        _continueButton.Pressed += HandleContinuePressed;
        _continueButton.Visible = false;
        _mode.Text = "TACTICAL TIMELINE";
        UiTheme.StyleButton(_continueButton, true);
    }

    /// <summary>Begins encounter and initializes the combat UI.</summary>
    /// <param name="title">The title used by this operation.</param>
    public void BeginEncounter(string title)
    {
        _encounterTitle.Text = title;
        _partyRosterPanel.SetMembers(Array.Empty<PartyCombatStatus>());
        _statusPanel.SetTurn(1, "PLAYER DECISION");
        _statusPanel.SetTimeline(Array.Empty<TimelineMarkerViewData>());
        _statusPanel.SetIntent(null);
        _messageLog.Clear();
        _actionPanel.SetActions(Array.Empty<CombatActionOption>());
        _continueButton.Visible = false;
    }

    /// <summary>Performs the apply view data operation for combat panel.</summary>
    /// <param name="viewData">The view data used by this operation.</param>
    public void ApplyViewData(CombatViewData viewData)
    {
        if (viewData == null)
        {
            GD.PushError("CombatPanel received null view data.");
            return;
        }

        _partyRosterPanel.SetMembers(viewData.Party);
        _statusPanel.SetTurn(viewData.Round, viewData.ActiveTurnLabel);
        _statusPanel.SetTimeline(viewData.Timeline);
        _statusPanel.SetIntent(viewData.Intent);
        _actionPanel.SetActions(viewData.AvailableActions);
        foreach (CombatMessage message in viewData.NewMessages)
        {
            _messageLog.Append(message);
        }
    }

    /// <summary>Sets timeline.</summary>
    /// <param name="entries">The entries used by this operation.</param>
    public void SetTimeline(TimelineMarkerViewData[] entries)
    {
        _statusPanel.SetTimeline(entries);
    }

    /// <summary>Plays party feedback feedback for the selected combatant or action.</summary>
    /// <param name="characterName">The character name used by this operation.</param>
    /// <param name="feedbackType">The feedback type used by this operation.</param>
    public void PlayPartyFeedback(string characterName, CombatFeedbackType feedbackType)
    {
        _partyRosterPanel.PlayFeedback(characterName, feedbackType);
    }

    /// <summary>Sets selected enemy target.</summary>
    /// <param name="enemyId">The enemy id used by this operation.</param>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
    public bool SetSelectedEnemyTarget(string enemyId)
    {
        return _actionPanel.SetSelectedEnemyTarget(enemyId);
    }

    /// <summary>Clears target selection from the current UI state.</summary>
    public void ClearTargetSelection()
    {
        _actionPanel.ClearTargetSelection();
    }

    /// <summary>Appends message to the combat message log.</summary>
    /// <param name="message">The message used by this operation.</param>
    public void AppendMessage(CombatMessage message)
    {
        _messageLog.Append(message);
    }

    /// <summary>Displays encounter complete in the combat interface.</summary>
    /// <param name="resultText">The result text used by this operation.</param>
    public void ShowEncounterComplete(string resultText)
    {
        _actionPanel.SetActions(Array.Empty<CombatActionOption>());
        _statusPanel.SetIntent(null);
        _continueButton.Text = "Return to Exploration";
        _continueButton.Visible = true;
        _encounterTitle.Text = resultText;
    }

    private void HandleActionSelected(CombatActionRequest request)
    {
        ActionSelected?.Invoke(request);
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
}
