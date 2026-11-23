using System;
using Godot;

/// <summary>Presents a target choice for an action such as Revive.</summary>
public partial class CombatActionButton : Button
{
    /// <summary>Raised with a Presentation-owned selection intent when the target is chosen.</summary>
    public event Action<CombatActionSelection> ActionSelected;

    /// <summary>Gets or sets the selection intent emitted by this button.</summary>
    public CombatActionSelection Selection { get; set; }

    public override void _Ready()
    {
        Pressed += EmitActionSelected;
    }

    private void EmitActionSelected()
    {
        if (Selection != null)
        {
            ActionSelected?.Invoke(Selection);
        }
    }
}
