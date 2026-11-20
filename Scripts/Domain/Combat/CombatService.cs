using System;
using System.Collections.Generic;
using System.Linq;

public sealed class CombatService : ICombatService
{
    private const int _victoryReward = 25;
    private const double _initialProgress = 100;
    private readonly Character _player;
    private readonly Character _enemy;
    private readonly Dictionary<string, CombatActionDefinition> _playerActions;
    private readonly List<CombatActionDefinition> _playerActionOrder = new List<CombatActionDefinition>();
    private readonly List<CombatActionDefinition> _enemyActions = new List<CombatActionDefinition>();
    private readonly Random _enemyRandom;
    private readonly CombatActionResolver _actionResolver = new CombatActionResolver();
    private readonly CombatEffectDispatcher _effectDispatcher = new CombatEffectDispatcher();
    private readonly CombatantEffectState _playerEffectState = new CombatantEffectState();
    private readonly CombatantEffectState _enemyEffectState = new CombatantEffectState();
    private CombatTimeline _timeline;
    private string _activeCombatantId = string.Empty;
    private int _round;
    private bool _encounterOver;

    public bool IsVictory { get; private set; }
    public bool IsEncounterOver => _encounterOver;
    public bool IsAwaitingPlayerDecision => !_encounterOver && _activeCombatantId == _player.Id;
    public double CombatTime => _timeline == null ? 0 : _timeline.CombatTime;
    public int RewardGold => IsVictory ? _victoryReward : 0;

    public CombatService(Character player, Character enemy, IEnumerable<CombatActionDefinition> playerActions)
        : this(player, enemy, playerActions, CreateDefaultEnemyActions(), null) { }

    public CombatService(Character player, Character enemy, IEnumerable<CombatActionDefinition> playerActions,
        IEnumerable<CombatActionDefinition> enemyActions, Random enemyRandom = null)
    {
        _player = player ?? throw new ArgumentNullException(nameof(player));
        _enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
        if (playerActions == null) throw new ArgumentNullException(nameof(playerActions));
        if (enemyActions == null) throw new ArgumentNullException(nameof(enemyActions));
        _playerActions = new Dictionary<string, CombatActionDefinition>(StringComparer.Ordinal);
        foreach (CombatActionDefinition action in playerActions)
        {
            if (action == null) throw new ArgumentException("Combat actions cannot contain null values.", nameof(playerActions));
            if (!_playerActions.TryAdd(action.Id, action)) throw new ArgumentException("Duplicate combat action ID: " + action.Id, nameof(playerActions));
            _playerActionOrder.Add(action);
        }
        HashSet<string> enemyIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (CombatActionDefinition action in enemyActions)
        {
            if (action == null || !enemyIds.Add(action.Id))
                throw new ArgumentException("Enemy actions must be non-null and have unique IDs.", nameof(enemyActions));
            _enemyActions.Add(action);
        }
        if (_enemyActions.Count == 0) throw new ArgumentException("At least one enemy action is required.", nameof(enemyActions));
        _enemyRandom = enemyRandom ?? new Random();
    }

    public bool TryStartEncounter(out string failureReason)
    {
        if (_player.IsDead)
        {
            _encounterOver = true;
            IsVictory = false;
            failureReason = "You cannot enter combat while your character is dead.";
            return false;
        }
        _enemy.Reset();
        _timeline = new CombatTimeline();
        _timeline.AddCombatant(_player.Id, _player.Stats.Luck, GetBaseSpeed(_player), _initialProgress);
        _timeline.AddCombatant(_enemy.Id, _enemy.Stats.Luck, GetBaseSpeed(_enemy), _initialProgress);
        _playerEffectState.Reset();
        _enemyEffectState.Reset();
        _round = 1;
        _encounterOver = false;
        IsVictory = false;
        _activeCombatantId = string.Empty;
        failureReason = string.Empty;
        AdvanceUntilPlayerDecision(new List<CombatEvent>());
        return !_encounterOver;
    }

    public CombatSnapshot GetSnapshot()
    {
        bool canPlayerAct = IsAwaitingPlayerDecision && _player.CanAct && _enemy.IsAlive;
        List<CombatActionOption> availableActions = new List<CombatActionOption>();
        if (canPlayerAct)
        {
            foreach (CombatActionDefinition action in _playerActionOrder)
            {
                bool available = IsActionAvailable(action, _player);
                bool requiresTarget = action.Behavior == CombatActionBehavior.Attack ||
                                      action.Behavior == CombatActionBehavior.Delay ||
                                      action.Behavior == CombatActionBehavior.Stun;
                availableActions.Add(new CombatActionOption(
                    action.Id, action.Label, action.CostLabel, action.Description,
                    available, available ? string.Empty : GetUnavailableReason(action),
                    requiresTarget ? _enemy.Id : string.Empty, _player.Id));
            }
        }
        EnemyCombatStatus[] enemies =
        {
            new EnemyCombatStatus(_enemy.Id, _enemy.Name, _enemy.Health, _enemy.MaxHealth, GetEnemyCondition())
        };
        return new CombatSnapshot(_round,
            IsAwaitingPlayerDecision ? "PLAYER DECISION" : _encounterOver ? "ENCOUNTER COMPLETE" : "ENEMY ACTION",
            enemies, null, availableActions.ToArray(), GetTimelineEntries());
    }

    public CombatResult ExecuteAction(CombatActionRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (_encounterOver) return Failure("The encounter has already ended.");
        if (!IsAwaitingPlayerDecision) return Failure("It is not the player's action window.");
        if (!string.IsNullOrWhiteSpace(request.ActorId) && request.ActorId != _player.Id)
            return Failure("The requested character is not ready to act.");
        if (!_player.CanAct || !_enemy.IsAlive) return Failure("The encounter has no valid player action.");
        if (!_playerActions.TryGetValue(request.ActionId, out CombatActionDefinition action))
            return Failure("That action is not available.");
        if (!IsActionAvailable(action, _player)) return Failure(GetUnavailableReason(action));
        bool requiresTarget = action.Behavior == CombatActionBehavior.Attack ||
                              action.Behavior == CombatActionBehavior.Delay ||
                              action.Behavior == CombatActionBehavior.Stun;
        if (requiresTarget && request.TargetId != _enemy.Id) return Failure("Choose the enemy as your target.");
        if (!_player.SpendStamina(action.StaminaCost)) return Failure(GetUnavailableReason(action));

        List<CombatEvent> events = new List<CombatEvent>();
        events.Add(new CombatEvent(CombatEventType.PlayerAction, _round, actorName: _player.Name, detail: action.ActionDetail));
        ApplyAction(action, _player, _enemy, _playerEffectState, _enemyEffectState, _player.Id, _enemy.Id, events);
        _timeline.ApplyRecovery(_player.Id, action.RecoveryCost);
        if (!_enemy.IsAlive) CompleteVictory(events);
        else AdvanceUntilPlayerDecision(events);
        return Success(events);
    }

    private void AdvanceUntilPlayerDecision(List<CombatEvent> events)
    {
        int safetyCounter = 0;
        while (!_encounterOver)
        {
            safetyCounter++;
            if (safetyCounter > 1000) throw new InvalidOperationException("Combat scheduler exceeded its safety limit.");
            string readyId = _timeline.GetNextReadyCombatantId();
            if (string.IsNullOrEmpty(readyId))
            {
                if (!_timeline.AdvanceToNextEvent()) throw new InvalidOperationException("Combat Timeline has no future event.");
                continue;
            }
            if (readyId == _player.Id) { _activeCombatantId = _player.Id; return; }
            if (readyId != _enemy.Id) throw new InvalidOperationException("Unknown ready combatant: " + readyId);
            double recoveryCost = ResolveEnemyAction(events);
            if (!_player.IsAlive) { CompleteDefeat(events); return; }
            if (!_enemy.IsAlive) { CompleteVictory(events); return; }
            _timeline.ApplyRecovery(_enemy.Id, recoveryCost);
            _round++;
        }
    }

    private double ResolveEnemyAction(List<CombatEvent> events)
    {
        List<CombatActionDefinition> available = _enemyActions.Where(action => IsActionAvailable(action, _enemy)).ToList();
        if (available.Count == 0) available.Add(_enemyActions[0]);
        CombatActionDefinition action = available[_enemyRandom.Next(available.Count)];
        _enemy.SpendStamina(action.StaminaCost);
        events.Add(new CombatEvent(CombatEventType.EnemyAction, _round,
            actorName: _enemy.Name.ToUpperInvariant(), detail: action.ActionDetail));
        ApplyAction(action, _enemy, _player, _enemyEffectState, _playerEffectState, _enemy.Id, _player.Id, events);
        return action.RecoveryCost;
    }

    private void ApplyAction(CombatActionDefinition action, Character actor, Character target,
        CombatantEffectState actorState, CombatantEffectState targetState,
        string actorId, string targetId, List<CombatEvent> events)
    {
        CombatEffectRequest[] requests = _actionResolver.Resolve(action, actorId, targetId);
        foreach (CombatEffectRequest request in requests)
        {
            bool targetsActor = request.TargetId == actorId;
            Character effectTarget = targetsActor ? actor : target;
            CombatantEffectState effectTargetState = targetsActor ? actorState : targetState;
            _effectDispatcher.Apply(request, actor, effectTarget, _timeline,
                actorState, effectTargetState, events, _round);
        }
    }

    private bool IsActionAvailable(CombatActionDefinition action, Character actor)
    {
        if (action.Behavior == CombatActionBehavior.RecoverStamina) return actor.Stamina < actor.MaxStamina;
        return actor.Stamina >= action.StaminaCost;
    }

    private static string GetUnavailableReason(CombatActionDefinition action)
    {
        return string.IsNullOrWhiteSpace(action.UnavailableReason) ? "This action is currently unavailable." : action.UnavailableReason;
    }

    private static double GetBaseSpeed(Character character) { return 5.0 + (character.Stats.Agility / 10.0); }

    private TimelineCombatantStatus[] GetTimelineEntries()
    {
        if (_timeline == null) return Array.Empty<TimelineCombatantStatus>();
        List<TimelineCombatantStatus> entries = new List<TimelineCombatantStatus>();
        AddTimelineEntry(entries, _player, false);
        AddTimelineEntry(entries, _enemy, true);
        return entries.OrderBy(entry => entry.IsStunned ? double.MaxValue : entry.RemainingProgress / entry.Speed)
            .ThenByDescending(entry => entry.Luck).ThenBy(entry => entry.Id, StringComparer.Ordinal).ToArray();
    }

    private void AddTimelineEntry(List<TimelineCombatantStatus> entries, Character character, bool isEnemy)
    {
        if (!character.IsAlive) return;
        CombatantTimelineState state = _timeline.GetCombatant(character.Id);
        entries.Add(new TimelineCombatantStatus(character.Id, character.Name, isEnemy,
            state.RemainingProgress, state.BaseSpeed, state.Luck, state.IsReady, state.IsStunned(_timeline.CombatTime)));
    }

    private void CompleteVictory(List<CombatEvent> events)
    {
        _encounterOver = true;
        IsVictory = true;
        _activeCombatantId = string.Empty;
        _player.AddGold(_victoryReward);
        events.Add(new CombatEvent(CombatEventType.StatusChanged, _round, detail: "The enemy falls. You win the encounter."));
        events.Add(new CombatEvent(CombatEventType.StatusChanged, _round, detail: "REWARD · You receive 25 G."));
    }

    private void CompleteDefeat(List<CombatEvent> events)
    {
        _encounterOver = true;
        IsVictory = false;
        _activeCombatantId = string.Empty;
        events.Add(new CombatEvent(CombatEventType.StatusChanged, _round, detail: "You have been defeated. The encounter is over."));
    }

    private CombatResult Success(List<CombatEvent> events) { return new CombatResult(true, GetSnapshot(), events.ToArray()); }
    private CombatResult Failure(string reason) { return new CombatResult(false, GetSnapshot(), Array.Empty<CombatEvent>(), reason); }

    private string GetEnemyCondition()
    {
        if (!_enemy.IsAlive) return "Defeated";
        if (_enemy.Health <= _enemy.MaxHealth / 2) return "Wounded";
        return "Healthy";
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
