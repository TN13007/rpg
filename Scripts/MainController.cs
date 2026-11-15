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
    private GameModeController _gameModeController;
    private LocationEventService _locationEventService;

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
        LocationContent location = AcademyContent.Get(_currentLocationId);
        List<ActionDefinition> choices = new List<ActionDefinition>();
        foreach (ActionDefinition action in location.Actions)
        {
            if (action.Id != "encounter_goblin")
            {
                choices.Add(action);
            }
        }

        foreach (string exitId in location.Exits)
        {
            LocationContent exit = AcademyContent.Get(exitId);
            choices.Add(new ActionDefinition("travel:" + exitId, "Travel to " + exit.Name, 0));
        }

        _gameModeController.ShowExploration();
        _storyPanel.ShowContent(
            location.Name,
            location.Region,
            location.Description,
            choices,
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
        _storyPanel.ShowContent(tab, "ADVENTURER RECORD", description, Array.Empty<ActionDefinition>(), _dayClock.GetDisplayText());
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

        if (selectedAction.Id == "encounter_goblin")
        {
            StartGoblinEncounter(selectedAction);
            return;
        }

        ExecuteAction(selectedAction);
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

    private void ExecuteAction(ActionDefinition action)
    {
        if (!_actionExecutionService.TryExecute(
            action,
            _player,
            _dayClock,
            out string failureReason,
            out int actualDuration))
        {
            _storyPanel.ShowFeedback(failureReason);
            return;
        }

        _characterPanel.SetCharacter(new CharacterViewModel(_player));
        ShowLocation();

        string feedback = action.Feedback;
        if (actualDuration > 0)
        {
            feedback += " (" + actualDuration + " minutes)";
        }

        if (_player.IsUnconscious)
        {
            feedback += " You collapse from exhaustion.";
        }

        _storyPanel.ShowFeedback(feedback);
    }

    private void StartGoblinEncounter(ActionDefinition action)
    {
        if (!_actionExecutionService.CanExecute(action, _player, out string failureReason))
        {
            _storyPanel.ShowFeedback(failureReason);
            return;
        }

        if (!_actionExecutionService.TryExecute(
            action,
            _player,
            _dayClock,
            out failureReason,
            out _))
        {
            _storyPanel.ShowFeedback(failureReason);
            return;
        }

        _characterPanel.SetCharacter(new CharacterViewModel(_player));

        if (_player.IsUnconscious)
        {
            _storyPanel.ShowFeedback("You collapse from exhaustion before the encounter begins.");
            return;
        }

        if (!_combatService.TryStartEncounter(out failureReason))
        {
            _player.RestoreEnergy(action.EnergyCost);
            _characterPanel.SetCharacter(new CharacterViewModel(_player));
            _storyPanel.ShowFeedback(failureReason);
            return;
        }

        _characterPanel.SetCharacter(new CharacterViewModel(_player));
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
    }

    private void TravelTo(string destinationId)
    {
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
            out int actualDuration))
        {
            _storyPanel.ShowFeedback(failureReason);
            return;
        }

        _currentLocationId = destinationId;
        _characterPanel.SetCharacter(new CharacterViewModel(_player));
        ShowLocation();
        string feedback = "You arrive at " + AcademyContent.Get(destinationId).Name + ". Travel took " + actualDuration + " minutes.";

        if (_player.IsUnconscious)
        {
            feedback += " You collapse from exhaustion.";
        }

        _storyPanel.ShowFeedback(feedback);
        TryTriggerLocationEvent();
    }

    private void TryTriggerLocationEvent()
    {
        if (_currentLocationId != "courtyard")
        {
            return;
        }

        if (!_locationEventService.TryTrigger("courtyard_goblin_ambush", _dayClock.Day, 0.5))
        {
            return;
        }

        ActionDefinition eventAction = FindChoice(AcademyContent.Get(_currentLocationId).Actions, "encounter_goblin");
        if (eventAction == null)
        {
            return;
        }

        _storyPanel.ShowFeedback("You hear movement near the courtyard gate...");
        StartGoblinEncounter(eventAction);
    }
}
