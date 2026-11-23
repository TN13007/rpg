# Cross-Layer Architecture

**Engine:** Godot 4.7.2 .NET / C#  
**Main scene:** `Scenes/Main.tscn`  
**Composition root:** `Scripts/Bootstrap/MainController.cs`

This document is the single source for repository structure, layer ownership, dependency direction and current cross-layer runtime flows. It combines the former Codebase Guide and Architecture documents.

## 1. Repository map

| Path | Responsibility | Start here |
|---|---|---|
| `Scripts/Bootstrap/` | Godot entry point and dependency composition | `MainController.cs` |
| `Scripts/Core/` | Shared state, definitions and rules | `Actions/`, `Characters/`, `Activities/`, `Time/` |
| `Scripts/Features/Exploration/` | Navigation, Location interactions, NPCs, Events and their presentation | Matching responsibility folder |
| `Scripts/Features/Combat/` | Combat rules, encounter lifecycle, setup, content and presentation | `Rules/`, `Encounter/`, `Presentation/` |
| `Scripts/Content/` | Cross-feature loaders, catalogs and reference validation | `GameContentLoader.cs`, `GameContentCatalog.cs` |
| `Scripts/UI/` | Shared shell controls, styles and presentation utilities | `Shell/`, `Shared/` |
| `Data/` | Authored JSON grouped by content category | `Locations/`, `Characters/`, `NPC/`, `Combat/` |
| `Scenes/` | Godot scene composition and previews | `Main.tscn` |
| `RPG.Tests/` | Tests arranged by Core, feature, content and integration responsibility | Matching production responsibility |
| `Tools/` | Repository validation and supporting scripts | `Validate.py` |

The source tree is feature-first, with a small Core for genuinely shared capabilities. A feature folder contains its gameplay rules/queries, flow coordination, presentation and feature-specific content. The logical responsibilities below still apply; they are not mirrored as global `Domain/`, `Application/`, `Presentation/` and `Infrastructure/` directories. [Scripts/README.md](../../Scripts/README.md) is the practical file locator.

## 2. Layer ownership

| Layer | Owns | Must not own |
|---|---|---|
| Bootstrap | Discover Godot nodes, construct dependencies, connect top-level events and start the game | Feature rules or detailed UI behavior |
| Domain | Character/runtime state, action definitions, combat/event rules, time and activity calculations | Godot UI, JSON parsing, Presentation types |
| Application | Use-case coordination, request validation at the application boundary, read-only context queries and execution orchestration | Rendering, file access or duplicated domain rules |
| Presentation | Godot controls, ViewModels, presentation mapping, formatting and screen/flow coordination | JSON loading, authoritative gameplay rules or direct state mutation outside the owning use case |
| Infrastructure | Read/validate external content and adapt Godot/platform APIs | Gameplay decisions or presentation formatting |

## 3. Dependency direction

Folder placement is feature-first; dependency ownership remains explicit:

```text
Bootstrap
  -> Features + Core + Content + UI

Feature UI / Presentation
  -> Feature flow and use-case coordination
  -> Core rules and state

Feature Content loaders
  -> authored Data + Core definitions

Content/GameContentLoader
  -> shared loaders + feature content loaders
  -> Content/GameContentCatalog reference validation

Core
  -X-> Features / UI / Content
```

A feature may contain code that performs different logical roles (rules, use-case coordination, UI mapping or content loading) because those parts implement one gameplay responsibility. Keep dependencies pointing inward: feature rules do not depend on Godot UI; UI does not mutate authoritative state directly; loaders do not decide gameplay outcomes. Core remains independent of feature-specific behavior.

Do not add interfaces, repositories, dependency-injection containers or factories pre-emptively. Introduce an abstraction only for a demonstrated replacement, isolation or testing need.

## 4. Current runtime flows

### Exploration choices

```text
Data / content loaders
  -> Domain definitions + runtime state
  -> Features/Exploration/Interactions/AvailableInteractionQuery
  -> Features/Exploration/Presentation/Mappers/ActionChoiceMapper
  -> ActionChoiceViewData
  -> StoryPanel
  -> ExplorationFlowController
  -> ActionExecutionService / owning domain flow
  -> Domain state changes
  -> query and map again
  -> refreshed StoryPanel
```

The query discovers choices for the current context without mutating state and returns a read-only collection; callers cannot cast the result back to a mutable list. The mapper converts query results into presentation-safe data. StoryPanel renders the choices and emits a stable source/action/target identity. Displayed availability is advisory, not an authorization token.

When a selection arrives, `ExplorationFlowController` resolves it again against the current context before calling execution: Player actions must still belong to the Player and be available at the current Location; Location actions are looked up again from the current Location; NPC interactions require the same selected NPC to remain present at the current GameClock time and the interaction to remain in that NPC's current query results; Travel must still originate at the current Location and target one of its current exits; Event choices must still belong to the active Event, match the selected encounter target and satisfy current NPC-presence requirements. The action path checks required NPC presence again immediately before execution.

`ActionExecutionService.TryExecute` is the final Actor-state gate. It calls `CanExecute` again immediately before calculating duration or mutating GameClock/Character state, so a previous availability query or preflight result cannot authorize an action after the Actor becomes incapacitated. On success it advances time, applies action Fatigue and then authored effects. Context checks stay in the owning flow because they depend on source-specific state; the generic executor owns Actor-level eligibility and effect application. Rejected requests must leave time and resources unchanged.

### NPC presence

```text
Data/NPC/NpcSchedules.json
  -> Features/Exploration/NPC/Content/NpcScheduleDataLoader / NpcScheduleCatalog
  -> GameContentCatalog reference validation
  -> Features/Exploration/NPC/NpcPresenceQuery
       <- CharacterData + current Location + GameClock minute
  -> ExplorationFlowController
       -> Location/Event description
       -> AvailableInteractionQuery NPC requirement filtering
       -> revalidate required NPC presence before action execution
```

Schedules are read-only content. The query does not move NPCs or mutate state; it resolves the active authored time interval when the current Location context is shown. The owning action flow still executes the selected Action.

### NPC interactions

```text
Data/NPC/NpcInteractions.json
  -> Features/Exploration/NPC/Content/NpcInteractionDataLoader / NpcInteractionCatalog
  -> GameContentCatalog Character / Location reference validation
  -> Features/Exploration/NPC/NpcInteractionQuery(characterId, locationId)
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

CombatService owns shared AP, Party action eligibility, Player/Enemy Turn transitions, deterministic EnemyPattern resolution, action resolution and combat-state mutation. Domain combat request/snapshot/result/event contracts live in Scripts/Features/Combat/Rules/CombatContracts.cs. CombatSessionService coordinates encounter entry and one-time completion costs. The mapper converts results/events into presentation-safe data. UI does not calculate damage or mutate combat resources.

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

Definitions are authored content; schema-specific loaders parse and validate their JSON before constructing content catalogs. `Features/Exploration/Locations/Content/LocationDataLoader` owns `Locations.json` file access, parsing and Location-level integrity checks; `Features/Exploration/Locations/Content/LocationCatalog` only indexes the already-loaded `LocationContent` definitions. `Content/GameContentCatalog` validates references between content domains. Runtime objects hold mutable state. A data definition can select only behavior already supported by code. Adding another content record that uses an existing behavior may be data-only; introducing a new behavior requires implementation and tests.

## 5. Character definitions and runtime resources

Character content and live character state have separate ownership.

### Authored definition data

`Data/Characters/*.json` is parsed by `Content/Characters/CharacterDataLoader` into `CharacterData` and indexed by `Content/Characters/CharacterCatalog`. The definition owns identity, role, authored Level and base Stats, resource limits, starting resource values, Player Actions and combat action IDs. `CharacterStats`, `PlayerActionDefinition`, `ActionDefinition` and `ActionEffectDefinition` are authored definitions, not runtime state.

Collection-valued definitions are copied at construction and exposed through read-only lists. This includes nested Player Action location IDs and Action effects, as well as the action list on `CombatantDefinition`. Consumers may enumerate or query these collections but must not modify shared content. To change authored behavior, edit the source content or its loader/schema and construct a new definition.

### Runtime character state

Each `Character` instance owns its current Health, Fatigue, Stamina, Mood and Gold. It reads identity, role, Level, Stats, limits and starting values from its immutable `CharacterData`; it does not own or mutate that definition. The Character methods (`TakeDamage`, `RestoreHealth`, `Revive`, `AddFatigue`, `RestoreFatigue`, `SpendStamina`, `RestoreStamina`, `ChangeMood`, `AddGold`, `ReduceGold` and `ResetToStartingValues`) are the supported mutation surface and enforce each resource's bounds.

The Character is the authoritative owner of these resource values. Gameplay services are authorized to call its mutation methods only as part of their use case:
The current codebase compiles its layers into one assembly, so C# accessibility cannot enforce which layer calls a public Character mutator. This permission boundary is therefore an explicit architecture contract: keep mutation calls inside the owning Application/Domain use cases, cover the behavior with tests, and do not call mutators from Presentation.
- `Core/Actions/Execution/ActionExecutionService` advances world time and applies action Fatigue; `Core/Actions/Effects/ActionEffectExecutor` applies the effects authored on an Action.
- `CombatService` coordinates combat actions, Stamina costs and victory rewards; `CombatEffectDispatcher` applies combat damage and other combat effects.
- `Features/Combat/Encounter/CombatSessionService` applies the encounter's one-time world-time and Fatigue cost.
- Bootstrap creates and shares the Player Character instance between Exploration and Combat. Party members retain their Character state across encounters; Enemy Characters are reset to authored starting values when a new encounter begins.

`ResetToStartingValues` is an explicit full reset of the five persistent resources. It is used during construction and when an Enemy is prepared for a new encounter. Encounter-only state is separate: Action Points, downed/revival flags, enemy pattern progress and temporary Guard/Evade effects belong to the combat runtime and are reset by `CombatService`, not by Character.

### Resource scope and presentation

Health, Fatigue, Stamina, Mood and Gold are Character-owned runtime resources. Their current and maximum values are displayed through `CharacterViewModel`; Presentation receives a read-only snapshot and does not mutate gameplay state. There is no separate Resource Manager: the Character already owns the invariants and the current prototype has no demonstrated need for another owner.

Gold is a signed Character balance: positive values represent available money and negative values represent debt. Action effects use explicit `AddGold` and `ReduceGold` operations; reducing Gold may cross zero. This does not introduce a shop, loan or repayment schedule. Level and base Stats are currently authored values; progression remains a separate design decision documented in [Progression](../Design/Progression.md).

## 6. Code discovery

Before changing a feature:

1. Search for its type, method, ID, signal or UI label.
2. Trace definition, callers/references and the path to state mutation and presentation.
3. Verify responsibility ownership and existing services.
4. Inspect tests and fixtures.
5. For cross-layer work, prepare a temporary Implementation Brief with current behavior, evidence, affected components, proposed change and verification.

If docs and code disagree, verify against current source and tests, then record the discrepancy.

## 7. Structural constraints

- Organize source folders by Core and gameplay feature, not by four global technical layers.
- Put a feature's behavior, flow coordination, feature-specific content loading, presentation mapping and UI beside that feature, grouped by player-facing responsibility.
- Put state and rules in Core only when shared by multiple features or foundational.
- Put cross-feature loading/composition and cross-category reference validation in `Scripts/Content/`; keep a feature-specific loader/catalog beside the feature that owns that content.
- Keep shared shell controls and reusable presentation utilities in `Scripts/UI/`; keep feature-only controls inside the owning feature.
- Keep logical dependency constraints in Sections 2–3. Folder proximity does not authorize a dependency from Core into a Feature or direct UI mutation of gameplay state.
- Keep one primary responsibility per file; closely related small contracts may share a file.
- Move each `.cs.uid` sidecar with its script and update scene/resource references.
- Preserve existing global C# types, serialized IDs and authored data meaning during structural moves.
- Keep `MainController` as composition root; do not move feature rules into it.
- Do not introduce universal executors for unrelated gameplay domains.

## 8. Action state preparation and commit

`ActionEffectDefinition` uses explicit directional effects rather than signed amounts: every authored `Amount` must be greater than zero, while the calculated result may be unchanged (zero delta) after resource bounds are applied. `IncreaseFatigue` raises the Fatigue value; `DecreaseFatigue` lowers it. Health, Fatigue, Stamina and Mood remain within their Character limits. Gold is a signed balance: a negative value represents debt, and `ReduceGold` may move the balance below zero.

Action execution calculates its complete result before writing either authoritative owner. `CharacterRuntimeState` and `GameClockState` are immutable snapshots; `ActionEffectExecutor.Apply` and the state calculation methods return new values. `ActionStateChange` groups the proposed Character and clock values. `ActionStateChangeApplier` validates both owners before committing through `Character.ApplyRuntimeState` and `GameClock.ApplyState`. The service rejects stale plans by comparing the live owners with the snapshots captured during preparation. This is a synchronous single-threaded commit boundary, not a general transaction or concurrency lock. See [Function Responsibilities](FunctionResponsibilities.md) for the 18-group mapping and explicit exceptions.
