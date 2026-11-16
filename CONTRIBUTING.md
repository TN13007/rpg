# Contributing to RPG

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
- Maintenance: `chore/issue-<number>-<short-name>`

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
