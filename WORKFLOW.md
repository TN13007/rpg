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

The accepted product direction is a player-directed modern student-life RPG:
- The protagonist is an ordinary university student in a contemporary city.
- Classes, studying, friends, part-time work, money and personal routines remain the everyday foundation.
- Paranormal incidents exist as optional opportunities discovered through rumors, conversations and local information.
- The player may investigate, ignore, leave or seek professional help; combat is not mandatory for every anomaly.
- Phones, laptops and the Internet are ordinary tools, not magical artifacts.
- Tactical Timeline combat remains a specialized rules domain for situations that justify confrontation.

The former fantasy Academy setting is superseded. New player-facing content must use the modern setting. The design source of truth is [Game Vision](Docs/Design/GameVision.md), [World and Narrative](Docs/Design/WorldAndNarrative.md), [Combat](Docs/Design/Combat.md) and [Product Roadmap](Docs/Design/Roadmap.md).

### Implementation reality

The refactor branch replaces the Academy content with a modern city graph in `Data/Locations.json`, renames the location catalog to `LocationCatalog`, and updates the startup path. The authored setting includes university, student housing, campus facilities, a part-time café, East District locations, a paranormal response office and the Unit 213 incident.

The four-person combat party is now Alex, Noah, Mira and Sera, with an Unstable Presence as the authored enemy. Daily schedules and interactions are authored for Mira, Noah, Sera and Ms. Carter. The Unit 213 event supports observation, cautious entry and retreat without forcing combat.

This branch has not yet been merged; latest `main` remains the previous Academy build until the refactor PR is accepted. The refactor has not yet received Godot runtime verification. Do not describe the new player journey as runtime-tested until it has been opened and played in Godot.

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

**Immediate implementation priority:** [Issue #110 — Modern student life and paranormal incident](https://github.com/TN13007/rpg/issues/110).

Review the refactor PR for these outcomes:
- No active game content or runtime startup path depends on the fantasy Academy.
- The modern campus/city graph loads with valid exits and content references.
- Daily-life actions, NPC schedules and NPC interactions use the existing supported contracts.
- The Unit 213 incident is optional and provides non-combat responses.
- The four-person Tactical Timeline remains a separate system and existing combat tests remain valid.
- Active documentation describes the modern setting and current runtime accurately.

After the PR is merged, manually run `Scenes/Main.tscn` in Godot 4.7.2 .NET and verify the campus start, travel, NPC presence, daily actions, Unit 213 event, retreat and debug Combat encounter.

[Issue #91 — Four-Person Tactical Timeline Encounter](https://github.com/TN13007/rpg/issues/91) remains a separate Combat backlog item. Keep its Enemy Intent and second enemy behavior work independent from the setting migration. [Issue #79](https://github.com/TN13007/rpg/issues/79) remains a later progression backlog item.

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
