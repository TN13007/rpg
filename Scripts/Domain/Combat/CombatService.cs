using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Owns the encounter roster and resolves discrete Party and Enemy Turns.</summary>
/// <remarks>Combat advances only through successful Party actions or an explicit End Player Turn request. No frame clock, speed, recovery timer or precommitted enemy intent is used.</remarks>
public sealed class CombatService : ICombatService
{
    private const int _defaultActionCapacity = 2;
    private const int _partyActionCost = 1;
    private const int _victoryReward = 25;
    private const int _revivalStaminaCost = 4;
    private const string _reviveActionId = "revive_ally";

    private sealed class CombatantRuntime
    {
        public Character Character { get; }
        public CombatantSide Side { get; }
        public bool IsVisible { get; }
        public List<CombatActionDefinition> ActionOrder { get; }
        public Dictionary<string, CombatActionDefinition> Actions { get; }
        public CombatantEffectState EffectState { get; }
        public EnemyPattern Pattern { get; }
        public bool IsDowned { get; set; }
        public bool HasBeenRevived { get; set; }
        public bool HasActedThisRound { get; set; }

        public CombatantRuntime(CombatantDefinition definition)
        {
            Character = definition.Character;
            Side = definition.Side;
            IsVisible = definition.IsVisible;
            ActionOrder = definition.Actions.ToList();
            Actions = ActionOrder.ToDictionary(action => action.Id, StringComparer.Ordinal);
            EffectState = new CombatantEffectState();
            IsDowned = IsPartyMember && !Character.IsAlive;
            Pattern = IsEnemy
                ? (definition.EnemyPattern ?? new EnemyPattern(ActionOrder.Select(action => action.Id)))
                    .CreateRuntimeCopy()
                : null;
        }

        public bool IsEnemy => Side == CombatantSide.Enemy;
        public bool IsPartyMember => Side == CombatantSide.Party;
    }

    private readonly Dictionary<string, CombatantRuntime> _combatants =
        new Dictionary<string, CombatantRuntime>(StringComparer.Ordinal);
    private readonly List<CombatantRuntime> _party = new List<CombatantRuntime>();
    private readonly List<CombatantRuntime> _enemies = new List<CombatantRuntime>();
    private readonly int _actionCapacity;
    private readonly CombatActionResolver _actionResolver = new CombatActionResolver();
    private readonly CombatEffectDispatcher _effectDispatcher = new CombatEffectDispatcher();
    private readonly Character _rewardRecipient;

    private ActionPool _actionPool;
    private CombatActionDefinition _reviveAction;
    private CombatPhase _phase = CombatPhase.NotStarted;
    private int _round;
    private bool _encounterStarted;
    private bool _encounterOver;

    /// <summary>Gets whether the most recently completed encounter ended in victory.</summary>
    public bool IsVictory { get; private set; }

    /// <summary>Gets whether the active encounter has reached a terminal state.</summary>
    public bool IsEncounterOver => _encounterStarted && _encounterOver;

    /// <summary>Gets whether the Domain is waiting for a Party action or End Turn request.</summary>
    public bool IsAwaitingPlayerDecision =>
        _encounterStarted &&
        !_encounterOver &&
        _phase == CombatPhase.PlayerTurn;

    /// <summary>Gets the current combat phase.</summary>
    public CombatPhase Phase => _phase;

    /// <summary>Gets the current Round number.</summary>
    public int Round => _round;

    /// <summary>Gets the Party's current shared Action Points.</summary>
    public int CurrentActionPoints =>
        _encounterStarted ? _actionPool.CurrentActionPoints : 0;

    /// <summary>Gets the Party's Action Point capacity for each Player Turn.</summary>
    public int ActionCapacity => _actionCapacity;

    /// <summary>Gets the gold reward granted for a victory, or zero before victory.</summary>
    public int RewardGold => IsVictory ? _victoryReward : 0;

    /// <summary>Gets the Party characters in their configured encounter order.</summary>
    public IReadOnlyList<Character> PartyMembers =>
        _party.Select(item => item.Character).ToArray();

    /// <summary>Creates an encounter from its complete Party and Enemy roster.</summary>
    /// <param name="combatants">The ordered roster. Party and Enemy order is preserved for deterministic resolution.</param>
    /// <param name="actionCapacity">The shared Party Action Point capacity, from one through four.</param>
    public CombatService(
        IEnumerable<CombatantDefinition> combatants,
        int actionCapacity = _defaultActionCapacity)
    {
        if (combatants == null)
        {
            throw new ArgumentNullException(nameof(combatants));
        }

        _actionPool = new ActionPool(actionCapacity);
        _actionCapacity = actionCapacity;

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
            throw new ArgumentException("An encounter requires at least one Party member.", nameof(combatants));
        }

        if (_enemies.Count == 0)
        {
            throw new ArgumentException("An encounter requires at least one Enemy.", nameof(combatants));
        }

        _rewardRecipient = _party[0].Character;
        _reviveAction = CreateReviveAction();
    }

    /// <summary>Resets encounter-only state and begins Round 1 with a full Party AP pool.</summary>
    /// <param name="failureReason">Receives a reason when the encounter cannot start.</param>
    /// <returns>True when the roster contains a living Party member and a living Enemy.</returns>
    public bool TryStartEncounter(out string failureReason)
    {
        if (_encounterStarted && !_encounterOver)
        {
            failureReason = "The current encounter is still active.";
            return false;
        }

        foreach (CombatantRuntime combatant in _combatants.Values)
        {
            if (combatant.IsEnemy)
            {
                combatant.Character.ResetToStartingValues();
            }

            combatant.EffectState.Reset();
            combatant.IsDowned = combatant.IsPartyMember && !combatant.Character.IsAlive;
            combatant.HasBeenRevived = false;
            combatant.HasActedThisRound = false;
            combatant.Pattern?.Reset();
        }

        if (!_party.Any(item => item.Character.IsAlive && !item.IsDowned))
        {
            _phase = CombatPhase.NotStarted;
            _encounterStarted = false;
            _encounterOver = false;
            IsVictory = false;
            failureReason = "At least one Party member must be able to fight.";
            return false;
        }

        if (!_enemies.Any(item => item.Character.IsAlive))
        {
            _phase = CombatPhase.NotStarted;
            _encounterStarted = false;
            _encounterOver = false;
            IsVictory = false;
            failureReason = "The encounter requires at least one living Enemy.";
            return false;
        }

        _actionPool = new ActionPool(_actionCapacity);
        _round = 1;
        _phase = CombatPhase.PlayerTurn;
        _encounterStarted = true;
        _encounterOver = false;
        IsVictory = false;
        failureReason = string.Empty;
        return true;
    }

    /// <summary>Builds a read-only snapshot with action options for every eligible Party member.</summary>
    public CombatSnapshot GetSnapshot()
    {
        int currentActionPoints = CurrentActionPoints;
        bool playerCanAct = IsAwaitingPlayerDecision && currentActionPoints > 0;

        PartyCombatStatus[] partyStatuses = _party
            .Select(item => new PartyCombatStatus(
                item.Character.Id,
                item.Character.Name,
                item.Character.Health,
                item.Character.MaxHealth,
                GetCharacterCondition(item),
                playerCanAct &&
                    item.Character.IsAlive &&
                    !item.IsDowned &&
                    !item.HasActedThisRound,
                item.Character.Stamina,
                item.Character.MaxStamina))
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

        CombatActionOption[] availableActions = playerCanAct
            ? GetAvailableActions()
            : Array.Empty<CombatActionOption>();

        string turnLabel = _phase switch
        {
            CombatPhase.PlayerTurn => "PLAYER TURN",
            CombatPhase.EnemyTurn => "ENEMY TURN",
            CombatPhase.Complete => "ENCOUNTER COMPLETE",
            _ => "NOT STARTED"
        };

        return new CombatSnapshot(
            _round,
            _phase,
            currentActionPoints,
            _actionCapacity,
            turnLabel,
            enemyStatuses,
            partyStatuses,
            availableActions);
    }

    /// <summary>Validates and resolves one action submitted by an eligible Party member.</summary>
    /// <param name="request">The required acting Party member, action identifier and optional target.</param>
    /// <returns>The updated snapshot and ordered events. Rejected requests do not consume AP or lock the actor.</returns>
    public CombatResult ExecuteAction(CombatActionRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (!_encounterStarted || _encounterOver)
        {
            return Failure("There is no active encounter.");
        }

        if (_phase != CombatPhase.PlayerTurn)
        {
            return Failure("It is not the Player Turn.");
        }

        if (_actionPool.CurrentActionPoints < _partyActionCost)
        {
            return Failure("The Party has no Action Points remaining.");
        }

        if (!_combatants.TryGetValue(request.ActorId, out CombatantRuntime actor) ||
            !actor.IsPartyMember ||
            !actor.Character.IsAlive ||
            actor.IsDowned)
        {
            return Failure("Choose a living Party member who can act.");
        }

        if (actor.HasActedThisRound)
        {
            return Failure("This Party member has already acted this Round.");
        }

        bool isRevive = request.ActionId == _reviveAction.Id;
        CombatActionDefinition action;
        if (isRevive)
        {
            action = _reviveAction;
        }
        else if (!actor.Actions.TryGetValue(request.ActionId, out action))
        {
            return Failure("That action is not available to this Party member.");
        }

        CombatantRuntime target = actor;
        if (isRevive)
        {
            if (string.IsNullOrWhiteSpace(request.TargetId) ||
                !_combatants.TryGetValue(request.TargetId, out target) ||
                !target.IsPartyMember ||
                target.Character.Id == actor.Character.Id ||
                !target.IsDowned ||
                target.HasBeenRevived)
            {
                return Failure("Choose an ally who is Downed and has not already been revived this encounter.");
            }
        }
        else if (RequiresEnemyTarget(action))
        {
            if (string.IsNullOrWhiteSpace(request.TargetId) ||
                !_combatants.TryGetValue(request.TargetId, out target) ||
                !target.IsEnemy ||
                !target.Character.IsAlive ||
                !target.IsVisible)
            {
                return Failure("Choose a visible, living Enemy as the target.");
            }
        }

        if (!IsActionAvailable(action, actor.Character))
        {
            return Failure(GetUnavailableReason(action));
        }

        if (!_actionPool.TrySpend(_partyActionCost))
        {
            return Failure("The Party has no Action Points remaining.");
        }

        if (!actor.Character.SpendStamina(action.StaminaCost))
        {
            throw new InvalidOperationException(
                "Stamina validation changed between action validation and spending.");
        }

        actor.HasActedThisRound = true;

        List<CombatEvent> events = new List<CombatEvent>
        {
            new CombatEvent(
                CombatEventType.PlayerAction,
                _round,
                actorName: actor.Character.Name,
                targetName: target.Character.Id == actor.Character.Id ? string.Empty : target.Character.Name,
                detail: action.ActionDetail,
                actionId: action.Id)
        };

        ApplyAction(action, actor, target, events);
        ResolveTerminalConditions(events);

        if (!_encounterOver && _actionPool.CurrentActionPoints == 0)
        {
            ResolveEnemyTurn(events);
        }

        return Success(events);
    }

    /// <summary>Ends the current Player Turn, discards unused AP, and resolves all eligible Enemy actions synchronously.</summary>
    public CombatResult EndPlayerTurn()
    {
        if (!_encounterStarted || _encounterOver)
        {
            return Failure("There is no active encounter.");
        }

        if (_phase != CombatPhase.PlayerTurn)
        {
            return Failure("The Player Turn is not active.");
        }

        List<CombatEvent> events = new List<CombatEvent>();
        ResolveEnemyTurn(events);
        return Success(events);
    }

    private CombatActionOption[] GetAvailableActions()
    {
        List<CombatActionOption> options = new List<CombatActionOption>();

        foreach (CombatantRuntime actor in _party)
        {
            if (!actor.Character.IsAlive || actor.IsDowned || actor.HasActedThisRound)
            {
                continue;
            }

            foreach (CombatActionDefinition action in actor.ActionOrder)
            {
                if (RequiresEnemyTarget(action))
                {
                    CombatActionTargetOption[] targets = GetLivingVisibleEnemyTargets();
                    if (targets.Length == 0)
                    {
                        continue;
                    }

                    options.Add(CreateActionOption(actor, action, targets));
                }
                else
                {
                    options.Add(CreateActionOption(actor, action));
                }
            }

            CombatActionTargetOption[] reviveTargets = GetReviveTargets(actor);
            if (reviveTargets.Length > 0)
            {
                options.Add(new CombatActionOption(
                    _reviveAction.Id,
                    actor.Character.Id,
                    actor.Character.Name + " · " + _reviveAction.Label,
                    _reviveAction.CostLabel,
                    _reviveAction.Description,
                    IsActionAvailable(_reviveAction, actor.Character),
                    IsActionAvailable(_reviveAction, actor.Character)
                        ? string.Empty
                        : GetUnavailableReason(_reviveAction),
                    reviveTargets));
            }
        }

        return options.ToArray();
    }

    private CombatActionOption CreateActionOption(
        CombatantRuntime actor,
        CombatActionDefinition action,
        CombatActionTargetOption[] targets = null)
    {
        bool isAvailable = IsActionAvailable(action, actor.Character);
        return new CombatActionOption(
            action.Id,
            actor.Character.Id,
            actor.Character.Name + " · " + action.Label,
            action.CostLabel,
            action.Description,
            isAvailable,
            isAvailable ? string.Empty : GetUnavailableReason(action),
            targets);
    }

    private CombatActionTargetOption[] GetLivingVisibleEnemyTargets()
    {
        return _enemies
            .Where(item => item.Character.IsAlive && item.IsVisible)
            .Select(item => new CombatActionTargetOption(
                item.Character.Id,
                item.Character.Name))
            .ToArray();
    }

    private CombatActionTargetOption[] GetReviveTargets(CombatantRuntime actor)
    {
        return _party
            .Where(item =>
                item.Character.Id != actor.Character.Id &&
                item.IsDowned &&
                !item.HasBeenRevived)
            .Select(item => new CombatActionTargetOption(
                item.Character.Id,
                item.Character.Name))
            .ToArray();
    }

    private void ResolveEnemyTurn(List<CombatEvent> events)
    {
        if (_encounterOver || _phase != CombatPhase.PlayerTurn)
        {
            return;
        }

        _phase = CombatPhase.EnemyTurn;
        events.Add(new CombatEvent(
            CombatEventType.TurnChanged,
            _round,
            detail: "The Enemy Turn begins."));

        foreach (CombatantRuntime enemy in _enemies)
        {
            if (_encounterOver)
            {
                break;
            }

            if (!enemy.Character.IsAlive)
            {
                continue;
            }

            string actionId = enemy.Pattern.NextActionId();
            if (!enemy.Actions.TryGetValue(actionId, out CombatActionDefinition action))
            {
                throw new InvalidOperationException(
                    "Enemy pattern references an action that is not present on its combatant: " + actionId);
            }

            if (!IsActionAvailable(action, enemy.Character))
            {
                events.Add(new CombatEvent(
                    CombatEventType.EnemyAction,
                    _round,
                    actorName: enemy.Character.Name,
                    detail: enemy.Character.Name + " cannot perform " + action.Label + " and loses this action.",
                    actionId: action.Id));
                continue;
            }

            CombatantRuntime target = enemy;
            if (RequiresEnemyTarget(action))
            {
                target = _party.FirstOrDefault(item =>
                    item.Character.IsAlive && !item.IsDowned);

                if (target == null)
                {
                    events.Add(new CombatEvent(
                        CombatEventType.EnemyAction,
                        _round,
                        actorName: enemy.Character.Name,
                        detail: enemy.Character.Name + " has no valid Party target and loses this action.",
                        actionId: action.Id));
                    continue;
                }
            }

            if (!enemy.Character.SpendStamina(action.StaminaCost))
            {
                events.Add(new CombatEvent(
                    CombatEventType.EnemyAction,
                    _round,
                    actorName: enemy.Character.Name,
                    detail: enemy.Character.Name + " cannot pay the Stamina cost for " + action.Label + ".",
                    actionId: action.Id));
                continue;
            }

            events.Add(new CombatEvent(
                CombatEventType.EnemyAction,
                _round,
                actorName: enemy.Character.Name,
                targetName: target.Character.Id == enemy.Character.Id ? string.Empty : target.Character.Name,
                detail: action.ActionDetail,
                actionId: action.Id));

            ApplyAction(action, enemy, target, events);
            ResolveTerminalConditions(events);
        }

        if (_encounterOver)
        {
            return;
        }

        _round++;
        foreach (CombatantRuntime member in _party)
        {
            member.HasActedThisRound = false;
        }

        _actionPool.BeginPlayerTurn();
        _phase = CombatPhase.PlayerTurn;

        events.Add(new CombatEvent(
            CombatEventType.RoundStarted,
            _round,
            detail: "Round " + _round + " begins."));
        events.Add(new CombatEvent(
            CombatEventType.TurnChanged,
            _round,
            detail: "The Player Turn begins with " +
                _actionPool.CurrentActionPoints + " of " +
                _actionPool.ActionCapacity + " AP."));
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
                actor.EffectState,
                effectTarget.EffectState,
                events,
                _round,
                _actionPool,
                actor.IsEnemy && effectTarget.IsPartyMember);

            if (request.Type == CombatEffectType.Revive &&
                effectTarget.IsPartyMember &&
                effectTarget.IsDowned &&
                effectTarget.Character.IsAlive)
            {
                effectTarget.IsDowned = false;
                effectTarget.HasBeenRevived = true;
                effectTarget.EffectState.Reset();
            }
            else if (!effectTarget.Character.IsAlive &&
                effectTarget.IsPartyMember &&
                !effectTarget.IsDowned)
            {
                effectTarget.IsDowned = true;
                effectTarget.EffectState.Reset();
                events.Add(new CombatEvent(
                    CombatEventType.StatusChanged,
                    _round,
                    targetName: effectTarget.Character.Name,
                    detail: effectTarget.Character.Name + " is Downed and cannot act."));
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

    private void CompleteVictory(List<CombatEvent> events)
    {
        if (_encounterOver)
        {
            return;
        }

        _encounterOver = true;
        _phase = CombatPhase.Complete;
        IsVictory = true;
        _rewardRecipient.AddGold(_victoryReward);
        events.Add(new CombatEvent(
            CombatEventType.StatusChanged,
            _round,
            detail: "The Enemy side is defeated. You win the encounter."));
        events.Add(new CombatEvent(
            CombatEventType.StatusChanged,
            _round,
            detail: "REWARD · You receive " + _victoryReward + " G."));
    }

    private void CompleteDefeat(List<CombatEvent> events)
    {
        if (_encounterOver)
        {
            return;
        }

        _encounterOver = true;
        _phase = CombatPhase.Complete;
        IsVictory = false;
        events.Add(new CombatEvent(
            CombatEventType.StatusChanged,
            _round,
            detail: "Every Party member is Downed. The encounter is over."));
    }

    private CombatResult Success(List<CombatEvent> events)
    {
        return new CombatResult(true, GetSnapshot(), events.ToArray());
    }

    private CombatResult Failure(string reason)
    {
        return new CombatResult(false, GetSnapshot(), Array.Empty<CombatEvent>(), reason);
    }

    private static CombatActionDefinition CreateReviveAction()
    {
        return new CombatActionDefinition(
            _reviveActionId,
            "Revive",
            "1 AP · 4 Stamina",
            "Revive one Downed ally who has not already been revived this encounter.",
            CombatActionBehavior.Revive,
            _revivalStaminaCost,
            0,
            0,
            "Not enough Stamina.",
            "You bring your ally back into the fight.");
    }

    private static bool IsActionAvailable(CombatActionDefinition action, Character actor)
    {
        if (action.Behavior == CombatActionBehavior.RecoverStamina)
        {
            return actor.Stamina < actor.MaxStamina;
        }

        return actor.Stamina >= action.StaminaCost;
    }

    private static bool RequiresEnemyTarget(CombatActionDefinition action)
    {
        return action.Behavior == CombatActionBehavior.Attack ||
               action.Behavior == CombatActionBehavior.Control;
    }

    private static string GetUnavailableReason(CombatActionDefinition action)
    {
        return string.IsNullOrWhiteSpace(action.UnavailableReason)
            ? "This action is currently unavailable."
            : action.UnavailableReason;
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
}