# WORKFLOW — Project Status and Session Handoff

**Repository:** [TN13007/rpg](https://github.com/TN13007/rpg)  
**Engine:** Godot 4.7.2 .NET / C#  
**Last reviewed:** 2026-11-21  
**Baseline:** latest `main`; open or unmerged PRs are proposals, not implemented behavior.

> This is the only changing project-status and handoff tracker. Do not create separate status, Session-history, decision or learning trackers.

## 1. Sources of truth

| Question | Source |
|---|---|
| What game are we trying to make? | [Design documentation](Docs/Design/README.md) |
| What does the current code actually do? | [Implementation documentation](Docs/Implementation/README.md), latest `main` and tests |
| What problem are we solving now? | GitHub Issue |
| What solution was proposed and verified? | Pull Request and its review/CI evidence |
| How do we organize work and Git history? | [CONTRIBUTING.md](CONTRIBUTING.md) |
| What is the current status and next handoff? | This file |

Design decisions do not imply implementation. If the desired experience and current code differ, keep both truths visible and track the gap through an Issue.

## 2. Current project snapshot

### Product direction

The intended product is an open-ended Life Simulation RPG set in a Science-Fantasy world with Renaissance-era civilization and common magic:
- The player creates a character and chooses how to spend time.
- The Academy and nearby town provide the initial social and exploration space.
- NPCs offer relationships, routines and activities without forcing a single story route.
- Content is discovered through places, people, activities and events.
- Combat and other challenging activities may exist as optional content; they are not the immediate product priority.
- The world scale and larger lore remain open.

The first player-facing target is **Opening Day**: character creation, horse-carriage arrival around 07:00–08:00, reaching the opening ceremony by following students or meeting Mira, a concise introduction, free afternoon exploration and returning to the dormitory to sleep. The authoritative direction is in [Game Vision](Docs/Design/GameVision.md), [World and Narrative](Docs/Design/WorldAndNarrative.md) and [Roadmap](Docs/Design/Roadmap.md).

### Implementation reality on main

The current build has a location-based Exploration prototype, GameClock day/minute tracking, Sleep/recovery actions and a four-person Tactical Timeline combat prototype. The current Exploration flow uses authored Location Actions, Travel choices and a narrow Courtyard Goblin Event. Combat includes Ari, Bram, Mira and Sera against one Goblin archetype, with Timeline, player decision pauses, Recovery Cost, Delay/Stun, Downed/Revive and Victory/Defeat.

Do not infer that character creation, horse-carriage arrival, a crowd of students, dialogue, general NPC presence, schedules, relationships, free-roam movement or a complete Academy/town day already exist. Verify source and tests before treating any of these as implemented. See [Implementation](Docs/Implementation/README.md) for current code contracts.

## 3. Session protocol

Each development Session should produce two separate outcomes:

- **Product:** a usable, testable improvement to the game.
- **Learning:** observable evidence that the learner can understand, explain, verify or independently change the relevant part of the project.

A merged feature, AI-generated code or passing CI does not by itself prove independent learning.

1. **Context check:** inspect latest `main`, the active Issue, related PRs, relevant Design and Implementation documents, source code and tests.
2. **Define outcomes:** agree on one product outcome and one practical learning objective.
3. **Implementation Brief:** for ambiguous or cross-layer work, summarize current behavior, evidence, responsibilities, proposed change, constraints and verification. Keep it as a temporary working artifact, not another permanent repository document.
4. **Predict and execute:** the learner explains expected behavior first; implement one focused logical change at a time.
5. **Verify:** run automated checks and the required Godot runtime journey. Compare actual results with the Issue acceptance criteria.
6. **Review and retain:** inspect the complete diff, resolve or record review decisions, update the Issue/PR, and record only meaningful learning evidence here.

The Git workflow, commit granularity and PR requirements are defined only in [CONTRIBUTING.md](CONTRIBUTING.md). Do not duplicate them here.

## 4. Current handoff

**Immediate implementation priority:** [Issue #103 — Deliver the first playable Academy day](https://github.com/TN13007/rpg/issues/103).

Start by inspecting the current character, GameClock, Location/Travel/Action, content loading, save/persistence and presentation flows. Map each Opening Day requirement to existing code, missing behavior and manual verification. Do not assume JSON alone can implement new runtime behavior.

Deliver in small increments:
1. Character creation and opening-day start.
2. Horse-carriage arrival, student/crowd context, route choice and ceremony convergence.
3. Free exploration of a deliberately small Academy/town location set.
4. Dormitory return, Sleep, next-day transition and persistence.
5. Full interactive Godot acceptance from character creation to sleeping.

Keep the scope narrow. Do not introduce universal NPC simulation, broad relationship frameworks, new combat content or a full world model unless the playable journey proves a dependency.

Deferred work:
- [Issue #91](https://github.com/TN13007/rpg/issues/91): four-person Tactical Timeline encounter.
- [Issue #79](https://github.com/TN13007/rpg/issues/79): party progression backlog.
- [Issue #62](https://github.com/TN13007/rpg/issues/62): manual Sleep/GameClock runtime verification; reuse its checks when validating the daily-life loop.

## 5. Learning evidence

Record learning only when it was actually demonstrated. Product completion and learning status are independent.

Use this compact format when a meaningful check occurs:

```text
YYYY-MM-DD — Issue / topic
Product: Done / Partial / Blocked — PR or evidence link
Learning: Not assessed / Needs practice / With support / Independent
Evidence: what the learner explained, changed or verified independently
Remaining gap: one specific concept or skill
Next practice: one small exercise
```

Do not maintain a historical table of every Session or duplicate completed implementation details from PRs. Keep only current blockers and useful learning evidence; PRs and Issues retain delivery history.
