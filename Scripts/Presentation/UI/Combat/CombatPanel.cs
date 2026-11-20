using System;
using Godot;

public partial class CombatPanel : PanelContainer
{
    public event Action<CombatActionRequest> ActionSelected;
    public event Action ContinueRequested;
    private Label _encounterTitle;
    private Label _mode;
    private Button _continueButton;
    private PartyRosterPanel _partyRosterPanel;
    private CombatStatusPanel _statusPanel;
    private CombatMessageLog _messageLog;
    private CombatActionPanel _actionPanel;

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
        _continueButton.Pressed += HandleContinuePressed;
        _continueButton.Visible = false;
        _mode.Text = "TACTICAL TIMELINE";
        UiTheme.StyleButton(_continueButton, true);
    }

    public void BeginEncounter(string title)
    {
        _encounterTitle.Text = title;
        _partyRosterPanel.SetMembers(Array.Empty<PartyCombatStatus>());
        _statusPanel.SetTurn(1, "PLAYER DECISION");
        _statusPanel.SetTimeline(Array.Empty<TimelineCombatantStatus>());
        _statusPanel.SetIntent(null);
        _messageLog.Clear();
        _actionPanel.SetActions(Array.Empty<CombatActionOption>());
        _continueButton.Visible = false;
    }

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

    public void AppendMessage(CombatMessage message)
    {
        _messageLog.Append(message);
    }

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

    private void HandleContinuePressed()
    {
        ContinueRequested?.Invoke();
    }
}
