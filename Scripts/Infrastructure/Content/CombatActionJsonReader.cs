using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>Parses serialized combat action json data into domain definitions.</summary>
public static class CombatActionJsonReader
{
    /// <summary>Reads read and converts it into domain definitions.</summary>
    /// <param name="json">The json used by this operation.</param>
    /// <returns>The resulting combat action definition.</returns>
    public static CombatActionDefinition[] Read(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Combat action JSON is required.", nameof(json));
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement actionsData = document.RootElement.GetProperty("actions");
            if (actionsData.ValueKind != JsonValueKind.Array) throw new InvalidOperationException("Combat actions must be an array.");
            List<CombatActionDefinition> actions = new List<CombatActionDefinition>();
            foreach (JsonElement item in actionsData.EnumerateArray())
            {
                string behaviorName = ReadString(item, "behavior");
                if (!Enum.TryParse(behaviorName, true, out CombatActionBehavior behavior) ||
                    !Enum.IsDefined(typeof(CombatActionBehavior), behavior))
                    throw new InvalidOperationException("Unknown combat action behavior: " + behaviorName);
                actions.Add(new CombatActionDefinition(
                    ReadString(item, "id"), ReadString(item, "label"),
                    ReadOptionalString(item, "costLabel"), ReadOptionalString(item, "description"),
                    behavior, ReadOptionalInt(item, "staminaCost"), ReadOptionalInt(item, "damage"),
                    ReadOptionalInt(item, "recoveryAmount"), ReadOptionalString(item, "unavailableReason"),
                    ReadOptionalString(item, "actionDetail"), ReadOptionalDouble(item, "recoveryCost", 100),
                    ReadOptionalDouble(item, "delayAmount"), ReadOptionalDouble(item, "stunDuration")));
            }
            return actions.ToArray();
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Invalid combat action JSON.", exception);
        }
    }

    private static string ReadString(JsonElement data, string name)
    {
        string value = data.GetProperty(name).GetString();
        if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("Combat action field is required: " + name);
        return value;
    }

    private static string ReadOptionalString(JsonElement data, string name)
    {
        return data.TryGetProperty(name, out JsonElement value) ? value.GetString() ?? string.Empty : string.Empty;
    }

    private static int ReadOptionalInt(JsonElement data, string name)
    {
        return data.TryGetProperty(name, out JsonElement value) ? value.GetInt32() : 0;
    }

    private static double ReadOptionalDouble(JsonElement data, string name, double defaultValue = 0)
    {
        return data.TryGetProperty(name, out JsonElement value) ? value.GetDouble() : defaultValue;
    }
}
