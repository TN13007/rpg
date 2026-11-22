using System;
using System.Collections.Generic;

/// <summary>Resolves an authored combat action into its domain effects.</summary>
public sealed class CombatActionResolver
{
    /// <summary>Creates the ordered effect requests produced by the supplied action.</summary>
    public CombatEffectRequest[] Resolve(CombatActionDefinition action, string actorId, string targetId)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor ID is required.", nameof(actorId));
        }

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
            case CombatActionBehavior.Control:
                if (action.Damage > 0)
                {
                    effects.Add(new CombatEffectRequest(CombatEffectType.Damage, targetId, action.Damage));
                }
                effects.Add(new CombatEffectRequest(CombatEffectType.Control, targetId));
                break;
            case CombatActionBehavior.Revive:
                effects.Add(new CombatEffectRequest(CombatEffectType.Revive, targetId));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action.Behavior));
        }

        return effects.ToArray();
    }
}