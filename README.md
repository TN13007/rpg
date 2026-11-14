# RPG — Adventurer Academy Prototype

Godot 4.7.2 .NET / C# project.

## Design documents

- [Game Overview](Docs/Design/GAME_OVERVIEW.md) — game concept, core experience and prototype boundaries.
- [UI Architecture](Docs/Design/UI_ARCHITECTURE.md) — main screen, sidebar, Main Panel modes and persistent character state.
- [Combat Panel Contract](Docs/Design/COMBAT_PANEL_REVIEW.md) — combat data boundary, message model, intent lifecycle and UI behavior.
- [Open Design Questions](Docs/Design/OPEN_DESIGN_QUESTIONS.md) — unresolved decisions and resolved design choices.
- [Playable Journey Sprint](Docs/Design/SPRINT_PLAYABLE_JOURNEY.md) — the end-to-end Goblin encounter target for this sprint.

## Run

Open the project in Godot 4.7.2 .NET and run the main scene (Scenes/Main.tscn).

## Current player journey

1. Start in the Academy Courtyard.
2. Choose **Investigate the rustling near the gate**.
3. The Main Panel switches from Exploration to the dedicated Combat mode.
4. Choose Attack, Defend or Dodge. The combat service validates and resolves the action, updates HP / Energy and lets the Goblin respond.
5. Defeat the Goblin to receive 25 G.
6. Choose **Return to Exploration**. The Main Panel returns to the same location.

The right Character Panel remains visible and shows Health, Energy, Mood and Gold. The left Sidebar contains Character, Diary and System navigation; location exploration remains in the Main Panel rather than becoming a separate sidebar tab.

## Combat implementation

- Scripts/Services/CombatService.cs: validates actions, resolves player and Goblin turns, applies damage, detects victory / defeat and grants the reward.
- Scripts/Data/PlayerState.cs: shared prototype player state for Health, Energy, Mood and Gold.
- Scripts/Data/CombatPresentation.cs: action request, result, safe snapshot, event and view-data contracts.
- Scripts/Data/CombatPresentationMapper.cs: maps service results into presentation messages and CombatViewData.
- Scripts/UI/Combat/CombatPanel.cs: coordinates combat status, message log, action selection and the return-to-exploration action.
- Scripts/UI/Combat/CombatStatusPanel.cs: current round, enemy HP / condition and observed intent.
- Scripts/UI/Combat/CombatMessageLog.cs: bounded history and conditional auto-scroll.
- Scripts/UI/Combat/CombatMessageEntry.cs: Player-right, Enemy-left and System-center message presentation.
- Scripts/UI/Combat/CombatActionPanel.cs: current action options and request emission.
- Scripts/MainController.cs: owns the Exploration / Combat mode transition and returns to the encounter's original location.

## Prototype limitations

Combat rules are intentionally small and deterministic for this sprint: basic attack deals 20 damage, Goblin has 40 HP and deals 4 damage, Defend costs 1 Energy and reduces the next hit to 1, Dodge costs 2 Energy and avoids the next hit. Victory grants 25 G.

This is a playable vertical slice, not the final combat balance or complete life-simulation system. The standalone Combat Preview remains illustrative and is not the main gameplay entry point.
