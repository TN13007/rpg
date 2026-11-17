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

public enum ActionTimeMode
{
    Duration,
    NextDay
}

public enum ActionEffectType
{
    RestoreHealth,
    RestoreEnergy,
    ChangeMood,
    AddGold
}

public sealed class ActionEffectDefinition
{
    public ActionEffectType Type { get; }
    public int Amount { get; }

    public ActionEffectDefinition(ActionEffectType type, int amount)
    {
        if (!Enum.IsDefined(typeof(ActionEffectType), type))
        {
            throw new ArgumentOutOfRangeException(nameof(type));
        }

        if (type != ActionEffectType.ChangeMood && amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "This effect amount cannot be negative.");
        }

        Type = type;
        Amount = amount;
    }
}

public sealed class ActionDefinition
{
    public string Id { get; }
    public string Label { get; }
    public string Feedback { get; }
    public int DurationMinutes { get; }
    public int EnergyCost { get; }
    public bool CanExecuteWhenUnconscious { get; }
    public ActivityTimeModifier TimeModifier { get; }
    public ActionTimeMode TimeMode { get; }
    public ActionEffectDefinition[] Effects { get; }
    public int? EstimatedDurationMinMinutes { get; }
    public int? EstimatedDurationMaxMinutes { get; }

    public ActionDefinition(
        string id,
        string label,
        int durationMinutes,
        ActivityTimeModifier timeModifier = null,
        string feedback = "",
        int energyCost = 0,
        ActionEffectDefinition[] effects = null,
        ActionTimeMode timeMode = ActionTimeMode.Duration,
        int? estimatedDurationMinMinutes = null,
        int? estimatedDurationMaxMinutes = null,
        bool canExecuteWhenUnconscious = false)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Action ID is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Action label is required.", nameof(label));
        if (durationMinutes < 0) throw new ArgumentOutOfRangeException(nameof(durationMinutes));
        if (energyCost < 0) throw new ArgumentOutOfRangeException(nameof(energyCost));
        if (!Enum.IsDefined(typeof(ActionTimeMode), timeMode)) throw new ArgumentOutOfRangeException(nameof(timeMode));
        if (estimatedDurationMinMinutes.HasValue != estimatedDurationMaxMinutes.HasValue)
        {
            throw new ArgumentException("Both estimated duration bounds must be provided together.");
        }
        if (estimatedDurationMinMinutes.HasValue &&
            (estimatedDurationMinMinutes.Value < 0 ||
             estimatedDurationMaxMinutes.Value < estimatedDurationMinMinutes.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(estimatedDurationMinMinutes));
        }
        if (timeMode == ActionTimeMode.NextDay &&
            (estimatedDurationMinMinutes.HasValue || estimatedDurationMaxMinutes.HasValue))
        {
            throw new ArgumentException("NextDay actions cannot define a duration estimate.");
        }

        Id = id;
        Label = label;
        DurationMinutes = durationMinutes;
        EnergyCost = energyCost;
        CanExecuteWhenUnconscious = canExecuteWhenUnconscious;
        TimeModifier = timeModifier;
        TimeMode = timeMode;
        Effects = effects ?? Array.Empty<ActionEffectDefinition>();
        Feedback = feedback ?? string.Empty;
        EstimatedDurationMinMinutes = estimatedDurationMinMinutes;
        EstimatedDurationMaxMinutes = estimatedDurationMaxMinutes;
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
    public LocationEventDefinition[] Events { get; }

    public LocationContent(
        string id,
        string name,
        string region,
        string description,
        string[] exits,
        ActionDefinition[] actions,
        LocationEventDefinition[] events = null)
    {
        Id = id;
        Name = name;
        Region = region;
        Description = description;
        Exits = exits ?? Array.Empty<string>();
        Actions = actions ?? Array.Empty<ActionDefinition>();
        Events = events ?? Array.Empty<LocationEventDefinition>();
    }
}

public static class AcademyContent
{
    private static Dictionary<string, LocationContent> _locations =
        new Dictionary<string, LocationContent>(StringComparer.Ordinal);

    public static IEnumerable<LocationContent> All => _locations.Values;
    public static ActionDefinition[] SelfActions { get; private set; } = Array.Empty<ActionDefinition>();
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
        LoadJson(json);
    }

    public static void LoadJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("Academy content JSON is required.", nameof(json));
        }

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
            ActionDefinition[] loadedSelfActions = root.TryGetProperty("selfActions", out JsonElement selfActionsData)
                ? ReadActions(selfActionsData)
                : Array.Empty<ActionDefinition>();

            JsonElement locations = root.GetProperty("locations");

            foreach (JsonElement locationData in locations.EnumerateArray())
            {
                string id = ReadString(locationData, "id");
                string name = ReadString(locationData, "name");
                string region = ReadString(locationData, "region");
                string description = ReadString(locationData, "description");
                string[] exits = ReadStringArray(locationData.GetProperty("exits"));
                ActionDefinition[] actions = ReadActions(locationData.GetProperty("actions"));
                LocationEventDefinition[] events = locationData.TryGetProperty("events", out JsonElement eventsData)
                    ? ReadEvents(eventsData)
                    : Array.Empty<LocationEventDefinition>();

                if (loadedLocations.ContainsKey(id))
                {
                    throw new InvalidOperationException("Duplicate location ID: " + id);
                }

                ValidateActionIds(id, actions);
                ValidateLocationEvents(id, actions, events);
                loadedLocations.Add(id, new LocationContent(id, name, region, description, exits, actions, events));
            }

            if (loadedLocations.Count == 0)
            {
                throw new InvalidOperationException("Academy content must contain at least one location.");
            }

            ValidateLocationExits(loadedLocations);
            ValidateSelfActionIds(loadedLocations, loadedSelfActions);

            _locations = loadedLocations;
            SelfActions = loadedSelfActions;
            Travel = loadedTravel;
            CombatEncounter = loadedCombatEncounter;
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Invalid academy content JSON.", exception);
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

    private static void ValidateSelfActionIds(
        Dictionary<string, LocationContent> locations,
        ActionDefinition[] selfActions)
    {
        ValidateActionIds("self", selfActions);

        HashSet<string> selfActionIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (ActionDefinition action in selfActions)
        {
            selfActionIds.Add(action.Id);
        }

        foreach (LocationContent location in locations.Values)
        {
            foreach (ActionDefinition action in location.Actions)
            {
                if (selfActionIds.Contains(action.Id))
                {
                    throw new InvalidOperationException(
                        "Self action ID '" + action.Id + "' conflicts with an action in location '" + location.Id + "'.");
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
                ActionDefinition action = ReadActionDefinition(choiceId, label, choiceData);
                choices.Add(new LocationEventChoiceDefinition(choiceId, action, encounterActionId));
            }

            events.Add(new LocationEventDefinition(
                id,
                title,
                region,
                description,
                chance,
                choices.ToArray()));
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
            actions.Add(ReadActionDefinition(
                ReadString(item, "id"),
                ReadString(item, "label"),
                item));
        }

        return actions.ToArray();
    }

    private static ActionDefinition ReadActionDefinition(string id, string label, JsonElement data)
    {
        int durationMinutes = data.GetProperty("durationMinutes").GetInt32();
        int energyCost = data.TryGetProperty("energyCost", out JsonElement energyCostData)
            ? energyCostData.GetInt32()
            : 0;
        ActivityTimeModifier modifier = data.TryGetProperty("timeModifier", out JsonElement modifierData)
            ? ReadTimeModifier(modifierData)
            : null;
        ActionEffectDefinition[] effects = data.TryGetProperty("effects", out JsonElement effectsData)
            ? ReadEffects(effectsData)
            : Array.Empty<ActionEffectDefinition>();
        ActionTimeMode timeMode = data.TryGetProperty("timeMode", out JsonElement timeModeData)
            ? ReadTimeMode(timeModeData)
            : ActionTimeMode.Duration;
        string feedback = data.TryGetProperty("feedback", out JsonElement feedbackData)
            ? feedbackData.GetString()
            : string.Empty;
        int? estimatedDurationMinMinutes = data.TryGetProperty("estimatedDurationMinMinutes", out JsonElement minDurationData)
            ? minDurationData.GetInt32()
            : null;
        int? estimatedDurationMaxMinutes = data.TryGetProperty("estimatedDurationMaxMinutes", out JsonElement maxDurationData)
            ? maxDurationData.GetInt32()
            : null;
        bool canExecuteWhenUnconscious = data.TryGetProperty("canExecuteWhenUnconscious", out JsonElement unconsciousData)
            && unconsciousData.GetBoolean();

        return new ActionDefinition(
            id,
            label,
            durationMinutes,
            modifier,
            feedback,
            energyCost,
            effects,
            timeMode,
            estimatedDurationMinMinutes,
            estimatedDurationMaxMinutes,
            canExecuteWhenUnconscious);
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

    private static ActionTimeMode ReadTimeMode(JsonElement data)
    {
        string modeName = data.GetString();

        if (!Enum.TryParse(modeName, true, out ActionTimeMode mode) ||
            !Enum.IsDefined(typeof(ActionTimeMode), mode))
        {
            throw new InvalidOperationException("Unknown action time mode: " + modeName);
        }

        return mode;
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
