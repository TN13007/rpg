using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Provides enemy behavior profile functionality for the application.</summary>
public sealed class EnemyBehaviorProfile
{
    private readonly Dictionary<string, int> _actionWeights;

    /// <summary>Gets the configured weight for each enemy action ID. A weight of zero disables that action.</summary>
    public IReadOnlyDictionary<string, int> ActionWeights => _actionWeights;

    /// <summary>Initializes a new instance of <see cref="EnemyBehaviorProfile"/>.</summary>
    /// <param name="actionWeights">Maps each enemy action ID to its selection weight. Weights must be non-negative, and at least one action must have a positive weight.</param>
    public EnemyBehaviorProfile(IDictionary<string, int> actionWeights)
    {
        if (actionWeights == null) throw new ArgumentNullException(nameof(actionWeights));
        _actionWeights = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (KeyValuePair<string, int> entry in actionWeights)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
                throw new ArgumentException("Enemy action IDs cannot be empty.", nameof(actionWeights));
            if (entry.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(actionWeights), "Enemy action weights cannot be negative.");
            _actionWeights.Add(entry.Key, entry.Value);
        }
        if (_actionWeights.Count == 0 || !_actionWeights.Values.Any(weight => weight > 0))
            throw new ArgumentException("An enemy behavior profile must enable at least one action.", nameof(actionWeights));
    }

    /// <summary>Returns the configured selection weight for an enemy action.</summary>
    /// <param name="actionId">The action id used by this operation.</param>
    /// <returns>The configured weight, or zero when the action ID is not present.</returns>
    public int GetWeight(string actionId)
    {
        return _actionWeights.TryGetValue(actionId, out int weight) ? weight : 0;
    }
}
