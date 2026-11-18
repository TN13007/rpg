# Scripts Architecture Map

The codebase is organized **layer-first, then by feature**. Start from the layer that owns the responsibility; do not search every C# file.

| Layer | Responsibility | Start here |
|---|---|---|
| `Bootstrap/` | Godot entry point and dependency composition | `MainController.cs` |
| `Domain/` | Game state, definitions, rules and calculations | `Characters/Character.cs`, `Combat/CombatService.cs`, `Time/DayClock.cs` |
| `Application/` | Use-case execution and read-only context queries | `Actions/ActionExecutionService.cs`, `Combat/CombatSessionService.cs`, `Exploration/AvailableInteractionQuery.cs` |
| `Presentation/` | UI, flow coordination, ViewData and mappers | `Controllers/ExplorationFlowController.cs`, `Controllers/CombatFlowController.cs` |
| `Infrastructure/` | JSON/content loading and engine/platform integration | `Content/AcademyContent.cs`, `Content/CharacterDataLoader.cs` |

## Follow a feature

### Exploration choices

```text
Infrastructure/Content
  -> Domain definitions + current runtime state
  -> Application/Exploration/AvailableInteractionQuery
  -> Presentation/Mappers/Exploration/ActionChoiceMapper
  -> Presentation/ViewModels/Exploration/ActionChoiceViewData
  -> Presentation/UI/StoryPanel
```

When the player selects a choice:

```text
StoryPanel
  -> Presentation/Controllers/ExplorationFlowController
  -> Application/Actions/ActionExecutionService
  -> Domain state/rules
  -> Query again -> Mapper -> StoryPanel
```

### Combat

```text
CombatPanel
  -> Presentation/Controllers/CombatFlowController
  -> Application/Combat/CombatSessionService
  -> Application/Actions/ActionExecutionService (encounter entry)
  -> Domain/Combat/CombatService (turns and resolution)
  -> Presentation/Mappers/Combat/CombatPresentationMapper
  -> Presentation/ViewModels/Combat/CombatPresentation
  -> CombatPanel
```

## Dependency rules

- Domain must not depend on Godot UI, Presentation or Infrastructure.
- Application coordinates use cases and depends on Domain; it must not render UI or read files.
- Presentation coordinates UI flows and delegates authoritative rules/state changes to Application/Domain services.
- Infrastructure loads/adapts external data; it does not decide gameplay outcomes.
- Bootstrap composes the application graph and connects top-level Godot signals.

For the full architecture contract and current implementation notes, see [Docs/Architecture.md](../Docs/Architecture.md).
