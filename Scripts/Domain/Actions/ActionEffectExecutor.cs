using System;
using System.Collections.Generic;

/// <summary>Prepares and applies the configured effects of an Action.</summary>
public sealed class ActionEffectExecutor
{
    /// <summary>Validates and snapshots every configured effect without changing the target.</summary>
    /// <param name="action">The authored Action whose effects will be prepared.</param>
    /// <returns>An immutable plan containing the validated effects.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
    /// <exception cref="InvalidOperationException">An effect is null or contains invalid data.</exception>
    public ActionEffectPlan Prepare(ActionDefinition action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        List<ActionEffectDefinition> preparedEffects = new List<ActionEffectDefinition>();
        foreach (ActionEffectDefinition effect in action.Effects)
        {
            if (effect == null)
            {
                throw new InvalidOperationException("An Action cannot contain a null effect.");
            }

            if (!Enum.IsDefined(typeof(ActionEffectType), effect.Type))
            {
                throw new InvalidOperationException("An Action contains an unknown effect type.");
            }

            if (effect.Type != ActionEffectType.ChangeMood && effect.Amount < 0)
            {
                throw new InvalidOperationException("This Action effect cannot have a negative amount.");
            }

            preparedEffects.Add(effect);
        }

        return new ActionEffectPlan(preparedEffects);
    }

    /// <summary>Applies a previously validated effect plan to the target character.</summary>
    /// <param name="plan">The immutable effect plan to apply.</param>
    /// <param name="target">The character that owns the affected resources.</param>
    public void Apply(ActionEffectPlan plan, Character target)
    {
        if (plan == null)
        {
            throw new ArgumentNullException(nameof(plan));
        }

        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        foreach (ActionEffectDefinition effect in plan.Effects)
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

    /// <summary>Validates and immediately applies an Action's effects for existing direct callers.</summary>
    /// <param name="action">The authored Action whose effects will be applied.</param>
    /// <param name="target">The character that owns the affected resources.</param>
    public void Apply(ActionDefinition action, Character target)
    {
        Apply(Prepare(action), target);
    }
}
