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

The intended product is a player-directed fantasy life-simulation RPG set in a connected world:
- Daily life, social interaction, exploration, adventures and tactical combat are different interaction contexts within the same continuing world.
- The Academy is an important place to live, study, train, rest and meet people; it is not merely a supporting menu for Combat.
- Combat remains a specialized four-person Tactical Timeline experience with its own rules and execution flow.
- Relevant choices and outcomes should affect character/world state and may change later opportunities across contexts.
- The player is not forced into one fixed Academy → Adventure → Combat loop; personal goals and interests shape the journey.

The accepted direction and unresolved design questions are owned by [Design](Docs/Design/README.md), especially [Game Vision](Docs/Design/GameVision.md), [World and Narrative](Docs/Design/WorldAndNarrative.md), [Combat](Docs/Design/Combat.md) and the [Product Roadmap](Docs/Design/Roadmap.md). Do not copy detailed rules or provisional balance values into this status file.

### Implementation reality on main

The current build contains an authored Opening Day location-based Exploration slice and a four-person Tactical Timeline Combat prototype. Exploration supports Location travel, context-sensitive Actions, authored Location Events, GameClock/Sleep and recovery actions. Combat composes Ari, Bram, Mira and Sera against one Goblin archetype; it has a shared real-time Combat Timeline, player decision pauses, action-specific Recovery Cost, Delay/Stun, Downed/Revive, Victory/Defeat and the post-encounter rescue path.

These are specialized runtime flows that coexist in the same game; the code does not yet provide a universal Interaction executor or a complete autonomous world simulation. The full Opening Day Godot journey and the complete interactive Combat encounter still require their documented runtime verification. Relevant outcomes should be connected across contexts through concrete player-facing requirements, not by assuming a generalized simulation framework already exists. Read [Implementation](Docs/Implementation/README.md) for verified code structure and runtime behavior.

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

**Immediate implementation priority:** [Issue #91 — Four-Person Tactical Timeline Encounter](https://github.com/TN13007/rpg/issues/91).

The immediate Combat increment remains Enemy Intent plus a second enemy behavior pattern, used in one encounter that creates competing threats and meaningful player responses. This completes the dedicated Combat context; it does not make the whole game Combat-first. Before implementation:
- Read [Game Vision](Docs/Design/GameVision.md), [Combat Design](Docs/Design/Combat.md) and [Roadmap Phase 1](Docs/Design/Roadmap.md).
- Read [Cross-Layer Runtime Systems](Docs/Implementation/CrossLayerRuntimeSystems.md) and trace the current Combat flow, source code and tests.
- Keep intent presentation separate from authoritative enemy action resolution; visible intent must not silently change.
- Keep [Issue #103 — Opening Day](https://github.com/TN13007/rpg/issues/103) for the complete manual journey from carriage through the Academy day and Sleep; the content is merged in PR #105, but runtime acceptance remains tracked by that Issue.
- Retain [Issue #62](https://github.com/TN13007/rpg/issues/62) for manual Sleep/GameClock runtime verification.
- Keep [Issue #79](https://github.com/TN13007/rpg/issues/79) as a later progression backlog item; sequence it according to the updated Roadmap rather than assuming it is automatically the next phase after Combat.

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
