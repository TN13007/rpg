using System;
using Godot;

public partial class CombatActionButton : Button
{
    public event Action<CombatActionRequest> ActionSelected;

    public CombatActionRequest Request { get; set; }

    public override void _Ready()
    {
        Pressed += EmitActionSelected;
    }

    private void EmitActionSelected()
    {
        if (Request != null)
        {
            ActionSelected?.Invoke(Request);
        }
    }
}
