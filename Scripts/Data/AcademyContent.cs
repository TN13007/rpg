using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

public sealed class ActivityTimeModifier
{
    public CharacterStat Stat { get; }
    public double MaxReduction { get; }

    public ActivityTimeModifier(CharacterStat stat, double maxReduction)
    {
        if (!Enum.IsDefined(typeof(CharacterStat), stat))
        {
            throw new ArgumentOutOfRangeException(nameof(stat));
        }

        if (maxReduction < 0 || maxReduction > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxReduction), "Maximum time reduction must be between 0 and 1.");
        }

        Stat = stat;
        MaxReduction = maxReduction;
    }
}

public sealed class ActionDefinition
{
    public string Id { get; }
    public string Label { get; }
    public string Feedback { get; }
    public int DurationMinutes { get; }
    public ActivityTimeModifier TimeModifier { get; }

    public ActionDefinition(string id, string label, int durationMinutes, ActivityTimeModifier timeModifier = null, string feedback = "")
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Action ID is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Action label is required.", nameof(label));
        if (durationMinutes < 0) throw new ArgumentOutOfRangeException(nameof(durationMinutes));
        Id = id;
        Label = label;
        DurationMinutes = durationMinutes;
        TimeModifier = timeModifier;
        Feedback = feedback ?? string.Empty;

        /*
         * FUTURE: Conditions - define when this action is available.
         * FUTURE: Costs - define resources consumed before execution.
         * FUTURE: Effects - define state changes after completion.
         * FUTURE: Modifiers - adjust results using character/world state.
         * FUTURE: Events - trigger events such as nightmares after sleep.
         * These features are intentionally not implemented yet.
         */
    }
}

public sealed class LocationContent
{
    public string Id { get; }
    public string Name { get; }
    public string Region { get; }
    public string Description { get; }
    public string[] Exits { get; }
    public ActionDefinition[] Actions { get; }

    public LocationContent(string id, string name, string region, string description, string[] exits, ActionDefinition[] actions)
    {
        Id = id;
        Name = name;
        Region = region;
        Description = description;
        Exits = exits ?? Array.Empty<string>();
        Actions = actions ?? Array.Empty<ActionDefinition>();
    }
}

public static class AcademyContent
{
    private static Dictionary<string, LocationContent> _locations =
        new Dictionary<string, LocationContent>(StringComparer.Ordinal);

    public static IEnumerable<LocationContent> All => _locations.Values;
    public static ActionDefinition Travel { get; private set; } = new ActionDefinition("travel", "Travel", 10);
    public static ActionDefinition CombatEncounter { get; private set; } = new ActionDefinition("combat_encounter", "Combat Encounter", 30);

    public static void Load(string path)
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
        Dictionary<string, LocationContent> loadedLocations =
            new Dictionary<string, LocationContent>(StringComparer.Ordinal);

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;

            ActionDefinition loadedTravel = root.TryGetProperty("travel", out JsonElement travelData)
                ? ReadActionDefinition("travel", "Travel", travelData)
                : new ActionDefinition("travel", "Travel", 10);

            ActionDefinition loadedCombatEncounter = root.TryGetProperty("combatEncounter", out JsonElement combatData)
                ? ReadActionDefinition("combat_encounter", "Combat Encounter", combatData)
                : new ActionDefinition("combat_encounter", "Combat Encounter", 30);

            JsonElement locations = root.GetProperty("locations");

            foreach (JsonElement locationData in locations.EnumerateArray())
            {
                string id = ReadString(locationData, "id");
                string name = ReadString(locationData, "name");
                string region = ReadString(locationData, "region");
                string description = ReadString(locationData, "description");
                string[] exits = ReadStringArray(locationData.GetProperty("exits"));
                ActionDefinition[] actions = ReadActions(locationData.GetProperty("actions"));

                if (loadedLocations.ContainsKey(id))
                {
                    throw new InvalidOperationException("Duplicate location ID: " + id);
                }

                ValidateActionIds(id, actions);
                loadedLocations.Add(id, new LocationContent(id, name, region, description, exits, actions));
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
            throw new InvalidOperationException("Invalid academy content JSON: " + path, exception);
        }
    }

    public static LocationContent Get(string id)
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
            int durationMinutes = item.GetProperty("durationMinutes").GetInt32();
            ActivityTimeModifier modifier = item.TryGetProperty("timeModifier", out JsonElement modifierData) ? ReadTimeModifier(modifierData) : null;
            string feedback = item.TryGetProperty("feedback", out JsonElement feedbackData)
                ? feedbackData.GetString()
                : string.Empty;

            actions.Add(new ActionDefinition(
                ReadString(item, "id"),
                ReadString(item, "label"),
                durationMinutes,
                modifier,
                feedback));
        }

        return actions.ToArray();
    }

    private static ActionDefinition ReadActionDefinition(string id, string label, JsonElement data)
    {
        int durationMinutes = data.GetProperty("durationMinutes").GetInt32();
        ActivityTimeModifier modifier = data.TryGetProperty("timeModifier", out JsonElement modifierData)
            ? ReadTimeModifier(modifierData)
            : null;

        return new ActionDefinition(id, label, durationMinutes, modifier);
    }

    private static ActivityTimeModifier ReadTimeModifier(JsonElement data)
    {
        string statName = ReadString(data, "stat");

        if (!Enum.TryParse(statName, true, out CharacterStat stat) ||
            !Enum.IsDefined(typeof(CharacterStat), stat))
        {
            throw new InvalidOperationException("Unknown character stat in time modifier: " + statName);
        }

        double maxReduction = data.GetProperty("maxReduction").GetDouble();
        return new ActivityTimeModifier(stat, maxReduction);
    }
}
