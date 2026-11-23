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
    /// <summary>Gets the selectable objects and furniture in this Location.</summary>
    public LocationInteractableDefinition[] Interactables { get; }

    /// <summary>Initializes a new instance of <see cref="LocationContent"/>.</summary>
    /// <param name="id">The id used to initialize the instance.</param>
    /// <param name="name">The name used to initialize the instance.</param>
    /// <param name="region">The region used to initialize the instance.</param>
    /// <param name="description">The description used to initialize the instance.</param>
    /// <param name="exits">The exits used to initialize the instance.</param>
    /// <param name="actions">The actions used to initialize the instance.</param>
    /// <param name="events">The events used to initialize the instance.</param>
    /// <param name="interactables">The selectable objects available in this Location.</param>
    public LocationContent(
        string id,
        string name,
        string region,
        string description,
        string[] exits,
        ActionDefinition[] actions,
        LocationEventDefinition[] events = null,
        LocationInteractableDefinition[] interactables = null)
        : this(id, name, region, description, description, exits, actions, events, interactables)
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
    /// <param name="interactables">The selectable objects available in this Location.</param>
    public LocationContent(
        string id,
        string name,
        string region,
        string firstVisitDescription,
        string returnVisitDescription,
        string[] exits,
        ActionDefinition[] actions,
        LocationEventDefinition[] events = null,
        LocationInteractableDefinition[] interactables = null)
    {
        Id = id;
        Name = name;
        Region = region;
        FirstVisitDescription = firstVisitDescription ?? string.Empty;
        ReturnVisitDescription = returnVisitDescription ?? FirstVisitDescription;
        Exits = exits ?? Array.Empty<string>();
        Actions = actions ?? Array.Empty<ActionDefinition>();
        Events = events ?? Array.Empty<LocationEventDefinition>();
        Interactables = interactables ?? Array.Empty<LocationInteractableDefinition>();
    }

    /// <summary>Returns description.</summary>
    /// <param name="firstVisit">The first visit used by this operation.</param>
    /// <returns>The resulting string.</returns>
    public string GetDescription(bool firstVisit)
    {
        return firstVisit ? FirstVisitDescription : ReturnVisitDescription;
    }
}

/// <summary>Defines a selectable object in a Location and the actions available through it.</summary>
public sealed class LocationInteractableDefinition
{
    /// <summary>Gets the stable object ID.</summary>
    public string Id { get; }
    /// <summary>Gets the player-facing object name.</summary>
    public string Name { get; }
    /// <summary>Gets the object description.</summary>
    public string Description { get; }
    /// <summary>Gets the IDs of Player Actions exposed by this object.</summary>
    public string[] PlayerActionIds { get; }
    /// <summary>Gets actions owned by this object.</summary>
    public ActionDefinition[] Actions { get; }

    /// <summary>Initializes a new selectable object in a Location.</summary>
    /// <param name="id">The stable object ID.</param>
    /// <param name="name">The player-facing object name.</param>
    /// <param name="description">The object description.</param>
    /// <param name="playerActionIds">Existing Player Action IDs exposed through this object.</param>
    /// <param name="actions">Object-owned actions.</param>
    public LocationInteractableDefinition(
        string id,
        string name,
        string description,
        string[] playerActionIds = null,
        ActionDefinition[] actions = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Interactable ID is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Interactable name is required.", nameof(name));

        Id = id;
        Name = name;
        Description = description ?? string.Empty;
        PlayerActionIds = playerActionIds == null ? Array.Empty<string>() : (string[])playerActionIds.Clone();
        Actions = actions == null ? Array.Empty<ActionDefinition>() : (ActionDefinition[])actions.Clone();
    }
}
