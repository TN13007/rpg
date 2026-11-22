# Cross-Layer Architecture

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

### NPC presence

```text
Data/NpcSchedules.json
  -> Infrastructure NpcScheduleDataLoader / NpcScheduleCatalog
  -> GameContentCatalog reference validation
  -> Application NpcPresenceQuery
       <- CharacterData + current Location + GameClock minute
  -> ExplorationFlowController
       -> Location/Event description
       -> AvailableInteractionQuery NPC requirement filtering
       -> revalidate required NPC presence before action execution
```

Schedules are read-only content. The query does not move NPCs or mutate state; it resolves the active authored time interval when the current Location context is shown. The owning action flow still executes the selected Action.

### NPC interactions

```text
Data/NpcInteractions.json
  -> Infrastructure NpcInteractionDataLoader / NpcInteractionCatalog
  -> GameContentCatalog Character / Location reference validation
  -> Application NpcInteractionQuery(characterId, locationId)
  -> ExplorationFlowController
       -> See around: list NPCs from NpcPresenceQuery
       -> select NPC: query that Character's available interactions
       -> ActionSelectionRequest (SourceKind.Npc, characterId, actionId)
       -> revalidate NPC presence before ActionExecutionService
```

Location content owns actions performed with the place itself. Character-specific interactions are authored separately and may be global to that NPC or restricted to selected Locations. Opening `See around` and selecting a person are navigation only; the selected NPC Action is what advances GameClock and applies effects.

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

CombatService owns shared AP, Party action eligibility, Player/Enemy Turn transitions, deterministic EnemyPattern resolution, action resolution and combat-state mutation. Domain combat request/snapshot/result/event contracts live in Scripts/Domain/Combat/CombatContracts.cs. CombatSessionService coordinates encounter entry and one-time completion costs. The mapper converts results/events into presentation-safe data. UI does not calculate damage or mutate combat resources.

### Content loading

```text
Data/*.json
  -> Infrastructure GameContentLoader
      -> schema-specific readers (LocationDataLoader, CharacterDataLoader, NPC readers, etc.)
      -> Domain definitions
      -> indexed content catalogs
  -> GameContentCatalog cross-content reference validation
  -> runtime objects and Application services
  -> Presentation
```

Definitions are authored content; schema-specific loaders parse and validate their JSON before constructing content catalogs. `LocationDataLoader` owns `Locations.json` file access, parsing and Location-level integrity checks; `LocationCatalog` only indexes the already-loaded `LocationContent` definitions. `GameContentCatalog` validates references between content domains. Runtime objects hold mutable state. A data definition can select only behavior already supported by code. Adding another content record that uses an existing behavior may be data-only; introducing a new behavior requires implementation and tests.

## 5. Character definitions and runtime resources

Character content and live character state have separate ownership.

### Authored definition data

`Data/Characters/*.json` is parsed by `CharacterDataLoader` into `CharacterData` and indexed by `CharacterCatalog`. The definition owns identity, role, authored Level and base Stats, resource limits, starting resource values, Player Actions and combat action IDs. `CharacterStats`, `PlayerActionDefinition`, `ActionDefinition` and `ActionEffectDefinition` are authored definitions, not runtime state.

Collection-valued definitions are copied at construction and exposed through read-only lists. This includes nested Player Action location IDs and Action effects, as well as the action list on `CombatantDefinition`. Consumers may enumerate or query these collections but must not modify shared content. To change authored behavior, edit the source content or its loader/schema and construct a new definition.

### Runtime character state

Each `Character` instance owns its current Health, Fatigue, Stamina, Mood and Gold. It reads identity, role, Level, Stats, limits and starting values from its immutable `CharacterData`; it does not own or mutate that definition. The Character methods (`TakeDamage`, `RestoreHealth`, `Revive`, `AddFatigue`, `RestoreFatigue`, `SpendStamina`, `RestoreStamina`, `ChangeMood`, `AddGold` and `ResetToStartingValues`) are the supported mutation surface and enforce each resource's bounds.

The Character is the authoritative owner of these resource values. Gameplay services are authorized to call its mutation methods only as part of their use case:
The current codebase compiles its layers into one assembly, so C# accessibility cannot enforce which layer calls a public Character mutator. This permission boundary is therefore an explicit architecture contract: keep mutation calls inside the owning Application/Domain use cases, cover the behavior with tests, and do not call mutators from Presentation.
- `ActionExecutionService` advances world time and applies action Fatigue; `ActionEffectExecutor` applies the effects authored on an Action.
- `CombatService` coordinates combat actions, Stamina costs and victory rewards; `CombatEffectDispatcher` applies combat damage and other combat effects.
- `CombatSessionService` applies the encounter's one-time world-time and Fatigue cost.
- Bootstrap creates and shares the Player Character instance between Exploration and Combat. Party members retain their Character state across encounters; Enemy Characters are reset to authored starting values when a new encounter begins.

`ResetToStartingValues` is an explicit full reset of the five persistent resources. It is used during construction and when an Enemy is prepared for a new encounter. Encounter-only state is separate: Action Points, downed/revival flags, enemy pattern progress and temporary Guard/Evade effects belong to the combat runtime and are reset by `CombatService`, not by Character.

### Resource scope and presentation

Health, Fatigue, Stamina, Mood and Gold are Character-owned runtime resources. Their current and maximum values are displayed through `CharacterViewModel`; Presentation receives a read-only snapshot and does not mutate gameplay state. There is no separate Resource Manager: the Character already owns the invariants and the current prototype has no demonstrated need for another owner.

Gold currently supports authored gains and combat victory rewards only. A purchase/spending rule is not defined yet, so this change does not invent a Gold cost API or shop behavior. Level and base Stats are currently authored values; progression remains a separate design decision documented in [Progression](../Design/Progression.md).

## 6. Code discovery

Before changing a feature:

1. Search for its type, method, ID, signal or UI label.
2. Trace definition, callers/references and the path to state mutation and presentation.
3. Verify responsibility ownership and existing services.
4. Inspect tests and fixtures.
5. For cross-layer work, prepare a temporary Implementation Brief with current behavior, evidence, affected components, proposed change and verification.

If docs and code disagree, verify against current source and tests, then record the discrepancy.

## 7. Structural constraints

- Put code in the layer that owns its responsibility, not the layer that happens to call it.
- Group code by feature inside each layer.
- Keep one primary responsibility per file; closely related small contracts may share a file.
- Keep Godot controls under `Presentation/UI` and scenes under `Scenes/`.
- Move each `.cs.uid` sidecar with its script and update scene/resource references.
- Preserve existing global C# types and serialized IDs during structural moves.
- Keep `MainController` as composition root; do not move feature rules into it.
- Do not introduce universal executors for unrelated gameplay domains.
