# Codebase Guide

**Repository:** TN13007/rpg  
**Engine:** Godot 4.7.2 .NET / C#  
**Main scene:** `Scenes/Main.tscn`  
**Composition root:** `Scripts/Bootstrap/MainController.cs`

## 1. Repository map

| Path | Responsibility | Start here |
|---|---|---|
| `Scripts/Bootstrap/` | Godot entry point and dependency composition | `MainController.cs` |
| `Scripts/Domain/` | Runtime state, definitions, game rules and calculations | Feature folders such as `Characters/`, `Combat/`, `Time/` |
| `Scripts/Application/` | Use-case coordination, request validation and queries | `Actions/`, `Exploration/`, `Combat/` |
| `Scripts/Presentation/` | UI, flow controllers, ViewModels, mappers and formatters | `Controllers/`, feature UI and mappers |
| `Scripts/Infrastructure/` | Content loading and engine/platform adapters | `Content/` |
| `Data/` | Authored JSON content and ID references | Identify the matching loader/schema first |
| `Scenes/` | Godot scene composition and previews | `Main.tscn` |
| `RPG.Tests/` | Automated tests grouped by feature | Open the matching feature folder |
| `Tools/` | Repository validation and supporting scripts | `Validate.py` |

The code is organized layer-first, then by feature. Do not start by reading every C# file.

## 2. Dependency direction

```text
Presentation -> Application -> Domain
Bootstrap composes the application graph.
Infrastructure loads/adapts content and platform concerns.
```

- Domain must not depend on Godot UI, Presentation or Infrastructure.
- Application coordinates use cases and queries; it must not render controls or read files.
- Presentation owns UI and flow coordination, not authoritative gameplay rules.
- Infrastructure loads/adapts external data; it does not decide gameplay outcomes.
- Bootstrap composes dependencies and connects top-level Godot signals.

## 3. Feature discovery

Use [Scripts Architecture Map](../../Scripts/README.md) for the current source-level flow diagrams. Typical entry points:

- Exploration choices: `AvailableInteractionQuery` → mapper → ViewData → `StoryPanel`.
- Selecting a choice: `ExplorationFlowController` → `ActionExecutionService` → Domain state/rules.
- Combat: `CombatFlowController` → `CombatSessionService` → `CombatService` → Combat presentation mapper and panel.
- Time: `GameClock` owns day/minute tracking and midnight rollover.

These are navigation hints. Trace the current implementation and tests before changing behavior.

## 4. Code discovery checklist

1. Locate the relevant type, method, data ID or UI label.
2. Trace callers and references through the complete path to the state change and presentation.
3. Verify which layer owns the behavior.
4. Inspect existing tests and fixtures.
5. Prepare an Implementation Brief for cross-layer changes.
6. Change the smallest coherent slice and verify it.

If docs and code disagree, report the mismatch and verify against implementation and tests. Do not silently assume either source is correct.

## 5. Validation

From the repository root:

```powershell
git fetch origin main
$branch = git branch --show-current
python Tools/Validate.py --base origin/main --head HEAD --branch $branch --check-branch
```

For documentation-only changes:

```powershell
python Tools/CheckMarkdown.py
```

For scene, resource, C# project or runtime changes, also follow [Testing](Testing.md) for Godot build/headless and manual runtime expectations.

## 6. Documentation ownership

- Player experience, game rules, ideas and design decisions → [Design](../Design/README.md).
- Code structure, runtime flows, data contracts, UI contracts and tests → this folder.
- Current status, session progress, blockers and handoff → [WORKFLOW.md](../../WORKFLOW.md).
- Session-specific working brief → current conversation, not a permanent duplicate document.
