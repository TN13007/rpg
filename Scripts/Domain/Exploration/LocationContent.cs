using System;

public sealed class LocationContent
{
    public string Id { get; }
    public string Name { get; }
    public string Region { get; }

    // Kept as an alias for callers that still use the original single description.
    public string Description => FirstVisitDescription;
    public string FirstVisitDescription { get; }
    public string ReturnVisitDescription { get; }

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
        : this(id, name, region, description, description, exits, actions, events)
    {
    }

    public LocationContent(
        string id,
        string name,
        string region,
        string firstVisitDescription,
        string returnVisitDescription,
        string[] exits,
        ActionDefinition[] actions,
        LocationEventDefinition[] events = null)
    {
        Id = id;
        Name = name;
        Region = region;
        FirstVisitDescription = firstVisitDescription ?? string.Empty;
        ReturnVisitDescription = returnVisitDescription ?? FirstVisitDescription;
        Exits = exits ?? Array.Empty<string>();
        Actions = actions ?? Array.Empty<ActionDefinition>();
        Events = events ?? Array.Empty<LocationEventDefinition>();
    }

    public string GetDescription(bool firstVisit)
    {
        return firstVisit ? FirstVisitDescription : ReturnVisitDescription;
    }
}
