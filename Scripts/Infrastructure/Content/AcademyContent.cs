using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

public sealed class AcademyContent
{
    private Dictionary<string, LocationContent> _locations =
        new Dictionary<string, LocationContent>(StringComparer.Ordinal);

    public IEnumerable<LocationContent> All => _locations.Values;
    public ActionDefinition Travel { get; private set; } = new ActionDefinition("travel", "Travel", 10);
    public ActionDefinition CombatEncounter { get; private set; } = new ActionDefinition("combat_encounter", "Combat Encounter", 30);

    public void Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Academy content path is required.", nameof(path));
        }

        if (!FileAccess.FileExists(path))
        {
            throw new InvalidOperationException("Academy content file was not found: " + path);
        }

        string json = FileAccess.GetFileAsString(path);
        LoadJson(json);
    }

    public void LoadJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("Academy content JSON is required.", nameof(json));
        }

        Dictionary<string, LocationContent> loadedLocations =
            new Dictionary<string, LocationContent>(StringComparer.Ordinal);

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;

            ActionDefinition loadedTravel = root.TryGetProperty("travel", out JsonElement travelData)
                ? ActionDefinitionJsonReader.ReadActionDefinition("travel", "Travel", travelData)
                : new ActionDefinition("travel", "Travel", 10);

            ActionDefinition loadedCombatEncounter = root.TryGetProperty("combatEncounter", out JsonElement combatData)
                ? ActionDefinitionJsonReader.ReadActionDefinition("combat_encounter", "Combat Encounter", combatData)
                : new ActionDefinition("combat_encounter", "Combat Encounter", 30);
            if (root.TryGetProperty("selfActions", out _))
            {
                throw new InvalidOperationException("Self actions must be defined in Player.json as playerActions.");
            }

            JsonElement locations = root.GetProperty("locations");

            foreach (JsonElement locationData in locations.EnumerateArray())
            {
                string id = ReadString(locationData, "id");
                string name = ReadString(locationData, "name");
                string region = ReadString(locationData, "region");
                string description = ReadString(locationData, "description");
                string[] exits = ReadStringArray(locationData.GetProperty("exits"));
                ActionDefinition[] actions = ReadActions(locationData.GetProperty("actions"));
                LocationEventDefinition[] events = locationData.TryGetProperty("events", out JsonElement eventsData)
                    ? ReadEvents(eventsData)
                    : Array.Empty<LocationEventDefinition>();

                if (loadedLocations.ContainsKey(id))
                {
                    throw new InvalidOperationException("Duplicate location ID: " + id);
                }

                ValidateActionIds(id, actions);
                ValidateLocationEvents(id, actions, events);
                loadedLocations.Add(id, new LocationContent(id, name, region, description, exits, actions, events));
            }

            if (loadedLocations.Count == 0)
            {
                throw new InvalidOperationException("Academy content must contain at least one location.");
            }

            ValidateLocationExits(loadedLocations);

            _locations = loadedLocations;
            Travel = loadedTravel;
            CombatEncounter = loadedCombatEncounter;
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Invalid academy content JSON.", exception);
        }
    }

    public LocationContent Get(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Location ID is required.", nameof(id));
        }

        if (!_locations.TryGetValue(id, out LocationContent location))
        {
            throw new KeyNotFoundException("Location was not found: " + id);
        }

        return location;
    }

    private static void ValidateLocationEvents(
        string locationId,
        ActionDefinition[] actions,
        LocationEventDefinition[] events)
    {
        HashSet<string> eventIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (LocationEventDefinition locationEvent in events)
        {
            if (locationEvent == null)
            {
                throw new InvalidOperationException("Location events cannot contain null values.");
            }

            if (!eventIds.Add(locationEvent.Id))
            {
                throw new InvalidOperationException(
                    "Duplicate event ID '" + locationEvent.Id + "' in location '" + locationId + "'.");
            }

            foreach (LocationEventChoiceDefinition choice in locationEvent.Choices)
            {
                if (choice.EncounterActionId == null)
                {
                    continue;
                }

                bool actionFound = false;
                foreach (ActionDefinition action in actions)
                {
                    if (action.Id == choice.EncounterActionId)
                    {
                        actionFound = true;
                        break;
                    }
                }

                if (!actionFound)
                {
                    throw new InvalidOperationException(
                        "Event choice '" + choice.Id + "' in location '" + locationId
                        + "' references unknown encounter action '" + choice.EncounterActionId + "'.");
                }
            }
        }
    }

    private static LocationEventDefinition[] ReadEvents(JsonElement data)
    {
        if (data.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Location events must be an array.");
        }

        List<LocationEventDefinition> events = new List<LocationEventDefinition>();

        foreach (JsonElement item in data.EnumerateArray())
        {
            string id = ReadString(item, "id");
            string title = ReadString(item, "title");
            string region = ReadString(item, "region");
            string description = ReadString(item, "description");
            double chance = item.GetProperty("chance").GetDouble();
            JsonElement choicesData = item.GetProperty("choices");

            if (choicesData.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException("Location event choices must be an array.");
            }

            List<LocationEventChoiceDefinition> choices = new List<LocationEventChoiceDefinition>();
            foreach (JsonElement choiceData in choicesData.EnumerateArray())
            {
                string choiceId = ReadString(choiceData, "id");
                string label = ReadString(choiceData, "label");
                string encounterActionId = choiceData.TryGetProperty("encounterActionId", out JsonElement encounterData)
                    ? encounterData.GetString()
                    : null;
                ActionDefinition action = ActionDefinitionJsonReader.ReadActionDefinition(choiceId, label, choiceData);
                choices.Add(new LocationEventChoiceDefinition(choiceId, action, encounterActionId));
            }

            events.Add(new LocationEventDefinition(
                id,
                title,
                region,
                description,
                chance,
                choices.ToArray()));
        }

        return events.ToArray();
    }

    private static void ValidateActionIds(string locationId, ActionDefinition[] actions)
    {
        HashSet<string> actionIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (ActionDefinition action in actions)
        {
            if (!actionIds.Add(action.Id))
            {
                throw new InvalidOperationException(
                    "Duplicate action ID '" + action.Id + "' in location '" + locationId + "'.");
            }
        }
    }

    private static void ValidateLocationExits(Dictionary<string, LocationContent> locations)
    {
        foreach (LocationContent location in locations.Values)
        {
            HashSet<string> exitIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (string exitId in location.Exits)
            {
                if (!exitIds.Add(exitId))
                {
                    throw new InvalidOperationException(
                        "Duplicate exit '" + exitId + "' in location '" + location.Id + "'.");
                }

                if (!locations.ContainsKey(exitId))
                {
                    throw new InvalidOperationException(
                        "Location '" + location.Id + "' references unknown exit '" + exitId + "'.");
                }
            }
        }
    }

    private static string ReadString(JsonElement data, string propertyName)
    {
        string value = data.GetProperty(propertyName).GetString();

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Academy content field is required: " + propertyName);
        }

        return value;
    }

    private static string[] ReadStringArray(JsonElement data)
    {
        List<string> values = new List<string>();

        foreach (JsonElement item in data.EnumerateArray())
        {
            string value = item.GetString();

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException("Location exit IDs cannot be empty.");
            }

            values.Add(value);
        }

        return values.ToArray();
    }

    private static ActionDefinition[] ReadActions(JsonElement data)
    {
        List<ActionDefinition> actions = new List<ActionDefinition>();

        foreach (JsonElement item in data.EnumerateArray())
        {
            actions.Add(ActionDefinitionJsonReader.ReadActionDefinition(
                ReadString(item, "id"),
                ReadString(item, "label"),
                item));
        }

        return actions.ToArray();
    }

    

    

    

    
}
