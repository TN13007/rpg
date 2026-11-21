using System;
using System.Collections.Generic;

public sealed class LocationEventService
{
    private readonly Dictionary<string, int> _lastAttemptedDayByLocation;
    private readonly Random _random;

    public LocationEventService()
        : this(new Random())
    {
    }

    public LocationEventService(Random random)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _lastAttemptedDayByLocation = new Dictionary<string, int>(StringComparer.Ordinal);
    }

    public bool TryTrigger(
        LocationContent location,
        int day,
        bool canAct,
        out LocationEventDefinition triggeredEvent,
        ISet<string> presentNpcIds = null)
    {
        if (location == null)
        {
            throw new ArgumentNullException(nameof(location));
        }

        if (string.IsNullOrWhiteSpace(location.Id))
        {
            throw new ArgumentException("Location ID is required.", nameof(location));
        }

        if (day < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(day));
        }

        triggeredEvent = null;

        if (!canAct || location.Events.Length == 0)
        {
            return false;
        }

        if (_lastAttemptedDayByLocation.TryGetValue(location.Id, out int lastAttemptedDay) &&
            lastAttemptedDay == day)
        {
            return false;
        }

        List<LocationEventDefinition> eligibleEvents = new List<LocationEventDefinition>();
        foreach (LocationEventDefinition locationEvent in location.Events)
        {
            if (locationEvent == null)
            {
                throw new ArgumentException("Location events cannot contain null values.", nameof(location));
            }

            if (!string.IsNullOrWhiteSpace(locationEvent.RequiredNpcId) &&
                (presentNpcIds == null || !presentNpcIds.Contains(locationEvent.RequiredNpcId)))
            {
                continue;
            }

            eligibleEvents.Add(locationEvent);
        }

        if (eligibleEvents.Count == 0)
        {
            return false;
        }

        _lastAttemptedDayByLocation[location.Id] = day;

        foreach (LocationEventDefinition locationEvent in eligibleEvents)
        {
            if (locationEvent.Chance <= 0)
            {
                continue;
            }

            if (locationEvent.Chance >= 1 || _random.NextDouble() < locationEvent.Chance)
            {
                triggeredEvent = locationEvent;
                return true;
            }
        }

        return false;
    }
}
