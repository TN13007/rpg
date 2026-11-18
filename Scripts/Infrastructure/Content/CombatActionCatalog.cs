using System;
using System.Collections.Generic;

public sealed class CombatActionCatalog
{
    private readonly Dictionary<string, CombatActionDefinition> _actions;

    public IEnumerable<CombatActionDefinition> All => _actions.Values;

    public CombatActionCatalog(IEnumerable<CombatActionDefinition> actions)
    {
        if (actions == null) throw new ArgumentNullException(nameof(actions));

        _actions = new Dictionary<string, CombatActionDefinition>(StringComparer.Ordinal);
        foreach (CombatActionDefinition action in actions)
        {
            if (action == null) throw new InvalidOperationException("Combat action definitions cannot contain null values.");
            if (!_actions.TryAdd(action.Id, action))
            {
                throw new InvalidOperationException("Duplicate combat action ID: " + action.Id);
            }
        }
    }

    public CombatActionDefinition Get(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Combat action ID is required.", nameof(id));
        if (!_actions.TryGetValue(id, out CombatActionDefinition action))
        {
            throw new KeyNotFoundException("Combat action was not found: " + id);
        }
        return action;
    }

    public CombatActionDefinition[] Resolve(IEnumerable<string> ids)
    {
        if (ids == null) throw new ArgumentNullException(nameof(ids));

        List<CombatActionDefinition> resolved = new List<CombatActionDefinition>();
        HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (string id in ids)
        {
            if (!seen.Add(id))
            {
                throw new InvalidOperationException("Duplicate combat action reference: " + id);
            }
            resolved.Add(Get(id));
        }
        return resolved.ToArray();
    }
}
