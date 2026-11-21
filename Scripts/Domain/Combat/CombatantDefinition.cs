using System;
using System.Collections.Generic;
using System.Linq;

public enum CombatantSide
{
    Party,
    Enemy
}

public sealed class CombatantDefinition
{
    public Character Character { get; }
    public CombatantSide Side { get; }
    public CombatActionDefinition[] Actions { get; }
    public bool IsVisible { get; }
    public EnemyBehaviorProfile BehaviorProfile { get; }

    public CombatantDefinition(
        Character character,
        CombatantSide side,
        IEnumerable<CombatActionDefinition> actions,
        bool isVisible = true,
        EnemyBehaviorProfile behaviorProfile = null)
    {
        Character = character ?? throw new ArgumentNullException(nameof(character));
        if (!Enum.IsDefined(typeof(CombatantSide), side))
        {
            throw new ArgumentOutOfRangeException(nameof(side));
        }

        if (actions == null)
        {
            throw new ArgumentNullException(nameof(actions));
        }

        Actions = actions.ToArray();
        if (Actions.Length == 0)
        {
            throw new ArgumentException("A combatant must have at least one combat action.", nameof(actions));
        }

        HashSet<string> actionIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (CombatActionDefinition action in Actions)
        {
            if (action == null || !actionIds.Add(action.Id))
            {
                throw new ArgumentException("Combatant actions must be non-null and have unique IDs.", nameof(actions));
            }
        }

        if (behaviorProfile != null && side != CombatantSide.Enemy)
        {
            throw new ArgumentException("Only enemies can have an enemy behavior profile.", nameof(behaviorProfile));
        }

        if (behaviorProfile != null && Actions.Any(action => !behaviorProfile.ActionWeights.ContainsKey(action.Id)))
        {
            throw new ArgumentException("A behavior profile must define a weight for every enemy action.", nameof(behaviorProfile));
        }

        if (behaviorProfile != null && behaviorProfile.ActionWeights.Keys.Any(id => !actionIds.Contains(id)))
        {
            throw new ArgumentException("A behavior profile cannot reference an action the enemy does not have.", nameof(behaviorProfile));
        }

        Side = side;
        IsVisible = side == CombatantSide.Party || isVisible;
        BehaviorProfile = behaviorProfile;
    }
}
