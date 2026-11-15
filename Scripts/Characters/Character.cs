public sealed class Character
{
    private readonly CharacterData _data;

    public string Id => _data.Id;
    public string Name => _data.Name;
    public string Role => _data.Role;
    public int Level => _data.Level;
    public int Health { get; private set; }
    public int MaxHealth => _data.MaxHealth;
    public int Energy { get; private set; }
    public int MaxEnergy => _data.MaxEnergy;
    public int Mood { get; private set; }
    public int MaxMood => _data.MaxMood;
    public int Gold { get; private set; }
    public CharacterStats Stats => _data.Stats;
    public bool IsAlive => Health > 0;
    public bool IsDead => !IsAlive;
    public bool IsUnconscious => IsAlive && MaxEnergy > 0 && Energy == 0;
    public bool CanAct => IsAlive && !IsUnconscious;

    public Character(CharacterData data)
    {
        _data = data ?? throw new System.ArgumentNullException(nameof(data));
        Reset();
    }

    public void Reset()
    {
        Health = _data.StartingHealth;
        Energy = _data.StartingEnergy;
        Mood = _data.StartingMood;
        Gold = _data.StartingGold;
    }

    public int TakeDamage(int amount)
    {
        int damage = System.Math.Max(0, amount);
        int appliedDamage = System.Math.Min(Health, damage);
        Health -= appliedDamage;
        return appliedDamage;
    }

    public bool SpendEnergy(int amount)
    {
        if (amount < 0 || Energy < amount)
        {
            return false;
        }

        Energy -= amount;
        return true;
    }

    public int RestoreHealth(int amount)
    {
        int restored = System.Math.Min(MaxHealth - Health, System.Math.Max(0, amount));
        Health += restored;
        return restored;
    }

    public int RestoreEnergy(int amount)
    {
        int restored = System.Math.Min(MaxEnergy - Energy, System.Math.Max(0, amount));
        Energy += restored;
        return restored;
    }

    public void ChangeMood(int amount)
    {
        Mood = (int)System.Math.Clamp((long)Mood + amount, 0L, MaxMood);
    }

    public void AddGold(int amount)
    {
        Gold = (int)System.Math.Min(int.MaxValue, (long)Gold + System.Math.Max(0, amount));
    }
}
