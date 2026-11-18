# UI Architecture

**Purpose:** Canonical presentation architecture and UI contracts for the main game interface and Combat panel.

The main interface section describes the current design direction. The Combat contract includes both integrated prototype behavior and explicitly unimplemented acceptance criteria; checked and unchecked items retain their original meaning.

## Main Game Interface

**Status:** Design direction with an integrated playable encounter slice  
**Related documents:** [Game Overview](GameDesign.md), [Exploration Action Choice Pipeline](Gameplay.md), [Combat Panel Contract](Ui.md)

### 1. Main Screen Composition

| Region | Responsibility |
|---|---|
| Left Sidebar | Navigate between major player-facing pages |
| Main Panel | Present current Location, story context or dedicated activity mode |
| Right Character Panel | Keep essential current Player state visible |

The Main Panel is the primary content area. It is not merely a text log.

### 2. Sidebar

| Group | Pages / content | Purpose |
|---|---|---|
| CHARACTER | Profile, Skills, Inventory, Relationships | Inspect and manage the character |
| DIARY | Current Goals, Journal Entries | Track objectives and recorded experiences |
| SYSTEM | Settings | Configure the game |

The Main Panel owns current Location exploration. A separate Explore sidebar page is not planned. A dedicated Map page remains undecided.

### 3. Main Panel Modes

#### 3.1 Exploration Mode

Exploration communicates:
- Current Location name, region and description.
- Relevant day/time context.
- Atmosphere and illustrative imagery where available.
- Available choices grouped by their presentation purpose.

The player does not need free-roam movement or pathfinding to interact with the world.

#### 3.2 Exploration choice groups

The long-term grouped presentation supports:

| Group | Source | Examples | Version 1 scope |
|---|---|---|---|
| Personal | Player definition | Rest, Sleep | Implement |
| Nearby | Present NPCs, Items and Interactable Objects | Talk to Mira, Inspect Chest | Future extension |
| At This Location | Current Location definition | Study Maps, Look Around | Implement |
| Travel | Current Location exits/connections | Go to Courtyard | Implement |
| Current Situation | Active Event | Confront Goblin, Sneak Past | Implement using focused Event context |

Groups with no choices are omitted. The UI renders the groups and ordering provided by its ViewModel. It does not decide which NPC is present, whether a chest is open or whether a Player meets an action requirement.

An active Event remains a focused situation screen. Version 1 preserves the current behavior: its Event Choices are shown in the Event context and ordinary Location/Travel choices are not mixed into that unresolved situation.

#### 3.3 Data-to-UI boundary

The intended Exploration pipeline is:

```text
JSON Loaders
  -> immutable content definitions
  -> runtime state + current context
  -> Available Interaction Query
  -> Action Choice Mapper
  -> ActionChoiceViewData list
  -> StoryPanel.ShowContent(header, time, choices)
```

Responsibilities:
- **Loader:** reads and validates its JSON source; creates data definitions.
- **Query:** selects choices that belong in the current context and determines advisory availability from current state. It does not mutate state.
- **Mapper:** converts query results into presentation-safe, grouped data. It does not read files or apply gameplay rules.
- **ActionChoiceViewData:** contains the player-facing label, duration label, group title and encoded ActionSelectionRequest in ChoiceId. The current prototype passes the screen header/time separately; it does not have an aggregate ExplorationViewModel.
- **StoryPanel:** renders groups and emits the selected choice identity. It does not execute actions or access domain objects.

The Loader is upstream from the Mapper, but the Mapper does not depend on or call the Loader. Query sits between loaded definitions/runtime state and the Mapper.

The UI's enabled/disabled state is advisory. The responsible gameplay handler/service validates every selected request again.

#### 3.4 Choice identity

A choice request must identify its source and action, and optionally its target/destination. Action IDs alone are not globally unique: two containers may both define `open`.

Conceptual request fields:
- Source kind.
- Source ID.
- Action ID.
- Optional target/destination ID.

Keep this contract as small as current features require. NPC/Object targets are future extensions, not Version 1 runtime requirements.

#### 3.5 Combat Mode

Combat uses a dedicated turn-based presentation, not the Exploration action groups.

It contains encounter context, combat status, observed enemy intent, event history and currently available Combat Actions.

The boundary remains:

```text
CombatService
  -> CombatResult + safe CombatSnapshot
  -> CombatPresentationMapper
  -> CombatViewData
  -> CombatPanel
```

CombatService owns validation, resolution and combat state mutation. UI availability is advisory and the service revalidates requests.

Detailed contracts are defined in [Combat Panel Contract](Ui.md).

#### 3.6 Other Activity Modes

Study, rest, social interaction and inventory management may later need focused views. Do not assume every activity should reuse the Combat message-log layout. Version 1 daily activities can use the Exploration choice/result flow.

### 4. Character Panel

The right-side Character Panel remains visible across Main Panel modes.

Initial persistent indicators:
- **Health:** survival; zero means death.
- **Fatigue:** daily-life exertion, accumulated from activity duration and intensity; it does not block actions in the initial version.
- **Combat Stamina:** short-term resource for defensive Combat Actions; zero does not cause unconsciousness or end Combat.
- **Mood:** emotional state.
- **Gold:** reward currency.

Detailed information belongs in relevant character pages. Keep the persistent panel minimal.

### 5. Information Ownership

- Character Panel: current persistent Player state.
- Combat Status: current encounter state.
- Combat Message Log: ordered history, not the source of truth for current HP.
- Exploration: current Location, context and grouped available choices.
- Diary: selected goals and meaningful records, not every transient UI message.

Avoid duplication unless repetition materially improves usability.

### 6. Mode Ownership and Transitions

MainController is the Godot composition root. It wires UI signals to the focused flow controllers and does not own feature-level transitions.

- ExplorationFlowController presents Locations, preserves active Event context and dispatches selected Exploration choices.
- CombatFlowController starts encounters, handles Combat requests and returns to Exploration.
- GameModeController switches the visible Main Panel mode.
- Character Panel remains persistent across both modes.

The flow controllers coordinate Presentation and call Application/Domain services. They must not duplicate action costs, mutate state outside the owning service or implement combat rules.

### 7. Prototype Reality Check

Currently implemented on `main`:
- Sidebar and persistent Character Panel.
- Exploration choices are discovered by `AvailableInteractionQuery` for Player, Location, Travel and Event sources.
- Rest and Sleep are Player Actions in `Data/Characters/Player.json`; Rest is available at every actionable Location and Sleep is restricted to Dormitory.
- `ActionChoiceMapper` converts available interactions into grouped `ActionChoiceViewData`; StoryPanel renders those groups.
- `ActionSelectionRequest` carries source kind, source ID, action ID and optional target ID. ExplorationFlowController validates the current source/context before dispatch.
- Combat uses a dedicated CombatService → CombatPresentationMapper → CombatViewData → CombatPanel pipeline.

Current transitional boundaries:
- MainController is the Godot composition root. ExplorationFlowController and CombatFlowController own the Exploration and Combat presentation flows and delegate gameplay rules to Application/Domain services.
- JSON loading is owned by Infrastructure; character, action and location definitions are separated into Domain files.
- Godot runtime verification must be recorded separately from .NET CI; a passing test suite does not prove scene/resource integration.

Not implemented:
- NPC population and presence, Inventory/item use, generic Interactable Objects, Containers, Puzzles and Nearby choice group.
- Save/load and broader life simulation.
- Skill progression and Combat Action unlocks.
- Full narrow-screen and assistive-technology verification.

### 8. UI Review Criteria

1. Can the Player identify current context and available choices?
2. Are groups meaningful and free of empty sections?
3. Does UI receive presentation-safe ViewModels instead of reading domain objects or JSON?
4. Are current state, history, observations and inferences distinguishable?
5. Do disabled choices explain why they are unavailable?
6. Does the owning service revalidate every request?
7. Does each activity receive a presentation suited to its decisions?


## Combat Panel Contract

**Status:** Integrated playable Goblin encounter slice; broader combat system remains prototype-level  
**Engine:** Godot 4.7.2 .NET / C#  
**Scope:** Combat presentation and its integration contract. The current preview does not implement combat rules.

### 1. Purpose

Combat uses a dedicated presentation mode. It must not reuse the Exploration panel with different text.

Exploration communicates a place and its available interactions. Combat communicates the current encounter, completed actions, outcomes, observed threats and currently available choices.

### 2. Screen Structure

| Area | Responsibility | Update behavior |
|---|---|---|
| Encounter Header | Encounter name and context | Encounter start or context change |
| Combat Status | Round, active turn, enemy HP / condition and current observed intent | Replace from latest view data |
| Message Log | Ordered history of actions, reactions, results and observations | Append new messages |
| Action Selection | Current actions, target, cost, description and availability | Replace the full action list |

The Character Panel remains outside Combat Panel and persistently displays Health, Fatigue, Stamina, Mood and Gold. It remains visible during Combat.

### 3. Architecture and Data Flow

The intended gameplay boundary is:

    Actor / UI
        |
        | CombatActionRequest
        v
    CombatService
        |-- Validate action and current turn
        |-- Resolve action using combat rules
        |-- Apply valid result to combat state
        |-- Return CombatResult with safe CombatSnapshot
        v
    CombatPresentationMapper
        |-- Convert public combat events into messages
        |-- Pass through presentation-safe status only
        |-- Build CombatViewData
        v
    CombatPanel
        |-- Replace current Status
        |-- Replace available Actions
        |-- Append new Messages
        v
    Status / Log / Action components

#### Ownership rules

- CombatService owns action validation, resolution and mutation of combat state. It does not reference Godot UI nodes.
- CombatResult reports success or failure, a failure reason when applicable, the resulting presentation-safe snapshot and events produced by the request.
- CombatSnapshot is a safe read model. It may contain current enemy HP, conditions, active turn, available actions and observed intent. It must not contain raw enemy AI state, planned action IDs, hidden targets or internal decision data.
- CombatPresentationMapper converts the result into CombatViewData and maps event types to message type, category, speaker and text.
- CombatPanel consumes CombatViewData. It does not calculate damage, spend resources, validate game rules or inspect enemy AI state.

The UI's enabled / disabled action state is advisory. CombatService.ExecuteAction must validate again because the state may have changed since the view was produced. If validation fails, the service returns a failure result and leaves combat state unchanged. If validation succeeds, it resolves the action, applies the result, then returns the updated safe snapshot and produced events.

#### Data contracts

| Type | Responsibility |
|---|---|
| CombatActionRequest | Action ID and optional target ID requested by the actor |
| CombatResult | Success / failure, failure reason, safe snapshot and produced events |
| CombatEvent | Completed gameplay event with type, round and relevant values |
| CombatSnapshot | Current presentation-safe encounter state |
| CombatViewData | Data the Combat Panel can render |
| CombatMessage | One historical, semantically typed message |
| IntentObservation | Information the character has actually observed or inferred |
| CombatActionOption | Player-facing action and its current availability |

ICombatService defines the application boundary. CombatService is the current concrete implementation for one deterministic Goblin encounter; it is intentionally not a generalized combat framework.

### 4. Message Contract

Every message has one of three semantic types. Alignment is a visual aid, not the only identity cue.

| Type | Alignment | Required visible identity |
|---|---|---|
| Player | Right | Speaker label such as YOU |
| Enemy | Left | Stable enemy label such as GOBLIN A |
| System | Center | SYSTEM label and category when applicable |

Each message contains a stable ID, Round, Type, Category, Speaker and Text.

Categories are None, Damage, Check, Turn, Environment, Inference, Emotion, StatusChange and Round.

Do not add a fourth message type for thoughts. Use a categorized System message. A Round separator is also a System message with the Round category.

Completed events use past tense. Intent and available-action descriptions may use present or future wording. Use stable target names consistently.

The current status is the source of truth for HP. The log records damage delta only; it must not repeat resulting current HP.

Example:
- Status: Goblin A — 40 / 60 HP.
- Log: SYSTEM · DAMAGE — Goblin A loses 20 HP.

### 5. Enemy Intent and Information Fairness

Enemy intent is an observation, not the enemy AI's internal next action.

- CombatService may know the planned action internally.
- The character may not know it.
- CombatSnapshot may expose only an IntentObservation produced by a valid observation / inference rule.
- IntentObservation contains enemy identity, player-facing description and confidence (Suspected or Recognized). It must not expose a hidden action ID or raw AI decision.

No observation means no intent card. Sending a snapshot with Intent = null clears the card.

#### Lifecycle

1. A valid observation creates or updates the intent.
2. A new observation replaces the previous observation for that enemy.
3. When the observed threat starts executing, is interrupted or otherwise becomes invalid, the next snapshot clears or replaces it.
4. A new encounter starts with no intent unless an observation has already occurred in that encounter.

Gameplay rules decide when an observation becomes invalid. The UI must not infer expiry from elapsed time or guess enemy behavior.

#### Information precedence

- Intent card: temporary, current actionable expectation.
- Message Log: historical record of what the character observed or what actually happened.
- Enemy action message: describes the visible action after it occurs.
- Inference message: explicitly describes the character's interpretation, not objective fact.

Do not repeat the same claim in all four places.

### 6. Action Availability

The latest snapshot replaces the entire available-action list; it is not appended to the previous list.

The service refreshes that list whenever the player turn begins or a relevant condition changes, including resources, target availability, status effects or encounter phase.

Each option contains an ID, label, optional target context, cost, description, availability and optional unavailable reason.

The UI disables unavailable actions and may display the reason. The service still validates every submitted request before applying it.

### 7. Status, Log and Encounter Lifecycle

- Status is current state; Log is historical sequence.
- Enemy HP is exact in this prototype.
- Log records damage deltas, not duplicate current HP.
- A new encounter clears the previous encounter log and intent.
- The log is bounded to 60 messages.
- Auto-scroll only when the reader was already at the bottom before a new message arrived.
- If the reader is inspecting older messages, preserve their scroll position.
- A round separator is a styled System message with category Round. Timestamps are not required for the prototype.

### 8. Accessibility and Layout

- Message identity remains visible in text labels, not only alignment or color.
- Action buttons are keyboard-focusable and operable.
- Unavailable actions expose a readable reason.
- Message bodies wrap when the window is resized.
- Maintain sufficient contrast between text, labels and message backgrounds.
- The preview is desktop-first. Exact narrow-screen stacking rules remain open and must be tested before mobile-sized layouts are claimed as supported.

### 9. Mode Transition

The Main Controller owns the transition between Exploration and Combat.

- Exploration → Combat: show the dedicated Combat Panel.
- During Combat: apply the latest view data.
- Combat → Exploration: return to the current location's Exploration presentation.
- Character Panel remains persistent.

The Main Controller currently implements this transition for the Goblin encounter. The standalone Combat Preview remains separate.

### 10. Prototype Reality Check

Implemented in the main playable slice:
- Dedicated Combat Panel and separate Status, Message Log and Action components.
- Typed messages with visible speaker and category labels.
- Presentation-safe CombatSnapshot, CombatResult, CombatViewData and mapper contracts.
- Intent observation model without a hidden AI action field.
- Bounded log and conditional auto-scroll.
- Keyboard focus enabled for action buttons.
- Main Screen integration with mutually exclusive Exploration and Combat modes.
- Deterministic CombatService for Attack, Defend and Dodge, including validation, enemy response, Health / Stamina mutation and victory / defeat.
- Victory reward of 25 G is applied to the Player Character through CombatService. Player and Goblin are Character domain objects created from CharacterData.
- Persistent Character Panel showing Health, Fatigue, Stamina, Mood and Gold.
- Return from Combat to the same Exploration location.

Not implemented:
- Multiple enemies, advanced enemy AI, perception checks or randomized combat.
- Skills, items, status effects, VFX, audio or save/load.
- Full activity simulation, relationship, diary or quest behavior.
- Responsive narrow-screen layout verification or full assistive-technology testing.
- Runtime build and end-to-end verification in Godot 4.7.2 .NET.

Combat rules are deliberately small and deterministic for this sprint. The standalone Combat Preview still uses illustrative values and is not the main gameplay entry point.

### 11. Acceptance Criteria

- [ ] Combat is a dedicated scene, separate from Exploration.
- [ ] CombatService is the only owner of action validation, resolution and combat-state mutation when gameplay integration is implemented.
- [x] UI presentation contracts do not expose raw enemy AI state.
- [x] Message data includes stable ID, round, semantic type, category, speaker and text.
- [x] Player, Enemy and System messages have visible text identities in addition to alignment.
- [x] Status shows current enemy HP; log records damage deltas.
- [x] Intent is represented as an observation and can be cleared by sending null.
- [x] Action availability is replaced from latest view data; service-side revalidation is specified.
- [x] Log is bounded to 60 messages.
- [x] Auto-scroll follows new messages only when the reader was already at the bottom.
- [x] Action buttons support keyboard focus.
- [x] Concrete CombatService validates, resolves and applies the current sprint actions.
- [x] Main Controller integrates Combat and Exploration modes for the Goblin encounter.
- [ ] Narrow-screen layout and assistive-technology behavior are tested.

### 12. External Review Questions

1. Is the Service → Result / Snapshot → Mapper → ViewData → UI boundary clear?
2. Does the safe snapshot contain exactly the information presentation needs?
3. Are message categories sufficient without a fourth message type?
4. Is the intent lifecycle consistent with the game's observation rules?
5. Are action availability refresh conditions complete?
6. Does the log preserve reading position correctly?
7. What responsive layout should be used below desktop width?

