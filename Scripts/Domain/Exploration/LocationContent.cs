using System;

public sealed class LocationContent
{
    public string Id { get; }
    public string Name { get; }
    public string Region { get; }
    public string Description { get; }
    public string[] Exits { get; }
    public ActionDefinition[] Actions { get; }
    public LocationEventDefinition[] Events { get; }

    public LocationContent(
        string id,
        string name,
        string region,
        string description,
        string[] exits,
        ActionDefinition[] actions,
        LocationEventDefinition[] events = null)
    {
        Id = id;
        Name = name;
        Region = region;
        Description = description;
        Exits = exits ?? Array.Empty<string>();
        Actions = actions ?? Array.Empty<ActionDefinition>();
        Events = events ?? Array.Empty<LocationEventDefinition>();
    }
}
