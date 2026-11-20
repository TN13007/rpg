# Scripts Source Map

The codebase is organized **layer-first, then by feature**. This file is a quick locator only; architecture rules and runtime flows are owned by [Implementation Architecture](../Docs/Implementation/Architecture.md).

| Layer | Responsibility | Start here |
|---|---|---|
| `Bootstrap/` | Godot entry point and dependency composition | `MainController.cs` |
| `Domain/` | Game state, definitions, rules and calculations | `Characters/Character.cs`, `Combat/CombatService.cs`, `Time/GameClock.cs` |
| `Application/` | Use-case execution and read-only context queries | `Actions/ActionExecutionService.cs`, `Combat/CombatSessionService.cs`, `Exploration/AvailableInteractionQuery.cs` |
| `Presentation/` | UI, flow coordination, ViewData, mappers and formatters | `Controllers/ExplorationFlowController.cs`, `Controllers/CombatFlowController.cs` |
| `Infrastructure/` | JSON/content loading and engine/platform integration | `Content/GameContentLoader.cs` and schema-specific readers |

For code changes, follow the discovery sequence in [AGENTS.md](../AGENTS.md): locate, trace, verify ownership, inspect tests, then plan. Do not treat this index as proof that a listed class owns every behavior; verify the current source.
