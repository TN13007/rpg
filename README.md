# RPG — Adventurer Academy Prototype

Godot 4.7.2 .NET / C# project.

## Design documents

- [Game Overview](Docs/Design/GAME_OVERVIEW.md) — game concept, core experience and prototype boundaries.
- [UI Architecture](Docs/Design/UI_ARCHITECTURE.md) — main screen, sidebar, Main Panel modes and persistent character state.
- [Combat Panel Review](Docs/Design/COMBAT_PANEL_REVIEW.md) — detailed Combat UI contract and external review questions.
- [Open Design Questions](Docs/Design/OPEN_DESIGN_QUESTIONS.md) — unresolved decisions; do not treat these as approved requirements.

## UI previews

- Main prototype: Scenes/Main.tscn
- Combat UI preview: Scenes/Previews/CombatPreview.tscn

To inspect the Combat UI, open Scenes/Previews/CombatPreview.tscn in Godot and run the current scene (F6).

## Combat UI responsibilities

- CombatPanel.cs: coordinates the combat UI components.
- CombatStatusPanel.cs: shows round, turn, enemy status and detected intent.
- CombatMessageLog.cs: owns the scrollable, bounded message history.
- CombatMessageEntry.cs: presents one Player, Enemy or System message.
- CombatActionPanel.cs: shows available actions and emits the chosen action ID.
- CombatPresentation.cs: data passed into the presentation layer.

The UI preview is not a combat simulation. It does not resolve attacks, calculate damage, spend resources or advance turns.
