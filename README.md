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

## Character and combat implementation

- Scripts/Characters/CharacterData.cs: immutable starting data for a character and the CharacterViewModel consumed by the Character Panel.
- Scripts/Characters/Character.cs: runtime character object. It owns Health, Energy, Mood and Gold, and controls damage, resource spending, recovery and reward changes.
- Scripts/Services/CombatService.cs: coordinates the player and enemy Character objects, validates actions, resolves turns and grants the victory reward.
- Scripts/Data/CombatPresentation.cs: action request, result, safe snapshot, event and combat view-data contracts.
- Scripts/Data/CombatPresentationMapper.cs: maps service results into presentation messages and CombatViewData.
- Scripts/UI/CharacterPanel.cs: renders CharacterViewModel and does not access the domain Character directly.
- Scripts/UI/Combat/CombatPanel.cs: coordinates combat status, message log, action selection and the return-to-exploration action.
- Scripts/UI/Combat/CombatStatusPanel.cs: current round, enemy HP / condition and observed intent.
- Scripts/UI/Combat/CombatMessageLog.cs: bounded history and conditional auto-scroll.
- Scripts/UI/Combat/CombatMessageEntry.cs: Player-right, Enemy-left and System-center message presentation.
- Scripts/UI/Combat/CombatActionPanel.cs: current action options and request emission.
- Scripts/MainController.cs: creates the Player and Goblin from CharacterData, owns the Exploration / Combat mode transition and refreshes the Character Panel from a new CharacterViewModel.

## Prototype limitations

Combat rules are intentionally small and deterministic for this sprint: basic attack deals 20 damage, Goblin has 40 HP and deals 4 damage, Defend costs 1 Energy and reduces the next hit to 1, Dodge costs 2 Energy and avoids the next hit. Victory grants 25 G.

The Character OOP integration has been committed to the feature branch, but this new integration still needs a Godot 4.7.2 .NET build and end-to-end runtime check. The standalone Combat Preview remains illustrative and is not the main gameplay entry point.
