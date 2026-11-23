using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

public sealed class ModernStudentContentTests
{
    [Fact]
    public void ModernWorldContentLoadsWithValidLocationCharacterAndScheduleReferences()
    {
        GameContentCatalog content = LoadContent();

        Assert.Equal(26, content.Locations.Count());
        LocationContent campusPlaza = content.GetLocation("university_campus");
        Assert.Equal("Campus Plaza", campusPlaza.Name);
        Assert.Equal("NORTH CAMPUS", campusPlaza.Region);
        Assert.Equal(campusPlaza.Region, content.GetLocation("lecture_hall").Region);
        Assert.Equal("The 2:13 AM Incident", content.GetLocation("apartment_block").Events.Single().Title);
        Assert.Equal("Response coordinator", content.GetCharacter("ms_carter").Role);

        LocationContent dormRoom = content.GetLocation("dormitory_room");
        Assert.Equal(new[] { "dormitory_lobby", "dormitory_bathroom" }, dormRoom.Exits);
        Assert.Contains(dormRoom.Interactables, item => item.Id == "bed" && item.PlayerActionIds.Contains("sleep"));
        Assert.Contains(dormRoom.Interactables, item => item.Id == "laptop");
        Assert.Contains(content.GetLocation("dormitory_lobby").Exits, id => id == "dormitory_exterior");
        Assert.Contains(campusPlaza.Exits, id => id == "dormitory_exterior");
        Assert.True(campusPlaza.IsFastTravelDestination);
        Assert.True(content.GetLocation("lecture_hall_exterior").IsFastTravelDestination);
        Assert.False(content.GetLocation("lecture_hall").IsFastTravelDestination);
        Assert.False(content.GetLocation("dormitory_room").IsFastTravelDestination);
        Assert.False(content.GetLocation("dormitory_bathroom").IsFastTravelDestination);
        Assert.False(content.GetLocation("dormitory_lobby").IsFastTravelDestination);
        Assert.Contains(content.GetLocation("lecture_hall_exterior").Exits, id => id == "lecture_hall");
        ActionDefinition attendLesson = content.GetLocation("lecture_hall").Actions.Single(action => action.Id == "attend_lesson");
        Assert.Equal(540, attendLesson.AvailableStartMinute);
        Assert.Equal(555, attendLesson.AvailableEndMinute);

        foreach (LocationContent location in content.Locations)
        {
            foreach (string exitId in location.Exits)
            {
                Assert.NotNull(content.GetLocation(exitId));
            }
        }
    }

    [Fact]
    public void FastTravelQueryExcludesCurrentAndInteriorLocations()
    {
        GameContentCatalog content = LoadContent();
        IReadOnlyList<AvailableInteraction> destinations = new AvailableInteractionQuery()
            .QueryFastTravelDestinations(
                content.Locations,
                "university_campus",
                new ActionDefinition("travel", "Travel", 10));

        Assert.NotEmpty(destinations);
        Assert.DoesNotContain(destinations, item => item.TargetId == "university_campus");
        Assert.Contains(destinations, item => item.TargetId == "lecture_hall_exterior");
        Assert.DoesNotContain(destinations, item => item.TargetId == "lecture_hall");
        Assert.DoesNotContain(destinations, item => item.TargetId == "dormitory_room");
        Assert.DoesNotContain(destinations, item => item.TargetId == "dormitory_bathroom");
        Assert.All(destinations, item => Assert.Equal(InteractionSourceKind.FastTravel, item.SourceKind));
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
