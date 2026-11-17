public sealed class ActionChoiceViewData
{
    public string Id { get; }
    public string Label { get; }
    public string DurationLabel { get; }

    public ActionChoiceViewData(string id, string label, string durationLabel)
    {
        Id = id ?? string.Empty;
        Label = label ?? string.Empty;
        DurationLabel = durationLabel ?? string.Empty;
    }
}
