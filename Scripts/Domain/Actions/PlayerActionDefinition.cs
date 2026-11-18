using System;

public sealed class PlayerActionDefinition
{
    public ActionDefinition Action { get; }
    public string[] AvailableLocationIds { get; }

    public PlayerActionDefinition(ActionDefinition action, string[] availableLocationIds = null)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
        AvailableLocationIds = availableLocationIds ?? Array.Empty<string>();
        foreach (string locationId in AvailableLocationIds)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                throw new ArgumentException("Player action location IDs cannot be empty.", nameof(availableLocationIds));
            }
        }
    }

    public bool IsAvailableAt(string locationId)
    {
        if (AvailableLocationIds.Length == 0) return true;
        foreach (string availableLocationId in AvailableLocationIds)
        {
            if (string.Equals(availableLocationId, locationId, StringComparison.Ordinal)) return true;
        }
        return false;
    }
}
