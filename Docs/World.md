# World Model

**Purpose:** Canonical conceptual vocabulary and modeling constraints for the game world.

The model remains provisional where sections explicitly say so. Review findings below distinguish implemented prototype behavior from concepts that are not yet represented in code. This is not a finalized database schema or a mandate to build generalized systems.

## World Model Foundation

**Project:** RPG — Adventurer Academy / Fantasy Life Simulation  
**Engine:** Godot 4.7.2 .NET / C#  
**Status:** Working proposal — not an implementation specification  
**Branch:** `docs/world-model-foundation`

> This document records the current modeling direction so the world and gameplay can be designed incrementally. Names, relationships and examples below are provisional until reviewed. Do not treat this ERD as a finalized database schema or assume these systems already exist in code.

### 1. Design Goal

Model a reusable game world that can support academy life, towns, wilderness, ruins, dungeons, varied characters, creatures, activities and encounters.

The architecture must not be organized around one location type (such as Dungeon), one enemy (such as Goblin), or one gameplay loop. Prefer a small number of reusable concepts with specialized rules where they are genuinely needed.

The immediate task is **domain modeling**, not adding systems or writing code. We will validate each concept against concrete gameplay scenarios before committing to implementation.

### 2. Normalize the Vocabulary

Do not place these terms in one flat list. They describe different levels of the game.

| Concept | Meaning | Example |
|---|---|---|
| Gameplay Domain | Broad area of player experience; a classification, not automatically a system | Daily Life, Study, Social, Exploration, Combat, Economy, Investigation |
| Activity | A goal-oriented piece of work that may consist of multiple actions | Eat breakfast, Attend class, Train swordsmanship, Investigate a disappearance |
| Action | A concrete operation selected or performed by an actor | Eat, Drink, Sleep, Read, Walk, Attack, Dodge, Talk |
| Effect | A request/operation asking a responsible System to make a domain change | Restore 20 health, consume an item, reveal a clue, change a relationship |
| State | The current condition of an entity or the world | Health, hunger, inventory quantity, NPC disposition, door unlocked |

A Domain is a taxonomy. An Activity is a useful grouping or player-facing task. An Action is an executable operation. An Effect requests a domain change. State stores the current condition after processing.

Not every Activity needs its own class or runtime object. Not every Action belongs to exactly one Domain. For example, Eat can be used at the academy, in a town, or while camping in the wilderness.

#### Example: daily life

“Daily Life” is not necessarily a separate system. It is a domain containing activities such as eating, sleeping, personal care and relaxation.

An activity such as **Eat breakfast** may involve selecting food, consuming it and applying effects. The same Eat action can be reused in other contexts. The location or context may change available options and consequences, but should not duplicate the action definition without a real rules difference.

### 3. Initial Gameplay Inventory

This inventory is a coverage checklist, not a commitment to implement every item.

| Domain | Activities | Example Actions |
|---|---|---|
| Daily Life | Meals, hydration, sleep, rest, personal care, self-treatment | Eat, Drink, Cook, Sleep, Rest, Relax, Bathe, Wash, Groom, Take Medicine |
| Study & Growth | Classes, self-study, research, practical training | Attend Class, Listen, Take Notes, Read, Study, Research, Practice, Train, Spar |
| Social | Conversation, relationship building, helping, invitations, recruitment | Talk, Ask, Listen, Give Gift, Invite, Help, Recruit, Negotiate |
| Exploration | Travel, observation, searching, tracking, gathering, field survival | Walk, Run, Travel, Climb, Inspect, Search, Track, Gather, Harvest, Mine, Camp, Light Fire |
| Combat | Offensive, defensive, tactical and support operations | Attack, Cast Spell, Defend, Dodge, Parry, Use Item, Heal, Buff |
| Economy | Trading, crafting, maintenance, property management | Buy, Sell, Trade, Craft, Cook, Repair, Equip, Store |
| Investigation | Questioning, evidence collection, analysis and resolution | Question, Examine, Collect Evidence, Compare Clues, Deduce, Report, Accuse, Reveal |

Some words can appear in multiple domains because domains classify player experience rather than own unique actions. For example, Cook may be part of daily life or crafting; its context and rules determine which use applies.

### 4. Object Inventory and Terminology

#### 4.1 Characters and roles

| Concept | Proposed meaning |
|---|---|
| Character | A living individual with attributes and state |
| Player | The character currently controlled by the user |
| NPC | A character controlled by game logic |
| Enemy | A character participating as a hostile opponent in a context |
| Companion | A character currently accompanying or assisting the player |
| Creature | A non-human living entity |

**Current decision:** Do not introduce a general-purpose `Actor` base concept yet. Model living participants as `Character`. Treat Player, NPC, Enemy and Companion as roles or contextual relationships rather than automatically creating separate entity types. A character might be an NPC in one context, an enemy in another, and later become a companion.

Non-living world objects are modeled separately where needed. An object such as a door, chest, trap or altar may expose interaction rules, but it does not need to inherit from Character or Actor. Use an Interactable capability only where an object genuinely supports player interaction.

#### 4.2 World and places

| Concept | Proposed meaning |
|---|---|
| World | The overall simulation context |
| Location | A place in the world where characters and objects can be present; locations may contain smaller locations |
| Location Presence | The fact that a character or object is currently present at a location |
| Interaction Context | The object or entity the player is currently interacting with, without necessarily changing location |

**Current direction:** Do not introduce Area as a required core entity. Represent places with a Location hierarchy using an optional `parent_location_id`. For example, Haunted House can contain Main Hall, Bedroom Floor 2 and Basement; Basement can contain Storage Room. The player's current Location remains the specific place they occupy.

Containment and navigation are different ideas. A child Location is part of its parent, but that does not by itself define every possible route. For the current text-based prototype, movement can be represented by an Action whose Target identifies the destination and whose Effect changes the current Location. A separate Location Connection model is not required yet; reconsider it only if routes need independent querying, shared conditions or map/path functionality.

An object such as a Mirror or Bed is not a Location. Interacting with it can change the Interaction Context while the player remains in the same Location.

#### 4.3 Objects and possessions

| Concept | Proposed meaning |
|---|---|
| Interactable | An object or entity that can expose interactions |
| Item | A defined object that can be owned, consumed, equipped, traded or used |
| Equipment | An item that can occupy an equipment slot and provide rules or effects |
| Resource | A material or collectible quantity used by other activities |
| Inventory | A container/ownership collection |
| Container | A world object that can hold items, such as a chest or storage box |

Equipment and Resource may be item categories or capabilities rather than independent inheritance branches. Decide after examining their shared fields and behavior.

#### 4.4 Situations, information and progression

| Concept | Proposed meaning |
|---|---|
| Encounter | A situation requiring the player or participants to respond |
| Combat | A specialized encounter with turn, targeting and combat-resolution rules |
| Dialogue | A conversation interaction/flow; not automatically an Encounter |
| Event | Something that happened in the simulation and may be recorded or trigger reactions |
| Knowledge | Information known by an actor or available to the player |
| Clue | A specific piece of evidence or information |
| Quest | A goal or chain of objectives |
| Relationship | A directed or mutual social state between actors |
| World State | Shared facts about the world, locations, factions or other entities |

Encounter and Event are not synonyms. A trap activation may be an event; being caught in a trap that requires a response may create an encounter. A conversation can be an ordinary interaction or a more structured situation depending on its rules.

### 5. Effects, State and Events

These concepts are related but are not interchangeable:

| Concept | Responsibility | Example |
|---|---|---|
| Effect | Declares a requested domain operation/change; it is not proof that the change succeeded | `RestoreHealth(20)`, `UnlockAction("InspectAncientDoor")` |
| State | Stores the current truth about an entity, relationship or world fact | `Player.Health = 80`, `WorldState.AncientDoorUnlocked = true` |
| Event | Records or announces a fact that has occurred; only create one when something needs to react to or remember it | `PlayerHealed(20)`, `AncientDoorUnlocked` |

The normal processing flow is:

```text
Action
  └── Effects (zero or more requested operations)
        └── Responsible Systems validate and process each Effect
              ├── Apply successful changes to the owning State
              └── Publish Events when useful or required
```

An Effect can be rejected or produce no State change. For example, `RestoreHealth(20)` may be valid but have no practical change when the character is already at maximum health. Conversely, an informational Action such as reading a letter may have no Effect at all. An Event is also optional: resting may restore Fatigue and Combat Stamina without needing a `PlayerRested` event unless another feature needs to react to it.

State remains the source of truth for current conditions and Action availability. Events may trigger notifications, quest reactions or other follow-up processing, but UI availability should be derived from current State and applicable rules rather than treating an Event as the current truth.

Examples:

- Eating may request item consumption, reduce Hunger and advance time.
- Sleeping may request time advancement and recovery-related character state changes.
- Training may request skill progression and fatigue changes.
- Defeating a creature may request a life-state change, grant rewards, alter a local threat and emit relevant events.
- Discovering a clue may request adding Knowledge and updating an Investigation or Quest objective.

Do not create a separate state system for every activity. First identify the owner of each state:

- Character state: health, Fatigue, Combat Stamina, mood, other needs, attributes and skills.
- Location state: discovered, blocked, damaged, resources remaining, local conditions.
- Item state: quantity, durability, ownership, equipped status.
- Relationship state: trust, affinity, hostility or other agreed dimensions.
- World state: persistent facts shared across locations and systems.
- Encounter state: participants, phase, outcome and encounter-specific variables.

These are candidate categories. Exact state fields and whether some belong together remain open.

### 6. Conceptual ERD (Provisional)

This diagram illustrates candidate entities and relationships only. It is not a normalized physical database schema. In particular, polymorphic references, state storage, action targets and effect payloads need further design before implementation.

```mermaid
erDiagram
    WORLD ||--o{ LOCATION : contains
    LOCATION o|--o{ LOCATION : contains
    LOCATION ||--o{ LOCATION_PRESENCE : records

    CHARACTER ||--o{ LOCATION_PRESENCE : appears_at
    CHARACTER ||--o{ ACTION_EXECUTION : performs
    ACTION_DEFINITION ||--o{ ACTION_EXECUTION : defines
    ACTION_EXECUTION o|--o| ACTION_TARGET : may_target
    ACTION_EXECUTION o|--o| ACTION_SOURCE : may_use
    ACTION_EXECUTION ||--o{ ACTION_EFFECT : produces

    CHARACTER ||--o{ INVENTORY : owns
    INVENTORY ||--o{ INVENTORY_ITEM : contains
    ITEM_DEFINITION ||--o{ INVENTORY_ITEM : identifies

    CHARACTER ||--o{ RELATIONSHIP : participant
    CHARACTER ||--o{ RELATIONSHIP : counterpart

    LOCATION ||--o{ ENCOUNTER : hosts
    ENCOUNTER ||--o{ ENCOUNTER_PARTICIPANT : includes
    CHARACTER ||--o{ ENCOUNTER_PARTICIPANT : participates

    CHARACTER ||--o{ KNOWLEDGE : holds
    KNOWLEDGE ||--o{ CLUE : includes

    QUEST ||--o{ QUEST_OBJECTIVE : defines
    CHARACTER ||--o{ QUEST_PROGRESS : tracks
    QUEST ||--o{ QUEST_PROGRESS : progresses

    WORLD ||--o{ WORLD_EVENT : records
    WORLD ||--o{ WORLD_STATE : maintains

    WORLD {
        string id PK
    }
    LOCATION {
        string id PK
        string world_id FK
        string parent_location_id FK
        string name
    }
    LOCATION_PRESENCE {
        string id PK
        string location_id FK
        string entity_ref
        string entity_kind
    }
    CHARACTER {
        string id PK
        string name
        string role
    }
    ACTION_DEFINITION {
        string id PK
        string owner_kind
        string owner_ref
        string name
    }
    ACTION_EXECUTION {
        string id PK
        string action_id FK
        string performer_id FK
        string target_ref
        string source_ref
    }
    ACTION_TARGET {
        string id PK
        string target_kind
        string target_ref
    }
    ACTION_SOURCE {
        string id PK
        string source_kind
        string source_ref
    }
    ACTION_EFFECT {
        string id PK
        string execution_id FK
        string effect_type
    }
    ITEM_DEFINITION {
        string id PK
        string name
        string category
    }
    INVENTORY {
        string id PK
        string owner_character_id FK
    }
    INVENTORY_ITEM {
        string inventory_id FK
        string item_id FK
        int quantity
    }
    RELATIONSHIP {
        string character_a_id FK
        string character_b_id FK
        string relationship_type
    }
    ENCOUNTER {
        string id PK
        string location_id FK
        string encounter_type
        string status
    }
    ENCOUNTER_PARTICIPANT {
        string encounter_id FK
        string character_id FK
    }
    KNOWLEDGE {
        string id PK
        string character_id FK
        string subject_ref
    }
    CLUE {
        string id PK
        string knowledge_id FK
    }
    QUEST {
        string id PK
        string name
    }
    QUEST_OBJECTIVE {
        string id PK
        string quest_id FK
    }
    QUEST_PROGRESS {
        string character_id FK
        string quest_id FK
        string status
    }
    WORLD_STATE {
        string id PK
        string world_id FK
        string state_key
    }
    WORLD_EVENT {
        string id PK
        string world_id FK
        string event_type
    }
```

#### ERD caveats to resolve

1. `LOCATION_PRESENCE`: characters and non-living objects may need different typed relations; avoid an unvalidated generic entity reference in the final schema.
2. `ACTION_DEFINITION.owner_kind / owner_ref`: the provider of an Action is distinct from its Performer. A Location, Object or Character may provide an Action; the Player may still be the Performer.
3. `ACTION_EXECUTION`: Performer is required; Target and Source are optional and may be null. Target and Source can refer to different kinds of domain objects.
4. `ACTION_EFFECT`: an execution may produce zero or more effects. Effects should be structured domain operations, not arbitrary executable code in content data.
5. `WORLD_STATE`: determine which facts belong to World, Location, Character or a relationship before selecting storage.
6. `KNOWLEDGE` / `CLUE`: distinguish raw evidence, discovered facts, and what the player-character actually knows.
7. `ENCOUNTER`: specialized encounter types may own their own state and rules; do not force all behavior into one generic table.

### 7. Modeling Rules

- A gameplay category does not automatically imply a software System.
- An Activity is not necessarily an executable Action.
- Reuse Actions across domains and locations when their rules are the same.
- A role such as Player, NPC, Enemy or Companion should not automatically become a separate entity type.
- Do not introduce a general Actor abstraction until concrete shared behavior requires it; use Character for living participants and model interactable objects separately.
- An Action is provided/owned by a Character, Location or Object; its provider is not necessarily its Performer.
- Action Execution records a required Performer, an optional Target, an optional Source and zero or more Effects. A null Target or Source is a valid case, not an execution error.
- Effects declare requested domain changes. The responsible System validates and applies the Effect; Action execution coordinates the process rather than embedding every System's rules.
- For the current prototype, use a shared Action execution pipeline: Request → Handle → Orchestrator → Guard/Validator → Effects → responsible Systems → Apply → updated State.
- Request is a distinct responsibility in the user's 13-responsibility code organization: it creates/packages/sends an instruction to the application boundary. Handle receives and routes it. Request may involve a request data object and a sender; it is not necessarily one function.
- Validate required conditions before applying changes. Multi-Effect atomicity, rollback and partial-failure handling remain implementation questions.
- Combat remains specialized; do not flatten its turn and resolution rules into generic Action processing.
- Effects are requests for domain changes; responsible Systems validate and process them, then Apply updates the owning State. Events record or announce facts that occurred when other parts of the game need to react or remember them. Effects, State changes and Events are related but none is mandatory for every Action.
- JSON/content data describes definitions and configuration. C# owns executable rules and validation.
- Do not put arbitrary code or formulas in JSON.
- Add a dedicated System only when there is a distinct responsibility and rules that justify it.
- Validate the model against multiple locations, actor types and gameplay situations before implementation.

### 8. Incremental Modeling Plan

We will review and revise one area at a time. Each step should produce a decision record, updated diagrams and a list of unresolved questions.

| Step | Scope | Validation scenario | Exit condition |
|---|---|---|---|
| 1 | Character and roles | Player, student NPC, hostile creature, recruitable companion | Character and contextual roles are clear; Actor abstraction is justified or omitted |
| 2 | World, Location hierarchy and interaction context | Academy, town, forest, multi-room haunted house, mirror and bed interactions | Containment, current location and interaction context are distinct |
| 3 | Activity, Action ownership and Action Execution | Eat breakfast, sleep, use potion, self-buff, give gift, travel, inspect mirror | Provider, Performer, optional Target/Source and zero-or-more Effects are clear |
| 4 | State, Effect and Event | Eat, train, defeat enemy, reveal clue, change location, start interaction | State ownership, Effect dispatch and mutation flow are unambiguous |
| 5 | Encounter | Combat, trap, chase, negotiation, discovery | Shared lifecycle is identified without erasing specialized rules |
| 6 | Items and inventory | Food, potion, weapon, quest evidence, raw material | Ownership, use, equipment and consumption are clear |
| 7 | Relationships and knowledge | Recruit NPC, learn monster weakness, gather evidence | Information and social state are modeled independently |
| 8 | Quest and progression | Multi-step investigation and academy assignment | Objectives can react to events and state changes |

#### Working method

For each step:

1. List real gameplay examples and edge cases.
2. Identify entities, attributes, ownership and relationships.
3. Mark concepts as **accepted**, **proposed**, or **unresolved**.
4. Update the ERD only when the relationships are understood.
5. Check whether the existing code already covers the concept.
6. Decide whether code changes are needed; do not implement speculatively.

### 9. Current Decisions vs. Open Questions

#### Current direction

- The game supports multiple kinds of places and activities, not only dungeons and combat.
- Location-based navigation is the current prototype direction; free-roam movement is not required.
- Location is hierarchical; Area is not currently part of the core model.
- Action providers can be Locations, Objects or Characters; the Performer is the entity that actually executes the Action.
- Actions are concrete operations; Effects declare requested changes and responsible Systems resolve and apply them.
- Combat has specialized rules and presentation.
- Content definitions and executable rules remain separated.

#### Open questions

- Which non-living objects need interaction capabilities, and how should those be represented without a universal Actor base?
- Are Activity and Action both runtime concepts, or is Activity only a grouping/presentation concept?
- Which character needs are simulated: Hunger, Thirst, Fatigue, Mood, Health or others?
- Does the player need to track both a current Location and a current Interaction Context, and what transitions clear or restore that context?
- How are interactable non-living objects represented without turning every object into a Location?
- What is the boundary between Encounter, Dialogue and Event?
- How is world state represented without creating one universal untyped state bag?
- How should source/target references be represented when supporting polymorphic NPC and Interactable definitions beyond the initial Version 1 request contract?
- What consistency/atomicity guarantees are required when an Action produces multiple Effects and one fails?

### 10. Implementation Status

This document is a design proposal. It does not claim that the listed entities, relationships, gameplay domains or systems are implemented.

The current prototype has its Academy/Goblin vertical slice and uses Health, Fatigue, Combat Stamina, Mood and Gold. Fatigue and Combat Stamina follow the initial rules in [Version 1 Plan](Version1Plan.md#session-2) and [Testing](Testing.md). The broader world model, additional needs and persistent NPC/object state remain design proposals until separately scoped, implemented and tested.


### 11. Exploration Action Sources and Choice Presentation

The following direction is accepted for the Version 1 Exploration pipeline and should be applied incrementally. It does not mean NPC, Item, Interactable or Puzzle systems already exist.

#### Action source vs. performer vs. target

An Action is performed by a Character, but its definition may be supplied by a different source. A Player Action is supplied by the Player's character data; a Location Action is supplied by the current Location; an NPC or object may later supply interactions. A target identifies the specific entity the operation acts upon. Do not treat these identities as interchangeable.

A concrete choice request should preserve enough identity to route unambiguously:
- Source kind and source ID.
- Action ID within that source.
- Optional target/destination ID.

Action IDs are not assumed globally unique. Two different chests may both define an `open` action without sharing the same choice identity.

#### Loader, Query, Mapper and View

The data-to-UI boundary is:

```text
JSON Loader -> immutable definitions
Definitions + current Runtime State / Context
    -> Available Interaction Query
    -> Action Choice Mapper
    -> grouped Exploration ViewModel
    -> StoryPanel
```

- Loaders parse and validate content. They do not decide which choices appear now.
- Query resolves contextual availability from definitions and current state. It is read-only.
- Mapper converts query results into presentation-safe grouped data. It does not read JSON or mutate domain state.
- ViewModel describes what the UI needs to display.
- View renders supplied groups and emits a selection request. It does not own gameplay rules.

#### Version 1 choice sources

Version 1 implements Player Actions, Location Actions, Travel choices derived from Location exits, and Event Choices. Their intended presentation groups are Personal, At This Location, Travel and Current Situation. The active Event remains a focused context; ordinary Location and Travel choices do not leak into an unresolved Event screen.

#### Future extension points

Present NPCs, Items and Interactable Objects may later provide choices in a Nearby group. Containers can expose Inspect/Open/Search/Take actions; doors and switches can expose their own interactions; Puzzles can expose Examine/Activate/Solve choices. Each feature still needs its own data definitions, runtime state and responsible rule owner. The generic UI pipeline only discovers, groups and renders those choices; it does not implement their gameplay behavior.

Combat remains a specialized pipeline. Skills represent learned capabilities; Combat Actions are operations available during combat and may be unlocked or modified by Skills. Do not mix Combat Actions into the Exploration Player Actions list.

See [Exploration Action Choice Pipeline](Gameplay.md) for the implementation contract.

## Review Findings: NPC, Object and Location

The design review in the archived [World Model Review](Archive/WorldModelReview.md) validated the provisional foundation against three concrete scenarios. Its conclusions are summarized here; the archived report retains the full scenario analysis.

### Current prototype boundaries

- **NPC interaction:** Talking to Mira is currently a Location Action. Mira is not yet represented as a Character record with Location Presence or an explicit target reference.
- **Object interaction:** Reading a letter or inspecting a notice board is currently a simple Location Action. The prototype has no separate Object definition or Object Presence record for these props.
- **Location activity and travel:** Location owns its Exits, Actions and Events. ActionExecutionService handles configured action costs, time and effects; LocationEventService evaluates destination events after successful travel. Travel still uses generated choice IDs and controller routing rather than a general typed target/effect contract.
- **Interaction Context:** The current one-step actions do not need a persistent selected-object/NPC context. A separate context becomes justified when a multi-step interaction keeps the player inside a focused object or NPC view.

### Accepted modeling decisions

- Keep the current simple Location Actions for immediate, stateless interactions.
- Model an NPC as a Character with contextual Location Presence when an actual NPC feature requires it; do not create a separate NPC class hierarchy for one interaction.
- Introduce an Object definition only when the object needs independent identity, presence, state, ownership or reusable interactions.
- Do not introduce a universal Interactable system, general Actor hierarchy, Location Connection model or typed travel target before a concrete gameplay requirement needs it.
- Keep Activity as a useful grouping and Action as the concrete operation; Event is not automatically an Encounter or Combat.

### Conclusion and unresolved boundaries

The review found no contradiction that requires redesigning the current foundation. The model remains provisional, and general NPC/Object presence, persistent interaction context, typed travel targets and saveable world/object state remain future requirements rather than implemented capabilities. Add them incrementally when a playable scenario requires them.
