# Repository Instructions for Coding Agents

This repository is a Godot 4.7.2 .NET project written in C#. The main scene is `Scenes/Main.tscn`; the C# application is composed by `Scripts/Bootstrap/MainController.cs`.

These instructions are a navigation entry point, not a duplicate specification. Follow links to the existing source of truth and inspect the current code before making changes.

## Start with the relevant context

1. Read the active GitHub Issue and any related open PR. Treat the latest `main` as the implemented baseline; an open PR is only a proposal.
2. At the beginning/end of a development Session, consult [WORKFLOW.md](WORKFLOW.md) for project status, handoff and the Implementation Brief process. Do not create another status tracker.
3. For code changes, use [Docs/Implementation/Architecture.md](Docs/Implementation/Architecture.md) for canonical layer boundaries and runtime flows, then [Scripts/README.md](Scripts/README.md) as the quick source-code map.
4. Read only the relevant document under `Docs/Design/` for intended player experience and rules; use `Docs/Implementation/` for code and verification contracts. Use [Docs/Implementation/Testing.md](Docs/Implementation/Testing.md) for test and runtime expectations.
5. Consult [CONTRIBUTING.md](CONTRIBUTING.md) when creating branches/commits/PRs or when naming and repository governance rules matter.

Do not read every document or every source file by default. Follow the task's actual dependencies.

## Code and content map

| Area | Responsibility | Useful starting point |
|---|---|---|
| `Scripts/Bootstrap/` | Godot entry point and composition root | `MainController.cs` |
| `Scripts/Domain/` | Runtime state, definitions, game rules and calculations | Start from the feature folder, e.g. `Characters/`, `Combat/`, `Time/` |
| `Scripts/Application/` | Use-case coordination, request validation and queries | Start from the relevant feature folder, e.g. `Actions/`, `Exploration/` |
| `Scripts/Presentation/` | UI, flow controllers, ViewModels and mappers | Start from `Controllers/`, then follow the feature's UI and mapper |
| `Scripts/Infrastructure/` | Content loading and engine/platform adapters | Start from `Content/` for JSON-backed content |
| `Data/` | Authored JSON content and ID references | Identify the relevant schema/loader before editing |
| `Scenes/` | Godot scene composition and previews | `Main.tscn` or the affected preview scene |
| `RPG.Tests/` | Automated tests grouped by feature | Open the matching feature folder before changing behavior |
| `Docs/` | Product, gameplay, UI and technical contracts | Use the document index in `README.md` |

For the full layer contract, dependency direction and current runtime flows, [Docs/Implementation/Architecture.md](Docs/Implementation/Architecture.md) is authoritative. The table above is only a locator.

## Required code-discovery sequence

Before editing an unfamiliar or cross-layer behavior:

1. **Locate:** Search for the relevant type, method, ID, signal or UI label. Prefer symbol search and exact text/reference search over opening every file.
2. **Trace:** Inspect the definition, callers/references and the complete relevant path from entry point to state change and presentation. Follow content loading when authored JSON is involved.
3. **Verify ownership:** Check which layer owns the behavior and whether another existing service already performs it.
4. **Inspect tests:** Find the feature's existing tests and fixtures. Identify normal, invalid and boundary cases affected by the change.
5. **Plan:** Summarize the current behavior, evidence, affected components/files, proposed change, constraints and verification in an Implementation Brief when the change spans multiple responsibilities.

Read implementation details only as far as needed to establish the real dependency path. If documentation and code disagree, report the mismatch and verify against implementation and tests; do not silently assume either is correct.

## Architectural constraints

- Keep dependency direction as documented: Presentation → Application → Domain; Bootstrap composes all layers; Infrastructure adapts external content/platform concerns and may depend on Domain contracts where needed.
- Domain must not depend on Godot UI, Presentation or Infrastructure. Application must not render controls or read files. Presentation must not own authoritative gameplay rules. Infrastructure must not decide gameplay outcomes.
- Keep `MainController` as the composition root; do not move feature rules into it.
- Do not add interfaces, repositories, factories, dependency-injection containers or universal executors pre-emptively. Introduce abstractions only for a demonstrated replacement, isolation or testing need.
- Preserve authored IDs, serialized contracts, scene paths and existing public type names unless the task explicitly requires a migration with tests.
- When moving a Godot C# script, move its `.cs.uid` sidecar with it and update scene/resource references.
- Keep changes focused. Do not split files, rename directories or clean up unrelated code solely for aesthetic reasons.

## Validation and reporting

Use the repository's documented validation entry point from the root:

```powershell
git fetch origin main
$branch = git branch --show-current
python Tools/Validate.py --base origin/main --head HEAD --branch $branch --check-branch
```

This runs Markdown validation, Python validator tests, changed-path naming checks and the full .NET test suite; changed C# files also receive the configured style check. See [CONTRIBUTING.md](CONTRIBUTING.md) for prerequisites and details.

For documentation-only changes, at minimum run:

```powershell
python Tools/CheckMarkdown.py
```

For changes affecting scenes, resources, C# project files or runtime behavior, also follow the Godot 4.7.2 .NET build/headless smoke and manual runtime expectations in [Docs/Implementation/Testing.md](Docs/Implementation/Testing.md). Automated smoke checks do not prove rendered UI, input wiring or a complete player journey.

In the PR or handoff, state exactly which commands/checks ran and their observed results. Separate automated results from manual Godot verification. Never claim a test, build, CI run or runtime check passed unless its result was observed; explicitly list skipped checks and why.

## Scope discipline

Do not add nested instruction files or tool-specific ignore files unless a module has unique, verified instructions that cannot be expressed here or in its canonical documentation. Do not add indexing infrastructure, vector search or generated maps without evidence that the existing repository search and navigation are insufficient.
