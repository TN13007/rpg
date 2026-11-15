using System;
using System.Collections.Generic;
using Godot;

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
        if (_combatPanel.Visible)
        {
            return;
        }

        ShowPage(tab);
    }

    private void ShowLocation()
    {
        LocationContent location = AcademyContent.Get(_currentLocationId);
        List<ActionDefinition> choices = new List<ActionDefinition>(location.Actions);

        foreach (string exitId in location.Exits)
        {
            LocationContent exit = AcademyContent.Get(exitId);
            choices.Add(new ActionDefinition("travel:" + exitId, "Travel to " + exit.Name, 0));
        }

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

        _storyPanel.Visible = true;
        _combatPanel.Visible = false;
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
        if (!_player.IsAlive)
        {
            _storyPanel.ShowFeedback("You are dead and cannot perform actions.");
            return;
        }

        if (choiceId.StartsWith("travel:", StringComparison.Ordinal))
        {
            TravelTo(choiceId.Substring("travel:".Length));
            return;
        }

        if (choiceId == "sleep")
        {
            Sleep();
            return;
        }

        if (choiceId == "encounter_goblin")
        {
            StartGoblinEncounter();
            return;
        }

        LocationContent location = AcademyContent.Get(_currentLocationId);
        ActionDefinition selectedChoice = FindChoice(location.Actions, choiceId);

        if (selectedChoice == null)
        {
            _storyPanel.ShowFeedback("That action is not available here.");
            return;
        }

        ExecuteAction(selectedChoice);
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

    private void ExecuteAction(ActionDefinition choice)
    {
        int actualDuration = _activityTimeCalculator.Calculate(choice, _player);
        _dayClock.AdvanceMinutes(actualDuration);
        ShowLocation();

        string feedback = choice.Feedback;
        if (actualDuration > 0)
        {
            feedback += " (" + actualDuration + " minutes)";
        }

        _storyPanel.ShowFeedback(feedback);
    }

    private void Sleep()
    {
        if (_currentLocationId != "dormitory")
        {
            _storyPanel.ShowFeedback("You need to return to your dormitory before sleeping.");
            return;
        }

        _dayClock.SleepUntilNextDay();
        ShowLocation();
        _storyPanel.ShowFeedback("You wake up after a night's rest. Another day at the academy begins.");
    }

    private void StartGoblinEncounter()
    {
        if (!_combatService.TryStartEncounter(out string failureReason))
        {
            _storyPanel.ShowFeedback(failureReason);
            return;
        }

        _storyPanel.Visible = false;
        _combatPanel.Visible = true;
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
        _combatPanel.Visible = false;
        _storyPanel.Visible = true;
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

        int actualDuration = _activityTimeCalculator.Calculate(AcademyContent.Travel, _player);
        _dayClock.AdvanceMinutes(actualDuration);
        _currentLocationId = destinationId;
        ShowLocation();
        _storyPanel.ShowFeedback("You arrive at " + AcademyContent.Get(destinationId).Name + ". Travel took " + actualDuration + " minutes.");
    }
}
