using System;
using System.Collections.Generic;

/// <summary>Applies resolved combat effects to their target combatants.</summary>
public sealed class CombatEffectDispatcher
{
    /// <summary>Applies one resolved effect to the relevant runtime state.</summary>
    /// <param name="request">The effect request to apply.</param>
    /// <param name="actor">The character responsible for the action.</param>
    /// <param name="target">The character receiving the effect.</param>
    /// <param name="actorState">The actor's temporary combat effect state.</param>
    /// <param name="targetState">The target's temporary combat effect state.</param>
    /// <param name="events">The ordered event list for the current operation.</param>
    /// <param name="round">The current Round number.</param>
    /// <param name="actionPool">The Party's shared Action Point pool.</param>
    /// <param name="applyPartyControlPenalty">Whether this control effect was caused by an enemy against a Party member.</param>
    public void Apply(
        CombatEffectRequest request,
        Character actor,
        Character target,
        CombatantEffectState actorState,
        CombatantEffectState targetState,
        List<CombatEvent> events,
        int round,
        ActionPool actionPool,
        bool applyPartyControlPenalty)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (actor == null) throw new ArgumentNullException(nameof(actor));
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (actorState == null) throw new ArgumentNullException(nameof(actorState));
        if (targetState == null) throw new ArgumentNullException(nameof(targetState));
        if (events == null) throw new ArgumentNullException(nameof(events));
        if (actionPool == null) throw new ArgumentNullException(nameof(actionPool));

        switch (request.Type)
        {
            case CombatEffectType.Damage:
                int damage = targetState.ApplyIncomingDamage(request.Amount);
                if (damage == 0)
                {
                    events.Add(new CombatEvent(
                        CombatEventType.Check,
                        round,
                        detail: target.Name + " avoids the attack."));
                    return;
                }

                int appliedDamage = target.TakeDamage(damage);
                events.Add(new CombatEvent(
                    CombatEventType.Damage,
                    round,
                    targetName: target.Name,
                    amount: appliedDamage));
                break;

            case CombatEffectType.Guard:
                actorState.Guard();
                events.Add(new CombatEvent(
                    CombatEventType.Check,
                    round,
                    actorName: actor.Name,
                    detail: actor.Name + " takes a guarded stance."));
                break;

            case CombatEffectType.Evade:
                actorState.Evade();
                events.Add(new CombatEvent(
                    CombatEventType.Check,
                    round,
                    actorName: actor.Name,
                    detail: actor.Name + " prepares to evade the next attack."));
                break;

            case CombatEffectType.RestoreStamina:
                int restored = actor.RestoreStamina(request.Amount);
                events.Add(new CombatEvent(
                    CombatEventType.Check,
                    round,
                    actorName: actor.Name,
                    amount: restored,
                    detail: actor.Name + " recovers " + restored + " Stamina."));
                break;

            case CombatEffectType.Control:
                if (applyPartyControlPenalty && target.IsAlive)
                {
                    actionPool.QueueNextPlayerTurnPenalty();
                    events.Add(new CombatEvent(
                        CombatEventType.StatusChanged,
                        round,
                        actorName: actor.Name,
                        targetName: target.Name,
                        detail: target.Name + " is disoriented; the next Player Turn loses 1 AP."));
                }
                break;

            case CombatEffectType.Revive:
                int revivalHealth = Math.Max(1, (int)Math.Ceiling(target.MaxHealth * 0.25));
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