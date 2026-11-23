using System;
using System.Collections.Generic;

/// <summary>Provides indexed access to already-loaded Location definitions.</summary>
public sealed class LocationCatalog
{
    private readonly Dictionary<string, LocationContent> _locations =
        new Dictionary<string, LocationContent>(StringComparer.Ordinal);

    /// <summary>Initializes an empty Location catalog for contexts that do not require authored locations.</summary>
    public LocationCatalog()
    {
    }

    /// <summary>Initializes a catalog from validated Location content.</summary>
    /// <param name="locations">The validated Location definitions.</param>
    /// <param name="travel">The shared Travel action.</param>
    /// <param name="combatEncounter">The shared Combat Encounter action.</param>
    public LocationCatalog(
        IEnumerable<LocationContent> locations,
        ActionDefinition travel,
        ActionDefinition combatEncounter)
    {
        if (locations == null) throw new ArgumentNullException(nameof(locations));
        Travel = travel ?? throw new ArgumentNullException(nameof(travel));
        CombatEncounter = combatEncounter ?? throw new ArgumentNullException(nameof(combatEncounter));

        foreach (LocationContent location in locations)
        {
            if (location == null) throw new ArgumentException("Location definitions cannot contain null values.", nameof(locations));
            _locations.Add(location.Id, location);
        }
    }

    /// <summary>Gets all loaded locations.</summary>
    public IEnumerable<LocationContent> All => _locations.Values;

    /// <summary>Gets the shared Travel action.</summary>
    public ActionDefinition Travel { get; } = new ActionDefinition("travel", "Travel", 10);

    /// <summary>Gets the shared Combat Encounter action.</summary>
    public ActionDefinition CombatEncounter { get; } =
        new ActionDefinition("combat_encounter", "Combat Encounter", 30);

    /// <summary>Returns a location by its stable ID.</summary>
    /// <param name="id">The location ID.</param>
    /// <returns>The matching location.</returns>
    public LocationContent Get(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Location ID is required.", nameof(id));
        }

        if (!_locations.TryGetValue(id, out LocationContent location))
        {
            throw new KeyNotFoundException("Location was not found: " + id);
        }

        return location;
    }
}
