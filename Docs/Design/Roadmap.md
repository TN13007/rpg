# Product Roadmap

**Status:** Directional player-facing sequence  
**Last updated:** 2026-11-24

This roadmap is organized around outcomes the player can experience. It is not an implementation task list. Current tasks, blockers and delivery status belong in GitHub Issues/PRs and [WORKFLOW.md](../../WORKFLOW.md).

## Current delivery focus

The active direction is a single modern student-life setting. The former fantasy Academy content has been replaced, not retained as a parallel scenario.

- The first content slice centers on university, student housing, a part-time café, the East District and the Unit 213 rumor.
- The first supernatural episode should connect a friend invitation, investigation and a party-based turn-based encounter, then return the player to ordinary student life.
- Combat remains optional when the player chooses to back out or seek help; it is not the mandatory response to every anomaly.

The setting migration should preserve reusable runtime systems and avoid introducing a universal Quest, Incident, Social Network or World Simulation framework.

## Phase 1 — Establish a believable student routine

Deliver a connected daily-life journey across campus, housing and nearby city locations. The player can attend a class, study, eat, meet friends, work a shift, rest and travel between locations. NPC schedules should make the world feel occupied without requiring autonomous simulation.

**Exit criteria**
- The starting location and route through the university, housing and nearby city are coherent.
- The player can complete a representative weekday from morning through Sleep and the next-day transition.
- A smaller weekend contrast demonstrates optional extracurricular practice and social opportunities using the same GameClock and domain-owned state.
- At least one scheduled class or work commitment competes meaningfully with flexible study, social or recreation choices.
- A small set of ordinary activities demonstrates both earning and spending money without requiring a city-wide economy.
- Everyday actions use the existing GameClock and Character state; NPC presence and interactions refresh as time advances.
- At least three NPCs have recognizable routines and available interactions.
- The player can complete a day without being forced into a paranormal event or Combat.

The detailed Loop 2 design contract is maintained in [Daily Life](DailyLife.md).

## Phase 2 — Prove the student-life-to-supernatural loop

Deliver one bounded episode using Unit 213: a friend introduces the lead, the group investigates the abandoned apartment, the player chooses whether to enter, and the existing party combat resolves the confrontation. The player then returns to the ordinary world.

**Exit criteria**
- The player can discover the lead through an existing friend interaction.
- The player can inspect clues and choose to confront the presence or back out.
- The confrontation uses the existing turn-based four-character Party combat.
- Combat completion returns the player to Exploration without breaking the ordinary journey.
- The aftermath makes the supernatural episode feel connected to student life.
- The episode works without introducing a generic Quest or Episode framework.

## Phase 3 — Add consequences only where proven necessary

Use the first incident to identify which facts must persist: whether the player investigated, what they learned, whether the location was reported and whether the response company became involved.

**Exit criteria**
- One concrete later scene reacts to a prior incident outcome.
- The player can understand why that scene changed.
- State is stored by the smallest suitable mechanism; no generic world simulation is introduced.

## Phase 4 — Expand the social and city layer

Add more classmates, coworkers, neighbors, local rumors and professional contacts when they create new player decisions. A phone or online community interface may be introduced only when the player journey benefits from it.

**Exit criteria**
- New content has distinct player-facing value.
- NPC schedules and interactions remain understandable and testable.
- Optional incidents coexist with daily-life activities rather than replacing them.

## Phase 5 — Integrate specialized combat journeys

Continue improving the discrete turn-based combat system as a separate domain. Connect it to the world only through concrete situations that require confrontation and define how injury, recovery and elapsed time affect daily life.

**Exit criteria**
- At least one complete journey can enter combat and return to the city with a visible consequence.
- Non-combat approaches remain valid when appropriate.
- Combat complexity is justified by player-facing decisions, not by a desire to simulate every threat.
