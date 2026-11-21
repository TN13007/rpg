using System;

/// <summary>Coordinates game mode flow and user interactions.</summary>
public sealed class GameModeController
{
    private readonly StoryPanel _storyPanel;
    private readonly CombatPanel _combatPanel;

    /// <summary>Gets a value indicating whether combat mode.</summary>
    public bool IsCombatMode => _combatPanel.Visible;

    /// <summary>Initializes a new instance of <see cref="GameModeController"/>.</summary>
    /// <param name="storyPanel">The story panel used to initialize the instance.</param>
    /// <param name="combatPanel">The combat panel used to initialize the instance.</param>
    public GameModeController(StoryPanel storyPanel, CombatPanel combatPanel)
    {
        _storyPanel = storyPanel ?? throw new ArgumentNullException(nameof(storyPanel));
        _combatPanel = combatPanel ?? throw new ArgumentNullException(nameof(combatPanel));
    }

    /// <summary>Displays exploration in the current game mode.</summary>
    public void ShowExploration()
    {
        _combatPanel.Visible = false;
        _storyPanel.Visible = true;
    }

    /// <summary>Displays combat in the current game mode.</summary>
    public void ShowCombat()
    {
        _storyPanel.Visible = false;
        _combatPanel.Visible = true;
    }
}
