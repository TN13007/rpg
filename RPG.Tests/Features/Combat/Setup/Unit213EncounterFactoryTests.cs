using System;
using System.IO;
using System.Linq;
using Xunit;

public sealed class Unit213EncounterFactoryTests
{
    [Fact]
    public void RuntimeEncounterComposesFourPartyMembersAndTwoFixedUnit213Patterns()
    {
        GameContentCatalog content = LoadContent();
        Character primaryCharacter = new Character(content.GetCharacter("player_student"));
        CombatantDefinition[] roster =
            Unit213EncounterFactory.CreateCombatants(content, primaryCharacter);

        Assert.Equal(6, roster.Length);
        Assert.Equal(4, roster.Count(item => item.Side == CombatantSide.Party));
        Assert.Equal(2, roster.Count(item => item.Side == CombatantSide.Enemy));
        Assert.Same(primaryCharacter, roster[0].Character);

        CombatantDefinition presence = Assert.Single(
            roster,
            item => item.Character.Id == "hostile_presence");
        CombatantDefinition echo = Assert.Single(
            roster,
            item => item.Character.Id == "hostile_echo");

        Assert.Equal(new[] { "presence_heavy_swing" }, presence.EnemyPattern.ActionIds);
        Assert.Equal(new[] { "presence_staggering_blow" }, echo.EnemyPattern.ActionIds);

        CombatService combat = new CombatService(roster);
        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);

        CombatSnapshot snapshot = combat.GetSnapshot();
        Assert.Equal(2, snapshot.ActionCapacity);
        Assert.Equal(2, snapshot.CurrentActionPoints);
        Assert.Null(typeof(EnemyCombatStatus).GetProperty("IntentDescription"));

        CombatResult enemyTurn = combat.EndPlayerTurn();
        Assert.Equal(
            new[] { "presence_heavy_swing", "presence_staggering_blow" },
            enemyTurn.Events
                .Where(item => item.Type == CombatEventType.EnemyAction)
                .Select(item => item.ActionId));
        Assert.Equal("Alex", enemyTurn.Events.First(item =>
            item.Type == CombatEventType.EnemyAction &&
            item.ActionId == "presence_heavy_swing").TargetName);
    }

    private static GameContentCatalog LoadContent()
    {
        string charactersDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "Characters");
        CharacterData[] characters = Directory.GetFiles(charactersDirectory, "*.json")
            .Select(path => CharacterDataLoader.LoadJson(File.ReadAllText(path), path))
            .ToArray();

        return new GameContentCatalog(
            new LocationCatalog(),
            new CharacterCatalog(characters),
            new CombatActionCatalog(TestCharacterFactory.CreateCombatActions()),
            new NpcScheduleCatalog(Array.Empty<NpcScheduleDefinition>()));
    }
}