using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

public enum CharacterStat
{
    Strength,
    Agility,
    Intelligence,
    Endurance,
    Wisdom
}

public sealed class CharacterStats
{
    public int Strength { get; }
    public int Agility { get; }
    public int Intelligence { get; }
    public int Endurance { get; }
    public int Wisdom { get; }

    public CharacterStats(int strength, int agility, int intelligence, int endurance, int wisdom)
    {
        ValidateValue(strength, nameof(strength));
        ValidateValue(agility, nameof(agility));
        ValidateValue(intelligence, nameof(intelligence));
        ValidateValue(endurance, nameof(endurance));
        ValidateValue(wisdom, nameof(wisdom));

        Strength = strength;
        Agility = agility;
        Intelligence = intelligence;
        Endurance = endurance;
        Wisdom = wisdom;
    }

    public int GetValue(CharacterStat stat)
    {
        return stat switch
        {
            CharacterStat.Strength => Strength,
            CharacterStat.Agility => Agility,
            CharacterStat.Intelligence => Intelligence,
            CharacterStat.Endurance => Endurance,
            CharacterStat.Wisdom => Wisdom,
            _ => throw new ArgumentOutOfRangeException(nameof(stat))
        };
    }

    private static void ValidateValue(int value, string name)
    {
        if (value < 0 || value > 100)
        {
            throw new ArgumentOutOfRangeException(name, "Character stats must be between 0 and 100.");
        }
    }
}

public sealed class PlayerActionDefinition
{
    public ActionDefinition Action { get; }
    public string[] AvailableLocationIds { get; }

    public PlayerActionDefinition(ActionDefinition action, string[] availableLocationIds = null)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
        AvailableLocationIds = availableLocationIds ?? Array.Empty<string>();
        foreach (string locationId in AvailableLocationIds)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                throw new ArgumentException("Player action location IDs cannot be empty.", nameof(availableLocationIds));
            }
        }
    }

    public bool IsAvailableAt(string locationId)
    {
        if (AvailableLocationIds.Length == 0) return true;
        foreach (string availableLocationId in AvailableLocationIds)
        {
            if (string.Equals(availableLocationId, locationId, StringComparison.Ordinal)) return true;
        }
        return false;
    }
}

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
        PlayerActionDefinition[] playerActions = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Character ID is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Character name is required.", nameof(name));
        }

        if (level < 1 || maxHealth < 1 || maxFatigue < 0 || maxStamina < 0 || maxMood < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(level), "Character base values are invalid.");
        }

        if (startingHealth < 0 || startingHealth > maxHealth)
        {
            throw new ArgumentOutOfRangeException(nameof(startingHealth));
        }

        if (double.IsNaN(startingFatigue) || double.IsInfinity(startingFatigue) ||
            startingFatigue < 0 || startingFatigue > maxFatigue)
        {
            throw new ArgumentOutOfRangeException(nameof(startingFatigue));
        }

        if (startingStamina < 0 || startingStamina > maxStamina)
        {
            throw new ArgumentOutOfRangeException(nameof(startingStamina));
        }

        if (startingMood < 0 || startingMood > maxMood)
        {
            throw new ArgumentOutOfRangeException(nameof(startingMood));
        }

        if (startingGold < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startingGold));
        }

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
        HashSet<string> actionIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (PlayerActionDefinition playerAction in PlayerActions)
        {
            if (playerAction == null || !actionIds.Add(playerAction.Action.Id))
            {
                throw new ArgumentException("Player actions must be non-null and have unique IDs.", nameof(playerActions));
            }
        }
    }
}

public static class CharacterDataLoader
{
    public static CharacterData Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Character data path is required.", nameof(path));
        }

        if (!FileAccess.FileExists(path))
        {
            throw new InvalidOperationException("Character data file was not found: " + path);
        }

        string json = FileAccess.GetFileAsString(path);

        return LoadJson(json, path);
    }

    public static CharacterData LoadJson(string json, string source = "Player.json")
    {
        if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Character data JSON is required.", nameof(json));
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement data = document.RootElement;

            return new CharacterData(
                data.GetProperty("id").GetString(),
                data.GetProperty("name").GetString(),
                data.GetProperty("role").GetString(),
                data.GetProperty("level").GetInt32(),
                data.GetProperty("maxHealth").GetInt32(),
                data.GetProperty("maxFatigue").GetDouble(),
                data.GetProperty("maxStamina").GetInt32(),
                data.GetProperty("maxMood").GetInt32(),
                data.GetProperty("startingHealth").GetInt32(),
                data.GetProperty("startingFatigue").GetDouble(),
                data.GetProperty("startingStamina").GetInt32(),
                data.GetProperty("startingMood").GetInt32(),
                data.GetProperty("startingGold").GetInt32(),
                ReadStats(data),
                ReadPlayerActions(data));
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Invalid character data JSON: " + source, exception);
        }
    }

    private static PlayerActionDefinition[] ReadPlayerActions(JsonElement data)
    {
        if (!data.TryGetProperty("playerActions", out JsonElement actionsData)) return Array.Empty<PlayerActionDefinition>();
        if (actionsData.ValueKind != JsonValueKind.Array) throw new InvalidOperationException("Player actions must be an array.");

        List<PlayerActionDefinition> actions = new List<PlayerActionDefinition>();
        HashSet<string> actionIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (JsonElement item in actionsData.EnumerateArray())
        {
            string id = item.GetProperty("id").GetString();
            string label = item.GetProperty("label").GetString();
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(label))
                throw new InvalidOperationException("Player action IDs and labels are required.");
            if (!actionIds.Add(id)) throw new InvalidOperationException("Duplicate player action ID: " + id);

            string[] availableLocationIds = Array.Empty<string>();
            if (item.TryGetProperty("availableLocationIds", out JsonElement locationsData))
            {
                if (locationsData.ValueKind != JsonValueKind.Array) throw new InvalidOperationException("Player action availableLocationIds must be an array.");
                List<string> locationIds = new List<string>();
                foreach (JsonElement locationData in locationsData.EnumerateArray())
                {
                    string locationId = locationData.GetString();
                    if (string.IsNullOrWhiteSpace(locationId)) throw new InvalidOperationException("Player action location IDs cannot be empty.");
                    locationIds.Add(locationId);
                }
                availableLocationIds = locationIds.ToArray();
            }

            actions.Add(new PlayerActionDefinition(
                AcademyContent.ReadActionDefinition(id, label, item),
                availableLocationIds));
        }
        return actions.ToArray();
    }

    private static CharacterStats ReadStats(JsonElement data)
    {
        if (!data.TryGetProperty("stats", out JsonElement stats))
        {
            return new CharacterStats(0, 0, 0, 0, 0);
        }

        return new CharacterStats(
            stats.GetProperty("strength").GetInt32(),
            stats.GetProperty("agility").GetInt32(),
            stats.GetProperty("intelligence").GetInt32(),
            stats.GetProperty("endurance").GetInt32(),
            stats.GetProperty("wisdom").GetInt32());
    }
}
