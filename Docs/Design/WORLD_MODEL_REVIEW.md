# World Model Review — NPC, Object and Location

**Related issue:** [#12 — Review NPC, Object and Location](https://github.com/TN13007/rpg/issues/12)  
**Reference documents:** [World Model Foundation](WORLD_MODEL_FOUNDATION.md), [Game Overview](GAME_OVERVIEW.md), [UI Architecture](UI_ARCHITECTURE.md), [Gameplay Loop and Location Events](GAME_LOOP_AND_LOCATION_EVENTS.md)  
**Scope:** Design review only. This document records what the current prototype actually models and what remains conceptual.

## 1. Summary

The existing world model is directionally compatible with the current Location / Action / Event prototype. No new universal Actor, Object, Interaction, Encounter or World Simulation system is justified by the reviewed examples.

However, the current prototype is narrower than the conceptual model:

- Location actions are authored directly inside each Location.
- The current Mira, letter and notice-board interactions are represented as Actions, not as fully modelled NPC/Object entities.
- Action provider, performer and target are not all explicitly represented in the current prototype data.
- There is no runtime Interaction Context.
- Location travel currently routes through a generated choice ID and controller logic, rather than a typed Action Target / Location Effect model.

These are known prototype boundaries, not reasons to introduce abstractions immediately.

## 2. Scenario review

### Scenario A — Talk to Mira (NPC interaction)

**Current prototype representation**

- `Data/academy_locations.json` defines `talk_mira` under the Courtyard's `actions`.
- The action costs 1 Energy, advances 15 minutes and applies a Mood effect.
- The label and feedback mention Mira.
- The current action definition does not identify Mira with a Character ID or an explicit target reference.

**What the world model already covers**

- A living individual is a `Character`.
- NPC is a contextual role of Character, not a separate mandatory entity type.
- A Character can be present at a Location.
- A Character may provide an Action; the Player may be the Performer.

**Review result**

The conceptual model supports the intended interaction, but the current prototype does not yet model Mira as a Character with Location Presence. The current action behaves as a Location-listed prototype choice, not as a complete NPC-targeted interaction.

**Decision**

Keep the current action for the vertical slice. When NPC interaction becomes a real feature, represent the NPC as Character content and define its presence and Action ownership/target explicitly. Do not create a separate NPC entity hierarchy or Relationship system solely for this one action.

### Scenario B — Inspect a letter or notice board (Object interaction)

**Current prototype representation**

- `inspect_letter` is defined as a Dormitory Location Action.
- `inspect_board` is defined as a Courtyard Location Action.
- Their labels and feedback describe physical objects, but there is no separate Object definition or Object Presence record in the current content.

**What the world model already covers**

- A non-living world object is separate from Character.
- An object may expose interactions when it needs them.
- Item, Equipment, Resource and Container are candidate concepts/capabilities, not mandatory inheritance branches.
- A Mirror, Bed, letter or notice board does not become a Location merely because the player interacts with it.

**Review result**

The current data models these interactions as simple Location Actions. That is adequate while each interaction is immediate, stateless and has no independent identity, ownership, durability or reusable interaction set.

The prototype does not yet prove the need for a universal Interactable runtime system.

**Decision**

Keep simple one-step interactions as Location Actions for now. Introduce an Object definition only when gameplay needs the object itself to have identity, presence, state, ownership or multiple reusable interactions. Do not convert every descriptive prop into an entity.

### Scenario C — Perform an activity at a Location

**Current prototype representation**

- `LocationContent` owns Exits, Actions and Events.
- Action definitions carry labels, Energy cost, duration, effects and feedback.
- Travel is validated against the current Location's exits.
- `ActionExecutionService` validates and executes the configured Action, including resource/time/effect handling.
- After successful travel, `LocationEventService` evaluates the destination Event pool.
- `MainController` coordinates the Location change and presentation.

**What the world model already covers**

- Location is the place where the player and world content are situated.
- Activity is a goal-oriented grouping; Action is a concrete operation.
- Location navigation and Location containment are different concepts.
- Event is not automatically an Encounter or Combat.

**Review result**

This scenario aligns with the current model and is the most complete implementation of the three. The current implementation remains a narrow Location-based prototype, not a general world-presence simulation.

One implementation detail is less explicit than the conceptual model: the travel destination is currently carried through the generated `travel:<locationId>` choice and controller routing. It is not yet a strongly typed Action Target / Location-changing Effect.

**Decision**

Keep the current route/action implementation for the prototype. Revisit typed travel targets only when a concrete requirement needs reusable target validation, route queries or a shared Location-changing Effect. Do not add a Location Connection model pre-emptively.

## 3. Interaction Context

The current prototype performs the reviewed actions immediately. It does not retain a selected NPC/Object as a separate runtime Interaction Context.

That is acceptable for one-step actions such as reading a letter or talking to Mira once.

A distinct Interaction Context becomes justified when the player enters a persistent subview or sequence—for example, selecting a chest and then choosing Inspect, Open or Pick Lock while remaining in the same Location. At that point, the game must retain which object is currently being interacted with and define when that context is cleared.

**Status:** Unresolved future requirement; not a blocker for the current vertical slice.

## 4. Action / Effect / State / Event flow

The current prototype flow can be summarized as:

```text
StoryPanel emits a choice ID
        |
        v
MainController routes the request
        |
        +--> ActionExecutionService validates and executes Action
        |        |
        |        +--> spends Energy / advances time / applies supported Effects
        |        +--> updates Character and DayClock state
        |
        +--> LocationEventService evaluates a destination Event pool
                 |
                 v
          MainController updates StoryPanel / CharacterPanel
```

Combat remains a specialized path through CombatService and its presentation mapper.

This is compatible with the responsibility boundaries in World Model Foundation. The current implementation does not yet provide a general typed provider / performer / target / source contract for every NPC and Object interaction.

## 5. Findings by status

| Area | Status | Finding |
|---|---|---|
| Character and contextual NPC role | Accepted conceptually | Current prototype has Player/Goblin Characters; Mira is not yet represented as a Character record |
| Location and Location Presence | Proposed conceptually | Locations and exits are implemented; general Character/Object presence is not |
| Object vs Location | Accepted conceptually | Letter and board are currently Location Actions, not Object entities |
| Action provider vs Performer | Accepted as a design rule | Current Location Actions do not encode a general provider reference |
| Action target / source | Partially modelled | Current Event can reference a Location Action; general typed target/source references are not implemented |
| Activity vs Action | Accepted conceptually | Durations and effects belong to Action definitions in the prototype; Activity remains a useful grouping |
| Interaction Context | Unresolved | Not needed for current immediate actions; may be needed for multi-step object/NPC views |
| Location Event vs Encounter vs Combat | Accepted and implemented in the vertical slice | Event choice may route into specialized Combat or a normal Action |
| Saveable world/object state | Unresolved | Save/Load and persistent object/world consequences are not implemented |

## 6. Conclusion

The existing `WORLD_MODEL_FOUNDATION.md` remains a suitable provisional foundation. The three scenarios reveal implementation gaps, but no contradiction requiring an immediate redesign.

The smallest correct next step is to preserve the existing concepts and add explicit NPC/Object presence and target ownership only when a gameplay feature needs them. No implementation change is required by this review.

Issue #12 can be considered satisfied by this review record together with the existing foundation document. The unresolved items above should remain open design questions rather than being silently converted into implementation tasks.
