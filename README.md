# RPG — Adventurer Academy Prototype

Godot 4.7.2 .NET / C# project.

Adventurer Academy is a fantasy life-simulation RPG prototype about daily life, character wellbeing, academy activities, location-based exploration and occasional encounters.

## Start here

- [AGENTS.md](AGENTS.md) — repository navigation and working instructions for coding agents.
- [WORKFLOW.md](WORKFLOW.md) — current implementation snapshot, Session progress, Learning evidence, blockers and handoff.
- [Version 1 Plan](Docs/Version1Plan.md) — detailed product and Learning-first plan for Version 1. It describes intended scope; it does not mean every Session is implemented.
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

## Active design and technical documents

- [Game Design](Docs/GameDesign.md) — product vision and long-term world direction.
- [Roadmap](Docs/Roadmap.md) — version sequence, player-facing outcomes and exit criteria.
- [World Model](Docs/World.md) — domain vocabulary, modeling constraints and review findings.
- [Gameplay](Docs/Gameplay.md) — Location/Event loop and Exploration action-choice pipeline.
- [UI Architecture](Docs/Ui.md) — main interface and Combat presentation contracts.
- [Application Architecture](Docs/Architecture.md) — MainController and application responsibility boundaries.
- [Testing and Acceptance](Docs/Testing.md) — automated tests, manual Godot checks and Day 1 scenario.

Historical records are retained under `Docs/Archive/` and are not current implementation guidance.

## Working with AI

Start with [AGENTS.md](AGENTS.md) for repository navigation and code-discovery instructions. At the beginning of each development Session, follow the Implementation Brief process in [WORKFLOW.md](WORKFLOW.md). AI should gather and synthesize only the relevant repository context; this document index is for navigation, not a checklist of files the learner must read.
