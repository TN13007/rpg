# UI Architecture — Main Game Interface

**Status:** Design direction / partial UI prototype  
**Related documents:** [Game Overview](GAME_OVERVIEW.md), [Combat Panel Review](COMBAT_PANEL_REVIEW.md)

## 1. Main Screen Composition

The intended main screen has three persistent regions:

| Region | Responsibility |
|---|---|
| Left Sidebar | Navigate between major player-facing pages. |
| Main Panel | Present the current location, story context or dedicated activity mode. |
| Right Character Panel | Keep the player's essential current state visible. |

The Main Panel is the primary content area. It is not merely a text log.

## 2. Sidebar — Proposed Information Architecture

The current proposed groups are:

| Group | Pages / content | Purpose |
|---|---|---|
| CHARACTER | Profile, Skills, Inventory, Relationships | Inspect and manage the character. |
| DIARY | Current Goals, Journal Entries | Keep track of current objectives and recorded experiences without framing the whole game as a quest list. |
| SYSTEM | Settings | Configure the game. |

**Not final:** This taxonomy is a working proposal, not a locked implementation.

A separate World or Explore sidebar group is currently not considered necessary. The Main Panel itself owns the current location and exploration experience. A dedicated Map page may still be useful, but whether it belongs in the sidebar or is needed at all remains undecided.

The current repository sidebar may still contain older World / Adventure entries. Those are prototype leftovers and must not be treated as the latest design decision.

## 3. Main Panel Modes

The Main Panel changes presentation according to the current activity. It should not force every activity into one generic layout.

### 3.1 Exploration Mode

Exploration is the default location-facing presentation.

It should communicate:
- Current location name and relevant time context.
- Scene description and atmosphere.
- An illustrative image when useful.
- NPCs and objects currently present.
- Available interactions or activity choices.

The player should be able to understand the current situation and choose what to do without needing a free-roam map or movement controls.

### 3.2 Combat Mode

Combat uses a dedicated turn-based presentation. It is not the Exploration panel with combat text inserted into it.

The current proposal contains:
- Encounter context.
- Combat status such as round, active turn, enemy HP / condition and observed enemy intent.
- A scrollable event/message history.
- Available action choices, with relevant target, cost, description and availability.

Message alignment:
- Player messages: right.
- Enemy messages: left.
- System messages: center.

Actions and their outcomes must be distinct. The UI may show only enemy intent the character has observed or inferred; hidden AI decisions must not leak into presentation.

Internal thoughts are optional. A thought or inference should be clearly labeled and should not be presented as objective fact.

See [Combat Panel Review](COMBAT_PANEL_REVIEW.md) for the detailed contract and review questions.

### 3.3 Other Activity Modes

Study, rest, social interaction, inventory management and other activities may need focused subviews or contextual panels. Their exact presentation patterns have not been designed yet.

Do not assume that all activities must use the same message-log layout as combat. Reuse should follow shared interaction needs, not visual uniformity for its own sake.

## 4. Character Panel

The right-side Character Panel is intended to remain visible across Main Panel modes.

Its initial always-visible indicators are:
- **Health:** survival; zero means death.
- **Energy:** action capacity and a source of challenge / progression.
- **Mood:** the character's emotional state.

Detailed information belongs in the relevant character pages rather than expanding the persistent panel into a dashboard full of every statistic.

The existing prototype may still display older sample fields such as HP, Stamina or Gold. Those values are not the agreed target specification. The Health / Energy / Mood design has not yet been fully implemented or integrated into the Combat preview.

## 5. Information Ownership

Keep current state separate from historical narration.

Examples:
- The Character Panel shows the player's current persistent indicators.
- Combat Status shows current encounter state.
- The Combat Message Log records ordered events and outcomes; it is not the source of truth for current HP.
- The Exploration view describes the current place and available interactions.
- Diary content may preserve selected goals or meaningful records, but it is not intended to duplicate every transient UI message.

Each area should have a clear responsibility. Avoid showing the same information in multiple places unless repetition materially improves usability.

## 6. Mode Ownership and Transitions

The Main Controller is expected to coordinate which Main Panel mode is active.

Conceptual flow:
- Entering a location presents Exploration.
- Starting an encounter switches the Main Panel to Combat.
- Ending combat returns the player to the previous/current location's Exploration presentation.
- The Character Panel remains persistent during the transition.

This is an intended interaction contract. The complete transition flow is not yet implemented.

## 7. Prototype Reality Check

The repository currently contains separate UI scenes and previews, but the full architecture above is not complete.

Reviewers should verify implementation status in the code rather than infer it from this design document. In particular:
- The current sidebar may not match the proposed taxonomy.
- The Character Panel may still use old sample indicators.
- The Combat preview is standalone and is not yet integrated into the main screen.
- Other activity modes are not specified or implemented.

## 8. UI Review Criteria

Evaluate proposed UI changes against these questions:

1. Can the player identify the current context and available choices?
2. Does each panel have a distinct responsibility?
3. Does the layout preserve the persistent character state without clutter?
4. Does each activity receive a presentation suited to its information and decisions?
5. Are current state, historical events, observations and inferences distinguishable?
6. Does the design avoid adding navigation or panels that duplicate the Main Panel's role?
