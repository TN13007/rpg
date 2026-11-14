using System;
using Godot;

public partial class CombatPanel : PanelContainer
{
    public event Action<string> ActionSelected;

    private Label _encounterTitle;
    private CombatStatusPanel _statusPanel;
    private CombatMessageLog _messageLog;
    private CombatActionPanel _actionPanel;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _encounterTitle = GetNode<Label>("Margin/Content/Header/EncounterTitle");
        _statusPanel = GetNode<CombatStatusPanel>("Margin/Content/CombatStatusPanel");
        _messageLog = GetNode<CombatMessageLog>("Margin/Content/CombatMessageLog");
        _actionPanel = GetNode<CombatActionPanel>("Margin/Content/CombatActionPanel");
        _actionPanel.ActionSelected += HandleActionSelected;
    }

    public void BeginEncounter(string title)
    {
        _encounterTitle.Text = title;
        _statusPanel.SetTurn(1, "PLAYER TURN");
        _statusPanel.SetEnemies(Array.Empty<EnemyCombatStatus>());
        _statusPanel.SetIntent(null);
        _messageLog.Clear();
        _actionPanel.SetActions(Array.Empty<CombatActionOption>());
    }

    public void ApplyViewData(CombatViewData viewData)
    {
        if (viewData == null)
        {
            GD.PushError("CombatPanel received null view data.");
            return;
        }

        _statusPanel.SetTurn(viewData.Round, viewData.ActiveTurnLabel);
        _statusPanel.SetEnemies(viewData.Enemies);
        _statusPanel.SetIntent(viewData.Intent);
        _actionPanel.SetActions(viewData.AvailableActions);

        foreach (CombatMessage message in viewData.NewMessages)
        {
            _messageLog.Append(message);
        }
    }

    private void HandleActionSelected(string actionId)
    {
        ActionSelected?.Invoke(actionId);
    }
}
