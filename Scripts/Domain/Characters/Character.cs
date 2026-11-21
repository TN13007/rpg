/// <summary>
/// Owns one character's mutable runtime state and exposes character-level rules.
/// </summary>
/// <remarks>
/// This is a Domain object. It reads its initial values from <see cref="CharacterData"/>
/// but does not load files, access Godot UI or decide how the state is presented.
/// </remarks>
public sealed class Character
{
    private readonly CharacterData _data;

    /// <summary>Gets the character's stable content identifier.</summary>
    public string Id => _data.Id;

    /// <summary>Gets the character's display name.</summary>
    public string Name => _data.Name;

    /// <summary>Gets the character's authored role.</summary>
    public string Role => _data.Role;

    /// <summary>Gets the character's current level.</summary>
    public int Level => _data.Level;

    /// <summary>Gets the character's current Health.</summary>
    /// <value>A value between zero and <see cref="MaxHealth"/>.</value>
    public int Health { get; private set; }

    /// <summary>Gets the maximum Health defined for this character.</summary>
    public int MaxHealth => _data.MaxHealth;

    /// <summary>Gets the character's current Fatigue.</summary>
    /// <value>A value between zero and <see cref="MaxFatigue"/>.</value>
    public double Fatigue { get; private set; }

    /// <summary>Gets the maximum Fatigue defined for this character.</summary>
    public double MaxFatigue => _data.MaxFatigue;

    /// <summary>Gets the character's current Stamina.</summary>
    /// <value>A value between zero and <see cref="MaxStamina"/>.</value>
    public int Stamina { get; private set; }

    /// <summary>Gets the maximum Stamina defined for this character.</summary>
    public int MaxStamina => _data.MaxStamina;

    /// <summary>Gets the character's current Mood score.</summary>
    /// <value>A value between zero and <see cref="MaxMood"/>.</value>
    public int Mood { get; private set; }

    /// <summary>Gets the Mood band calculated from the current Mood score.</summary>
    public MoodBand MoodBand => MoodBandCalculator.Calculate(Mood, MaxMood);

    /// <summary>Gets the maximum Mood score defined for this character.</summary>
    public int MaxMood => _data.MaxMood;

    /// <summary>Gets the character's current Gold.</summary>
    public int Gold { get; private set; }

    /// <summary>Gets the character's authored base statistics.</summary>
    public CharacterStats Stats => _data.Stats;

    /// <summary>Gets whether the character currently has more than zero Health.</summary>
    public bool IsAlive => Health > 0;

    /// <summary>Gets whether the character has zero Health.</summary>
    public bool IsDead => !IsAlive;

    /// <summary>Gets whether the character is currently allowed to act.</summary>
    /// <remarks>In the current character model, a character can act exactly when alive.</remarks>
    public bool CanAct => IsAlive;

    /// <summary>
    /// Creates a runtime character from its authored data and initializes its starting state.
    /// </summary>
    /// <param name="data">The character definition containing identity, limits and starting values.</param>
    /// <exception cref="System.ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
    public Character(CharacterData data)
    {
        _data = data ?? throw new System.ArgumentNullException(nameof(data));
        Reset();
    }

    /// <summary>Restores Health, Fatigue, Stamina, Mood and Gold to their authored starting values.</summary>
    public void Reset()
    {
        Health = _data.StartingHealth;
        Fatigue = _data.StartingFatigue;
        Stamina = _data.StartingStamina;
        Mood = _data.StartingMood;
        Gold = _data.StartingGold;
    }

    /// <summary>
    /// Applies damage without allowing Health to fall below zero.
    /// </summary>
    /// <param name="amount">The requested damage. Negative values are treated as zero.</param>
    /// <returns>The amount of damage actually applied.</returns>
    public int TakeDamage(int amount)
    {
        int damage = System.Math.Max(0, amount);
        int appliedDamage = System.Math.Min(Health, damage);
        Health -= appliedDamage;
        return appliedDamage;
    }

    /// <summary>
    /// Restores Health without exceeding the character's maximum.
    /// </summary>
    /// <param name="amount">The requested Health restoration. Negative values are treated as zero.</param>
    /// <returns>The amount of Health actually restored; zero when the character is dead or already at maximum Health.</returns>
    /// <remarks>Use <see cref="Revive"/> to restore a character whose Health is zero.</remarks>
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
    /// Revives a character whose Health is zero.
    /// </summary>
    /// <param name="healthAmount">The requested Health after revival. Must be greater than zero.</param>
    /// <returns>The character's resulting Health; zero when the character was already alive or the requested amount is not positive.</returns>
    public int Revive(int healthAmount)
    {
        if (IsAlive || healthAmount <= 0)
        {
            return 0;
        }

        Health = System.Math.Min(MaxHealth, healthAmount);
        return Health;
    }

    /// <summary>
    /// Increases Fatigue without exceeding the character's maximum.
    /// </summary>
    /// <param name="amount">The requested Fatigue increase. Non-positive, NaN and infinite values are ignored.</param>
    /// <returns>The amount of Fatigue actually added.</returns>
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

    /// <summary>
    /// Reduces Fatigue without going below zero.
    /// </summary>
    /// <param name="amount">The requested Fatigue reduction. Non-positive, NaN and infinite values are ignored.</param>
    /// <returns>The amount of Fatigue actually removed.</returns>
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

    /// <summary>
    /// Spends Stamina when the character has enough available.
    /// </summary>
    /// <param name="amount">The requested Stamina cost. Negative values are rejected.</param>
    /// <returns><see langword="true"/> when the cost is paid; otherwise, <see langword="false"/> and Stamina remains unchanged.</returns>
    public bool SpendStamina(int amount)
    {
        if (amount < 0 || Stamina < amount)
        {
            return false;
        }

        Stamina -= amount;
        return true;
    }

    /// <summary>
    /// Restores Stamina without exceeding the character's maximum.
    /// </summary>
    /// <param name="amount">The requested Stamina restoration. Negative values are treated as zero.</param>
    /// <returns>The amount of Stamina actually restored.</returns>
    public int RestoreStamina(int amount)
    {
        int restored = System.Math.Min(MaxStamina - Stamina, System.Math.Max(0, amount));
        Stamina += restored;
        return restored;
    }

    /// <summary>
    /// Changes Mood while keeping it within the range from zero to its maximum.
    /// </summary>
    /// <param name="amount">The signed Mood change. Positive values increase Mood; negative values decrease it.</param>
    public void ChangeMood(int amount)
    {
        Mood = (int)System.Math.Clamp((long)Mood + amount, 0L, MaxMood);
    }

    /// <summary>
    /// Adds Gold without allowing the balance to become negative or overflow <see cref="int"/>.
    /// </summary>
    /// <param name="amount">The requested Gold gain. Negative values are treated as zero.</param>
    public void AddGold(int amount)
    {
        Gold = (int)System.Math.Min(int.MaxValue, (long)Gold + System.Math.Max(0, amount));
    }
}
