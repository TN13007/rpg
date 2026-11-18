using System;

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
