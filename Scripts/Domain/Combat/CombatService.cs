using System;
using System.Collections.Generic;
using System.Linq;

public sealed class CombatService : ICombatService
{
    private const int _victoryReward = 25;
    private const double _initialProgress = 100;

    private sealed class CombatantRuntime
    {
        public Character Character { get; }
        public CombatantSide Side { get; }
        public bool IsVisible { get; }
        public List<CombatActionDefinition> ActionOrder { get; }
        public Dictionary<string, CombatActionDefinition> Actions { get; }
        public CombatantEffectState EffectState { get; }

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
    private string _activeCombatantId = string.Empty;
    private int _round;
    private bool _encounterOver;

    public bool IsVictory { get; private set; }
    public bool IsEncounterOver => _encounterOver;
    public bool IsAwaitingPlayerDecision =>
        !_encounterOver &&
        _combatants.TryGetValue(_activeCombatantId, out CombatantRuntime active) &&
        active.IsPartyMember;
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

        foreach (CombatantRuntime enemy in _enemies)
        {
            enemy.Character.Reset();
        }

        if (!_enemies.Any(item => item.Character.IsAlive))
        {
            _encounterOver = true;
            IsVictory = false;
            failureReason = "The encounter requires at least one living enemy.";
            return false;
        }

        _timeline = new CombatTimeline();
        foreach (CombatantRuntime combatant in _combatants.Values)
        {
            combatant.EffectState.Reset();
            if (!combatant.Character.IsAlive)
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
        if (!_encounterOver)
        {
            AdvanceUntilPlayerDecision(startEvents);
        }

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
                    CombatantRuntime[] targets = _enemies
                        .Where(item => item.Character.IsAlive && item.IsVisible)
                        .ToArray();

                    foreach (CombatantRuntime enemy in targets)
                    {
                        string label = targets.Length > 1
                            ? action.Label + " → " + enemy.Character.Name
                            : action.Label;

                        availableActions.Add(new CombatActionOption(
                            action.Id,
                            label,
                            action.CostLabel,
                            action.Description,
                            available,
                            available ? string.Empty : GetUnavailableReason(action),
                            enemy.Character.Id,
                            active.Character.Id));
                    }
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
                        string.Empty,
                        active.Character.Id));
                }
            }
        }

        PartyCombatStatus[] partyStatuses = _party
            .Where(item => item.Character.IsAlive)
            .Select(item => new PartyCombatStatus(
                item.Character.Id,
                item.Character.Name,
                item.Character.Health,
                item.Character.MaxHealth,
                GetCharacterCondition(item.Character),
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
                : "ENEMY ACTION";

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
            AdvanceUntilPlayerDecision(events);
        }

        return Success(events);
    }

    private void AdvanceUntilPlayerDecision(List<CombatEvent> events)
    {
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
                return;
            }

            string readyId = _timeline.GetNextReadyCombatantId();
            if (string.IsNullOrEmpty(readyId))
            {
                if (!_timeline.AdvanceToNextEvent())
                {
                    throw new InvalidOperationException("Combat Timeline has no future event.");
                }

                continue;
            }

            if (!_combatants.TryGetValue(readyId, out CombatantRuntime readyCombatant) ||
                !readyCombatant.Character.IsAlive)
            {
                _timeline.RemoveCombatant(readyId);
                continue;
            }

            if (readyCombatant.IsPartyMember)
            {
                _activeCombatantId = readyCombatant.Character.Id;
                return;
            }

            double recoveryCost = ResolveEnemyAction(readyCombatant, events);
            ResolveTerminalConditions(events);
            if (_encounterOver)
            {
                return;
            }

            if (readyCombatant.Character.IsAlive && _timeline.ContainsCombatant(readyCombatant.Character.Id))
            {
                _timeline.ApplyRecovery(readyCombatant.Character.Id, recoveryCost);
            }

            _round++;
        }
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

            if (!effectTarget.Character.IsAlive)
            {
                _timeline.RemoveCombatant(effectTarget.Character.Id);
            }
        }
    }

    private void ResolveTerminalConditions(List<CombatEvent> events)
    {
        if (_encounterOver)
        {
            return;
        }

        if (!_party.Any(item => item.Character.IsAlive))
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
               action.Behavior == CombatActionBehavior.Stun;
    }

    private static string GetUnavailableReason(CombatActionDefinition action)
    {
        return string.IsNullOrWhiteSpace(action.UnavailableReason)
            ? "This action is currently unavailable."
            : action.UnavailableReason;
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
            if (!combatant.Character.IsAlive || !_timeline.ContainsCombatant(combatant.Character.Id))
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
                combatant.Character.Id == _rewardRecipient.Id));
        }

        return entries
            .OrderBy(entry => entry.IsStunned ? double.MaxValue : entry.RemainingProgress / entry.Speed)
            .ThenByDescending(entry => entry.Luck)
            .ThenBy(entry => entry.Id, StringComparer.Ordinal)
            .ToArray();
    }

    private static string GetCharacterCondition(Character character)
    {
        return character.Health <= character.MaxHealth / 2 ? "Wounded" : "Healthy";
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
            new CombatActionDefinition("goblin_quick_jab", "Quick Jab", "", "A fast, light attack.",
                CombatActionBehavior.Attack, 0, 4, 0, "", "The goblin snaps out a quick jab.", 75),
            new CombatActionDefinition("goblin_heavy_swing", "Heavy Swing", "", "A slower, harder attack.",
                CombatActionBehavior.Attack, 0, 8, 0, "", "The goblin commits to a heavy swing.", 130),
            new CombatActionDefinition("goblin_staggering_blow", "Staggering Blow", "", "A brief stun.",
                CombatActionBehavior.Stun, 0, 3, 0, "", "The goblin lands a staggering blow.", 115, 0, 0.5)
        };
    }
}
