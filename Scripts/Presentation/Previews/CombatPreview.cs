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
        _combatPanel.BeginEncounter("Unit 213 Incident");

        CombatSnapshot snapshot = new CombatSnapshot(
            2,
            "PLAYER TURN",
            new[]
            {
                new EnemyCombatStatus("hostile_presence", "Unstable Presence", 40, 60, "Wounded")
            },
            new IntentObservation(
                "hostile_presence",
                "Unstable Presence",
                "The enemy appears to be preparing a forward attack.",
                IntentConfidence.Suspected),
            new[]
            {
                new CombatActionOption("attack", "Attack Unstable Presence", "No Stamina cost", "Deal damage with a basic attack.", targetId: "hostile_presence"),
                new CombatActionOption("defend", "Defend", "1 Stamina", "Reduce the impact of the next incoming attack."),
                new CombatActionOption("dodge", "Dodge", "2 Stamina", "Attempt to evade the incoming attack."),
                new CombatActionOption("catch_breath", "Catch Breath", "Recover 3 Stamina", "Recover your composure before the enemy responds."),
                new CombatActionOption("items", "Use Item", "Varies", "Choose an item from your inventory.")
            },
            timeline: new[]
            {
                new TimelineCombatantStatus(
                    "player_student",
                    "Alex",
                    false,
                    20.0,
                    10.0,
                    15,
                    false,
                    false,
                    true,
                    true),
                new TimelineCombatantStatus(
                    "hostile_presence",
                    "Unstable Presence",
                    true,
                    100.0,
                    10.0,
                    5,
                    false,
                    false)
            });

        CombatResult previewResult = new CombatResult(
            true,
            snapshot,
            new[]
            {
                new CombatEvent(CombatEventType.PlayerAction, 2, detail: "You attack Unstable Presence with a basic strike.", id: "preview-player-attack"),
                new CombatEvent(CombatEventType.Damage, 2, targetName: "Unstable Presence", amount: 20, id: "preview-damage"),
                new CombatEvent(CombatEventType.EnemyAction, 2, actorName: "UNSTABLE PRESENCE", detail: "The presence snarls angrily and lowers its stance.", id: "preview-enemy-reaction"),
                new CombatEvent(CombatEventType.Check, 2, detail: "OBSERVATION · SUCCESS — You notice the presence shifting its weight forward.", id: "preview-perception")
            });

        CombatViewData viewData = _mapper.Map(previewResult);
        _combatPanel.ApplyViewData(viewData);
    }

    private void OnActionSelected(CombatActionRequest request)
    {
        GD.Print("Combat preview selected action: " + request.ActionId + ", target: " + request.TargetId + ". No combat service is connected.");
    }
}
