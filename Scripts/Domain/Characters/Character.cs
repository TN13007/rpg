/// <summary>
/// Represents a character's authoritative gameplay state and resource values.
/// </summary>
/// <remarks>
/// This domain object owns health, fatigue, stamina, mood and gold mutations. It does not
/// create UI, play animations or emit presentation effects; callers coordinate those concerns
/// in the appropriate application or presentation layer.
/// </remarks>
public sealed class Character
{
    private readonly CharacterData _data;

    /// <summary>Gets the character's stable content identifier.</summary>
    public string Id => _data.Id;

    /// <summary>Gets the character's display name.</summary>
    public string Name => _data.Name;

    /// <summary>Gets the character's gameplay role.</summary>
    public string Role => _data.Role;

    /// <summary>Gets the character's current level from its data definition.</summary>
    public int Level => _data.Level;

    /// <summary>Gets the character's current health.</summary>
    public int Health { get; private set; }

    /// <summary>Gets the maximum health defined for this character.</summary>
    public int MaxHealth => _data.MaxHealth;

    /// <summary>Gets the character's current fatigue.</summary>
    public double Fatigue { get; private set; }

    /// <summary>Gets the maximum fatigue defined for this character.</summary>
    public double MaxFatigue => _data.MaxFatigue;

    /// <summary>Gets the character's current stamina.</summary>
    public int Stamina { get; private set; }

    /// <summary>Gets the maximum stamina defined for this character.</summary>
    public int MaxStamina => _data.MaxStamina;

    /// <summary>Gets the character's current mood value.</summary>
    public int Mood { get; private set; }

    /// <summary>Gets the mood band calculated from the current mood and its maximum.</summary>
    public MoodBand MoodBand => MoodBandCalculator.Calculate(Mood, MaxMood);

    /// <summary>Gets the maximum mood value defined for this character.</summary>
    public int MaxMood => _data.MaxMood;

    /// <summary>Gets the character's current gold.</summary>
    public int Gold { get; private set; }

    /// <summary>Gets the character's base statistics.</summary>
    public CharacterStats Stats => _data.Stats;

    /// <summary>Gets whether the character has more than zero health.</summary>
    public bool IsAlive => Health > 0;

    /// <summary>Gets whether the character has zero health.</summary>
    public bool IsDead => !IsAlive;

    /// <summary>Gets whether the character is currently able to act.</summary>
    /// <remarks>Currently equivalent to <see cref="IsAlive"/>; additional action restrictions belong to the systems that own them.</remarks>
    public bool CanAct => IsAlive;

    /// <summary>
    /// Creates a character from its immutable data definition and initializes its runtime resources.
    /// </summary>
    /// <param name="data">The character's starting and maximum values.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    public Character(CharacterData data)
    {
        _data = data ?? throw new System.ArgumentNullException(nameof(data));
        Reset();
    }

    /// <summary>Restores all runtime resources to the starting values in the character definition.</summary>
    public void Reset()
    {
        Health = _data.StartingHealth;
        Fatigue = _data.StartingFatigue;
        Stamina = _data.StartingStamina;
        Mood = _data.StartingMood;
        Gold = _data.StartingGold;
    }

    /// <summary>
    /// Removes health from the character, clamping requested damage to a non-negative value.
    /// </summary>
    /// <param name="amount">The requested damage.</param>
    /// <returns>The amount of health actually removed, which cannot exceed current health.</returns>
    public int TakeDamage(int amount)
    {
        int damage = System.Math.Max(0, amount);
        int appliedDamage = System.Math.Min(Health, damage);
        Health -= appliedDamage;
        return appliedDamage;
    }

    /// <summary>
    /// Restores health without allowing the value to exceed maximum health.
    /// </summary>
    /// <param name="amount">The requested amount of health to restore.</param>
    /// <returns>The amount of health actually restored; returns zero if the character is dead.</returns>
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

    /// <summary>
    /// Revives a dead character with the requested positive health amount.
    /// </summary>
    /// <param name="healthAmount">The health to assign, capped at maximum health.</param>
    /// <returns>The resulting health after revival, or zero if revival was not performed.</returns>
    public int Revive(int healthAmount)
    {
        if (IsAlive || healthAmount <= 0)
        {
            return 0;
        }

        Health = System.Math.Min(MaxHealth, healthAmount);
        return Health;
    }

    /// <summary>Adds fatigue while keeping the value within its valid range.</summary>
    /// <param name="amount">The positive fatigue amount to add. Non-finite and non-positive values are ignored.</param>
    /// <returns>The amount of fatigue actually added.</returns>
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

    /// <summary>Reduces fatigue while keeping the value within its valid range.</summary>
    /// <param name="amount">The positive fatigue amount to restore. Non-finite and non-positive values are ignored.</param>
    /// <returns>The amount of fatigue actually removed.</returns>
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

    /// <summary>Spends stamina if the character has enough available.</summary>
    /// <param name="amount">The non-negative stamina cost.</param>
    /// <returns><see langword="true"/> if the cost was paid; otherwise, <see langword="false"/>.</returns>
    /// <remarks>A negative cost is rejected and does not change stamina.</remarks>
    public bool SpendStamina(int amount)
    {
        if (amount < 0 || Stamina < amount)
        {
            return false;
        }

        Stamina -= amount;
        return true;
    }

    /// <summary>Restores stamina without allowing it to exceed maximum stamina.</summary>
    /// <param name="amount">The requested amount of stamina to restore.</param>
    /// <returns>The amount of stamina actually restored.</returns>
    public int RestoreStamina(int amount)
    {
        int restored = System.Math.Min(MaxStamina - Stamina, System.Math.Max(0, amount));
        Stamina += restored;
        return restored;
    }

    /// <summary>Changes mood and clamps the result between zero and maximum mood.</summary>
    /// <param name="amount">The signed mood adjustment; positive values increase mood and negative values decrease it.</param>
    public void ChangeMood(int amount)
    {
        Mood = (int)System.Math.Clamp((long)Mood + amount, 0L, MaxMood);
    }

    /// <summary>Adds a non-negative amount of gold, capped at <see cref="int.MaxValue"/>.</summary>
    /// <param name="amount">The requested gold amount. Negative values are treated as zero.</param>
    public void AddGold(int amount)
    {
        Gold = (int)System.Math.Min(int.MaxValue, (long)Gold + System.Math.Max(0, amount));
    }
}
