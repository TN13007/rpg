using System;
using System.Collections.Generic;
using System.Text.Json;

public static class CombatActionJsonReader
{
    public static CombatActionDefinition[] Read(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Combat action JSON is required.", nameof(json));

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement actionsData = document.RootElement.GetProperty("actions");
            if (actionsData.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException("Combat actions must be an array.");
            }

            List<CombatActionDefinition> actions = new List<CombatActionDefinition>();
            foreach (JsonElement item in actionsData.EnumerateArray())
            {
                string behaviorName = ReadString(item, "behavior");
                if (!Enum.TryParse(behaviorName, true, out CombatActionBehavior behavior) ||
                    !Enum.IsDefined(typeof(CombatActionBehavior), behavior))
                {
                    throw new InvalidOperationException("Unknown combat action behavior: " + behaviorName);
                }

                actions.Add(new CombatActionDefinition(
                    ReadString(item, "id"),
                    ReadString(item, "label"),
                    ReadOptionalString(item, "costLabel"),
                    ReadOptionalString(item, "description"),
                    behavior,
                    ReadOptionalInt(item, "staminaCost"),
                    ReadOptionalInt(item, "damage"),
                    ReadOptionalInt(item, "recoveryAmount"),
                    ReadOptionalString(item, "unavailableReason"),
                    ReadOptionalString(item, "actionDetail")));
            }

            return actions.ToArray();
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Invalid combat action JSON.", exception);
        }
    }

    private static string ReadString(JsonElement data, string propertyName)
    {
        string value = data.GetProperty(propertyName).GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Combat action field is required: " + propertyName);
        }
        return value;
    }

    private static string ReadOptionalString(JsonElement data, string propertyName)
    {
        return data.TryGetProperty(propertyName, out JsonElement value) ? value.GetString() ?? string.Empty : string.Empty;
    }

    private static int ReadOptionalInt(JsonElement data, string propertyName)
    {
        return data.TryGetProperty(propertyName, out JsonElement value) ? value.GetInt32() : 0;
    }
}
