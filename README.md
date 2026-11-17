# RPG — Adventurer Academy Prototype

Godot 4.7.2 .NET / C# project.

Adventurer Academy is a fantasy life-simulation RPG prototype about daily life, character wellbeing, academy activities, location-based exploration and occasional encounters.

## Start here

- [WORKFLOW.md](WORKFLOW.md) — the single source for the current implementation snapshot, Session progress, Learning evidence, blockers and handoff.
- [Version 1 Session Plan](Docs/Design/Version1SessionPlan.md) — the accepted nine-Session product plan. It defines intended scope and acceptance criteria; it does not mean those Sessions are already implemented.

Treat `main` as the implemented baseline. Open or unmerged PRs are proposals.

## Run the project

Open the repository in Godot 4.7.2 .NET and run `Scenes/Main.tscn`.

Run all automated repository checks from the repository root:

```powershell
git fetch origin main
$branch = git branch --show-current
python Tools/Validate.py --base origin/main --head HEAD --branch $branch --check-branch
```

This runs the Python validator tests, project naming checks, changed-file C# naming checks and the .NET test suite. Godot runtime/UI checks remain manual and must be recorded separately in `WORKFLOW.md`.

## Design and technical references

- [Game Overview](Docs/Design/GameOverview.md)
- [Game Development Roadmap](Docs/Design/GameDevelopmentRoadmap.md)
- [Gameplay Loop and Location Events](Docs/Design/GameLoopAndLocationEvents.md)
- [UI Architecture](Docs/Design/UiArchitecture.md)
- [MainController Responsibilities](Docs/Architecture/MainController.md)
- [World Model Foundation](Docs/Design/WorldModelFoundation.md)
- [Open Design Questions](Docs/Design/OpenDesignQuestions.md)
- [Automated Gameplay Tests](Docs/Testing/AutomatedTests.md)
- [Day 1 Playable Scenario](Docs/QA/Day1PlayableScenario.md)

See [CONTRIBUTING.md](CONTRIBUTING.md) for the Issue → Branch → Validate → Pull Request workflow. Run the complete automated checks with one command: `python Tools/Validate.py` (see the required base/branch arguments in the guide).
