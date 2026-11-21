using System.Collections.Generic;
using Godot;

/*
 * MainController is the Godot composition root.
 * It discovers scene nodes, constructs the application graph and connects top-level signals.
 */
public partial class MainController : Control
{
    private Sidebar _sidebar;
    private GameModeController _gameModeController;
    private ExplorationFlowController _explorationController;
    private CombatFlowController _combatController;
    private ActionDefinition _debugEncounterAction;

    public override void _Ready()
    {
        _sidebar = GetNode<Sidebar>("SafeArea/Layout/Sidebar");
        StoryPanel storyPanel = GetNode<StoryPanel>("SafeArea/Layout/StoryPanel");
        CombatPanel combatPanel = GetNode<CombatPanel>("SafeArea/Layout/CombatPanel");
        CharacterPanel characterPanel = GetNode<CharacterPanel>("SafeArea/Layout/CharacterPanel");

        GameContentCatalog content = GameContentLoader.Load("res://Data");
        CharacterData playerData = content.GetCharacter("player_student");
        CharacterData goblinData = content.GetCharacter("hostile_presence");
        Character player = new Character(playerData);
        Character enemy = new Character(goblinData);
        _debugEncounterAction = content.CombatEncounter;

        if (_debugEncounterAction == null)
        {
            throw new System.InvalidOperationException("Debug Goblin encounter action is missing from courtyard content.");
        }

        GameClock gameClock = new GameClock();
        ActivityTimeCalculator activityTimeCalculator = new ActivityTimeCalculator();
        ActivityFatigueCalculator activityFatigueCalculator = new ActivityFatigueCalculator();
        ActionExecutionService actionExecutionService = new ActionExecutionService(
            activityTimeCalculator, new ActionEffectExecutor(), activityFatigueCalculator);
        ActionDurationEstimateMapper durationEstimateMapper = new ActionDurationEstimateMapper(activityTimeCalculator);
        AvailableInteractionQuery availableInteractionQuery = new AvailableInteractionQuery();
        ActionChoiceMapper actionChoiceMapper = new ActionChoiceMapper(durationEstimateMapper);

        List<CombatantDefinition> encounterCombatants =
            new List<CombatantDefinition>(StartingParty.CreateCombatants(content, player));
        encounterCombatants.Add(new CombatantDefinition(
            enemy,
            CombatantSide.Enemy,
            content.GetCombatActionsForCharacter(goblinData.Id)));

        CombatService combatService = new CombatService(encounterCombatants);
        CombatPresentationMapper combatPresentationMapper = new CombatPresentationMapper();
        LocationEventService locationEventService = new LocationEventService();
        NpcPresenceQuery npcPresenceQuery = new NpcPresenceQuery(
            content.NpcSchedules.All,
            content.Characters.All);
        NpcInteractionQuery npcInteractionQuery = new NpcInteractionQuery(content.NpcInteractions);

        _gameModeController = new GameModeController(storyPanel, combatPanel);
        CombatSessionService combatSessionService = new CombatSessionService(
            player, combatService, gameClock, activityTimeCalculator,
            activityFatigueCalculator, actionExecutionService, content.CombatEncounter);
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
            storyPanel, characterPanel, _gameModeController, player,
            playerData.PlayerActions, content.Locations, content.GetLocation,
            content.Travel, gameClock, actionExecutionService, availableInteractionQuery,
            actionChoiceMapper, locationEventService, npcPresenceQuery, npcInteractionQuery,
            action => _combatController.StartEncounter(action, "Unit 213 Incident", "An unstable presence emerges from the dark apartment."));

        _sidebar.TabSelected += OpenTab;
        _sidebar.DebugEncounterRequested += StartDebugEncounter;
        storyPanel.ChoiceSelected += _explorationController.HandleChoice;
        storyPanel.ReturnToLocationRequested += _explorationController.ShowLocation;
        combatPanel.ActionSelected += _combatController.HandleCombatAction;
        combatPanel.ContinueRequested += ReturnToExploration;
        characterPanel.SetCharacter(new CharacterViewModel(player));
        _explorationController.ShowLocation();
    }

    public override void _Process(double delta)
    {
        _combatController?.AdvanceCombatTime(delta);
    }

    private void OpenTab(string tab)
    {
        if (_gameModeController.IsCombatMode) return;
        _explorationController.ShowPage(tab);
    }

    private void StartDebugEncounter()
    {
        if (_explorationController == null || _debugEncounterAction == null)
        {
            return;
        }

        _combatController.StartEncounter(_debugEncounterAction, "Debug Encounter", "An unstable presence appears.");
    }

    private void ReturnToExploration()
    {
        _combatController.ReturnToExploration(_explorationController.ShowLocation);
    }
}
