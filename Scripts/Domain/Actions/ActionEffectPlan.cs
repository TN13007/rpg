using System;
using System.Collections.Generic;

/// <summary>Immutable snapshot of the effects validated during Action preparation.</summary>
public sealed class ActionEffectPlan
{
    /// <summary>Gets the validated effects in their authored application order.</summary>
    public IReadOnlyList<ActionEffectDefinition> Effects { get; }

    internal ActionEffectPlan(IEnumerable<ActionEffectDefinition> effects)
    {
        if (effects == null)
        {
            throw new ArgumentNullException(nameof(effects));
        }

        Effects = Array.AsReadOnly(new List<ActionEffectDefinition>(effects).ToArray());
    }
}
