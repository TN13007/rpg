using System;
using System.Collections.Generic;

/// <summary>Coordinates location event behavior and applies its domain rules.</summary>
public sealed class LocationEventService
{
    private readonly Random _random;

    /// <summary>Initializes a new instance of <see cref="LocationEventService"/>.</summary>
    public LocationEventService()
        : this(new Random())
    {
    }

    /// <summary>Initializes a new instance of <see cref="LocationEventService"/>.</summary>
    /// <param name="random">The random used to initialize the instance.</param>
    public LocationEventService(Random random)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
    }

    /// <summary>Attempts to trigger and reports whether the operation succeeded.</summary>
    /// <param name="location">The location used by this operation.</param>
    /// <param name="canAct">The can act used by this operation.</param>
    /// <param name="triggeredEvent">The triggered event used by this operation.</param>
    /// <param name="presentNpcIds">The present npc ids used by this operation.</param>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
    public bool TryTrigger(
        LocationContent location,
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

        triggeredEvent = null;

        if (!canAct || location.Events.Length == 0)
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
