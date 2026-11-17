# RPG — Adventurer Academy Prototype

Godot 4.7.2 .NET / C# project.

Adventurer Academy is a fantasy life-simulation RPG prototype about daily life, character wellbeing, academy activities, location-based exploration and occasional encounters.

## Start here

- [WORKFLOW.md](WORKFLOW.md) — the single source for the current implementation snapshot, Session progress, Learning evidence, blockers and handoff.
- [Version 1 Session Plan](Docs/Design/VERSION_1_SESSION_PLAN.md) — the accepted nine-Session product plan. It defines intended scope and acceptance criteria; it does not mean those Sessions are already implemented.

Treat `main` as the implemented baseline. Open or unmerged PRs are proposals.

## Run the project

Open the repository in Godot 4.7.2 .NET and run `Scenes/Main.tscn`.

Run automated tests from the repository root:

```powershell
dotnet test RPG.Tests/RPG.Tests.csproj --configuration Release
```

.NET tests do not launch Godot or verify scene wiring, rendered UI or input interactions. Use the relevant Session acceptance criteria and record runtime results in `WORKFLOW.md`.

## Design and technical references

- [Game Overview](Docs/Design/GAME_OVERVIEW.md)
- [Game Development Roadmap](Docs/Design/GAME_DEVELOPMENT_ROADMAP.md)
- [Gameplay Loop and Location Events](Docs/Design/GAME_LOOP_AND_LOCATION_EVENTS.md)
- [UI Architecture](Docs/Design/UI_ARCHITECTURE.md)
- [MainController Responsibilities](Docs/Architecture/MainController.md)
- [World Model Foundation](Docs/Design/WORLD_MODEL_FOUNDATION.md)
- [Open Design Questions](Docs/Design/OPEN_DESIGN_QUESTIONS.md)
- [Automated Gameplay Tests](Docs/Testing/AUTOMATED_TESTS.md)
- [Day 1 Playable Scenario](Docs/QA/DAY_1_PLAYABLE_SCENARIO.md)

See [CONTRIBUTING.md](CONTRIBUTING.md) for the Issue → Branch → Validate → Pull Request workflow. Run the complete automated checks with one command: `python Tools/Validate.py` (see the required base/branch arguments in the guide).
