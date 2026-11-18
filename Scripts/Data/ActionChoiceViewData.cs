public sealed class ActionChoiceViewData
{
    public string Id { get; }
    public string Label { get; }
    public string DurationLabel { get; }
    public string GroupTitle { get; }

    public ActionChoiceViewData(string id, string label, string durationLabel, string groupTitle = "")
    {
        Id = id ?? string.Empty;
        Label = label ?? string.Empty;
        DurationLabel = durationLabel ?? string.Empty;
        GroupTitle = groupTitle ?? string.Empty;
    }
}
