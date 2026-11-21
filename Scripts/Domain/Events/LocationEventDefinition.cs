using System;
using System.Collections.Generic;

/// <summary>Defines the location event choice data and its validation rules.</summary>
public sealed class LocationEventChoiceDefinition
{
    /// <summary>Gets the id.</summary>
    public string Id { get; }
    /// <summary>Gets the action.</summary>
    public ActionDefinition Action { get; }
    /// <summary>Gets the encounter action id.</summary>
    public string EncounterActionId { get; }

    /// <summary>Initializes a new instance of <see cref="LocationEventChoiceDefinition"/>.</summary>
    /// <param name="id">The id used to initialize the instance.</param>
    /// <param name="action">The action used to initialize the instance.</param>
    /// <param name="encounterActionId">The encounter action id used to initialize the instance.</param>
    public LocationEventChoiceDefinition(string id, ActionDefinition action, string encounterActionId = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Event choice ID is required.", nameof(id));
        }

        Action = action ?? throw new ArgumentNullException(nameof(action));

        if (action.Id != id)
        {
            throw new ArgumentException("Event choice ID must match its Action ID.", nameof(action));
        }

        if (!string.IsNullOrWhiteSpace(encounterActionId) && encounterActionId == action.Id)
        {
            throw new ArgumentException("An event choice cannot reference itself as its encounter action.", nameof(encounterActionId));
        }

        Id = id;
        EncounterActionId = string.IsNullOrWhiteSpace(encounterActionId) ? null : encounterActionId;
    }
}

/// <summary>Defines the location event data and its validation rules.</summary>
public sealed class LocationEventDefinition
{
    /// <summary>Gets the id.</summary>
    public string Id { get; }
    /// <summary>Gets the title.</summary>
    public string Title { get; }
    /// <summary>Gets the region.</summary>
    public string Region { get; }
    /// <summary>Gets the description.</summary>
    public string Description { get; }
    /// <summary>Gets the chance.</summary>
    public double Chance { get; }
    /// <summary>Gets the choices.</summary>
    public LocationEventChoiceDefinition[] Choices { get; }
    /// <summary>Gets the required npc id.</summary>
    public string RequiredNpcId { get; }

    /// <summary>Initializes a new instance of <see cref="LocationEventDefinition"/>.</summary>
    /// <param name="id">The id used to initialize the instance.</param>
    /// <param name="title">The title used to initialize the instance.</param>
    /// <param name="region">The region used to initialize the instance.</param>
    /// <param name="description">The description used to initialize the instance.</param>
    /// <param name="chance">The chance used to initialize the instance.</param>
    /// <param name="choices">The choices used to initialize the instance.</param>
    /// <param name="requiredNpcId">The required npc id used to initialize the instance.</param>
    public LocationEventDefinition(
        string id,
        string title,
        string region,
        string description,
        double chance,
        LocationEventChoiceDefinition[] choices,
        string requiredNpcId = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Location event ID is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Location event title is required.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(region))
        {
            throw new ArgumentException("Location event region is required.", nameof(region));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Location event description is required.", nameof(description));
        }

        if (double.IsNaN(chance) || double.IsInfinity(chance) || chance < 0 || chance > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(chance), "Event chance must be between 0 and 1.");
        }

        if (choices == null || choices.Length == 0)
        {
            throw new ArgumentException("A location event must contain at least one choice.", nameof(choices));
        }

        HashSet<string> choiceIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (LocationEventChoiceDefinition choice in choices)
        {
            if (choice == null)
            {
                throw new ArgumentException("Location event choices cannot contain null values.", nameof(choices));
            }

            if (!choiceIds.Add(choice.Id))
            {
                throw new ArgumentException("Duplicate event choice ID: " + choice.Id, nameof(choices));
            }
        }

        Id = id;
        Title = title;
        Region = region;
        Description = description;
        Chance = chance;
        Choices = (LocationEventChoiceDefinition[])choices.Clone();
        RequiredNpcId = string.IsNullOrWhiteSpace(requiredNpcId) ? null : requiredNpcId;
    }
}
