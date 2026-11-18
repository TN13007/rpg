using System;
using System.Collections.Generic;

public sealed class CombatService : ICombatService
{
    private const int _enemyAttackDamage = 4;
    private const int _victoryReward = 25;

    private readonly Character _player;
    private readonly Character _enemy;
    private readonly Dictionary<string, CombatActionDefinition> _playerActions;
    private int _round;
    private bool _encounterOver;
    private bool _defending;
    private bool _dodging;

    public bool IsVictory { get; private set; }
    public bool IsEncounterOver => _encounterOver;
    public int RewardGold => IsVictory ? _victoryReward : 0;

    public CombatService(
        Character player,
        Character enemy,
        IEnumerable<CombatActionDefinition> playerActions)
    {
        _player = player ?? throw new ArgumentNullException(nameof(player));
        _enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
        if (playerActions == null) throw new ArgumentNullException(nameof(playerActions));

        _playerActions = new Dictionary<string, CombatActionDefinition>(StringComparer.Ordinal);
        foreach (CombatActionDefinition action in playerActions)
        {
            if (action == null) throw new ArgumentException("Combat actions cannot contain null values.", nameof(playerActions));
            if (!_playerActions.TryAdd(action.Id, action))
                throw new ArgumentException("Duplicate combat action ID: " + action.Id, nameof(playerActions));
        }
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
        _round = 1;
        _encounterOver = false;
        IsVictory = false;
        _defending = false;
        _dodging = false;
        failureReason = string.Empty;
        return true;
    }

    public CombatSnapshot GetSnapshot()
    {
        bool canAct = !_encounterOver && _player.CanAct && _enemy.IsAlive;
        IntentObservation intent = canAct
            ? new IntentObservation(_enemy.Id, _enemy.Name, "The enemy is watching you and preparing to strike.", IntentConfidence.Suspected)
            : null;

        List<CombatActionOption> availableActions = new List<CombatActionOption>();
        if (canAct)
        {
            foreach (CombatActionDefinition action in _playerActions.Values)
            {
                bool isAvailable = IsActionAvailable(action);
                string unavailableReason = isAvailable
                    ? string.Empty
                    : GetUnavailableReason(action);
                availableActions.Add(new CombatActionOption(
                    action.Id,
                    action.Label,
                    action.CostLabel,
                    action.Description,
                    isAvailable,
                    unavailableReason,
                    action.Behavior == CombatActionBehavior.Attack ? _enemy.Id : string.Empty));
            }
        }

        EnemyCombatStatus[] enemies =
        {
            new EnemyCombatStatus(_enemy.Id, _enemy.Name, _enemy.Health, _enemy.MaxHealth, GetEnemyCondition())
        };

        return new CombatSnapshot(_round, "PLAYER TURN", enemies, intent, availableActions.ToArray());
    }

    public CombatResult ExecuteAction(CombatActionRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (_encounterOver) return Failure("The encounter has already ended.");
        if (_player.IsDead)
        {
            _encounterOver = true;
            IsVictory = false;
            return Failure("Your character is dead and cannot act.");
        }
        if (!_enemy.IsAlive)
        {
            _encounterOver = true;
            IsVictory = false;
            return Failure("There is no living enemy to fight.");
        }
        if (!_playerActions.TryGetValue(request.ActionId, out CombatActionDefinition action))
            return Failure("That action is not available.");
        if (!IsActionAvailable(action))
            return Failure(GetUnavailableReason(action));

        if (action.Behavior == CombatActionBehavior.Attack && request.TargetId != _enemy.Id)
            return Failure("Choose the enemy as your target.");

        List<CombatEvent> events = new List<CombatEvent>();
        switch (action.Behavior)
        {
            case CombatActionBehavior.Attack:
                events.Add(new CombatEvent(CombatEventType.PlayerAction, _round, detail: action.ActionDetail));
                int damage = _enemy.TakeDamage(action.Damage);
                events.Add(new CombatEvent(CombatEventType.Damage, _round, targetName: _enemy.Name, amount: damage));
                break;

            case CombatActionBehavior.Defend:
                if (!_player.SpendStamina(action.StaminaCost))
                    return Failure(GetUnavailableReason(action));
                _defending = true;
                events.Add(new CombatEvent(CombatEventType.PlayerAction, _round, detail: action.ActionDetail));
                break;

            case CombatActionBehavior.Dodge:
                if (!_player.SpendStamina(action.StaminaCost))
                    return Failure(GetUnavailableReason(action));
                _dodging = true;
                events.Add(new CombatEvent(CombatEventType.PlayerAction, _round, detail: action.ActionDetail));
                break;

            case CombatActionBehavior.RecoverStamina:
                int restored = _player.RestoreStamina(action.RecoveryAmount);
                events.Add(new CombatEvent(
                    CombatEventType.PlayerAction,
                    _round,
                    detail: action.ActionDetail.Replace("{amount}", restored.ToString())));
                break;

            default:
                return Failure("The action uses an unsupported combat behavior.");
        }

        if (!_enemy.IsAlive)
        {
            CompleteVictory(events);
            return Success(events);
        }

        ResolveEnemyTurn(events);
        if (_player.IsDead)
        {
            _encounterOver = true;
            IsVictory = false;
            events.Add(new CombatEvent(CombatEventType.StatusChanged, _round, detail: "You have died. The encounter is over."));
        }

        _round++;
        return Success(events);
    }

    private bool IsActionAvailable(CombatActionDefinition action)
    {
        if (action.Behavior == CombatActionBehavior.RecoverStamina)
            return _player.Stamina < _player.MaxStamina;
        return _player.Stamina >= action.StaminaCost;
    }

    private static string GetUnavailableReason(CombatActionDefinition action)
    {
        return string.IsNullOrWhiteSpace(action.UnavailableReason)
            ? "This action is currently unavailable."
            : action.UnavailableReason;
    }

    private void ResolveEnemyTurn(List<CombatEvent> events)
    {
        events.Add(new CombatEvent(CombatEventType.EnemyAction, _round, actorName: _enemy.Name.ToUpperInvariant(), detail: "The enemy lunges at you."));
        if (_dodging)
        {
            events.Add(new CombatEvent(CombatEventType.Check, _round, detail: "You evade the enemy's attack."));
        }
        else
        {
            int damage = _defending ? 1 : _enemyAttackDamage;
            int appliedDamage = _player.TakeDamage(damage);
            events.Add(new CombatEvent(CombatEventType.Damage, _round, targetName: _player.Name, amount: appliedDamage));
        }

        _defending = false;
        _dodging = false;
    }

    private void CompleteVictory(List<CombatEvent> events)
    {
        _encounterOver = true;
        IsVictory = true;
        _player.AddGold(_victoryReward);
        events.Add(new CombatEvent(CombatEventType.StatusChanged, _round, detail: "The enemy falls. You win the encounter."));
        events.Add(new CombatEvent(CombatEventType.StatusChanged, _round, detail: "REWARD · You receive 25 G."));
    }

    private CombatResult Success(List<CombatEvent> events)
    {
        return new CombatResult(true, GetSnapshot(), events.ToArray());
    }

    private CombatResult Failure(string reason)
    {
        return new CombatResult(false, GetSnapshot(), Array.Empty<CombatEvent>(), reason);
    }

    private string GetEnemyCondition()
    {
        if (!_enemy.IsAlive) return "Defeated";
        if (_enemy.Health <= _enemy.MaxHealth / 2) return "Wounded";
        return "Healthy";
    }
}
