using System;
using Godot;

public partial class CombatActionButton : Button
{
    public event Action<string> ActionSelected;

    public string ActionId { get; set; } = string.Empty;

    public override void _Ready()
    {
        Pressed += EmitActionSelected;
    }

    private void EmitActionSelected()
    {
        ActionSelected?.Invoke(ActionId);
    }
}
