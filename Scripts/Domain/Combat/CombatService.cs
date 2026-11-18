using System;
using System.Collections.Generic;

public sealed class CombatService : ICombatService
{
    private const int _playerAttackDamage = 20;
    private const int _enemyAttackDamage = 4;
    private const int _victoryReward = 25;
    private const int _defendStaminaCost = 1;
    private const int _dodgeStaminaCost = 2;
    private const int _catchBreathRecovery = 3;

    private readonly Character _player;
    private readonly Character _enemy;
    private int _round;
    private bool _encounterOver;
    private bool _defending;
    private bool _dodging;

    public bool IsVictory { get; private set; }
    public bool IsEncounterOver => _encounterOver;
    public int RewardGold => IsVictory ? _victoryReward : 0;

    public CombatService(Character player, Character enemy)
    {
        _player = player ?? throw new ArgumentNullException(nameof(player));
        _enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
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

        CombatActionOption[] actions = canAct
            ? new[]
            {
                new CombatActionOption("attack", "Attack", "No Stamina cost", "Deal 20 damage to the enemy.", targetId: _enemy.Id),
                new CombatActionOption("defend", "Defend", _defendStaminaCost + " Stamina", "Reduce the next enemy hit to 1 damage.", _player.Stamina >= _defendStaminaCost, "Not enough Stamina."),
                new CombatActionOption("dodge", "Dodge", _dodgeStaminaCost + " Stamina", "Avoid the next enemy attack.", _player.Stamina >= _dodgeStaminaCost, "Not enough Stamina."),
                new CombatActionOption("catch_breath", "Catch Breath", "Recover " + _catchBreathRecovery + " Stamina", "Recover your composure; the enemy still gets its turn.", _player.Stamina < _player.MaxStamina, "Stamina is already full.")
            }
            : Array.Empty<CombatActionOption>();

        EnemyCombatStatus[] enemies =
        {
            new EnemyCombatStatus(_enemy.Id, _enemy.Name, _enemy.Health, _enemy.MaxHealth, GetEnemyCondition())
        };

        return new CombatSnapshot(_round, "PLAYER TURN", enemies, intent, actions);
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

        List<CombatEvent> events = new List<CombatEvent>();

        if (request.ActionId == "attack")
        {
            if (request.TargetId != _enemy.Id)
            {
                return Failure("Choose the enemy as your target.");
            }

            events.Add(new CombatEvent(CombatEventType.PlayerAction, _round, detail: "You attack the enemy with a basic strike."));
            int damage = _enemy.TakeDamage(_playerAttackDamage);
            events.Add(new CombatEvent(CombatEventType.Damage, _round, targetName: _enemy.Name, amount: damage));
        }
        else if (request.ActionId == "defend")
        {
            if (!_player.SpendStamina(_defendStaminaCost))
            {
                return Failure("Not enough Stamina to defend.");
            }

            _defending = true;
            events.Add(new CombatEvent(CombatEventType.PlayerAction, _round, detail: "You take a guarded stance."));
        }
        else if (request.ActionId == "dodge")
        {
            if (!_player.SpendStamina(_dodgeStaminaCost))
            {
                return Failure("Not enough Stamina to dodge.");
            }

            _dodging = true;
            events.Add(new CombatEvent(CombatEventType.PlayerAction, _round, detail: "You prepare to evade the next attack."));
        }
        else if (request.ActionId == "catch_breath")
        {
            if (_player.Stamina >= _player.MaxStamina)
            {
                return Failure("Your Stamina is already full.");
            }

            int restored = _player.RestoreStamina(_catchBreathRecovery);
            events.Add(new CombatEvent(
                CombatEventType.PlayerAction,
                _round,
                detail: "You catch your breath and recover " + restored + " Stamina."));
        }
        else
        {
            return Failure("That action is not available.");
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
