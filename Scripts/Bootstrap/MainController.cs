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
    private ActionDefinition _debugGoblinEncounterAction;

    public override void _Ready()
    {
        _sidebar = GetNode<Sidebar>("SafeArea/Layout/Sidebar");
        StoryPanel storyPanel = GetNode<StoryPanel>("SafeArea/Layout/StoryPanel");
        CombatPanel combatPanel = GetNode<CombatPanel>("SafeArea/Layout/CombatPanel");
        CharacterPanel characterPanel = GetNode<CharacterPanel>("SafeArea/Layout/CharacterPanel");

        GameContentCatalog content = GameContentLoader.Load("res://Data");
        CharacterData playerData = content.GetCharacter("player-ari");
        CharacterData goblinData = content.GetCharacter("goblin-a");
        CharacterData bruteData = content.GetCharacter("brute_a");
        CharacterData disruptorData = content.GetCharacter("disruptor_a");
        Character player = new Character(playerData);
        _debugGoblinEncounterAction = FindAction(
            content.GetLocation("courtyard").Actions,
            "encounter_goblin");

        if (_debugGoblinEncounterAction == null)
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
            new Character(goblinData),
            CombatantSide.Enemy,
            content.GetCombatActionsForCharacter(goblinData.Id),
            behaviorProfile: EnemyBehaviorProfiles.CreateGoblinSkirmisher()));
        encounterCombatants.Add(new CombatantDefinition(
            new Character(bruteData),
            CombatantSide.Enemy,
            content.GetCombatActionsForCharacter(bruteData.Id),
            behaviorProfile: EnemyBehaviorProfiles.CreateBrute()));
        encounterCombatants.Add(new CombatantDefinition(
            new Character(disruptorData),
            CombatantSide.Enemy,
            content.GetCombatActionsForCharacter(disruptorData.Id),
            behaviorProfile: EnemyBehaviorProfiles.CreateDisruptor()));

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
            _combatController.StartGoblinEncounter);

        _sidebar.TabSelected += OpenTab;
        _sidebar.DebugGoblinEncounterRequested += StartDebugGoblinEncounter;
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

    private void StartDebugGoblinEncounter()
    {
        if (_explorationController == null || _debugGoblinEncounterAction == null)
        {
            return;
        }

        _explorationController.StartDebugGoblinEncounter(_debugGoblinEncounterAction);
    }

    private static ActionDefinition FindAction(ActionDefinition[] actions, string actionId)
    {
        foreach (ActionDefinition action in actions)
        {
            if (action.Id == actionId)
            {
                return action;
            }
        }

        return null;
    }

    private void ReturnToExploration()
    {
        _combatController.ReturnToExploration(_explorationController.ShowLocation);
    }
}
