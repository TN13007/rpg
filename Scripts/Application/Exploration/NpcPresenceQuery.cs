using System;
using System.Collections.Generic;

/// <summary>Provides npc presence functionality for the application.</summary>
public sealed class NpcPresence
{
    /// <summary>Gets the character id.</summary>
    public string CharacterId { get; }
    /// <summary>Gets the name.</summary>
    public string Name { get; }
    /// <summary>Gets the location id.</summary>
    public string LocationId { get; }
    /// <summary>Gets the activity description.</summary>
    public string ActivityDescription { get; }

    /// <summary>Initializes a new instance of <see cref="NpcPresence"/>.</summary>
    /// <param name="characterId">The character id used to initialize the instance.</param>
    /// <param name="name">The name used to initialize the instance.</param>
    /// <param name="locationId">The location id used to initialize the instance.</param>
    /// <param name="activityDescription">The activity description used to initialize the instance.</param>
    public NpcPresence(
        string characterId,
        string name,
        string locationId,
        string activityDescription)
    {
        CharacterId = characterId ?? string.Empty;
        Name = name ?? string.Empty;
        LocationId = locationId ?? string.Empty;
        ActivityDescription = activityDescription ?? string.Empty;
    }
}

/// <summary>Retrieves npc presence information from the current game state.</summary>
public sealed class NpcPresenceQuery
{
    private readonly NpcScheduleDefinition[] _schedules;
    private readonly Dictionary<string, CharacterData> _characters;

    /// <summary>Initializes a new instance of <see cref="NpcPresenceQuery"/>.</summary>
    /// <param name="schedules">The schedules used to initialize the instance.</param>
    /// <param name="characters">The characters used to initialize the instance.</param>
    public NpcPresenceQuery(
        IEnumerable<NpcScheduleDefinition> schedules,
        IEnumerable<CharacterData> characters)
    {
        if (schedules == null)
        {
            throw new ArgumentNullException(nameof(schedules));
        }

        if (characters == null)
        {
            throw new ArgumentNullException(nameof(characters));
        }

        _characters = new Dictionary<string, CharacterData>(StringComparer.Ordinal);
        foreach (CharacterData character in characters)
        {
            if (character == null)
            {
                throw new InvalidOperationException("Character definitions cannot contain null values.");
            }

            if (!_characters.TryAdd(character.Id, character))
            {
                throw new InvalidOperationException("Duplicate Character ID: " + character.Id);
            }
        }

        List<NpcScheduleDefinition> scheduleList = new List<NpcScheduleDefinition>();
        HashSet<string> scheduledCharacterIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (NpcScheduleDefinition schedule in schedules)
        {
            if (schedule == null)
            {
                throw new InvalidOperationException("NPC schedule definitions cannot contain null values.");
            }

            if (!_characters.ContainsKey(schedule.CharacterId))
            {
                throw new InvalidOperationException(
                    "NPC schedule references unknown Character ID: " + schedule.CharacterId);
            }

            if (!scheduledCharacterIds.Add(schedule.CharacterId))
            {
                throw new InvalidOperationException(
                    "Duplicate NPC schedule for Character ID: " + schedule.CharacterId);
            }

            scheduleList.Add(schedule);
        }

        _schedules = scheduleList.ToArray();
    }

    /// <summary>Returns the interactions or presence records matching the supplied criteria.</summary>
    /// <param name="locationId">The location id used by this operation.</param>
    /// <param name="minuteOfDay">The minute of day used by this operation.</param>
    /// <returns>The resulting i read only list npc presence.</returns>
    public IReadOnlyList<NpcPresence> Query(string locationId, int minuteOfDay)
    {
        if (string.IsNullOrWhiteSpace(locationId))
        {
            throw new ArgumentException("Current Location ID is required.", nameof(locationId));
        }

        if (minuteOfDay < 0 || minuteOfDay >= 1440)
        {
            throw new ArgumentOutOfRangeException(nameof(minuteOfDay));
        }

        List<NpcPresence> presentNpcs = new List<NpcPresence>();
        foreach (NpcScheduleDefinition schedule in _schedules)
        {
            NpcScheduleEntryDefinition entry = schedule.GetEntryAt(minuteOfDay);
            if (entry == null || !string.Equals(entry.LocationId, locationId, StringComparison.Ordinal))
            {
                continue;
            }

            CharacterData character = _characters[schedule.CharacterId];
            presentNpcs.Add(new NpcPresence(
                character.Id,
                character.Name,
                entry.LocationId,
                entry.ActivityDescription));
        }

        presentNpcs.Sort((left, right) => StringComparer.Ordinal.Compare(left.Name, right.Name));
        return presentNpcs;
    }

    /// <summary>Determines whether present.</summary>
    /// <param name="characterId">The character id used by this operation.</param>
    /// <param name="locationId">The location id used by this operation.</param>
    /// <param name="minuteOfDay">The minute of day used by this operation.</param>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
    public bool IsPresent(string characterId, string locationId, int minuteOfDay)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            throw new ArgumentException("Character ID is required.", nameof(characterId));
        }

        IReadOnlyList<NpcPresence> presentNpcs = Query(locationId, minuteOfDay);
        foreach (NpcPresence presence in presentNpcs)
        {
            if (string.Equals(presence.CharacterId, characterId, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Returns present character ids.</summary>
    /// <param name="locationId">The location id used by this operation.</param>
    /// <param name="minuteOfDay">The minute of day used by this operation.</param>
    /// <returns>The resulting hash set string.</returns>
    public HashSet<string> GetPresentCharacterIds(string locationId, int minuteOfDay)
    {
        IReadOnlyList<NpcPresence> presentNpcs = Query(locationId, minuteOfDay);
        HashSet<string> characterIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (NpcPresence presence in presentNpcs)
        {
            characterIds.Add(presence.CharacterId);
        }

        return characterIds;
    }
}
