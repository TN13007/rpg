# Contributing to RPG

Use [WORKFLOW.md](WORKFLOW.md) as the single project/session status and Learning-first hub. Treat latest `main` as the official implementation baseline; open or unmerged PRs are proposals. Follow the Session protocol there, and keep Product verification separate from Learning verification.

This repository uses an Issue → Branch → Commit + Tests → Pull Request → Review → Merge workflow.

## 1. Start with an Issue

Before changing code or design:
1. Search existing Issues and design documents.
2. Read the relevant README and design/spec files.
3. Create or select an Issue with a clear goal, scope, and acceptance criteria.
4. Keep one Issue focused on one reviewable outcome.

Do not create a new system or abstraction merely because it seems reusable. Validate the need against concrete gameplay scenarios first.

## 2. Create a branch

Create a branch from the latest `main` for each task.

Naming:
- Feature: `feat/issue-<number>-<short-name>`
- Bug fix: `fix/issue-<number>-<short-name>`
- Design / documentation: `docs/issue-<number>-<short-name>`
- Tests: `test/issue-<number>-<short-name>`
- Refactoring: `refactor/issue-<number>-<short-name>`
- Maintenance: `chore/issue-<number>-<short-name>`

Use a `kebab-case` short description. The allowed branch prefixes are `feat`, `fix`, `docs`, `test`, `refactor` and `chore`.

Examples:
- `docs/issue-11-gameplay-loop`
- `fix/issue-18-event-choice-routing`

Do not commit unrelated work to the same branch.

## 3. Implement in small commits

- Keep each commit focused on one logical change.
- Prefer explicit code and clear responsibility boundaries.
- Keep gameplay rules in their owning domain/service, not in UI components.
- Keep content definitions separate from executable rules.
- Update tests in the same task as the implementation.

Suggested commit prefixes: `feat:`, `fix:`, `test:`, `refactor:`, `docs:`, `chore:`.

## 4. Test before opening a Pull Request

For code changes, run the unified validation command in Section 7. It runs the Python validator tests, project naming checks, IDE1006 checks for changed C# files and the complete .NET test suite. GitHub Actions uses the same Python entry point.

Tests should cover normal behavior, invalid and boundary inputs, state/resource invariants, relevant edge-case combinations and regression behavior for the changed flow.

If a change affects Godot scenes, node paths, UI, input or gameplay runtime, also open the project in Godot 4.7.2 .NET and verify the affected journey manually. The automated command does not launch Godot or prove scene wiring/rendered UI behavior. Record any manual checks and anything not tested in the PR.

For documentation-only changes, review links, terminology, consistency with existing decisions and rendered Markdown. Do not claim runtime tests were run when no runtime code changed.

## 5. Open a Pull Request

- Target `main`.
- Use the Pull Request template.
- Link the Issue using `Closes #<number>` when the PR fully resolves it.
- Explain what changed, why, and which responsibilities own the behavior.
- Record tests actually run and any checks not performed.
- Keep PRs small enough to review.

Draft PRs are appropriate when implementation or verification is incomplete.

## 6. Review and merge

Before merge:
- All required automated checks must pass.
- Review the diff for scope creep, duplicated rules and hard-coded content assumptions.
- Confirm documentation and tests match the implementation.
- Resolve review feedback.
- Do not merge a PR that still has unresolved runtime verification required by its acceptance criteria.

Merge only after the PR is ready and the repository owner approves it.

## 7. One-command validation

Prerequisites: Git, Python 3.10+, .NET 8 SDK and (for manual runtime checks) Godot 4.7.2 .NET.

From the repository root, fetch the latest base branch and run the complete automated validation:

```powershell
git fetch origin main
$branch = git branch --show-current
python Tools/Validate.py --base origin/main --head HEAD --branch $branch --check-branch
```

The command runs, in order:
1. Python unit tests for the project naming validator and validation runner.
2. Project naming checks for changed paths and JSON content; branch naming is checked when `--check-branch` is supplied.
3. C# IDE1006 naming checks for changed application/test files only. If no C# files changed, the check is explicitly reported as skipped.
4. The full .NET test suite in Release configuration.

The command stops at the first failure and returns a non-zero exit code. GitHub Actions calls this same script with the Pull Request base/head commits, so local and CI validation use the same orchestration. Documentation-only changes still run the suite; this keeps the verification path predictable.

For a focused Python validator test run while developing the validator itself:

```powershell
python -m unittest discover -s Tools/Tests -p "*Tests.py" -v
```

To audit every Git-tracked file and directory, including unchanged legacy paths, run:

```powershell
python Tools/CheckProjectNaming.py --audit-all
```

This standalone audit reports violations and safe PascalCase suggestions; it does not rename files. It exits with code 1 while violations remain. Untracked files are intentionally excluded. The initial findings and staged remediation plan are recorded in [Project Naming Audit](Docs/Quality/ProjectNamingAudit.md).

The validator checks the following scoped rules:

- JSON property names are checked when newly introduced or renamed; unchanged legacy property names are grandfathered.
- New or changed content IDs and ID references must use `snake_case`. An unchanged ID/reference remains grandfathered only when it belongs to the same JSON object and field; moving objects within arrays does not by itself count as an ID change.
- String values under `type`, `timeMode` and `stat` must use `PascalCase` in a modified JSON document.
- Every newly added or renamed tracked file is checked for a `PascalCase` logical name regardless of extension. Godot sidecars such as `.cs.uid` and `.svg.import` are checked using the underlying source/asset name.
- Newly created directories must use `PascalCase`; unchanged legacy paths are not revalidated just because file contents change. Use the standalone `--audit-all` mode to find legacy path violations.
- Dot-separated PascalCase .NET project names such as `RPG.Tests` and `RPG.Tests.csproj` are accepted as project naming conventions.
- Explicit exceptions are the repository entry/configuration files `README.md`, `CONTRIBUTING.md`, `.editorconfig`, `.gitattributes`, `.gitignore`, and Godot's `project.godot`, `icon.svg`, and `icon.svg.import`; the `.github` and `.godot` platform/tool-managed trees are also exempt.

The Pull Request source branch must match `<type>/issue-<number>-<kebab-case-description>`.

Open the project in Godot 4.7.2 .NET and run:

```text
Scenes/Main.tscn
```

## 8. Definition of Done

A task is done when:
- Its acceptance criteria are met.
- Relevant tests pass, including edge cases.
- Runtime behavior is manually checked when applicable.
- Documentation reflects the actual implementation.
- The PR is reviewed and merged.

## 9. Naming conventions

Use the [Google C# Style Guide](https://google.github.io/styleguide/csharpguide.html) as the naming baseline for C# code. The rules below adapt it to this Godot project and define conventions for project areas that the C# guide does not cover. This section adopts naming rules only; it does not automatically adopt every Google formatting rule. Follow these rules for new code and files. Do not rename existing serialized IDs or resources as a drive-by cleanup; migrate them in a focused change and update every reference.

### C# identifiers

| Element | Convention | Example |
|---|---|---|
| Class, struct, record, enum, delegate, method and test method | `PascalCase` | `CharacterData`, `CalculateDuration`, `RestAtMaximumEnergyDoesNotExceedMaximum` |
| Public field/property, and event at any accessibility | `PascalCase` | `MaxHealth`, `ActionSelected` |
| Interface | `I` + `PascalCase` | `IActionExecutor` |
| Parameter and local variable | `camelCase` | `startingEnergy` |
| Private, protected or internal field/property | `_camelCase` | `_combatService` |
| Namespace | `PascalCase` | `RPG.Services` |

Casing depends on visibility, not on modifiers: `static`, `const` and `readonly` do not change the naming rule. Treat acronyms as ordinary words: use `UiTheme`, `LoadRpcRequest` and `CharacterId`, not `UITheme`, `LoadRPCRequest` or `CharacterID`. Prefer clear, descriptive names over unexplained abbreviations.

Use namespaces only where they provide a useful boundary. Keep namespaces to two levels where practical; do not create namespaces mechanically from the folder tree. This is a Godot application, so existing application-level types may remain in the global namespace until a deliberate namespace design is adopted.

### Files, folders and Godot resources

- Use `PascalCase` for directory and file names.
- A C# file should match its main type, and should generally contain one core type: `CombatService.cs`.
- Name test methods in `PascalCase`; do not use underscores to divide test-name segments.
- Use `PascalCase` for Godot scene files, custom node names and resource files: `CombatPanel.tscn`.
- Use `PascalCase` for JSON data filenames, assets and Markdown document filenames: `AcademyLocations.json`, `CharacterPortrait.svg`, `MainController.md`.
- Keep the conventional repository entry files `README.md` and `CONTRIBUTING.md`, plus platform/tool-managed names such as `.github` and `.editorconfig`, unchanged.

### JSON and game content

| Element | Convention | Example |
|---|---|---|
| JSON property | `camelCase` | `startingEnergy`, `maxHealth` |
| Game content ID | `snake_case` | `player_ari`, `inspect_letter`, `courtyard_goblin_ambush` |
| Serialized enum-like value | `PascalCase` | `RestoreEnergy`, `NextDay`, `Agility` |
| Player-facing label/text | Natural language | `"Read the letter on your desk"` |

Use `snake_case` consistently for IDs of Characters, Locations, Actions, Events, Choices and other authored content. IDs are machine-facing stable identifiers; labels and descriptions are player-facing text. Do not mix hyphens and underscores within the same ID system.

### Git names

Google's C# guide does not define Git naming. This repository uses:

- Branch: `<type>/issue-<number>-<short-description>`, with a `kebab-case` description. Allowed types: `feat`, `fix`, `docs`, `test`, `refactor`, `chore`.
- Commit: Conventional Commit prefixes in lowercase, such as `feat:`, `fix:`, `test:`, `refactor:`, `docs:` and `chore:`.

Examples: `feat/issue-26-location-action-pipeline`, `docs/issue-28-naming-conventions`, `feat: add location action pipeline`.

