# Product Roadmap

**Status:** Directional player-facing sequence  
**Last updated:** 2026-11-22

This roadmap is organized around outcomes the player can experience. It is not an implementation task list. Current tasks, blockers and delivery status belong in GitHub Issues/PRs and [WORKFLOW.md](../../WORKFLOW.md).

## Current delivery focus

The active direction is a single modern student-life setting. The former fantasy Academy content has been replaced, not retained as a parallel scenario.

- [Issue #110 — Modern student life and paranormal incident](https://github.com/TN13007/rpg/issues/110) tracked the setting migration and first bounded incident journey; it is closed.
- The first content slice centers on university, student housing, a part-time café, the East District and the Unit 213 rumor.
- The first incident supports observation and retreat without forcing combat. Professional response is introduced as an ordinary service, while actual dispatch/payment behavior remains a later runtime gap.
- [Issue #91 — Turn-Based Combat, UI Redesign and Arousal MVP](https://github.com/TN13007/rpg/issues/91) owns the replacement of the current Timeline combat runtime with discrete Party and Enemy Turns using shared Action Points.

Combat should be available for situations that justify it, not mandatory for every anomaly. Preserve reusable runtime behavior and avoid introducing a universal Quest, Incident, Social Network or World Simulation framework.

## Phase 1 — Establish a believable student routine

Deliver a connected daily-life journey across campus, housing and nearby city locations. The player can attend a class, study, eat, meet friends, work a shift, rest and travel between locations. NPC schedules should make the world feel occupied without requiring autonomous simulation.

**Exit criteria**
- The starting location and route through the university are coherent.
- Everyday actions use the existing GameClock and supported effects.
- At least three NPCs have recognizable daily routines and available interactions.
- The player can complete a day without being forced into a paranormal event.

## Phase 2 — Prove an optional paranormal incident

Deliver one bounded incident, beginning with a rumor and ending with a player-chosen response. Unit 213 is the first candidate: reports describe a child crying at the same time each night in a vacant apartment.

**Exit criteria**
- The player can discover the rumor through ordinary conversation or local information.
- The player can investigate, observe, enter cautiously or leave.
- The player can ignore the incident and continue ordinary life.
- The event does not require combat to reach a valid outcome.
- The narrative distinguishes what the player observed from what is known to be true.

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

## Phase 5 — Integrate discrete Party combat journeys

Deliver a compact turn-based combat experience for confrontations where the player chooses to fight. The player spends a shared Action Point pool across the four-person Party, while enemies respond through authored action patterns and conditions. Training can gradually increase the Party's Action Capacity.

**Exit criteria**
- The player understands the Player Turn / Enemy Turn loop without tracking a real-time Timeline.
- The Party starts with 2 shared AP; team-training milestones can raise Action Capacity to 3 AP and then 4 AP.
- Each Party member is limited to one AP-funded action per Round by a one-Round actor cooldown.
- Enemy patterns create readable, deterministic pressure; current condition rules/states are visible without exposing the next action or target.
- The Attack/HP loop is playable. Arousal/Orgasm is included only if the product owner reconfirms it as part of the MVP.
- Downed and revival work within turn and AP rules.
- A complete encounter can return to the same Location with its configured consequences.
- Non-combat approaches remain valid when appropriate.

## Phase 6 — Expand combat only where playtests justify it

Use the first playable encounter to decide whether additional control states, enemy pattern branches, further resource interactions or deeper training activities create meaningful decisions. The first slice already includes a one-Round actor cooldown and two Action Capacity training milestones. Do not rebuild the old Timeline under different names.

**Exit criteria**
- Every added mechanic creates an observable player decision.
- Additional complexity improves Party combinations or enemy responses without hiding the turn structure.
- The player can understand action costs, conditions and consequences from the interface.
