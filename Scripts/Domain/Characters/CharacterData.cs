using System;
using System.Collections.Generic;

/// <summary>Provides immutable authored character data for gameplay.</summary>
/// <remarks>Collection inputs are copied and exposed through read-only views.</remarks>
public sealed class CharacterData
{
    /// <summary>Gets the id.</summary>
    public string Id { get; }
    /// <summary>Gets the name.</summary>
    public string Name { get; }
    /// <summary>Gets the role.</summary>
    public string Role { get; }
    /// <summary>Gets the level.</summary>
    public int Level { get; }
    /// <summary>Gets the max health.</summary>
    public int MaxHealth { get; }
    /// <summary>Gets the max fatigue.</summary>
    public double MaxFatigue { get; }
    /// <summary>Gets the max stamina.</summary>
    public int MaxStamina { get; }
    /// <summary>Gets the max mood.</summary>
    public int MaxMood { get; }
    /// <summary>Gets the starting health.</summary>
    public int StartingHealth { get; }
    /// <summary>Gets the starting fatigue.</summary>
    public double StartingFatigue { get; }
    /// <summary>Gets the starting stamina.</summary>
    public int StartingStamina { get; }
    /// <summary>Gets the starting mood.</summary>
    public int StartingMood { get; }
    /// <summary>Gets the starting gold.</summary>
    public int StartingGold { get; }
    /// <summary>Gets the stats.</summary>
    public CharacterStats Stats { get; }
    /// <summary>Gets the player actions.</summary>
    public IReadOnlyList<PlayerActionDefinition> PlayerActions { get; }
    /// <summary>Gets the combat action ids.</summary>
    public IReadOnlyList<string> CombatActionIds { get; }

    /// <summary>Initializes a new instance of <see cref="CharacterData"/>.</summary>
    /// <param name="id">The id used to initialize the instance.</param>
    /// <param name="name">The name used to initialize the instance.</param>
    /// <param name="role">The role used to initialize the instance.</param>
    /// <param name="level">The level used to initialize the instance.</param>
    /// <param name="maxHealth">The max health used to initialize the instance.</param>
    /// <param name="maxFatigue">The max fatigue used to initialize the instance.</param>
    /// <param name="maxStamina">The max stamina used to initialize the instance.</param>
    /// <param name="maxMood">The max mood used to initialize the instance.</param>
    /// <param name="startingHealth">The starting health used to initialize the instance.</param>
    /// <param name="startingFatigue">The starting fatigue used to initialize the instance.</param>
    /// <param name="startingStamina">The starting stamina used to initialize the instance.</param>
    /// <param name="startingMood">The starting mood used to initialize the instance.</param>
    /// <param name="startingGold">The starting gold used to initialize the instance.</param>
    /// <param name="stats">The stats used to initialize the instance.</param>
    /// <param name="playerActions">The player actions used to initialize the instance.</param>
    /// <param name="combatActionIds">The combat action ids used to initialize the instance.</param>
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
        PlayerActions = Array.AsReadOnly(playerActions == null ? Array.Empty<PlayerActionDefinition>() : (PlayerActionDefinition[])playerActions.Clone());
        CombatActionIds = Array.AsReadOnly(combatActionIds == null ? Array.Empty<string>() : (string[])combatActionIds.Clone());

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
