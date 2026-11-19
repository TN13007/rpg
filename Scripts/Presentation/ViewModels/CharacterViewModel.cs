using System;

public sealed class CharacterViewModel
{
    public string Name { get; }
    public string Role { get; }
    public int Level { get; }
    public int Health { get; }
    public int MaxHealth { get; }
    public double Fatigue { get; }
    public double MaxFatigue { get; }
    public int Stamina { get; }
    public int MaxStamina { get; }
    public int Mood { get; }
    public MoodBand MoodBand { get; }
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
        Fatigue = character.Fatigue;
        MaxFatigue = character.MaxFatigue;
        Stamina = character.Stamina;
        MaxStamina = character.MaxStamina;
        Mood = character.Mood;
        MoodBand = character.MoodBand;
        MaxMood = character.MaxMood;
        Gold = character.Gold;
        Stats = character.Stats;
    }
}
