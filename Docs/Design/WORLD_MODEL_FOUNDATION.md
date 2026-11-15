# World Model Foundation — Conceptual Design

**Project:** RPG — Adventurer Academy / Fantasy Life Simulation  
**Engine:** Godot 4.7.2 .NET / C#  
**Status:** Working proposal — not an implementation specification  
**Branch:** `docs/world-model-foundation`

> This document records the current modeling direction so the world and gameplay can be designed incrementally. Names, relationships and examples below are provisional until reviewed. Do not treat this ERD as a finalized database schema or assume these systems already exist in code.

## 1. Design Goal

Model a reusable game world that can support academy life, towns, wilderness, ruins, dungeons, varied characters, creatures, activities and encounters.

The architecture must not be organized around one location type (such as Dungeon), one enemy (such as Goblin), or one gameplay loop. Prefer a small number of reusable concepts with specialized rules where they are genuinely needed.

The immediate task is **domain modeling**, not adding systems or writing code. We will validate each concept against concrete gameplay scenarios before committing to implementation.

## 2. Normalize the Vocabulary

Do not place these terms in one flat list. They describe different levels of the game.

| Concept | Meaning | Example |
|---|---|---|
| Gameplay Domain | Broad area of player experience; a classification, not automatically a system | Daily Life, Study, Social, Exploration, Combat, Economy, Investigation |
| Activity | A goal-oriented piece of work that may consist of multiple actions | Eat breakfast, Attend class, Train swordsmanship, Investigate a disappearance |
| Action | A concrete operation selected or performed by an actor | Eat, Drink, Sleep, Read, Walk, Attack, Dodge, Talk |
| Effect | A result that changes something | Restore health, consume an item, reveal a clue, change relationship |
| State | The current condition of an entity or the world | Health, hunger, inventory quantity, NPC disposition, door unlocked |

A Domain is a taxonomy. An Activity is a useful grouping or player-facing task. An Action is an executable operation. An Effect describes a change. State stores the resulting condition.

Not every Activity needs its own class or runtime object. Not every Action belongs to exactly one Domain. For example, Eat can be used at the academy, in a town, or while camping in the wilderness.

### Example: daily life

“Daily Life” is not necessarily a separate system. It is a domain containing activities such as eating, sleeping, personal care and relaxation.

An activity such as **Eat breakfast** may involve selecting food, consuming it and applying effects. The same Eat action can be reused in other contexts. The location or context may change available options and consequences, but should not duplicate the action definition without a real rules difference.

## 3. Initial Gameplay Inventory

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

## 4. Object Inventory and Terminology

### 4.1 Actors and characters

| Concept | Proposed meaning | Modeling question |
|---|---|---|
| Actor | An entity capable of participating in gameplay actions or situations | Is Actor a runtime abstraction, and what capabilities does it require? |
| Character | A living individual with character attributes and state | Is Character a specialized Actor? |
| Player | The character currently controlled by the user | Is this a control role rather than a separate entity type? |
| NPC | A character controlled by game logic | How are schedules, needs and behavior represented? |
| Enemy | An actor participating as a hostile opponent in a context | Is hostility contextual rather than a permanent class? |
| Companion | A character currently accompanying or assisting the player | Is companion status a relationship/role with availability and rules? |
| Creature | A non-human living entity | Which creature capabilities differ from humanoid characters? |

Initial direction: Player, NPC, Enemy and Companion should be examined as roles or relationships before creating separate entity hierarchies. A character might be an NPC in one context, an enemy in another, and later become a companion.

Not every interactable thing is an Actor. A door, chest, trap or altar may be an object with interaction rules, not a living participant.

### 4.2 World and places

| Concept | Proposed meaning |
|---|---|
| World | The overall simulation context |
| Area | A broad region containing multiple locations, e.g. Academy Grounds, Town District, Forest |
| Location | A place where actors and interactable objects can be present |
| Location Connection | A traversable relationship between two locations, possibly conditional |
| Location Presence | The fact that an actor or object is currently present at a location |

A Dungeon is a possible kind of Area or a structured set of Locations, not the universal model for exploration. A Forest, Town, Ruins and Academy should use the same foundational place model where their rules overlap.

### 4.3 Objects and possessions

| Concept | Proposed meaning |
|---|---|
| Interactable | An object or entity that can expose interactions |
| Item | A defined object that can be owned, consumed, equipped, traded or used |
| Equipment | An item that can occupy an equipment slot and provide rules or effects |
| Resource | A material or collectible quantity used by other activities |
| Inventory | A container/ownership collection |
| Container | A world object that can hold items, such as a chest or storage box |

Equipment and Resource may be item categories or capabilities rather than independent inheritance branches. Decide after examining their shared fields and behavior.

### 4.4 Situations, information and progression

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

## 5. Effects and State Ownership

Effects describe changes; state belongs to the entity or relationship being changed.

Examples:

- Eating may consume an Item, reduce Hunger and advance time.
- Sleeping may advance time and change recovery-related character state.
- Training may change skill progression and fatigue.
- Defeating a creature may change its life state, grant rewards, alter a local threat and emit an event.
- Discovering a clue may add Knowledge and update an Investigation or Quest objective.

Do not create a separate state system for every activity. First identify the owner of each state:

- Character state: health, fatigue/energy, mood, needs, attributes, skills.
- Location state: discovered, blocked, damaged, resources remaining, local conditions.
- Item state: quantity, durability, ownership, equipped status.
- Relationship state: trust, affinity, hostility or other agreed dimensions.
- World state: persistent facts shared across locations and systems.
- Encounter state: participants, phase, outcome and encounter-specific variables.

These are candidate categories. Exact state fields and whether some belong together remain open.

## 6. Conceptual ERD (Provisional)

This diagram illustrates candidate entities and relationships only. It is not a normalized physical database schema. In particular, polymorphic references, state storage, action targets and effect payloads need further design before implementation.

```mermaid
erDiagram
    WORLD ||--o{ AREA : contains
    AREA ||--o{ LOCATION : contains
    LOCATION ||--o{ LOCATION_CONNECTION : has
    LOCATION ||--o{ LOCATION_PRESENCE : records

    ACTOR ||--o| CHARACTER : may_have
    ACTOR ||--o{ ACTOR_ROLE : has
    ACTOR ||--o{ LOCATION_PRESENCE : appears_at

    ACTOR ||--o{ ACTION_EXECUTION : performs
    ACTION_DEFINITION ||--o{ ACTION_EXECUTION : defines
    ACTION_EXECUTION ||--o{ ACTION_EFFECT : produces

    ACTOR ||--o{ INVENTORY : owns
    INVENTORY ||--o{ INVENTORY_ITEM : contains
    ITEM_DEFINITION ||--o{ INVENTORY_ITEM : identifies

    ACTOR ||--o{ RELATIONSHIP : participant
    ACTOR ||--o{ RELATIONSHIP : counterpart

    LOCATION ||--o{ ENCOUNTER : hosts
    ENCOUNTER ||--o{ ENCOUNTER_PARTICIPANT : includes
    ACTOR ||--o{ ENCOUNTER_PARTICIPANT : participates

    ACTOR ||--o{ KNOWLEDGE : holds
    KNOWLEDGE ||--o{ CLUE : includes

    QUEST ||--o{ QUEST_OBJECTIVE : defines
    ACTOR ||--o{ QUEST_PROGRESS : tracks
    QUEST ||--o{ QUEST_PROGRESS : progresses

    WORLD ||--o{ WORLD_EVENT : records
    WORLD ||--o{ WORLD_STATE : maintains

    WORLD {
        string id PK
    }
    AREA {
        string id PK
        string world_id FK
        string name
    }
    LOCATION {
        string id PK
        string area_id FK
        string name
    }
    LOCATION_CONNECTION {
        string id PK
        string source_location_id FK
        string destination_location_id FK
    }
    LOCATION_PRESENCE {
        string id PK
        string location_id FK
        string entity_id
        string entity_kind
    }
    ACTOR {
        string id PK
        string name
    }
    CHARACTER {
        string actor_id PK,FK
    }
    ACTOR_ROLE {
        string actor_id FK
        string role
    }
    ACTION_DEFINITION {
        string id PK
        string name
    }
    ACTION_EXECUTION {
        string id PK
        string action_id FK
        string actor_id FK
        string target_ref
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
        string owner_actor_id FK
    }
    INVENTORY_ITEM {
        string inventory_id FK
        string item_id FK
        int quantity
    }
    RELATIONSHIP {
        string actor_a_id FK
        string actor_b_id FK
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
        string actor_id FK
    }
    KNOWLEDGE {
        string id PK
        string actor_id FK
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
        string actor_id FK
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

### ERD caveats to resolve

1. `ACTOR` / `CHARACTER`: decide whether Actor is a true supertype or whether Character itself is sufficient for the first playable scope.
2. `LOCATION_PRESENCE`: actors and objects may need different typed relations; avoid an unvalidated generic entity reference in the final schema.
3. `ACTION_EXECUTION.target_ref`: targets can be actors, objects, locations or no target. Define a safe typed target model.
4. `ACTION_EFFECT`: effects may be structured domain operations, not arbitrary string/value pairs or executable code in content data.
5. `WORLD_STATE`: determine which facts belong to World, Location, Actor or a relationship before selecting storage.
6. `KNOWLEDGE` / `CLUE`: distinguish raw evidence, discovered facts, and what the player-character actually knows.
7. `ENCOUNTER`: specialized encounter types may own their own state and rules; do not force all behavior into one generic table.

## 7. Modeling Rules

- A gameplay category does not automatically imply a software System.
- An Activity is not necessarily an executable Action.
- Reuse Actions across domains and locations when their rules are the same.
- A role such as Enemy or Companion should not automatically become a separate entity type.
- Combat remains specialized; do not flatten its turn and resolution rules into generic Action processing.
- Effects change state; Events record or announce what happened.
- JSON/content data describes definitions and configuration. C# owns executable rules and validation.
- Do not put arbitrary code or formulas in JSON.
- Add a dedicated System only when there is a distinct responsibility and rules that justify it.
- Validate the model against multiple locations, actor types and gameplay situations before implementation.

## 8. Incremental Modeling Plan

We will review and revise one area at a time. Each step should produce a decision record, updated diagrams and a list of unresolved questions.

| Step | Scope | Validation scenario | Exit condition |
|---|---|---|---|
| 1 | Actor, Character and roles | Player, student NPC, hostile creature, recruitable companion | Clear identity, capability and role boundaries |
| 2 | World, Area, Location and connections | Academy, town, forest, ruins and a multi-room dungeon | Places can be composed without location-specific systems |
| 3 | Activity and Action | Eat breakfast, attend class, travel, attack, investigate | No category/action duplication; action execution has clear inputs |
| 4 | State, Effect and Event | Eat, train, defeat enemy, reveal clue | Ownership and mutation flow are unambiguous |
| 5 | Encounter | Combat, trap, chase, negotiation, discovery | Shared lifecycle is identified without erasing specialized rules |
| 6 | Items and inventory | Food, potion, weapon, quest evidence, raw material | Ownership, use, equipment and consumption are clear |
| 7 | Relationships and knowledge | Recruit NPC, learn monster weakness, gather evidence | Information and social state are modeled independently |
| 8 | Quest and progression | Multi-step investigation and academy assignment | Objectives can react to events and state changes |

### Working method

For each step:

1. List real gameplay examples and edge cases.
2. Identify entities, attributes, ownership and relationships.
3. Mark concepts as **accepted**, **proposed**, or **unresolved**.
4. Update the ERD only when the relationships are understood.
5. Check whether the existing code already covers the concept.
6. Decide whether code changes are needed; do not implement speculatively.

## 9. Current Decisions vs. Open Questions

### Current direction

- The game supports multiple kinds of places and activities, not only dungeons and combat.
- Location-based navigation is the current prototype direction; free-roam movement is not required.
- Actions are concrete operations; systems resolve their rules and consequences.
- Combat has specialized rules and presentation.
- Content definitions and executable rules remain separated.

### Open questions

- Is Actor necessary as a separate abstraction from Character?
- Are Activity and Action both runtime concepts, or is Activity only a grouping/presentation concept?
- Which character needs are simulated: Hunger, Thirst, Fatigue, Mood, Health or others?
- Are Area and Location both required at runtime?
- How are interactable non-living objects represented?
- What is the boundary between Encounter, Dialogue and Event?
- How is world state represented without creating one universal untyped state bag?
- How are actions targeted and how are effects typed and validated?

## 10. Implementation Status

This document is a design proposal. It does not claim that the listed entities, relationships, gameplay domains or systems are implemented.

The current prototype still has its existing Academy/Goblin vertical slice and Energy-based character model. The proposed broader world model and any future Fatigue/AP redesign must be treated as design work until separately implemented and tested.
