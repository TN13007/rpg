using System;
using System.IO;
using System.Linq;
using Xunit;

public sealed class ModernStudentContentTests
{
    [Fact]
    public void ModernWorldContentLoadsWithValidLocationCharacterAndScheduleReferences()
    {
        GameContentCatalog content = LoadContent();

        Assert.Equal(13, content.Locations.Count());
        Assert.Equal("University Campus", content.GetLocation("university_campus").Name);
        Assert.Equal("The 2:13 AM Incident", content.GetLocation("apartment_block").Events.Single().Title);
        Assert.Equal("Response coordinator", content.GetCharacter("ms_carter").Role);

        foreach (LocationContent location in content.Locations)
        {
            foreach (string exitId in location.Exits)
            {
                Assert.NotNull(content.GetLocation(exitId));
            }
        }
    }

    [Fact]
    public void StartingPartyUsesTheFourStudentCharacters()
    {
        GameContentCatalog content = LoadContent();
        Character player = new Character(content.GetCharacter("player_student"));

        CombatantDefinition[] party = StartingParty.CreateCombatants(content, player);

        Assert.Equal(
            new[] { "player_student", "noah", "mira", "sera" },
            party.Select(member => member.Character.Id));
        Assert.All(party, member => Assert.NotEmpty(member.Actions));
    }

    [Fact]
    public void Unit213IncidentOffersNonCombatResponses()
    {
        GameContentCatalog content = LoadContent();
        LocationEventDefinition incident = Assert.Single(content.GetLocation("apartment_block").Events);

        Assert.Equal("apartment_213_incident", incident.Id);
        Assert.Equal(
            new[] { "listen_from_hallway", "enter_unit_213", "leave_the_building" },
            incident.Choices.Select(choice => choice.Id));
        Assert.All(incident.Choices, choice => Assert.Null(choice.EncounterActionId));
    }

    private static GameContentCatalog LoadContent()
    {
        string dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
        string charactersDirectory = Path.Combine(dataDirectory, "Characters");

        LocationCatalog locations = LocationDataLoader.LoadJson(
            File.ReadAllText(Path.Combine(dataDirectory, "Locations", "Locations.json")));

        CharacterData[] characters = Directory.GetFiles(charactersDirectory, "*.json")
            .Select(path => CharacterDataLoader.LoadJson(File.ReadAllText(path), path))
            .ToArray();

        CombatActionCatalog combatActions = new CombatActionCatalog(
            CombatActionJsonReader.Read(File.ReadAllText(
                Path.Combine(dataDirectory, "Combat", "CombatActions.json"))));
        NpcScheduleCatalog schedules = NpcScheduleDataLoader.LoadJson(
            File.ReadAllText(Path.Combine(dataDirectory, "NPC", "NpcSchedules.json")));
        NpcInteractionCatalog interactions = NpcInteractionDataLoader.LoadJson(
            File.ReadAllText(Path.Combine(dataDirectory, "NPC", "NpcInteractions.json")));

        return new GameContentCatalog(
            locations,
            new CharacterCatalog(characters),
            combatActions,
            schedules,
            interactions);
    }
}
