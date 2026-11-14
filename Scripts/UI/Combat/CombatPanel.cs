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

    public void BeginEncounter(string title, int round, string turnLabel, EnemyCombatStatus[] enemies)
    {
        _encounterTitle.Text = title;
        _statusPanel.SetTurn(round, turnLabel);
        _statusPanel.SetEnemies(enemies);
        _statusPanel.ClearIntent();
        _messageLog.Clear();
        _actionPanel.SetActions(Array.Empty<CombatActionOption>());
    }

    public void SetTurn(int round, string turnLabel)
    {
        _statusPanel.SetTurn(round, turnLabel);
    }

    public void SetEnemies(EnemyCombatStatus[] enemies)
    {
        _statusPanel.SetEnemies(enemies);
    }

    public void ShowEnemyIntent(EnemyIntent intent)
    {
        _statusPanel.ShowIntent(intent);
    }

    public void AppendMessage(CombatMessage message)
    {
        _messageLog.Append(message);
    }

    public void SetAvailableActions(CombatActionOption[] actions)
    {
        _actionPanel.SetActions(actions);
    }

    public void ClearMessages()
    {
        _messageLog.Clear();
    }

    private void HandleActionSelected(string actionId)
    {
        ActionSelected?.Invoke(actionId);
    }
}
