using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

public sealed class NpcInteractionTests
{
    [Fact]
    public void DataLoader_ReadsNpcInteractionsAndLocationRestrictions()
    {
        string json = File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory, "Data", "NPC", "NpcInteractions.json"));
        NpcInteractionCatalog catalog = NpcInteractionDataLoader.LoadJson(json);
        IReadOnlyList<NpcInteractionDefinition> interactions = catalog.GetForCharacter("mira");

        Assert.Equal(2, interactions.Count);
        Assert.True(interactions[0].IsAvailableAt("campus_cafe"));
        Assert.True(interactions[0].IsAvailableAt("campus_library"));
        Assert.True(interactions[1].IsAvailableAt("campus_cafe"));
        Assert.False(interactions[1].IsAvailableAt("campus_library"));
    }

    [Fact]
    public void Query_ReturnsOnlyInteractionsForSelectedNpcAndCurrentLocation()
    {
        string json = File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory, "Data", "NPC", "NpcInteractions.json"));
        NpcInteractionCatalog catalog = NpcInteractionDataLoader.LoadJson(json);
        NpcInteractionQuery query = new NpcInteractionQuery(catalog);

        IReadOnlyList<AvailableInteraction> atCanteen = query.Query("mira", "campus_cafe");
        IReadOnlyList<AvailableInteraction> atLibrary = query.Query("mira", "campus_library");
        IReadOnlyList<AvailableInteraction> forUnknownNpc = query.Query("unknown", "campus_cafe");

        Assert.Equal(2, atCanteen.Count);
        Assert.Contains(atCanteen, item => item.ActionId == "talk");
        Assert.Contains(atCanteen, item => item.ActionId == "join_mira_for_lunch");
        Assert.Single(atLibrary);
        Assert.Contains(atLibrary, item => item.ActionId == "talk");
        Assert.Empty(forUnknownNpc);
        Assert.All(atCanteen, item => Assert.Equal(InteractionSourceKind.Npc, item.SourceKind));
        Assert.All(atCanteen, item => Assert.Equal("mira", item.SourceId));
    }

    [Fact]
    public void Catalog_RejectsDuplicateInteractionIdsForOneCharacter()
    {
        ActionDefinition first = new ActionDefinition("talk", "Talk", 5);
        ActionDefinition second = new ActionDefinition("talk", "Talk again", 10);

        Assert.Throws<InvalidOperationException>(() => new NpcInteractionCatalog(new[]
        {
            new NpcInteractionDefinition("mira", first),
            new NpcInteractionDefinition("mira", second)
        }));
    }

    [Fact]
    public void Definition_RejectsReservedSelectionActionId()
    {
        ActionDefinition selection = new ActionDefinition("select", "Select NPC", 0);

        Assert.Throws<ArgumentException>(() =>
            new NpcInteractionDefinition("mira", selection));
    }
}
