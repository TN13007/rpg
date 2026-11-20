> Canonical location: [Architecture.md](Architecture.md). This document describes implementation, architecture, authored content contracts or delivery planning—not the intended player experience.

# Application Architecture

**Purpose:** Make the codebase's responsibility boundaries visible from the repository tree and define the allowed dependency direction.

## 1. Repository map

C# code is organized layer-first, then by feature. The directory is an architectural index: developers should be able to locate a responsibility before opening a source file.

```text
Scripts/
├── Bootstrap/       # Godot entry point and composition root
├── Domain/          # Runtime state, game concepts, rules and calculations
│   ├── Actions/
│   ├── Activities/
│   ├── Characters/
│   ├── Combat/
│   ├── Events/
│   ├── Exploration/
│   └── Time/
├── Application/     # Queries and use-case coordination
│   ├── Actions/
│   └── Exploration/
├── Presentation/    # UI, view data, mappers and presentation controllers
│   ├── Controllers/
│   ├── Formatters/
│   ├── Mappers/
│   ├── Previews/
│   ├── UI/
│   └── ViewModels/
└── Infrastructure/  # File/content loading and engine/platform integration
    └── Content/
```

Other project areas:
- `Data/`: authored JSON content; it is not executable code.
- `Scenes/`: Godot scenes and their composition.
- `RPG.Tests/`: automated tests grouped by feature.
- `Docs/`: canonical product, gameplay, UI and technical contracts.

## 2. Layers and ownership

| Layer | Owns | Must not own |
|---|---|---|
| Bootstrap | Discover Godot nodes, construct dependencies, connect top-level events and start the game | Feature rules or detailed UI behavior |
| Domain | Character/runtime state, action definitions, combat/event rules, time and activity calculations | Godot UI, JSON parsing, Presentation types |
| Application | Use-case coordination, request validation at the application boundary, read-only context queries and execution orchestration | Rendering, file access or duplicated domain rules |
| Presentation | Godot UI controls, ViewModels, presentation mapping, display formatting and screen/flow coordination | JSON loading, authoritative game rules or direct state mutation outside the owning use case |
| Infrastructure | Read/validate external content and adapt Godot/platform APIs | Gameplay decisions or presentation formatting |

### Dependency direction

```text
Presentation ──> Application ──> Domain
Bootstrap ─────> Presentation + Application + Domain + Infrastructure
Infrastructure ─> Domain contracts/types where required
```

Domain must remain independent of Godot UI and Infrastructure. Application must not read files or render controls. Presentation asks Application to perform work and maps results for display. Bootstrap is the only composition root that may know all layers.

Do not add interfaces, repositories, dependency-injection containers or factories pre-emptively. Introduce an abstraction only when a concrete replacement, isolation or testing need exists.

## 3. Main runtime flows

### Exploration

```text
Data/*.json
  -> Infrastructure content loaders
  -> Domain content definitions + runtime state
  -> Application AvailableInteractionQuery
  -> Presentation ActionChoiceMapper
  -> Exploration ViewData
  -> StoryPanel
  -> ActionSelectionRequest
  -> Presentation ExplorationFlowController
  -> Application ActionExecutionService or owning Domain flow
  -> Domain state changes
  -> query + map again
  -> refreshed StoryPanel
```

The query discovers choices for the current context and does not mutate state. Its input includes the location resolver and Travel definition; it does not reach into the static content loader. The mapper only converts query results into presentation data. StoryPanel renders groups and emits the stable source/action/target identity.

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

CombatService owns combat turn validation, resolution, enemy response and combat state. CombatSessionService coordinates encounter entry through ActionExecutionService and applies configured encounter duration/Fatigue on completion. The mapper converts results and events into presentation-safe data. CombatPanel and CombatFlowController do not calculate damage or mutate combat resources.

### Content loading

The composition root calls one entry point. Specialized readers still parse each schema; they are implementation details behind that entry point, not competing places for the game to initialize content.

```text
Data/*.json
  -> GameContentLoader
      -> schema-specific JSON readers
      -> Domain definitions
      -> ID/reference validation
  -> GameContentCatalog
  -> runtime objects and Application services
  -> Presentation
```

Content follows four distinct concepts:

1. **Definition:** immutable authored data such as a Combat Action's label, supported behavior and parameters.
2. **ID reference:** another definition stores a stable ID instead of copying the referenced data. For example, CharacterData stores `combatActionIds`.
3. **Catalog:** owns loaded definitions, indexes them by ID and rejects duplicate definitions or unresolved references before gameplay begins.
4. **Runtime:** Character and CombatService use resolved definitions while mutable Health, Stamina and encounter state remain runtime state.

Infrastructure owns JSON parsing, file access, catalog construction and cross-reference validation. Domain owns the meaning and constraints of definitions and the gameplay behavior that executes them. Presentation maps the resulting snapshots to UI; it does not parse content or decide gameplay rules.

A data definition can select only behavior already supported by code. Adding another action that uses an existing `CombatActionBehavior` can be content-only; introducing a genuinely new behavior requires a Domain implementation and tests. Combat AI, skill progression and arbitrary executable scripts in JSON are not part of this content pipeline.

The current pilot uses `Data/Combat/CombatActions.json` as the shared definition source and each file under `Data/Characters/` can reference actions through `combatActionIds`. `GameContentLoader` is the single composition entry point and discovers character JSON files in that directory. `CombatActionJsonReader`, `CharacterDataLoader` and `AcademyContent` remain focused schema-specific readers. `GameContentCatalog` owns the loaded Academy data, Character Catalog and Combat Action catalog; it does not rely on process-wide static Academy state.

## 4. File organization rules

1. Put code in the layer that owns its responsibility, not the layer that happens to call it.
2. Group by feature inside each layer. For example, Exploration mappers belong under `Presentation/Mappers/Exploration`.
3. Keep one primary responsibility per file. Small closely related enums/contracts may share a file; a query and a presentation mapper should not.
4. Keep Godot controls under `Presentation/UI`; keep scene files under `Scenes`.
5. Move each `.cs.uid` sidecar with its script and update all scene references when a script moves.
6. Preserve existing global C# types and serialized IDs during structural moves. Namespace/API migrations require a separate reason and tests.
7. Tests are grouped by feature, so a developer can find all regression coverage for one behavior together.

## 5. Current implementation and remaining extraction

The repository now has explicit layer directories and the Exploration contracts, query and mapper are separated. `MainController` is the Godot composition root; `ExplorationFlowController` and `CombatFlowController` own their respective presentation flows. The controllers coordinate UI and call Application/Domain services; they do not own authoritative gameplay rules. Issue [#15](https://github.com/TN13007/rpg/issues/15) tracks final verification and integration.

Do not interpret the folder map as proof that every dependency is already ideal. New code must follow the boundaries above; legacy coupling is removed when a focused change provides a testable improvement.

## 6. Design constraints

- Do not introduce a universal action executor for Travel, Events, Combat, Puzzles or future target interactions.
- Do not move business rules into MainController to avoid creating a focused owner.
- Do not make UI availability the authoritative validation; the owning service/handler revalidates requests.
- Do not perform broad directory or namespace migrations as drive-by cleanup.
