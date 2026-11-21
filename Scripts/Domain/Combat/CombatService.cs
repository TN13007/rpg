using System;
using System.Collections.Generic;
using System.Linq;

public sealed class CombatService : ICombatService
{
    private const int _victoryReward = 25;
    private const int _revivalStaminaCost = 4;
    private const string _reviveActionId = "revive_ally";
    private const double _initialProgress = 100;
    private const double _maximumRecoveryTimeSeconds = 4.0;

    private sealed class CombatantRuntime
    {
        public Character Character { get; }
        public CombatantSide Side { get; }
        public bool IsVisible { get; }
        public List<CombatActionDefinition> ActionOrder { get; }
        public Dictionary<string, CombatActionDefinition> Actions { get; }
        public CombatantEffectState EffectState { get; }
        public bool IsDowned { get; set; }
        public bool HasBeenRevived { get; set; }

        public CombatantRuntime(CombatantDefinition definition)
        {
            Character = definition.Character;
            Side = definition.Side;
            IsVisible = definition.IsVisible;
            ActionOrder = definition.Actions.ToList();
            Actions = new Dictionary<string, CombatActionDefinition>(StringComparer.Ordinal);
            foreach (CombatActionDefinition action in ActionOrder)
            {
                Actions.Add(action.Id, action);
            }

            EffectState = new CombatantEffectState();
            IsDowned = Side == CombatantSide.Party && !Character.IsAlive;
        }

        public bool IsEnemy => Side == CombatantSide.Enemy;
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

    public bool IsVictory { get; private set; }
    public bool IsEncounterOver => _encounterOver;
    public bool IsAwaitingPlayerDecision =>
        !_encounterOver &&
        _combatants.TryGetValue(_activeCombatantId, out CombatantRuntime active) &&
        active.IsPartyMember &&
        !active.IsDowned &&
        active.Character.CanAct;
    public string ActiveCombatantId => _activeCombatantId;
    public double CombatTime => _timeline == null ? 0 : _timeline.CombatTime;
    public int RewardGold => IsVictory ? _victoryReward : 0;
    public IReadOnlyList<Character> PartyMembers => _party.Select(item => item.Character).ToArray();

    public CombatService(Character player, Character enemy, IEnumerable<CombatActionDefinition> playerActions)
        : this(player, enemy, playerActions, CreateDefaultEnemyActions(), null)
    {
    }

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

        _round = 1;
        _encounterOver = false;
        IsVictory = false;
        _activeCombatantId = string.Empty;
        failureReason = string.Empty;

        List<CombatEvent> startEvents = new List<CombatEvent>();
        ResolveTerminalConditions(startEvents);

        return !_encounterOver;
    }

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

        EnemyCombatStatus[] enemyStatuses = _enemies
            .Select(item => new EnemyCombatStatus(
                item.Character.Id,
                item.Character.Name,
                item.Character.Health,
                item.Character.MaxHealth,
                GetEnemyCondition(item.Character),
                item.IsVisible))
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
            null,
            availableActions.ToArray(),
            GetTimelineEntries(),
            partyStatuses,
            _activeCombatantId);
    }

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
            detail: action.ActionDetail));

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
                detail: _reviveAction.ActionDetail)
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
        List<CombatActionDefinition> availableActions = actor.ActionOrder
            .Where(action => IsActionAvailable(action, actor.Character))
            .ToList();

        CombatActionDefinition action = availableActions.Count == 0
            ? actor.ActionOrder[0]
            : availableActions[_enemyRandom.Next(availableActions.Count)];

        if (!actor.Character.SpendStamina(action.StaminaCost))
        {
            events.Add(new CombatEvent(
                CombatEventType.EnemyAction,
                _round,
                actorName: actor.Character.Name.ToUpperInvariant(),
                detail: actor.Character.Name + " cannot afford an action and loses the opportunity."));
            return action.RecoveryCost;
        }

        CombatantRuntime target = actor;
        if (RequiresTarget(action))
        {
            List<CombatantRuntime> validTargets = _party
                .Where(item => item.Character.IsAlive)
                .ToList();

            if (validTargets.Count == 0)
            {
                return action.RecoveryCost;
            }

            int targetIndex = validTargets.Count == 1 ? 0 : _enemyRandom.Next(validTargets.Count);
            target = validTargets[targetIndex];
        }

        events.Add(new CombatEvent(
            CombatEventType.EnemyAction,
            _round,
            actorName: actor.Character.Name.ToUpperInvariant(),
            targetName: target.Character.Id == actor.Character.Id ? string.Empty : target.Character.Name,
            detail: action.ActionDetail));

        ApplyAction(action, actor, target, events);
        return action.RecoveryCost;
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
                    : 0.0));
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
