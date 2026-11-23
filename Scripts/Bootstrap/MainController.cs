using System;
using Godot;

/// <summary>Composes runtime services and connects the top-level exploration and combat signals.</summary>
public partial class MainController : Control
{
    private Sidebar _sidebar;
    private GameModeController _gameModeController;
    private ExplorationFlowController _explorationController;
    private CombatFlowController _combatController;
    private ActionDefinition _debugEncounterAction;
    private const int _unit213ActionCapacity = 2;

    /// <summary>Builds the application graph, connects presentation signals and shows the initial Location.</summary>
    public override void _Ready()
    {
        _sidebar = GetNode<Sidebar>("SafeArea/Layout/Sidebar");
        StoryPanel storyPanel = GetNode<StoryPanel>("SafeArea/Layout/StoryPanel");
        CombatPanel combatPanel = GetNode<CombatPanel>("SafeArea/Layout/CombatPanel");
        CharacterPanel characterPanel = GetNode<CharacterPanel>("SafeArea/Layout/CharacterPanel");

        GameContentCatalog content = GameContentLoader.Load("res://Data");
        CharacterData playerData = content.GetCharacter("player_student");
        Character player = new Character(playerData);
        ExplorationSessionState explorationSessionState = new ExplorationSessionState("university_campus");
        _debugEncounterAction = content.CombatEncounter;

        GameClock gameClock = new GameClock(startingWeekday: DayOfWeek.Monday);
        ActivityTimeCalculator activityTimeCalculator = new ActivityTimeCalculator();
        ActivityFatigueCalculator activityFatigueCalculator = new ActivityFatigueCalculator();
        ActionExecutionService actionExecutionService = new ActionExecutionService(
            activityTimeCalculator, new ActionEffectExecutor(), activityFatigueCalculator);
        ActionDurationEstimateMapper durationEstimateMapper = new ActionDurationEstimateMapper(activityTimeCalculator);
        AvailableInteractionQuery availableInteractionQuery = new AvailableInteractionQuery();
        ActionChoiceMapper actionChoiceMapper = new ActionChoiceMapper(durationEstimateMapper);

        CombatantDefinition[] encounterCombatants =
            Unit213EncounterFactory.CreateCombatants(content, player);

        CombatService combatService = new CombatService(
            encounterCombatants,
            actionCapacity: _unit213ActionCapacity);
        CombatPresentationMapper combatPresentationMapper = new CombatPresentationMapper();
        LocationEventService locationEventService = new LocationEventService();
        NpcPresenceQuery npcPresenceQuery = new NpcPresenceQuery(
            content.NpcSchedules.All,
            content.Characters.All);
        NpcInteractionQuery npcInteractionQuery = new NpcInteractionQuery(content.NpcInteractions);

        _gameModeController = new GameModeController(storyPanel, combatPanel);
        CombatSessionService combatSessionService = new CombatSessionService(
            player,
            combatService,
            gameClock,
            activityTimeCalculator,
            activityFatigueCalculator,
            actionExecutionService,
            content.CombatEncounter);
        CombatPresentationOrchestrator combatPresentation = new CombatPresentationOrchestrator(
            _gameModeController,
            _sidebar,
            combatPanel,
            characterPanel,
            combatPresentationMapper,
            combatService.PartyMembers);
        _combatController = new CombatFlowController(
            storyPanel,
            combatPresentation,
            combatSessionService);
        _explorationController = new ExplorationFlowController(
            storyPanel,
            characterPanel,
            _gameModeController,
            player,
            explorationSessionState,
            playerData.PlayerActions,
            content.Locations,
            content.GetLocation,
            content.Travel,
            gameClock,
            actionExecutionService,
            availableInteractionQuery,
            actionChoiceMapper,
            locationEventService,
            npcPresenceQuery,
            npcInteractionQuery,
            action => _combatController.StartEncounter(
                action,
                "Unit 213 Incident",
                "An unstable presence emerges from the dark apartment."));

        _sidebar.TabSelected += OpenTab;
        _sidebar.DebugEncounterRequested += StartDebugEncounter;
        storyPanel.ChoiceSelected += _explorationController.HandleChoice;
        storyPanel.ReturnToLocationRequested += _explorationController.ShowLocation;
        combatPanel.ActionSelected += HandleCombatAction;
        combatPanel.EndPlayerTurnRequested += HandleEndPlayerTurn;
        combatPanel.ContinueRequested += ReturnToExploration;
        characterPanel.SetCharacter(new CharacterViewModel(player));
        _explorationController.ShowLocation();
    }

    private void OpenTab(string tab)
    {
        if (_gameModeController.IsCombatMode)
        {
            return;
        }

        _explorationController.ShowPage(tab);
    }

    private void StartDebugEncounter()
    {
        if (_explorationController == null || _debugEncounterAction == null)
        {
            return;
        }

        _combatController.StartEncounter(
            _debugEncounterAction,
            "Debug Encounter",
            "An unstable presence appears.");
    }

    private void HandleCombatAction(CombatActionSelection selection)
    {
        if (selection == null)
        {
            return;
        }

        GetNode<CombatPanel>("SafeArea/Layout/CombatPanel").ClearTargetSelection();
        CombatActionRequest request = new CombatActionRequest(
            selection.ActionId,
            selection.TargetId,
            selection.ActorId);
        _combatController.HandleCombatAction(request);
    }

    private void HandleEndPlayerTurn()
    {
        GetNode<CombatPanel>("SafeArea/Layout/CombatPanel").ClearTargetSelection();
        _combatController.HandleEndPlayerTurn();
    }

    private void ReturnToExploration()
    {
        GetNode<CombatPanel>("SafeArea/Layout/CombatPanel").ClearTargetSelection();
        _combatController.ReturnToExploration(_explorationController.ShowLocation);
    }
}