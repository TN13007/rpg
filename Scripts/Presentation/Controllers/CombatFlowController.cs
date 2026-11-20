using System;

public sealed class CombatFlowController
{
    private readonly StoryPanel _storyPanel;
    private readonly CombatPanel _combatPanel;
    private readonly CharacterPanel _characterPanel;
    private readonly Sidebar _sidebar;
    private readonly GameModeController _gameModeController;
    private readonly Character _player;
    private readonly CombatSessionService _combatSession;
    private readonly CombatPresentationMapper _combatMapper;

    public CombatFlowController(
        StoryPanel storyPanel,
        CombatPanel combatPanel,
        CharacterPanel characterPanel,
        Sidebar sidebar,
        GameModeController gameModeController,
        Character player,
        CombatSessionService combatSession,
        CombatPresentationMapper combatMapper)
    {
        _storyPanel = storyPanel ?? throw new ArgumentNullException(nameof(storyPanel));
        _combatPanel = combatPanel ?? throw new ArgumentNullException(nameof(combatPanel));
        _characterPanel = characterPanel ?? throw new ArgumentNullException(nameof(characterPanel));
        _sidebar = sidebar ?? throw new ArgumentNullException(nameof(sidebar));
        _gameModeController = gameModeController ?? throw new ArgumentNullException(nameof(gameModeController));
        _player = player ?? throw new ArgumentNullException(nameof(player));
        _combatSession = combatSession ?? throw new ArgumentNullException(nameof(combatSession));
        _combatMapper = combatMapper ?? throw new ArgumentNullException(nameof(combatMapper));
    }

    public bool StartGoblinEncounter(ActionDefinition action)
    {
        if (!_combatSession.TryStartEncounter(action, out string failureReason))
        {
            _storyPanel.ShowFeedback(failureReason);
            return false;
        }

        _characterPanel.SetCharacter(new CharacterViewModel(_player));
        _gameModeController.ShowCombat();
        _combatPanel.BeginEncounter("Goblin Ambush");

        CombatSnapshot snapshot = _combatSession.GetSnapshot();
        _sidebar.EnterCombat(snapshot.Enemies);

        _combatPanel.AppendMessage(new CombatMessage(
            "goblin-encounter-start",
            1,
            CombatMessageType.System,
            CombatMessageCategory.Environment,
            "SYSTEM",
            "A goblin jumps out from the bushes and blocks your way."));
        _combatPanel.ApplyViewData(_combatMapper.Map(snapshot));
        return true;
    }

    public void HandleCombatAction(CombatActionRequest request)
    {
        if (_combatSession.IsEncounterOver)
        {
            return;
        }

        CombatResult result = _combatSession.ExecuteAction(request);
        if (result == null)
        {
            return;
        }

        CombatViewData viewData = _combatMapper.Map(result);
        _sidebar.UpdateCombatEnemies(viewData.Enemies);
        _combatPanel.ApplyViewData(viewData);
        _characterPanel.SetCharacter(new CharacterViewModel(_player));

        if (!_combatSession.IsEncounterOver)
        {
            return;
        }

        _characterPanel.SetCharacter(new CharacterViewModel(_player));
        _combatPanel.ShowEncounterComplete(_combatSession.IsVictory ? "VICTORY" : "DEFEAT");
    }

    public void ReturnToExploration(Action showLocation)
    {
        if (showLocation == null)
        {
            throw new ArgumentNullException(nameof(showLocation));
        }

        _sidebar.ExitCombat();
        _gameModeController.ShowExploration();
        _characterPanel.SetCharacter(new CharacterViewModel(_player));
        showLocation();

        if (_combatSession.TryConsumeTimeAdvance(
            out int startDay,
            out int startMinutes,
            out int endDay,
            out int endMinutes))
        {
            _storyPanel.ShowTimeAdvanceFeedback(
                startDay,
                startMinutes,
                endDay,
                endMinutes);
        }
    }
}
