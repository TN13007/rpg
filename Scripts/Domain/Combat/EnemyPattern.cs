using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Advances through an enemy's authored action sequence in a deterministic repeating order.</summary>
public sealed class EnemyPattern
{
    private readonly string[] _actionIds;
    private int _nextActionIndex;

    /// <summary>Gets a copy of the ordered action identifiers in this pattern.</summary>
    public string[] ActionIds => (string[])_actionIds.Clone();

    /// <summary>Initializes a non-empty repeating enemy action pattern.</summary>
    /// <param name="actionIds">The ordered action IDs. Repeated IDs are allowed to author deliberate repetition.</param>
    public EnemyPattern(IEnumerable<string> actionIds)
    {
        if (actionIds == null)
        {
            throw new ArgumentNullException(nameof(actionIds));
        }

        _actionIds = actionIds.ToArray();
        if (_actionIds.Length == 0 || _actionIds.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("An enemy pattern requires at least one valid action ID.", nameof(actionIds));
        }

        _nextActionIndex = 0;
    }

    /// <summary>Returns the next action ID and advances the pattern, wrapping to its first step when needed.</summary>
    public string NextActionId()
    {
        string actionId = _actionIds[_nextActionIndex];
        _nextActionIndex = (_nextActionIndex + 1) % _actionIds.Length;
        return actionId;
    }

    /// <summary>Resets the next step to the beginning of the pattern.</summary>
    public void Reset()
    {
        _nextActionIndex = 0;
    }

    /// <summary>Creates an independent runtime cursor with the same authored action sequence.</summary>
    public EnemyPattern CreateRuntimeCopy()
    {
        return new EnemyPattern(_actionIds);
    }
}