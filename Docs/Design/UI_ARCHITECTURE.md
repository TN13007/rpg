# UI Architecture — Main Game Interface

**Status:** Design direction / partial UI prototype  
**Related documents:** [Game Overview](GAME_OVERVIEW.md), [Combat Panel Contract](COMBAT_PANEL_REVIEW.md)

## 1. Main Screen Composition

| Region | Responsibility |
|---|---|
| Left Sidebar | Navigate between major player-facing pages. |
| Main Panel | Present the current location, story context or dedicated activity mode. |
| Right Character Panel | Keep the player's essential current state visible. |

The Main Panel is the primary content area. It is not merely a text log.

## 2. Sidebar — Proposed Information Architecture

| Group | Pages / content | Purpose |
|---|---|---|
| CHARACTER | Profile, Skills, Inventory, Relationships | Inspect and manage the character. |
| DIARY | Current Goals, Journal Entries | Track current objectives and recorded experiences. |
| SYSTEM | Settings | Configure the game. |

This taxonomy is a working proposal, not a locked implementation.

A separate World or Explore sidebar group is not currently planned. The Main Panel owns the current location and exploration experience. A dedicated Map page remains undecided.

The repository sidebar may still contain older World / Adventure entries. Those are prototype leftovers.

## 3. Main Panel Modes

### 3.1 Exploration Mode

Exploration is the default location-facing presentation. It communicates the current location, relevant time context, atmosphere, illustrative imagery, present NPCs / objects and available interactions.

The player should understand the situation and choose what to do without requiring free-roam movement or pathfinding.

### 3.2 Combat Mode

Combat uses a dedicated turn-based presentation rather than reusing the Exploration panel.

It contains encounter context, current combat status, observed enemy intent, event history and currently available actions.

The Combat UI receives presentation data; it does not own combat rules. The intended boundary is:

    Actor → CombatService → CombatResult + safe CombatSnapshot
                             → CombatPresentationMapper
                             → CombatViewData → Combat UI

CombatService owns validation, resolution and combat-state mutation. The Mapper converts the service result into UI-safe data. The UI only renders that data.

Detailed message, intent, action-refresh and lifecycle contracts are defined in [Combat Panel Contract](COMBAT_PANEL_REVIEW.md). This document intentionally does not duplicate those rules.

### 3.3 Other Activity Modes

Study, rest, social interaction, inventory management and other activities may need focused subviews or contextual panels. Their exact presentation patterns have not been designed yet.

Do not assume every activity must use the same message-log layout as Combat.

## 4. Character Panel

The right-side Character Panel remains visible across Main Panel modes.

Its initial always-visible indicators are:
- **Health:** survival; zero means death.
- **Energy:** action capacity and a source of challenge / progression.
- **Mood:** the character's emotional state.

Detailed information belongs in relevant character pages. The persistent panel should remain minimal.

The existing prototype may still display old sample fields such as HP, Stamina or Gold. Those are not the agreed target specification.

## 5. Information Ownership

Keep current state separate from historical narration.

- Character Panel: current persistent player indicators.
- Combat Status: current encounter state.
- Combat Message Log: ordered event history, not the source of truth for current HP.
- Exploration: current place and available interactions.
- Diary: selected goals and meaningful records, not every transient UI message.

Avoid duplicating information unless repetition materially improves usability.

## 6. Mode Ownership and Transitions

The Main Controller owns which Main Panel mode is active.

- Entering a location presents Exploration.
- Starting an encounter switches the Main Panel to Combat.
- Ending combat returns to the current location's Exploration presentation.
- Character Panel remains persistent.

The complete transition flow is not implemented.

## 7. Prototype Reality Check

The repository contains separate UI scenes and previews, but the full architecture is incomplete.

- Sidebar taxonomy may not match the proposal.
- Character Panel may still use old sample indicators.
- Combat preview is standalone and is not integrated into the main screen.
- Other activity modes are not specified or implemented.
- ICombatService is an interface / contract only; no combat simulation is connected.

## 8. UI Review Criteria

1. Can the player identify current context and available choices?
2. Does each panel have a distinct responsibility?
3. Is persistent character state visible without clutter?
4. Does each activity receive a presentation suited to its decisions?
5. Are current state, historical events, observations and inferences distinguishable?
6. Does navigation avoid duplicating the Main Panel's role?
