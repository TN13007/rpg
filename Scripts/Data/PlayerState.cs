public sealed class PlayerState
{
    public string Name { get; }
    public string Role { get; }
    public int Level { get; }
    public int Health { get; private set; }
    public int MaxHealth { get; }
    public int Energy { get; private set; }
    public int MaxEnergy { get; }
    public int Mood { get; private set; }
    public int Gold { get; private set; }

    public PlayerState(string name, string role, int level, int health, int maxHealth, int energy, int maxEnergy, int mood, int gold)
    {
        Name = name;
        Role = role;
        Level = level;
        Health = health;
        MaxHealth = maxHealth;
        Energy = energy;
        MaxEnergy = maxEnergy;
        Mood = mood;
        Gold = gold;
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

    public void TakeDamage(int amount)
    {
        Health = System.Math.Max(0, Health - System.Math.Max(0, amount));
    }

    public void AddGold(int amount)
    {
        Gold += System.Math.Max(0, amount);
    }
}
