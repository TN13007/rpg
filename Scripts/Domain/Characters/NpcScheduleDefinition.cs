using System;

public sealed class NpcScheduleEntryDefinition
{
    public int StartMinute { get; }
    public int EndMinute { get; }
    public string LocationId { get; }
    public string ActivityDescription { get; }

    public NpcScheduleEntryDefinition(
        int startMinute,
        int endMinute,
        string locationId,
        string activityDescription)
    {
        if (startMinute < 0 || startMinute >= 1440)
        {
            throw new ArgumentOutOfRangeException(nameof(startMinute));
        }

        if (endMinute <= 0 || endMinute > 1440 || endMinute <= startMinute)
        {
            throw new ArgumentOutOfRangeException(nameof(endMinute));
        }

        if (string.IsNullOrWhiteSpace(locationId))
        {
            throw new ArgumentException("NPC schedule Location ID is required.", nameof(locationId));
        }

        if (string.IsNullOrWhiteSpace(activityDescription))
        {
            throw new ArgumentException("NPC schedule activity description is required.", nameof(activityDescription));
        }

        StartMinute = startMinute;
        EndMinute = endMinute;
        LocationId = locationId;
        ActivityDescription = activityDescription;
    }

    public bool Contains(int minuteOfDay)
    {
        return minuteOfDay >= StartMinute && minuteOfDay < EndMinute;
    }
}

public sealed class NpcScheduleDefinition
{
    private readonly NpcScheduleEntryDefinition[] _entries;

    public string CharacterId { get; }
    public NpcScheduleEntryDefinition[] Entries => (NpcScheduleEntryDefinition[])_entries.Clone();

    public NpcScheduleDefinition(string characterId, NpcScheduleEntryDefinition[] entries)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            throw new ArgumentException("NPC schedule Character ID is required.", nameof(characterId));
        }

        if (entries == null || entries.Length == 0)
        {
            throw new ArgumentException("An NPC schedule must contain at least one entry.", nameof(entries));
        }

        _entries = (NpcScheduleEntryDefinition[])entries.Clone();
        foreach (NpcScheduleEntryDefinition entry in _entries)
        {
            if (entry == null)
            {
                throw new ArgumentException("NPC schedule entries cannot contain null values.", nameof(entries));
            }
        }

        Array.Sort(_entries, (left, right) => left.StartMinute.CompareTo(right.StartMinute));

        int previousEndMinute = -1;
        foreach (NpcScheduleEntryDefinition entry in _entries)
        {
            if (entry.StartMinute < previousEndMinute)
            {
                throw new ArgumentException(
                    "NPC schedule entries cannot overlap for Character '" + characterId + "'.",
                    nameof(entries));
            }

            previousEndMinute = entry.EndMinute;
        }

        CharacterId = characterId;
    }

    public NpcScheduleEntryDefinition GetEntryAt(int minuteOfDay)
    {
        if (minuteOfDay < 0 || minuteOfDay >= 1440)
        {
            throw new ArgumentOutOfRangeException(nameof(minuteOfDay));
        }

        foreach (NpcScheduleEntryDefinition entry in _entries)
        {
            if (entry.Contains(minuteOfDay))
            {
                return entry;
            }

            if (entry.StartMinute > minuteOfDay)
            {
                break;
            }
        }

        return null;
    }
}
