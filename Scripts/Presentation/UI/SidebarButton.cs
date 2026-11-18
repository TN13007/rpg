using System;
using Godot;

public partial class SidebarButton : Button
{
    public event Action<string> Selected;
    public string TabId { get; set; } = string.Empty;

    public override void _Ready()
    {
        Pressed += EmitSelected;
    }

    private void EmitSelected()
    {
        Selected?.Invoke(TabId);
    }
}
