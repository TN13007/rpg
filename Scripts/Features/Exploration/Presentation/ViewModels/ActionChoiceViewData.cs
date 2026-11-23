/// <summary>Carries action choice information to the presentation layer.</summary>
public sealed class ActionChoiceViewData
{
    /// <summary>Gets the id.</summary>
    public string Id { get; }
    /// <summary>Gets the label.</summary>
    public string Label { get; }
    /// <summary>Gets the duration label.</summary>
    public string DurationLabel { get; }
    /// <summary>Gets the group title.</summary>
    public string GroupTitle { get; }

    /// <summary>Initializes a new instance of <see cref="ActionChoiceViewData"/>.</summary>
    /// <param name="id">The id used to initialize the instance.</param>
    /// <param name="label">The label used to initialize the instance.</param>
    /// <param name="durationLabel">The duration label used to initialize the instance.</param>
    /// <param name="groupTitle">The group title used to initialize the instance.</param>
    public ActionChoiceViewData(string id, string label, string durationLabel, string groupTitle = "")
    {
        Id = id ?? string.Empty;
        Label = label ?? string.Empty;
        DurationLabel = durationLabel ?? string.Empty;
        GroupTitle = groupTitle ?? string.Empty;
    }
}
