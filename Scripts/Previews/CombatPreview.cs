using Godot;

public partial class CombatPreview : Control
{
    private CombatPanel _combatPanel;

    public override void _Ready()
    {
        _combatPanel = GetNode<CombatPanel>("Margin/CombatPanel");
        _combatPanel.ActionSelected += OnActionSelected;

        _combatPanel.BeginEncounter(
            "Goblin Ambush",
            2,
            "PLAYER TURN",
            new[]
            {
                new EnemyCombatStatus("Goblin A", 40, 60, "Wounded")
            });

        _combatPanel.AppendMessage(new CombatMessage(
            CombatMessageType.Player,
            "YOU",
            "YOU · ROUND 02",
            "You attack Goblin A with a basic strike."));

        _combatPanel.AppendMessage(new CombatMessage(
            CombatMessageType.System,
            "SYSTEM",
            "ATTACK RESULT",
            "20 physical damage. Goblin A: HP 60 → 40."));

        _combatPanel.AppendMessage(new CombatMessage(
            CombatMessageType.Enemy,
            "GOBLIN A",
            "GOBLIN A",
            "The goblin snarls angrily. It tightens its grip on the weapon and lowers its stance."));

        _combatPanel.AppendMessage(new CombatMessage(
            CombatMessageType.System,
            "SYSTEM",
            "PERCEPTION CHECK · SUCCESS",
            "You notice the goblin shifting its weight forward. It appears to be preparing a dangerous lunge."));

        _combatPanel.ShowEnemyIntent(new EnemyIntent(
            "Goblin A",
            "Possible Heavy Lunge",
            "The enemy appears to be preparing a forward attack.",
            "PREDICTED · PERCEPTION SUCCESS"));

        _combatPanel.SetAvailableActions(new[]
        {
            new CombatActionOption("attack", "Attack Goblin A", "No cost", "Deal damage with a basic attack."),
            new CombatActionOption("defend", "Defend", "1 Energy", "Reduce the impact of the next incoming attack."),
            new CombatActionOption("dodge", "Dodge", "2 Energy", "Attempt to evade the incoming attack."),
            new CombatActionOption("items", "Use Item", "Varies", "Choose an item from your inventory.")
        });
    }

    private void OnActionSelected(string actionId)
    {
        GD.Print("Combat preview selected action: " + actionId + ". No combat resolver is connected.");
    }
}
