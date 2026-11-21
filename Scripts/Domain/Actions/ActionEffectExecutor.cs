using System;

/// <summary>Applies the configured action effect effects to a target.</summary>
public sealed class ActionEffectExecutor
{
    /// <summary>Applies apply to the target character.</summary>
    /// <param name="action">The action used by this operation.</param>
    /// <param name="target">The target used by this operation.</param>
    public void Apply(ActionDefinition action, Character target)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        foreach (ActionEffectDefinition effect in action.Effects)
        {
            switch (effect.Type)
            {
                case ActionEffectType.RestoreHealth:
                    target.RestoreHealth(effect.Amount);
                    break;

                case ActionEffectType.RestoreFatigue:
                    target.RestoreFatigue(effect.Amount);
                    break;

                case ActionEffectType.RestoreStamina:
                    target.RestoreStamina(effect.Amount);
                    break;

                case ActionEffectType.ChangeMood:
                    target.ChangeMood(effect.Amount);
                    break;

                case ActionEffectType.AddGold:
                    target.AddGold(effect.Amount);
                    break;

                case ActionEffectType.Revive:
                    target.Revive(effect.Amount);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(effect.Type));
            }
        }
    }
}
