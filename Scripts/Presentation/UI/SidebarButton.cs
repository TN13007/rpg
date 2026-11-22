using System;
using Godot;

/// <summary>Represents a selectable navigation or story choice button.</summary>
public partial class SidebarButton : Button
{
    /// <summary>Raised when selected occurs.</summary>
    public event Action<string> Selected;
    /// <summary>Gets the tab id.</summary>
    public string TabId { get; set; } = string.Empty;

    /// <summary>Initializes the Godot control and connects its UI signals.</summary>
    public override void _Ready()
    {
        Pressed += EmitSelected;
    }

    private void EmitSelected()
    {
        Selected?.Invoke(TabId);
    }
}
