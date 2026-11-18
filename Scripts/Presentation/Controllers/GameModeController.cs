using System;

public sealed class GameModeController
{
    private readonly StoryPanel _storyPanel;
    private readonly CombatPanel _combatPanel;

    public bool IsCombatMode => _combatPanel.Visible;

    public GameModeController(StoryPanel storyPanel, CombatPanel combatPanel)
    {
        _storyPanel = storyPanel ?? throw new ArgumentNullException(nameof(storyPanel));
        _combatPanel = combatPanel ?? throw new ArgumentNullException(nameof(combatPanel));
    }

    public void ShowExploration()
    {
        _combatPanel.Visible = false;
        _storyPanel.Visible = true;
    }

    public void ShowCombat()
    {
        _storyPanel.Visible = false;
        _combatPanel.Visible = true;
    }
}
