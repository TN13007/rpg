using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

public sealed class ExplorationInteractionQueryTests
{
    private LocationCatalog _locations;
    [Fact]
    public void Query_OffersRestEverywhereAndSleepOnlyInStudentApartment()
    {
        LoadContent();
        Character player = TestCharacterFactory.CreatePlayer();
        CharacterData playerData = LoadPlayerData();
        AvailableInteractionQuery query = new AvailableInteractionQuery();

        IReadOnlyList<AvailableInteraction> studentApartmentChoices = query.Query(
            player, playerData.PlayerActions, _locations.Get("student_apartment"), _locations.Get, _locations.Travel);
        IReadOnlyList<AvailableInteraction> campusChoices = query.Query(
            player, playerData.PlayerActions, _locations.Get("university_campus"), _locations.Get, _locations.Travel);

        Assert.Contains(studentApartmentChoices, item => item.SourceKind == InteractionSourceKind.Player && item.ActionId == "rest");
        Assert.Contains(studentApartmentChoices, item => item.SourceKind == InteractionSourceKind.Player && item.ActionId == "sleep");
        Assert.Contains(campusChoices, item => item.SourceKind == InteractionSourceKind.Player && item.ActionId == "rest");
        Assert.DoesNotContain(campusChoices, item => item.SourceKind == InteractionSourceKind.Player && item.ActionId == "sleep");
    }

    [Fact]
    public void Query_ExcludesEventEncounterActionsAndCreatesTravelTargets()
    {
        LoadContent();
        Character player = TestCharacterFactory.CreatePlayer();
        CharacterData playerData = LoadPlayerData();
        IReadOnlyList<AvailableInteraction> interactions = new AvailableInteractionQuery().Query(
            player, playerData.PlayerActions, _locations.Get("apartment_block"), _locations.Get, _locations.Travel);

        Assert.DoesNotContain(interactions, item => item.SourceKind == InteractionSourceKind.Location && item.ActionId == "enter_unit_213");
        Assert.Contains(interactions, item =>
            item.SourceKind == InteractionSourceKind.Travel &&
            item.SourceId == "apartment_block" &&
            item.TargetId == "east_district");
    }

    [Fact]
    public void Query_OffersInfirmaryTreatmentAndReturnTravel()
    {
        LoadContent();
        Character player = TestCharacterFactory.CreatePlayer();
        CharacterData playerData = LoadPlayerData();
        LocationContent clinic = _locations.Get("campus_clinic");

        IReadOnlyList<AvailableInteraction> interactions = new AvailableInteractionQuery().Query(
            player, playerData.PlayerActions, clinic, _locations.Get, _locations.Travel);

        Assert.Contains(interactions, item =>
            item.SourceKind == InteractionSourceKind.Location &&
            item.SourceId == "campus_clinic" &&
            item.ActionId == "infirmary_treatment");
        Assert.Contains(interactions, item =>
            item.SourceKind == InteractionSourceKind.Travel &&
            item.SourceId == "campus_clinic" &&
            item.TargetId == "east_district");
    }

    [Fact]
    public void Query_DoesNotMutatePlayerState()
    {
        LoadContent();
        Character player = TestCharacterFactory.CreatePlayer(
            health: 12, fatigue: 45, stamina: 3, mood: 35, gold: 7);
        CharacterData playerData = LoadPlayerData();

        new AvailableInteractionQuery().Query(
            player,
            playerData.PlayerActions,
            _locations.Get("student_apartment"),
            _locations.Get,
            _locations.Travel);

        Assert.Equal(12, player.Health);
        Assert.Equal(45, player.Fatigue);
        Assert.Equal(3, player.Stamina);
        Assert.Equal(35, player.Mood);
        Assert.Equal(7, player.Gold);
    }

    [Fact]
    public void QueryResults_AreReadOnlyForNormalRecoveryAndEventChoices()
    {
        LoadContent();
        CharacterData playerData = LoadPlayerData();
        AvailableInteractionQuery query = new AvailableInteractionQuery();

        Character activePlayer = TestCharacterFactory.CreatePlayer();
        IReadOnlyList<AvailableInteraction> normalChoices = query.Query(
            activePlayer,
            playerData.PlayerActions,
            _locations.Get("student_apartment"),
            _locations.Get,
            _locations.Travel);
        AssertReadOnly(normalChoices);

        Character incapacitatedPlayer = TestCharacterFactory.CreatePlayer(health: 0);
        IReadOnlyList<AvailableInteraction> recoveryChoices = query.Query(
            incapacitatedPlayer,
            playerData.PlayerActions,
            _locations.Get("student_apartment"),
            _locations.Get,
            _locations.Travel);
        AssertReadOnly(recoveryChoices);

        LocationEventDefinition locationEvent = _locations.Get("campus_cafe").Events[0];
        IReadOnlyList<AvailableInteraction> eventChoices = query.Query(locationEvent);
        AssertReadOnly(eventChoices);
    }

    [Fact]
    public void Mapper_PreservesSourceIdentityAndPresentationGroups()
    {
        LoadContent();
        Character player = TestCharacterFactory.CreatePlayer();
        CharacterData playerData = LoadPlayerData();
        IReadOnlyList<AvailableInteraction> interactions = new AvailableInteractionQuery().Query(
            player, playerData.PlayerActions, _locations.Get("student_apartment"), _locations.Get, _locations.Travel);
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
    public void EventChoices_UseSharedSelectionContractWithoutForcingCombat()
    {
        LoadContent();
        Character player = TestCharacterFactory.CreatePlayer();
        LocationEventDefinition locationEvent = _locations.Get("campus_cafe").Events[0];
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
            request.ActionId == "join_the_conversation" &&
            string.IsNullOrEmpty(request.TargetId));
    }

    [Fact]
    public void SelectionRequest_RoundTripsAndRejectsMalformedValues()
    {
        ActionSelectionRequest request = new ActionSelectionRequest(
            InteractionSourceKind.Travel, "university_campus", "travel", "student_union");

        Assert.True(ActionSelectionRequest.TryParse(request.Encode(), out ActionSelectionRequest parsed));
        Assert.Equal(request.SourceKind, parsed.SourceKind);
        Assert.Equal(request.SourceId, parsed.SourceId);
        Assert.Equal(request.ActionId, parsed.ActionId);
        Assert.Equal(request.TargetId, parsed.TargetId);
        Assert.False(ActionSelectionRequest.TryParse("travel:campus", out _));
    }

    [Fact]
    public void Query_OffersNpcRequiredLocationActionOnlyWhenNpcIsPresent()
    {
        Character player = TestCharacterFactory.CreatePlayer();
        ActionDefinition talkToMira = new ActionDefinition(
            "talk_to_mira", "Talk with Mira", 10, requiredNpcId: "mira");
        LocationContent location = new LocationContent(
            "campus_cafe",
            "Campus Café",
            "CAMPUS",
            "A campus café.",
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
            "campus_cafe_conversation",
            "A Seat at the Table",
            "CAMPUS CAFÉ",
            "Students are sharing coffee and discussing a neighborhood post.",
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

    [Fact]
    public void EventQuery_RechecksEventLevelNpcRequirement()
    {
        ActionDefinition join = new ActionDefinition("join", "Join", 10);
        LocationEventDefinition locationEvent = new LocationEventDefinition(
            "mira_meetup",
            "Meetup",
            "CAMPUS",
            "Mira is waiting here.",
            1.0,
            new[] { new LocationEventChoiceDefinition("join", join) },
            requiredNpcId: "mira");
        AvailableInteractionQuery query = new AvailableInteractionQuery();

        IReadOnlyList<AvailableInteraction> absent = query.Query(
            locationEvent,
            new HashSet<string>(StringComparer.Ordinal));
        IReadOnlyList<AvailableInteraction> present = query.Query(
            locationEvent,
            new HashSet<string>(StringComparer.Ordinal) { "mira" });

        Assert.Empty(absent);
        AssertReadOnly(absent);
        Assert.Contains(present, interaction => interaction.ActionId == "join");
        AssertReadOnly(present);
    }

    private static void AssertReadOnly(IReadOnlyList<AvailableInteraction> interactions)
    {
        IList<AvailableInteraction> mutableView =
            Assert.IsAssignableFrom<IList<AvailableInteraction>>(interactions);
        AvailableInteraction item = interactions.Count > 0
            ? interactions[0]
            : new AvailableInteraction(
                InteractionSourceKind.Event, "test-event", "test-action", string.Empty,
                new ActionDefinition("test-action", "Test", 0));

        Assert.True(mutableView.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => mutableView.Add(item));
        if (mutableView.Count > 0)
        {
            Assert.Throws<NotSupportedException>(() => mutableView[0] = item);
        }
    }

    private void LoadContent()
    {
        _locations = LocationDataLoader.LoadJson(File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Data", "Locations.json")));
    }

    private static CharacterData LoadPlayerData()
    {
        return CharacterDataLoader.LoadJson(File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Data", "Player.json")));
    }
}
