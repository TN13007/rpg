# Testing Strategy and Runtime Verification

This document describes how to verify the current repository. Stable Git workflow and the unified validation command are owned by [CONTRIBUTING.md](../../CONTRIBUTING.md).

## 1. Verification layers

| Layer | What it proves | What it does not prove |
|---|---|---|
| Domain/Application unit tests | Rules, calculations, validation, state changes and edge cases | Godot scene wiring or rendered UI |
| Content/validator checks | Authored data shape, IDs, references and repository conventions | Player experience quality |
| .NET build and Godot headless smoke checks | C# compilation, resource import and bounded scene startup | Full interaction, input behavior or visual correctness |
| Manual Godot runtime acceptance | The affected player journey works in the actual project | Independent learner understanding unless separately demonstrated |

Never treat one layer as a substitute for another.

## 2. Automated test organization

The test project is `RPG.Tests/RPG.Tests.csproj`, targeting .NET 8. Tests are grouped by feature and should cover:
- Normal behavior and expected state changes.
- Invalid requests and rejected operations without unintended mutation.
- Minimum/maximum and other boundary values.
- Important combinations of state and effects.
- Regression cases for the changed player journey.

The current suite includes coverage for Character resources, time, actions, content loading, Exploration and the deterministic Combat prototype. Inspect the matching test folder before changing a behavior; do not preserve stale test counts in this document.

Use the complete validation command in [CONTRIBUTING — One-command validation](../../CONTRIBUTING.md#7-one-command-validation). For a documentation-only change, run `python Tools/CheckMarkdown.py`.

## 3. Godot runtime verification

Open the project in Godot 4.7.2 .NET and run `Scenes/Main.tscn`. For changes to scenes, node paths, UI, input or runtime behavior, manually verify the affected journey in addition to automated checks.

For the current prototype, the main smoke journey should cover:
- Main scene opens without script/resource errors.
- Character Panel displays and refreshes Health, Fatigue, Stamina, Mood and Gold.
- Exploration shows the current Location and available choices.
- Player/Location/Travel/Event choices route to the expected behavior.
- Rest and Sleep apply their configured state/time changes.
- The Unit 213 incident presents observation, cautious entry and retreat as non-combat choices; Combat remains separately testable through the debug encounter.
- Combat status, message log and available actions refresh correctly.
- Combat completion returns to the same Location and applies encounter outcomes once.

Only verify the steps affected by the change, but run the complete journey when a change crosses several flows.

## 4. Evidence in Issues and Pull Requests

Record:
- Tested branch and commit.
- Exact automated commands and observed results.
- Godot version and the manual journey performed.
- Screenshots, recording or relevant Output log when useful.
- Checks not performed and why.
- Any remaining mismatch between expected and actual behavior.

A user-confirmed runtime journey is valid product acceptance evidence when the tested build and journey are clear. It is not independent learning evidence by itself.

## 5. Learning verification

Product verification and Learning verification are separate. To record independent learning, ask the learner to predict, explain, trace, test or modify the behavior without simply copying the completed implementation. Record only observed evidence in [WORKFLOW.md](../../WORKFLOW.md).
