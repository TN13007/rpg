using System;
using Godot;

public partial class StoryChoiceButton : Button
{
    public event Action<string> ChoiceSelected;
    public string ChoiceId { get; set; } = string.Empty;

    public override void _Ready()
    {
        Pressed += EmitChoiceSelected;
    }

    private void EmitChoiceSelected()
    {
        ChoiceSelected?.Invoke(ChoiceId);
    }
}
