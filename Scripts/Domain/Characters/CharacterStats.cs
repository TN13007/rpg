using System;

/// <summary>Identifies a character attribute used by gameplay calculations.</summary>
public enum CharacterStat
{
    /// <summary>Represents physical strength.</summary>
    Strength,
    /// <summary>Represents agility and contributes to combat speed.</summary>
    Agility,
    /// <summary>Represents intelligence.</summary>
    Intelligence,
    /// <summary>Represents endurance.</summary>
    Endurance,
    /// <summary>Represents wisdom.</summary>
    Wisdom,
    /// <summary>Represents luck and breaks combat timeline ties.</summary>
    Luck
}

/// <summary>Provides character stats functionality for gameplay.</summary>
public sealed class CharacterStats
{
    /// <summary>Gets the strength.</summary>
    public int Strength { get; }
    /// <summary>Gets the agility.</summary>
    public int Agility { get; }
    /// <summary>Gets the intelligence.</summary>
    public int Intelligence { get; }
    /// <summary>Gets the endurance.</summary>
    public int Endurance { get; }
    /// <summary>Gets the wisdom.</summary>
    public int Wisdom { get; }
    /// <summary>Gets the luck.</summary>
    public int Luck { get; }

    /// <summary>Initializes a new instance of <see cref="CharacterStats"/>.</summary>
    /// <param name="strength">The strength used to initialize the instance.</param>
    /// <param name="agility">The agility used to initialize the instance.</param>
    /// <param name="intelligence">The intelligence used to initialize the instance.</param>
    /// <param name="endurance">The endurance used to initialize the instance.</param>
    /// <param name="wisdom">The wisdom used to initialize the instance.</param>
    /// <param name="luck">The luck used to initialize the instance.</param>
    public CharacterStats(int strength, int agility, int intelligence, int endurance, int wisdom, int luck = 0)
    {
        ValidateValue(strength, nameof(strength));
        ValidateValue(agility, nameof(agility));
        ValidateValue(intelligence, nameof(intelligence));
        ValidateValue(endurance, nameof(endurance));
        ValidateValue(wisdom, nameof(wisdom));
        ValidateValue(luck, nameof(luck));

        Strength = strength;
        Agility = agility;
        Intelligence = intelligence;
        Endurance = endurance;
        Wisdom = wisdom;
        Luck = luck;
    }

    /// <summary>Returns value.</summary>
    /// <param name="stat">The stat used by this operation.</param>
    /// <returns>The resulting int.</returns>
    public int GetValue(CharacterStat stat)
    {
        return stat switch
        {
            CharacterStat.Strength => Strength,
            CharacterStat.Agility => Agility,
            CharacterStat.Intelligence => Intelligence,
            CharacterStat.Endurance => Endurance,
            CharacterStat.Wisdom => Wisdom,
            CharacterStat.Luck => Luck,
            _ => throw new ArgumentOutOfRangeException(nameof(stat))
        };
    }

    private static void ValidateValue(int value, string name)
    {
        if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException(name, "Character stats must be between 0 and 100.");
    }
}
