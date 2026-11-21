using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

public static class NpcInteractionDataLoader
{
    public static NpcInteractionCatalog Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("NPC interaction content path is required.", nameof(path));
        }

        if (!FileAccess.FileExists(path))
        {
            throw new InvalidOperationException("NPC interaction content file was not found: " + path);
        }

        return LoadJson(FileAccess.GetFileAsString(path));
    }

    public static NpcInteractionCatalog LoadJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("NPC interaction JSON is required.", nameof(json));
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement interactionsData = document.RootElement.GetProperty("interactions");
            if (interactionsData.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException("NPC interactions must be an array.");
            }

            List<NpcInteractionDefinition> interactions = new List<NpcInteractionDefinition>();
            foreach (JsonElement interactionData in interactionsData.EnumerateArray())
            {
                string characterId = ReadString(interactionData, "characterId");
                string actionId = ReadString(interactionData, "id");
                string label = ReadString(interactionData, "label");
                string[] availableLocationIds = interactionData.TryGetProperty(
                    "availableLocationIds", out JsonElement locationIdsData)
                    ? ReadStringArray(locationIdsData)
                    : Array.Empty<string>();

                ActionDefinition action = ActionDefinitionJsonReader.ReadActionDefinition(
                    actionId,
                    label,
                    interactionData);

                interactions.Add(new NpcInteractionDefinition(
                    characterId,
                    action,
                    availableLocationIds));
            }

            return new NpcInteractionCatalog(interactions);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Invalid NPC interaction JSON.", exception);
        }
    }

    private static string ReadString(JsonElement data, string propertyName)
    {
        string value = data.GetProperty(propertyName).GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("NPC interaction field is required: " + propertyName);
        }

        return value;
    }

    private static string[] ReadStringArray(JsonElement data)
    {
        if (data.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("NPC interaction availableLocationIds must be an array.");
        }

        List<string> values = new List<string>();
        foreach (JsonElement item in data.EnumerateArray())
        {
            string value = item.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException("NPC interaction Location IDs cannot be empty.");
            }

            values.Add(value);
        }

        return values.ToArray();
    }
}
