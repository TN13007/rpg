# Product Roadmap

**Status:** Directional player-facing sequence  
**Last updated:** 2026-11-21

This roadmap is organized around outcomes the player can experience. It is not an implementation task list. Current tasks, blockers and delivery status belong in GitHub Issues/PRs and [WORKFLOW.md](../../WORKFLOW.md).

## Current delivery focus

The current build contains two important prototype slices in the same game: the authored Opening Day Location/Action/Event journey and a four-person Continuous Tactical Timeline encounter. They prove that daily-life interactions and specialized combat can coexist in the repository, but they do not yet demonstrate a fully verified journey connecting a world situation, a Combat encounter and its consequences on return.

- [Issue #103 — First playable Academy day](https://github.com/TN13007/rpg/issues/103) tracks the Opening Day player journey; its content was added in [PR #105](https://github.com/TN13007/rpg/pull/105). The complete manual Godot journey remains to be verified.
- [Issue #91 — Four-Person Tactical Timeline Encounter](https://github.com/TN13007/rpg/issues/91) remains the active Combat delivery target. It completes the specialized Combat context; it does not define the entire game's identity.

Do not treat archived Academy Session plans or closed/deferred Issues as the current implementation queue. Party progression ([Issue #79](https://github.com/TN13007/rpg/issues/79)) remains a later growth milestone, not an automatic next step before the world-interaction direction is proved.

## Phase 1 — Prove a connected world journey

Deliver a small, understandable player journey in which ordinary life and danger belong to one continuing world. The player can move through Locations, spend time on everyday interactions, encounter an authored situation, enter the specialized Combat flow when appropriate, and return to a world context where the relevant outcome is visible.

**Exit criteria**

- The player can navigate a connected set of Locations and understand the current context.
- Daily actions and authored Event choices produce readable feedback and relevant state changes.
- At least one world situation can lead into the dedicated Combat flow without making Event and Combat the same system.
- Combat uses its own Timeline and rules; daily-life time and Combat Time are not conflated.
- Victory and defeat return through a coherent encounter lifecycle.
- Relevant encounter outcomes are reflected in the continuing character/world context rather than silently resetting.
- The journey can be completed and understood in a Godot runtime session, not only through isolated unit tests.

**Current linked work:** [#103 — Opening Day](https://github.com/TN13007/rpg/issues/103) and [#91 — Tactical Timeline Encounter](https://github.com/TN13007/rpg/issues/91).

## Phase 2 — Make interactions respond to world and character state

Turn important authored interactions from fixed choices into context-sensitive choices when a real player journey requires it. Start with a small number of conditions and outcomes that make revisiting people or places meaningfully different.

**Exit criteria**

- At least one authored choice changes a later available interaction or response.
- At least one important character or Location reacts differently to a prior player action.
- The player can understand why an option is available, unavailable or changed.
- State changes are validated and tested through the owning gameplay domain.
- No general-purpose quest, dialogue, event scheduler or NPC simulation framework is introduced without a concrete requirement.

## Phase 3 — Prove growth across different activities

Deliver a compact progression loop in which study, training, relationships, exploration and rewards can expand the player's options. Growth should influence more than combat statistics.

**Exit criteria**

- The player makes meaningful choices about what capability or opportunity to pursue.
- At least two viable development directions lead to different available actions or approaches.
- Equipment, skills or knowledge change decisions rather than only increasing numeric output.
- Growth can be demonstrated in more than one interaction context.

## Phase 4 — Prove evolving adventures and threats

Deliver a compact adventure or developing threat that competes with the player's other goals and can be approached in more than one way.

**Exit criteria**

- The player understands what is at stake and can choose whether and how to respond.
- Preparation, exploration and encounters create different decisions.
- Partial success, failure or inaction can produce proportionate consequences.
- The player's actions can change a Location, character circumstance or later opportunity.

## Phase 5 — Deliver the first complete narrative arc

Connect the player's current life, relationships and wider challenges into a story with meaningful motivations, choices and consequences.

**Exit criteria**

- The player can explain the central conflict and personal stakes.
- Major choices produce observable consequences.
- NPCs and Locations reflect important events.
- The arc has a coherent beginning, escalation and resolution without requiring every optional activity.

## Phase 6 — Expand the world

Add Locations, characters, enemy families, personal goals, narrative arcs and long-term world challenges based on evidence from earlier playable journeys.

Do not expand breadth before the existing player experience is satisfying.

## Scope rule

A phase is complete when its player-facing exit criteria can be demonstrated in a playable build. A document, architecture layer or collection of systems is not by itself proof of a complete player experience.
