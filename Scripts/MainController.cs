using System;
using System.Collections.Generic;
using Godot;

/*
 * MainController is the application-level orchestrator.
 * It connects UI input to the relevant game systems and refreshes presentation.
 * It does not implement action costs, effects, time rules, combat rules, or mode visibility.
 */
public partial class MainController : Control
{
    private Sidebar _sidebar;
    private StoryPanel _storyPanel;
    private CombatPanel _combatPanel;
    private CharacterPanel _characterPanel;
    private Character _player;
    private Character _goblin;
    private CombatService _combatService;
    private CombatPresentationMapper _combatMapper;
    private DayClock _dayClock;
    private ActivityTimeCalculator _activityTimeCalculator;
    private ActionExecutionService _actionExecutionService;
    private ActionDurationEstimateMapper _durationEstimateMapper;
    private GameModeController _gameModeController;
    private LocationEventService _locationEventService;
    private LocationEventDefinition _activeLocationEvent;
    private int _combatStartDay;
    private int _combatStartMinutes;
    private bool _hasCombatTimeFeedback;

    private string _currentLocationId = "dormitory";

    public override void _Ready()
    {
        _sidebar = GetNode<Sidebar>("SafeArea/Layout/Sidebar");
        _storyPanel = GetNode<StoryPanel>("SafeArea/Layout/StoryPanel");
        _combatPanel = GetNode<CombatPanel>("SafeArea/Layout/CombatPanel");
        _characterPanel = GetNode<CharacterPanel>("SafeArea/Layout/CharacterPanel");

        AcademyContent.Load("res://Data/academy_locations.json");

        CharacterData playerData = CharacterDataLoader.Load("res://Data/Characters/player.json");
        CharacterData goblinData = CharacterDataLoader.Load("res://Data/Characters/goblin.json");

        _player = new Character(playerData);
        _goblin = new Character(goblinData);
        _combatService = new CombatService(_player, _goblin);
        _combatMapper = new CombatPresentationMapper();
        _dayClock = new DayClock();
        _activityTimeCalculator = new ActivityTimeCalculator();
        _actionExecutionService = new ActionExecutionService(
            _activityTimeCalculator,
            new ActionEffectExecutor());
        _durationEstimateMapper = new ActionDurationEstimateMapper(_activityTimeCalculator);
        _gameModeController = new GameModeController(_storyPanel, _combatPanel);
        _locationEventService = new LocationEventService();

        _sidebar.TabSelected += OpenTab;
        _storyPanel.ChoiceSelected += HandleChoice;
        _storyPanel.ReturnToLocationRequested += ShowLocation;
        _combatPanel.ActionSelected += HandleCombatAction;
        _combatPanel.ContinueRequested += ReturnToExploration;

        _characterPanel.SetCharacter(new CharacterViewModel(_player));
        ShowLocation();
    }

    private void OpenTab(string tab)
    {
        if (_gameModeController.IsCombatMode)
        {
            return;
        }

        ShowPage(tab);
    }

    private void ShowLocation()
    {
        if (_activeLocationEvent != null)
        {
            ShowLocationEvent(_activeLocationEvent);
            return;
        }

        LocationContent location = AcademyContent.Get(_currentLocationId);
        List<ActionDefinition> choices = new List<ActionDefinition>();
        foreach (ActionDefinition action in location.Actions)
        {
            if (!IsEventEncounterAction(location, action.Id))
            {
                choices.Add(action);
            }
        }

        foreach (string exitId in location.Exits)
        {
            LocationContent exit = AcademyContent.Get(exitId);
            ActionDefinition travel = AcademyContent.Travel;
            choices.Add(new ActionDefinition(
                "travel:" + exitId,
                "Travel to " + exit.Name,
                travel.DurationMinutes,
                travel.TimeModifier,
                travel.Feedback,
                travel.EnergyCost,
                travel.Effects,
                travel.TimeMode,
                travel.EstimatedDurationMinMinutes,
                travel.EstimatedDurationMaxMinutes));
        }

        _gameModeController.ShowExploration();
        _storyPanel.ShowContent(
            location.Name,
            location.Region,
            location.Description,
            MapActionChoices(choices),
            _dayClock.GetDisplayText());
        _storyPanel.ShowLocationButton(false);
    }

    private void ShowPage(string tab)
    {
        string description = tab switch
        {
            "Profile" => GetProfileDescription(),
            "Skills" => "Techniques available to a novice adventurer.",
            "Inventory" => "Items and equipment carried by your character.",
            "Relationships" => "People you have met during your time at the academy.",
            "Goals" => "Your current assignments and short-term objectives.",
            "Journal" => "A record of meaningful events from your academy life.",
            _ => "Prototype preferences and game settings."
        };

        _gameModeController.ShowExploration();
        _storyPanel.ShowContent(tab, "ADVENTURER RECORD", description, Array.Empty<ActionChoiceViewData>(), _dayClock.GetDisplayText());
        _storyPanel.ShowLocationButton(true);
    }

    private string GetProfileDescription()
    {
        CharacterStats stats = _player.Stats;

        return "A summary of your adventurer and current condition."
            + "\n\nATTRIBUTES"
            + "\nStrength: " + stats.Strength
            + "\nAgility: " + stats.Agility
            + "\nIntelligence: " + stats.Intelligence
            + "\nEndurance: " + stats.Endurance
            + "\nWisdom: " + stats.Wisdom;
    }

    private void HandleChoice(string choiceId)
    {
        if (_activeLocationEvent != null)
        {
            HandleLocationEventChoice(choiceId);
            return;
        }

        if (choiceId.StartsWith("travel:", StringComparison.Ordinal))
        {
            TravelTo(choiceId.Substring("travel:".Length));
            return;
        }

        LocationContent location = AcademyContent.Get(_currentLocationId);
        ActionDefinition selectedAction = FindChoice(location.Actions, choiceId);

        if (selectedAction == null)
        {
            _storyPanel.ShowFeedback("That action is not available here.");
            return;
        }

        ExecuteAction(selectedAction);
    }

    private bool IsEventEncounterAction(LocationContent location, string actionId)
    {
        foreach (LocationEventDefinition locationEvent in location.Events)
        {
            foreach (LocationEventChoiceDefinition choice in locationEvent.Choices)
            {
                if (choice.EncounterActionId == actionId)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private List<ActionChoiceViewData> MapActionChoices(IReadOnlyList<ActionDefinition> actions)
    {
        List<ActionChoiceViewData> choices = new List<ActionChoiceViewData>();

        foreach (ActionDefinition action in actions)
        {
            string durationLabel = _durationEstimateMapper.Map(action, _player);
            choices.Add(new ActionChoiceViewData(action.Id, action.Label, durationLabel));
        }

        return choices;
    }

    private ActionDefinition FindChoice(ActionDefinition[] choices, string choiceId)
    {
        foreach (ActionDefinition choice in choices)
        {
            if (choice.Id == choiceId)
            {
                return choice;
            }
        }

        return null;
    }

    private bool ExecuteAction(ActionDefinition action)
    {
        int startDay = _dayClock.Day;
        int startMinutes = _dayClock.MinutesOfDay;

        if (!_actionExecutionService.TryExecute(
            action,
            _player,
            _dayClock,
            out string failureReason,
            out _))
        {
            _storyPanel.ShowFeedback(failureReason);
            return false;
        }

        _characterPanel.SetCharacter(new CharacterViewModel(_player));
        ShowLocation();
        _storyPanel.ShowTimeAdvanceFeedback(
            startDay,
            startMinutes,
            _dayClock.Day,
            _dayClock.MinutesOfDay);

        string feedback = action.Feedback;

        if (_player.IsUnconscious)
        {
            feedback += " You collapse from exhaustion.";
        }

        _storyPanel.ShowFeedback(feedback);
        return true;
    }

    private bool StartGoblinEncounter(ActionDefinition action)
    {
        if (!_actionExecutionService.CanExecute(action, _player, out string failureReason))
        {
            _storyPanel.ShowFeedback(failureReason);
            return false;
        }

        if (_player.Energy <= action.EnergyCost)
        {
            _storyPanel.ShowFeedback("You need to keep at least 1 Energy to enter combat.");
            return false;
        }

        int startDay = _dayClock.Day;
        int startMinutes = _dayClock.MinutesOfDay;

        if (!_actionExecutionService.TryExecute(
            action,
            _player,
            _dayClock,
            out failureReason,
            out _))
        {
            _storyPanel.ShowFeedback(failureReason);
            return false;
        }

        _characterPanel.SetCharacter(new CharacterViewModel(_player));

        if (_player.IsUnconscious)
        {
            _storyPanel.ShowFeedback("You collapse from exhaustion before the encounter begins.");
            return false;
        }

        if (!_combatService.TryStartEncounter(out failureReason))
        {
            _player.RestoreEnergy(action.EnergyCost);
            _characterPanel.SetCharacter(new CharacterViewModel(_player));
            _storyPanel.ShowFeedback(failureReason);
            return false;
        }

        _characterPanel.SetCharacter(new CharacterViewModel(_player));
        _combatStartDay = startDay;
        _combatStartMinutes = startMinutes;
        _hasCombatTimeFeedback = true;
        _activeLocationEvent = null;
        _gameModeController.ShowCombat();
        _combatPanel.BeginEncounter("Goblin Ambush");
        _combatPanel.AppendMessage(new CombatMessage(
            "goblin-encounter-start",
            1,
            CombatMessageType.System,
            CombatMessageCategory.Environment,
            "SYSTEM",
            "A goblin jumps out from the bushes and blocks your way."));
        _combatPanel.ApplyViewData(_combatMapper.Map(_combatService.GetSnapshot()));
        return true;
    }

    private void HandleCombatAction(CombatActionRequest request)
    {
        if (_combatService.IsEncounterOver)
        {
            return;
        }

        CombatResult result = _combatService.ExecuteAction(request);
        _combatPanel.ApplyViewData(_combatMapper.Map(result));
        _characterPanel.SetCharacter(new CharacterViewModel(_player));

        if (!_combatService.IsEncounterOver)
        {
            return;
        }

        int encounterDuration = _activityTimeCalculator.Calculate(AcademyContent.CombatEncounter, _player);
        _dayClock.AdvanceMinutes(encounterDuration);
        _combatPanel.ShowEncounterComplete(_combatService.IsVictory ? "VICTORY" : "DEFEAT");
    }

    private void ReturnToExploration()
    {
        _gameModeController.ShowExploration();
        _characterPanel.SetCharacter(new CharacterViewModel(_player));
        ShowLocation();

        if (_hasCombatTimeFeedback)
        {
            _storyPanel.ShowTimeAdvanceFeedback(
                _combatStartDay,
                _combatStartMinutes,
                _dayClock.Day,
                _dayClock.MinutesOfDay);
            _hasCombatTimeFeedback = false;
        }
    }

    private void TravelTo(string destinationId)
    {
        int startDay = _dayClock.Day;
        int startMinutes = _dayClock.MinutesOfDay;
        LocationContent current = AcademyContent.Get(_currentLocationId);
        if (Array.IndexOf(current.Exits, destinationId) < 0)
        {
            _storyPanel.ShowFeedback("That location is not connected to your current position.");
            return;
        }

        if (!_actionExecutionService.TryExecute(
            AcademyContent.Travel,
            _player,
            _dayClock,
            out string failureReason,
            out _))
        {
            _storyPanel.ShowFeedback(failureReason);
            return;
        }

        _activeLocationEvent = null;
        _currentLocationId = destinationId;
        _characterPanel.SetCharacter(new CharacterViewModel(_player));
        ShowLocation();
        _storyPanel.ShowTimeAdvanceFeedback(
            startDay,
            startMinutes,
            _dayClock.Day,
            _dayClock.MinutesOfDay);
        string feedback = "You arrive at " + AcademyContent.Get(destinationId).Name + ".";

        if (_player.IsUnconscious)
        {
            feedback += " You collapse from exhaustion.";
        }

        _storyPanel.ShowFeedback(feedback);
        TryTriggerLocationEvent();
    }

    private void TryTriggerLocationEvent()
    {
        LocationContent location = AcademyContent.Get(_currentLocationId);

        if (!_locationEventService.TryTrigger(
            location,
            _dayClock.Day,
            _player.CanAct,
            out LocationEventDefinition triggeredEvent))
        {
            return;
        }

        _activeLocationEvent = triggeredEvent;
        ShowLocationEvent(triggeredEvent);
    }

    private void ShowLocationEvent(LocationEventDefinition locationEvent)
    {
        List<ActionDefinition> choices = new List<ActionDefinition>();

        foreach (LocationEventChoiceDefinition choice in locationEvent.Choices)
        {
            choices.Add(choice.Action);
        }

        _gameModeController.ShowExploration();
        _storyPanel.ShowContent(
            locationEvent.Title,
            locationEvent.Region,
            locationEvent.Description,
            MapActionChoices(choices),
            _dayClock.GetDisplayText());
        _storyPanel.ShowLocationButton(false);
    }

    private void HandleLocationEventChoice(string choiceId)
    {
        LocationEventChoiceDefinition selectedChoice = null;

        foreach (LocationEventChoiceDefinition choice in _activeLocationEvent.Choices)
        {
            if (choice.Action.Id == choiceId)
            {
                selectedChoice = choice;
                break;
            }
        }

        if (selectedChoice == null)
        {
            _storyPanel.ShowFeedback("That event choice is not available.");
            return;
        }

        if (selectedChoice.EncounterActionId != null)
        {
            ActionDefinition encounterAction = FindChoice(
                AcademyContent.Get(_currentLocationId).Actions,
                selectedChoice.EncounterActionId);

            if (encounterAction == null)
            {
                _storyPanel.ShowFeedback("The encounter action for this event is unavailable.");
                return;
            }

            StartGoblinEncounter(encounterAction);
            return;
        }

        LocationEventDefinition previousEvent = _activeLocationEvent;
        _activeLocationEvent = null;

        if (!ExecuteAction(selectedChoice.Action))
        {
            _activeLocationEvent = previousEvent;
            ShowLocationEvent(previousEvent);
        }
    }
}
