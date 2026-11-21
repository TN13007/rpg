using System;
using Godot;

/// <summary>Presents a selectable combat action action in the combat interface.</summary>
public partial class CombatActionButton : Button
{
    /// <summary>Raised when action selected occurs.</summary>
    public event Action<CombatActionRequest> ActionSelected;

    /// <summary>Gets the request.</summary>
    public CombatActionRequest Request { get; set; }

    /// <summary>Initializes the Godot control and connects its UI signals.</summary>
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
