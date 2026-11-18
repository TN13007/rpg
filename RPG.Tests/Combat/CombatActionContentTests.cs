using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public sealed class CombatActionContentTests
{
    [Fact]
    public void CombatActionJsonReaderLoadsAuthoredDefinitions()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "Combat", "CombatActions.json");
        CombatActionDefinition[] actions = CombatActionJsonReader.Read(File.ReadAllText(path));

        Assert.Equal(4, actions.Length);
        Assert.Equal("attack", actions[0].Id);
        Assert.Equal(CombatActionBehavior.Attack, actions[0].Behavior);
        Assert.Equal(20, actions[0].Damage);
        Assert.Equal(1, actions[1].StaminaCost);
        Assert.Equal(2, actions[2].StaminaCost);
        Assert.Equal(3, actions[3].RecoveryAmount);
    }

    [Fact]
    public void CatalogResolvesCharacterReferencesByStableId()
    {
        CombatActionCatalog catalog = new CombatActionCatalog(TestCharacterFactory.CreateCombatActions());

        CombatActionDefinition[] resolved = catalog.Resolve(new[] { "attack", "dodge" });

        Assert.Equal(new[] { "attack", "dodge" }, resolved.Select(action => action.Id));
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

        Assert.Equal(new[] { "attack", "defend", "dodge", "catch_breath" }, character.CombatActionIds);
    }

    [Fact]
    public void CharacterDataRejectsDuplicateCombatActionReferences()
    {
        Assert.Throws<ArgumentException>(() => new CharacterData(
            "test", "Test", "Test", 1,
            10, 100, 10, 100,
            10, 0, 10, 50, 0,
            combatActionIds: new[] { "attack", "attack" }));
    }
}
