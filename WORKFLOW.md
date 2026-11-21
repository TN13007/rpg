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
- The player chooses activities and personal goals rather than following a mandatory central story.
- The Academy and nearby town provide the initial exploration and social space.
- Authored Locations, Actions, Effects and Events are the main content-building tools for the first slice.
- NPC autonomy and more advanced simulation are added only when a playable scenario needs them.
- Combat and other challenging activities may exist as optional content; they are not the immediate product priority.
- The world scale and larger lore remain open.

The first player-facing target is **Opening Day**: begin at a horse-carriage location around 07:00–08:00, travel to the Academy, follow the students or meet Mira, attend the opening ceremony, eat at the canteen, explore a small set of Academy/town locations and return to the Dormitory to sleep. Use the existing protagonist data; character creation is not in this slice. See [Game Vision](Docs/Design/GameVision.md), [World and Narrative](Docs/Design/WorldAndNarrative.md), [Roadmap](Docs/Design/Roadmap.md) and [Issue #103](https://github.com/TN13007/rpg/issues/103).

### Implementation reality on main

The current build has a location-based Exploration prototype, GameClock day/minute tracking, Sleep/recovery actions and a four-person Tactical Timeline combat prototype. The current Exploration flow uses authored Location Actions, Travel choices and a narrow Courtyard Goblin Event. Combat includes Ari, Bram, Mira and Sera against one Goblin archetype, with Timeline, player decision pauses, Recovery Cost, Delay/Stun, Downed/Revive and Victory/Defeat.

The existing Action Effect types include RestoreHealth, RestoreFatigue, RestoreStamina, ChangeMood, AddGold and Revive. The existing Location Event presents one authored description and a set of choices; it does not yet provide a multi-page narrative sequence. The player currently starts from hardcoded protagonist data and the Dormitory. Do not infer that a horse-carriage opening, Academy arrival routes, dialogue system, multi-page ceremony, general NPC presence or a complete town already exist. Verify source and tests before treating any of these as implemented.

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

Start by reshaping the existing Academy content and Location graph, not by building new universal systems. Trace the current travel, action/effect, event and sleep paths, then deliver:
1. Start the game at a horse-carriage Location around 07:00–08:00.
2. Add Academy arrival and two connected routes: follow the crowd or meet Mira; both reach the ceremony.
3. Author the ceremony using the existing event description and choices. Add only a small sequential-passage behavior if the current one-description event is insufficient.
4. Add the canteen meal using RestoreStamina and simple authored interactions with Mira/students.
5. Add a small set of Academy/town Locations and connect evening travel back to the existing Dormitory.
6. Reuse the existing eight-hour Sleep action; verify rollover and the complete journey in Godot.

Do not add a ChangeLocation Effect when existing Travel already changes Location. Do not build crowd simulation, character creator, general dialogue/quest frameworks, universal NPC schedules or a new Energy resource for this slice unless a concrete runtime limitation requires it.

Deferred work:
- [Issue #91](https://github.com/TN13007/rpg/issues/91): four-person Tactical Timeline encounter.
- [Issue #79](https://github.com/TN13007/rpg/issues/79): party progression backlog.
- [Issue #62](https://github.com/TN13007/rpg/issues/62): manual Sleep/GameClock runtime verification; reuse its checks for the Opening Day end state.

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
