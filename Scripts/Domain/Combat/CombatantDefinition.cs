using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Identifies whether a combatant belongs to the party or the enemy side.</summary>
public enum CombatantSide
{
    Party,
    Enemy
}

/// <summary>Defines the combatant data and its validation rules.</summary>
public sealed class CombatantDefinition
{
    /// <summary>Gets the character.</summary>
    public Character Character { get; }
    /// <summary>Gets the side.</summary>
    public CombatantSide Side { get; }
    /// <summary>Gets the actions.</summary>
    public CombatActionDefinition[] Actions { get; }
    /// <summary>Gets a value indicating whether visible.</summary>
    public bool IsVisible { get; }
    /// <summary>Gets the behavior profile.</summary>
    public EnemyBehaviorProfile BehaviorProfile { get; }

    /// <summary>Initializes a new instance of <see cref="CombatantDefinition"/>.</summary>
    /// <param name="character">The character used to initialize the instance.</param>
    /// <param name="side">The side used to initialize the instance.</param>
    /// <param name="actions">The actions used to initialize the instance.</param>
    /// <param name="isVisible">The is visible used to initialize the instance.</param>
    /// <param name="behaviorProfile">The behavior profile used to initialize the instance.</param>
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
