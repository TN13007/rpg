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

    public override void _Ready()
    {
        _sidebar = GetNode<Sidebar>("SafeArea/Layout/Sidebar");
        StoryPanel storyPanel = GetNode<StoryPanel>("SafeArea/Layout/StoryPanel");
        CombatPanel combatPanel = GetNode<CombatPanel>("SafeArea/Layout/CombatPanel");
        CharacterPanel characterPanel = GetNode<CharacterPanel>("SafeArea/Layout/CharacterPanel");

        AcademyContent.Load("res://Data/AcademyLocations.json");

        CharacterData playerData = CharacterDataLoader.Load("res://Data/Characters/Player.json");
        CharacterData goblinData = CharacterDataLoader.Load("res://Data/Characters/Goblin.json");
        Character player = new Character(playerData);
        Character goblin = new Character(goblinData);

        DayClock dayClock = new DayClock();
        ActivityTimeCalculator activityTimeCalculator = new ActivityTimeCalculator();
        ActivityFatigueCalculator activityFatigueCalculator = new ActivityFatigueCalculator();
        ActionExecutionService actionExecutionService = new ActionExecutionService(
            activityTimeCalculator,
            new ActionEffectExecutor(),
            activityFatigueCalculator);
        ActionDurationEstimateMapper durationEstimateMapper =
            new ActionDurationEstimateMapper(activityTimeCalculator);
        AvailableInteractionQuery availableInteractionQuery = new AvailableInteractionQuery();
        ActionChoiceMapper actionChoiceMapper = new ActionChoiceMapper(durationEstimateMapper);
        CombatService combatService = new CombatService(player, goblin);
        CombatPresentationMapper combatPresentationMapper = new CombatPresentationMapper();
        LocationEventService locationEventService = new LocationEventService();

        _gameModeController = new GameModeController(storyPanel, combatPanel);
        _combatController = new CombatFlowController(
            storyPanel,
            combatPanel,
            characterPanel,
            _gameModeController,
            player,
            combatService,
            combatPresentationMapper,
            dayClock,
            activityTimeCalculator,
            activityFatigueCalculator,
            actionExecutionService,
            AcademyContent.CombatEncounter);
        _explorationController = new ExplorationFlowController(
            storyPanel,
            characterPanel,
            _gameModeController,
            player,
            playerData.PlayerActions,
            AcademyContent.All,
            AcademyContent.Get,
            AcademyContent.Travel,
            dayClock,
            actionExecutionService,
            availableInteractionQuery,
            actionChoiceMapper,
            locationEventService,
            _combatController.StartGoblinEncounter);

        _sidebar.TabSelected += OpenTab;
        storyPanel.ChoiceSelected += _explorationController.HandleChoice;
        storyPanel.ReturnToLocationRequested += _explorationController.ShowLocation;
        combatPanel.ActionSelected += _combatController.HandleCombatAction;
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

    private void ReturnToExploration()
    {
        _combatController.ReturnToExploration(_explorationController.ShowLocation);
    }
}
