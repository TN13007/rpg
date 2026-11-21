# Presentation User Interface and Contracts

**Status:** Describes the current repository UI composition and presentation contracts  
**Boundary:** This document owns current UI composition and presentation contracts. Product experience goals belong in [Design](../Design/README.md).

## 1. Main screen composition

| Region | Current responsibility |
|---|---|
| Sidebar | Navigation between available pages |
| Main Panel | Current Location exploration or dedicated activity mode |
| Character Panel | Persistent display of current Player state |

The Main Panel changes mode between Exploration and Combat. The Character Panel remains visible across both.

## 2. Exploration presentation

Exploration displays current Location context and available choices. Choices are discovered by Application queries and mapped into grouped, presentation-safe data. `StoryPanel` renders Location prose in a scrollable `RichTextLabel`, so long first-visit introductions remain readable without pushing choices off-screen. The Exploration flow chooses `firstVisit` on a Location's first entry during the current process and `returnVisit` on later entries; the selected text stays stable during the current stay. Visit history is not persisted across restarts.

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
MainController._Process(delta)
  -> CombatFlowController.AdvanceCombatTime(delta)
  -> CombatSessionService.AdvanceCombatTime(delta)
  -> CombatService.AdvanceCombatTime(delta)
      -> CombatSnapshot
          -> CombatPresentationMapper
      -> CombatViewData
          -> CombatPanel
              -> CombatStatusPanel
                  -> TimelineTrack.tscn
                      -> TimelineMarker.tscn instances
```

The current panel separates:
- Encounter context and Party roster, including a visible Downed condition.
- Expected action timing on a shared Time-to-Ready axis.
- Ordered message history.
- Available Combat Actions are listed once per action, regardless of the number of valid targets. Selecting an enemy-targeting action activates the valid EnemyCards in the Sidebar; the player selects one card and confirms it. Revive remains a separate ally-target flow because its targets are Party members, not enemies.

`CombatPresentationMapper` converts each combatant's Remaining Progress, Speed and remaining Stun duration into `TimelineMarkerViewData.TimeToReadySeconds`. The UI consumes this presentation value; it does not calculate combat timing from Domain state.

`TimelineTrack` owns marker instances and their positions. Its default display horizon is 10 seconds and is configurable in the Inspector. NOW is on the left; the right edge represents the end of the display horizon. Actors beyond the horizon are clamped to the right edge. Party markers appear above the shared rail and Enemy markers below it. Markers that overlap within the same side are stacked into additional vertical lanes without changing their horizontal timing. The track is refreshed each runtime frame from the Domain snapshot, so at the current 1.0x speed a marker takes the same real-world time as its Time-to-Ready value to reach NOW; no fixed-duration marker Tween is used.

`TimelineMarker` owns one combatant's visual presentation: faction color, tooltip and Stunned indicator. Its `TextureRect` is ready to display an icon, but the current combat content does not yet supply portrait textures, so markers remain color-coded circles. Hidden enemies are omitted by the presentation layer while remaining in combat simulation.

### Ownership

- CombatService owns action validation, resolution and combat-state mutation.
- CombatSnapshot contains only data safe for presentation.
- CombatPresentationMapper converts events and snapshots into view data.
- CombatPanel renders the view data; it does not calculate damage, spend resources or inspect hidden enemy decisions.
- The latest snapshot replaces current status and available actions; new messages append to history.

### Reusable presentation components

Combat UI components consume presentation contracts and do not access Domain entities or services directly:

| Component | Input contract | Responsibility |
|---|---|---|
| `EnemyCard` | `EnemyCombatStatus` | Render enemy name, HP, condition and committed intent; expose target-selection state and selection event |
| `PartyMemberCard` | `PartyCombatStatus` | Render one party member's status and receive action feedback |
| `CombatActionCard` | `CombatActionOption` | Render one available action and emit the selected option |
| `TimelineMarker` | `TimelineMarkerViewData` | Render one combatant on the shared Timeline |
| `CombatMessageEntry` | `CombatMessage` | Render one historical combat message |

Parent panels own component collections and lifecycle. They create, update, reorder and release instances by stable entity ID. A component receives a presentation object through `Set...` methods; it must not look up `Character`, query `CombatService`, parse content files or decide whether an action is legal. Components may expose UI events, but the owning controller/service revalidates the request.

`CombatPresentationMapper` also maps resolved action events into `CombatFeedbackViewData`. The orchestrator routes those cues to the matching PartyMemberCard or EnemyCard. Attack, Guard, Evade, Catch Breath, Revive and Hit currently use short visual color pulses; these are presentation-only and do not affect combat resolution.
### Message semantics

Messages use Player, Enemy or System identity. Speaker/category labels remain visible; alignment or color is not the only identity cue. Completed events are historical facts, while observations/inferences must be presented as such.

The current prototype uses exact enemy HP in status and records damage deltas in the log. The log is bounded and preserves reading position when the user scrolls older messages. Target selection is confirmed before execution; invalid targets are not selectable and the sidebar cursor changes only for valid EnemyCards.

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

The current UI is a prototype, not a final interface specification. The playable MainController encounter is wired with Alex, Noah, Mira and Sera against one Unstable Presence. Only one Unstable Presence behavior archetype is currently authored. Portrait textures are not yet supplied to Timeline markers. Interactive Godot acceptance is still required to verify target selection, action feedback and the full encounter flow. Broad skill selection, inventory, relationships, diary, responsive narrow-screen behavior and full assistive-technology verification remain incomplete.

When the intended UI changes, first update the relevant Design document if the player experience changes; update this document only when the current presentation contract changes.
