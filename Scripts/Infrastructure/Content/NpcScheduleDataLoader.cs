using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

/// <summary>Loads npc schedule data data from the configured content source.</summary>
public static class NpcScheduleDataLoader
{
    /// <summary>Performs the load operation for npc schedule data loader.</summary>
    /// <param name="path">The path used by this operation.</param>
    /// <returns>The resulting npc schedule catalog.</returns>
    public static NpcScheduleCatalog Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("NPC schedule content path is required.", nameof(path));
        }

        if (!FileAccess.FileExists(path))
        {
            throw new InvalidOperationException("NPC schedule content file was not found: " + path);
        }

        return LoadJson(FileAccess.GetFileAsString(path));
    }

    /// <summary>Performs the load json operation for npc schedule data loader.</summary>
    /// <param name="json">The json used by this operation.</param>
    /// <returns>The resulting npc schedule catalog.</returns>
    public static NpcScheduleCatalog LoadJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("NPC schedule JSON is required.", nameof(json));
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement schedulesData = document.RootElement.GetProperty("schedules");
            if (schedulesData.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException("NPC schedules must be an array.");
            }

            List<NpcScheduleDefinition> schedules = new List<NpcScheduleDefinition>();
            foreach (JsonElement scheduleData in schedulesData.EnumerateArray())
            {
                string characterId = ReadString(scheduleData, "characterId");
                JsonElement entriesData = scheduleData.GetProperty("entries");
                if (entriesData.ValueKind != JsonValueKind.Array)
                {
                    throw new InvalidOperationException(
                        "NPC schedule entries must be an array for Character '" + characterId + "'.");
                }

                List<NpcScheduleEntryDefinition> entries = new List<NpcScheduleEntryDefinition>();
                foreach (JsonElement entryData in entriesData.EnumerateArray())
                {
                    entries.Add(new NpcScheduleEntryDefinition(
                        entryData.GetProperty("startMinute").GetInt32(),
                        entryData.GetProperty("endMinute").GetInt32(),
                        ReadString(entryData, "locationId"),
                        ReadString(entryData, "activityDescription")));
                }

                schedules.Add(new NpcScheduleDefinition(characterId, entries.ToArray()));
            }

            return new NpcScheduleCatalog(schedules);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Invalid NPC schedule JSON.", exception);
        }
    }

    private static string ReadString(JsonElement data, string propertyName)
    {
        string value = data.GetProperty(propertyName).GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("NPC schedule field is required: " + propertyName);
        }

        return value;
    }
}
