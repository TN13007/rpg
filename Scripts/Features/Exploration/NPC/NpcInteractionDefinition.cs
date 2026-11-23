using System;
using System.Collections.Generic;

/// <summary>Defines the npc interaction data and its validation rules.</summary>
public sealed class NpcInteractionDefinition
{
    private readonly string[] _availableLocationIds;

    /// <summary>Gets the character id.</summary>
    public string CharacterId { get; }
    /// <summary>Gets the action.</summary>
    public ActionDefinition Action { get; }
    /// <summary>Gets the available location ids.</summary>
    public string[] AvailableLocationIds => (string[])_availableLocationIds.Clone();

    /// <summary>Initializes a new instance of <see cref="NpcInteractionDefinition"/>.</summary>
    /// <param name="characterId">The character id used to initialize the instance.</param>
    /// <param name="action">The action used to initialize the instance.</param>
    /// <param name="availableLocationIds">The available location ids used to initialize the instance.</param>
    public NpcInteractionDefinition(
        string characterId,
        ActionDefinition action,
        string[] availableLocationIds = null)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            throw new ArgumentException("NPC interaction Character ID is required.", nameof(characterId));
        }

        Action = action ?? throw new ArgumentNullException(nameof(action));
        if (string.Equals(Action.Id, "select", StringComparison.Ordinal))
        {
            throw new ArgumentException("'select' is reserved for NPC menu navigation.", nameof(action));
        }

        _availableLocationIds = availableLocationIds == null
            ? Array.Empty<string>()
            : (string[])availableLocationIds.Clone();

        HashSet<string> locationIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (string locationId in _availableLocationIds)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                throw new ArgumentException("NPC interaction Location IDs cannot be empty.", nameof(availableLocationIds));
            }

            if (!locationIds.Add(locationId))
            {
                throw new ArgumentException(
                    "Duplicate NPC interaction Location ID: " + locationId,
                    nameof(availableLocationIds));
            }
        }

        CharacterId = characterId;
    }

    /// <summary>Determines whether available at.</summary>
    /// <param name="locationId">The location id used by this operation.</param>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
    public bool IsAvailableAt(string locationId)
    {
        if (string.IsNullOrWhiteSpace(locationId))
        {
            throw new ArgumentException("Current Location ID is required.", nameof(locationId));
        }

        if (_availableLocationIds.Length == 0)
        {
            return true;
        }

        foreach (string availableLocationId in _availableLocationIds)
        {
            if (string.Equals(availableLocationId, locationId, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
