using System;

/// <summary>Immutable snapshot of the five Character-owned runtime resources used by Action calculations.</summary>
public sealed class CharacterRuntimeState : IEquatable<CharacterRuntimeState>
{
    /// <summary>Gets the current Health in this snapshot.</summary>
    public int Health { get; }
    /// <summary>Gets the current Fatigue in this snapshot.</summary>
    public double Fatigue { get; }
    /// <summary>Gets the current Stamina in this snapshot.</summary>
    public int Stamina { get; }
    /// <summary>Gets the current Mood in this snapshot.</summary>
    public int Mood { get; }
    /// <summary>Gets the current Gold in this snapshot.</summary>
    public int Gold { get; }
    /// <summary>Gets the Health limit used by calculations.</summary>
    public int MaxHealth { get; }
    /// <summary>Gets the Fatigue limit used by calculations.</summary>
    public double MaxFatigue { get; }
    /// <summary>Gets the Stamina limit used by calculations.</summary>
    public int MaxStamina { get; }
    /// <summary>Gets the Mood limit used by calculations.</summary>
    public int MaxMood { get; }

    internal CharacterRuntimeState(
        int health, double fatigue, int stamina, int mood, int gold,
        int maxHealth, double maxFatigue, int maxStamina, int maxMood)
    {
        if (health < 0 || health > maxHealth) throw new ArgumentOutOfRangeException(nameof(health));
        if (double.IsNaN(fatigue) || double.IsInfinity(fatigue) || fatigue < 0 || fatigue > maxFatigue)
            throw new ArgumentOutOfRangeException(nameof(fatigue));
        if (stamina < 0 || stamina > maxStamina) throw new ArgumentOutOfRangeException(nameof(stamina));
        if (mood < 0 || mood > maxMood) throw new ArgumentOutOfRangeException(nameof(mood));
        if (gold < 0) throw new ArgumentOutOfRangeException(nameof(gold));
        Health = health; Fatigue = fatigue; Stamina = stamina; Mood = mood; Gold = gold;
        MaxHealth = maxHealth; MaxFatigue = maxFatigue; MaxStamina = maxStamina; MaxMood = maxMood;
    }

    /// <summary>Returns a detached snapshot of the Character's current resources.</summary>
    public static CharacterRuntimeState Capture(Character character)
    {
        if (character == null) throw new ArgumentNullException(nameof(character));
        return character.CaptureRuntimeState();
    }

    /// <summary>Returns a new state with the requested Fatigue increase applied.</summary>
    public CharacterRuntimeState AddFatigue(double amount)
    {
        if (double.IsNaN(amount) || double.IsInfinity(amount) || amount <= 0) return this;
        return Copy(fatigue: Math.Clamp(Fatigue + amount, 0.0, MaxFatigue));
    }

    /// <summary>Returns a new state with the authored effect applied, preserving Character resource rules.</summary>
    public CharacterRuntimeState Apply(ActionEffectDefinition effect)
    {
        if (effect == null) throw new ArgumentNullException(nameof(effect));
        switch (effect.Type)
        {
            case ActionEffectType.RestoreHealth:
                return Health == 0 ? this : Copy(health: Health + Math.Min(MaxHealth - Health, Math.Max(0, effect.Amount)));
            case ActionEffectType.RestoreFatigue:
                if (effect.Amount <= 0) return this;
                return Copy(fatigue: Math.Clamp(Fatigue - effect.Amount, 0.0, MaxFatigue));
            case ActionEffectType.RestoreStamina:
                return Copy(stamina: Stamina + Math.Min(MaxStamina - Stamina, Math.Max(0, effect.Amount)));
            case ActionEffectType.ChangeMood:
                return Copy(mood: (int)Math.Clamp((long)Mood + effect.Amount, 0L, MaxMood));
            case ActionEffectType.AddGold:
                return Copy(gold: (int)Math.Min(int.MaxValue, (long)Gold + Math.Max(0, effect.Amount)));
            case ActionEffectType.Revive:
                return Health > 0 || effect.Amount <= 0 ? this : Copy(health: Math.Min(MaxHealth, effect.Amount));
            default:
                throw new ArgumentOutOfRangeException(nameof(effect.Type));
        }
    }

    private CharacterRuntimeState Copy(
        int? health = null, double? fatigue = null, int? stamina = null, int? mood = null, int? gold = null)
    {
        return new CharacterRuntimeState(
            health ?? Health, fatigue ?? Fatigue, stamina ?? Stamina, mood ?? Mood, gold ?? Gold,
            MaxHealth, MaxFatigue, MaxStamina, MaxMood);
    }

    /// <summary>Determines whether another snapshot contains the same current resource values.</summary>
    public bool Equals(CharacterRuntimeState other)
    {
        return other != null && Health == other.Health && Fatigue.Equals(other.Fatigue)
            && Stamina == other.Stamina && Mood == other.Mood && Gold == other.Gold;
    }

    /// <summary>Determines whether another object is an equal resource snapshot.</summary>
    public override bool Equals(object obj) => Equals(obj as CharacterRuntimeState);
    /// <summary>Returns a hash code for the current resource values.</summary>
    public override int GetHashCode() => HashCode.Combine(Health, Fatigue, Stamina, Mood, Gold);
}
