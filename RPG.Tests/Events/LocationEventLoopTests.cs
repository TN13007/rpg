using System;
using Xunit;

public sealed class LocationEventLoopTests
{
    private readonly AcademyContent _academy = new AcademyContent();

    [Fact]
    public void AcademyContent_LoadsFirstAndReturnVisitDescriptions()
    {
        const string json = @"{""locations"":[{""id"":""gate"",""name"":""Gate"",""region"":""REGION"",""description"":{""firstVisit"":""A long first introduction."",""returnVisit"":""A short familiar description.""},""exits"":[],""actions"":[]}]}";

        _academy.LoadJson(json);

        LocationContent location = _academy.Get("gate");
        Assert.Equal("A long first introduction.", location.GetDescription(true));
        Assert.Equal("A short familiar description.", location.GetDescription(false));
    }

    [Fact]
    public void AcademyContent_LegacyStringDescriptionRemainsCompatible()
    {
        const string json = @"{""locations"":[{""id"":""gate"",""name"":""Gate"",""region"":""REGION"",""description"":""The old single description."",""exits"":[],""actions"":[]}]}";

        _academy.LoadJson(json);

        LocationContent location = _academy.Get("gate");
        Assert.Equal("The old single description.", location.GetDescription(true));
        Assert.Equal("The old single description.", location.GetDescription(false));
    }

    [Fact]
    public void AcademyContent_LoadsCourtyardEventAndItsChoices()
    {
        LoadAcademyContent();

        LocationContent courtyard = _academy.Get("courtyard");

        Assert.Single(courtyard.Events);
        Assert.Equal("courtyard_goblin_ambush", courtyard.Events[0].Id);
        Assert.Equal(0, courtyard.Events[0].Chance);
        Assert.Equal(2, courtyard.Events[0].Choices.Length);
        Assert.Equal("encounter_goblin", courtyard.Events[0].Choices[0].EncounterActionId);
        Assert.Null(courtyard.Events[0].Choices[1].EncounterActionId);
        Assert.Equal(10, courtyard.Events[0].Choices[1].Action.DurationMinutes);
        Assert.Equal(2.0, courtyard.Events[0].Choices[1].Action.ActivityMet);
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
    public void TravelToCourtyard_TriggersEvent_AndSneakingAppliesCostAndTime()
    {
        LoadAcademyContent();
        Character player = TestCharacterFactory.CreatePlayer(agility: 30);
        GameClock clock = new GameClock();
        ActionExecutionService actionService = CreateActionService();

        bool travelled = actionService.TryExecute(
            _academy.Travel,
            player,
            clock,
            out string travelReason,
            out int travelDuration);

        Assert.True(travelled, travelReason);
        Assert.Equal(9, travelDuration);
        Assert.Equal(1.08, player.Fatigue, 6);
        Assert.Equal(8, player.Stamina);

        LocationContent courtyard = WithGuaranteedEvents(_academy.Get("courtyard"));
        LocationEventService eventService = new LocationEventService(new Random(1));
        bool triggered = eventService.TryTrigger(courtyard, clock.Day, player.CanAct, out LocationEventDefinition locationEvent);

        Assert.True(triggered);
        Assert.NotNull(locationEvent);

        LocationEventChoiceDefinition sneakChoice = FindChoice(locationEvent, "event_goblin_sneak");
        bool sneaked = actionService.TryExecute(
            sneakChoice.Action,
            player,
            clock,
            out string sneakReason,
            out int sneakDuration);

        Assert.True(sneaked, sneakReason);
        Assert.Equal(10, sneakDuration);
        Assert.Equal(1.7466666667, player.Fatigue, 6);
        Assert.Equal(8, player.Stamina);
        Assert.Equal(469, clock.MinutesOfDay);
        Assert.True(player.CanAct);
    }

    [Fact]
    public void ConfrontChoice_SpendsInvestigationCostAndStartsCombat()
    {
        LoadAcademyContent();
        Character player = TestCharacterFactory.CreatePlayer(agility: 30);
        Character enemy = TestCharacterFactory.CreateEnemy();
        GameClock clock = new GameClock();
        ActionExecutionService actionService = CreateActionService();

        Assert.True(actionService.TryExecute(_academy.Travel, player, clock, out string travelReason, out _), travelReason);

        LocationContent courtyard = WithGuaranteedEvents(_academy.Get("courtyard"));
        LocationEventService eventService = new LocationEventService(new Random(1));
        Assert.True(eventService.TryTrigger(courtyard, clock.Day, player.CanAct, out LocationEventDefinition locationEvent));

        LocationEventChoiceDefinition confrontChoice = FindChoice(locationEvent, "event_goblin_attack");
        Assert.Equal("encounter_goblin", confrontChoice.EncounterActionId);

        ActionDefinition encounterAction = FindAction(courtyard, confrontChoice.EncounterActionId);
        Assert.True(actionService.TryExecute(encounterAction, player, clock, out string actionReason, out _), actionReason);
        Assert.Equal(1.08, player.Fatigue, 6);
        Assert.Equal(8, player.Stamina);

        CombatService combat = new CombatService(player, enemy, TestCharacterFactory.CreateCombatActions());
        Assert.True(combat.TryStartEncounter(out string combatReason), combatReason);
        Assert.Equal(40, enemy.Health);
        Assert.True(player.CanAct);
    }

    [Fact]
    public void HighFatigueAndZeroStamina_DoNotBlockEventChoice()
    {
        LoadAcademyContent();
        LocationEventDefinition locationEvent = _academy.Get("courtyard").Events[0];
        LocationEventChoiceDefinition sneakChoice = FindChoice(locationEvent, "event_goblin_sneak");
        Character player = TestCharacterFactory.CreatePlayer(fatigue: 100, stamina: 0);
        GameClock clock = new GameClock();
        ActionExecutionService actionService = CreateActionService();

        bool result = actionService.TryExecute(
            sneakChoice.Action,
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

        Assert.Throws<InvalidOperationException>(() => _academy.LoadJson(json));
    }

    [Fact]
    public void ContentLoader_RejectsDuplicateEventIds()
    {
        string eventJson = "["
            + "{ \"id\": \"event\", \"title\": \"Event\", \"region\": \"REGION\", \"description\": \"Description\", \"chance\": 1, \"choices\": [{ \"id\": \"a\", \"label\": \"A\", \"durationMinutes\": 0 }] },"
            + "{ \"id\": \"event\", \"title\": \"Event 2\", \"region\": \"REGION\", \"description\": \"Description\", \"chance\": 1, \"choices\": [{ \"id\": \"b\", \"label\": \"B\", \"durationMinutes\": 0 }] }"
            + "]";
        string json = CreateContentJson(eventJson);

        Assert.Throws<InvalidOperationException>(() => _academy.LoadJson(json));
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

    private static LocationEventDefinition CreateEvent(string id, double chance)
    {
        string choiceId = id + "_choice";
        LocationEventChoiceDefinition choice = new LocationEventChoiceDefinition(
            choiceId,
            new ActionDefinition(choiceId, "Action", 0));
        return new LocationEventDefinition(id, "Event", "REGION", "Description", chance, new[] { choice });
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

    private void LoadAcademyContent()
    {
        string path = System.IO.Path.Combine(System.AppContext.BaseDirectory, "Data", "AcademyLocations.json");
        _academy.LoadJson(System.IO.File.ReadAllText(path));
    }
}
