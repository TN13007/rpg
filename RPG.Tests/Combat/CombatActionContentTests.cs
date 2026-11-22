using System;
using System.IO;
using System.Linq;
using Xunit;

public sealed class CombatActionContentTests
{
    [Fact]
    public void CombatActionJsonReaderLoadsTurnBasedPlayerAndEnemyActionDefinitions()
    {
        string path = Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "Combat",
            "CombatActions.json");
        CombatActionDefinition[] actions = CombatActionJsonReader.Read(File.ReadAllText(path));

        Assert.Equal(9, actions.Length);
        Assert.Equal("quick_strike", actions[0].Id);
        Assert.Equal(CombatActionBehavior.Attack, actions[0].Behavior);
        Assert.Equal(8, actions[0].Damage);
        Assert.Equal("1 AP", actions[0].CostLabel);

        CombatActionDefinition disrupt = Assert.Single(
            actions,
            action => action.Id == "disrupting_strike");
        Assert.Equal(CombatActionBehavior.Attack, disrupt.Behavior);
        Assert.Equal(5, disrupt.Damage);

        CombatActionDefinition control = Assert.Single(
            actions,
            action => action.Id == "presence_staggering_blow");
        Assert.Equal(CombatActionBehavior.Control, control.Behavior);
        Assert.Equal(3, control.Damage);
    }

    [Fact]
    public void CatalogResolvesCharacterReferencesByStableId()
    {
        CombatActionCatalog catalog = new CombatActionCatalog(TestCharacterFactory.CreateCombatActions());
        CombatActionDefinition[] resolved = catalog.Resolve(new[] { "quick_strike", "evade" });
        Assert.Equal(new[] { "quick_strike", "evade" }, resolved.Select(action => action.Id));
    }

    [Fact]
    public void CatalogRejectsDuplicateDefinitionIds()
    {
        CombatActionDefinition action = TestCharacterFactory.CreateCombatActions()[0];
        Assert.Throws<InvalidOperationException>(
            () => new CombatActionCatalog(new[] { action, action }));
    }

    [Fact]
    public void CharacterDataLoaderReadsCombatActionIdsWithoutEmbeddingDefinitions()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "Player.json");
        CharacterData character = CharacterDataLoader.LoadJson(File.ReadAllText(path));
        Assert.Equal(
            new[] { "quick_strike", "heavy_strike", "guard", "evade", "catch_breath", "disrupting_strike" },
            character.CombatActionIds);
    }

    [Fact]
    public void CharacterCatalogRejectsDuplicateCharacterIds()
    {
        CharacterData character = new CharacterData("same_id", "First", "Test", 1, 10, 100, 10, 100, 10, 0, 10, 50, 0);
        CharacterData duplicate = new CharacterData("same_id", "Second", "Test", 1, 10, 100, 10, 100, 10, 0, 10, 50, 0);
        Assert.Throws<InvalidOperationException>(() => new CharacterCatalog(new[] { character, duplicate }));
    }

    [Fact]
    public void GameContentCatalogRejectsUnknownCharacterActionReferences()
    {
        LocationCatalog locations = LocationDataLoader.LoadJson(
            "{\"locations\":[{\"id\":\"test_location\",\"name\":\"Test\",\"region\":\"Test\",\"description\":\"Test\",\"exits\":[],\"actions\":[]}]}");
        CharacterData character = new CharacterData(
            "test_character", "Test", "Test", 1, 10, 100, 10, 100, 10, 0, 10, 50, 0,
            combatActionIds: new[] { "unknown_action" });
        CharacterCatalog characters = new CharacterCatalog(new[] { character });
        CombatActionCatalog actions = new CombatActionCatalog(TestCharacterFactory.CreateCombatActions());
        Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => new GameContentCatalog(
            locations,
            characters,
            actions,
            new NpcScheduleCatalog(Array.Empty<NpcScheduleDefinition>())));
    }

    [Fact]
    public void CharacterDataRejectsDuplicateCombatActionReferences()
    {
        Assert.Throws<ArgumentException>(() => new CharacterData(
            "test", "Test", "Test", 1, 10, 100, 10, 100, 10, 0, 10, 50, 0,
            combatActionIds: new[] { "quick_strike", "quick_strike" }));
    }
}