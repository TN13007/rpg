using System;
using System.Collections.Generic;

/// <summary>Applies resolved combat effects to their target combatants.</summary>
public sealed class CombatEffectDispatcher
{
    /// <summary>Applies apply to the target combatant.</summary>
    /// <param name="request">The request used by this operation.</param>
    /// <param name="actor">The actor used by this operation.</param>
    /// <param name="target">The target used by this operation.</param>
    /// <param name="timeline">The timeline used by this operation.</param>
    /// <param name="actorState">The actor state used by this operation.</param>
    /// <param name="targetState">The target state used by this operation.</param>
    /// <param name="events">The events used by this operation.</param>
    /// <param name="round">The round used by this operation.</param>
    public void Apply(
        CombatEffectRequest request, Character actor, Character target,
        CombatTimeline timeline, CombatantEffectState actorState,
        CombatantEffectState targetState, List<CombatEvent> events, int round)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (actor == null) throw new ArgumentNullException(nameof(actor));
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (timeline == null) throw new ArgumentNullException(nameof(timeline));
        if (actorState == null) throw new ArgumentNullException(nameof(actorState));
        if (targetState == null) throw new ArgumentNullException(nameof(targetState));
        if (events == null) throw new ArgumentNullException(nameof(events));

        switch (request.Type)
        {
            case CombatEffectType.Damage:
                int damage = targetState.ApplyIncomingDamage(request.Amount);
                if (damage == 0)
                {
                    events.Add(new CombatEvent(CombatEventType.Check, round, detail: target.Name + " avoids the attack."));
                    return;
                }
                int appliedDamage = target.TakeDamage(damage);
                events.Add(new CombatEvent(CombatEventType.Damage, round, targetName: target.Name, amount: appliedDamage));
                break;
            case CombatEffectType.Guard:
                actorState.Guard();
                events.Add(new CombatEvent(CombatEventType.Check, round, actorName: actor.Name, detail: actor.Name + " takes a guarded stance."));
                break;
            case CombatEffectType.Evade:
                actorState.Evade();
                events.Add(new CombatEvent(CombatEventType.Check, round, actorName: actor.Name, detail: actor.Name + " prepares to evade the next attack."));
                break;
            case CombatEffectType.RestoreStamina:
                int restored = actor.RestoreStamina(request.Amount);
                events.Add(new CombatEvent(CombatEventType.Check, round, actorName: actor.Name, amount: restored, detail: actor.Name + " recovers " + restored + " Stamina."));
                break;
            case CombatEffectType.Delay:
                if (target.IsAlive)
                {
                    timeline.ApplyDelay(request.TargetId, request.Duration);
                    events.Add(new CombatEvent(CombatEventType.StatusChanged, round, targetName: target.Name, detail: target.Name + " is delayed."));
                }
                break;
            case CombatEffectType.Stun:
                if (target.IsAlive)
                {
                    timeline.ApplyStun(request.TargetId, request.Duration);
                    events.Add(new CombatEvent(CombatEventType.StatusChanged, round, targetName: target.Name, detail: target.Name + " is stunned."));
                }
                break;
            case CombatEffectType.Revive:
                int revivalHealth = System.Math.Max(1, (int)System.Math.Ceiling(target.MaxHealth * 0.25));
                int restoredHealth = target.Revive(revivalHealth);
                if (restoredHealth > 0)
                {
                    events.Add(new CombatEvent(
                        CombatEventType.StatusChanged,
                        round,
                        actorName: actor.Name,
                        targetName: target.Name,
                        amount: restoredHealth,
                        detail: target.Name + " is revived with " + restoredHealth + " HP."));
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(request.Type));
        }
    }
}
