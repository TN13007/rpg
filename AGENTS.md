# Repository Instructions for Coding Agents

This repository is a Godot 4.7.2 .NET project written in C#. The main scene is `Scenes/Main.tscn`; the composition root is `Scripts/Bootstrap/MainController.cs`.

These instructions are a navigation entry point, not a duplicate specification. Follow the canonical documents and inspect the current code before making changes.

## Start with the task context

1. Read the active GitHub Issue and related PRs. Latest `main` is the implemented baseline; open PRs are proposals.
2. Read [WORKFLOW.md](WORKFLOW.md) for current status, blockers, Session handoff and learning evidence.
3. Read the relevant document under [Docs/Design](Docs/Design/README.md) to understand intended player experience and rules.
4. Read the relevant document under [Docs/Implementation](Docs/Implementation/README.md) to understand current architecture, runtime behavior, UI contracts and verification.
5. Read [CONTRIBUTING.md](CONTRIBUTING.md) when creating Issues, branches, commits or PRs, or when naming and repository governance rules matter.

Do not read every document or every source file by default. Follow the task's actual dependencies.

## Codebase navigation

| Area | Responsibility | Starting point |
|---|---|---|
| `Scripts/Bootstrap/` | Godot entry point and dependency composition | `MainController.cs` |
| `Scripts/Domain/` | Runtime state, definitions, rules and calculations | Feature folders such as `Characters/`, `Combat/`, `Time/` |
| `Scripts/Application/` | Use-case coordination, request validation and queries | `Actions/`, `Exploration/`, `Combat/` |
| `Scripts/Presentation/` | UI, flow controllers, ViewModels and mappers | `Controllers/`, feature UI and mappers |
| `Scripts/Infrastructure/` | Content loading and engine/platform adapters | `Content/` |
| `Data/` | Authored JSON content and ID references | Identify the matching loader/schema first |
| `Scenes/` | Godot scene composition and previews | `Main.tscn` |
| `RPG.Tests/` | Automated tests grouped by feature | Matching feature folder |
| `Tools/` | Repository validation and supporting scripts | `Validate.py` |

[Cross-Layer Architecture](Docs/Implementation/CrossLayerArchitecture.md) owns layer boundaries, dependencies and runtime flows. [Scripts/README.md](Scripts/README.md) is a short source-code locator only.

## Required code-discovery sequence

Before editing unfamiliar or cross-layer behavior:

1. **Locate:** Search for the relevant type, method, ID, signal or UI label.
2. **Trace:** Inspect its definition, callers/references and the complete path from entry point to state change and presentation.
3. **Verify ownership:** Confirm which layer owns the behavior and whether an existing service already performs it.
4. **Inspect tests:** Find the feature's tests and fixtures; identify normal, invalid, boundary and regression cases.
5. **Plan:** For work spanning multiple responsibilities, prepare an Implementation Brief covering current behavior, evidence, affected components, proposed change, constraints and verification.

If documentation and code disagree, report the mismatch and verify against implementation and tests. Do not silently assume either source is correct.

## Architectural constraints

- Follow the dependency direction: Presentation → Application → Domain; Bootstrap composes all layers; Infrastructure adapts content/platform concerns.
- Domain must not depend on Godot UI, Presentation or Infrastructure. Application must not render controls or read files. Presentation must not own authoritative gameplay rules. Infrastructure must not decide gameplay outcomes.
- Keep `MainController` as the composition root.
- Do not add interfaces, repositories, factories, dependency-injection containers or universal executors pre-emptively. Require a demonstrated replacement, isolation or testing need.
- Preserve authored IDs, serialized contracts, scene paths and public type names unless a focused migration explicitly requires changing them.
- When moving a Godot C# script, move its `.cs.uid` sidecar and update scene/resource references.
- Do not split files, rename directories or clean up unrelated code solely for aesthetics.

## Validation and reporting

Follow the validation and manual runtime requirements in [CONTRIBUTING.md](CONTRIBUTING.md) and [Testing Strategy](Docs/Implementation/TestingStrategy.md).

For documentation-only changes, run `python Tools/CheckMarkdown.py`. For code changes, run the repository validation command. For changes affecting scenes, node paths, UI, input or gameplay runtime, also verify the affected journey in Godot 4.7.2 .NET.

In the PR, state exactly which checks ran and their observed results. Separate automated checks from manual runtime verification. Never claim a check passed unless its result was observed.
