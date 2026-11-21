using System;

/// <summary>
/// Coordinates the application-level lifecycle of one combat encounter.
/// </summary>
/// <remarks>
/// The service connects the combat domain with entry-action execution and world-time costs.
/// It does not render UI; presentation reads combat snapshots and results through the owning flow.
/// </remarks>
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

    /// <summary>Gets whether no encounter is active or the combat domain has reached a terminal state.</summary>
    public bool IsEncounterOver => !_encounterStarted || _combatService.IsEncounterOver;

    /// <summary>Gets whether the current combat domain session ended in victory.</summary>
    public bool IsVictory => _combatService.IsVictory;

    /// <summary>Gets whether a living Party member is currently ready to choose an action.</summary>
    public bool IsAwaitingPlayerDecision => _combatService.IsAwaitingPlayerDecision;

    /// <summary>
    /// Creates a combat session coordinator with the services and definitions required by an encounter.
    /// </summary>
    /// <param name="player">The player character whose entry action and encounter fatigue are applied.</param>
    /// <param name="combatService">The Domain service that owns combat rules and state.</param>
    /// <param name="gameClock">The shared world clock advanced by the entry action and encounter cost.</param>
    /// <param name="activityTimeCalculator">Calculates the world-time cost of the encounter.</param>
    /// <param name="activityFatigueCalculator">Calculates the Fatigue cost of the encounter.</param>
    /// <param name="actionExecutionService">Validates and executes the action used to enter combat.</param>
    /// <param name="combatEncounterAction">The authored action definition used to calculate encounter costs.</param>
    /// <exception cref="ArgumentNullException">Any required dependency is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Attempts to execute the encounter entry action and start the combat domain session.
    /// </summary>
    /// <param name="entryAction">The action that must pass execution validation and be applied before the combat Timeline starts.</param>
    /// <param name="failureReason">Receives the validation or startup failure reason when the operation returns <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when both entry-action execution and combat startup succeed; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// The world-clock position is captured before the entry action, so the eventual feedback interval includes the entry action and the encounter's completion cost.
    /// An already active encounter cannot be started again. If validation or entry-action execution fails, combat does not start. If domain startup fails after the entry action has succeeded, the entry action's world and character changes are not rolled back.
    /// </remarks>
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

    /// <summary>Returns the latest combat-domain snapshot without advancing the Timeline or applying encounter costs.</summary>
    /// <returns>A snapshot of the current combat state, action options, intents and Timeline entries.</returns>
    public CombatSnapshot GetSnapshot()
    {
        return _combatService.GetSnapshot();
    }

    /// <summary>
    /// Executes a player combat action and applies encounter completion costs when combat ends.
    /// </summary>
    /// <param name="request">The requested combat action, actor and target information.</param>
    /// <returns>The domain result, including its updated snapshot and emitted events; returns <see langword="null"/> when no active encounter can accept an action.</returns>
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

    /// <summary>
    /// Advances combat simulation time and applies encounter completion costs when combat ends.
    /// </summary>
    /// <param name="elapsedCombatTime">Non-negative elapsed combat simulation time in seconds, forwarded unchanged to <see cref="CombatService.AdvanceCombatTime"/>.</param>
    /// <returns>The domain result, including its updated snapshot and emitted events; returns <see langword="null"/> when the encounter has not started or its completion costs have already been applied.</returns>
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

    /// <summary>
    /// Attempts to consume the world-clock interval recorded for the last successful encounter start.
    /// </summary>
    /// <param name="startDay">Receives the day before the entry action was executed.</param>
    /// <param name="startMinutes">Receives the minute of day before the entry action was executed.</param>
    /// <param name="endDay">Receives the current day after the encounter's world-time costs.</param>
    /// <param name="endMinutes">Receives the current minute of day after the encounter's world-time costs.</param>
    /// <returns><see langword="true"/> when a recorded interval is available; otherwise, <see langword="false"/> and all output values are set to zero.</returns>
    /// <remarks>The interval starts before the encounter entry action and ends after the encounter's world-time and Fatigue costs are applied. It can be consumed only once; a successful call clears the pending feedback flag. A failed call sets all four output values to zero.</remarks>
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
