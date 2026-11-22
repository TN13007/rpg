/// <summary>Tracks the mutable combatant effect state used during combat.</summary>
public sealed class CombatantEffectState
{
    /// <summary>Gets a value indicating whether guarding.</summary>
    public bool IsGuarding { get; private set; }
    /// <summary>Gets a value indicating whether evading.</summary>
    public bool IsEvading { get; private set; }

    /// <summary>Clears temporary combat effects from the combatant.</summary>
    public void Reset()
    {
        IsGuarding = false;
        IsEvading = false;
    }

    /// <summary>Enables guarding against the next incoming attack.</summary>
    public void Guard()
    {
        IsGuarding = true;
        IsEvading = false;
    }

    /// <summary>Prepares the combatant to evade the next incoming attack.</summary>
    public void Evade()
    {
        IsEvading = true;
        IsGuarding = false;
    }

    /// <summary>Consumes the pending evade state and reports whether it prevented damage.</summary>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
    public bool ConsumeEvade()
    {
        bool result = IsEvading;
        IsEvading = false;
        return result;
    }

    /// <summary>Applies incoming damage to the target combatant.</summary>
    /// <param name="damage">The damage used by this operation.</param>
    /// <returns>The resulting int.</returns>
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
