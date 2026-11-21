using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

public sealed class ExplorationInteractionQueryTests
{
    private readonly AcademyContent _academy = new AcademyContent();
    [Fact]
    public void Query_OffersRestEverywhereAndSleepOnlyInDormitory()
    {
        LoadContent();
        Character player = TestCharacterFactory.CreatePlayer();
        CharacterData playerData = LoadPlayerData();
        AvailableInteractionQuery query = new AvailableInteractionQuery();

        IReadOnlyList<AvailableInteraction> dormitory = query.Query(
            player, playerData.PlayerActions, _academy.Get("dormitory"), _academy.Get, _academy.Travel);
        IReadOnlyList<AvailableInteraction> courtyard = query.Query(
            player, playerData.PlayerActions, _academy.Get("courtyard"), _academy.Get, _academy.Travel);

        Assert.Contains(dormitory, item => item.SourceKind == InteractionSourceKind.Player && item.ActionId == "rest");
        Assert.Contains(dormitory, item => item.SourceKind == InteractionSourceKind.Player && item.ActionId == "sleep");
        Assert.Contains(courtyard, item => item.SourceKind == InteractionSourceKind.Player && item.ActionId == "rest");
        Assert.DoesNotContain(courtyard, item => item.SourceKind == InteractionSourceKind.Player && item.ActionId == "sleep");
    }

    [Fact]
    public void Query_ExcludesEventEncounterActionsAndCreatesTravelTargets()
    {
        LoadContent();
        Character player = TestCharacterFactory.CreatePlayer();
        CharacterData playerData = LoadPlayerData();
        IReadOnlyList<AvailableInteraction> interactions = new AvailableInteractionQuery().Query(
            player, playerData.PlayerActions, _academy.Get("courtyard"), _academy.Get, _academy.Travel);

        Assert.DoesNotContain(interactions, item => item.SourceKind == InteractionSourceKind.Location && item.ActionId == "encounter_goblin");
        Assert.Contains(interactions, item =>
            item.SourceKind == InteractionSourceKind.Travel &&
            item.SourceId == "courtyard" &&
            item.TargetId == "dormitory");
    }

    [Fact]
    public void Query_OffersInfirmaryTreatmentAndReturnTravel()
    {
        LoadContent();
        Character player = TestCharacterFactory.CreatePlayer();
        CharacterData playerData = LoadPlayerData();
        LocationContent infirmary = _academy.Get("infirmary");

        IReadOnlyList<AvailableInteraction> interactions = new AvailableInteractionQuery().Query(
            player, playerData.PlayerActions, infirmary, _academy.Get, _academy.Travel);

        Assert.Contains(interactions, item =>
            item.SourceKind == InteractionSourceKind.Location &&
            item.SourceId == "infirmary" &&
            item.ActionId == "infirmary_treatment");
        Assert.Contains(interactions, item =>
            item.SourceKind == InteractionSourceKind.Travel &&
            item.SourceId == "infirmary" &&
            item.TargetId == "courtyard");
    }

    [Fact]
    public void Query_DoesNotMutatePlayerState()
    {
        LoadContent();
        Character player = TestCharacterFactory.CreatePlayer(fatigue: 45, stamina: 3);
        CharacterData playerData = LoadPlayerData();

        new AvailableInteractionQuery().Query(player, playerData.PlayerActions, _academy.Get("dormitory"), _academy.Get, _academy.Travel);

        Assert.Equal(45, player.Fatigue);
        Assert.Equal(3, player.Stamina);
    }

    [Fact]
    public void Mapper_PreservesSourceIdentityAndPresentationGroups()
    {
        LoadContent();
        Character player = TestCharacterFactory.CreatePlayer();
        CharacterData playerData = LoadPlayerData();
        IReadOnlyList<AvailableInteraction> interactions = new AvailableInteractionQuery().Query(
            player, playerData.PlayerActions, _academy.Get("dormitory"), _academy.Get, _academy.Travel);
        ActionChoiceMapper mapper = new ActionChoiceMapper(
            new ActionDurationEstimateMapper(new ActivityTimeCalculator()));

        List<ActionChoiceViewData> choices = mapper.Map(interactions, player);

        Assert.Contains(choices, choice => choice.GroupTitle == "PERSONAL");
        Assert.Contains(choices, choice => choice.GroupTitle == "AT THIS LOCATION");
        Assert.Contains(choices, choice => choice.GroupTitle == "TRAVEL");
        Assert.Contains(choices, choice =>
            ActionSelectionRequest.TryParse(choice.Id, out ActionSelectionRequest request) &&
            request.SourceKind == InteractionSourceKind.Player &&
            request.ActionId == "sleep" &&
            request.SourceId == player.Id);
    }

    [Fact]
    public void EventChoices_UseSharedSelectionContractAndKeepEncounterTarget()
    {
        LoadContent();
        Character player = TestCharacterFactory.CreatePlayer();
        LocationEventDefinition locationEvent = _academy.Get("courtyard").Events[0];
        IReadOnlyList<AvailableInteraction> interactions =
            new AvailableInteractionQuery().Query(locationEvent);
        List<ActionChoiceViewData> choices = new ActionChoiceMapper(
            new ActionDurationEstimateMapper(new ActivityTimeCalculator())).Map(interactions, player);

        Assert.Equal(2, choices.Count);
        Assert.All(choices, choice => Assert.Equal("CURRENT SITUATION", choice.GroupTitle));
        Assert.Contains(choices, choice =>
            ActionSelectionRequest.TryParse(choice.Id, out ActionSelectionRequest request) &&
            request.SourceKind == InteractionSourceKind.Event &&
            request.SourceId == locationEvent.Id &&
            request.ActionId == "event_goblin_attack" &&
            request.TargetId == "encounter_goblin");
    }

    [Fact]
    public void SelectionRequest_RoundTripsAndRejectsMalformedValues()
    {
        ActionSelectionRequest request = new ActionSelectionRequest(
            InteractionSourceKind.Travel, "courtyard", "travel", "guild_hall");

        Assert.True(ActionSelectionRequest.TryParse(request.Encode(), out ActionSelectionRequest parsed));
        Assert.Equal(request.SourceKind, parsed.SourceKind);
        Assert.Equal(request.SourceId, parsed.SourceId);
        Assert.Equal(request.ActionId, parsed.ActionId);
        Assert.Equal(request.TargetId, parsed.TargetId);
        Assert.False(ActionSelectionRequest.TryParse("travel:courtyard", out _));
    }

    [Fact]
    public void Query_OffersNpcRequiredLocationActionOnlyWhenNpcIsPresent()
    {
        Character player = TestCharacterFactory.CreatePlayer();
        ActionDefinition talkToMira = new ActionDefinition(
            "talk_to_mira", "Talk with Mira", 10, requiredNpcId: "mira");
        LocationContent location = new LocationContent(
            "canteen",
            "Canteen",
            "ACADEMY",
            "A canteen.",
            Array.Empty<string>(),
            new[] { talkToMira });
        AvailableInteractionQuery query = new AvailableInteractionQuery();
        ActionDefinition travel = new ActionDefinition("travel", "Travel", 10);

        IReadOnlyList<AvailableInteraction> absent = query.Query(
            player,
            Array.Empty<PlayerActionDefinition>(),
            location,
            _ => location,
            travel,
            new HashSet<string>(StringComparer.Ordinal));
        IReadOnlyList<AvailableInteraction> present = query.Query(
            player,
            Array.Empty<PlayerActionDefinition>(),
            location,
            _ => location,
            travel,
            new HashSet<string>(StringComparer.Ordinal) { "mira" });

        Assert.DoesNotContain(absent, item => item.ActionId == "talk_to_mira");
        Assert.Contains(present, item => item.ActionId == "talk_to_mira");
    }

    [Fact]
    public void EventChoices_OfferNpcRequiredChoiceOnlyWhenNpcIsPresent()
    {
        ActionDefinition joinMira = new ActionDefinition(
            "join_mira", "Join Mira", 10, requiredNpcId: "mira");
        ActionDefinition joinStudents = new ActionDefinition("join_students", "Join other students", 10);
        LocationEventDefinition locationEvent = new LocationEventDefinition(
            "canteen_arrival",
            "A place to sit",
            "CANTEEN",
            "Students are looking for seats.",
            1.0,
            new[]
            {
                new LocationEventChoiceDefinition("join_mira", joinMira),
                new LocationEventChoiceDefinition("join_students", joinStudents)
            });
        AvailableInteractionQuery query = new AvailableInteractionQuery();

        IReadOnlyList<AvailableInteraction> absent = query.Query(
            locationEvent, new HashSet<string>(StringComparer.Ordinal));
        IReadOnlyList<AvailableInteraction> present = query.Query(
            locationEvent, new HashSet<string>(StringComparer.Ordinal) { "mira" });

        Assert.DoesNotContain(absent, item => item.ActionId == "join_mira");
        Assert.Contains(absent, item => item.ActionId == "join_students");
        Assert.Contains(present, item => item.ActionId == "join_mira");
    }

    private void LoadContent()
    {
        _academy.LoadJson(File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Data", "AcademyLocations.json")));
    }

    private static CharacterData LoadPlayerData()
    {
        return CharacterDataLoader.LoadJson(File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Data", "Player.json")));
    }
}
