using System;
using System.Collections.Generic;
using Godot;

public partial class MainController : Control
{
    private Sidebar _sidebar;
    private StoryPanel _storyPanel;
    private CharacterPanel _characterPanel;

    private string _currentLocationId = "courtyard";
    private int _day = 1;

    public override void _Ready()
    {
        _sidebar = GetNode<Sidebar>("SafeArea/Layout/Sidebar");
        _storyPanel = GetNode<StoryPanel>("SafeArea/Layout/StoryPanel");
        _characterPanel = GetNode<CharacterPanel>("SafeArea/Layout/CharacterPanel");

        _sidebar.TabSelected += OpenTab;
        _storyPanel.ChoiceSelected += HandleChoice;

        _characterPanel.SetCharacter("Ari", "First-year adventurer", 1, 20, 20, 8, 10, 20);
        ShowLocation();
    }

    private void OpenTab(string tab)
    {
        switch (tab)
        {
            case "Explore":
                ShowLocation();
                return;

            case "Map":
                ShowMap();
                return;

            default:
                ShowPage(tab);
                return;
        }
    }

    private void ShowLocation()
    {
        LocationContent location = AcademyContent.Get(_currentLocationId);
        var choices = new List<StoryChoice>(location.Actions);

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
            "DAY " + _day.ToString("00") + "  ·  07:30");
    }

    private void ShowMap()
    {
        var choices = new List<StoryChoice>();

        foreach (LocationContent location in AcademyContent.All)
        {
            string label = location.Name;
            if (location.Id == _currentLocationId)
            {
                label += "  ·  CURRENT";
            }

            choices.Add(new StoryChoice("map:" + location.Id, label));
        }

        _storyPanel.ShowContent(
            "Academy Map",
            "ASTERIA ACADEMY",
            "Select a location to view its scene. The map is a simple location overview for this prototype.",
            choices,
            "DAY " + _day.ToString("00") + "  ·  07:30");
    }

    private void ShowPage(string tab)
    {
        string description = tab switch
        {
            "Profile" => "A summary of your adventurer and current condition.",
            "Relationships" => "People you have met during your time at the academy.",
            "Inventory" => "Items and equipment carried by your character.",
            "Quests" => "Your current assignments and completed objectives.",
            "Skills" => "Techniques available to a novice adventurer.",
            _ => "Prototype preferences and game settings."
        };

        _storyPanel.ShowContent(
            tab,
            "ADVENTURER RECORD",
            description,
            Array.Empty<StoryChoice>(),
            "DAY " + _day.ToString("00"));
    }

    private void HandleChoice(string choiceId)
    {
        if (choiceId.StartsWith("travel:", StringComparison.Ordinal))
        {
            TravelTo(choiceId.Substring("travel:".Length), false);
            return;
        }

        if (choiceId.StartsWith("map:", StringComparison.Ordinal))
        {
            TravelTo(choiceId.Substring("map:".Length), true);
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

        _storyPanel.ShowFeedback(message);
    }

    private void TravelTo(string destinationId, bool fromMap)
    {
        LocationContent current = AcademyContent.Get(_currentLocationId);

        if (!fromMap && Array.IndexOf(current.Exits, destinationId) < 0)
        {
            _storyPanel.ShowFeedback("That location is not connected to your current position.");
            return;
        }

        _currentLocationId = destinationId;
        ShowLocation();
    }
}
