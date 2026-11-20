# Application Architecture

**Engine:** Godot 4.7.2 .NET / C#  
**Main scene:** `Scenes/Main.tscn`  
**Composition root:** `Scripts/Bootstrap/MainController.cs`

This document is the single source for repository structure, layer ownership, dependency direction and current cross-layer runtime flows. It combines the former Codebase Guide and Architecture documents.

## 1. Repository map

| Path | Responsibility | Starting point |
|---|---|---|
| `Scripts/Bootstrap/` | Godot entry point and dependency composition | `MainController.cs` |
| `Scripts/Domain/` | Runtime state, definitions, game rules and calculations | `Characters/`, `Combat/`, `Time/` |
| `Scripts/Application/` | Use-case coordination, request validation and queries | `Actions/`, `Exploration/`, `Combat/` |
| `Scripts/Presentation/` | UI, flow controllers, ViewModels, mappers and formatters | `Controllers/`, feature UI and mappers |
| `Scripts/Infrastructure/` | Content loading and engine/platform adapters | `Content/` |
| `Data/` | Authored JSON content and ID references | Matching loader/schema |
| `Scenes/` | Godot scene composition and previews | `Main.tscn` |
| `RPG.Tests/` | Automated tests grouped by feature | Matching feature folder |
| `Tools/` | Repository validation and supporting scripts | `Validate.py` |

Code is organized layer-first, then by feature. Do not start by reading every C# file. [Scripts/README.md](../../Scripts/README.md) is a shorter source locator.

## 2. Layer ownership

| Layer | Owns | Must not own |
|---|---|---|
| Bootstrap | Discover Godot nodes, construct dependencies, connect top-level events and start the game | Feature rules or detailed UI behavior |
| Domain | Character/runtime state, action definitions, combat/event rules, time and activity calculations | Godot UI, JSON parsing, Presentation types |
| Application | Use-case coordination, request validation at the application boundary, read-only context queries and execution orchestration | Rendering, file access or duplicated domain rules |
| Presentation | Godot controls, ViewModels, presentation mapping, formatting and screen/flow coordination | JSON loading, authoritative gameplay rules or direct state mutation outside the owning use case |
| Infrastructure | Read/validate external content and adapt Godot/platform APIs | Gameplay decisions or presentation formatting |

## 3. Dependency direction

```text
Presentation -> Application -> Domain
Bootstrap -> Presentation + Application + Domain + Infrastructure
Infrastructure -> Domain contracts/types where required
```

Domain remains independent of Godot UI and Infrastructure. Application does not read files or render controls. Presentation asks Application to perform work and maps results for display. Bootstrap is the composition root that may know all layers.

Do not add interfaces, repositories, dependency-injection containers or factories pre-emptively. Introduce an abstraction only for a demonstrated replacement, isolation or testing need.

## 4. Current runtime flows

### Exploration choices

```text
Data / content loaders
  -> Domain definitions + runtime state
  -> Application AvailableInteractionQuery
  -> Presentation ActionChoiceMapper
  -> ActionChoiceViewData
  -> StoryPanel
  -> ExplorationFlowController
  -> ActionExecutionService / owning domain flow
  -> Domain state changes
  -> query and map again
  -> refreshed StoryPanel
```

The query discovers choices for the current context without mutating state. The mapper converts query results into presentation-safe data. StoryPanel renders the choices and emits a stable source/action/target identity. The flow controller coordinates the request; the owning service revalidates it.

### Combat

```text
CombatPanel
  -> CombatFlowController
  -> Application CombatSessionService
  -> Domain CombatService + Application ActionExecutionService
  -> CombatResult + CombatSnapshot
  -> CombatPresentationMapper
  -> CombatViewData
  -> CombatPanel
```

CombatService owns turn validation, resolution, enemy response and combat-state mutation in the current prototype. CombatSessionService coordinates encounter entry and completion. The mapper converts results/events into presentation-safe data. UI does not calculate damage or mutate combat resources.

### Content loading

```text
Data/*.json
  -> Infrastructure GameContentLoader
      -> schema-specific readers
      -> Domain definitions
      -> ID/reference validation
  -> GameContentCatalog
  -> runtime objects and Application services
  -> Presentation
```

Definitions are authored content; IDs reference other definitions; catalogs index and validate loaded definitions; runtime objects hold mutable state. A data definition can select only behavior already supported by code. Adding another content record that uses an existing behavior may be data-only; introducing a new behavior requires implementation and tests.

## 5. Code discovery

Before changing a feature:

1. Search for its type, method, ID, signal or UI label.
2. Trace definition, callers/references and the path to state mutation and presentation.
3. Verify responsibility ownership and existing services.
4. Inspect tests and fixtures.
5. For cross-layer work, prepare a temporary Implementation Brief with current behavior, evidence, affected components, proposed change and verification.

If docs and code disagree, verify against current source and tests, then record the discrepancy.

## 6. Structural constraints

- Put code in the layer that owns its responsibility, not the layer that happens to call it.
- Group code by feature inside each layer.
- Keep one primary responsibility per file; closely related small contracts may share a file.
- Keep Godot controls under `Presentation/UI` and scenes under `Scenes/`.
- Move each `.cs.uid` sidecar with its script and update scene/resource references.
- Preserve existing global C# types and serialized IDs during structural moves.
- Keep `MainController` as composition root; do not move feature rules into it.
- Do not introduce universal executors for unrelated gameplay domains.
