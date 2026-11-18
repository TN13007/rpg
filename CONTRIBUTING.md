# Contributing to RPG

Use [WORKFLOW.md](WORKFLOW.md) as the single project/session status and Learning-first hub. Treat latest `main` as the official implementation baseline; open or unmerged PRs are proposals. Follow the Session protocol there, and keep Product verification separate from Learning verification.

This repository uses an Issue → Branch → Commit + Tests → Pull Request → Review → Merge workflow.

## 1. Start with an Issue

Before changing code or design:
1. Search existing Issues and design documents.
2. Read the relevant README, specification and design documents.
3. Create or select an Issue using the appropriate template.
4. Define one problem or requirement and one independently verifiable outcome.
5. Record dependencies and related Issues when the work is not independent.

An Issue is the unit of planning. It is **not** a list of files to edit and should not prescribe implementation details before the problem is understood.

A good Issue answers:
- **Context / Problem:** What is wrong, missing or needed? What happens today?
- **Goal / Expected outcome:** What should be true after the work is complete?
- **Scope:** What is included, and what is explicitly out of scope?
- **Acceptance criteria:** What observable conditions prove the Issue is complete?
- **Verification:** How will each important criterion be checked?
- **References / Dependencies:** Which specifications, code areas or other Issues are relevant?

Keep one Issue focused on one coherent outcome. Split work when the outcomes can be implemented, verified and delivered independently. Keep related Issues linked when they contribute to a larger initiative; do not create separate Issues merely because a solution touches multiple files.

For bugs, include reliable reproduction steps and actual versus expected behavior. For design work, state the decision to be made and the gameplay scenarios or constraints that will validate it. A design Issue should produce a documented decision; implementation should be tracked separately when it is a distinct deliverable.

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

## 3. Make focused commits

A commit is **one coherent, atomic logical change — not one file**.

A good commit:
- Has one clear intent that can be explained in one sentence.
- Includes every related implementation, test, configuration, data and documentation change needed for that logical change to remain coherent.
- Excludes unrelated changes, even when those files were edited during the same work session.
- Leaves the codebase in a coherent state; do not intentionally commit a broken intermediate state that a later commit is expected to repair.
- Can be understood and verified from its diff and commit message.

Group files by the behavior or maintenance outcome they jointly implement. For example, renaming a JSON file, updating its loader, changing the test fixture path and updating the project copy configuration belong in the same logical commit when they are all required for that rename. Do not create one commit per file.

One Issue may require multiple commits when implementation has several meaningful steps. Keep each commit focused on one logical step; do not split a single indivisible change into artificial per-file commits. Separate unrelated outcomes into separate commits or PRs.

Use Conventional Commits:

```text
<type>(<optional-scope>): <imperative summary>
```

Allowed types:
- `feat`: add a capability or user-visible behavior.
- `fix`: correct behavior that does not meet its requirements.
- `refactor`: restructure code without intentionally changing behavior.
- `test`: add or change tests without changing production behavior.
- `docs`: change documentation.
- `chore`: maintenance, tooling, configuration or repository tasks.

Examples:
- `feat(actions): add action duration preview`
- `fix(energy): prevent negative values`
- `refactor(combat): extract encounter rules`
- `test(actions): cover unconscious Rest`
- `docs(workflow): clarify commit granularity`
- `chore(ci): enforce repository naming audit`

Write a concise imperative summary, use lowercase type and scope, and omit the trailing period. Add a commit body when the reason, trade-off or implementation detail is not clear from the summary. Use `!` and a `BREAKING CHANGE:` footer only when a change intentionally breaks an existing contract.

Prefer creating commits locally so related files can be staged together (use interactive staging when needed). Avoid a history made of one-file commits, vague messages such as `update files`, or temporary fix-up commits in the final PR history.

## 4. Test before opening a Pull Request

For code changes, run the unified validation command in Section 7. It runs the Python validator tests, project naming checks, IDE1006 checks for changed C# files and the complete .NET test suite. GitHub Actions uses the same Python entry point.

Tests should cover normal behavior, invalid and boundary inputs, state/resource invariants, relevant edge-case combinations and regression behavior for the changed flow.

If a change affects Godot scenes, node paths, UI, input or gameplay runtime, also open the project in Godot 4.7.2 .NET and verify the affected journey manually. The automated command does not launch Godot or prove scene wiring/rendered UI behavior. Record any manual checks and anything not tested in the PR.

For documentation-only changes, review links, terminology, consistency with existing decisions and rendered Markdown. Do not claim runtime tests were run when no runtime code changed.

## 5. Open a Pull Request

A PR is **one reviewable solution to a clearly defined problem or requirement**. It is the unit of review and integration.

A good PR:
- Delivers one coherent outcome linked to an Issue.
- May contain multiple commits and multiple files; file count does not define PR scope.
- Explains the problem, expected behavior, proposed solution and important implementation decisions.
- Lists the meaningful changes and explicitly names important non-goals or known limitations.
- Provides evidence for the tests and manual checks that were actually performed.
- Is small and focused enough for a reviewer to understand the complete change without unrelated context.

Use a title consistent with Conventional Commits:

```text
<type>(<optional-scope>): <outcome>
```

Examples: `fix(energy): prevent negative Energy`, `feat(summary): add daily activity summary`, `docs(workflow): define commit and PR standards`.

In the PR description:
- Use `Closes #<number>` only when this PR fully satisfies that Issue's acceptance criteria.
- Use `Part of #<number>` when the PR contributes to an Issue but does not complete it.
- Use `Related to #<number>` for context that does not represent completion.
- Do not mark an Issue complete merely because its implementation has been started or its PR has been opened.
- Explain any risks, compatibility concerns, migration steps and checks that remain unperformed.

Split work into separate PRs when it produces independent outcomes that can be reviewed and merged independently. Keep changes together when multiple files or commits are required to deliver one coherent outcome. For dependent PRs, state the dependency and merge order explicitly; target the appropriate parent branch until the dependency is merged, then retarget to `main`.

Use the Pull Request template. Open a Draft PR when the implementation or required verification is incomplete. Before requesting review, inspect the full diff yourself and remove accidental, generated or unrelated changes.

## 6. Review and merge

Before merge:
- All required automated checks must pass, including the full repository naming audit.
- Review the diff for scope creep, duplicated rules, unnecessary abstractions and hard-coded content assumptions.
- Confirm tests, documentation and the implementation describe the same behavior.
- Resolve review feedback or record an explicit decision for any deferred comment.
- Complete the required Godot runtime verification when scenes, node paths, UI, input or gameplay runtime are affected.
- Do not claim that a check passed unless it was actually run and its result was observed.

Merge only after the PR is review-ready and the repository owner approves it. A passing CI is necessary evidence, but it does not replace design review or manual runtime verification when those are required.

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

This standalone audit reports violations and safe PascalCase suggestions; it does not rename files. It exits with code 1 while violations remain. Untracked files are intentionally excluded. The initial findings and staged remediation plan are recorded in [Project Naming Audit](Docs/Archive/ProjectNamingAudit.md).

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

This repository uses the following Git conventions:

- **Branch:** `<type>/issue-<number>-<short-description>`; the description is `kebab-case`. Allowed types: `feat`, `fix`, `docs`, `test`, `refactor`, `chore`.
- **Commit:** `<type>(<optional-scope>): <imperative-summary>`, using the Conventional Commit types defined in Section 3.
- **Pull Request title:** use the same type/scope pattern and describe the outcome delivered by the PR.
- **Issue title:** use the prefix supplied by its template: `[Bug]`, `[Task]` or `[Design]`.

The three work items have different responsibilities:
- **Issue = one problem or requirement with verifiable acceptance criteria.**
- **Commit = one coherent logical change, which may include multiple related files.**
- **Pull Request = one reviewable solution, which may contain multiple commits and files.**

Examples:
- Branch: `feat/issue-26-location-action-pipeline`
- Commit: `feat(actions): add location action pipeline`
- PR: `feat(actions): add location action pipeline`
- Issue: `[Task] Add location action pipeline`

A single Issue can be implemented by several commits or, when delivery must be separated, by several explicitly linked PRs. Do not create one commit or PR per file.

## 10. Documentation governance

Documentation exists to preserve durable project knowledge, not to record every work session. Keep the smallest set of canonical documents that lets a contributor understand and change the project without duplicating the same rule in several places.

### 10.1 Source-of-truth ownership

Use one authoritative home for each kind of information:

- `README.md`: project entry point, quick start and links to active documentation.
- `WORKFLOW.md`: changing project status, Session handoff, blockers and Learning evidence.
- `CONTRIBUTING.md`: stable collaboration, Git and documentation rules.
- A domain design/technical document: accepted rules, contracts and rationale for that domain.
- GitHub Issue: one problem, requirement or unresolved design decision, with scope and acceptance criteria.
- GitHub PR: proposed implementation, changed files, review discussion and verification evidence.

If the same rule appears in multiple places, choose its canonical owner and replace the other copies with a short link. A Session Brief is a temporary working view; it does not override any of these sources.

### 10.2 Create and update documents

Before creating a Markdown file:
1. Search the existing documents and Issues for the topic.
2. Prefer adding or correcting a section in the existing canonical domain document.
3. Create a new file only when it has a distinct domain, audience, lifecycle or independently maintained contract that cannot reasonably live in an existing document.
4. Explain in the PR description why the existing document cannot own the content, what the new file owns and which active document will link to it.

Do not create a permanent document for an individual Session, meeting, temporary implementation plan, status update or list of open questions. Put work scope and acceptance criteria in Issues; put implementation and verification evidence in PRs; put current handoff in `WORKFLOW.md`.

An unresolved design question belongs in a GitHub Issue using the Design task template. When resolved, update the canonical design document with the accepted decision and close the Issue with a link to that section. The Issue discussion is decision history, not a second specification.

### 10.3 Document ownership and lifecycle

Every active document must have one clearly stated purpose and one canonical location. The repository maintainer is accountable for keeping it current; do not add a redundant per-file `Owner: @handle` line unless multiple contributors genuinely need explicit ownership assignment.

Use this lifecycle:

- **Draft:** incomplete proposal; must not be treated as accepted behavior.
- **Active:** current, maintained source of truth and linked from the README or an active parent document.
- **Deprecated:** superseded or no longer authoritative; add a notice linking to its replacement and stop using it for new work.
- **Archived:** retained for historical context under `Docs/Archive/`; not linked as current guidance.

Move a document to Deprecated when its content has been absorbed, superseded or invalidated. Archive it only after the replacement is available and incoming links have been updated. Age alone (for example, three months without edits) is not sufficient reason to archive a still-valid specification. When archiving documents, maintain `Docs/Archive/README.md` as the historical index.

### 10.4 Review and automation

Every documentation PR must check:
- Internal Markdown links and heading anchors affected by the change.
- Whether the document is linked from the README or an active parent document.
- Whether the change creates duplicate, contradictory or stale guidance.
- Whether the implementation and tests still agree with the documented behavior.

The repository should automate internal-link validation and detect orphaned active documents in CI. Until those checks are present, reviewers must perform these checks manually. Do not add a style/lint tool merely for cosmetic consistency; automate checks that prevent broken navigation or conflicting sources of truth.
