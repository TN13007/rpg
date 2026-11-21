using System;
using Godot;

public partial class CombatActionCard : Button
{
    public event Action<CombatActionOption> Selected;

    private CombatActionOption _action;

    public override void _Ready()
    {
        FocusMode = FocusModeEnum.All;
        MouseDefaultCursorShape = CursorShape.PointingHand;
        Pressed += HandlePressed;
    }

    public void SetAction(CombatActionOption action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        _action = action;
        string cost = string.IsNullOrWhiteSpace(action.CostLabel)
            ? string.Empty
            : "  ·  " + action.CostLabel;
        string detail = string.IsNullOrWhiteSpace(action.Description)
            ? string.Empty
            : "\n" + action.Description;
        string unavailableReason = action.IsAvailable || string.IsNullOrWhiteSpace(action.UnavailableReason)
            ? string.Empty
            : "\n" + action.UnavailableReason;

        Text = action.Label + cost + detail + unavailableReason;
        Disabled = !action.IsAvailable;
    }

    private void HandlePressed()
    {
        if (_action != null && _action.IsAvailable)
        {
            Selected?.Invoke(_action);
        }
    }
}
