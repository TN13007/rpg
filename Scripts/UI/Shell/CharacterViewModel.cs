using System;

/// <summary>Exposes the current character's gameplay values as read-only presentation data.</summary>
public sealed class CharacterViewModel
{
    /// <summary>Gets the name.</summary>
    public string Name { get; }
    /// <summary>Gets the role.</summary>
    public string Role { get; }
    /// <summary>Gets the level.</summary>
    public int Level { get; }
    /// <summary>Gets the health.</summary>
    public int Health { get; }
    /// <summary>Gets the max health.</summary>
    public int MaxHealth { get; }
    /// <summary>Gets the fatigue.</summary>
    public double Fatigue { get; }
    /// <summary>Gets the max fatigue.</summary>
    public double MaxFatigue { get; }
    /// <summary>Gets the stamina.</summary>
    public int Stamina { get; }
    /// <summary>Gets the max stamina.</summary>
    public int MaxStamina { get; }
    /// <summary>Gets the mood.</summary>
    public int Mood { get; }
    /// <summary>Gets the mood band.</summary>
    public MoodBand MoodBand { get; }
    /// <summary>Gets the max mood.</summary>
    public int MaxMood { get; }
    /// <summary>Gets the gold.</summary>
    public int Gold { get; }
    /// <summary>Gets the stats.</summary>
    public CharacterStats Stats { get; }

    /// <summary>Initializes a new instance of <see cref="CharacterViewModel"/>.</summary>
    /// <param name="character">The character used to initialize the instance.</param>
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
