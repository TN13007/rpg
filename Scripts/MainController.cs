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

    private string _currentLocationId = "courtyard";

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
        List<StoryChoice> choices = new List<StoryChoice>(location.Actions);

        foreach (string exitId in location.Exits)
        {
            LocationContent exit = AcademyContent.Get(exitId);
            choices.Add(new StoryChoice("travel:" + exitId, "Travel to " + exit.Name));
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
            "Profile" => "A summary of your adventurer and current condition.",
            "Skills" => "Techniques available to a novice adventurer.",
            "Inventory" => "Items and equipment carried by your character.",
            "Relationships" => "People you have met during your time at the academy.",
            "Goals" => "Your current assignments and short-term objectives.",
            "Journal" => "A record of meaningful events from your academy life.",
            _ => "Prototype preferences and game settings."
        };

        _storyPanel.Visible = true;
        _combatPanel.Visible = false;
        _storyPanel.ShowContent(tab, "ADVENTURER RECORD", description, Array.Empty<StoryChoice>(), _dayClock.GetDisplayText());
        _storyPanel.ShowLocationButton(true);
    }

    private void HandleChoice(string choiceId)
    {
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

        string message = choiceId switch
        {
            "talk_mira" => "Mira smiles. “Morning! Are you heading to practical training too?”",
            "inspect_board" => "The notice board lists today's classes and the training-dungeon briefing.",
            "talk_instructor" => "The instructor asks you to report to the Guild Hall for your first assignment.",
            "inspect_maps" => "The field maps show the academy grounds and a small supervised dungeon.",
            "talk_clerk" => "The clerk explains how expedition reports are submitted.",
            "inspect_requests" => "Most requests are reserved for senior students. One training survey is available.",
            "inspect_letter" => "A short note from the academy reminds you to attend the morning briefing.",
            "rest" => "You sit by the window for a moment. The day can wait another minute.",
            _ => "There is nothing more to do here right now."
        };

        AdvanceForAction(choiceId, message);
    }

    private void AdvanceForAction(string choiceId, string feedback)
    {
        LocationContent location = AcademyContent.Get(_currentLocationId);

        foreach (StoryChoice action in location.Actions)
        {
            if (action.Id != choiceId)
            {
                continue;
            }

            _dayClock.AdvanceMinutes(action.DurationMinutes);
            ShowLocation();
            _storyPanel.ShowFeedback(feedback);
            return;
        }
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
        _combatService.StartEncounter();
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

        _dayClock.AdvanceMinutes(30);
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

        _dayClock.AdvanceMinutes(10);
        _currentLocationId = destinationId;
        ShowLocation();
    }
}
