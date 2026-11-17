# WORKFLOW — RPG Project Hub

**Repository:** [TN13007/rpg](https://github.com/TN13007/rpg)  
**Engine:** Godot 4.7.2 .NET / C#  
**Last reviewed:** 2026-11-17  
**Official implementation baseline:** `main` — `5e0bad3ae2ccda8fd55d0445ffbc3cbb49d5e207`

> Start here at the beginning and end of every Session. This is the single project-status and handoff tracker. Do not create separate project-state, decision, learning or per-session tracking files.

## 1. Working agreement

Every development Session has two separate outcomes:

- **Product:** a usable, testable improvement to Adventurer Academy.
- **Learning:** a demonstrated increase in the ability to understand, explain, verify or independently change the relevant part of the project.

A merged feature, AI-generated code, copied explanation or passing CI does not prove independent mastery. Product can be complete while Learning remains unassessed.

### Sources of truth

1. GitHub is the source of truth for repository content.
2. Latest `main` is the official implemented state.
3. Open or unmerged PRs are proposals, not shipped functionality.
4. This file owns changing project status: Session progress, learner evidence, blockers, handoff and documentation synchronization.
5. The Version 1 Session Plan owns proposed Session scope and acceptance criteria. It does not own progress status.
6. Technical/design documents own their detailed rules and contracts. Link to them rather than duplicating specifications here.

### Keep tracking in one place

For normal Session bookkeeping, update only this file: Product/Learning status, verification evidence, learner observations, blockers, next steps and documentation mismatches. Update a design or technical document only when its actual requirements or contract changes. The one-file rule applies to tracking, not to avoiding necessary implementation, test or specification files.

## 2. Learning-first workflow

A Session follows these eight steps. The learner should remain involved in prediction, diagnosis and verification; AI assistance may accelerate execution but must not be treated as evidence of learning.

### 1. Context Check

Inspect the latest `main`, active branch, related Issues and open PRs. Read the relevant code, content, tests and design documents. Separate implemented behavior from proposals, assumptions and unverified behavior.

**Output:** an evidence-based starting point and a clear Session boundary.

### 2. Learning Objective

Define one product outcome and one observable skill or concept to practice. A learning objective must be demonstrable, such as tracing a data flow, explaining a responsibility boundary, writing a test or diagnosing a failure.

**Output:** separate Product and Learning objectives.

### 3. Predict & Plan

Before changing anything, predict the expected behavior, affected components, inputs/outputs, likely edge cases and verification approach. Then choose the smallest useful implementation slice.

**Output:** learner prediction, implementation plan and test plan.

### 4. Understand

Explain only the concepts required for this task. Use the actual project code and data; trace a complete relevant path and explain why each responsibility belongs to its owner. Distinguish the current implementation from the proposed design.

**Output:** a working mental model of the relevant behavior and boundaries.

### 5. Execute

Implement a small, coherent increment. AI may handle repetitive or complex edits, but must explain the intent and affected files. The learner should own a suitably sized action—such as a test, small code change, trace, diagnosis or design decision—when the Session has a learning objective.

**Output:** reviewable changes and focused commits.

### 6. Verify

Verify Product and Learning independently.

**Product verification:** run relevant automated tests/builds; run Godot 4.7.2 .NET when scenes, UI, input or gameplay runtime are affected; check acceptance criteria; record what was and was not run.

**Learning verification:** ask the learner to explain, predict, diagnose or complete a related task without step-by-step guidance. Record actual evidence, not an AI-generated answer or copied explanation.

**Output:** separate Product and Learning results, each supported by evidence.

### 7. Improve

When something fails or behaves unexpectedly, reproduce it and identify the root cause before changing code. Check assumptions, responsibility boundaries, tests and the development approach; fix the cause rather than hiding the symptom, then repeat verification. Do not silently skip the diagnosis step after an error.

**Output:** the identified cause, corrective change and retest result—or a clear statement that no failure required correction.

### 8. Retain

Summarize what changed, what the learner actually demonstrated, remaining gaps, decisions and the next useful step. Add evidence to this file so the next Session can resume without reconstructing context.

**Output:** a concise, evidence-based handoff.

### Light Mode

Use Light Mode for a small, familiar task or when the learner explicitly asks for direct execution. Keep the same sequence, but reduce the depth: a brief context check, one-line objective/prediction, only the explanation needed, focused execution, proportionate verification and a short record. Run Improve whenever something fails. Do not skip required tests, Godot runtime checks or any specifically requested independent learning check. For a purely administrative task that has no learning outcome, do not invent a learning objective or claim learning progress.

## 3. Current project snapshot — official main

### Product direction

Adventurer Academy is a fantasy life-simulation RPG prototype about daily life, character wellbeing, academy activities, location-based exploration and occasional encounters. The current baseline is a narrow academy/Goblin vertical slice.

### Implemented capabilities

- Godot 4.7.2 .NET / C# project and main scene.
- Player Health, Energy, Mood, Gold and alive/unconscious state.
- DayClock with day/minute tracking, midnight rollover and NextDay transition.
- Location travel among Dormitory, Courtyard, Practical Classroom and Guild Hall.
- Location Actions, travel duration calculation and inline duration estimates.
- Location Events, including the Courtyard Goblin Ambush with Confront and Sneak choices.
- Dedicated Combat flow with Attack, Defend and Dodge.
- Rest is currently a global Self Action in root `selfActions` in `Data/academy_locations.json`. It takes 30 minutes, costs 0 Energy, restores up to 3 Energy and adds 2 Mood. It explicitly permits execution while unconscious.
- Sleep is currently a Dormitory Location Action. It uses `NextDay` and has configured Health, Energy and Mood recovery Effects.

### Current behavior boundaries

- Selecting an Exploration action executes immediately; the duration estimate is informational. There is no Preview / Confirm / Cancel screen.
- Health = 0 means Dead and blocks all actions. Health > 0 with Energy = 0 means Unconscious. Only actions explicitly configured with `canExecuteWhenUnconscious` may run while unconscious; the flag does not waive Energy cost.
- Rest currently works while unconscious; Sleep is not configured to do so.
- Current Exploration choices are assembled by MainController into a flat list. Rest comes from global `selfActions`; Sleep remains a Location Action.
- The grouped Query → Mapper → Exploration ViewModel pipeline and Player-owned Player Actions are accepted design from merged PR #23, but are not yet implemented on `main`.
- The complete academy routine, daily summary and full Save/Load loop are not implemented yet.

## 4. Session index and progress

The nine-Session Version 1 plan is accepted design scope, merged through [PR #23](https://github.com/TN13007/rpg/pull/23). It defines intended product scope and acceptance criteria; it does not mean its future Sessions are implemented. Learning objectives, independent practices and knowledge checks remain AI-drafted proposals and require explicit approval before the corresponding Session begins.

| Session | Scope | Product status | Learning status |
|---|---|---|---|
| [Session 1](https://github.com/TN13007/rpg/blob/main/Docs/Design/VERSION_1_SESSION_PLAN.md#session-1) | Inline Action Duration Estimates | **Implemented** — PR #21 merged; .NET CI passed; Godot runtime acceptance pending | **Not assessed** — no structured independent evidence recorded |
| [Session 2](https://github.com/TN13007/rpg/blob/main/Docs/Design/VERSION_1_SESSION_PLAN.md#session-2) | Energy Economy and Rest | **Implemented** — PR #22 merged; .NET CI passed; Godot runtime acceptance pending | **Not assessed** — no structured independent evidence recorded |
| [Session 3](https://github.com/TN13007/rpg/blob/main/Docs/Design/VERSION_1_SESSION_PLAN.md#session-3) | Exploration Action Choice Pipeline | **Planned** — accepted scope; no implementation on main | **Proposed** — learning target requires user approval |
| [Session 4](https://github.com/TN13007/rpg/blob/main/Docs/Design/VERSION_1_SESSION_PLAN.md#session-4) | Sleep and Next-Day Transition | **Planned** — accepted scope; not implemented | **Proposed** — learning target requires user approval |
| [Session 5](https://github.com/TN13007/rpg/blob/main/Docs/Design/VERSION_1_SESSION_PLAN.md#session-5) | Health, Injury and Recovery | **Planned** — accepted scope; not implemented | **Proposed** — learning target requires user approval |
| [Session 6](https://github.com/TN13007/rpg/blob/main/Docs/Design/VERSION_1_SESSION_PLAN.md#session-6) | Mood and Daily Wellbeing | **Planned** — accepted scope; not implemented | **Proposed** — learning target requires user approval |
| [Session 7](https://github.com/TN13007/rpg/blob/main/Docs/Design/VERSION_1_SESSION_PLAN.md#session-7) | A Small, Complete Academy Routine | **Planned** — accepted scope; not implemented | **Proposed** — learning target requires user approval |
| [Session 8](https://github.com/TN13007/rpg/blob/main/Docs/Design/VERSION_1_SESSION_PLAN.md#session-8) | Daily Summary and Day Continuity | **Planned** — accepted scope; not implemented | **Proposed** — learning target requires user approval |
| [Session 9](https://github.com/TN13007/rpg/blob/main/Docs/Design/VERSION_1_SESSION_PLAN.md#session-9) | Save and Load the Daily-Life Loop | **Planned** — accepted scope; not implemented | **Proposed** — learning target requires user approval |

### Learning evidence log

Add an entry only after an actual learner check. Keep it short and link evidence instead of copying large artifacts.

No structured independent learning evidence has been recorded for Sessions 1–2 in this project log. This means “not assessed here”, not that the learner has learned nothing outside the repository.

**Entry format**

```text
YYYY-MM-DD — Session N
Product: Done / Partial / Blocked — evidence link
Learning: Not assessed / Needs practice / With support / Independent
Learner evidence: what the learner actually explained, changed or verified
Remaining gap: specific concept or skill
Next practice: one small task
```

## 5. Documentation synchronization register

Track known disagreements between the documents and official implementation here. Do not duplicate the same discrepancy in another tracker.

| ID | Mismatch / risk | Canonical truth or owner | Status / next action |
|---|---|---|---|
| SYNC-01 | Day 1 scenario previously expected Preview / Confirm / Cancel, although Session 1 executes choices immediately. | PR #21 defines immediate execution; [Day 1 QA](Docs/QA/DAY_1_PLAYABLE_SCENARIO.md) was corrected in merged PR #24. | **Resolved — PR #24 merged.** |
| SYNC-02 | Automated-test documentation previously misstated the unconscious-state rule. | `ActionExecutionService`, `academy_locations.json` and `RPG.Tests/ActionExecutionTests.cs` confirm the explicit Rest exception; [Automated Tests](Docs/Testing/AUTOMATED_TESTS.md) was corrected in merged PR #24. | **Resolved — PR #24 merged.** |
| SYNC-03 | The accepted Version 1 plan targets a grouped Player/Location/Travel/Event choice pipeline; current MainController still assembles a flat list. | [Exploration Pipeline](Docs/Design/EXPLORATION_ACTION_PIPELINE.md) and [Version 1 Session Plan](Docs/Design/VERSION_1_SESSION_PLAN.md) were accepted/merged in PR #23. | **Design accepted; implementation pending Session 3.** |
| SYNC-04 | Rest and Sleep ownership differ from the proposed Player Action model. | Current main: Rest is root `selfActions`; Sleep is a Dormitory Location Action. Target proposal: Player-owned actions in Session 3. | **Design-to-implementation work is not started.** |
| SYNC-05 | Sessions 1–2 passed their .NET CI checks, but Godot runtime acceptance remains unrecorded. | [PR #21](https://github.com/TN13007/rpg/pull/21) and [PR #22](https://github.com/TN13007/rpg/pull/22). | **Run the affected journeys in Godot or explicitly defer with a reason.** |
| SYNC-06 | Issue #15 (extract Location/Event and Combat flow from MainController) may overlap with Session 3's Exploration query/choice pipeline. | [Issue #15](https://github.com/TN13007/rpg/issues/15) and [Exploration Pipeline](Docs/Design/EXPLORATION_ACTION_PIPELINE.md). | **Reconcile ownership and boundaries before Session 3 implementation; keep Issue #15 open.** |

### Other active work

- [Issue #11 — Decide the long-term gameplay loop](https://github.com/TN13007/rpg/issues/11) remains a separate design question. Do not add speculative progression systems to Version 1 to resolve it.
- [PR #23 — Version 1 Exploration architecture and Session plan](https://github.com/TN13007/rpg/pull/23) is merged; the accepted design is not yet fully implemented.
- [PR #24 — QA documentation alignment](https://github.com/TN13007/rpg/pull/24) is merged.
- [PR #31 — naming-validator hardening](https://github.com/TN13007/rpg/pull/31) is open and requires review of ambiguous/duplicate owner identities before merge.
- [PR #33 — unified validation and repository workflow](https://github.com/TN13007/rpg/pull/33) is open; it closes Issue #32 when merged.

## 6. Next Session handoff

1. Complete or explicitly defer the Godot runtime checks for Sessions 1–2; .NET CI is not runtime evidence.
2. Perform independent Learning checks for Sessions 1–2 before claiming mastery.
3. Implement Session 3 from the accepted Version 1 plan; first reconcile Issue #15 with the agreed Exploration/MainController boundaries.
4. Approve or revise the Session 3 Learning objective, independent practice and knowledge check before using them.
5. Record Product verification, Learning evidence and remaining blockers in this file only.
