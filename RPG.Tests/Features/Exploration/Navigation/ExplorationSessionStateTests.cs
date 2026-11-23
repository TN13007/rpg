using System;
using Xunit;

public sealed class ExplorationSessionStateTests
{
    [Fact]
    public void Constructor_SetsTheStartingLocation()
    {
        ExplorationSessionState session = new ExplorationSessionState("campus");

        Assert.Equal("campus", session.CurrentLocationId);
    }

    [Fact]
    public void Constructor_RejectsAnEmptyStartingLocation()
    {
        Assert.Throws<ArgumentException>(() => new ExplorationSessionState(" "));
    }

    [Fact]
    public void SetCurrentLocation_ChangesTheCurrentLocation()
    {
        ExplorationSessionState session = new ExplorationSessionState("campus");

        session.SetCurrentLocation("library");

        Assert.Equal("library", session.CurrentLocationId);
    }

    [Fact]
    public void SetCurrentLocation_RejectsAnEmptyLocation()
    {
        ExplorationSessionState session = new ExplorationSessionState("campus");

        Assert.Throws<ArgumentException>(() => session.SetCurrentLocation(string.Empty));

        Assert.Equal("campus", session.CurrentLocationId);
    }

    [Fact]
    public void GetCurrentLocationDescription_UsesFirstVisitDescription()
    {
        ExplorationSessionState session = new ExplorationSessionState("campus");
        LocationContent location = CreateLocation("campus", "First visit.", "Return visit.");

        Assert.Equal("First visit.", session.GetCurrentLocationDescription(location));
    }

    [Fact]
    public void GetCurrentLocationDescription_RepeatedReadsKeepTheSameVisitDescription()
    {
        ExplorationSessionState session = new ExplorationSessionState("campus");
        LocationContent location = CreateLocation("campus", "First visit.", "Return visit.");

        Assert.Equal("First visit.", session.GetCurrentLocationDescription(location));
        Assert.Equal("First visit.", session.GetCurrentLocationDescription(location));
    }

    [Fact]
    public void GetCurrentLocationDescription_UsesReturnDescriptionAfterLeavingAndReturning()
    {
        ExplorationSessionState session = new ExplorationSessionState("campus");
        LocationContent campus = CreateLocation("campus", "Campus first visit.", "Campus return visit.");
        LocationContent library = CreateLocation("library", "Library first visit.", "Library return visit.");

        Assert.Equal("Campus first visit.", session.GetCurrentLocationDescription(campus));

        session.SetCurrentLocation("library");
        Assert.Equal("Library first visit.", session.GetCurrentLocationDescription(library));

        session.SetCurrentLocation("campus");
        Assert.Equal("Campus return visit.", session.GetCurrentLocationDescription(campus));
    }

    [Fact]
    public void GetCurrentLocationDescription_RejectsAStaleLocationDefinition()
    {
        ExplorationSessionState session = new ExplorationSessionState("campus");
        LocationContent library = CreateLocation("library", "Library first visit.", "Library return visit.");

        Assert.Throws<InvalidOperationException>(
            () => session.GetCurrentLocationDescription(library));
    }

    private static LocationContent CreateLocation(
        string id,
        string firstVisitDescription,
        string returnVisitDescription)
    {
        return new LocationContent(
            id,
            id,
            "TEST",
            firstVisitDescription,
            returnVisitDescription,
            Array.Empty<string>(),
            Array.Empty<ActionDefinition>());
    }
}
