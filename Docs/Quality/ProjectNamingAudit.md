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

## 4. Remediation status

The initial findings have now been remediated on `main`:

1. **Documentation paths — complete.** PR [#37](https://github.com/TN13007/rpg/pull/37) renamed the 15 Markdown documents and updated their references.
2. **Game data paths — complete.** PR [#39](https://github.com/TN13007/rpg/pull/39) renamed the three JSON files, updated runtime/test/project references, and preserved JSON content.
3. **Godot asset paths — complete.** PR [#41](https://github.com/TN13007/rpg/pull/41) renamed both SVG files and their import sidecars, updated scene references, and preserved import settings and UIDs.
4. **CI enforcement — implemented in [Issue #42](https://github.com/TN13007/rpg/issues/42).** The audit is now intended to run as a blocking workflow step rather than report-only.

The cleanup was verified in [GitHub Actions Run #115](https://github.com/TN13007/rpg/actions/runs/44198070161): 123 tracked files and 26 directories were checked, with zero path naming violations. That run occurred before blocking enforcement was enabled; the PR for Issue #42 must provide the separate CI evidence for the required gate.

The original baseline findings below are retained as historical audit evidence.

## 5. How to run the audit

From the repository root:

```powershell
python Tools/CheckProjectNaming.py --audit-all
```

The command checks every Git-tracked path, including unchanged legacy paths. It prints each violation and a suggested path when the conversion is unambiguous. It exits with code 1 while violations remain.

GitHub Actions now runs the full audit as a required, blocking step:

```bash
python3 Tools/CheckProjectNaming.py --audit-all
```

The command exits with code 1 when any tracked path violates the naming convention. A detected violation therefore fails the workflow. The `--report-only` option remains available for deliberate diagnostic use, but CI does not use it.


## 6. Post-cleanup enforcement

The post-cleanup snapshot on `main` was recorded by [GitHub Actions Run #115](https://github.com/TN13007/rpg/actions/runs/44198070161):

| Item | Result |
|---|---:|
| Tracked files inspected | 123 |
| Tracked directories inspected | 26 |
| Non-conforming directories | 0 |
| Non-conforming file paths | 0 |

The workflow step is named **Enforce full repository naming audit** and runs without `--report-only`. This makes the full path audit a blocking CI check. Future tracked files and directories must satisfy the same naming rules, subject to the documented entry-file and platform-managed-tree exceptions.

The enforcement change is tracked by [Issue #42](https://github.com/TN13007/rpg/issues/42). Its Pull Request must pass the complete repository validation and the blocking audit before the issue can be closed.
