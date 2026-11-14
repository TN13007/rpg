using System;

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
        int startingGold)
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

        if (startingMood < 0 || startingMood > maxMood || startingGold < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startingMood));
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
    }
}

public sealed class CharacterViewModel
{
    public string Name { get; }
    public string Role { get; }
    public int Level { get; }
    public int Health { get; }
    public int MaxHealth { get; }
    public int Energy { get; }
    public int MaxEnergy { get; }
    public int Mood { get; }
    public int MaxMood { get; }
    public int Gold { get; }

    public CharacterViewModel(Character character)
    {
        if (character == null)
        {
            throw new ArgumentNullException(nameof(character));
        }

        Name = character.Name;
        Role = character.Role;
        Level = character.Level;
        Health = character.Health;
        MaxHealth = character.MaxHealth;
        Energy = character.Energy;
        MaxEnergy = character.MaxEnergy;
        Mood = character.Mood;
        MaxMood = character.MaxMood;
        Gold = character.Gold;
    }
}
