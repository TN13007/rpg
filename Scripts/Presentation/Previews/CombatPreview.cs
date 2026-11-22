using Godot;

/// <summary>Provides a Godot preview of the turn-based combat presentation.</summary>
public partial class CombatPreview : Control
{
    private CombatPanel _combatPanel;
    private CombatPresentationMapper _mapper;

    /// <summary>Initializes the preview with sample Party, Enemy, AP and action-card data.</summary>
    public override void _Ready()
    {
        _combatPanel = GetNode<CombatPanel>("Margin/CombatPanel");
        _mapper = new CombatPresentationMapper();
        _combatPanel.ActionSelected += OnActionSelected;
        _combatPanel.EndPlayerTurnRequested += OnEndPlayerTurn;
        _combatPanel.BeginEncounter("Unit 213 Incident");

        CombatSnapshot snapshot = new CombatSnapshot(
            2,
            CombatPhase.PlayerTurn,
            2,
            2,
            "PLAYER TURN",
            new[]
            {
                new EnemyCombatStatus("hostile_presence", "Unstable Presence", 40, 60, "Wounded"),
                new EnemyCombatStatus("hostile_echo", "Unstable Echo", 60, 60, "Healthy")
            },
            new[]
            {
                new PartyCombatStatus("player_student", "Alex", 20, 20, "Healthy", true),
                new PartyCombatStatus("noah", "Noah", 35, 40, "Healthy", true)
            },
            new[]
            {
                new CombatActionOption(
                    "quick_strike",
                    "player_student",
                    "Alex · Quick Strike",
                    "1 AP",
                    "Deal 8 damage with a fast attack.",
                    targets: new[] { new CombatActionTargetOption("hostile_presence", "Unstable Presence") }),
                new CombatActionOption(
                    "heavy_strike",
                    "noah",
                    "Noah · Heavy Strike",
                    "1 AP",
                    "Deal 14 damage with a slower attack.",
                    targets: new[] { new CombatActionTargetOption("hostile_presence", "Unstable Presence") }),
                new CombatActionOption(
                    "guard",
                    "player_student",
                    "Alex · Guard",
                    "1 AP · 1 Stamina",
                    "Reduce the next incoming hit.")
            });

        CombatResult previewResult = new CombatResult(
            true,
            snapshot,
            new[]
            {
                new CombatEvent(CombatEventType.PlayerAction, 2, actorName: "Alex", targetName: "Unstable Presence", detail: "Alex performs a quick strike.", id: "preview-player-attack", actionId: "quick_strike"),
                new CombatEvent(CombatEventType.Damage, 2, targetName: "Unstable Presence", amount: 8, id: "preview-damage"),
                new CombatEvent(CombatEventType.TurnChanged, 2, detail: "The Enemy Turn begins.", id: "preview-enemy-turn"),
                new CombatEvent(CombatEventType.EnemyAction, 2, actorName: "Unstable Echo", targetName: "Noah", detail: "The echo releases a disorienting pulse.", id: "preview-enemy-action")
            });

        _combatPanel.ApplyViewData(_mapper.Map(previewResult));
    }

    private void OnActionSelected(CombatActionSelection selection)
    {
        GD.Print("Combat preview selected action: " + selection.ActionId + ", actor: " + selection.ActorId + ", target: " + selection.TargetId + ".");
    }

    private void OnEndPlayerTurn()
    {
        GD.Print("Combat preview received End Player Turn. No combat service is connected.");
    }
}