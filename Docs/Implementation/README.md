# Implementation Documentation

This folder is the source of truth for how the current repository is built, how runtime responsibilities are divided, how authored data is loaded, and how changes are verified.

## Documents

| Document | Owns |
|---|---|
| [Codebase Guide](CodebaseGuide.md) | Repository map and where to begin code discovery |
| [Architecture](Architecture.md) | Layers, dependency direction, composition root and runtime flows |
| [Exploration Implementation](Exploration.md) | Current Location/Event behavior and Exploration action pipeline |
| [World Model](WorldModel.md) | Domain vocabulary, modeling constraints and implementation review findings |
| [UI Contracts](UI.md) | UI composition, presentation flow and view contracts |
| [Testing](Testing.md) | Automated checks, manual Godot verification and acceptance journeys |
| [Version 1 Plan](Version1Plan.md) | Historical Version 1 implementation scope and acceptance criteria for the existing prototype |

## Rules

- Describe code that exists as **Implemented** only when verified on `main`.
- Describe planned code as **Planned** or **Proposed**; an Issue or PR does not make it implemented.
- Codebase facts must be traceable to paths, symbols, tests or observed runtime behavior.
- Do not define player fantasy, combat design, story ideas or progression goals here. Link to [Design Documentation](../Design/README.md).
- Do not duplicate current Session status, delivery order or handoff. [WORKFLOW.md](../../WORKFLOW.md), GitHub Issues and PRs own current work tracking.
- The old reincarnation life-simulation delivery roadmap is retained in [Archive](../Archive/DevelopmentRoadmap-v0-reincarnation-life-sim.md), not as current implementation authority.
- When an accepted design rule changes, update the relevant Design document first, then update this folder only if the implementation contract or code state changes.
