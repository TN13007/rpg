# WORKFLOW — Project Status and Session Handoff

**Repository:** [TN13007/rpg](https://github.com/TN13007/rpg)  
**Engine:** Godot 4.7.2 .NET / C#  
**Last reviewed:** 2026-11-24  
**Baseline:** `main` at `9388aa8`; open or unmerged PRs are proposals, not implemented behavior.

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

**Latest baseline:** `main` at `9388aa807919279c62474a7ebfeba3c2c64abd4f`. The feature-first source migration was merged through PR #164. The campus morning content and Campus Plaza clarification were merged through PR #166, which closes Issue #165. The current main runtime still starts Alex at `university_campus` at 07:30.

**Active implementation:** [Issue #167 — Add the dormitory-to-campus opening journey](https://github.com/TN13007/rpg/issues/167), [Draft PR #168](https://github.com/TN13007/rpg/pull/168), on branch `feat/issue-167-dormitory-opening-journey`.

The proposal changes the opening start to a private `dormitory_room`, adds separate Bathroom and Lobby Locations, and connects the Lobby to the existing Campus Plaza. The room introduces target-first Bed, Laptop and Wardrobe interactions. Bed reuses the existing Sleep Player Action; laptop applications and wardrobe choices are selectable placeholders without simulated app state or invented persistent effects.

The outdoor world layout now uses a simple 2D grid: Campus Plaza connects to the Lecture Hall, Library, Dormitory, Sports Field and East District; the Library, Campus Café and Student Union form a short connected campus walkway; East District branches through Commercial Lane and Residential Lane to the local destinations. Sixteen outdoor arrival points carry unique `mapPosition` grid coordinates. These coordinates are map presentation data only; normal travel still follows authored exits.

The branch also adds a 15-minute Wait action at the Lecture Hall and an authored daily 09:00–09:15 availability window for `attend_lesson`. Fast Travel is implemented as a separate destination-selection page. It lists only authored outdoor arrival points and building entrances; interior Locations such as the Lecture Hall, private dormitory room and bathroom are excluded. Selecting a destination uses the existing Travel Action for time and Fatigue, then places the player outside the building. Existing NPC schedule and interaction contracts remain in use.

**Fast-travel follow-up:** [Issue #169](https://github.com/TN13007/rpg/issues/169) is implemented on dependent branch `feat/issue-169-building-fast-travel`, targeting the dormitory journey branch until PR #168 is merged. Review the Fast Travel destination list and entrance-to-interior transitions alongside the parent journey. Both PRs remain proposals until merged; do not treat their behavior as part of `main` yet.

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
