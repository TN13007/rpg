# RPG — Adventurer Academy Prototype

Godot 4.7.2 .NET / C# project.

Adventurer Academy is a fantasy combat-first RPG prototype. The player commands a four-character party through Timeline-based tactical battles, while the Academy supports preparation, progression, relationships and a deeper narrative.

## Start here

- [AGENTS.md](AGENTS.md) — repository navigation and working instructions for coding agents.
- [WORKFLOW.md](WORKFLOW.md) — current implementation snapshot, Session progress, Learning evidence, blockers and handoff.
- [Version 1 Plan](Docs/Implementation/Version1Plan.md) — detailed product and Learning-first plan for Version 1. It describes intended scope; it does not mean every Session is implemented.
- [CONTRIBUTING.md](CONTRIBUTING.md) — Issue → Branch → Validate → Pull Request workflow, naming and documentation governance.

## Run the project

Open the repository in Godot 4.7.2 .NET and run `Scenes/Main.tscn`.

Run all automated repository checks from the repository root:

```powershell
git fetch origin main
$branch = git branch --show-current
python Tools/Validate.py --base origin/main --head HEAD --branch $branch --check-branch
```

This runs the Python validator tests, project naming checks, changed-file C# naming checks and the .NET test suite. Godot runtime/UI checks remain manual and must be recorded separately in `WORKFLOW.md`.

## Documentation

- [Documentation Hub](Docs/README.md) — choose Design or Implementation.
- [Design](Docs/Design/README.md) — game vision, player experience, gameplay rules, combat, progression, world and product roadmap.
- [Implementation](Docs/Implementation/README.md) — codebase guide, architecture, runtime flows, UI contracts, testing and delivery plan.
- [WORKFLOW.md](WORKFLOW.md) — the single project-status, Session progress and handoff tracker.

Historical or superseded material is retained under `Docs/Archive/`; it is not current authority.

## Working with AI

Start with [AGENTS.md](AGENTS.md) for repository navigation and code-discovery instructions. At the beginning of each development Session, follow the Implementation Brief process in [WORKFLOW.md](WORKFLOW.md). AI should gather and synthesize only the relevant repository context; this document index is for navigation, not a checklist of files the learner must read.
