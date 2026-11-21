using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Owns the combat roster and resolves encounter rules, player actions, enemy intents and Timeline progression.</summary>
/// <remarks>Mutates the supplied <see cref="Character"/> instances during combat. Application and presentation layers should use this service as the authoritative source of combat state rather than duplicating combat rules.</remarks>
public sealed class CombatService : ICombatService
{
    private const int _victoryReward = 25;
    private const int _revivalStaminaCost = 4;
    private const string _reviveActionId = "revive_ally";
    private const double _initialProgress = 100;
    private const double _maximumRecoveryTimeSeconds = 4.0;

    private sealed class CombatantRuntime
    {
        /// <summary>Gets the character.</summary>
        public Character Character { get; }
        /// <summary>Gets the side.</summary>
        public CombatantSide Side { get; }
        /// <summary>Gets a value indicating whether visible.</summary>
        public bool IsVisible { get; }
        /// <summary>Gets the behavior profile.</summary>
        public EnemyBehaviorProfile BehaviorProfile { get; }
        /// <summary>Gets the committed action.</summary>
        public CombatActionDefinition CommittedAction { get; set; }
        /// <summary>Gets the committed target id.</summary>
        public string CommittedTargetId { get; set; } = string.Empty;
        /// <summary>Gets the action order.</summary>
        public List<CombatActionDefinition> ActionOrder { get; }
        public Dictionary<string, CombatActionDefinition> Actions { get; }
        /// <summary>Gets the effect state.</summary>
        public CombatantEffectState EffectState { get; }
        /// <summary>Gets a value indicating whether downed.</summary>
        public bool IsDowned { get; set; }
        /// <summary>Gets a value indicating whether been revived.</summary>
        public bool HasBeenRevived { get; set; }

        /// <summary>Initializes a new instance of <see cref="CombatantRuntime"/>.</summary>
        /// <param name="definition">The definition used to initialize the instance.</param>
        public CombatantRuntime(CombatantDefinition definition)
        {
            Character = definition.Character;
            Side = definition.Side;
            IsVisible = definition.IsVisible;
            BehaviorProfile = definition.BehaviorProfile;
            ActionOrder = definition.Actions.ToList();
            Actions = new Dictionary<string, CombatActionDefinition>(StringComparer.Ordinal);
            foreach (CombatActionDefinition action in ActionOrder)
            {
                Actions.Add(action.Id, action);
            }

            EffectState = new CombatantEffectState();
            IsDowned = Side == CombatantSide.Party && !Character.IsAlive;
        }

        /// <summary>Gets a value indicating whether enemy.</summary>
        public bool IsEnemy => Side == CombatantSide.Enemy;
        /// <summary>Gets a value indicating whether party member.</summary>
        public bool IsPartyMember => Side == CombatantSide.Party;
    }

    private readonly Dictionary<string, CombatantRuntime> _combatants =
        new Dictionary<string, CombatantRuntime>(StringComparer.Ordinal);
    private readonly List<CombatantRuntime> _party = new List<CombatantRuntime>();
    private readonly List<CombatantRuntime> _enemies = new List<CombatantRuntime>();
    private readonly Random _enemyRandom;
    private readonly CombatActionResolver _actionResolver = new CombatActionResolver();
    private readonly CombatEffectDispatcher _effectDispatcher = new CombatEffectDispatcher();
    private readonly Character _rewardRecipient;

    private CombatTimeline _timeline;
    private CombatActionDefinition _reviveAction;
    private double _revivalRecoveryCost;
    private string _activeCombatantId = string.Empty;
    private int _round;
    private bool _encounterOver;

    /// <summary>Gets whether the most recently started encounter ended in victory.</summary>
    /// <remarks>Remains <see langword="false"/> before an encounter starts and after a defeat.</remarks>
    public bool IsVictory { get; private set; }
    /// <summary>Gets whether the encounter has reached a terminal victory or defeat state.</summary>
    public bool IsEncounterOver => _encounterOver;
    /// <summary>Gets whether a living, able Party member is currently ready for a player decision.</summary>
    public bool IsAwaitingPlayerDecision =>
        !_encounterOver &&
        _combatants.TryGetValue(_activeCombatantId, out CombatantRuntime active) &&
        active.IsPartyMember &&
        !active.IsDowned &&
        active.Character.CanAct;
    /// <summary>Gets the character ID whose action window is currently active, or an empty string while the Timeline is running.</summary>
    public string ActiveCombatantId => _activeCombatantId;
    /// <summary>Gets elapsed combat simulation time in seconds; returns zero before the Timeline is created.</summary>
    public double CombatTime => _timeline == null ? 0 : _timeline.CombatTime;
    /// <summary>Gets the gold reward granted for a victory, or zero when the encounter has not been won.</summary>
    public int RewardGold => IsVictory ? _victoryReward : 0;
    /// <summary>Gets the Party characters in their configured encounter order.</summary>
    /// <remarks>The returned collection is a new read-only view of the roster; the contained <see cref="Character"/> objects are the live combat instances.</remarks>
    public IReadOnlyList<Character> PartyMembers => _party.Select(item => item.Character).ToArray();

    /// <summary>Creates a one-Party-member, one-enemy encounter using the built-in default enemy actions.</summary>
    /// <param name="player">The Party character whose stats and state are used during combat.</param>
    /// <param name="enemy">The enemy character whose stats and state are used during combat.</param>
    /// <param name="playerActions">The combat actions available to the player character.</param>
    public CombatService(Character player, Character enemy, IEnumerable<CombatActionDefinition> playerActions)
        : this(player, enemy, playerActions, CreateDefaultEnemyActions(), null)
    {
    }

    /// <summary>Creates a one-Party-member, one-enemy encounter with explicitly supplied action sets.</summary>
    /// <param name="player">The Party character whose stats and state are used during combat.</param>
    /// <param name="enemy">The enemy character whose stats and state are used during combat.</param>
    /// <param name="playerActions">The actions the player character may select during its action window.</param>
    /// <param name="enemyActions">The actions the enemy AI may select when planning and resolving its turns.</param>
    /// <param name="enemyRandom">Optional random source for enemy action and target selection. Supply a seeded instance when deterministic combat tests are required.</param>
    public CombatService(
        Character player,
        Character enemy,
        IEnumerable<CombatActionDefinition> playerActions,
        IEnumerable<CombatActionDefinition> enemyActions,
        Random enemyRandom = null)
        : this(
            new[]
            {
                new CombatantDefinition(player, CombatantSide.Party, playerActions),
                new CombatantDefinition(enemy, CombatantSide.Enemy, enemyActions)
            },
            enemyRandom)
    {
    }

    /// <summary>Creates an encounter from a roster of Party members and enemies.</summary>
    /// <param name="combatants">The complete encounter roster. Each definition supplies a character, side, visibility, actions and optional enemy behavior profile.</param>
    /// <param name="enemyRandom">Optional random source used by enemy decision-making; pass a seeded instance to make those decisions reproducible.</param>
    /// <remarks>The roster must contain at least one Party member and one enemy, and every character ID must be unique. The service retains and mutates the supplied character instances.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="combatants"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">A definition is null, a character ID is duplicated, or the roster has no Party member or no enemy.</exception>
    public CombatService(IEnumerable<CombatantDefinition> combatants, Random enemyRandom = null)
    {
        if (combatants == null)
        {
            throw new ArgumentNullException(nameof(combatants));
        }

        foreach (CombatantDefinition definition in combatants)
        {
            if (definition == null)
            {
                throw new ArgumentException("Combatant definitions cannot contain null values.", nameof(combatants));
            }

            CombatantRuntime runtime = new CombatantRuntime(definition);
            if (!_combatants.TryAdd(runtime.Character.Id, runtime))
            {
                throw new ArgumentException("Duplicate combatant ID: " + runtime.Character.Id, nameof(combatants));
            }

            if (runtime.IsPartyMember)
            {
                _party.Add(runtime);
            }
            else
            {
                _enemies.Add(runtime);
            }
        }

        if (_party.Count == 0)
        {
            throw new ArgumentException("An encounter requires at least one party combatant.", nameof(combatants));
        }

        if (_enemies.Count == 0)
        {
            throw new ArgumentException("An encounter requires at least one enemy combatant.", nameof(combatants));
        }

        _rewardRecipient = _party[0].Character;
        _enemyRandom = enemyRandom ?? new Random();
    }

    /// <summary>Resets encounter runtime state and initializes the Timeline for a new combat session.</summary>
    /// <param name="failureReason">Receives an explanation when startup is rejected; is an empty string when startup succeeds.</param>
    /// <returns><see langword="true"/> when at least one living Party member and one living enemy can enter the encounter; otherwise, <see langword="false"/>.</returns>
    /// <remarks>Enemy characters are reset, all combat effect state and committed enemy intents are cleared, and a new Timeline is created. Party character stats are not reset. Enemy intents are planned before the first player decision.</remarks>
    public bool TryStartEncounter(out string failureReason)
    {
        if (!_party.Any(item => item.Character.IsAlive))
        {
            _encounterOver = true;
            IsVictory = false;
            failureReason = "At least one party member must be able to fight.";
            return false;
        }

        foreach (CombatantRuntime combatant in _combatants.Values)
        {
            if (combatant.IsEnemy)
            {
                combatant.Character.Reset();
            }

            combatant.EffectState.Reset();
            combatant.IsDowned = combatant.IsPartyMember && !combatant.Character.IsAlive;
            combatant.HasBeenRevived = false;
            combatant.CommittedAction = null;
            combatant.CommittedTargetId = string.Empty;
        }

        if (!_enemies.Any(item => item.Character.IsAlive))
        {
            _encounterOver = true;
            IsVictory = false;
            failureReason = "The encounter requires at least one living enemy.";
            return false;
        }

        double progressScale = CalculateProgressScale(out _revivalRecoveryCost);
        _reviveAction = CreateReviveAction(_revivalRecoveryCost);
        _timeline = new CombatTimeline(progressScale);
        foreach (CombatantRuntime combatant in _combatants.Values)
        {
            if (!combatant.Character.IsAlive || combatant.IsDowned)
            {
                continue;
            }

            _timeline.AddCombatant(
                combatant.Character.Id,
                combatant.Character.Stats.Luck,
                GetBaseSpeed(combatant.Character),
                _initialProgress);
        }

        foreach (CombatantRuntime enemy in _enemies.Where(item => item.Character.IsAlive))
        {
            PlanEnemyIntent(enemy);
        }

        _round = 1;
        _encounterOver = false;
        IsVictory = false;
        _activeCombatantId = string.Empty;
        failureReason = string.Empty;

        List<CombatEvent> startEvents = new List<CombatEvent>();
        ResolveTerminalConditions(startEvents);

        return !_encounterOver;
    }

    /// <summary>Builds a read-only snapshot of the current encounter for application and presentation layers.</summary>
    /// <returns>A snapshot containing the active decision window, available Party actions and targets, enemy status and intent, Party status, and Timeline order.</returns>
    /// <remarks>Calling this method does not advance combat time or mutate encounter state. Before startup, the snapshot reflects the service's uninitialized combat state.</remarks>
    public CombatSnapshot GetSnapshot()
    {
        CombatantRuntime active = null;
        if (!string.IsNullOrWhiteSpace(_activeCombatantId))
        {
            _combatants.TryGetValue(_activeCombatantId, out active);
        }

        List<CombatActionOption> availableActions = new List<CombatActionOption>();
        if (active != null && active.IsPartyMember && active.Character.CanAct)
        {
            foreach (CombatActionDefinition action in active.ActionOrder)
            {
                bool available = IsActionAvailable(action, active.Character);
                if (RequiresTarget(action))
                {
                    CombatActionTargetOption[] targets = _enemies
                        .Where(item => item.Character.IsAlive && item.IsVisible)
                        .Select(item => new CombatActionTargetOption(
                            item.Character.Id,
                            item.Character.Name))
                        .ToArray();

                    if (targets.Length == 0)
                    {
                        continue;
                    }

                    availableActions.Add(new CombatActionOption(
                        action.Id,
                        action.Label,
                        action.CostLabel,
                        action.Description,
                        available,
                        available ? string.Empty : GetUnavailableReason(action),
                        active.Character.Id,
                        targets));
                }
                else
                {
                    availableActions.Add(new CombatActionOption(
                        action.Id,
                        action.Label,
                        action.CostLabel,
                        action.Description,
                        available,
                        available ? string.Empty : GetUnavailableReason(action),
                        active.Character.Id));
                }
            }
        }

        if (active != null && active.IsPartyMember && active.Character.CanAct && !active.IsDowned)
        {
            CombatActionTargetOption[] reviveTargets = _party
                .Where(item =>
                    item.Character.Id != active.Character.Id &&
                    item.IsDowned &&
                    !item.HasBeenRevived)
                .Select(item => new CombatActionTargetOption(
                    item.Character.Id,
                    item.Character.Name))
                .ToArray();

            if (reviveTargets.Length > 0)
            {
                bool available = IsActionAvailable(_reviveAction, active.Character);
                availableActions.Add(new CombatActionOption(
                    _reviveAction.Id,
                    _reviveAction.Label,
                    _reviveAction.CostLabel,
                    _reviveAction.Description,
                    available,
                    available ? string.Empty : GetUnavailableReason(_reviveAction),
                    active.Character.Id,
                    reviveTargets));
            }
        }

        PartyCombatStatus[] partyStatuses = _party
            .Select(item => new PartyCombatStatus(
                item.Character.Id,
                item.Character.Name,
                item.Character.Health,
                item.Character.MaxHealth,
                GetCharacterCondition(item),
                item.Character.Id == _activeCombatantId))
            .ToArray();

        TimelineCombatantStatus[] timelineEntries = GetTimelineEntries();
        Dictionary<string, string> enemyIntents = timelineEntries
            .Where(entry => entry.IsEnemy && !string.IsNullOrWhiteSpace(entry.IntentDescription))
            .ToDictionary(entry => entry.Id, entry => entry.IntentDescription, StringComparer.Ordinal);

        EnemyCombatStatus[] enemyStatuses = _enemies
            .Select(item => new EnemyCombatStatus(
                item.Character.Id,
                item.Character.Name,
                item.Character.Health,
                item.Character.MaxHealth,
                GetEnemyCondition(item.Character),
                item.IsVisible,
                enemyIntents.TryGetValue(item.Character.Id, out string intentDescription)
                    ? intentDescription
                    : string.Empty))
            .ToArray();

        string turnLabel = _encounterOver
            ? "ENCOUNTER COMPLETE"
            : active != null && active.IsPartyMember
                ? "PARTY DECISION · " + active.Character.Name.ToUpperInvariant()
                : "TIMELINE RUNNING";

        return new CombatSnapshot(
            _round,
            turnLabel,
            enemyStatuses,
            GetPrimaryEnemyIntent(),
            availableActions.ToArray(),
            timelineEntries,
            partyStatuses,
            _activeCombatantId);
    }

    /// <summary>Validates and resolves the active Party member's requested combat action.</summary>
    /// <param name="request">The action ID, optional acting character ID, and target character ID. When <see cref="CombatActionRequest.ActorId"/> is omitted, the currently active Party member is used.</param>
    /// <returns>A result containing success or failure, the updated combat snapshot, and any events produced by a successful action.</returns>
    /// <remarks>Actions can be submitted only during a Party decision window. Targeted actions require a living, visible enemy. A successful action spends Stamina, applies its effects and recovery cost, then resumes Timeline scheduling. Invalid requests return a failed result without applying the action.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    public CombatResult ExecuteAction(CombatActionRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (_encounterOver)
        {
            return Failure("The encounter has already ended.");
        }

        if (!IsAwaitingPlayerDecision)
        {
            return Failure("It is not the player's action window.");
        }

        CombatantRuntime actor = _combatants[_activeCombatantId];
        if (!string.IsNullOrWhiteSpace(request.ActorId) && request.ActorId != actor.Character.Id)
        {
            return Failure("The requested character is not ready to act.");
        }

        if (!actor.Character.CanAct)
        {
            return Failure("This party member cannot act.");
        }

        if (_reviveAction != null && request.ActionId == _reviveAction.Id)
        {
            return ExecuteReviveAction(actor, request.TargetId);
        }

        if (!actor.Actions.TryGetValue(request.ActionId, out CombatActionDefinition action))
        {
            return Failure("That action is not available to this party member.");
        }

        if (!IsActionAvailable(action, actor.Character))
        {
            return Failure(GetUnavailableReason(action));
        }

        CombatantRuntime target = actor;
        if (RequiresTarget(action))
        {
            if (string.IsNullOrWhiteSpace(request.TargetId) ||
                !_combatants.TryGetValue(request.TargetId, out target) ||
                !target.IsEnemy ||
                !target.Character.IsAlive ||
                !target.IsVisible)
            {
                return Failure("Choose a visible, living enemy as your target.");
            }
        }

        if (!actor.Character.SpendStamina(action.StaminaCost))
        {
            return Failure(GetUnavailableReason(action));
        }

        List<CombatEvent> events = new List<CombatEvent>();
        events.Add(new CombatEvent(
            CombatEventType.PlayerAction,
            _round,
            actorName: actor.Character.Name,
            targetName: target.Character.Id == actor.Character.Id ? string.Empty : target.Character.Name,
            detail: action.ActionDetail,
            actionId: action.Id));

        ApplyAction(action, actor, target, events);
        ResolveTerminalConditions(events);

        if (!_encounterOver && actor.Character.IsAlive && _timeline.ContainsCombatant(actor.Character.Id))
        {
            _timeline.ApplyRecovery(actor.Character.Id, action.RecoveryCost);
        }

        if (!_encounterOver)
        {
            _activeCombatantId = string.Empty;
        }

        return Success(events);
    }

    /// <summary>Advances the combat Timeline and resolves enemy turns until the elapsed-time budget is consumed or a Party decision becomes available.</summary>
    /// <param name="elapsedCombatTime">Non-negative elapsed simulation time in seconds, normally the frame delta supplied by Godot.</param>
    /// <returns>A successful result containing the latest snapshot and all enemy/status events resolved during this advance.</returns>
    /// <remarks>No time is advanced before Timeline initialization, after encounter completion, or while a Party member is waiting for input. The method may resolve multiple enemy turns in one call. A zero value still allows already-ready Timeline events to be resolved.</remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="elapsedCombatTime"/> is negative, NaN or infinite.</exception>
    /// <exception cref="InvalidOperationException">The scheduler exceeds its safety limit or the Timeline reports an invalid future event.</exception>
    public CombatResult AdvanceCombatTime(double elapsedCombatTime)
    {
        if (double.IsNaN(elapsedCombatTime) ||
            double.IsInfinity(elapsedCombatTime) ||
            elapsedCombatTime < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedCombatTime));
        }

        List<CombatEvent> events = new List<CombatEvent>();
        if (_timeline == null || _encounterOver || IsAwaitingPlayerDecision)
        {
            return Success(events);
        }

        double remainingElapsed = elapsedCombatTime;
        int safetyCounter = 0;

        while (!_encounterOver)
        {
            safetyCounter++;
            if (safetyCounter > 1000)
            {
                throw new InvalidOperationException("Combat scheduler exceeded its safety limit.");
            }

            ResolveTerminalConditions(events);
            if (_encounterOver)
            {
                break;
            }

            string readyId = _timeline.GetNextReadyCombatantId();
            if (!string.IsNullOrEmpty(readyId))
            {
                if (!_combatants.TryGetValue(readyId, out CombatantRuntime readyCombatant) ||
                    !readyCombatant.Character.IsAlive)
                {
                    _timeline.RemoveCombatant(readyId);
                    continue;
                }

                if (readyCombatant.IsPartyMember)
                {
                    _activeCombatantId = readyCombatant.Character.Id;
                    break;
                }

                double recoveryCost = ResolveEnemyAction(readyCombatant, events);
                ResolveTerminalConditions(events);
                if (_encounterOver)
                {
                    break;
                }

                if (readyCombatant.Character.IsAlive &&
                    _timeline.ContainsCombatant(readyCombatant.Character.Id))
                {
                    _timeline.ApplyRecovery(readyCombatant.Character.Id, recoveryCost);
                }

                _round++;
                continue;
            }

            if (remainingElapsed <= 0.0)
            {
                break;
            }

            double? timeUntilNextEvent = _timeline.GetTimeUntilNextEvent();
            if (!timeUntilNextEvent.HasValue)
            {
                throw new InvalidOperationException("Combat Timeline has no future event.");
            }

            double elapsedToAdvance = Math.Min(remainingElapsed, timeUntilNextEvent.Value);
            if (elapsedToAdvance <= 0.0)
            {
                throw new InvalidOperationException("Combat Timeline returned a non-positive future event.");
            }

            _timeline.AdvanceTime(elapsedToAdvance);
            remainingElapsed = Math.Max(0.0, remainingElapsed - elapsedToAdvance);
        }

        return Success(events);
    }

    private CombatResult ExecuteReviveAction(CombatantRuntime actor, string targetId)
    {
        if (string.IsNullOrWhiteSpace(targetId) ||
            !_combatants.TryGetValue(targetId, out CombatantRuntime target) ||
            !target.IsPartyMember ||
            !target.IsDowned ||
            target.HasBeenRevived ||
            target.Character.Id == actor.Character.Id)
        {
            return Failure("Choose an ally who is Downed and has not already been revived this encounter.");
        }

        if (!IsActionAvailable(_reviveAction, actor.Character))
        {
            return Failure(GetUnavailableReason(_reviveAction));
        }

        if (!actor.Character.SpendStamina(_reviveAction.StaminaCost))
        {
            return Failure(GetUnavailableReason(_reviveAction));
        }

        List<CombatEvent> events = new List<CombatEvent>
        {
            new CombatEvent(
                CombatEventType.PlayerAction,
                _round,
                actorName: actor.Character.Name,
                targetName: target.Character.Name,
                detail: _reviveAction.ActionDetail,
                actionId: _reviveAction.Id)
        };

        ApplyAction(_reviveAction, actor, target, events);
        ResolveTerminalConditions(events);

        if (!_encounterOver &&
            actor.Character.IsAlive &&
            _timeline.ContainsCombatant(actor.Character.Id))
        {
            _timeline.ApplyRecovery(actor.Character.Id, _reviveAction.RecoveryCost);
        }

        if (!_encounterOver)
        {
            _activeCombatantId = string.Empty;
        }

        return Success(events);
    }

    private double ResolveEnemyAction(CombatantRuntime actor, List<CombatEvent> events)
    {
        CombatActionDefinition action = actor.CommittedAction ?? SelectEnemyAction(actor);
        string committedTargetId = actor.CommittedTargetId;
        actor.CommittedAction = null;
        actor.CommittedTargetId = string.Empty;

        if (!actor.Character.SpendStamina(action.StaminaCost))
        {
            events.Add(new CombatEvent(
                CombatEventType.EnemyAction,
                _round,
                actorName: actor.Character.Name.ToUpperInvariant(),
                detail: actor.Character.Name + " cannot afford its committed action and loses the opportunity."));
            PlanEnemyIntent(actor);
            return action.RecoveryCost;
        }

        CombatantRuntime target = actor;
        if (RequiresTarget(action))
        {
            if (string.IsNullOrWhiteSpace(committedTargetId) ||
                !_combatants.TryGetValue(committedTargetId, out target) ||
                !target.IsPartyMember ||
                !target.Character.IsAlive ||
                target.IsDowned)
            {
                events.Add(new CombatEvent(
                    CombatEventType.EnemyAction,
                    _round,
                    actorName: actor.Character.Name.ToUpperInvariant(),
                    detail: actor.Character.Name + " loses its committed action because the target is no longer available."));
                PlanEnemyIntent(actor);
                return action.RecoveryCost;
            }
        }

        events.Add(new CombatEvent(
            CombatEventType.EnemyAction,
            _round,
            actorName: actor.Character.Name.ToUpperInvariant(),
            targetName: target.Character.Id == actor.Character.Id ? string.Empty : target.Character.Name,
            detail: action.ActionDetail,
            actionId: action.Id));

        ApplyAction(action, actor, target, events);
        PlanEnemyIntent(actor);
        return action.RecoveryCost;
    }

    private CombatActionDefinition SelectEnemyAction(CombatantRuntime enemy)
    {
        List<CombatActionDefinition> availableActions = enemy.ActionOrder
            .Where(action => IsActionAvailable(action, enemy.Character))
            .ToList();

        if (availableActions.Count == 0)
        {
            return enemy.ActionOrder[0];
        }

        if (enemy.BehaviorProfile == null)
        {
            return availableActions[_enemyRandom.Next(availableActions.Count)];
        }

        int totalWeight = availableActions.Sum(action => enemy.BehaviorProfile.GetWeight(action.Id));
        if (totalWeight <= 0)
        {
            return availableActions[0];
        }

        int selection = _enemyRandom.Next(totalWeight);
        foreach (CombatActionDefinition action in availableActions)
        {
            selection -= enemy.BehaviorProfile.GetWeight(action.Id);
            if (selection < 0)
            {
                return action;
            }
        }

        return availableActions[availableActions.Count - 1];
    }

    private void PlanEnemyIntent(CombatantRuntime enemy)
    {
        enemy.CommittedAction = SelectEnemyAction(enemy);
        enemy.CommittedTargetId = string.Empty;

        if (!RequiresTarget(enemy.CommittedAction))
        {
            return;
        }

        List<CombatantRuntime> validTargets = _party
            .Where(item => item.Character.IsAlive && !item.IsDowned)
            .ToList();

        if (validTargets.Count == 0)
        {
            enemy.CommittedAction = null;
            return;
        }

        int targetIndex = validTargets.Count == 1 ? 0 : _enemyRandom.Next(validTargets.Count);
        enemy.CommittedTargetId = validTargets[targetIndex].Character.Id;
    }

    private string GetEnemyIntentDescription(CombatantRuntime enemy)
    {
        if (enemy.CommittedAction == null)
        {
            return string.Empty;
        }

        string description = enemy.CommittedAction.Label;
        if (!string.IsNullOrWhiteSpace(enemy.CommittedTargetId) &&
            _combatants.TryGetValue(enemy.CommittedTargetId, out CombatantRuntime target))
        {
            description += " → " + target.Character.Name;
        }

        return description;
    }

    private IntentObservation GetPrimaryEnemyIntent()
    {
        TimelineCombatantStatus nextEnemy = GetTimelineEntries()
            .Where(entry => entry.IsEnemy && entry.IsVisible)
            .OrderBy(entry => entry.RemainingStunTime + entry.RemainingProgress / entry.Speed)
            .ThenByDescending(entry => entry.Luck)
            .ThenBy(entry => entry.Id, StringComparer.Ordinal)
            .FirstOrDefault();

        if (nextEnemy == null ||
            !_combatants.TryGetValue(nextEnemy.Id, out CombatantRuntime enemy) ||
            enemy.CommittedAction == null)
        {
            return null;
        }

        return new IntentObservation(
            enemy.Character.Id,
            enemy.Character.Name,
            GetEnemyIntentDescription(enemy),
            IntentConfidence.Recognized);
    }

    private void ApplyAction(
        CombatActionDefinition action,
        CombatantRuntime actor,
        CombatantRuntime target,
        List<CombatEvent> events)
    {
        CombatEffectRequest[] requests = _actionResolver.Resolve(
            action,
            actor.Character.Id,
            target.Character.Id);

        foreach (CombatEffectRequest request in requests)
        {
            if (!_combatants.TryGetValue(request.TargetId, out CombatantRuntime effectTarget))
            {
                continue;
            }

            _effectDispatcher.Apply(
                request,
                actor.Character,
                effectTarget.Character,
                _timeline,
                actor.EffectState,
                effectTarget.EffectState,
                events,
                _round);

            if (effectTarget.IsPartyMember && effectTarget.IsDowned && effectTarget.Character.IsAlive)
            {
                effectTarget.IsDowned = false;
                effectTarget.HasBeenRevived = true;
                effectTarget.EffectState.Reset();
                _timeline.AddCombatant(
                    effectTarget.Character.Id,
                    effectTarget.Character.Stats.Luck,
                    GetBaseSpeed(effectTarget.Character),
                    _revivalRecoveryCost);
            }
            else if (!effectTarget.Character.IsAlive)
            {
                _timeline.RemoveCombatant(effectTarget.Character.Id);
                if (effectTarget.IsPartyMember && !effectTarget.IsDowned)
                {
                    effectTarget.IsDowned = true;
                    effectTarget.EffectState.Reset();
                    events.Add(new CombatEvent(
                        CombatEventType.StatusChanged,
                        _round,
                        targetName: effectTarget.Character.Name,
                        detail: effectTarget.Character.Name + " is Downed and leaves the Timeline."));
                }
            }
        }
    }

    private void ResolveTerminalConditions(List<CombatEvent> events)
    {
        if (_encounterOver)
        {
            return;
        }

        if (!_party.Any(item => item.Character.IsAlive && !item.IsDowned))
        {
            CompleteDefeat(events);
            return;
        }

        if (!_enemies.Any(item => item.Character.IsAlive))
        {
            CompleteVictory(events);
        }
    }

    private bool IsActionAvailable(CombatActionDefinition action, Character actor)
    {
        if (action.Behavior == CombatActionBehavior.RecoverStamina)
        {
            return actor.Stamina < actor.MaxStamina;
        }

        return actor.Stamina >= action.StaminaCost;
    }

    private static bool RequiresTarget(CombatActionDefinition action)
    {
        return action.Behavior == CombatActionBehavior.Attack ||
               action.Behavior == CombatActionBehavior.Delay ||
               action.Behavior == CombatActionBehavior.Stun ||
               action.Behavior == CombatActionBehavior.Revive;
    }

    private static string GetUnavailableReason(CombatActionDefinition action)
    {
        return string.IsNullOrWhiteSpace(action.UnavailableReason)
            ? "This action is currently unavailable."
            : action.UnavailableReason;
    }

    private double CalculateProgressScale(out double largestRecoveryCost)
    {
        CombatantRuntime[] activeCombatants = _combatants.Values
            .Where(combatant => combatant.Character.IsAlive && !combatant.IsDowned)
            .ToArray();

        double slowestBaseSpeed = activeCombatants
            .Min(combatant => GetBaseSpeed(combatant.Character));
        largestRecoveryCost = activeCombatants
            .SelectMany(combatant => combatant.ActionOrder)
            .Max(action => action.RecoveryCost);

        if (largestRecoveryCost <= 0.0)
        {
            throw new InvalidOperationException(
                "At least one active combat action must have a positive Recovery Cost.");
        }

        // Normalize Progress units so the slowest combatant's longest authored recovery
        // takes no longer than the current four-second pacing target.
        return _maximumRecoveryTimeSeconds * slowestBaseSpeed / largestRecoveryCost;
    }

    private static CombatActionDefinition CreateReviveAction(double recoveryCost)
    {
        return new CombatActionDefinition(
            _reviveActionId,
            "Revive",
            _revivalStaminaCost + " Stamina · Recovery",
            "Revive one Downed ally who has not already been revived this encounter.",
            CombatActionBehavior.Revive,
            _revivalStaminaCost,
            0,
            0,
            "Not enough Stamina.",
            "You bring your ally back into the fight.",
            recoveryCost);
    }

    private static double GetBaseSpeed(Character character)
    {
        return 5.0 + (character.Stats.Agility / 10.0);
    }

    private TimelineCombatantStatus[] GetTimelineEntries()
    {
        if (_timeline == null)
        {
            return Array.Empty<TimelineCombatantStatus>();
        }

        List<TimelineCombatantStatus> entries = new List<TimelineCombatantStatus>();
        foreach (CombatantRuntime combatant in _combatants.Values)
        {
            if (!combatant.Character.IsAlive ||
                combatant.IsDowned ||
                !_timeline.ContainsCombatant(combatant.Character.Id))
            {
                continue;
            }

            CombatantTimelineState state = _timeline.GetCombatant(combatant.Character.Id);
            entries.Add(new TimelineCombatantStatus(
                combatant.Character.Id,
                combatant.Character.Name,
                combatant.IsEnemy,
                state.RemainingProgress,
                state.BaseSpeed,
                state.Luck,
                state.IsReady,
                state.IsStunned(_timeline.CombatTime),
                combatant.IsVisible,
                combatant.Character.Id == _rewardRecipient.Id,
                state.IsStunned(_timeline.CombatTime)
                    ? Math.Max(0.0, state.StunEndTime.Value - _timeline.CombatTime)
                    : 0.0,
                combatant.IsEnemy && combatant.IsVisible
                    ? GetEnemyIntentDescription(combatant)
                    : string.Empty));
        }

        return entries
            .OrderBy(entry => entry.RemainingStunTime + entry.RemainingProgress / entry.Speed)
            .ThenByDescending(entry => entry.Luck)
            .ThenBy(entry => entry.Id, StringComparer.Ordinal)
            .ToArray();
    }

    private static string GetCharacterCondition(CombatantRuntime combatant)
    {
        if (combatant.IsDowned)
        {
            return "Downed";
        }

        return combatant.Character.Health <= combatant.Character.MaxHealth / 2
            ? "Wounded"
            : "Healthy";
    }

    private static string GetEnemyCondition(Character character)
    {
        if (!character.IsAlive)
        {
            return "Defeated";
        }

        return character.Health <= character.MaxHealth / 2 ? "Wounded" : "Healthy";
    }

    private void CompleteVictory(List<CombatEvent> events)
    {
        _encounterOver = true;
        IsVictory = true;
        _activeCombatantId = string.Empty;
        _rewardRecipient.AddGold(_victoryReward);
        events.Add(new CombatEvent(
            CombatEventType.StatusChanged,
            _round,
            detail: "The enemy falls. You win the encounter."));
        events.Add(new CombatEvent(
            CombatEventType.StatusChanged,
            _round,
            detail: "REWARD · You receive 25 G."));
    }

    private void CompleteDefeat(List<CombatEvent> events)
    {
        _encounterOver = true;
        IsVictory = false;
        _activeCombatantId = string.Empty;
        events.Add(new CombatEvent(
            CombatEventType.StatusChanged,
            _round,
            detail: "Your party has been defeated. The encounter is over."));
    }

    private CombatResult Success(List<CombatEvent> events)
    {
        return new CombatResult(true, GetSnapshot(), events.ToArray());
    }

    private CombatResult Failure(string reason)
    {
        return new CombatResult(false, GetSnapshot(), Array.Empty<CombatEvent>(), reason);
    }

    private static CombatActionDefinition[] CreateDefaultEnemyActions()
    {
        return new[]
        {
            new CombatActionDefinition("presence_quick_jab", "Quick Jab", "", "A fast, light attack.",
                CombatActionBehavior.Attack, 0, 4, 0, "", "The presence lunges forward without warning.", 75),
            new CombatActionDefinition("presence_heavy_swing", "Heavy Swing", "", "A slower, harder attack.",
                CombatActionBehavior.Attack, 0, 8, 0, "", "The presence slams into its target.", 130),
            new CombatActionDefinition("presence_staggering_blow", "Staggering Blow", "", "A brief stun.",
                CombatActionBehavior.Stun, 0, 3, 0, "", "A pulse of pressure leaves its target momentarily disoriented.", 115, 0, 0.5)
        };
    }
}
