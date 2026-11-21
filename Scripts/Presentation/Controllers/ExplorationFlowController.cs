using System;
using System.Collections.Generic;
using System.Text;

/// <summary>Coordinates exploration flow flow and user interactions.</summary>
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
    private readonly NpcPresenceQuery _npcPresenceQuery;
    private readonly NpcInteractionQuery _npcInteractionQuery;
    private readonly Func<ActionDefinition, bool> _startEncounter;

    private string _currentLocationId = "university_campus";
    private LocationEventDefinition _activeLocationEvent;
    private readonly HashSet<string> _visitedLocationIds = new HashSet<string>(StringComparer.Ordinal);
    private string _descriptionLocationId;
    private string _selectedNpcId;

    private const string _seeAroundChoiceId = "navigation|see-around";
    private const string _backToPeopleChoiceId = "navigation|back-to-people";
    private const string _selectNpcActionId = "select";
    private string _currentLocationDescription;

    /// <summary>Initializes a new instance of <see cref="ExplorationFlowController"/>.</summary>
    /// <param name="storyPanel">The story panel used to initialize the instance.</param>
    /// <param name="characterPanel">The character panel used to initialize the instance.</param>
    /// <param name="gameModeController">The game mode controller used to initialize the instance.</param>
    /// <param name="player">The player used to initialize the instance.</param>
    /// <param name="playerActions">The player actions used to initialize the instance.</param>
    /// <param name="locations">The locations used to initialize the instance.</param>
    /// <param name="string">The string used to initialize the instance.</param>
    /// <param name="getLocation">The get location used to initialize the instance.</param>
    /// <param name="travelAction">The travel action used to initialize the instance.</param>
    /// <param name="gameClock">The game clock used to initialize the instance.</param>
    /// <param name="actionExecutionService">The action execution service used to initialize the instance.</param>
    /// <param name="availableInteractionQuery">The available interaction query used to initialize the instance.</param>
    /// <param name="actionChoiceMapper">The action choice mapper used to initialize the instance.</param>
    /// <param name="locationEventService">The location event service used to initialize the instance.</param>
    /// <param name="npcPresenceQuery">The npc presence query used to initialize the instance.</param>
    /// <param name="npcInteractionQuery">The npc interaction query used to initialize the instance.</param>
    /// <param name="ActionDefinition">The action definition used to initialize the instance.</param>
    /// <param name="startEncounter">The start encounter used to initialize the instance.</param>
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
        NpcPresenceQuery npcPresenceQuery,
        NpcInteractionQuery npcInteractionQuery,
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
        _npcPresenceQuery = npcPresenceQuery ?? throw new ArgumentNullException(nameof(npcPresenceQuery));
        _npcInteractionQuery = npcInteractionQuery ?? throw new ArgumentNullException(nameof(npcInteractionQuery));
        _startEncounter = startEncounter ?? throw new ArgumentNullException(nameof(startEncounter));

        ValidatePlayerActions();
    }

    /// <summary>Displays location in the current game mode.</summary>
    public void ShowLocation()
    {
        if (_activeLocationEvent != null)
        {
            ShowLocationEvent(_activeLocationEvent);
            return;
        }

        LocationContent location = _getLocation(_currentLocationId);
        _selectedNpcId = null;
        IReadOnlyList<NpcPresence> presentNpcs = _npcPresenceQuery.Query(location.Id, _gameClock.MinutesOfDay);
        HashSet<string> presentNpcIds = GetNpcIds(presentNpcs);
        IReadOnlyList<AvailableInteraction> interactions = _availableInteractionQuery.Query(
            _player,
            _playerActions,
            location,
            _getLocation,
            _travelAction,
            presentNpcIds);
        List<ActionChoiceViewData> choices = _actionChoiceMapper.Map(interactions, _player);
        if (_player.CanAct && presentNpcs.Count > 0)
        {
            choices.Add(new ActionChoiceViewData(
                _seeAroundChoiceId,
                "See around",
                string.Empty,
                "OBSERVE"));
        }

        _gameModeController.ShowExploration();
        _storyPanel.ShowContent(
            location.Name,
            location.Region,
            GetCurrentLocationDescription(location),
            choices,
            GameClockDisplayFormatter.Format(_gameClock));
        _storyPanel.ShowLocationButton(false);
    }

    private static HashSet<string> GetNpcIds(IReadOnlyList<NpcPresence> presentNpcs)
    {
        HashSet<string> characterIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (NpcPresence presence in presentNpcs)
        {
            characterIds.Add(presence.CharacterId);
        }

        return characterIds;
    }

    private void ShowPeopleHere()
    {
        IReadOnlyList<NpcPresence> presentNpcs = _npcPresenceQuery.Query(
            _currentLocationId,
            _gameClock.MinutesOfDay);
        if (presentNpcs.Count == 0)
        {
            ShowLocation();
            _storyPanel.ShowFeedback("There is no one nearby to interact with.");
            return;
        }

        _selectedNpcId = null;
        StringBuilder description = new StringBuilder("You take a moment to look around.");
        List<ActionChoiceViewData> choices = new List<ActionChoiceViewData>();
        foreach (NpcPresence presence in presentNpcs)
        {
            description.Append("\n\n")
                .Append(presence.Name)
                .Append(" — ")
                .Append(presence.ActivityDescription);

            ActionSelectionRequest request = new ActionSelectionRequest(
                InteractionSourceKind.Npc,
                presence.CharacterId,
                _selectNpcActionId);
            choices.Add(new ActionChoiceViewData(
                request.Encode(),
                presence.Name,
                string.Empty,
                "PEOPLE HERE"));
        }

        _gameModeController.ShowExploration();
        _storyPanel.ShowContent(
            "People here",
            _getLocation(_currentLocationId).Name.ToUpperInvariant(),
            description.ToString(),
            choices,
            GameClockDisplayFormatter.Format(_gameClock));
        _storyPanel.ShowLocationButton(true);
    }

    private void ShowNpcInteractions(string characterId)
    {
        IReadOnlyList<NpcPresence> presentNpcs = _npcPresenceQuery.Query(
            _currentLocationId,
            _gameClock.MinutesOfDay);
        NpcPresence selectedNpc = null;
        foreach (NpcPresence presence in presentNpcs)
        {
            if (string.Equals(presence.CharacterId, characterId, StringComparison.Ordinal))
            {
                selectedNpc = presence;
                break;
            }
        }

        if (selectedNpc == null)
        {
            ShowLocation();
            _storyPanel.ShowFeedback("That character is no longer here.");
            return;
        }

        _selectedNpcId = characterId;
        IReadOnlyList<AvailableInteraction> interactions = _npcInteractionQuery.Query(
            characterId,
            _currentLocationId);
        List<ActionChoiceViewData> choices = _actionChoiceMapper.Map(interactions, _player);
        choices.Add(new ActionChoiceViewData(
            _backToPeopleChoiceId,
            "Back to people",
            string.Empty,
            "NAVIGATION"));

        string description = selectedNpc.ActivityDescription;
        if (interactions.Count == 0)
        {
            description += "\n\nThere are no available interactions here right now.";
        }
        else
        {
            description += "\n\nWhat would you like to do?";
        }

        _gameModeController.ShowExploration();
        _storyPanel.ShowContent(
            selectedNpc.Name,
            _getLocation(_currentLocationId).Name.ToUpperInvariant(),
            description,
            choices,
            GameClockDisplayFormatter.Format(_gameClock));
        _storyPanel.ShowLocationButton(true);
    }

    private string GetCurrentLocationDescription(LocationContent location)
    {
        if (!string.Equals(_descriptionLocationId, location.Id, StringComparison.Ordinal))
        {
            bool firstVisit = _visitedLocationIds.Add(location.Id);
            _descriptionLocationId = location.Id;
            _currentLocationDescription = location.GetDescription(firstVisit);
        }

        return _currentLocationDescription;
    }

    /// <summary>Displays page in the current game mode.</summary>
    /// <param name="tab">The tab used by this operation.</param>
    public void ShowPage(string tab)
    {
        string description = tab switch
        {
            "Profile" => GetProfileDescription(),
            "Skills" => "Techniques you have practiced and can use.",
            "Inventory" => "Items and equipment carried by your character.",
            "Relationships" => "People you have met around campus and the city.",
            "Goals" => "Your current assignments and short-term objectives.",
            "Journal" => "A record of meaningful events from your student life.",
            _ => "Prototype preferences and game settings."
        };

        _selectedNpcId = null;
        _gameModeController.ShowExploration();
        _storyPanel.ShowContent(
            tab,
            "STUDENT RECORD",
            description,
            Array.Empty<ActionChoiceViewData>(),
            GameClockDisplayFormatter.Format(_gameClock));
        _storyPanel.ShowLocationButton(true);
    }

    /// <summary>Starts debug encounter and initializes its presentation flow.</summary>
    /// <param name="entryAction">The entry action used by this operation.</param>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
    public bool StartDebugEncounter(ActionDefinition entryAction)
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

    /// <summary>Handles choice from the presentation layer.</summary>
    /// <param name="choiceId">The choice id used by this operation.</param>
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

        if (choiceId == _seeAroundChoiceId)
        {
            ShowPeopleHere();
            return;
        }

        if (choiceId == _backToPeopleChoiceId)
        {
            ShowPeopleHere();
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

            case InteractionSourceKind.Npc:
                if (request.ActionId == _selectNpcActionId)
                {
                    if (_selectedNpcId != null)
                    {
                        _storyPanel.ShowFeedback("That people list is outdated.");
                        return;
                    }

                    ShowNpcInteractions(request.SourceId);
                    return;
                }

                if (!string.Equals(_selectedNpcId, request.SourceId, StringComparison.Ordinal) ||
                    !_npcPresenceQuery.IsPresent(
                        request.SourceId,
                        _currentLocationId,
                        _gameClock.MinutesOfDay))
                {
                    ShowLocation();
                    _storyPanel.ShowFeedback("That character is no longer here.");
                    return;
                }

                foreach (AvailableInteraction interaction in _npcInteractionQuery.Query(
                    request.SourceId,
                    _currentLocationId))
                {
                    if (interaction.ActionId == request.ActionId)
                    {
                        ExecuteAction(interaction.Action);
                        return;
                    }
                }

                _storyPanel.ShowFeedback("That interaction is no longer available.");
                ShowNpcInteractions(request.SourceId);
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

        return "A summary of your student profile and current condition."
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

        if (!string.IsNullOrWhiteSpace(action.RequiredNpcId) &&
            !_npcPresenceQuery.IsPresent(
                action.RequiredNpcId,
                _currentLocationId,
                _gameClock.MinutesOfDay))
        {
            _storyPanel.ShowFeedback("That character is no longer here.");
            return false;
        }

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

        HashSet<string> presentNpcIds = _npcPresenceQuery.GetPresentCharacterIds(
            location.Id,
            _gameClock.MinutesOfDay);
        if (!_locationEventService.TryTrigger(
            location,
            _gameClock.Day,
            _player.CanAct,
            out LocationEventDefinition triggeredEvent,
            presentNpcIds))
        {
            return;
        }

        _activeLocationEvent = triggeredEvent;
        ShowLocationEvent(triggeredEvent);
    }

    private void ShowLocationEvent(LocationEventDefinition locationEvent)
    {
        HashSet<string> presentNpcIds = _npcPresenceQuery.GetPresentCharacterIds(
            _currentLocationId,
            _gameClock.MinutesOfDay);
        IReadOnlyList<AvailableInteraction> interactions = _availableInteractionQuery.Query(
            locationEvent,
            presentNpcIds);
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

        if (!string.IsNullOrWhiteSpace(selectedChoice.Action.RequiredNpcId) &&
            !_npcPresenceQuery.IsPresent(
                selectedChoice.Action.RequiredNpcId,
                _currentLocationId,
                _gameClock.MinutesOfDay))
        {
            _storyPanel.ShowFeedback("That character is no longer here.");
            ShowLocationEvent(_activeLocationEvent);
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
