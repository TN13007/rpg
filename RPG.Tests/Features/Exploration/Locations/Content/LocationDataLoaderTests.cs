using System;
using Xunit;

/// <summary>Verifies Location JSON parsing and authored-content validation.</summary>
public sealed class LocationDataLoaderTests
{
    [Fact]
    public void LoadJsonRejectsEmptyInput()
    {
        Assert.Throws<ArgumentException>(() => LocationDataLoader.LoadJson(null));
        Assert.Throws<ArgumentException>(() => LocationDataLoader.LoadJson(string.Empty));
        Assert.Throws<ArgumentException>(() => LocationDataLoader.LoadJson("  "));
    }

    [Fact]
    public void LoadRejectsEmptyPath()
    {
        Assert.Throws<ArgumentException>(() => LocationDataLoader.Load(null));
        Assert.Throws<ArgumentException>(() => LocationDataLoader.Load(string.Empty));
    }

    [Fact]
    public void LoadJsonRejectsMalformedJson()
    {
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => LocationDataLoader.LoadJson("{"));
        Assert.Equal("Invalid location content JSON.", exception.Message);
    }

    [Fact]
    public void LoadJsonRejectsDuplicateLocationIds()
    {
        string json = CreateLocations(
            CreateLocation("same", "[]", "[]"),
            CreateLocation("same", "[]", "[]"));

        Assert.Equal(
            "Duplicate location ID: same",
            Assert.Throws<InvalidOperationException>(() => LocationDataLoader.LoadJson(json)).Message);
    }

    [Fact]
    public void LoadJsonRejectsUnknownLocationExits()
    {
        string json = CreateLocations(CreateLocation("first", "[\"missing\"]", "[]"));

        Assert.Equal(
            "Location 'first' references unknown exit 'missing'.",
            Assert.Throws<InvalidOperationException>(() => LocationDataLoader.LoadJson(json)).Message);
    }

    [Fact]
    public void LoadJsonRejectsDuplicateLocationExits()
    {
        string json = CreateLocations(
            CreateLocation("first", "[\"second\",\"second\"]", "[]"),
            CreateLocation("second", "[]", "[]"));

        Assert.Equal(
            "Duplicate exit 'second' in location 'first'.",
            Assert.Throws<InvalidOperationException>(() => LocationDataLoader.LoadJson(json)).Message);
    }

    [Fact]
    public void LoadJsonRejectsDuplicateActionIdsWithinLocation()
    {
        string actions = "[{\"id\":\"rest\",\"label\":\"Rest\",\"durationMinutes\":1},"
            + "{\"id\":\"rest\",\"label\":\"Rest again\",\"durationMinutes\":1}]";
        string json = CreateLocations(CreateLocation("first", "[]", actions));

        Assert.Contains(
            "Duplicate action ID 'rest' in location 'first'.",
            Assert.Throws<InvalidOperationException>(() => LocationDataLoader.LoadJson(json)).Message);
    }

    [Fact]
    public void LoadJsonRejectsEmptyLocationCollection()
    {
        Assert.Throws<InvalidOperationException>(() => LocationDataLoader.LoadJson("{\"locations\":[]}"));
    }

    [Fact]
    public void LoadJsonReturnsIndexedCatalogAndDefaultSharedActions()
    {
        string json = CreateLocations(CreateLocation("first", "[]", "[]"));
        LocationCatalog catalog = LocationDataLoader.LoadJson(json);

        Assert.Equal("first", catalog.Get("first").Id);
        Assert.Equal("travel", catalog.Travel.Id);
        Assert.Equal("combat_encounter", catalog.CombatEncounter.Id);
    }

    private static string CreateLocations(params string[] locations)
    {
        return "{\"locations\":[" + string.Join(",", locations) + "]}";
    }

    private static string CreateLocation(string id, string exits, string actions)
    {
        return "{\"id\":\"" + id + "\",\"name\":\"Location\","
            + "\"region\":\"REGION\",\"description\":\"Description\","
            + "\"exits\":" + exits + ",\"actions\":" + actions + "}";
    }
}
