using System;
using System.Collections.Generic;

public sealed class CharacterData
{
    public string Id { get; }
    public string Name { get; }
    public string Role { get; }
    public int Level { get; }
    public int MaxHealth { get; }
    public double MaxFatigue { get; }
    public int MaxStamina { get; }
    public int MaxMood { get; }
    public int StartingHealth { get; }
    public double StartingFatigue { get; }
    public int StartingStamina { get; }
    public int StartingMood { get; }
    public int StartingGold { get; }
    public CharacterStats Stats { get; }
    public PlayerActionDefinition[] PlayerActions { get; }
    public string[] CombatActionIds { get; }

    public CharacterData(
        string id,
        string name,
        string role,
        int level,
        int maxHealth,
        double maxFatigue,
        int maxStamina,
        int maxMood,
        int startingHealth,
        double startingFatigue,
        int startingStamina,
        int startingMood,
        int startingGold,
        CharacterStats stats = null,
        PlayerActionDefinition[] playerActions = null,
        string[] combatActionIds = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Character ID is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Character name is required.", nameof(name));
        if (level < 1 || maxHealth < 1 || maxFatigue < 0 || maxStamina < 0 || maxMood < 0)
            throw new ArgumentOutOfRangeException(nameof(level), "Character base values are invalid.");
        if (startingHealth < 0 || startingHealth > maxHealth) throw new ArgumentOutOfRangeException(nameof(startingHealth));
        if (double.IsNaN(startingFatigue) || double.IsInfinity(startingFatigue) || startingFatigue < 0 || startingFatigue > maxFatigue)
            throw new ArgumentOutOfRangeException(nameof(startingFatigue));
        if (startingStamina < 0 || startingStamina > maxStamina) throw new ArgumentOutOfRangeException(nameof(startingStamina));
        if (startingMood < 0 || startingMood > maxMood) throw new ArgumentOutOfRangeException(nameof(startingMood));
        if (startingGold < 0) throw new ArgumentOutOfRangeException(nameof(startingGold));

        Id = id;
        Name = name;
        Role = role ?? string.Empty;
        Level = level;
        MaxHealth = maxHealth;
        MaxFatigue = maxFatigue;
        MaxStamina = maxStamina;
        MaxMood = maxMood;
        StartingHealth = startingHealth;
        StartingFatigue = startingFatigue;
        StartingStamina = startingStamina;
        StartingMood = startingMood;
        StartingGold = startingGold;
        Stats = stats ?? new CharacterStats(0, 0, 0, 0, 0);
        PlayerActions = playerActions ?? Array.Empty<PlayerActionDefinition>();
        CombatActionIds = combatActionIds ?? Array.Empty<string>();

        HashSet<string> playerActionIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (PlayerActionDefinition playerAction in PlayerActions)
        {
            if (playerAction == null || !playerActionIds.Add(playerAction.Action.Id))
                throw new ArgumentException("Player actions must be non-null and have unique IDs.", nameof(playerActions));
        }

        HashSet<string> combatActionIdsSeen = new HashSet<string>(StringComparer.Ordinal);
        foreach (string actionId in CombatActionIds)
        {
            if (string.IsNullOrWhiteSpace(actionId) || !combatActionIdsSeen.Add(actionId))
                throw new ArgumentException("Combat action references must be non-empty and unique.", nameof(combatActionIds));
        }
    }
}
