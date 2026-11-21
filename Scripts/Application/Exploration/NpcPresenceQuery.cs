using System;
using System.Collections.Generic;

public sealed class NpcPresence
{
    public string CharacterId { get; }
    public string Name { get; }
    public string LocationId { get; }
    public string ActivityDescription { get; }

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

public sealed class NpcPresenceQuery
{
    private readonly NpcScheduleDefinition[] _schedules;
    private readonly Dictionary<string, CharacterData> _characters;

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
