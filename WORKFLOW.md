# WORKFLOW — Project Status and Session Handoff

**Repository:** [TN13007/rpg](https://github.com/TN13007/rpg)  
**Engine:** Godot 4.7.2 .NET / C#  
**Last reviewed:** 2026-11-24  
**Baseline:** `main` at `6f73c887`; open or unmerged PRs are proposals, not implemented behavior.

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

The accepted product direction is a high-energy student-life RPG with supernatural adventures. By day, the player attends classes, plays sports, socializes, dates, works and enjoys campus life. After hours, the same student group investigates strange places and fights supernatural entities through the existing party-based turn-based combat.

Daily life is enjoyable in its own right and also develops the capabilities, money, relationships and party support that make adventures possible. Supernatural episodes return new outcomes and consequences to ordinary life. The tone is energetic, funny and suspenseful rather than grimdark.

The protagonist is an ordinary university student in a contemporary city. Paranormal incidents remain optional: the player may investigate, ignore, leave or seek help. Combat remains turn-based and is not the mandatory response to every unusual situation.

The design source of truth is [Game Vision](Docs/Design/GameVision.md), [World and Narrative](Docs/Design/WorldAndNarrative.md), [Combat](Docs/Design/Combat.md), [Progression](Docs/Design/Progression.md) and [Product Roadmap](Docs/Design/Roadmap.md).

### Implementation reality

The modern student-life/city refactor is merged into `main`; Issue #110 is closed. The authored setting includes the university, student housing, campus facilities, a part-time café, East District locations, a paranormal response office and the Unit 213 incident. The four-person Party is Alex, Noah, Mira and Sera. Unit 213 supports observation, cautious entry and retreat without forcing combat.

The combat runtime migration is merged. PR #131 added the shared Domain ActionPool, and PR #133 replaced Timeline/Intent with discrete Player Turn → Enemy Turn rounds. The playable Unit 213 encounter starts with 2 shared AP; each successful Party action costs 1 AP; each living Enemy resolves one authored pattern step per Enemy Turn; world time and Fatigue are applied once at encounter completion.

The three-region Combat HUD is also merged through PR #136, and Issue #135 is closed. It contains the Enemy region, selected-character portrait, four-member Party Bar and three-column Action Panel. Non-target actions execute on selection; target-dependent actions enter target selection directly. PR #136 CI passed 211 .NET tests, the Godot 4.7.2 Debug build, headless import/Main/Combat Preview checks and the repository naming audit. **Interactive Godot acceptance of the complete Unit 213 HUD journey is still not recorded.**

The documentation alignment was merged through PR #139 (merge commit `f736c065`). The Design documentation now describes the layered gameplay vision and discrete shared-AP combat. PR #138 is an older duplicate of that documentation change and remains open; it is superseded by PR #139.

Current runtime gaps and active follow-up work:

- [Issue #134](https://github.com/TN13007/rpg/issues/134) owns the Tease/Arousal/Orgasm MVP. Arousal/Lust is not yet supplied by the Combat runtime; Presentation must not fabricate values.
- [Issue #137](https://github.com/TN13007/rpg/issues/137) owns the derived MP resource. MP is not yet supplied by the Character/Combat runtime; the HUD must keep the optional field hidden until authoritative values exist.
- The portrait contract supports pose, expression, outfit and status layers, but current content still uses a generic placeholder portrait.
- Combat events are currently presented synchronously; sequential event/animation playback remains follow-up work.
- Durable incident flags, delayed incident progression, a social-feed interface and actual professional dispatch/payment behavior are not implemented. Do not add a generic Quest, Incident, Social Network or World Simulation framework without a concrete player-facing requirement.

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

**Latest baseline:** `main` at `6f73c887cd95cc3a69f2e24c314dd4eef2b25773`. PR #168 (dormitory-to-campus opening journey) and its dependent Fast Travel work are merged. The current runtime starts Alex in `dormitory_room` at 07:30 and contains the campus, East District, scheduled NPCs, location events and the Unit 213 turn-based encounter.

**Active implementation:** [Issue #172 — Connect student life to the first supernatural episode](https://github.com/TN13007/rpg/issues/172), [Draft PR #173](https://github.com/TN13007/rpg/pull/173), branch `feat/issue-172-first-supernatural-episode`, based directly on latest `main`.

The branch currently:
- Gives Mira a direct invitation to investigate the abandoned apartment.
- Rewrites the Unit 213 event with visible clues and choices to confront or back out.
- Routes the confrontation choice to the existing four-character turn-based encounter.
- Updates the encounter title/opening narration and adds content assertions.
- Revises Game Vision, World and Narrative, and Roadmap to reflect the accepted student-life + supernatural adventure direction.

**Verification status:** PR #173 is Draft. GitHub Actions run #641 passed on head commit `249bae2bcae6114007efc43c7b9656996bd68f53`: repository validation, 265 .NET tests, Godot 4.7.2 .NET build, import/main-scene/combat-preview smoke checks and naming audit. No interactive playthrough has been performed. Do not merge or mark Issue #172 complete until the full dormitory → Mira invitation → evening meetup → Old Apartment Block → combat → return-to-Exploration journey has been manually verified.

**Important boundaries:** Do not add a generic Quest/Episode/Incident framework, replace turn-based combat, or expand into broad relationship/economy/equipment systems for this slice. Use the existing content and runtime contracts unless a concrete player-facing blocker proves a change is necessary.

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
