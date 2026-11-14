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

public sealed class ActivityDefinition
{
    public int DurationMinutes { get; }
    public ActivityTimeModifier TimeModifier { get; }

    public ActivityDefinition(int durationMinutes, ActivityTimeModifier timeModifier = null)
    {
        if (durationMinutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationMinutes));
        }

        DurationMinutes = durationMinutes;
        TimeModifier = timeModifier;
    }
}

public sealed class StoryChoice
{
    public string Id { get; }
    public string Label { get; }
    public int DurationMinutes { get; }
    public ActivityTimeModifier TimeModifier { get; }

    public StoryChoice(
        string id,
        string label,
        int durationMinutes = 0,
        ActivityTimeModifier timeModifier = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Choice ID is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Choice label is required.", nameof(label));
        }

        if (durationMinutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationMinutes));
        }

        Id = id;
        Label = label;
        DurationMinutes = durationMinutes;
        TimeModifier = timeModifier;
    }
}

public sealed class LocationContent
{
    public string Id { get; }
    public string Name { get; }
    public string Region { get; }
    public string Description { get; }
    public string[] Exits { get; }
    public StoryChoice[] Actions { get; }

    public LocationContent(string id, string name, string region, string description, string[] exits, StoryChoice[] actions)
    {
        Id = id;
        Name = name;
        Region = region;
        Description = description;
        Exits = exits ?? Array.Empty<string>();
        Actions = actions ?? Array.Empty<StoryChoice>();
    }
}

public static class AcademyContent
{
    private static Dictionary<string, LocationContent> _locations = new Dictionary<string, LocationContent>();

    public static IEnumerable<LocationContent> All => _locations.Values;
    public static ActivityDefinition Travel { get; private set; } = new ActivityDefinition(10);

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
        Dictionary<string, LocationContent> loadedLocations = new Dictionary<string, LocationContent>();

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;
            ActivityDefinition loadedTravel = root.TryGetProperty("travel", out JsonElement travelData)
                ? ReadActivityDefinition(travelData)
                : new ActivityDefinition(10);
            JsonElement locations = root.GetProperty("locations");

            foreach (JsonElement locationData in locations.EnumerateArray())
            {
                string id = ReadString(locationData, "id");
                string name = ReadString(locationData, "name");
                string region = ReadString(locationData, "region");
                string description = ReadString(locationData, "description");
                string[] exits = ReadStringArray(locationData.GetProperty("exits"));
                StoryChoice[] actions = ReadActions(locationData.GetProperty("actions"));

                if (loadedLocations.ContainsKey(id))
                {
                    throw new InvalidOperationException("Duplicate location ID: " + id);
                }

                loadedLocations.Add(id, new LocationContent(id, name, region, description, exits, actions));
            }

            if (loadedLocations.Count == 0)
            {
                throw new InvalidOperationException("Academy content must contain at least one location.");
            }

            _locations = loadedLocations;
            Travel = loadedTravel;
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

            if (!string.IsNullOrWhiteSpace(value))
            {
                values.Add(value);
            }
        }

        return values.ToArray();
    }

    private static StoryChoice[] ReadActions(JsonElement data)
    {
        List<StoryChoice> actions = new List<StoryChoice>();

        foreach (JsonElement item in data.EnumerateArray())
        {
            int durationMinutes = item.TryGetProperty("durationMinutes", out JsonElement duration)
                ? duration.GetInt32()
                : 0;

            ActivityTimeModifier modifier = item.TryGetProperty("timeModifier", out JsonElement modifierData)
                ? ReadTimeModifier(modifierData)
                : null;

            actions.Add(new StoryChoice(
                ReadString(item, "id"),
                ReadString(item, "label"),
                durationMinutes,
                modifier));
        }

        return actions.ToArray();
    }

    private static ActivityDefinition ReadActivityDefinition(JsonElement data)
    {
        int durationMinutes = data.GetProperty("durationMinutes").GetInt32();
        ActivityTimeModifier modifier = data.TryGetProperty("timeModifier", out JsonElement modifierData)
            ? ReadTimeModifier(modifierData)
            : null;

        return new ActivityDefinition(durationMinutes, modifier);
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
