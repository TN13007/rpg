using System;

public sealed class CombatFlowController
{
    private readonly StoryPanel _storyPanel;
    private readonly CombatPanel _combatPanel;
    private readonly CharacterPanel _characterPanel;
    private readonly GameModeController _gameModeController;
    private readonly Character _player;
    private readonly CombatService _combatService;
    private readonly CombatPresentationMapper _combatMapper;
    private readonly DayClock _dayClock;
    private readonly ActivityTimeCalculator _activityTimeCalculator;
    private readonly ActivityFatigueCalculator _activityFatigueCalculator;
    private readonly ActionExecutionService _actionExecutionService;

    private int _combatStartDay;
    private int _combatStartMinutes;
    private bool _hasCombatTimeFeedback;

    public CombatFlowController(
        StoryPanel storyPanel,
        CombatPanel combatPanel,
        CharacterPanel characterPanel,
        GameModeController gameModeController,
        Character player,
        CombatService combatService,
        CombatPresentationMapper combatMapper,
        DayClock dayClock,
        ActivityTimeCalculator activityTimeCalculator,
        ActivityFatigueCalculator activityFatigueCalculator,
        ActionExecutionService actionExecutionService)
    {
        _storyPanel = storyPanel ?? throw new ArgumentNullException(nameof(storyPanel));
        _combatPanel = combatPanel ?? throw new ArgumentNullException(nameof(combatPanel));
        _characterPanel = characterPanel ?? throw new ArgumentNullException(nameof(characterPanel));
        _gameModeController = gameModeController ?? throw new ArgumentNullException(nameof(gameModeController));
        _player = player ?? throw new ArgumentNullException(nameof(player));
        _combatService = combatService ?? throw new ArgumentNullException(nameof(combatService));
        _combatMapper = combatMapper ?? throw new ArgumentNullException(nameof(combatMapper));
        _dayClock = dayClock ?? throw new ArgumentNullException(nameof(dayClock));
        _activityTimeCalculator = activityTimeCalculator ?? throw new ArgumentNullException(nameof(activityTimeCalculator));
        _activityFatigueCalculator = activityFatigueCalculator ?? throw new ArgumentNullException(nameof(activityFatigueCalculator));
        _actionExecutionService = actionExecutionService ?? throw new ArgumentNullException(nameof(actionExecutionService));
    }

    public bool StartGoblinEncounter(ActionDefinition action)
    {
        if (!_actionExecutionService.CanExecute(action, _player, out string failureReason))
        {
            _storyPanel.ShowFeedback(failureReason);
            return false;
        }

        int startDay = _dayClock.Day;
        int startMinutes = _dayClock.MinutesOfDay;

        if (!_actionExecutionService.TryExecute(
            action,
            _player,
            _dayClock,
            out failureReason,
            out _))
        {
            _storyPanel.ShowFeedback(failureReason);
            return false;
        }

        _characterPanel.SetCharacter(new CharacterViewModel(_player));

        if (!_combatService.TryStartEncounter(out failureReason))
        {
            _storyPanel.ShowFeedback(failureReason);
            return false;
        }

        _characterPanel.SetCharacter(new CharacterViewModel(_player));
        _combatStartDay = startDay;
        _combatStartMinutes = startMinutes;
        _hasCombatTimeFeedback = true;
        _gameModeController.ShowCombat();
        _combatPanel.BeginEncounter("Goblin Ambush");
        _combatPanel.AppendMessage(new CombatMessage(
            "goblin-encounter-start",
            1,
            CombatMessageType.System,
            CombatMessageCategory.Environment,
            "SYSTEM",
            "A goblin jumps out from the bushes and blocks your way."));
        _combatPanel.ApplyViewData(_combatMapper.Map(_combatService.GetSnapshot()));
        return true;
    }

    public void HandleCombatAction(CombatActionRequest request)
    {
        if (_combatService.IsEncounterOver)
        {
            return;
        }

        CombatResult result = _combatService.ExecuteAction(request);
        _combatPanel.ApplyViewData(_combatMapper.Map(result));
        _characterPanel.SetCharacter(new CharacterViewModel(_player));

        if (!_combatService.IsEncounterOver)
        {
            return;
        }

        int encounterDuration = _activityTimeCalculator.Calculate(AcademyContent.CombatEncounter, _player);
        _dayClock.AdvanceMinutes(encounterDuration);
        double encounterFatigue = _activityFatigueCalculator.Calculate(
            AcademyContent.CombatEncounter,
            encounterDuration);
        _player.AddFatigue(encounterFatigue);
        _characterPanel.SetCharacter(new CharacterViewModel(_player));
        _combatPanel.ShowEncounterComplete(_combatService.IsVictory ? "VICTORY" : "DEFEAT");
    }

    public void ReturnToExploration(Action showLocation)
    {
        if (showLocation == null)
        {
            throw new ArgumentNullException(nameof(showLocation));
        }

        _gameModeController.ShowExploration();
        _characterPanel.SetCharacter(new CharacterViewModel(_player));
        showLocation();

        if (_hasCombatTimeFeedback)
        {
            _storyPanel.ShowTimeAdvanceFeedback(
                _combatStartDay,
                _combatStartMinutes,
                _dayClock.Day,
                _dayClock.MinutesOfDay);
            _hasCombatTimeFeedback = false;
        }
    }
}
