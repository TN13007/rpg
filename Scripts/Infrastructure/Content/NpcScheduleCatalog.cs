using System;
using System.Collections.Generic;

/// <summary>Provides indexed access to configured npc schedule definitions.</summary>
public sealed class NpcScheduleCatalog
{
    private readonly Dictionary<string, NpcScheduleDefinition> _schedules;

    /// <summary>Gets the all.</summary>
    public IEnumerable<NpcScheduleDefinition> All => _schedules.Values;

    /// <summary>Initializes a new instance of <see cref="NpcScheduleCatalog"/>.</summary>
    /// <param name="schedules">The schedules used to initialize the instance.</param>
    public NpcScheduleCatalog(IEnumerable<NpcScheduleDefinition> schedules)
    {
        if (schedules == null)
        {
            throw new ArgumentNullException(nameof(schedules));
        }

        _schedules = new Dictionary<string, NpcScheduleDefinition>(StringComparer.Ordinal);
        foreach (NpcScheduleDefinition schedule in schedules)
        {
            if (schedule == null)
            {
                throw new InvalidOperationException("NPC schedule definitions cannot contain null values.");
            }

            if (!_schedules.TryAdd(schedule.CharacterId, schedule))
            {
                throw new InvalidOperationException(
                    "Duplicate NPC schedule for Character ID: " + schedule.CharacterId);
            }
        }
    }

    /// <summary>Attempts to get and reports whether the operation succeeded.</summary>
    /// <param name="characterId">The character id used by this operation.</param>
    /// <param name="schedule">The schedule used by this operation.</param>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
    public bool TryGet(string characterId, out NpcScheduleDefinition schedule)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            throw new ArgumentException("NPC schedule Character ID is required.", nameof(characterId));
        }

        return _schedules.TryGetValue(characterId, out schedule);
    }

    /// <summary>Returns get.</summary>
    /// <param name="characterId">The character id used by this operation.</param>
    /// <returns>The resulting npc schedule definition.</returns>
    public NpcScheduleDefinition Get(string characterId)
    {
        if (!TryGet(characterId, out NpcScheduleDefinition schedule))
        {
            throw new KeyNotFoundException("NPC schedule was not found for Character ID: " + characterId);
        }

        return schedule;
    }
}
