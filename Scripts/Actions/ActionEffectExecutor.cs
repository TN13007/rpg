using System;

public sealed class ActionEffectExecutor
{
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

                case ActionEffectType.RestoreEnergy:
                    target.RestoreEnergy(effect.Amount);
                    break;

                case ActionEffectType.ChangeMood:
                    target.ChangeMood(effect.Amount);
                    break;

                case ActionEffectType.AddGold:
                    target.AddGold(effect.Amount);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(effect.Type));
            }
        }
    }
}
