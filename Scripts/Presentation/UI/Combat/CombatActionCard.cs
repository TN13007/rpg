using System;
using Godot;

/// <summary>Presents a selectable combat action option in the combat interface.</summary>
public partial class CombatActionCard : Button
{
    /// <summary>Raised when selected occurs.</summary>
    public event Action<CombatActionOption> Selected;

    private CombatActionOption _action;

    /// <summary>Initializes the Godot control and connects its UI signals.</summary>
    public override void _Ready()
    {
        FocusMode = FocusModeEnum.All;
        MouseDefaultCursorShape = CursorShape.PointingHand;
        Pressed += HandlePressed;
    }

    /// <summary>Sets action.</summary>
    /// <param name="action">The action used by this operation.</param>
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
