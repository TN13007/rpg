using System;

public sealed class CombatSessionService
{
    private readonly Character _player;
    private readonly CombatService _combatService;
    private readonly GameClock _gameClock;
    private readonly ActivityTimeCalculator _activityTimeCalculator;
    private readonly ActivityFatigueCalculator _activityFatigueCalculator;
    private readonly ActionExecutionService _actionExecutionService;
    private readonly ActionDefinition _combatEncounterAction;

    private int _combatStartDay;
    private int _combatStartMinutes;
    private bool _hasCombatTimeFeedback;
    private bool _encounterStarted;
    private bool _encounterCompleted;

    public bool IsEncounterOver => !_encounterStarted || _combatService.IsEncounterOver;
    public bool IsVictory => _combatService.IsVictory;
    public bool IsAwaitingPlayerDecision => _combatService.IsAwaitingPlayerDecision;

    public CombatSessionService(
        Character player,
        CombatService combatService,
        GameClock gameClock,
        ActivityTimeCalculator activityTimeCalculator,
        ActivityFatigueCalculator activityFatigueCalculator,
        ActionExecutionService actionExecutionService,
        ActionDefinition combatEncounterAction)
    {
        _player = player ?? throw new ArgumentNullException(nameof(player));
        _combatService = combatService ?? throw new ArgumentNullException(nameof(combatService));
        _gameClock = gameClock ?? throw new ArgumentNullException(nameof(gameClock));
        _activityTimeCalculator = activityTimeCalculator ?? throw new ArgumentNullException(nameof(activityTimeCalculator));
        _activityFatigueCalculator = activityFatigueCalculator ?? throw new ArgumentNullException(nameof(activityFatigueCalculator));
        _actionExecutionService = actionExecutionService ?? throw new ArgumentNullException(nameof(actionExecutionService));
        _combatEncounterAction = combatEncounterAction ?? throw new ArgumentNullException(nameof(combatEncounterAction));
    }

    public bool TryStartEncounter(ActionDefinition entryAction, out string failureReason)
    {
        if (_encounterStarted && !_encounterCompleted)
        {
            failureReason = "An encounter is already active.";
            return false;
        }

        if (!_actionExecutionService.CanExecute(entryAction, _player, out failureReason))
        {
            return false;
        }

        int startDay = _gameClock.Day;
        int startMinutes = _gameClock.MinutesOfDay;

        if (!_actionExecutionService.TryExecute(
            entryAction,
            _player,
            _gameClock,
            out failureReason,
            out _))
        {
            return false;
        }

        if (!_combatService.TryStartEncounter(out failureReason))
        {
            return false;
        }

        _combatStartDay = startDay;
        _combatStartMinutes = startMinutes;
        _hasCombatTimeFeedback = true;
        _encounterStarted = true;
        _encounterCompleted = false;
        return true;
    }

    public CombatSnapshot GetSnapshot()
    {
        return _combatService.GetSnapshot();
    }

    public CombatResult ExecuteAction(CombatActionRequest request)
    {
        if (IsEncounterOver)
        {
            return null;
        }

        CombatResult result = _combatService.ExecuteAction(request);

        if (_combatService.IsEncounterOver && !_encounterCompleted)
        {
            CompleteEncounter();
        }

        return result;
    }

    public CombatResult AdvanceCombatTime(double elapsedCombatTime)
    {
        if (!_encounterStarted || _encounterCompleted)
        {
            return null;
        }

        CombatResult result = _combatService.AdvanceCombatTime(elapsedCombatTime);

        if (_combatService.IsEncounterOver && !_encounterCompleted)
        {
            CompleteEncounter();
        }

        return result;
    }

    public bool TryConsumeTimeAdvance(
        out int startDay,
        out int startMinutes,
        out int endDay,
        out int endMinutes)
    {
        if (!_hasCombatTimeFeedback)
        {
            startDay = 0;
            startMinutes = 0;
            endDay = 0;
            endMinutes = 0;
            return false;
        }

        startDay = _combatStartDay;
        startMinutes = _combatStartMinutes;
        endDay = _gameClock.Day;
        endMinutes = _gameClock.MinutesOfDay;
        _hasCombatTimeFeedback = false;
        return true;
    }

    private void CompleteEncounter()
    {
        int encounterDuration = _activityTimeCalculator.Calculate(_combatEncounterAction, _player);
        _gameClock.AdvanceMinutes(encounterDuration);
        double encounterFatigue = _activityFatigueCalculator.Calculate(
            _combatEncounterAction,
            encounterDuration);
        _player.AddFatigue(encounterFatigue);
        _encounterCompleted = true;
    }
}
