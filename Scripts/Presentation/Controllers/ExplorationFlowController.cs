using System;
using System.Collections.Generic;

public sealed class ExplorationFlowController
{
    private readonly StoryPanel _storyPanel;
    private readonly CharacterPanel _characterPanel;
    private readonly GameModeController _gameModeController;
    private readonly Character _player;
    private readonly PlayerActionDefinition[] _playerActions;
    private readonly List<LocationContent> _locations;
    private readonly Func<string, LocationContent> _getLocation;
    private readonly ActionDefinition _travelAction;
    private readonly GameClock _gameClock;
    private readonly ActionExecutionService _actionExecutionService;
    private readonly AvailableInteractionQuery _availableInteractionQuery;
    private readonly ActionChoiceMapper _actionChoiceMapper;
    private readonly LocationEventService _locationEventService;
    private readonly Func<ActionDefinition, bool> _startEncounter;

    private string _currentLocationId = "dormitory";
    private LocationEventDefinition _activeLocationEvent;

    public ExplorationFlowController(
        StoryPanel storyPanel,
        CharacterPanel characterPanel,
        GameModeController gameModeController,
        Character player,
        PlayerActionDefinition[] playerActions,
        IEnumerable<LocationContent> locations,
        Func<string, LocationContent> getLocation,
        ActionDefinition travelAction,
        GameClock gameClock,
        ActionExecutionService actionExecutionService,
        AvailableInteractionQuery availableInteractionQuery,
        ActionChoiceMapper actionChoiceMapper,
        LocationEventService locationEventService,
        Func<ActionDefinition, bool> startEncounter)
    {
        _storyPanel = storyPanel ?? throw new ArgumentNullException(nameof(storyPanel));
        _characterPanel = characterPanel ?? throw new ArgumentNullException(nameof(characterPanel));
        _gameModeController = gameModeController ?? throw new ArgumentNullException(nameof(gameModeController));
        _player = player ?? throw new ArgumentNullException(nameof(player));
        _playerActions = playerActions ?? throw new ArgumentNullException(nameof(playerActions));
        _locations = new List<LocationContent>(locations ?? throw new ArgumentNullException(nameof(locations)));
        _getLocation = getLocation ?? throw new ArgumentNullException(nameof(getLocation));
        _travelAction = travelAction ?? throw new ArgumentNullException(nameof(travelAction));
        _gameClock = gameClock ?? throw new ArgumentNullException(nameof(gameClock));
        _actionExecutionService = actionExecutionService ?? throw new ArgumentNullException(nameof(actionExecutionService));
        _availableInteractionQuery = availableInteractionQuery ?? throw new ArgumentNullException(nameof(availableInteractionQuery));
        _actionChoiceMapper = actionChoiceMapper ?? throw new ArgumentNullException(nameof(actionChoiceMapper));
        _locationEventService = locationEventService ?? throw new ArgumentNullException(nameof(locationEventService));
        _startEncounter = startEncounter ?? throw new ArgumentNullException(nameof(startEncounter));

        ValidatePlayerActions();
    }

    public void ShowLocation()
    {
        if (_activeLocationEvent != null)
        {
            ShowLocationEvent(_activeLocationEvent);
            return;
        }

        LocationContent location = _getLocation(_currentLocationId);
        IReadOnlyList<AvailableInteraction> interactions = _availableInteractionQuery.Query(
            _player,
            _playerActions,
            location,
            _getLocation,
            _travelAction);
        List<ActionChoiceViewData> choices = _actionChoiceMapper.Map(interactions, _player);

        _gameModeController.ShowExploration();
        _storyPanel.ShowContent(
            location.Name,
            location.Region,
            location.Description,
            choices,
            GameClockDisplayFormatter.Format(_gameClock));
        _storyPanel.ShowLocationButton(false);
    }

    public void ShowPage(string tab)
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
        _storyPanel.ShowContent(
            tab,
            "ADVENTURER RECORD",
            description,
            Array.Empty<ActionChoiceViewData>(),
            GameClockDisplayFormatter.Format(_gameClock));
        _storyPanel.ShowLocationButton(true);
    }

    public bool StartDebugGoblinEncounter(ActionDefinition entryAction)
    {
        if (entryAction == null)
        {
            throw new ArgumentNullException(nameof(entryAction));
        }

        if (_gameModeController.IsCombatMode)
        {
            return false;
        }

        if (!_startEncounter(entryAction))
        {
            return false;
        }

        _activeLocationEvent = null;
        return true;
    }

    public void HandleChoice(string choiceId)
    {
        if (_activeLocationEvent != null)
        {
            if (!ActionSelectionRequest.TryParse(choiceId, out ActionSelectionRequest eventRequest) ||
                eventRequest.SourceKind != InteractionSourceKind.Event ||
                eventRequest.SourceId != _activeLocationEvent.Id)
            {
                _storyPanel.ShowFeedback("That event choice is invalid or outdated.");
                return;
            }

            HandleLocationEventChoice(eventRequest.ActionId, eventRequest.TargetId);
            return;
        }

        if (!ActionSelectionRequest.TryParse(choiceId, out ActionSelectionRequest request))
        {
            _storyPanel.ShowFeedback("That action selection is invalid or outdated.");
            return;
        }

        switch (request.SourceKind)
        {
            case InteractionSourceKind.Player:
                if (request.SourceId != _player.Id)
                {
                    _storyPanel.ShowFeedback("That player action selection is outdated.");
                    return;
                }

                ExecutePlayerAction(request.ActionId);
                return;

            case InteractionSourceKind.Location:
                if (request.SourceId != _currentLocationId)
                {
                    _storyPanel.ShowFeedback("That location action selection is outdated.");
                    return;
                }

                ActionDefinition locationAction = FindChoice(
                    _getLocation(_currentLocationId).Actions,
                    request.ActionId);
                if (locationAction == null)
                {
                    _storyPanel.ShowFeedback("That action is no longer available here.");
                    return;
                }

                ExecuteAction(locationAction);
                return;

            case InteractionSourceKind.Travel:
                if (request.SourceId != _currentLocationId ||
                    request.ActionId != _travelAction.Id ||
                    string.IsNullOrWhiteSpace(request.TargetId))
                {
                    _storyPanel.ShowFeedback("That travel selection is outdated.");
                    return;
                }

                TravelTo(request.TargetId);
                return;

            default:
                _storyPanel.ShowFeedback("That action source is not supported.");
                return;
        }
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

    private void ExecutePlayerAction(string actionId)
    {
        foreach (PlayerActionDefinition playerAction in _playerActions)
        {
            if (playerAction.Action.Id != actionId) continue;
            if (!playerAction.IsAvailableAt(_currentLocationId))
            {
                _storyPanel.ShowFeedback("That personal action is not available here.");
                return;
            }

            ExecuteAction(playerAction.Action);
            return;
        }

        _storyPanel.ShowFeedback("That personal action is no longer available.");
    }

    private void ValidatePlayerActions()
    {
        HashSet<string> actionIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (LocationContent location in _locations)
        {
            foreach (ActionDefinition action in location.Actions)
            {
                if (!actionIds.Add(action.Id))
                {
                    throw new InvalidOperationException("Duplicate action ID across locations: " + action.Id);
                }
            }
        }

        foreach (PlayerActionDefinition playerAction in _playerActions)
        {
            if (!actionIds.Add(playerAction.Action.Id))
            {
                throw new InvalidOperationException("Player action ID conflicts with another action: " + playerAction.Action.Id);
            }

            foreach (string locationId in playerAction.AvailableLocationIds)
            {
                bool locationExists = false;
                foreach (LocationContent location in _locations)
                {
                    if (location.Id == locationId)
                    {
                        locationExists = true;
                        break;
                    }
                }

                if (!locationExists)
                {
                    throw new InvalidOperationException(
                        "Player action '" + playerAction.Action.Id + "' references unknown location '" + locationId + "'.");
                }
            }
        }
    }

    private static ActionDefinition FindChoice(ActionDefinition[] choices, string choiceId)
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
        int startDay = _gameClock.Day;
        int startMinutes = _gameClock.MinutesOfDay;
        int moodBeforeAction = _player.Mood;

        if (!_actionExecutionService.TryExecute(
            action,
            _player,
            _gameClock,
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
            _gameClock.Day,
            _gameClock.MinutesOfDay);
        _storyPanel.ShowFeedback(ActionMoodFeedbackFormatter.Format(action, _player.Mood - moodBeforeAction));
        return true;
    }

    private void TravelTo(string destinationId)
    {
        int startDay = _gameClock.Day;
        int startMinutes = _gameClock.MinutesOfDay;
        LocationContent current = _getLocation(_currentLocationId);
        if (Array.IndexOf(current.Exits, destinationId) < 0)
        {
            _storyPanel.ShowFeedback("That location is not connected to your current position.");
            return;
        }

        if (!_actionExecutionService.TryExecute(
            _travelAction,
            _player,
            _gameClock,
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
            _gameClock.Day,
            _gameClock.MinutesOfDay);
        _storyPanel.ShowFeedback("You arrive at " + _getLocation(destinationId).Name + ".");
        TryTriggerLocationEvent();
    }

    private void TryTriggerLocationEvent()
    {
        LocationContent location = _getLocation(_currentLocationId);

        if (!_locationEventService.TryTrigger(
            location,
            _gameClock.Day,
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
        IReadOnlyList<AvailableInteraction> interactions = _availableInteractionQuery.Query(locationEvent);
        List<ActionChoiceViewData> choices = _actionChoiceMapper.Map(interactions, _player);

        _gameModeController.ShowExploration();
        _storyPanel.ShowContent(
            locationEvent.Title,
            locationEvent.Region,
            locationEvent.Description,
            choices,
            GameClockDisplayFormatter.Format(_gameClock));
        _storyPanel.ShowLocationButton(false);
    }

    private void HandleLocationEventChoice(string choiceId, string targetId)
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

        if (selectedChoice == null ||
            (selectedChoice.EncounterActionId ?? string.Empty) != (targetId ?? string.Empty))
        {
            _storyPanel.ShowFeedback("That event choice is not available or has changed.");
            return;
        }

        if (selectedChoice.EncounterActionId != null)
        {
            ActionDefinition encounterAction = FindChoice(
                _getLocation(_currentLocationId).Actions,
                selectedChoice.EncounterActionId);

            if (encounterAction == null)
            {
                _storyPanel.ShowFeedback("The encounter action for this event is unavailable.");
                return;
            }

            if (_startEncounter(encounterAction))
            {
                _activeLocationEvent = null;
            }

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
