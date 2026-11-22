using System;

/// <summary>Coordinates encounter entry, turn-based Domain combat and one-time encounter completion costs.</summary>
/// <remarks>The service owns world time and Fatigue application. It does not create combatants, scenes or UI view models.</remarks>
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

    /// <summary>Gets whether there is no active encounter or the Domain encounter has ended.</summary>
    public bool IsEncounterOver => !_encounterStarted || _combatService.IsEncounterOver;

    /// <summary>Gets whether the current Domain encounter ended in victory.</summary>
    public bool IsVictory => _combatService.IsVictory;

    /// <summary>Gets whether the Domain is waiting for a Player Turn decision.</summary>
    public bool IsAwaitingPlayerDecision => _combatService.IsAwaitingPlayerDecision;

    /// <summary>Creates the session boundary for the supplied player, Domain service and application services.</summary>
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

    /// <summary>Executes and validates the encounter entry action before starting the Domain encounter.</summary>
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

    /// <summary>Returns the current Domain snapshot without advancing world time.</summary>
    public CombatSnapshot GetSnapshot()
    {
        return _combatService.GetSnapshot();
    }

    /// <summary>Executes a Party action and applies the encounter cost once if it ends the encounter.</summary>
    public CombatResult ExecuteAction(CombatActionRequest request)
    {
        if (IsEncounterOver)
        {
            return null;
        }

        CombatResult result = _combatService.ExecuteAction(request);
        CompleteIfEncounterEnded();
        return result;
    }

    /// <summary>Ends the Player Turn and resolves the complete Enemy Turn synchronously.</summary>
    public CombatResult EndPlayerTurn()
    {
        if (IsEncounterOver)
        {
            return null;
        }

        CombatResult result = _combatService.EndPlayerTurn();
        CompleteIfEncounterEnded();
        return result;
    }

    /// <summary>Returns the latest encounter snapshot.</summary>
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

    private void CompleteIfEncounterEnded()
    {
        if (_combatService.IsEncounterOver && !_encounterCompleted)
        {
            CompleteEncounter();
        }
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