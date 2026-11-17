using System;

public sealed class ActionPreviewData
{
    public string Title { get; }
    public string CurrentTime { get; }
    public string Duration { get; }
    public string EnergyCost { get; }
    public string ExpectedTime { get; }
    public string Effects { get; }
    public bool CanConfirm { get; }
    public string RejectionReason { get; }

    public ActionPreviewData(string title, string currentTime, string duration, string energyCost, string expectedTime, string effects, bool canConfirm, string rejectionReason)
    {
        Title = title ?? string.Empty;
        CurrentTime = currentTime ?? string.Empty;
        Duration = duration ?? string.Empty;
        EnergyCost = energyCost ?? string.Empty;
        ExpectedTime = expectedTime ?? string.Empty;
        Effects = effects ?? string.Empty;
        CanConfirm = canConfirm;
        RejectionReason = rejectionReason ?? string.Empty;
    }
}
