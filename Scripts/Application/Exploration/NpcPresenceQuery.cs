using System;
using System.Collections.Generic;

/// <summary>Describes one Character's scheduled presence at a Location for presentation and interaction discovery.</summary>
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

    /// <summary>Creates a presence record for one Character at a Location.</summary>
    /// <param name="characterId">The stable ID of the Character who is present.</param>
    /// <param name="name">The Character's display name.</param>
    /// <param name="locationId">The ID of the Location where the Character is present.</param>
    /// <param name="activityDescription">The authored description of what the Character is doing during this schedule interval.</param>
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

    /// <summary>Creates a read-only NPC presence query from validated schedule and Character definitions.</summary>
    /// <param name="schedules">The daily schedules that map Character IDs to Location/time intervals.</param>
    /// <param name="characters">The Character definitions used to resolve names and validate schedule references.</param>
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

    /// <summary>Finds Characters scheduled at the specified Location and minute of the in-game day.</summary>
    /// <param name="locationId">The current Location ID to match against each schedule entry.</param>
    /// <param name="minuteOfDay">The minute within the day, from 0 inclusive to 1440 exclusive.</param>
    /// <returns>Present NPC records sorted by Character name; the list is empty when no schedule matches.</returns>
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

    /// <summary>Checks whether a Character is scheduled at the specified Location and time.</summary>
    /// <param name="characterId">The Character ID whose presence should be checked.</param>
    /// <param name="locationId">The location id used by this operation.</param>
    /// <param name="minuteOfDay">The minute of day used by this operation.</param>
    /// <returns><see langword="true"/> when the Character appears in the matching presence results; otherwise, <see langword="false"/>.</returns>
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

    /// <summary>Gets the IDs of all Characters scheduled at the specified Location and time.</summary>
    /// <param name="locationId">The location id used by this operation.</param>
    /// <param name="minuteOfDay">The minute of day used by this operation.</param>
    /// <returns>A new set of present Character IDs using ordinal ID comparison.</returns>
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
