using System;

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
    public CharacterStats Stats { get; }

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
        Stats = character.Stats;
    }
}
