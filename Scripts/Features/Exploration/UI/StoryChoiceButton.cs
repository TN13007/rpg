using System;
using Godot;

/// <summary>Represents a selectable navigation or story choice button.</summary>
public partial class StoryChoiceButton : Button
{
    /// <summary>Raised when choice selected occurs.</summary>
    public event Action<string> ChoiceSelected;
    /// <summary>Gets the choice id.</summary>
    public string ChoiceId { get; set; } = string.Empty;

    /// <summary>Initializes the Godot control and connects its UI signals.</summary>
    public override void _Ready()
    {
        Pressed += EmitChoiceSelected;
    }

    private void EmitChoiceSelected()
    {
        ChoiceSelected?.Invoke(ChoiceId);
    }
}
