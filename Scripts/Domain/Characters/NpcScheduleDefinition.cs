using System;

/// <summary>Defines the npc schedule entry data and its validation rules.</summary>
public sealed class NpcScheduleEntryDefinition
{
    /// <summary>Gets the start minute.</summary>
    public int StartMinute { get; }
    /// <summary>Gets the end minute.</summary>
    public int EndMinute { get; }
    /// <summary>Gets the location id.</summary>
    public string LocationId { get; }
    /// <summary>Gets the activity description.</summary>
    public string ActivityDescription { get; }

    /// <summary>Initializes a new instance of <see cref="NpcScheduleEntryDefinition"/>.</summary>
    /// <param name="startMinute">The start minute used to initialize the instance.</param>
    /// <param name="endMinute">The end minute used to initialize the instance.</param>
    /// <param name="locationId">The location id used to initialize the instance.</param>
    /// <param name="activityDescription">The activity description used to initialize the instance.</param>
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

    /// <summary>Performs the contains operation for npc schedule definition.</summary>
    /// <param name="minuteOfDay">The minute of day used by this operation.</param>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
    public bool Contains(int minuteOfDay)
    {
        return minuteOfDay >= StartMinute && minuteOfDay < EndMinute;
    }
}

/// <summary>Defines the npc schedule data and its validation rules.</summary>
public sealed class NpcScheduleDefinition
{
    private readonly NpcScheduleEntryDefinition[] _entries;

    /// <summary>Gets the character id.</summary>
    public string CharacterId { get; }
    /// <summary>Gets the entries.</summary>
    public NpcScheduleEntryDefinition[] Entries => (NpcScheduleEntryDefinition[])_entries.Clone();

    /// <summary>Initializes a new instance of <see cref="NpcScheduleDefinition"/>.</summary>
    /// <param name="characterId">The character id used to initialize the instance.</param>
    /// <param name="entries">The entries used to initialize the instance.</param>
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

    /// <summary>Returns entry at.</summary>
    /// <param name="minuteOfDay">The minute of day used by this operation.</param>
    /// <returns>The resulting npc schedule entry definition.</returns>
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
