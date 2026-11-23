using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>Parses serialized action definition json data into domain definitions.</summary>
public static class ActionDefinitionJsonReader
{
    /// <summary>Reads action definition and converts it into domain definitions.</summary>
    /// <param name="id">The id used by this operation.</param>
    /// <param name="label">The label used by this operation.</param>
    /// <param name="data">The data used by this operation.</param>
    /// <returns>The resulting action definition.</returns>
    public static ActionDefinition ReadActionDefinition(string id, string label, JsonElement data)
    {
        int durationMinutes = data.GetProperty("durationMinutes").GetInt32();
        double activityMet = data.TryGetProperty("activityMet", out JsonElement activityMetData)
            ? activityMetData.GetDouble()
            : 1.0;
        ActivityTimeModifier modifier = data.TryGetProperty("timeModifier", out JsonElement modifierData)
            ? ReadTimeModifier(modifierData)
            : null;
        ActionEffectDefinition[] effects = data.TryGetProperty("effects", out JsonElement effectsData)
            ? ReadEffects(effectsData)
            : Array.Empty<ActionEffectDefinition>();
        string feedback = data.TryGetProperty("feedback", out JsonElement feedbackData)
            ? feedbackData.GetString()
            : string.Empty;
        int? estimatedDurationMinMinutes = data.TryGetProperty("estimatedDurationMinMinutes", out JsonElement minDurationData)
            ? minDurationData.GetInt32()
            : null;
        int? estimatedDurationMaxMinutes = data.TryGetProperty("estimatedDurationMaxMinutes", out JsonElement maxDurationData)
            ? maxDurationData.GetInt32()
            : null;
        string requiredNpcId = data.TryGetProperty("requiredNpcId", out JsonElement requiredNpcData)
            ? requiredNpcData.GetString()
            : null;
        int? availableStartMinute = data.TryGetProperty("availableStartMinute", out JsonElement startMinuteData)
            ? startMinuteData.GetInt32()
            : null;
        int? availableEndMinute = data.TryGetProperty("availableEndMinute", out JsonElement endMinuteData)
            ? endMinuteData.GetInt32()
            : null;

        return new ActionDefinition(
            id,
            label,
            durationMinutes,
            modifier,
            feedback,
            activityMet,
            effects,
            estimatedDurationMinMinutes,
            estimatedDurationMaxMinutes,
            requiredNpcId,
            availableStartMinute,
            availableEndMinute);
    }

    private static ActionEffectDefinition[] ReadEffects(JsonElement data)
    {
        if (data.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Action effects must be an array.");
        }

        List<ActionEffectDefinition> effects = new List<ActionEffectDefinition>();

        foreach (JsonElement item in data.EnumerateArray())
        {
            string typeName = ReadString(item, "type");

            if (!Enum.TryParse(typeName, true, out ActionEffectType type) ||
                !Enum.IsDefined(typeof(ActionEffectType), type))
            {
                throw new InvalidOperationException("Unknown action effect type: " + typeName);
            }

            int amount = item.GetProperty("amount").GetInt32();
            effects.Add(new ActionEffectDefinition(type, amount));
        }

        return effects.ToArray();
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

    private static string ReadString(JsonElement data, string propertyName)
    {
        string value = data.GetProperty(propertyName).GetString();

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Academy content field is required: " + propertyName);
        }

        return value;
    }
}
