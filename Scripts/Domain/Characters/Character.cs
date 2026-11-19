public sealed class Character
{
    private readonly CharacterData _data;

    public string Id => _data.Id;
    public string Name => _data.Name;
    public string Role => _data.Role;
    public int Level => _data.Level;
    public int Health { get; private set; }
    public int MaxHealth => _data.MaxHealth;
    public double Fatigue { get; private set; }
    public double MaxFatigue => _data.MaxFatigue;
    public int Stamina { get; private set; }
    public int MaxStamina => _data.MaxStamina;
    public int Mood { get; private set; }
    public MoodBand MoodBand => MoodBandCalculator.Calculate(Mood, MaxMood);
    public int MaxMood => _data.MaxMood;
    public int Gold { get; private set; }
    public CharacterStats Stats => _data.Stats;
    public bool IsAlive => Health > 0;
    public bool IsDead => !IsAlive;
    public bool CanAct => IsAlive;

    public Character(CharacterData data)
    {
        _data = data ?? throw new System.ArgumentNullException(nameof(data));
        Reset();
    }

    public void Reset()
    {
        Health = _data.StartingHealth;
        Fatigue = _data.StartingFatigue;
        Stamina = _data.StartingStamina;
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

    public int RestoreHealth(int amount)
    {
        if (IsDead)
        {
            return 0;
        }

        int restored = System.Math.Min(MaxHealth - Health, System.Math.Max(0, amount));
        Health += restored;
        return restored;
    }

    public double AddFatigue(double amount)
    {
        if (double.IsNaN(amount) || double.IsInfinity(amount) || amount <= 0)
        {
            return 0;
        }

        double previous = Fatigue;
        Fatigue = System.Math.Clamp(Fatigue + amount, 0.0, MaxFatigue);
        return Fatigue - previous;
    }

    public double RestoreFatigue(double amount)
    {
        if (double.IsNaN(amount) || double.IsInfinity(amount) || amount <= 0)
        {
            return 0;
        }

        double previous = Fatigue;
        Fatigue = System.Math.Clamp(Fatigue - amount, 0.0, MaxFatigue);
        return previous - Fatigue;
    }

    public bool SpendStamina(int amount)
    {
        if (amount < 0 || Stamina < amount)
        {
            return false;
        }

        Stamina -= amount;
        return true;
    }

    public int RestoreStamina(int amount)
    {
        int restored = System.Math.Min(MaxStamina - Stamina, System.Math.Max(0, amount));
        Stamina += restored;
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
