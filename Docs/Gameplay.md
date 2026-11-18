# Gameplay

**Purpose:** Canonical description of the current Location/Event loop and the accepted Exploration action-choice pipeline.

The Location/Event section describes behavior recorded as implemented on `main`. The action-choice pipeline records the accepted design direction and its incremental implementation boundary. Do not treat future extension points as implemented systems.

## Location Loop and Events

**Status:** Implemented on `main` through PR #9. The rules below define the current vertical slice; they are not a final random-world simulation specification.

### 1. Core Gameplay Loop

The game uses a location-based loop. It does not require an Expedition, Run or continuous dungeon session abstraction.

```text
Player is at a Location
        |
        v
Choose an available Action or travel
        |
        v
If travel succeeds, update CurrentLocation
        |
        v
Evaluate the destination's Location Events
        |
        +---- No event ----> Show Location Exploration
        |
        +---- Event -------> Show Event choices
                                  |
                     +------------+------------+
                     |                         |
                  Combat                   Other action
                     |                         |
              Dedicated Combat UI       ActionExecutionService
                     |                         |
              CombatService resolves    Apply effects and time
                     |                         |
                     +------------+------------+
                                  |
                                  v
                       Return to same Location
                                  |
                                  v
                         Continue the day
```

An Event is an optional situation introduced during exploration. It does not automatically start an Encounter or Combat. The player must choose a resolution.

### 2. Location Event Ownership

- A `Location` declares the Event Definitions that may occur there.
- An Event Definition is reusable content: identity, title, description, chance and available choices.
- A runtime occurrence exists only when the selection process chooses that definition. The current prototype presents it immediately and keeps the active choice context in `MainController`.
- Event content is stored in `Data/AcademyLocations.json`; C# owns selection, validation and execution rules.
- An Event is not the same as an Encounter. The Goblin event offers a confrontation choice that starts the existing Combat encounter. Sneaking resolves through the normal Action execution path.

### 3. Prototype Event Selection Rules

1. Evaluate events only after a successful travel action changes the player's current Location.
2. The player must be able to act. If the player is dead or unconscious, the event evaluation is skipped and does not consume the daily attempt.
3. A Location's event pool is evaluated at most once per in-game day.
4. If the Location has no events, no attempt is recorded.
5. Evaluate event definitions in their authored JSON order. The first event whose chance succeeds is selected; at most one event is shown for that Location on that day.
6. A chance is a value from 0 through 1. Zero never triggers; one always triggers.
7. If all event rolls fail, revisiting the same Location on that day does not reroll. The Location can be evaluated again on the next in-game day.
8. Event-attempt state is runtime-only in this prototype. It resets when the game process restarts because save/load is not implemented.

The once-per-location-per-day rule is a deliberately small prototype policy to prevent repeated travel from farming the same event. It is not a decision that every future Location or Event must use this cadence.

### 4. Event Choice Resolution

An Event owns its available choices. A choice contains an Action Definition for presentation and normal action costs/time/effects. It may also reference an existing Location Action through `encounterActionId`.

- A choice with `encounterActionId` delegates to the referenced Location Action, then starts the existing Encounter/Combat flow.
- A choice without `encounterActionId` executes its own Action Definition through `ActionExecutionService`.
- The Event Choice ID must match its Action ID so the UI's emitted choice identifier always resolves to exactly one authored choice.
- A choice that cannot currently execute is rejected without spending resources or time; the Event remains available for another choice.
- Successful non-combat resolution returns to Exploration at the same Location.
- Successful Combat resolution uses the dedicated Combat UI and `CombatService`. The existing victory reward, defeat/death rules and encounter duration remain authoritative.

The first event is `courtyard_goblin_ambush`:
- **Confront the goblin:** references the Courtyard's existing `encounter_goblin` action and enters Combat.
- **Sneak past the goblin:** spends 1 Energy and 10 minutes, then returns to Courtyard Exploration.

### 5. Choice Presentation Boundary

An Event owns its Event Choices, but it does not own the general Exploration UI. Event Choices use the common Action Choice presentation contract described in [Exploration Action Choice Pipeline](Gameplay.md). For Version 1, an active Event is a focused context: show its situation choices and do not mix ordinary Location or Travel choices into the unresolved Event screen.

The Event query/presentation path is responsible for turning the active Event's authored choices into presentation data. Event resolution remains with the Event/application flow; a non-combat choice may use the normal action execution rules, while a confrontation can enter the dedicated Combat pipeline.

### 6. Responsibility Boundaries

| Responsibility | Owner |
|---|---|
| Event and choice definitions, content validation | `AcademyContent` and `LocationEventDefinition` |
| Daily event-pool attempt and random selection | `LocationEventService` |
| Travel, active event context, choice routing and mode transitions | `MainController` (application orchestrator) |
| Action cost, time and effect execution | `ActionExecutionService` |
| Combat validation, turns, enemy response and victory reward | `CombatService` |
| Exploration and Combat rendering | Existing UI panels |

No separate Expedition, Run or general-purpose Event System is introduced.

### 7. Automated Test Coverage

`RPG.Tests/LocationEventLoopTests.cs` verifies:
- Loading the Courtyard Event and its choices from JSON.
- Guaranteed and impossible event chances.
- Once-per-location-per-day behavior, next-day reset and independent locations.
- Skipping event evaluation when the character cannot act.
- Authored event order and one selected event per evaluation.
- Invalid event probabilities and choice definitions.
- Travel followed by event selection and successful Stealth resolution.
- Confrontation linking to the existing encounter action and starting Combat.
- Failed action execution not consuming Energy or advancing time.
- Content validation for duplicate Event IDs and unknown encounter-action references.

These are automated domain/content tests. GitHub Actions run [#43](https://github.com/TN13007/rpg/actions/runs/43769290553) for commit `f5f018dfe31adb4d600ea4efff95f6987e45250c` completed with 64 passed and 0 failed. This result covers automated .NET tests, not the Godot UI.

#### Runtime verification record

[Issue #10](https://github.com/TN13007/rpg/issues/10) was closed by the project owner with all five playable-journey acceptance criteria marked complete for the Godot 4.7.2 .NET runtime check. The retrospective [QA record](Archive/PlayableJourneyTestReport.md) records the verified scope and its audit limitations. The Issue did not retain Godot Output logs, screenshots/video, exact per-action values or the exact local build revision; therefore the result is owner-confirmed, not independently reproducible from attached evidence.

### 8. Deliberately Not Implemented Yet

- Event conditions beyond the player's ability to act and the Location's event pool.
- Weighted selection, priorities, cooldown variants or multiple events per day.
- Persistent Event history, save/load or world-state consequences across restarts.
- Dialogue, social, stealth checks, investigation and other specialized resolution modes.
- A general Encounter framework for every Event.
- Multiple Event pools and authored events at every Location.


## Exploration Action Choice Pipeline

**Project:** RPG — Adventurer Academy  
**Status:** Accepted design direction; implementation is incremental through Version 1  
**Related documents:** [World Model Foundation](World.md), [UI Architecture](Ui.md), [Version 1 Session Plan](Version1Plan.md)

### 1. Purpose

The Exploration UI must show the choices that make sense in the player's current context. It must not own the rules for discovering, validating or executing those choices.

The player may choose actions that originate from different parts of the world:

- Player-owned actions, such as Rest.
- Location-provided actions, such as Inspect the Notice Board.
- Travel choices derived from the current Location's exits/connections.
- Interactions supplied by present NPCs, items or world objects.
- Choices supplied by an active Event.

These sources should feed one presentation pipeline. The StoryPanel renders grouped view data and does not need to know which JSON file or domain object supplied a choice.

Combat remains a separate, specialized pipeline and UI.

### 2. Vocabulary and boundaries

| Concept | Meaning | Example |
|---|---|---|
| Action Definition | Authored description of an operation and its common execution data | Rest, Read Notice Board, Open Chest |
| Action Source | The entity/content scope that supplies an action definition | Player, Location, NPC, Interactable, Event |
| Action Choice | A concrete option offered now, associated with its source and optional target | Rest now; Talk to Mira; Open chest_01 |
| Availability | Whether a choice belongs in the current context and whether it can currently execute | Mira is present; Player has enough Energy |
| Action Request | The identity of the selected choice sent for handling | Source kind + source ID + action ID + target ID |
| Effect | A requested domain change processed by its owning rules | RestoreEnergy(3), UnlockDoor |
| ViewModel | Presentation-safe data that the UI can render | Group title and choice label |

Do not confuse the action's source with its UI group. A Player Action may appear under Personal; an NPC interaction may appear under Nearby. The group is a presentation decision, while the source identifies where the choice came from.

A common Action Definition is appropriate for shared fields such as ID, label, description, duration, Energy cost, availability requirements and common effects. Specialized systems still own rules that are unique to Travel, Dialogue, Containers, Puzzles or Combat. Do not force all behavior into a universal action executor.

### 3. Choice sources

#### 3.1 Player Actions

Player Actions are authored as part of the Player's character definition in `Data/Characters/Player.json`, under `playerActions`.

Examples:
- Rest: can be used at any Location.
- Check Condition: can be used anywhere and may be informational/zero-duration.
- Sleep: owned by the Player, but available only when its context requirements are met (initially Dormitory).

Ownership does not imply universal availability. An action can belong to the Player and still require a bed, an item, a target or a suitable Location.

The Version 1 migration moves the existing Rest and Sleep definitions out of the academy-wide root `selfActions` list and into Player Actions. The current `selfActions` field is transitional and must not become a second permanent source.

#### 3.2 Location Actions

Location Actions are authored by a Location in `Data/AcademyLocations.json`.

Examples:
- Read the notice board.
- Study field maps.
- Look around.
- Enjoy the scenery.
- Practice at the training area.

They are offered only while the Player is at the Location that owns them, subject to their availability requirements.

#### 3.3 Travel Choices

Travel choices are derived from the current Location's outgoing exits/connections. They are not duplicated as individually authored actions for every destination.

The query resolves each valid destination and produces a choice with:
- A stable destination reference.
- A player-facing destination label.
- The shared travel cost/time configuration.
- A source/group identity suitable for request routing.

The current prototype uses `Location.Exits`. Keep this model for Version 1. Introduce a separate Location Connection definition only when routes need independent conditions, costs, visibility, map metadata or shared references.

#### 3.4 Target Interactions — future extension point

An NPC, Item, Interactable Object or other present target may supply interactions when that target exists in the current context.

Examples:
- NPC: Talk, Ask, Trade.
- Chest: Inspect, Open, Search, Take Items.
- Door: Inspect, Open, Lockpick.
- Switch: Inspect, Activate.
- World item: Inspect, Pick Up.

A target interaction is not a Player Action merely because the Player performs it. Its availability depends on the target's presence and state. The action request must retain the target identity so two objects with the same action ID cannot be confused.

NPC definitions, inventory/item data, containers and generic interactables are not Version 1 implementation requirements. Add their loaders, runtime state and rule owners only when a playable feature needs them.

#### 3.5 Event Choices

An active Event supplies choices for resolving that specific situation. Event choices are contextual and temporary; they do not become permanent Player or Location Actions.

The current prototype treats an active Event as a focused interaction screen. Preserve that behavior in Version 1: show the active Event's choices and do not mix ordinary Location/Travel choices into the unresolved Event screen. Event resolution returns to Exploration or enters the dedicated Combat flow.

### 4. Exploration choice groups

The ViewModel should support groups rather than one flat list.

Proposed group order when ordinary Exploration is active:

1. **Personal** — Player Actions available in the current context.
2. **Nearby** — interactions with present NPCs, items and objects (future).
3. **At This Location** — Location Actions.
4. **Travel** — destinations reachable from the current Location.
5. **Current Situation** — Event Choices, when an Event is active.

For Version 1, implement Personal, At This Location and Travel. Event Choices use the same choice presentation contract but remain in their focused Event context. Nearby is a defined future extension point, not a reason to implement NPC/Object systems now.

Empty groups are omitted. The UI may render groups in the order supplied by the ViewModel; it must not infer domain rules from group names.

### 5. Data loading and runtime query

The intended pipeline is:

```text
JSON content
   |
   +--> CharacterDataLoader --> CharacterData (Player Actions)
   |
   +--> AcademyContent -------> LocationContent (Location Actions, exits, Events)
   |
   +--> future content loaders for NPCs / Items / Interactables
                                  |
                                  v
                         Runtime world state
                                  |
                                  v
                    AvailableInteractionQuery
                                  |
                                  v
                    ActionChoiceMapper
                                  |
                                  v
                    ExplorationViewModel
                                  |
                                  v
                    StoryPanel / Exploration UI
```

#### Loader

A Loader reads and validates its own source data and creates immutable definitions. Examples:
- `CharacterDataLoader`: `Player.json` and `playerActions`.
- `AcademyContent`: Locations, their actions, exits and Event definitions.
- Future loaders should be introduced with their respective content features.

A Mapper does not call a Loader and does not read JSON. It consumes already-loaded definitions and query results.

#### Available Interaction Query

The Query combines the loaded definitions with current runtime state:
- Player identity/state and current capabilities.
- Current Location.
- Location exits and authored Location Actions.
- Present targets and their runtime states, when implemented.
- Active Event and its choices, when present.
- Relevant contextual requirements such as required Location, required item, target state or player life state.

The Query answers: **Which choices belong in this context, and which are currently executable?**

It does not mutate gameplay state. Availability shown in the UI is advisory. The authoritative handler/service must validate every request again at execution time.

Distinguish:
- **Not present in context:** omit the choice (for example, an NPC who is not here).
- **Present but temporarily unavailable:** include it as disabled with a reason when that information is useful to the player (for example, not enough Energy).
- **Available:** include it as enabled.

#### Mapper

The Mapper transforms the query result into presentation-safe data. It formats duration and cost labels, maps choices into groups, and supplies stable identifiers. It does not load content, discover world objects, mutate state or decide gameplay outcomes.

### 6. ViewModel contract

The Exploration view should receive a composed ViewModel similar to:

```text
ExplorationViewModel
  LocationTitle
  RegionLabel
  Description
  TimeLabel
  ActionGroups[]
    GroupId
    GroupTitle
    Choices[]
      ChoiceId
      SourceKind
      SourceId
      ActionId
      TargetId (optional)
      Label
      Description
      DurationLabel
      CostLabel
      IsEnabled
      UnavailableReason
```

This is a conceptual contract, not a requirement to copy these exact property names. Keep it small and add fields only when a real view needs them.

Choice identity must be unambiguous. Action IDs alone are not globally unique: two chests may both define `open`. Preserve source and target identity in the request. Do not route an action using only a display label or a bare action ID when more than one source can provide it.

The UI:
- Renders the groups and choices it receives.
- Shows duration/cost/description and any unavailable reason.
- Emits the selected choice identity.
- Does not read JSON or runtime domain objects.
- Does not calculate costs, check puzzle answers, open chests or mutate game state.

### 7. Selection and execution flow

```text
Player selects a rendered choice
        |
        v
ActionSelectionRequest (source + action + optional target)
        |
        v
MainController / request handler
        |
        v
Resolve the responsible rule owner
        |
        +--> Player / Location general action -> ActionExecutionService
        +--> Travel -> travel/location rule owner
        +--> Event choice -> Event resolution owner
        +--> NPC / Object interaction -> future specialized service
        +--> Combat choice -> CombatService (separate mode)
        |
        v
Validate again -> execute -> update runtime state
        |
        v
Re-run AvailableInteractionQuery
        |
        v
Mapper -> ViewModel -> render updated choices
```

MainController orchestrates which handler owns the request. It must not implement action requirements, Energy rules, puzzle rules, chest behavior, NPC dialogue or combat damage.

After a successful state change, rebuild the current Exploration ViewModel from current state. This naturally removes choices whose target/state/conditions have changed.

### 8. Relationship to Skills and Combat Actions

Action and Skill are related but different concepts:
- An **Action** is an operation a character performs: Rest, Talk, Attack, Parry.
- A **Skill** is a learned capability or proficiency: Swordsmanship, Parry Mastery, Herbalism.
- A **Combat Action** is an operation selected inside the Combat mode: Attack, Defend, Dodge, later Parry or a weapon technique.
- A Skill may unlock a Combat Action or modify its effectiveness, cost, duration or result.

The current Combat pipeline remains separate:

```text
Combat state / character capabilities
    -> CombatService
    -> CombatSnapshot / CombatResult
    -> CombatPresentationMapper
    -> CombatViewData
    -> CombatPanel
```

Do not put Attack, Defend, Dodge or future Parry into the Exploration Player Actions list. Do not build the Skill system as part of the Version 1 Exploration choice pipeline; Skill progression belongs to Version 2.

### 9. Version 1 implementation boundary

Implement now:
- Player Actions loaded from `Player.json`.
- Location Actions loaded from academy content.
- Travel choices derived from current Location exits.
- Event choices rendered through the same choice presentation contract while preserving focused Event behavior.
- Available Interaction Query for the sources above.
- Grouped Exploration ViewModel and generic grouped UI rendering.
- Stable source/action identity in requests.
- Existing time, Energy, Effect and Event/Combat execution rules preserved.

Defer:
- NPC presence, schedules and dialogue.
- Inventory and item-use interactions.
- Generic Interactable Object and Container systems.
- Puzzle definitions and PuzzleService.
- General-purpose action plugin/registry frameworks.
- Combat Actions and Skill progression inside the Exploration query.

### 10. Acceptance criteria

- Adding a Player Action to `Player.json` makes it available according to its authored context requirements without editing StoryPanel.
- Rest appears in Personal at every Location, including when the Player is unconscious; death still blocks it.
- Sleep is a Player-owned action but appears only in an allowed context.
- Adding a Location Action to a Location's JSON makes it appear only at that Location.
- Travel choices are derived from valid exits and carry the correct destination identity.
- An active Event presents its own choices through the common view contract without leaking ordinary Travel/Location choices into the focused Event screen.
- The Mapper does not read files or mutate runtime state.
- Query does not mutate runtime state.
- UI does not own gameplay validation or execution.
- Duplicate action IDs from different sources do not route to the wrong handler/target.
- Unavailable requests are revalidated and fail without partial state mutation.
- Successful actions refresh choices from the resulting state.
- Combat remains on its existing dedicated Service → Mapper → ViewData → UI path.
- Automated tests cover normal cases, missing/invalid IDs, duplicate source-local IDs, unavailable context, state changes, and important combinations.

