public sealed class CombatantEffectState
{
    public bool IsGuarding { get; private set; }
    public bool IsEvading { get; private set; }

    public void Reset()
    {
        IsGuarding = false;
        IsEvading = false;
    }

    public void Guard()
    {
        IsGuarding = true;
        IsEvading = false;
    }

    public void Evade()
    {
        IsEvading = true;
        IsGuarding = false;
    }

    public bool ConsumeEvade()
    {
        bool result = IsEvading;
        IsEvading = false;
        return result;
    }

    public int ApplyIncomingDamage(int damage)
    {
        if (ConsumeEvade()) return 0;
        if (IsGuarding)
        {
            IsGuarding = false;
            return damage <= 0 ? 0 : System.Math.Max(1, damage / 4);
        }
        return damage;
    }
}
