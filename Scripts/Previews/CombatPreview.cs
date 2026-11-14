using Godot;

public partial class CombatPreview : Control
{
    private CombatPanel _combatPanel;
    private CombatPresentationMapper _mapper;

    public override void _Ready()
    {
        _combatPanel = GetNode<CombatPanel>("Margin/CombatPanel");
        _mapper = new CombatPresentationMapper();
        _combatPanel.ActionSelected += OnActionSelected;
        _combatPanel.BeginEncounter("Goblin Ambush");

        CombatSnapshot snapshot = new CombatSnapshot(
            2,
            "PLAYER TURN",
            new[]
            {
                new EnemyCombatStatus("goblin-a", "Goblin A", 40, 60, "Wounded")
            },
            new IntentObservation(
                "goblin-a",
                "Goblin A",
                "The enemy appears to be preparing a forward attack.",
                IntentConfidence.Suspected),
            new[]
            {
                new CombatActionOption("attack", "Attack Goblin A", "No cost", "Deal damage with a basic attack.", targetId: "goblin-a"),
                new CombatActionOption("defend", "Defend", "1 Energy", "Reduce the impact of the next incoming attack."),
                new CombatActionOption("dodge", "Dodge", "2 Energy", "Attempt to evade the incoming attack."),
                new CombatActionOption("items", "Use Item", "Varies", "Choose an item from your inventory.")
            });

        CombatResult previewResult = new CombatResult(
            true,
            snapshot,
            new[]
            {
                new CombatEvent(CombatEventType.PlayerAction, 2, detail: "You attack Goblin A with a basic strike.", id: "preview-player-attack"),
                new CombatEvent(CombatEventType.Damage, 2, targetName: "Goblin A", amount: 20, id: "preview-damage"),
                new CombatEvent(CombatEventType.EnemyAction, 2, actorName: "GOBLIN A", detail: "The goblin snarls angrily and lowers its stance.", id: "preview-enemy-reaction"),
                new CombatEvent(CombatEventType.Check, 2, detail: "PERCEPTION CHECK · SUCCESS — You notice the goblin shifting its weight forward.", id: "preview-perception")
            });

        CombatViewData viewData = _mapper.Map(previewResult);
        _combatPanel.ApplyViewData(viewData);
    }

    private void OnActionSelected(CombatActionRequest request)
    {
        GD.Print("Combat preview selected action: " + request.ActionId + ", target: " + request.TargetId + ". No combat service is connected.");
    }
}
