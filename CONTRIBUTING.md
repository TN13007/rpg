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

For code changes, run:

```powershell
dotnet test RPG.Tests/RPG.Tests.csproj --configuration Release
```

Tests should cover:
- Normal behavior.
- Invalid and boundary inputs.
- State/resource invariants.
- Relevant combinations of edge cases.
- Regression behavior for the changed flow.

If a change affects Godot scenes, node paths, UI or input, also run the main scene in Godot 4.7.2 .NET and verify the affected journey manually. Automated .NET tests do not prove that scene wiring or UI interactions work.

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

## 7. Current project commands

Automated tests:

```powershell
dotnet test RPG.Tests/RPG.Tests.csproj --configuration Release
```

Check naming conventions for a changed application file:

```powershell
dotnet format style RPG.csproj --verify-no-changes --diagnostics IDE1006 --severity warn --include Scripts/Actions/ActionExecutionService.cs
```

Check naming conventions for a changed test file:

```powershell
dotnet format style RPG.Tests/RPG.Tests.csproj --verify-no-changes --diagnostics IDE1006 --severity warn --include RPG.Tests/ActionExecutionTests.cs
```

Replace the example paths with the C# files changed in your task. The CI workflow runs the same IDE1006 check on changed C# files only, so unrelated legacy naming issues do not block changes to other files.

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
| Public or protected property, field, event | `PascalCase` | `MaxHealth`, `ActionSelected` |
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

