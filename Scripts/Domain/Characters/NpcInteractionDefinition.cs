using System;
using System.Collections.Generic;

public sealed class NpcInteractionDefinition
{
    private readonly string[] _availableLocationIds;

    public string CharacterId { get; }
    public ActionDefinition Action { get; }
    public string[] AvailableLocationIds => (string[])_availableLocationIds.Clone();

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
