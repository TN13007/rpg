using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Identifies whether a combatant belongs to the Party or Enemy side.</summary>
public enum CombatantSide
{
    /// <summary>A combatant controlled by the player.</summary>
    Party,
    /// <summary>A combatant controlled by the combat encounter.</summary>
    Enemy
}

/// <summary>Defines the immutable combatant data used to compose an encounter roster.</summary>
public sealed class CombatantDefinition
{
    /// <summary>Gets the character instance retained and mutated by the combat service.</summary>
    public Character Character { get; }
    /// <summary>Gets the combatant's side.</summary>
    public CombatantSide Side { get; }
    /// <summary>Gets the actions authored for this combatant.</summary>
    public CombatActionDefinition[] Actions { get; }
    /// <summary>Gets whether this enemy is visible to the player.</summary>
    public bool IsVisible { get; }
    /// <summary>Gets the authored deterministic pattern for an enemy, or null for Party members.</summary>
    public EnemyPattern EnemyPattern { get; }

    /// <summary>Creates a combatant definition and validates its actions and optional enemy pattern.</summary>
    public CombatantDefinition(
        Character character,
        CombatantSide side,
        IEnumerable<CombatActionDefinition> actions,
        bool isVisible = true,
        EnemyPattern enemyPattern = null)
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

        if (side == CombatantSide.Party && enemyPattern != null)
        {
            throw new ArgumentException("Only enemies can have an authored enemy pattern.", nameof(enemyPattern));
        }

        if (enemyPattern != null && enemyPattern.ActionIds.Any(id => !actionIds.Contains(id)))
        {
            throw new ArgumentException("An enemy pattern can reference only actions authored for that enemy.", nameof(enemyPattern));
        }

        Side = side;
        IsVisible = side == CombatantSide.Party || isVisible;
        EnemyPattern = enemyPattern;
    }
}