public enum CombatMessageType
{
    Player,
    Enemy,
    System
}

public sealed class CombatMessage
{
    public CombatMessageType Type { get; }
    public string Speaker { get; }
    public string Category { get; }
    public string Text { get; }

    public CombatMessage(CombatMessageType type, string speaker, string category, string text)
    {
        Type = type;
        Speaker = speaker;
        Category = category;
        Text = text;
    }
}

public sealed class EnemyCombatStatus
{
    public string Name { get; }
    public int CurrentHealth { get; }
    public int MaxHealth { get; }
    public string Condition { get; }

    public EnemyCombatStatus(string name, int currentHealth, int maxHealth, string condition)
    {
        Name = name;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        Condition = condition;
    }
}

public sealed class EnemyIntent
{
    public string EnemyName { get; }
    public string ActionName { get; }
    public string Description { get; }
    public string ConfidenceLabel { get; }

    public EnemyIntent(string enemyName, string actionName, string description, string confidenceLabel)
    {
        EnemyName = enemyName;
        ActionName = actionName;
        Description = description;
        ConfidenceLabel = confidenceLabel;
    }
}

public sealed class CombatActionOption
{
    public string Id { get; }
    public string Label { get; }
    public string CostLabel { get; }
    public string Description { get; }
    public bool IsAvailable { get; }

    public CombatActionOption(string id, string label, string costLabel, string description, bool isAvailable = true)
    {
        Id = id;
        Label = label;
        CostLabel = costLabel;
        Description = description;
        IsAvailable = isAvailable;
    }
}
