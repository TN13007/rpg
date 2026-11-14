using System;
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

public sealed class CharacterData
{
    public string Id { get; }
    public string Name { get; }
    public string Role { get; }
    public int Level { get; }
    public int MaxHealth { get; }
    public int MaxEnergy { get; }
    public int MaxMood { get; }
    public int StartingHealth { get; }
    public int StartingEnergy { get; }
    public int StartingMood { get; }
    public int StartingGold { get; }
    public CharacterStats Stats { get; }

    public CharacterData(
        string id,
        string name,
        string role,
        int level,
        int maxHealth,
        int maxEnergy,
        int maxMood,
        int startingHealth,
        int startingEnergy,
        int startingMood,
        int startingGold,
        CharacterStats stats = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Character ID is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Character name is required.", nameof(name));
        }

        if (level < 1 || maxHealth < 1 || maxEnergy < 0 || maxMood < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(level), "Character base values are invalid.");
        }

        if (startingHealth < 0 || startingHealth > maxHealth)
        {
            throw new ArgumentOutOfRangeException(nameof(startingHealth));
        }

        if (startingEnergy < 0 || startingEnergy > maxEnergy)
        {
            throw new ArgumentOutOfRangeException(nameof(startingEnergy));
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
        MaxEnergy = maxEnergy;
        MaxMood = maxMood;
        StartingHealth = startingHealth;
        StartingEnergy = startingEnergy;
        StartingMood = startingMood;
        StartingGold = startingGold;
        Stats = stats ?? new CharacterStats(0, 0, 0, 0, 0);
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
                data.GetProperty("maxEnergy").GetInt32(),
                data.GetProperty("maxMood").GetInt32(),
                data.GetProperty("startingHealth").GetInt32(),
                data.GetProperty("startingEnergy").GetInt32(),
                data.GetProperty("startingMood").GetInt32(),
                data.GetProperty("startingGold").GetInt32(),
                ReadStats(data));
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Invalid character data JSON: " + path, exception);
        }
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
