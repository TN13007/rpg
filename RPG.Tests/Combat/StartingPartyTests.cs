using System;
using System.IO;
using System.Linq;
using Xunit;

public sealed class StartingPartyTests
{
    [Fact]
    public void CreatesFourPartyMembersInStableOrderAndReusesThePrimaryCharacter()
    {
        GameContentCatalog content = LoadContent();
        Character primaryCharacter = new Character(content.GetCharacter("player_student"));
        CombatantDefinition[] party = StartingParty.CreateCombatants(content, primaryCharacter);

        Assert.Equal(4, party.Length);
        Assert.Equal(
            new[] { "player_student", "noah", "mira", "sera" },
            party.Select(combatant => combatant.Character.Id));
        Assert.Same(primaryCharacter, party[0].Character);
        Assert.All(party, combatant => Assert.Equal(CombatantSide.Party, combatant.Side));
        Assert.All(party, combatant => Assert.NotEmpty(combatant.Actions));
    }

    [Fact]
    public void UsesTheCombatActionsAuthoredForEachPartyMember()
    {
        GameContentCatalog content = LoadContent();
        Character primaryCharacter = new Character(content.GetCharacter("player_student"));
        CombatantDefinition[] party = StartingParty.CreateCombatants(content, primaryCharacter);

        Assert.Equal(new[] { "heavy_strike", "guard", "catch_breath" }, GetActionIds(party, "noah"));
        Assert.Equal(new[] { "quick_strike", "disrupting_strike", "evade", "catch_breath" }, GetActionIds(party, "mira"));
        Assert.Equal(new[] { "quick_strike", "heavy_strike", "evade" }, GetActionIds(party, "sera"));
    }

    [Fact]
    public void AllFourStartingPartyMembersCanSpendOnePointEachWhenCapacityIsFour()
    {
        GameContentCatalog content = LoadContent();
        Character primaryCharacter = new Character(content.GetCharacter("player_student"));
        CombatantDefinition[] partyDefinitions = StartingParty.CreateCombatants(content, primaryCharacter);
        Character enemy = new Character(content.GetCharacter("hostile_presence"));

        CombatantDefinition[] roster = partyDefinitions
            .Concat(new[]
            {
                new CombatantDefinition(
                    enemy,
                    CombatantSide.Enemy,
                    content.GetCombatActionsForCharacter(enemy.Id),
                    enemyPattern: new EnemyPattern(new[] { "presence_quick_jab" }))
            })
            .ToArray();

        CombatService combat = new CombatService(roster, actionCapacity: 4);
        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);

        Assert.Equal(4, combat.GetSnapshot().AvailableActions.Select(action => action.ActorId).Distinct().Count());
        string[] expectedActors = { "player_student", "noah", "mira", "sera" };
        string[] expectedActions = { "quick_strike", "heavy_strike", "quick_strike", "quick_strike" };

        for (int index = 0; index < expectedActors.Length; index++)
        {
            CombatSnapshot snapshot = combat.GetSnapshot();
            Assert.Contains(snapshot.AvailableActions, option =>
                option.ActorId == expectedActors[index] &&
                option.Id == expectedActions[index] &&
                option.IsAvailable);

            CombatResult result = combat.ExecuteAction(new CombatActionRequest(
                expectedActions[index],
                enemy.Id,
                expectedActors[index]));

            Assert.True(result.Success, result.FailureReason);
            int expectedRound = index == expectedActors.Length - 1 ? 2 : 1;
            int expectedActionPoints = index == expectedActors.Length - 1 ? 4 : 3 - index;
            Assert.Equal(expectedRound, result.Snapshot.Round);
            Assert.Equal(expectedActionPoints, result.Snapshot.CurrentActionPoints);
        }

        CombatSnapshot nextRound = combat.GetSnapshot();
        Assert.Equal(2, nextRound.Round);
        Assert.Equal(4, nextRound.CurrentActionPoints);
        Assert.All(nextRound.Party, member => Assert.True(member.IsActive));
    }

    [Fact]
    public void RejectsAPrimaryCharacterOutsideTheStartingParty()
    {
        GameContentCatalog content = LoadContent();
        Character outsider = new Character(new CharacterData(
            "outsider", "Outsider", "Test", 1, 10, 100, 10, 100, 10, 0, 10, 50, 0));

        Assert.Throws<ArgumentException>(
            () => StartingParty.CreateCombatants(content, outsider));
    }

    private static string[] GetActionIds(CombatantDefinition[] party, string characterId)
    {
        CombatantDefinition member = party.Single(combatant => combatant.Character.Id == characterId);
        return member.Actions.Select(action => action.Id).ToArray();
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