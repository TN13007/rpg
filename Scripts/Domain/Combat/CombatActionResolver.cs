using System;
using System.Collections.Generic;

public sealed class CombatActionResolver
{
    public CombatEffectRequest[] Resolve(CombatActionDefinition action, string actorId, string targetId)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (string.IsNullOrWhiteSpace(actorId)) throw new ArgumentException("Actor ID is required.", nameof(actorId));
        List<CombatEffectRequest> effects = new List<CombatEffectRequest>();
        switch (action.Behavior)
        {
            case CombatActionBehavior.Attack:
                effects.Add(new CombatEffectRequest(CombatEffectType.Damage, targetId, action.Damage));
                break;
            case CombatActionBehavior.Defend:
                effects.Add(new CombatEffectRequest(CombatEffectType.Guard, actorId));
                break;
            case CombatActionBehavior.Dodge:
                effects.Add(new CombatEffectRequest(CombatEffectType.Evade, actorId));
                break;
            case CombatActionBehavior.RecoverStamina:
                effects.Add(new CombatEffectRequest(CombatEffectType.RestoreStamina, actorId, action.RecoveryAmount));
                break;
            case CombatActionBehavior.Delay:
                if (action.Damage > 0) effects.Add(new CombatEffectRequest(CombatEffectType.Damage, targetId, action.Damage));
                effects.Add(new CombatEffectRequest(CombatEffectType.Delay, targetId, duration: action.DelayAmount));
                break;
            case CombatActionBehavior.Stun:
                if (action.Damage > 0) effects.Add(new CombatEffectRequest(CombatEffectType.Damage, targetId, action.Damage));
                effects.Add(new CombatEffectRequest(CombatEffectType.Stun, targetId, duration: action.StunDuration));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action.Behavior));
        }
        return effects.ToArray();
    }
}
