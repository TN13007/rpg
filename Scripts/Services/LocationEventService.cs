using System;
using System.Collections.Generic;

public sealed class LocationEventService
{
    private readonly HashSet<string> _attemptedEvents;
    private readonly Random _random;

    public LocationEventService()
        : this(new Random())
    {
    }

    public LocationEventService(Random random)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _attemptedEvents = new HashSet<string>(StringComparer.Ordinal);
    }

    public bool TryTrigger(string eventId, int day, double chance)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            throw new ArgumentException("Event ID is required.", nameof(eventId));
        }

        if (day < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(day));
        }

        if (chance < 0 || chance > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(chance));
        }

        string attemptKey = eventId + ":" + day;

        if (!_attemptedEvents.Add(attemptKey))
        {
            return false;
        }

        return _random.NextDouble() < chance;
    }
}
