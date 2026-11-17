# Project Naming Audit

## 1. Audit snapshot

This is the initial repository-wide path audit against the file and directory rules in Section 9 of [CONTRIBUTING.md](../../CONTRIBUTING.md).

| Item | Result |
|---|---:|
| Baseline | `main` at `d62567eeb4dc717a66b8bc9cead6e526d392d2b3` |
| Tracked files inspected | 122 |
| Tracked directories inspected | 25 |
| Non-conforming directories | 0 |
| Non-conforming file paths | 22 |

The baseline scan includes unchanged legacy paths. It excludes the `.github` and `.godot` platform/tool-managed trees and preserves the documented repository entry-file exceptions. Dot-separated PascalCase .NET project names such as `RPG.Tests` and `RPG.Tests.csproj` are treated as valid project names.

This report audits **path names only**. JSON property names, serialized IDs, enum-like values and C# IDE1006 are handled by their existing, separate validation checks. No existing files, directories, Godot resources or content IDs were renamed or modified as part of this audit.

## 2. Findings and proposed names

The suggestions below apply the existing PascalCase rule and the acronym guidance in Section 9 (for example, `UI` becomes `Ui`). Godot source assets and their import sidecars must be renamed together.

### Assets

| Current path | Proposed path |
|---|---|
| `Assets/academy_scene.svg` | `Assets/AcademyScene.svg` |
| `Assets/academy_scene.svg.import` | `Assets/AcademyScene.svg.import` |
| `Assets/character_portrait.svg` | `Assets/CharacterPortrait.svg` |
| `Assets/character_portrait.svg.import` | `Assets/CharacterPortrait.svg.import` |

### Game data

| Current path | Proposed path |
|---|---|
| `Data/academy_locations.json` | `Data/AcademyLocations.json` |
| `Data/Characters/goblin.json` | `Data/Characters/Goblin.json` |
| `Data/Characters/player.json` | `Data/Characters/Player.json` |

### Design documentation

| Current path | Proposed path |
|---|---|
| `Docs/Design/COMBAT_PANEL_REVIEW.md` | `Docs/Design/CombatPanelReview.md` |
| `Docs/Design/EXPLORATION_ACTION_PIPELINE.md` | `Docs/Design/ExplorationActionPipeline.md` |
| `Docs/Design/GAME_DEVELOPMENT_ROADMAP.md` | `Docs/Design/GameDevelopmentRoadmap.md` |
| `Docs/Design/GAME_LOOP_AND_LOCATION_EVENTS.md` | `Docs/Design/GameLoopAndLocationEvents.md` |
| `Docs/Design/GAME_OVERVIEW.md` | `Docs/Design/GameOverview.md` |
| `Docs/Design/LONG_TERM_WORLD_AND_CHALLENGES.md` | `Docs/Design/LongTermWorldAndChallenges.md` |
| `Docs/Design/OPEN_DESIGN_QUESTIONS.md` | `Docs/Design/OpenDesignQuestions.md` |
| `Docs/Design/SPRINT_PLAYABLE_JOURNEY.md` | `Docs/Design/SprintPlayableJourney.md` |
| `Docs/Design/UI_ARCHITECTURE.md` | `Docs/Design/UiArchitecture.md` |
| `Docs/Design/VERSION_1_SESSION_PLAN.md` | `Docs/Design/Version1SessionPlan.md` |
| `Docs/Design/WORLD_MODEL_FOUNDATION.md` | `Docs/Design/WorldModelFoundation.md` |
| `Docs/Design/WORLD_MODEL_REVIEW.md` | `Docs/Design/WorldModelReview.md` |

### QA and testing documentation

| Current path | Proposed path |
|---|---|
| `Docs/QA/DAY_1_PLAYABLE_SCENARIO.md` | `Docs/QA/Day1PlayableScenario.md` |
| `Docs/QA/PLAYABLE_JOURNEY_TEST_REPORT.md` | `Docs/QA/PlayableJourneyTestReport.md` |
| `Docs/Testing/AUTOMATED_TESTS.md` | `Docs/Testing/AutomatedTests.md` |

## 3. Reference and compatibility risks

The path changes are not isolated filesystem operations. The following references were confirmed during the initial review:

- `Scripts/MainController.cs` refers to `Data/academy_locations.json`, `Data/Characters/goblin.json` and `Data/Characters/player.json`. Renaming those files requires updating the loader paths in the same change.
- The documentation inventory contains cross-links among the affected Markdown files. References occur in `README.md`, `WORKFLOW.md`, `CONTRIBUTING.md` and other design, QA and testing documents. Every relative link must be updated and checked after each documentation rename batch.
- Each SVG `.import` sidecar must move with its source SVG. Before the asset rename, search the complete project for `res://Assets/` references and verify the affected resources in Godot.
- The JSON file rename must not alter serialized content IDs, properties, or authored game data. Those changes are outside this audit.

The initial reference review is a starting point, not a claim that every runtime reference has already been mapped. Each rename PR must search the full repository for the old path and verify that no references remain.

## 4. Recommended remediation sequence

1. **Documentation paths (15 files).** Rename in small groups by folder. Update all Markdown links in the same PR and run a repository-wide stale-link check.
2. **Game data paths (3 files).** Rename the three JSON files and update the loader paths in `MainController.cs`. Run the complete .NET test suite and play the affected data-loading journey in Godot.
3. **Asset paths (4 files).** Rename each SVG with its matching `.import` sidecar. Search all scene, resource and script references first, then reopen the affected scenes in Godot and verify asset loading.
4. **Enforce the rule.** Once the audit reports zero path violations, remove the informational-only behavior from the CI audit step and make full repository path auditing a required check.

Each remediation PR should link to Issue #34, preserve content IDs and gameplay behavior, include a before/after reference search, and record the relevant automated and Godot runtime checks.

## 5. How to run the audit

From the repository root:

```powershell
python Tools/CheckProjectNaming.py --audit-all
```

The command checks every Git-tracked path, including unchanged legacy paths. It prints each violation and a suggested path when the conversion is unambiguous. It exits with code 1 while violations remain.

CI currently runs the same scan in informational mode so the findings stay visible without blocking unrelated work:

```bash
python3 Tools/CheckProjectNaming.py --audit-all --report-only
```
