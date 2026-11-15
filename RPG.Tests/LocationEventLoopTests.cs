using System;
using Xunit;

public sealed class LocationEventLoopTests
{
    [Fact]
    public void AcademyContent_LoadsCourtyardEventAndItsChoices()
    {
        LoadAcademyContent();

        LocationContent courtyard = AcademyContent.Get("courtyard");

        Assert.Single(courtyard.Events);
        Assert.Equal("courtyard_goblin_ambush", courtyard.Events[0].Id);
        Assert.Equal(0.5, courtyard.Events[0].Chance);
        Assert.Equal(2, courtyard.Events[0].Choices.Length);
        Assert.Equal("encounter_goblin", courtyard.Events[0].Choices[0].EncounterActionId);
        Assert.Null(courtyard.Events[1].EncounterActionId);
        Assert.Equal(10, courtyard.Events[1].Action.DurationMinutes);
        Assert.Equal(1, courtyard.Events[1].Action.EnergyCost);
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
        Character player = TestCharacterFactory.CreatePlayer(energy: 8, agility: 30);
        DayClock clock = new DayClock();
        ActionExecutionService actionService = CreateActionService();

        bool travelled = actionService.TryExecute(
            AcademyContent.Travel,
            player,
            clock,
            out string travelReason,
            out int travelDuration);

        Assert.True(travelled, travelReason);
        Assert.Equal(9, travelDuration);
        Assert.Equal(7, player.Energy);

        LocationContent courtyard = AcademyContent.Get("courtyard");
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
        Assert.Equal(6, player.Energy);
        Assert.Equal(469, clock.MinutesOfDay);
        Assert.True(player.CanAct);
    }

    [Fact]
    public void ConfrontChoice_SpendsInvestigationCostAndStartsCombat()
    {
        LoadAcademyContent();
        Character player = TestCharacterFactory.CreatePlayer(energy: 8, agility: 30);
        Character enemy = TestCharacterFactory.CreateEnemy();
        DayClock clock = new DayClock();
        ActionExecutionService actionService = CreateActionService();

        Assert.True(actionService.TryExecute(AcademyContent.Travel, player, clock, out string travelReason, out _), travelReason);

        LocationContent courtyard = AcademyContent.Get("courtyard");
        LocationEventService eventService = new LocationEventService(new Random(1));
        Assert.True(eventService.TryTrigger(courtyard, clock.Day, player.CanAct, out LocationEventDefinition locationEvent));

        LocationEventChoiceDefinition confrontChoice = FindChoice(locationEvent, "event_goblin_attack");
        Assert.Equal("encounter_goblin", confrontChoice.EncounterActionId);

        ActionDefinition encounterAction = FindAction(courtyard, confrontChoice.EncounterActionId);
        Assert.True(actionService.TryExecute(encounterAction, player, clock, out string actionReason, out _), actionReason);
        Assert.Equal(5, player.Energy);

        CombatService combat = new CombatService(player, enemy);
        Assert.True(combat.TryStartEncounter(out string combatReason), combatReason);
        Assert.Equal(40, enemy.Health);
        Assert.True(player.CanAct);
    }

    [Fact]
    public void InsufficientEnergyForEventChoice_DoesNotAdvanceTimeOrSpendEnergy()
    {
        LoadAcademyContent();
        LocationEventDefinition locationEvent = AcademyContent.Get("courtyard").Events[0];
        LocationEventChoiceDefinition sneakChoice = FindChoice(locationEvent, "event_goblin_sneak");
        Character player = TestCharacterFactory.CreatePlayer(energy: 0);
        DayClock clock = new DayClock();
        ActionExecutionService actionService = CreateActionService();

        bool result = actionService.TryExecute(
            sneakChoice.Action,
            player,
            clock,
            out string reason,
            out int duration);

        Assert.False(result);
        Assert.NotEmpty(reason);
        Assert.Equal(0, duration);
        Assert.Equal(0, player.Energy);
        Assert.Equal(450, clock.MinutesOfDay);
    }

    [Fact]
    public void ContentLoader_RejectsEventChoiceWithUnknownEncounterAction()
    {
        string json = CreateContentJson(
            "[{ \"id\": \"event\", \"title\": \"Event\", \"region\": \"REGION\", \"description\": \"Description\", \"chance\": 1, \"choices\": [{ \"id\": \"choice\", \"label\": \"Choice\", \"durationMinutes\": 0, \"encounterActionId\": \"missing\" }] }]");

        Assert.Throws<InvalidOperationException>(() => AcademyContent.LoadJson(json));
    }

    [Fact]
    public void ContentLoader_RejectsDuplicateEventIds()
    {
        string eventJson = "["
            + "{ \"id\": \"event\", \"title\": \"Event\", \"region\": \"REGION\", \"description\": \"Description\", \"chance\": 1, \"choices\": [{ \"id\": \"a\", \"label\": \"A\", \"durationMinutes\": 0 }] },"
            + "{ \"id\": \"event\", \"title\": \"Event 2\", \"region\": \"REGION\", \"description\": \"Description\", \"chance\": 1, \"choices\": [{ \"id\": \"b\", \"label\": \"B\", \"durationMinutes\": 0 }] }"
            + "]";
        string json = CreateContentJson(eventJson);

        Assert.Throws<InvalidOperationException>(() => AcademyContent.LoadJson(json));
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
        LocationEventChoiceDefinition choice = new LocationEventChoiceDefinition(
            id + "_choice",
            new ActionDefinition(id + "_action", "Action", 0));
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

    private static void LoadAcademyContent()
    {
        string path = System.IO.Path.Combine(System.AppContext.BaseDirectory, "Data", "academy_locations.json");
        AcademyContent.LoadJson(System.IO.File.ReadAllText(path));
    }
}
