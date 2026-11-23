using System;
using System.Collections.Generic;

/// <summary>Validates authored Action effects and calculates their immutable result.</summary>
public sealed class ActionEffectExecutor
{
    /// <summary>Validates and snapshots every configured effect without changing runtime state.</summary>
    public ActionEffectPlan Prepare(ActionDefinition action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        List<ActionEffectDefinition> preparedEffects = new List<ActionEffectDefinition>();
        foreach (ActionEffectDefinition effect in action.Effects)
        {
            if (effect == null)
                throw new InvalidOperationException("An Action cannot contain a null effect.");
            if (!Enum.IsDefined(typeof(ActionEffectType), effect.Type))
                throw new InvalidOperationException("An Action contains an unknown effect type.");
            if (effect.Amount <= 0)
                throw new InvalidOperationException("Action effect amounts must be greater than zero.");
            preparedEffects.Add(effect);
        }
        return new ActionEffectPlan(preparedEffects);
    }

    /// <summary>Calculates and returns a new Character state without mutating the supplied snapshot.</summary>
    public CharacterRuntimeState Apply(ActionEffectPlan plan, CharacterRuntimeState state)
    {
        if (plan == null) throw new ArgumentNullException(nameof(plan));
        if (state == null) throw new ArgumentNullException(nameof(state));
        CharacterRuntimeState result = state;
        foreach (ActionEffectDefinition effect in plan.Effects)
            result = result.Apply(effect);
        return result;
    }

    /// <summary>Compatibility facade that calculates then commits effects through the Character owner.</summary>
    public void Apply(ActionEffectPlan plan, Character target)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));
        CharacterRuntimeState result = Apply(plan, CharacterRuntimeState.Capture(target));
        target.ApplyRuntimeState(result);
    }

    /// <summary>Validates and immediately applies an Action's effects for existing direct callers.</summary>
    public void Apply(ActionDefinition action, Character target)
    {
        Apply(Prepare(action), target);
    }
}
