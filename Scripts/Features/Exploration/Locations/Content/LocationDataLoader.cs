using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

/// <summary>Reads and validates authored Location content before creating its in-memory catalog.</summary>
public static class LocationDataLoader
{
    /// <summary>Loads and validates Location content from a Godot resource path.</summary>
    /// <param name="path">The Godot resource path to the JSON file.</param>
    public static LocationCatalog Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Location content path is required.", nameof(path));
        }

        if (!FileAccess.FileExists(path))
        {
            throw new InvalidOperationException("Location content file was not found: " + path);
        }

        string json = FileAccess.GetFileAsString(path);
        return LoadJson(json);
    }

    /// <summary>Parses and validates authored Location JSON.</summary>
    /// <param name="json">The authored Location JSON.</param>
    /// <returns>A catalog containing the validated Location definitions.</returns>
    public static LocationCatalog LoadJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("Location content JSON is required.", nameof(json));
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
                ReadLocationDescriptions(locationData.GetProperty("description"), out string firstVisitDescription, out string returnVisitDescription);
                string[] exits = ReadStringArray(locationData.GetProperty("exits"));
                ActionDefinition[] actions = ReadActions(locationData.GetProperty("actions"));
                LocationEventDefinition[] events = locationData.TryGetProperty("events", out JsonElement eventsData)
                    ? ReadEvents(eventsData)
                    : Array.Empty<LocationEventDefinition>();
                LocationInteractableDefinition[] interactables = locationData.TryGetProperty("interactables", out JsonElement interactablesData)
                    ? ReadInteractables(interactablesData)
                    : Array.Empty<LocationInteractableDefinition>();
                bool isFastTravelDestination = locationData.TryGetProperty("fastTravelDestination", out JsonElement fastTravelData)
                    && fastTravelData.ValueKind == JsonValueKind.True;
                LocationGridPosition mapPosition = locationData.TryGetProperty("mapPosition", out JsonElement mapPositionData)
                    ? ReadMapPosition(mapPositionData)
                    : null;

                if (loadedLocations.ContainsKey(id))
                {
                    throw new InvalidOperationException("Duplicate location ID: " + id);
                }

                ValidateActionIds(id, actions);
                ValidateInteractables(id, actions, interactables);
                ValidateLocationEvents(id, actions, events);
                loadedLocations.Add(id, new LocationContent(
                    id,
                    name,
                    region,
                    firstVisitDescription,
                    returnVisitDescription,
                    exits,
                    actions,
                    events,
                    interactables,
                    isFastTravelDestination,
                    mapPosition));
            }

            if (loadedLocations.Count == 0)
            {
                throw new InvalidOperationException("Location content must contain at least one location.");
            }

            ValidateLocationExits(loadedLocations);
            ValidateLocationMapPositions(loadedLocations);

            return new LocationCatalog(loadedLocations.Values, loadedTravel, loadedCombatEncounter);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Invalid location content JSON.", exception);
        }
    }

    private static LocationGridPosition ReadMapPosition(JsonElement data)
    {
        if (data.ValueKind != JsonValueKind.Object
            || !data.TryGetProperty("column", out JsonElement columnData)
            || !data.TryGetProperty("row", out JsonElement rowData)
            || !columnData.TryGetInt32(out int column)
            || !rowData.TryGetInt32(out int row)
            || column < 0
            || row < 0)
        {
            throw new InvalidOperationException("Location mapPosition must contain non-negative integer column and row values.");
        }

        return new LocationGridPosition(column, row);
    }

    private static LocationInteractableDefinition[] ReadInteractables(JsonElement data)
    {
        if (data.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Location interactables must be an array.");
        }

        List<LocationInteractableDefinition> interactables = new List<LocationInteractableDefinition>();
        foreach (JsonElement item in data.EnumerateArray())
        {
            string id = ReadString(item, "id");
            string name = ReadString(item, "name");
            string description = item.TryGetProperty("description", out JsonElement descriptionData)
                ? descriptionData.GetString()
                : string.Empty;
            string[] playerActionIds = item.TryGetProperty("playerActionIds", out JsonElement playerActionsData)
                ? ReadInteractablePlayerActionIds(playerActionsData)
                : Array.Empty<string>();
            ActionDefinition[] actions = item.TryGetProperty("actions", out JsonElement actionsData)
                ? ReadActions(actionsData)
                : Array.Empty<ActionDefinition>();

            interactables.Add(new LocationInteractableDefinition(
                id,
                name,
                description,
                playerActionIds,
                actions));
        }

        return interactables.ToArray();
    }

    private static string[] ReadInteractablePlayerActionIds(JsonElement data)
    {
        if (data.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Interactable playerActionIds must be an array.");
        }

        List<string> ids = new List<string>();
        foreach (JsonElement item in data.EnumerateArray())
        {
            string id = item.GetString();
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new InvalidOperationException("Interactable player action IDs cannot be empty.");
            }

            ids.Add(id);
        }

        return ids.ToArray();
    }

    private static void ValidateInteractables(
        string locationId,
        ActionDefinition[] locationActions,
        LocationInteractableDefinition[] interactables)
    {
        HashSet<string> interactableIds = new HashSet<string>(StringComparer.Ordinal);
        HashSet<string> actionIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (ActionDefinition action in locationActions)
        {
            actionIds.Add(action.Id);
        }

        foreach (LocationInteractableDefinition interactable in interactables)
        {
            if (!interactableIds.Add(interactable.Id))
            {
                throw new InvalidOperationException(
                    "Duplicate interactable ID '" + interactable.Id + "' in location '" + locationId + "'.");
            }

            HashSet<string> playerActionIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (string playerActionId in interactable.PlayerActionIds)
            {
                if (!playerActionIds.Add(playerActionId))
                {
                    throw new InvalidOperationException(
                        "Duplicate Player Action reference '" + playerActionId + "' on interactable '" + interactable.Id + "'.");
                }
            }

            foreach (ActionDefinition action in interactable.Actions)
            {
                if (action.Id == "select")
                {
                    throw new InvalidOperationException(
                        "Interactable action ID 'select' is reserved for selecting the object itself.");
                }

                if (!actionIds.Add(action.Id))
                {
                    throw new InvalidOperationException(
                        "Duplicate action ID '" + action.Id + "' in location '" + locationId + "'.");
                }
            }
        }
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
            string requiredNpcId = item.TryGetProperty("requiredNpcId", out JsonElement requiredNpcData)
                ? requiredNpcData.GetString()
                : null;
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
                choices.ToArray(),
                requiredNpcId));
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

    private static void ValidateLocationMapPositions(Dictionary<string, LocationContent> locations)
    {
        HashSet<(int Column, int Row)> occupiedCells = new HashSet<(int Column, int Row)>();

        foreach (LocationContent location in locations.Values)
        {
            if (location.IsFastTravelDestination && location.MapPosition == null)
            {
                throw new InvalidOperationException("Fast-travel Location '" + location.Id + "' must define a mapPosition.");
            }

            if (location.MapPosition == null)
            {
                continue;
            }

            (int Column, int Row) cell = (location.MapPosition.Column, location.MapPosition.Row);
            if (!occupiedCells.Add(cell))
            {
                throw new InvalidOperationException("Duplicate Location mapPosition at column " + cell.Column + ", row " + cell.Row + ".");
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

    private static void ReadLocationDescriptions(
        JsonElement data,
        out string firstVisitDescription,
        out string returnVisitDescription)
    {
        if (data.ValueKind == JsonValueKind.String)
        {
            string legacyDescription = data.GetString();
            if (string.IsNullOrWhiteSpace(legacyDescription))
            {
                throw new InvalidOperationException("Location description cannot be empty.");
            }

            firstVisitDescription = legacyDescription;
            returnVisitDescription = legacyDescription;
            return;
        }

        if (data.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                "Location description must be a string or an object with firstVisit and returnVisit.");
        }

        firstVisitDescription = ReadString(data, "firstVisit");
        returnVisitDescription = ReadString(data, "returnVisit");
    }

    private static string ReadString(JsonElement data, string propertyName)
    {
        string value = data.GetProperty(propertyName).GetString();

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Location content field is required: " + propertyName);
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
