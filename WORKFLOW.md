# WORKFLOW — Project Status and Session Handoff

**Repository:** [TN13007/rpg](https://github.com/TN13007/rpg)  
**Engine:** Godot 4.7.2 .NET / C#  
**Last reviewed:** 2026-11-23  
**Baseline:** `main` at `c4d81766`; open or unmerged PRs are proposals, not implemented behavior.

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

The accepted product direction is a player-directed modern student-life RPG built from cumulative gameplay layers. Loop 1 provides the shared Foundation; Loop 2 provides repeatable Daily Life; Loop 3 provides specialized activities. See [Game Vision](Docs/Design/GameVision.md) and [Daily Life](Docs/Design/DailyLife.md) for the design source of truth.

The modern student-life/city runtime and the discrete shared-AP Combat migration are merged. The Combat HUD is merged; its complete interactive Godot acceptance journey remains a separate verification item. Issues #134 and #137 remain separate Combat follow-ups.

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

**Active implementation task:** [Issue #154 — Prepare Action execution before applying runtime state](https://github.com/TN13007/rpg/issues/154).

The current Action runtime combines eligibility checks, time calculation, clock advancement, Fatigue mutation and Effect application in one execution path. The refactor introduces a prepare-before-apply boundary while preserving the authored Action JSON contract and existing gameplay behavior.

The intended boundary is:

- **Prepare:** revalidate the actor, calculate actual duration and Fatigue, verify the GameClock can advance, and validate/snapshot all configured effects without changing runtime state.
- **Apply:** confirm the prepared Character and GameClock state has not changed, then advance time and apply the prepared Fatigue/effects once.
- **Ownership:** Character and GameClock continue to own their state. The Action service orchestrates; calculators calculate; the Effect executor prepares and applies authored effects.

This is a synchronous Action execution boundary, not a general transaction framework. It does not implement Gold spending, Membership/Knowledge requirements, weekday commitments or start windows. Those remain separate Loop 2 implementation tasks.

After this refactor, use the local Godot 4.7.2 .NET environment to run the .NET test suite and the relevant headless/build checks. Passing CI does not replace local runtime acceptance or demonstrate independent learning.

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
