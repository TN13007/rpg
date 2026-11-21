/// <summary>
/// Coordinates a combat encounter with the wider game clock and activity resource systems.
/// </summary>
/// <remarks>
/// This application-layer service validates and starts an encounter, delegates combat rules to
/// <see cref="CombatService"/>, and applies the encounter's time and fatigue cost when it ends.
/// It does not own the combat timeline or render the combat interface.
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

    /// <summary>Gets whether no active encounter remains.</summary>
    public bool IsEncounterOver => !_encounterStarted || _combatService.IsEncounterOver;

    /// <summary>Gets whether the underlying combat service has declared victory.</summary>
    public bool IsVictory => _combatService.IsVictory;

    /// <summary>Gets whether the player must choose an action for the active party member.</summary>
    public bool IsAwaitingPlayerDecision => _combatService.IsAwaitingPlayerDecision;

    /// <summary>
    /// Creates a session coordinator with the services required to start and settle encounters.
    /// </summary>
    /// <param name="player">The player character whose activity resources are affected by the encounter.</param>
    /// <param name="combatService">The domain service that owns combat state and resolution.</param>
    /// <param name="gameClock">The world clock advanced when the encounter is completed.</param>
    /// <param name="activityTimeCalculator">Calculates the encounter's time cost.</param>
    /// <param name="activityFatigueCalculator">Calculates fatigue from the encounter's duration.</param>
    /// <param name="actionExecutionService">Validates and executes the action used to enter the encounter.</param>
    /// <param name="combatEncounterAction">The action definition used to calculate encounter time and fatigue costs.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required dependency is null.</exception>
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
    /// Validates and starts an encounter, charging the entry action before initializing combat.
    /// </summary>
    /// <param name="entryAction">The action that represents entering this encounter.</param>
    /// <param name="failureReason">Receives the reason the encounter could not start, or an empty string on success.</param>
    /// <returns><see langword="true"/> when the entry action and combat initialization both succeed.</returns>
    /// <remarks>
    /// The entry action is executed before <see cref="CombatService.TryStartEncounter"/> is called.
    /// If combat initialization fails after the entry action succeeds, this method does not roll back
    /// the entry action's effects.
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

    /// <summary>Gets the current combat snapshot for presentation.</summary>
    /// <returns>A snapshot of the current combat state.</returns>
    /// <remarks>This is a read operation; it delegates snapshot construction to the combat domain service.</remarks>
    public CombatSnapshot GetSnapshot()
    {
        return _combatService.GetSnapshot();
    }

    /// <summary>Executes the selected combat action and completes the session if combat ends.</summary>
    /// <param name="request">The action and target selected for the active combatant.</param>
    /// <returns>The combat result, or <see langword="null"/> if the encounter is already over.</returns>
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

    /// <summary>Advances combat time and completes the session if the encounter ends during advancement.</summary>
    /// <param name="elapsedCombatTime">The amount of combat time to advance, in the combat timeline's time unit.</param>
    /// <returns>The combat result, or <see langword="null"/> when the session has not started or is already completed.</returns>
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
    /// Returns the world-clock interval consumed by the encounter, if that feedback is pending.
    /// </summary>
    /// <param name="startDay">Receives the world day on which the encounter started.</param>
    /// <param name="startMinutes">Receives the time of day in minutes when the encounter started.</param>
    /// <param name="endDay">Receives the current world day.</param>
    /// <param name="endMinutes">Receives the current time of day in minutes.</param>
    /// <returns><see langword="true"/> if pending time feedback was consumed; otherwise, <see langword="false"/>.</returns>
    /// <remarks>Calling this method successfully consumes the pending feedback; subsequent calls return false until another encounter starts.</remarks>
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
