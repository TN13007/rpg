using System;
using System.Collections.Generic;
using System.Linq;

public sealed class EnemyBehaviorProfile
{
    private readonly Dictionary<string, int> _actionWeights;

    public IReadOnlyDictionary<string, int> ActionWeights => _actionWeights;

    public EnemyBehaviorProfile(IDictionary<string, int> actionWeights)
    {
        if (actionWeights == null)
        {
            throw new ArgumentNullException(nameof(actionWeights));
        }

        _actionWeights = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (KeyValuePair<string, int> entry in actionWeights)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
            {
                throw new ArgumentException("Enemy action IDs cannot be empty.", nameof(actionWeights));
            }

            if (entry.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(actionWeights), "Enemy action weights cannot be negative.");
            }

            _actionWeights.Add(entry.Key, entry.Value);
        }

        if (_actionWeights.Count == 0 || !_actionWeights.Values.AnyWeight())
        {
            throw new ArgumentException("An enemy behavior profile must enable at least one action.", nameof(actionWeights));
        }
    }

    public int GetWeight(string actionId)
    {
        return _actionWeights.TryGetValue(actionId, out int weight) ? weight : 0;
    }
}

internal static class EnemyBehaviorProfileWeightExtensions
{
    public static bool AnyWeight(this IEnumerable<int> weights)
    {
        foreach (int weight in weights)
        {
            if (weight > 0)
            {
                return true;
            }
        }

        return false;
    }
}
