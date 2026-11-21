using System;
using System.Collections.Generic;
using System.Linq;
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
    private string[] _pendingEnemyTargetIds = Array.Empty<string>();

    public override void _Ready()
    {
        _sidebar = GetNode<Sidebar>("SafeArea/Layout/Sidebar");
        StoryPanel storyPanel = GetNode<StoryPanel>("SafeArea/Layout/StoryPanel");
        CombatPanel combatPanel = GetNode<CombatPanel>("SafeArea/Layout/CombatPanel");
        CharacterPanel characterPanel = GetNode<CharacterPanel>("SafeArea/Layout/CharacterPanel");

        GameContentCatalog content = GameContentLoader.Load("res://Data");
        CharacterData playerData = content.GetCharacter("player_student");
        CharacterData enemyData = content.GetCharacter("hostile_presence");
        Character player = new Character(playerData);
        Character enemy = new Character(enemyData);
        _debugEncounterAction = content.CombatEncounter;

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
            content.GetCombatActionsForCharacter(enemyData.Id),
            behaviorProfile: EnemyBehaviorProfiles.CreateUnstablePresence()));

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
        _sidebar.EnemySelected += HandleEnemySelected;
        storyPanel.ChoiceSelected += _explorationController.HandleChoice;
        storyPanel.ReturnToLocationRequested += _explorationController.ShowLocation;
        combatPanel.ActionSelected += HandleCombatAction;
        combatPanel.EnemyTargetSelectionRequested += BeginEnemyTargetSelection;
        combatPanel.EnemyTargetSelectionCancelled += ClearEnemyTargetSelection;
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

    private void BeginEnemyTargetSelection(string[] targetIds)
    {
        _pendingEnemyTargetIds = targetIds ?? Array.Empty<string>();
        _sidebar.SetEnemyTargetSelection(_pendingEnemyTargetIds);
    }

    private void HandleEnemySelected(string enemyId)
    {
        if (!_pendingEnemyTargetIds.Contains(enemyId, StringComparer.Ordinal))
        {
            return;
        }

        CombatPanel combatPanel = GetNode<CombatPanel>("SafeArea/Layout/CombatPanel");
        combatPanel.SetSelectedEnemyTarget(enemyId);
    }

    private void ClearEnemyTargetSelection()
    {
        _pendingEnemyTargetIds = Array.Empty<string>();
        _sidebar.ClearEnemyTargetSelection();
    }

    private void HandleCombatAction(CombatActionRequest request)
    {
        ClearEnemyTargetSelection();
        GetNode<CombatPanel>("SafeArea/Layout/CombatPanel").ClearTargetSelection();
        _combatController.HandleCombatAction(request);
    }

    private void ReturnToExploration()
    {
        ClearEnemyTargetSelection();
        _combatController.ReturnToExploration(_explorationController.ShowLocation);
    }
}
