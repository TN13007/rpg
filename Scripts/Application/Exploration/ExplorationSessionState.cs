using System;
using System.Collections.Generic;

/// <summary>Owns the transient Location state for one Exploration session.</summary>
/// <remarks>
/// This state belongs to the Application layer so Presentation can render and coordinate
/// Exploration without owning the current world Location or visit history.
/// </remarks>
public sealed class ExplorationSessionState
{
    private readonly HashSet<string> _visitedLocationIds =
        new HashSet<string>(StringComparer.Ordinal);

    private string _descriptionLocationId;
    private string _currentLocationDescription;

    /// <summary>Gets the Location currently occupied by the Player.</summary>
    public string CurrentLocationId { get; private set; }

    /// <summary>Creates an Exploration session at the supplied starting Location.</summary>
    /// <param name="startingLocationId">The validated starting Location ID.</param>
    public ExplorationSessionState(string startingLocationId)
    {
        ValidateLocationId(startingLocationId, nameof(startingLocationId));
        CurrentLocationId = startingLocationId;
    }

    /// <summary>Updates the current Location after the caller validates the travel request.</summary>
    /// <param name="locationId">The destination Location ID.</param>
    public void SetCurrentLocation(string locationId)
    {
        ValidateLocationId(locationId, nameof(locationId));
        CurrentLocationId = locationId;
    }

    /// <summary>Returns a stable first-visit or return-visit description for the current Location.</summary>
    /// <param name="location">The current Location definition.</param>
    /// <returns>The description selected for this visit.</returns>
    public string GetCurrentLocationDescription(LocationContent location)
    {
        if (location == null)
        {
            throw new ArgumentNullException(nameof(location));
        }

        if (!string.Equals(location.Id, CurrentLocationId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Cannot display Location '" + location.Id
                + "' while the current Location is '" + CurrentLocationId + "'.");
        }

        if (!string.Equals(_descriptionLocationId, location.Id, StringComparison.Ordinal))
        {
            bool isFirstVisit = _visitedLocationIds.Add(location.Id);
            _descriptionLocationId = location.Id;
            _currentLocationDescription = location.GetDescription(isFirstVisit);
        }

        return _currentLocationDescription;
    }

    private static void ValidateLocationId(string locationId, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(locationId))
        {
            throw new ArgumentException("Location ID is required.", parameterName);
        }
    }
}
