# RPG — Adventurer Academy Prototype

Godot 4.7.2 .NET / C# project.

## Design documents

- [Game Overview](Docs/Design/GAME_OVERVIEW.md) — game concept, core experience and prototype boundaries.
- [UI Architecture](Docs/Design/UI_ARCHITECTURE.md) — main screen, sidebar, Main Panel modes and persistent character state.
- [Combat Panel Contract](Docs/Design/COMBAT_PANEL_REVIEW.md) — combat data boundary, message model, intent lifecycle and UI behavior.
- [Open Design Questions](Docs/Design/OPEN_DESIGN_QUESTIONS.md) — unresolved decisions and resolved design choices.

## UI previews

- Main prototype: Scenes/Main.tscn
- Combat UI preview: Scenes/Previews/CombatPreview.tscn

To inspect the Combat UI, open Scenes/Previews/CombatPreview.tscn in Godot and run the current scene (F6).

## Combat UI responsibilities

- CombatPanel.cs: applies CombatViewData to Status, Message Log and Action components.
- CombatStatusPanel.cs: shows round, turn, enemy status and observed intent.
- CombatMessageLog.cs: owns bounded history and preserves the reader's scroll position.
- CombatMessageEntry.cs: presents one semantically typed Player, Enemy or System message.
- CombatActionPanel.cs: shows available actions and emits the chosen action ID.
- CombatPresentation.cs: defines action requests, service result contracts and presentation-safe data.
- CombatPresentationMapper.cs: maps service results into CombatViewData.

The current preview uses illustrative data. ICombatService is the intended gameplay boundary; no concrete combat service, combat rules or simulation are connected yet.
