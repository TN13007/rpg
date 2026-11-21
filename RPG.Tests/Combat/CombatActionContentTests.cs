using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public sealed class CombatActionContentTests
{
    [Fact]
    public void CombatActionJsonReaderLoadsPlayerAndPresenceActionDefinitions()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "Combat", "CombatActions.json");
        CombatActionDefinition[] actions = CombatActionJsonReader.Read(File.ReadAllText(path));
        Assert.Equal(9, actions.Length);
        Assert.Equal("quick_strike", actions[0].Id);
        Assert.Equal(CombatActionBehavior.Attack, actions[0].Behavior);
        Assert.Equal(8, actions[0].Damage);
        Assert.Equal(70, actions[0].RecoveryCost);
        Assert.Equal(35, actions[5].DelayAmount);
        Assert.Equal(0.5, actions[8].StunDuration);
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
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => new CombatActionCatalog(new[] { action, action }));
        Assert.Contains("Duplicate combat action ID", exception.Message);
    }

    [Fact]
    public void CatalogRejectsUnknownCharacterActionReference()
    {
        CombatActionCatalog catalog = new CombatActionCatalog(TestCharacterFactory.CreateCombatActions());
        KeyNotFoundException exception = Assert.Throws<KeyNotFoundException>(
            () => catalog.Resolve(new[] { "missing_action" }));
        Assert.Contains("missing_action", exception.Message);
    }

    [Fact]
    public void CharacterDataLoaderReadsCombatActionIdsWithoutEmbeddingDefinitions()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "Player.json");
        CharacterData character = CharacterDataLoader.LoadJson(File.ReadAllText(path));
        Assert.Equal(new[] { "quick_strike", "heavy_strike", "guard", "evade", "catch_breath", "disrupting_strike" }, character.CombatActionIds);
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
        LocationCatalog academy = new LocationCatalog();
        academy.LoadJson("{\"locations\":[{\"id\":\"test_location\",\"name\":\"Test\",\"region\":\"Test\",\"description\":\"Test\",\"exits\":[],\"actions\":[]}]}");
        CharacterData character = new CharacterData("test_character", "Test", "Test", 1, 10, 100, 10, 100, 10, 0, 10, 50, 0,
            combatActionIds: new[] { "unknown_action" });
        CharacterCatalog characters = new CharacterCatalog(new[] { character });
        CombatActionCatalog actions = new CombatActionCatalog(TestCharacterFactory.CreateCombatActions());
        Assert.Throws<KeyNotFoundException>(() => new GameContentCatalog(
            academy,
            characters,
            actions,
            new NpcScheduleCatalog(Array.Empty<NpcScheduleDefinition>())));
    }

    [Fact]
    public void CharacterDataRejectsDuplicateCombatActionReferences()
    {
        Assert.Throws<ArgumentException>(() => new CharacterData("test", "Test", "Test", 1, 10, 100, 10, 100, 10, 0, 10, 50, 0,
            combatActionIds: new[] { "quick_strike", "quick_strike" }));
    }
}
