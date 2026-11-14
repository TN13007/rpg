using System;
using System.Collections.Generic;

public sealed class CombatService : ICombatService
{
    private const int PlayerAttackDamage = 20;
    private const int GoblinAttackDamage = 4;
    private const int VictoryReward = 25;

    private readonly Character _player;
    private readonly Character _enemy;
    private int _round;
    private bool _encounterOver;
    private bool _defending;
    private bool _dodging;

    public bool IsVictory { get; private set; }
    public bool IsEncounterOver => _encounterOver;
    public int RewardGold => IsVictory ? VictoryReward : 0;

    public CombatService(Character player, Character enemy)
    {
        _player = player ?? throw new ArgumentNullException(nameof(player));
        _enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
    }

    public void StartEncounter()
    {
        _enemy.Reset();
        _round = 1;
        _encounterOver = false;
        IsVictory = false;
        _defending = false;
        _dodging = false;
    }

    public CombatSnapshot GetSnapshot()
    {
        IntentObservation intent = _encounterOver
            ? null
            : new IntentObservation(_enemy.Id, _enemy.Name, "The goblin is watching you and preparing to strike.", IntentConfidence.Suspected);

        CombatActionOption[] actions = _encounterOver
            ? Array.Empty<CombatActionOption>()
            : new[]
            {
                new CombatActionOption("attack", "Attack", "No cost", "Deal 20 damage to the goblin.", targetId: _enemy.Id),
                new CombatActionOption("defend", "Defend", "1 Energy", "Reduce the next goblin hit to 1 damage.", _player.Energy >= 1, "Not enough Energy."),
                new CombatActionOption("dodge", "Dodge", "2 Energy", "Avoid the goblin's next attack.", _player.Energy >= 2, "Not enough Energy.")
            };

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

        List<CombatEvent> events = new List<CombatEvent>();

        if (request.ActionId == "attack")
        {
            if (request.TargetId != _enemy.Id)
            {
                return Failure("Choose the goblin as your target.");
            }

            events.Add(new CombatEvent(CombatEventType.PlayerAction, _round, detail: "You attack the goblin with a basic strike."));
            int damage = _enemy.TakeDamage(PlayerAttackDamage);
            events.Add(new CombatEvent(CombatEventType.Damage, _round, targetName: _enemy.Name, amount: damage));
        }
        else if (request.ActionId == "defend")
        {
            if (!_player.SpendEnergy(1))
            {
                return Failure("Not enough Energy to defend.");
            }

            _defending = true;
            events.Add(new CombatEvent(CombatEventType.PlayerAction, _round, detail: "You take a guarded stance."));
        }
        else if (request.ActionId == "dodge")
        {
            if (!_player.SpendEnergy(2))
            {
                return Failure("Not enough Energy to dodge.");
            }

            _dodging = true;
            events.Add(new CombatEvent(CombatEventType.PlayerAction, _round, detail: "You prepare to evade the next attack."));
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
        if (!_player.IsAlive)
        {
            _encounterOver = true;
            events.Add(new CombatEvent(CombatEventType.StatusChanged, _round, detail: "You collapse. The encounter is over."));
        }

        _round++;
        return Success(events);
    }

    private void ResolveEnemyTurn(List<CombatEvent> events)
    {
        events.Add(new CombatEvent(CombatEventType.EnemyAction, _round, actorName: _enemy.Name.ToUpperInvariant(), detail: "The goblin lunges at you."));

        if (_dodging)
        {
            events.Add(new CombatEvent(CombatEventType.Check, _round, detail: "You evade the goblin's attack."));
        }
        else
        {
            int damage = _defending ? 1 : GoblinAttackDamage;
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
        _player.AddGold(VictoryReward);
        events.Add(new CombatEvent(CombatEventType.StatusChanged, _round, detail: "The goblin falls. You win the encounter."));
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
