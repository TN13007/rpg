# Product Roadmap

**Status:** Directional player-facing sequence  
**Last updated:** 2026-11-21

This roadmap is organized around outcomes the player can experience. It is not an implementation task list. Current tasks, blockers and delivery status belong in GitHub Issues/PRs and [WORKFLOW.md](../../WORKFLOW.md).

## Current delivery focus — Opening Day

The current product direction is an open-ended Life Simulation RPG. The first delivery target is [Issue #103 — Deliver the first playable Academy day](https://github.com/TN13007/rpg/issues/103).

Build one authored day with the existing Location, Travel, Action, Effect and Location Event model. Start at a horse carriage, arrive at the Academy, choose to follow students or meet Mira, attend the opening ceremony, eat at the canteen, explore a small set of locations and return to the Dormitory to sleep.

### Exit criteria

- The day starts around 07:00–08:00 at a carriage location using the existing protagonist data.
- Travel changes Location through the existing exit/travel flow.
- Both arrival routes converge on the same opening ceremony.
- The ceremony presents its authored story passages and choices.
- Canteen actions can advance time and restore Stamina.
- Mira and a few students can be interacted with through authored content.
- The player can explore a deliberately small Academy/town location graph.
- The player can return to Dormitory and use the existing eight-hour Sleep action.
- Automated tests cover any new sequential event behavior and route convergence.
- The complete day is manually verified in Godot.

## Phase 1 — Make one day playable

Deliver the smallest reliable daily-life loop after the opening.

### Exit criteria

- The GameClock advances through activities and handles day rollover.
- The player can travel between a small set of locations.
- Available activities depend on location and relevant conditions.
- Sleep and recovery work consistently.
- State that matters to the next day is preserved.

## Phase 2 — Make the Academy feel inhabited

Add only the NPC behavior needed for visible daily life.

### Exit criteria

- A small authored cast has recognizable identities and locations.
- Selected NPCs follow simple, testable schedules when needed.
- The player can meet, talk to and participate in activities with them.
- NPC availability and event outcomes respond consistently to time and location.
- At least a few meaningful interactions are reflected later.

## Phase 3 — Expand activities and personal progression

Add optional activities based on player interest, such as study, training, work, shopping, crafting, exploration and combat.

### Exit criteria

- Activities offer distinct outcomes and trade-offs.
- The player can choose which activities to pursue.
- Skills, knowledge, resources or relationships change what the player can do.
- No single optional activity becomes a mandatory progression gate without an explicit design decision.

## Phase 4 — Expand the world and its stories

Add more locations, people, events and optional personal stories based on what the first playable region proves useful.

### Exit criteria

- New locations provide distinct reasons to visit.
- NPCs and events create discoverable stories.
- Player actions can cause understandable local consequences.
- World scale and history expand only when supported by actual content needs.

## Existing systems and deferred work

The repository already contains a four-person Tactical Timeline combat prototype and a location-based Exploration prototype. These remain available foundations, but combat is not required by Opening Day and is no longer the immediate product priority.

- [Issue #91](https://github.com/TN13007/rpg/issues/91) remains deferred combat work.
- [Issue #79](https://github.com/TN13007/rpg/issues/79) remains deferred party-progression work.
- [Issue #62](https://github.com/TN13007/rpg/issues/62) remains a manual GameClock/Sleep runtime verification task and can be reused when validating the daily-life loop.

Do not start NPC autonomy, broad progression or combat expansion in parallel with Opening Day unless the opening journey demonstrates a concrete dependency.

## Scope rule

A phase is complete when its player-facing exit criteria can be demonstrated in a playable build. A document, architecture layer or collection of systems is not by itself proof of a complete experience.
