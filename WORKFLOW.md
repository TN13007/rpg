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
- Combat is a specialized, discrete turn-based interaction. The player controls a four-person Party through one shared Action Point pool; enemies act through authored patterns and conditions.

The former fantasy Academy setting is superseded. New player-facing content must use the modern setting. The current combat target is defined in [Combat Design](Docs/Design/Combat.md) and is tracked by [Issue #91 — Turn-Based Combat, UI Redesign and Arousal MVP](https://github.com/TN13007/rpg/issues/91).

### Implementation reality

The latest `main` includes the modern student-life setting from PR #112 and the mixed-threat Unit 213 encounter from PR #127. Treat `main` as the moving implementation baseline; do not pin a commit SHA in this handoff document.

The authored world includes university, student housing, campus facilities, the East District and the Unit 213 incident. The current playable combat composition contains Alex, Noah, Mira and Sera against two Unit 213 manifestations: Unstable Presence and Unstable Echo. Daily NPC schedules and character-specific interactions are authored separately from Location actions.

**The runtime on main is still Timeline-based.** It still uses CombatTimeline, Combat Clock, Remaining Progress, Speed, Recovery Cost, Stun/Delay and Enemy Intent. The current Combat UI still presents Timeline and Intent data. These are implementation facts, not the target design.

Turn-based shared AP, Action Capacity training, Enemy action patterns and the redesigned combat UI are design targets until the implementation refactor is merged. Do not update Implementation documentation to claim those features exist before the code, tests and Godot runtime are migrated.

The current world runtime does not yet provide durable incident state, delayed incident progression, a social-feed interface or actual professional dispatch/payment behavior. Do not add a generic Quest, Incident, Social Network or World Simulation framework without a concrete player-facing requirement.

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

**Immediate direction:** finish documenting and reviewing the discrete turn-based combat design under [Issue #91](https://github.com/TN13007/rpg/issues/91), then perform its Phase 0 implementation audit before changing runtime code.

The target combat loop is:

1. Player Turn begins with the Party's shared Action Point pool refreshed to its current Action Capacity.
2. The player spends AP on skills from eligible Party members; each member may perform at most one AP-funded action per Round.
3. The player may end early; unused AP does not carry over.
4. Each living Enemy resolves one authored pattern action during Enemy Turn, using current conditions and deterministic encounter order.
5. If the encounter continues, the next Round begins and Party AP refreshes.

Action Capacity starts at 2 AP and can reach 4 AP through two team-training milestones (2 → 3 → 4). The activity requirements and costs remain open for playtesting.

**Important migration boundary:** Issue #91 owns replacing the current Timeline/Intent runtime, translating Downed/Revive, adding the HP/Arousal MVP, redesigning UI, and validating the complete Exploration → Combat → Exploration journey. Keep the existing Implementation documents accurate about the Timeline runtime until that migration is actually complete.

Issue #117's Timeline/Intent foundation is superseded by the discrete turn-based direction in Issue #91. Its Stance and Accept/Continue concepts are not automatically carried into the new model; they remain deferred and require a separate design issue if revived. Issue #79 remains a later progression backlog item and should not be pulled into the combat refactor without a concrete dependency.

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

Do not maintain a historical table of every Session or duplicate completed implementation details from PRs and Issues. Keep only current blockers and useful learning evidence; PRs and Issues retain delivery history.
