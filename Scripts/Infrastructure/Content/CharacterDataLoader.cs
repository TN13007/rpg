using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

public static class CharacterDataLoader
{
    public static CharacterData Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Character data path is required.", nameof(path));
        if (!FileAccess.FileExists(path)) throw new InvalidOperationException("Character data file was not found: " + path);
        return LoadJson(FileAccess.GetFileAsString(path), path);
    }

    public static CharacterData LoadJson(string json, string source = "Player.json")
    {
        if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Character data JSON is required.", nameof(json));
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement data = document.RootElement;
            return new CharacterData(
                data.GetProperty("id").GetString(),
                data.GetProperty("name").GetString(),
                data.GetProperty("role").GetString(),
                data.GetProperty("level").GetInt32(),
                data.GetProperty("maxHealth").GetInt32(),
                data.GetProperty("maxFatigue").GetDouble(),
                data.GetProperty("maxStamina").GetInt32(),
                data.GetProperty("maxMood").GetInt32(),
                data.GetProperty("startingHealth").GetInt32(),
                data.GetProperty("startingFatigue").GetDouble(),
                data.GetProperty("startingStamina").GetInt32(),
                data.GetProperty("startingMood").GetInt32(),
                data.GetProperty("startingGold").GetInt32(),
                ReadStats(data),
                ReadPlayerActions(data),
                ReadStringArray(data, "combatActionIds"));
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Invalid character data JSON: " + source, exception);
        }
    }

    private static PlayerActionDefinition[] ReadPlayerActions(JsonElement data)
    {
        if (!data.TryGetProperty("playerActions", out JsonElement actionsData)) return Array.Empty<PlayerActionDefinition>();
        if (actionsData.ValueKind != JsonValueKind.Array) throw new InvalidOperationException("Player actions must be an array.");

        List<PlayerActionDefinition> actions = new List<PlayerActionDefinition>();
        HashSet<string> actionIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (JsonElement item in actionsData.EnumerateArray())
        {
            string id = item.GetProperty("id").GetString();
            string label = item.GetProperty("label").GetString();
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(label))
                throw new InvalidOperationException("Player action IDs and labels are required.");
            if (!actionIds.Add(id)) throw new InvalidOperationException("Duplicate player action ID: " + id);

            string[] availableLocationIds = item.TryGetProperty("availableLocationIds", out JsonElement locationsData)
                ? ReadStringArray(locationsData, "availableLocationIds")
                : Array.Empty<string>();

            actions.Add(new PlayerActionDefinition(
                ActionDefinitionJsonReader.ReadActionDefinition(id, label, item),
                availableLocationIds));
        }
        return actions.ToArray();
    }

    private static string[] ReadStringArray(JsonElement data, string propertyName)
    {
        if (!data.TryGetProperty(propertyName, out JsonElement valuesData)) return Array.Empty<string>();
        return ReadStringArray(valuesData, propertyName);
    }

    private static string[] ReadStringArray(JsonElement data, string propertyName)
    {
        if (data.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException(propertyName + " must be an array.");

        List<string> values = new List<string>();
        HashSet<string> uniqueValues = new HashSet<string>(StringComparer.Ordinal);
        foreach (JsonElement item in data.EnumerateArray())
        {
            string value = item.GetString();
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(propertyName + " cannot contain empty IDs.");
            if (!uniqueValues.Add(value))
                throw new InvalidOperationException(propertyName + " cannot contain duplicate IDs: " + value);
            values.Add(value);
        }
        return values.ToArray();
    }

    private static CharacterStats ReadStats(JsonElement data)
    {
        if (!data.TryGetProperty("stats", out JsonElement stats)) return new CharacterStats(0, 0, 0, 0, 0);
        return new CharacterStats(
            stats.GetProperty("strength").GetInt32(),
            stats.GetProperty("agility").GetInt32(),
            stats.GetProperty("intelligence").GetInt32(),
            stats.GetProperty("endurance").GetInt32(),
            stats.GetProperty("wisdom").GetInt32());
    }
}
