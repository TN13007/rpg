# WORKFLOW — RPG Project Hub

**Repository:** [TN13007/rpg](https://github.com/TN13007/rpg)  
**Engine:** Godot 4.7.2 .NET / C#  
**Last reviewed:** 2026-11-18  
**Official implementation baseline:** latest `main` — consult the current branch head rather than a copied commit hash.

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

**Output:** an evidence-based starting point, a clear Session boundary and the source material needed for one Implementation Brief. AI performs this inspection and presents the synthesis; the learner is not asked to read every discovered document.

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

### Implementation Brief — the learner's working document

At the start of a development Session, AI prepares one concise Implementation Brief in the working conversation. The learner reads this brief as the starting point instead of opening and cross-reading the repository's documents.

The brief is a task-specific view assembled from the existing sources of truth, not a new source of truth and not a permanent Markdown file. AI must inspect the relevant sources itself: latest `main`, active Issue, relevant open PRs/branch, canonical design or technical documents, affected code and tests. Do not infer implementation state from an old Session note or an unmerged PR.

The brief must:
1. **Direction:** state the Product objective and observable Learning objective.
2. **Project snapshot:** identify the relevant baseline, Issue and PR/branch, and distinguish merged implementation from proposals.
3. **Current behavior:** summarize only the code behavior and design rules relevant to this task, with repository file/section references.
4. **Decisions and uncertainty:** separate confirmed decisions, assumptions that need validation and unresolved questions. Never present an assumption as an accepted rule.
5. **Scope:** define what this Session will and will not change, including the Issue's acceptance criteria.
6. **Implementation plan:** give a small, ordered sequence of changes and identify where the learner should predict, explain, diagnose or make a decision.
7. **Code impact:** list likely files/components and their responsibilities. Mark this as expected impact until verified against the code.
8. **Verification:** map each important acceptance criterion to an automated test, manual Godot journey or other observable check. State what cannot be verified in the current environment.
9. **Risks and handoff:** list dependencies, likely failure points and the next decision or action.

The brief should be short enough to read before implementation. Include only relevant context; link to source documents for traceability, but do not require the learner to read them all. AI may open additional sources during implementation when necessary and should explain the specific information that matters. The goal is one reading surface for the learner, not an artificial ban on consulting references.

After the Session, update the correct durable owner: Issue for scope/acceptance criteria and unresolved work; PR for implementation decisions and verification evidence; the canonical design/technical document when an accepted rule or contract changes; this file for project status, learning evidence and handoff. Do not save a separate permanent file for each Implementation Brief or copy full design specifications into it.

## 3. Current project snapshot — official main

### Product direction

Adventurer Academy is a fantasy life-simulation RPG prototype about daily life, character wellbeing, academy activities, location-based exploration and occasional encounters. The current baseline is a narrow academy/Goblin vertical slice.

### Implemented capabilities

- Godot 4.7.2 .NET / C# project and main scene.
- Player Health, Fatigue, Stamina, Mood and Gold. Health = 0 means Dead; Fatigue and Stamina do not create an unconscious state.
- DayClock with day/minute tracking, midnight rollover and NextDay transition.
- Location travel among Dormitory, Courtyard, Practical Classroom and Guild Hall.
- Location Actions, travel duration calculation and inline duration estimates.
- Rest and Sleep are Player Actions in `Data/Characters/Player.json`. Rest is available at every actionable Location; Sleep is restricted to Dormitory.
- Exploration choices flow through AvailableInteractionQuery → ActionChoiceMapper → grouped ActionChoiceViewData → StoryPanel. Requests preserve source kind, source ID, action ID and optional target ID.
- Location Events, including the Courtyard Goblin Ambush with Confront and Sneak choices.
- Dedicated Combat flow with Attack, Defend, Dodge and Catch Breath.
- Combat presentation follows CombatService → CombatPresentationMapper → CombatViewData → CombatPanel.

### Current behavior boundaries

- Selecting an Exploration action executes immediately; the duration estimate is informational. There is no Preview / Confirm / Cancel screen.
- Health = 0 means Dead and blocks gameplay actions. There is no Unconscious state in the current resource model.
- Fatigue (0–100) represents cumulative daily-life exertion. It does not cause unconsciousness or block actions in this initial version.
- Stamina (0–10) is reserved for defensive Combat Actions: Defend costs 1, Dodge costs 2, Attack is free, and Catch Breath restores up to 3 while the enemy still responds. Insufficient Stamina does not end Combat.
- MainController is the Godot composition root. ExplorationFlowController and CombatFlowController own their respective presentation flows; gameplay rules remain in Application/Domain services.
- The complete academy routine, daily summary and full Save/Load loop are not implemented yet.

## 4. Session index and progress

The nine-Session Version 1 plan is accepted design scope, merged through [PR #23](https://github.com/TN13007/rpg/pull/23). It defines intended product scope and acceptance criteria; it does not mean its future Sessions are implemented. Learning objectives, independent practices and knowledge checks remain AI-drafted proposals and require explicit approval before the corresponding Session begins.

| Session | Scope | Product status | Learning status |
|---|---|---|---|
| [Session 1](https://github.com/TN13007/rpg/blob/main/Docs/Version1Plan.md#session-1) | Inline Action Duration Estimates | **Implemented** — PR #21 merged; .NET CI passed; Godot runtime acceptance pending | **Not assessed** — no structured independent evidence recorded |
| [Session 2](https://github.com/TN13007/rpg/blob/main/Docs/Version1Plan.md#session-2) | Fatigue Accumulation, Rest and Combat Stamina | **Implemented** — [PR #47 merged](https://github.com/TN13007/rpg/pull/47); [CI #142](https://github.com/TN13007/rpg/actions/runs/44315667902) passed on the final feature-branch head before merge. Godot evidence needs reconciliation because the PR checklist is checked while its verification note still says pending. | **Not assessed** — independent evidence is still required |
| [Session 3](https://github.com/TN13007/rpg/blob/main/Docs/Version1Plan.md#session-3) | Exploration Action Choice Pipeline | **Implemented** — [PR #57 merged](https://github.com/TN13007/rpg/pull/57); 84/84 .NET tests passed. PR #58 CI also passes Godot 4.7.2 headless import, main-scene and Combat-preview smoke checks. Full interactive player-journey verification remains separate. | **Not assessed** — independent pipeline-tracing practice remains outstanding |
| [Session 4](https://github.com/TN13007/rpg/blob/main/Docs/Version1Plan.md#session-4) | Sleep and Next-Day Transition | **Planned** — accepted scope; not implemented | **Proposed** — learning target requires user approval |
| [Session 5](https://github.com/TN13007/rpg/blob/main/Docs/Version1Plan.md#session-5) | Health, Injury and Recovery | **Planned** — accepted scope; not implemented | **Proposed** — learning target requires user approval |
| [Session 6](https://github.com/TN13007/rpg/blob/main/Docs/Version1Plan.md#session-6) | Mood and Daily Wellbeing | **Planned** — accepted scope; not implemented | **Proposed** — learning target requires user approval |
| [Session 7](https://github.com/TN13007/rpg/blob/main/Docs/Version1Plan.md#session-7) | A Small, Complete Academy Routine | **Planned** — accepted scope; not implemented | **Proposed** — learning target requires user approval |
| [Session 8](https://github.com/TN13007/rpg/blob/main/Docs/Version1Plan.md#session-8) | Daily Summary and Day Continuity | **Planned** — accepted scope; not implemented | **Proposed** — learning target requires user approval |
| [Session 9](https://github.com/TN13007/rpg/blob/main/Docs/Version1Plan.md#session-9) | Save and Load the Daily-Life Loop | **Planned** — accepted scope; not implemented | **Proposed** — learning target requires user approval |

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
| SYNC-01 | Day 1 scenario previously expected Preview / Confirm / Cancel, although Session 1 executes choices immediately. | PR #21 defines immediate execution; [Day 1 QA](Docs/Testing.md) was corrected in merged PR #24. | **Resolved — PR #24 merged.** |
| SYNC-02 | Automated-test documentation previously misstated the unconscious-state rule. | `ActionExecutionService`, `AcademyLocations.json` and `RPG.Tests/Actions/ActionExecutionTests.cs` confirm the explicit Rest exception; [Automated Tests](Docs/Testing.md) was corrected in merged PR #24. | **Resolved — PR #24 merged.** |
| SYNC-03 | The accepted Player/Location/Travel/Event choice pipeline differed from the implementation. | [PR #57](https://github.com/TN13007/rpg/pull/57), [Exploration Pipeline](Docs/Gameplay.md) and [Version 1 Session Plan](Docs/Version1Plan.md). | **Resolved — PR #57 merged; .NET tests and PR #58 Godot headless scene smoke checks pass. Full interactive journey remains unverified.** |
| SYNC-04 | Rest and Sleep ownership differed from the Player Action model. | `Data/Characters/Player.json` and PR #57. | **Resolved — both are Player Actions; Sleep retains its Dormitory restriction.** |
| SYNC-05 | Godot runtime evidence for earlier Sessions is incomplete or ambiguous. Session 1 runtime acceptance is not recorded. PR #47's checklist is checked, but its closing verification note still says runtime verification is pending. | [PR #21](https://github.com/TN13007/rpg/pull/21), [PR #22](https://github.com/TN13007/rpg/pull/22) and [PR #47](https://github.com/TN13007/rpg/pull/47). | **Historical evidence is ambiguous; do not block Session 3. Re-run only when the relevant journey is next touched, and record observed results.** |
| SYNC-06 | MainController retained too much Exploration and Combat flow orchestration after Session 3. | [Issue #15](https://github.com/TN13007/rpg/issues/15) and [Architecture](Docs/Architecture.md). | **In progress — flow controllers and layer boundaries are implemented in PR #58; .NET, formatting, naming and Godot headless smoke checks pass. Awaiting PR integration and separate interactive journey review.** |
| SYNC-07 | The accepted Fatigue/Stamina resource model previously differed from `main` while PR #47 was open. | [Issue #46](https://github.com/TN13007/rpg/issues/46), [PR #47](https://github.com/TN13007/rpg/pull/47), [Version 1 Plan](Docs/Version1Plan.md) and [Testing](Docs/Testing.md). | **Resolved — PR #47 merged and Issue #46 closed. Runtime evidence reconciliation is tracked in SYNC-05.** |

### Other active work

- [Issue #11 — Decide the long-term gameplay loop](https://github.com/TN13007/rpg/issues/11) remains a separate long-term design question and does not block Version 1 implementation.
- [PR #57 — Exploration Action Choice Pipeline](https://github.com/TN13007/rpg/pull/57) is merged; Session 3 Product Outcome is implemented.
- [PR #58 — Explicit layered architecture](https://github.com/TN13007/rpg/pull/58) is in progress as part of Issue #15.
- [Issue #15 — Establish explicit layers and extract MainController flows](https://github.com/TN13007/rpg/issues/15) tracks the architecture and controller-boundary refactor.
- [PR #47 — Fatigue and Combat Stamina](https://github.com/TN13007/rpg/pull/47) is merged; the resource model is part of official `main`.

## 6. Next Session handoff

1. Complete Issue #15's layered-structure and controller-boundary refactor before starting the next Version 1 feature.
2. Keep Session 3 Learning Outcome marked **Not assessed** until the user independently traces the Player Action pipeline and completes the agreed small practice. Merged code and passing CI are not learning evidence.
3. PR #58 CI now imports the project and launches the main scene and Combat preview in Godot 4.7.2 .NET headless mode. This confirms startup/resource loading, not full interactive UI acceptance. Earlier Session 1–2 runtime evidence remains historically incomplete/ambiguous; record any later manual journey separately.
4. After Issue #15 is merged and its verification is complete, resume Session 4 — Sleep and Next-Day Transition using the existing Implementation Brief process.
5. Record Product verification, Learning evidence and remaining blockers in this file only.
