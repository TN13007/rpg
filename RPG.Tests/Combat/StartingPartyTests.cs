using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

public sealed class StartingPartyTests
{
    [Fact]
    public void CreatesFourPartyMembersInStableOrderAndReusesThePrimaryCharacter()
    {
        GameContentCatalog content = LoadContent();
        Character primaryCharacter = new Character(content.GetCharacter("player-ari"));

        CombatantDefinition[] party = StartingParty.CreateCombatants(content, primaryCharacter);

        Assert.Equal(4, party.Length);
        Assert.Equal(
            new[] { "player-ari", "bram", "mira", "sera" },
            party.Select(combatant => combatant.Character.Id));
        Assert.Same(primaryCharacter, party[0].Character);
        Assert.All(party, combatant => Assert.Equal(CombatantSide.Party, combatant.Side));
        Assert.All(party, combatant => Assert.NotEmpty(combatant.Actions));
    }

    [Fact]
    public void UsesTheCombatActionsAuthoredForEachPartyMember()
    {
        GameContentCatalog content = LoadContent();
        Character primaryCharacter = new Character(content.GetCharacter("player-ari"));

        CombatantDefinition[] party = StartingParty.CreateCombatants(content, primaryCharacter);

        Assert.Equal(
            new[] { "heavy_strike", "guard", "catch_breath" },
            GetActionIds(party, "bram"));
        Assert.Equal(
            new[] { "quick_strike", "disrupting_strike", "evade", "catch_breath" },
            GetActionIds(party, "mira"));
        Assert.Equal(
            new[] { "quick_strike", "heavy_strike", "evade" },
            GetActionIds(party, "sera"));
    }

    [Fact]
    public void FourPersonPartyCanStartAnEncounterWithTheExistingCombatService()
    {
        GameContentCatalog content = LoadContent();
        Character primaryCharacter = new Character(content.GetCharacter("player-ari"));
        CombatService combat = CreateCombat(content, primaryCharacter);

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);
        Assert.Equal(4, combat.PartyMembers.Count);
        Assert.Equal(
            new[] { "player-ari", "bram", "mira", "sera" },
            combat.PartyMembers.Select(character => character.Id));
    }

    [Fact]
    public void EveryPartyMemberReceivesAnIndependentPlayerDecisionWindow()
    {
        GameContentCatalog content = LoadContent();
        Character primaryCharacter = new Character(content.GetCharacter("player-ari"));
        CombatService combat = CreateCombat(content, primaryCharacter);

        string[] expectedActorIds = { "player-ari", "sera", "mira", "bram" };
        string[] expectedActionIds = { "quick_strike", "quick_strike", "quick_strike", "heavy_strike" };

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);

        for (int index = 0; index < expectedActorIds.Length; index++)
        {
            combat.AdvanceCombatTime(4.0);

            CombatSnapshot snapshot = combat.GetSnapshot();
            string actorId = expectedActorIds[index];
            string actionId = expectedActionIds[index];

            Assert.True(combat.IsAwaitingPlayerDecision);
            Assert.Equal(actorId, snapshot.ActiveCombatantId);
            Assert.NotEmpty(snapshot.AvailableActions);
            Assert.All(
                snapshot.AvailableActions,
                action => Assert.Equal(actorId, action.ActorId));

            Assert.Contains(
                snapshot.AvailableActions,
                action => action.Id == actionId
                    && action.TargetId == "goblin-a"
                    && action.IsAvailable);

            CombatResult result = combat.ExecuteAction(
                new CombatActionRequest(actionId, "goblin-a", actorId));

            Assert.True(result.Success, result.FailureReason);
        }
    }

    [Fact]
    public void RejectsAPrimaryCharacterOutsideTheStartingParty()
    {
        GameContentCatalog content = LoadContent();
        Character outsider = new Character(new CharacterData(
            "outsider",
            "Outsider",
            "Test",
            1,
            10,
            100,
            10,
            100,
            10,
            0,
            10,
            70,
            0));

        Assert.Throws<ArgumentException>(
            () => StartingParty.CreateCombatants(content, outsider));
    }

    private static CombatService CreateCombat(
        GameContentCatalog content,
        Character primaryCharacter)
    {
        Character goblin = new Character(content.GetCharacter("goblin-a"));
        List<CombatantDefinition> encounter = new List<CombatantDefinition>(
            StartingParty.CreateCombatants(content, primaryCharacter));

        encounter.Add(new CombatantDefinition(
            goblin,
            CombatantSide.Enemy,
            content.GetCombatActionsForCharacter(goblin.Id)));

        return new CombatService(encounter, new Random(7));
    }

    private static string[] GetActionIds(CombatantDefinition[] party, string characterId)
    {
        CombatantDefinition member = party.Single(
            combatant => combatant.Character.Id == characterId);

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
            new AcademyContent(),
            new CharacterCatalog(characters),
            new CombatActionCatalog(TestCharacterFactory.CreateCombatActions()),
            new NpcScheduleCatalog(Array.Empty<NpcScheduleDefinition>()));
    }
}
