# WORKFLOW — Project Status and Session Handoff

**Repository:** [TN13007/rpg](https://github.com/TN13007/rpg)  
**Engine:** Godot 4.7.2 .NET / C#  
**Last reviewed:** 2026-11-23  
**Baseline:** `main` at `8f2182e`; open or unmerged PRs are proposals, not implemented behavior.

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

The accepted product direction is a player-directed modern student-life RPG. The complete experience is built from cumulative gameplay layers:

- **Loop 1 — Foundation:** time, characters, locations, actions, resources and progression. It is used most frequently because every higher layer depends on it.
- **Loop 2 — Daily Life:** classes, studying, work, money, rest, relationships, shops and city exploration. It gives the player a repeatable everyday context.
- **Loop 3 — Specialized Activities:** social interaction, combat, romance/intimacy, investigation, recreation and other activities that add rules and decisions beyond ordinary daily actions.
- **Loop 4+ — Extended Gameplay:** deeper combinations of party, skills, relationships, activities and persistent outcomes.

Each layer builds on the capabilities of the previous layers. This is a dependency model, not a mandatory order of player actions. The intended identity combines the everyday freedom of *Degrees of Lewdity*, the party-based RPG and character-interaction direction of *Karryn's Prison*, and a stronger emphasis on practical skill-driven gameplay.

The protagonist is an ordinary university student in a contemporary city. Paranormal incidents are optional; the player may investigate, ignore, leave or seek professional help. Combat is a specialized turn-based activity, not the mandatory response to every unusual situation.

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

**Active Loop 2 implementation task:** [Issue #160 — Add Gregorian campaign dates to GameClock](https://github.com/TN13007/rpg/issues/160), proposed in the current branch `feat/issue-160-gregorian-game-calendar`.

PR #159 is merged at `8f2182e`; GameClock derives weekday from campaign Day. PR #157 is also merged. Action Effects use explicit positive-amount directional operations, and Gold is a signed balance that may represent debt. This is a shared foundation change; it does not implement purchase validation or a general transaction framework.

The current calendar task adds a Gregorian campaign date beginning Monday, 1 September 2025, derives weekday/date from the same GameClock, displays the calendar in Exploration and Combat time feedback, and preserves the authored 480-minute Sleep behavior. Campaign Day/date/weekday currently serve time progression and calendar presentation only; no gameplay system may use them for availability, daily throttles or reset rules yet. Location Event and future Quest repeat/cooldown/completion rules are deferred to a dedicated system. No daily reset, schedule framework or Loop 3 gameplay is included.

After the calendar is reviewed and merged, continue the Loop 2 implementation sequence:
1. Add the shared scheduled-activity data contract and availability query.
2. Implement atomic Gold spending/adjustment at the owning activity boundary, consistent with the signed-balance design.
3. Add only the typed, domain-owned persistent records required by the selected Loop 2 slice.
4. Extend selected NPC presence schedules with weekday restrictions while preserving every-day behavior when weekdays are omitted.
5. Validate a complete ordinary student-life journey in Godot without making Loop 3 activities acceptance requirements.

Keep each later capability in its own focused Issue and acceptance criteria.

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
