# WORKFLOW — Project Status and Session Handoff

**Repository:** [TN13007/rpg](https://github.com/TN13007/rpg)  
**Engine:** Godot 4.7.2 .NET / C#  
**Last reviewed:** 2026-11-22  
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

The accepted product direction is a player-directed modern student-life RPG:
- The protagonist is an ordinary university student in a contemporary city.
- Classes, studying, friends, part-time work, money and personal routines remain the everyday foundation.
- Paranormal incidents exist as optional opportunities discovered through rumors, conversations and local information.
- The player may investigate, ignore, leave or seek professional help; combat is not mandatory for every anomaly.
- Phones, laptops and the Internet are ordinary tools, not magical artifacts.
- Discrete shared-AP turn-based combat remains a specialized rules domain for situations that justify confrontation.

The former fantasy Academy setting is superseded. New player-facing content must use the modern setting. The design source of truth is [Game Vision](Docs/Design/GameVision.md), [World and Narrative](Docs/Design/WorldAndNarrative.md), [Combat](Docs/Design/Combat.md) and [Product Roadmap](Docs/Design/Roadmap.md).

### Implementation reality

The refactor branch replaces the Academy content with a modern city graph in `Data/Locations.json`, renames the location catalog to `LocationCatalog`, and updates the startup path. The authored setting includes university, student housing, campus facilities, a part-time café, East District locations, a paranormal response office and the Unit 213 incident.

The four-person combat party is now Alex, Noah, Mira and Sera, with an Unstable Presence as the authored enemy. Daily schedules and interactions are authored for Mira, Noah, Sera and Ms. Carter. The Unit 213 event supports observation, cautious entry and retreat without forcing combat.

The modern student-life/city refactor is already merged into `main`; Issue #110 is closed. The four-person Party is Alex, Noah, Mira and Sera. Unit 213 remains an optional incident with non-combat responses as well as a playable Combat encounter.

The combat runtime migration is also merged: PR #131 added the shared Domain ActionPool, and PR #133 replaced Timeline/Intent with discrete Player Turn → Enemy Turn rounds. The playable Unit 213 encounter starts with 2 shared AP, each successful Party action costs 1 AP, each living Enemy resolves one authored pattern step per Enemy Turn, and world time/fatigue completion remains one-time at encounter exit. The PR CI passed 202 .NET tests, the Godot 4.7.2 Debug build and headless Main/Combat Preview checks. A full interactive Unit 213 acceptance run is still not recorded.

Issue #134 tracks the Arousal/Tease/Orgasm runtime MVP; do not fabricate meter values in Presentation before that runtime supplies them. Issue #135 / PR #136 is implementing the three-region Combat HUD: Enemy list (name/HP/Lust), large selected-character portrait, Party Bar (name/HP/Lust/MP) and a three-column command/action/details panel. Selecting non-target actions executes immediately; target-dependent actions move directly into Enemy or ally selection. The prior PR head passed run #509 with 206 .NET tests, Godot 4.7.2 Debug build, headless import/Main/Combat Preview and naming audit. This layout revision still needs its own CI run and interactive Godot acceptance. Lust/Arousal and MP remain optional hidden UI fields until actual runtime values and content assets exist.

The runtime still does not provide durable incident flags, delayed incident progression, a social-feed interface or actual professional dispatch/payment behavior. Do not add a generic Quest, Incident, Social Network or World Simulation framework without a concrete player-facing requirement.

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

**Immediate implementation priority:** [Issue #135 — Character-focused, data-driven Combat UI](https://github.com/TN13007/rpg/issues/135), tracked in [PR #136](https://github.com/TN13007/rpg/pull/136).

PR #136 moves the combat UI to Presentation-owned view models and reusable scenes. The target layout is EnemyRegion + CharacterPortrait above a four-member Party Bar and a fixed three-column Action Panel. Confirm buttons are removed: non-target skills execute on selection, Enemy-targeted skills highlight valid Enemies and execute on click, and ally-targeted skills show valid allies. The header shows Round and remaining shared AP. The latest CI run on the changed head must pass before this revision is considered code-verified. Then perform an interactive Godot 4.7.2 acceptance run with one and multiple Enemies, all four Party members, portrait changes, optional meter visibility, action categories/details, immediate action execution, valid/invalid target highlight/click/cancel, ally target selection, feedback cues, End Turn and return to Exploration.

[Issue #91 — Turn-Based Combat, UI Redesign and Arousal MVP](https://github.com/TN13007/rpg/issues/91) remains the umbrella acceptance issue. [Issue #134](https://github.com/TN13007/rpg/issues/134) owns Tease/Arousal/Orgasm runtime behavior; the UI must show only real meter values once that system is integrated. [Issue #79](https://github.com/TN13007/rpg/issues/79) remains a later progression backlog item.
Do not begin autonomous NPC simulation, broad Character abstractions or a general refactor unless a demonstrated player-facing requirement needs them.

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
