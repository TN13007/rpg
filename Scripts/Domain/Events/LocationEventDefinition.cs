using System;
using System.Collections.Generic;

public sealed class LocationEventChoiceDefinition
{
    public string Id { get; }
    public ActionDefinition Action { get; }
    public string EncounterActionId { get; }

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

public sealed class LocationEventDefinition
{
    public string Id { get; }
    public string Title { get; }
    public string Region { get; }
    public string Description { get; }
    public double Chance { get; }
    public LocationEventChoiceDefinition[] Choices { get; }

    public LocationEventDefinition(
        string id,
        string title,
        string region,
        string description,
        double chance,
        LocationEventChoiceDefinition[] choices)
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
    }
}
