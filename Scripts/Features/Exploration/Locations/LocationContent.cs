using System;

/// <summary>Defines the data and available interactions for a location.</summary>
public sealed class LocationContent
{
    /// <summary>Gets the id.</summary>
    public string Id { get; }
    /// <summary>Gets the name.</summary>
    public string Name { get; }
    /// <summary>Gets the region.</summary>
    public string Region { get; }

    // Kept as an alias for callers that still use the original single description.
    /// <summary>Gets the description.</summary>
    public string Description => FirstVisitDescription;
    /// <summary>Gets the first visit description.</summary>
    public string FirstVisitDescription { get; }
    /// <summary>Gets the return visit description.</summary>
    public string ReturnVisitDescription { get; }

    /// <summary>Gets the exits.</summary>
    public string[] Exits { get; }
    /// <summary>Gets the actions.</summary>
    public ActionDefinition[] Actions { get; }
    /// <summary>Gets the events.</summary>
    public LocationEventDefinition[] Events { get; }

    /// <summary>Initializes a new instance of <see cref="LocationContent"/>.</summary>
    /// <param name="id">The id used to initialize the instance.</param>
    /// <param name="name">The name used to initialize the instance.</param>
    /// <param name="region">The region used to initialize the instance.</param>
    /// <param name="description">The description used to initialize the instance.</param>
    /// <param name="exits">The exits used to initialize the instance.</param>
    /// <param name="actions">The actions used to initialize the instance.</param>
    /// <param name="events">The events used to initialize the instance.</param>
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

    /// <summary>Initializes a new instance of <see cref="LocationContent"/>.</summary>
    /// <param name="id">The id used to initialize the instance.</param>
    /// <param name="name">The name used to initialize the instance.</param>
    /// <param name="region">The region used to initialize the instance.</param>
    /// <param name="firstVisitDescription">The first visit description used to initialize the instance.</param>
    /// <param name="returnVisitDescription">The return visit description used to initialize the instance.</param>
    /// <param name="exits">The exits used to initialize the instance.</param>
    /// <param name="actions">The actions used to initialize the instance.</param>
    /// <param name="events">The events used to initialize the instance.</param>
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

    /// <summary>Returns description.</summary>
    /// <param name="firstVisit">The first visit used by this operation.</param>
    /// <returns>The resulting string.</returns>
    public string GetDescription(bool firstVisit)
    {
        return firstVisit ? FirstVisitDescription : ReturnVisitDescription;
    }
}
