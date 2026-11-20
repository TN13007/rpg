# WORKFLOW — Project Status and Session Handoff

**Repository:** [TN13007/rpg](https://github.com/TN13007/rpg)  
**Engine:** Godot 4.7.2 .NET / C#  
**Last reviewed:** 2026-11-20  
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

The intended product is a combat-first Tactical Timeline RPG:
- Four active party members with full player control.
- No battlefield grid or front/back positioning.
- A visible Timeline that makes action order and timing tactically meaningful.
- Challenging encounters built around party synergy, control, resource decisions and enemy behavior.
- The Academy is a supporting hub for preparation, growth, recovery and relationships.
- Adventures and combat are the primary repeatable gameplay; narrative gives the journey meaning.

The accepted direction and unresolved design questions are owned by [Design](Docs/Design/README.md), especially [Combat](Docs/Design/Combat.md). Do not copy detailed rules or provisional balance values into this status file.

### Implementation reality on main

The current repository is still an earlier Academy/location/Event/Goblin prototype. It has a Godot main scene, layered C# code, authored JSON content, Location-based Exploration choices, a deterministic single-Goblin Combat flow, character resources and a dedicated Combat presentation.

The current implementation does **not** yet realize the intended four-person Timeline combat, party synergy, Downed/revival loop or the full adventure/progression experience. Read [Implementation](Docs/Implementation/README.md) for verified code structure and runtime behavior. Do not use the old Version 1 Session plan as the current product roadmap.

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

- Choose the next product/design task from an existing GitHub Issue or create one using the appropriate template.
- Do not infer a next Session from the archived Version 1 plan.
- Before starting a task, confirm whether it changes the intended design, the current implementation, or both. Update the owning source of truth only.

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
