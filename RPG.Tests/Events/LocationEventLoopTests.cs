using System;
using System.Collections.Generic;
using Xunit;

public sealed class LocationEventLoopTests
{
    private LocationCatalog _locations;

    [Fact]
    public void LocationCatalog_LoadsFirstAndReturnVisitDescriptions()
    {
        const string json = @"{""locations"":[{""id"":""gate"",""name"":""Gate"",""region"":""REGION"",""description"":{""firstVisit"":""A long first introduction."",""returnVisit"":""A short familiar description.""},""exits"":[],""actions"":[]}]}";

        _locations = LocationDataLoader.LoadJson(json);

        LocationContent location = _locations.Get("gate");
        Assert.Equal("A long first introduction.", location.GetDescription(true));
        Assert.Equal("A short familiar description.", location.GetDescription(false));
    }

    [Fact]
    public void LocationCatalog_LegacyStringDescriptionRemainsCompatible()
    {
        const string json = @"{""locations"":[{""id"":""gate"",""name"":""Gate"",""region"":""REGION"",""description"":""The old single description."",""exits"":[],""actions"":[]}]}";

        _locations = LocationDataLoader.LoadJson(json);

        LocationContent location = _locations.Get("gate");
        Assert.Equal("The old single description.", location.GetDescription(true));
        Assert.Equal("The old single description.", location.GetDescription(false));
    }

    [Fact]
    public void LocationCatalog_LoadsApartmentIncidentAndItsChoices()
    {
        LoadLocationCatalog();
        LocationContent apartmentBlock = _locations.Get("apartment_block");

        Assert.Single(apartmentBlock.Events);
        Assert.Equal("apartment_213_incident", apartmentBlock.Events[0].Id);
        Assert.Equal(1, apartmentBlock.Events[0].Chance);
        Assert.Equal(3, apartmentBlock.Events[0].Choices.Length);
        Assert.Equal("listen_from_hallway", apartmentBlock.Events[0].Choices[0].Id);
        Assert.Null(apartmentBlock.Events[0].Choices[0].EncounterActionId);
        Assert.Equal(10, apartmentBlock.Events[0].Choices[0].Action.DurationMinutes);
        Assert.Equal("enter_unit_213", apartmentBlock.Events[0].Choices[1].Id);
    }

    [Fact]
    public void EventService_TriggersGuaranteedEventOnlyOncePerLocationPerDay()
    {
        LocationEventDefinition locationEvent = CreateEvent("guaranteed", 1);
        LocationContent location = CreateLocation("courtyard", locationEvent);
        LocationEventService service = new LocationEventService(new Random(1));

        bool firstResult = service.TryTrigger(location, 1, true, out LocationEventDefinition firstEvent);
        bool secondResult = service.TryTrigger(location, 1, true, out LocationEventDefinition secondEvent);
        bool nextDayResult = service.TryTrigger(location, 2, true, out LocationEventDefinition nextDayEvent);

        Assert.True(firstResult);
        Assert.Same(locationEvent, firstEvent);
        Assert.False(secondResult);
        Assert.Null(secondEvent);
        Assert.True(nextDayResult);
        Assert.Same(locationEvent, nextDayEvent);
    }

    [Fact]
    public void EventService_RequiredNpcMustBePresentAndAbsenceDoesNotConsumeDailyAttempt()
    {
        LocationEventDefinition locationEvent = CreateEvent("mira_arrival", 1, "mira");
        LocationContent location = CreateLocation("campus_cafe", locationEvent);
        LocationEventService service = new LocationEventService(new Random(1));

        bool absent = service.TryTrigger(
            location, 1, true, out LocationEventDefinition absentEvent,
            new HashSet<string>(StringComparer.Ordinal));
        bool present = service.TryTrigger(
            location, 1, true, out LocationEventDefinition presentEvent,
            new HashSet<string>(StringComparer.Ordinal) { "mira" });

        Assert.False(absent);
        Assert.Null(absentEvent);
        Assert.True(present);
        Assert.Same(locationEvent, presentEvent);
    }

    [Fact]
    public void LocationCatalog_LoadsEventNpcRequirement()
    {
        string events = "[{ \"id\": \"mira_arrival\", \"title\": \"Mira arrives\","
            + " \"region\": \"REGION\", \"description\": \"Mira is here.\","
            + " \"chance\": 1, \"requiredNpcId\": \"mira\","
            + " \"choices\": [{ \"id\": \"say_hello\", \"label\": \"Say hello\","
            + " \"durationMinutes\": 1 }] }]";

        _locations = LocationDataLoader.LoadJson(CreateContentJson(events));

        Assert.Equal("mira", _locations.Get("courtyard").Events[0].RequiredNpcId);
    }

    [Fact]
    public void EventService_FailedChanceIsNotRerolledOnSameDay()
    {
        LocationContent location = CreateLocation("courtyard", CreateEvent("never", 0));
        LocationEventService service = new LocationEventService(new Random(1));

        bool firstResult = service.TryTrigger(location, 1, true, out LocationEventDefinition firstEvent);
        bool secondResult = service.TryTrigger(location, 1, true, out LocationEventDefinition secondEvent);

        Assert.False(firstResult);
        Assert.False(secondResult);
        Assert.Null(firstEvent);
        Assert.Null(secondEvent);
    }

    [Fact]
    public void EventService_DifferentLocationsHaveIndependentDailyRolls()
    {
        LocationEventDefinition locationEvent = CreateEvent("guaranteed", 1);
        LocationEventService service = new LocationEventService(new Random(1));

        bool firstLocation = service.TryTrigger(CreateLocation("courtyard", locationEvent), 1, true, out _);
        bool secondLocation = service.TryTrigger(CreateLocation("classroom", locationEvent), 1, true, out _);

        Assert.True(firstLocation);
        Assert.True(secondLocation);
    }

    [Fact]
    public void EventService_UnavailableCharacterDoesNotConsumeDailyAttempt()
    {
        LocationContent location = CreateLocation("courtyard", CreateEvent("guaranteed", 1));
        LocationEventService service = new LocationEventService(new Random(1));

        bool unavailableResult = service.TryTrigger(location, 1, false, out LocationEventDefinition unavailableEvent);
        bool availableResult = service.TryTrigger(location, 1, true, out LocationEventDefinition availableEvent);

        Assert.False(unavailableResult);
        Assert.Null(unavailableEvent);
        Assert.True(availableResult);
        Assert.NotNull(availableEvent);
    }

    [Fact]
    public void EventService_UsesAuthoredOrderAndTriggersAtMostOneEvent()
    {
        LocationEventDefinition first = CreateEvent("first", 1);
        LocationEventDefinition second = CreateEvent("second", 1);
        LocationContent location = CreateLocation("courtyard", first, second);
        LocationEventService service = new LocationEventService(new Random(1));

        bool result = service.TryTrigger(location, 1, true, out LocationEventDefinition triggeredEvent);

        Assert.True(result);
        Assert.Same(first, triggeredEvent);
    }

    [Fact]
    public void EventService_LocationWithoutEventsDoesNotConsumeDailyAttempt()
    {
        LocationEventService service = new LocationEventService(new Random(1));
        LocationContent emptyLocation = CreateLocation("courtyard");
        LocationContent eventLocation = CreateLocation("courtyard", CreateEvent("guaranteed", 1));

        bool emptyResult = service.TryTrigger(emptyLocation, 1, true, out LocationEventDefinition emptyEvent);
        bool eventResult = service.TryTrigger(eventLocation, 1, true, out LocationEventDefinition triggeredEvent);

        Assert.False(emptyResult);
        Assert.Null(emptyEvent);
        Assert.True(eventResult);
        Assert.NotNull(triggeredEvent);
    }

    [Fact]
    public void EventService_SkipsFailedCandidateAndSelectsNextSuccessfulCandidate()
    {
        LocationEventDefinition first = CreateEvent("first", 0);
        LocationEventDefinition second = CreateEvent("second", 1);
        LocationContent location = CreateLocation("courtyard", first, second);
        LocationEventService service = new LocationEventService(new Random(1));

        bool result = service.TryTrigger(location, 1, true, out LocationEventDefinition triggeredEvent);

        Assert.True(result);
        Assert.Same(second, triggeredEvent);
    }

    [Fact]
    public void EventChoiceDefinition_RequiresActionIdToMatchChoiceId()
    {
        ActionDefinition action = new ActionDefinition("action-id", "Action", 0);

        Assert.Throws<ArgumentException>(() => new LocationEventChoiceDefinition("different-id", action));
    }

    [Fact]
    public void EventChoiceDefinition_RejectsSelfReferencingEncounterAction()
    {
        ActionDefinition action = new ActionDefinition("same-id", "Action", 0);

        Assert.Throws<ArgumentException>(() => new LocationEventChoiceDefinition("same-id", action, "same-id"));
    }

    [Fact]
    public void EventService_RejectsLocationWithoutIdentity()
    {
        LocationContent location = new LocationContent(
            null,
            "Location",
            "REGION",
            "Description",
            Array.Empty<string>(),
            Array.Empty<ActionDefinition>());
        LocationEventService service = new LocationEventService(new Random(1));

        Assert.Throws<ArgumentException>(() => service.TryTrigger(location, 1, true, out _));
    }

    [Fact]
    public void EventService_RejectsInvalidArguments()
    {
        LocationEventService service = new LocationEventService(new Random(1));
        LocationContent location = CreateLocation("courtyard", CreateEvent("guaranteed", 1));

        Assert.Throws<ArgumentNullException>(() => service.TryTrigger(null, 1, true, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => service.TryTrigger(location, 0, true, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => service.TryTrigger(location, -1, true, out _));
        Assert.Throws<ArgumentNullException>(() => new LocationEventService(null));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void EventDefinition_RejectsChanceOutsideZeroToOne(double chance)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateEvent("invalid", chance));
    }

    [Fact]
    public void EventDefinition_RejectsMissingOrDuplicateChoices()
    {
        ActionDefinition action = new ActionDefinition("choice", "Choice", 0);

        Assert.Throws<ArgumentException>(() => new LocationEventDefinition(
            "event", "Event", "REGION", "Description", 1, null));
        Assert.Throws<ArgumentException>(() => new LocationEventDefinition(
            "event", "Event", "REGION", "Description", 1, Array.Empty<LocationEventChoiceDefinition>()));
        Assert.Throws<ArgumentException>(() => new LocationEventDefinition(
            "event",
            "Event",
            "REGION",
            "Description",
            1,
            new[]
            {
                new LocationEventChoiceDefinition("duplicate", action),
                new LocationEventChoiceDefinition("duplicate", action)
            }));
    }

    [Fact]
    public void TravelToApartmentBlock_TriggersEvent_AndListeningAppliesTime()
    {
        LoadLocationCatalog();
        Character player = TestCharacterFactory.CreatePlayer(agility: 30);
        GameClock clock = new GameClock();
        ActionExecutionService actionService = CreateActionService();

        bool travelled = actionService.TryExecute(
            _locations.Travel,
            player,
            clock,
            out string travelReason,
            out int travelDuration);

        Assert.True(travelled, travelReason);
        Assert.Equal(9, travelDuration);
        Assert.Equal(1.08, player.Fatigue, 6);
        Assert.Equal(8, player.Stamina);

        LocationContent apartmentBlock = WithGuaranteedEvents(_locations.Get("apartment_block"));
        LocationEventService eventService = new LocationEventService(new Random(1));
        bool triggered = eventService.TryTrigger(apartmentBlock, clock.Day, player.CanAct, out LocationEventDefinition locationEvent);

        Assert.True(triggered);
        Assert.NotNull(locationEvent);

        LocationEventChoiceDefinition listenChoice = FindChoice(locationEvent, "listen_from_hallway");
        bool listened = actionService.TryExecute(
            listenChoice.Action,
            player,
            clock,
            out string listenReason,
            out int listenDuration);

        Assert.True(listened, listenReason);
        Assert.Equal(10, listenDuration);
        Assert.Equal(1.7466666667, player.Fatigue, 6);
        Assert.Equal(8, player.Stamina);
        Assert.Equal(469, clock.MinutesOfDay);
        Assert.True(player.CanAct);
    }

    [Fact]
    public void EnterChoiceAppliesItsAuthoredCostWithoutForcingCombat()
    {
        LoadLocationCatalog();
        LocationContent apartmentBlock = WithGuaranteedEvents(_locations.Get("apartment_block"));
        LocationEventService eventService = new LocationEventService(new Random(1));
        Assert.True(eventService.TryTrigger(apartmentBlock, 1, true, out LocationEventDefinition locationEvent));

        LocationEventChoiceDefinition enterChoice = FindChoice(locationEvent, "enter_unit_213");
        Assert.Null(enterChoice.EncounterActionId);

        Character player = TestCharacterFactory.CreatePlayer();
        GameClock clock = new GameClock();
        ActionExecutionService actionService = CreateActionService();
        Assert.True(actionService.TryExecute(enterChoice.Action, player, clock, out string reason, out int duration), reason);
        Assert.Equal(5, duration);
    }

    [Fact]
    public void HighFatigueAndZeroStamina_DoNotBlockEventChoice()
    {
        LoadLocationCatalog();
        LocationEventDefinition locationEvent = _locations.Get("apartment_block").Events[0];
        LocationEventChoiceDefinition listenChoice = FindChoice(locationEvent, "listen_from_hallway");
        Character player = TestCharacterFactory.CreatePlayer(fatigue: 100, stamina: 0);
        GameClock clock = new GameClock();
        ActionExecutionService actionService = CreateActionService();

        bool result = actionService.TryExecute(
            listenChoice.Action,
            player,
            clock,
            out string reason,
            out int duration);

        Assert.True(result, reason);
        Assert.Empty(reason);
        Assert.Equal(10, duration);
        Assert.Equal(100, player.Fatigue);
        Assert.Equal(0, player.Stamina);
        Assert.Equal(460, clock.MinutesOfDay);
    }

    [Fact]
    public void ContentLoader_RejectsEventChoiceWithUnknownEncounterAction()
    {
        string json = CreateContentJson(
            "[{ \"id\": \"event\", \"title\": \"Event\", \"region\": \"REGION\", \"description\": \"Description\", \"chance\": 1, \"choices\": [{ \"id\": \"choice\", \"label\": \"Choice\", \"durationMinutes\": 0, \"encounterActionId\": \"missing\" }] }]");

        Assert.Throws<InvalidOperationException>(() => LocationDataLoader.LoadJson(json));
    }

    [Fact]
    public void ContentLoader_RejectsDuplicateEventIds()
    {
        string eventJson = "["
            + "{ \"id\": \"event\", \"title\": \"Event\", \"region\": \"REGION\", \"description\": \"Description\", \"chance\": 1, \"choices\": [{ \"id\": \"a\", \"label\": \"A\", \"durationMinutes\": 0 }] },"
            + "{ \"id\": \"event\", \"title\": \"Event 2\", \"region\": \"REGION\", \"description\": \"Description\", \"chance\": 1, \"choices\": [{ \"id\": \"b\", \"label\": \"B\", \"durationMinutes\": 0 }] }"
            + "]";
        string json = CreateContentJson(eventJson);

        Assert.Throws<InvalidOperationException>(() => LocationDataLoader.LoadJson(json));
    }

    private static LocationContent WithGuaranteedEvents(LocationContent location)
    {
        LocationEventDefinition[] guaranteedEvents = new LocationEventDefinition[location.Events.Length];

        for (int index = 0; index < location.Events.Length; index++)
        {
            LocationEventDefinition locationEvent = location.Events[index];
            guaranteedEvents[index] = new LocationEventDefinition(
                locationEvent.Id,
                locationEvent.Title,
                locationEvent.Region,
                locationEvent.Description,
                1,
                locationEvent.Choices);
        }

        return new LocationContent(
            location.Id,
            location.Name,
            location.Region,
            location.FirstVisitDescription,
            location.ReturnVisitDescription,
            location.Exits,
            location.Actions,
            guaranteedEvents);
    }

    private static ActionExecutionService CreateActionService()
    {
        return new ActionExecutionService(new ActivityTimeCalculator(), new ActionEffectExecutor());
    }

    private static LocationEventChoiceDefinition FindChoice(LocationEventDefinition locationEvent, string choiceId)
    {
        foreach (LocationEventChoiceDefinition choice in locationEvent.Choices)
        {
            if (choice.Id == choiceId)
            {
                return choice;
            }
        }

        throw new InvalidOperationException("Event choice was not found: " + choiceId);
    }

    private static ActionDefinition FindAction(LocationContent location, string actionId)
    {
        foreach (ActionDefinition action in location.Actions)
        {
            if (action.Id == actionId)
            {
                return action;
            }
        }

        throw new InvalidOperationException("Location action was not found: " + actionId);
    }

    private static LocationEventDefinition CreateEvent(
        string id,
        double chance,
        string requiredNpcId = null)
    {
        string choiceId = id + "_choice";
        LocationEventChoiceDefinition choice = new LocationEventChoiceDefinition(
            choiceId,
            new ActionDefinition(choiceId, "Action", 0));
        return new LocationEventDefinition(
            id, "Event", "REGION", "Description", chance, new[] { choice }, requiredNpcId);
    }

    private static LocationContent CreateLocation(string id, params LocationEventDefinition[] events)
    {
        return new LocationContent(id, "Location", "REGION", "Description", Array.Empty<string>(), Array.Empty<ActionDefinition>(), events);
    }

    private static string CreateContentJson(string eventsJson)
    {
        return "{"
            + "\"locations\":[{"
            + "\"id\":\"courtyard\","
            + "\"name\":\"Courtyard\","
            + "\"region\":\"REGION\","
            + "\"description\":\"Description\","
            + "\"exits\":[],"
            + "\"actions\":[{\"id\":\"known\",\"label\":\"Known\",\"durationMinutes\":0}],"
            + "\"events\":" + eventsJson
            + "}]"
            + "}";
    }

    private void LoadLocationCatalog()
    {
        string path = System.IO.Path.Combine(System.AppContext.BaseDirectory, "Data", "Locations.json");
        _locations = LocationDataLoader.LoadJson(System.IO.File.ReadAllText(path));
    }
}
