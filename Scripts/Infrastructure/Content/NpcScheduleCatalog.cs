using System;
using System.Collections.Generic;

public sealed class NpcScheduleCatalog
{
    private readonly Dictionary<string, NpcScheduleDefinition> _schedules;

    public IEnumerable<NpcScheduleDefinition> All => _schedules.Values;

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

    public bool TryGet(string characterId, out NpcScheduleDefinition schedule)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            throw new ArgumentException("NPC schedule Character ID is required.", nameof(characterId));
        }

        return _schedules.TryGetValue(characterId, out schedule);
    }

    public NpcScheduleDefinition Get(string characterId)
    {
        if (!TryGet(characterId, out NpcScheduleDefinition schedule))
        {
            throw new KeyNotFoundException("NPC schedule was not found for Character ID: " + characterId);
        }

        return schedule;
    }
}
