using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

public sealed class StoryChoice
{
    public string Id { get; }
    public string Label { get; }

    public StoryChoice(string id, string label)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Choice ID is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Choice label is required.", nameof(label));
        }

        Id = id;
        Label = label;
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
            JsonElement locations = document.RootElement.GetProperty("locations");

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
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Invalid academy content JSON: " + path, exception);
        }

        if (loadedLocations.Count == 0)
        {
            throw new InvalidOperationException("Academy content must contain at least one location.");
        }

        _locations = loadedLocations;
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
            actions.Add(new StoryChoice(
                ReadString(item, "id"),
                ReadString(item, "label")));
        }

        return actions.ToArray();
    }
}
