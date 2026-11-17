# UI Architecture — Main Game Interface

**Status:** Design direction with an integrated playable encounter slice  
**Related documents:** [Game Overview](GameOverview.md), [Exploration Action Choice Pipeline](ExplorationActionPipeline.md), [Combat Panel Contract](CombatPanelReview.md)

## 1. Main Screen Composition

| Region | Responsibility |
|---|---|
| Left Sidebar | Navigate between major player-facing pages |
| Main Panel | Present current Location, story context or dedicated activity mode |
| Right Character Panel | Keep essential current Player state visible |

The Main Panel is the primary content area. It is not merely a text log.

## 2. Sidebar

| Group | Pages / content | Purpose |
|---|---|---|
| CHARACTER | Profile, Skills, Inventory, Relationships | Inspect and manage the character |
| DIARY | Current Goals, Journal Entries | Track objectives and recorded experiences |
| SYSTEM | Settings | Configure the game |

The Main Panel owns current Location exploration. A separate Explore sidebar page is not planned. A dedicated Map page remains undecided.

## 3. Main Panel Modes

### 3.1 Exploration Mode

Exploration communicates:
- Current Location name, region and description.
- Relevant day/time context.
- Atmosphere and illustrative imagery where available.
- Available choices grouped by their presentation purpose.

The player does not need free-roam movement or pathfinding to interact with the world.

### 3.2 Exploration choice groups

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

### 3.3 Data-to-UI boundary

The intended Exploration pipeline is:

```text
JSON Loaders
  -> immutable content definitions
  -> runtime state + current context
  -> Available Interaction Query
  -> Action Choice Mapper
  -> ExplorationViewModel / ActionGroupViewModel
  -> StoryPanel
```

Responsibilities:
- **Loader:** reads and validates its JSON source; creates data definitions.
- **Query:** selects choices that belong in the current context and determines advisory availability from current state. It does not mutate state.
- **Mapper:** converts query results into presentation-safe, grouped data. It does not read files or apply gameplay rules.
- **ViewModel:** contains labels, duration/cost text, stable choice identity, enabled state and unavailable reason needed by the View.
- **StoryPanel:** renders groups and emits the selected choice identity. It does not execute actions or access domain objects.

The Loader is upstream from the Mapper, but the Mapper does not depend on or call the Loader. Query sits between loaded definitions/runtime state and the Mapper.

The UI's enabled/disabled state is advisory. The responsible gameplay handler/service validates every selected request again.

### 3.4 Choice identity

A choice request must identify its source and action, and optionally its target/destination. Action IDs alone are not globally unique: two containers may both define `open`.

Conceptual request fields:
- Source kind.
- Source ID.
- Action ID.
- Optional target/destination ID.

Keep this contract as small as current features require. NPC/Object targets are future extensions, not Version 1 runtime requirements.

### 3.5 Combat Mode

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

Detailed contracts are defined in [Combat Panel Contract](CombatPanelReview.md).

### 3.6 Other Activity Modes

Study, rest, social interaction and inventory management may later need focused views. Do not assume every activity should reuse the Combat message-log layout. Version 1 daily activities can use the Exploration choice/result flow.

## 4. Character Panel

The right-side Character Panel remains visible across Main Panel modes.

Initial persistent indicators:
- **Health:** survival; zero means death.
- **Energy:** action capacity; zero while alive means unconsciousness.
- **Mood:** emotional state.
- **Gold:** reward currency.

Detailed information belongs in relevant character pages. Keep the persistent panel minimal.

## 5. Information Ownership

- Character Panel: current persistent Player state.
- Combat Status: current encounter state.
- Combat Message Log: ordered history, not the source of truth for current HP.
- Exploration: current Location, context and grouped available choices.
- Diary: selected goals and meaningful records, not every transient UI message.

Avoid duplication unless repetition materially improves usability.

## 6. Mode Ownership and Transitions

MainController owns high-level Main Panel mode transitions:
- Entering a Location presents Exploration.
- Selecting an Event choice preserves the focused Event context until resolved.
- Starting an encounter switches to Combat.
- Ending Combat returns to the same Location's Exploration presentation.
- Character Panel remains persistent.

MainController orchestrates handlers and refreshes ViewModels. It must not calculate action costs, decide object state, resolve puzzles or implement combat rules.

## 7. Prototype Reality Check

Currently implemented:
- Sidebar and persistent Character Panel.
- Exploration list of choices rendered by StoryPanel.
- Flat ActionChoiceViewData with ID, label and duration label.
- MainController currently combines Location Actions, root academy Self Actions and derived Travel choices.
- AcademyContent reads Location content and the transitional root `selfActions` list.
- CharacterDataLoader reads Player identity/base values/stats, but not Player Actions.
- A dedicated Combat Panel and Combat Service → Mapper → ViewData pipeline for the Goblin encounter.

Planned in Version 1:
- Player Actions authored in `player.json`.
- Available Interaction Query for Player, Location, Travel and Event sources.
- Grouped Exploration ViewModel and grouped StoryPanel rendering.
- Stable source/action request identity and MainController orchestration without flat-list composition.

Not implemented:
- NPC population and presence, Inventory/item use, generic Interactable Objects, Containers, Puzzles and Nearby choice group.
- Save/load and broader life simulation.
- Skill progression and Combat Action unlocks.
- Full narrow-screen and assistive-technology verification.

## 8. UI Review Criteria

1. Can the Player identify current context and available choices?
2. Are groups meaningful and free of empty sections?
3. Does UI receive presentation-safe ViewModels instead of reading domain objects or JSON?
4. Are current state, history, observations and inferences distinguishable?
5. Do disabled choices explain why they are unavailable?
6. Does the owning service revalidate every request?
7. Does each activity receive a presentation suited to its decisions?
