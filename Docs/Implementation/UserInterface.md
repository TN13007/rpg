# User Interface and Presentation Contracts

**Status:** Describes the current main-screen and Combat presentation on latest `main`  
**Boundary:** This document owns current UI composition and presentation contracts. Product experience goals belong in [Design](../Design/README.md).

## 1. Main screen composition

| Region | Current responsibility |
|---|---|
| Sidebar | Navigation between available pages |
| Main Panel | Current Location exploration or dedicated activity mode |
| Character Panel | Persistent display of current Player state |

The Main Panel changes mode between Exploration and Combat. The Character Panel remains visible across both.

## 2. Exploration presentation

Exploration displays current Location context and available choices. Choices are discovered by Application queries and mapped into grouped, presentation-safe data.

Current pipeline:

```text
AvailableInteractionQuery
  -> ActionChoiceMapper
  -> ActionChoiceViewData
  -> StoryPanel
  -> ActionSelectionRequest
  -> ExplorationFlowController
```

The UI renders the groups and ordering supplied by its view data. It does not decide which actions are valid, inspect JSON or mutate Domain state.

The current request identifies:
- Source kind.
- Source ID.
- Action ID.
- Optional target/destination ID.

Action IDs alone are not globally unique. The owning flow/service revalidates each request because UI availability is advisory and state may have changed.

## 3. Combat presentation

Combat uses a dedicated Combat Panel rather than reusing the Exploration choice groups.

The presentation pipeline is:

```text
CombatService
  -> CombatResult + presentation-safe CombatSnapshot
  -> CombatPresentationMapper
  -> CombatViewData
  -> CombatPanel
```

The current panel separates:
- Encounter context.
- Current combat status.
- Ordered message history.
- Available Combat Actions.

### Ownership

- CombatService owns action validation, resolution and combat-state mutation.
- CombatSnapshot contains only data safe for presentation.
- CombatPresentationMapper converts events and snapshots into view data.
- CombatPanel renders the view data; it does not calculate damage, spend resources or inspect hidden enemy decisions.
- The latest snapshot replaces current status and available actions; new messages append to history.

### Message semantics

Messages use Player, Enemy or System identity. Speaker/category labels remain visible; alignment or color is not the only identity cue. Completed events are historical facts, while observations/inferences must be presented as such.

The current prototype uses exact enemy HP in status and records damage deltas in the log. The log is bounded and preserves reading position when the user scrolls older messages.

## 4. Character Panel

The current Character Panel displays Health, Fatigue, Stamina, Mood and Gold. These are current prototype resources, not a commitment to the final game HUD.

The panel presents state; it does not own resource rules or mutate the Character.

## 5. Mode transitions

- Exploration → Combat: show the dedicated Combat Panel.
- During Combat: replace status/action data from the latest view.
- Combat → Exploration: return to the current Location's Exploration presentation.
- Character Panel remains persistent.

MainController composes the screen and top-level signals. ExplorationFlowController and CombatFlowController own their respective presentation flows and delegate gameplay rules to Application/Domain services.

## 6. Current limitations

The current UI is a prototype, not a final interface specification. Multiple party members, Timeline presentation, Downed/revival, broad skill selection, inventory, relationships, diary, responsive narrow-screen behavior and full assistive-technology verification are not complete.

When the intended UI changes, first update the relevant Design document if the player experience changes; update this document only when the current presentation contract changes.
