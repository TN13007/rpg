using System;

/// <summary>Defines the player action data and its validation rules.</summary>
public sealed class PlayerActionDefinition
{
    /// <summary>Gets the action.</summary>
    public ActionDefinition Action { get; }
    /// <summary>Gets the available location ids.</summary>
    public string[] AvailableLocationIds { get; }

    /// <summary>Initializes a new instance of <see cref="PlayerActionDefinition"/>.</summary>
    /// <param name="action">The action used to initialize the instance.</param>
    /// <param name="availableLocationIds">The available location ids used to initialize the instance.</param>
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

    /// <summary>Determines whether available at.</summary>
    /// <param name="locationId">The location id used by this operation.</param>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
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
