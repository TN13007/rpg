using Godot;

public partial class CombatCardVfx : Node
{
    private CanvasItem _visual;

    public override void _Ready()
    {
        _visual = GetParent() as CanvasItem;
        if (_visual == null)
        {
            GD.PushError("CombatCardVfx must be a child of a CanvasItem.");
        }
    }

    public void PlayFeedback(CombatFeedbackType feedbackType)
    {
        Color color = feedbackType switch
        {
            CombatFeedbackType.Attack => new Color("#e7a45d"),
            CombatFeedbackType.Guard => new Color("#78a9d2"),
            CombatFeedbackType.Evade => new Color("#79c9d2"),
            CombatFeedbackType.Recover => new Color("#8bc78d"),
            CombatFeedbackType.Revive => new Color("#a5d68d"),
            CombatFeedbackType.Hit => new Color("#e07870"),
            _ => Colors.White
        };

        PlayPulse(color, 0.28, 0.12);
    }

    public void PlayTargetSelected()
    {
        PlayPulse(new Color("#f0c56e"), 0.22, 0.0);
    }

    private void PlayPulse(Color color, double duration, double delay)
    {
        if (_visual == null || !GodotObject.IsInstanceValid(_visual))
        {
            return;
        }

        Tween tween = CreateTween();
        tween.TweenProperty(_visual, "self_modulate", color, 0.04);
        tween.TweenInterval(delay);
        tween.TweenProperty(_visual, "self_modulate", Colors.White, duration);
    }
}
