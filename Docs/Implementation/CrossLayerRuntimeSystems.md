# Cross-Layer Runtime Systems

**Status:** Describes the current repository runtime behavior  
**Boundary:** This document records implemented runtime behavior and the limits of the current model. Desired future behavior belongs in [Design](../Design/README.md).

## 1. Current prototype overview

The current build is an Academy/location-based prototype with a deterministic Goblin encounter. It is not yet the combat-first Tactical Timeline game described by Design.

Implemented areas include:
- Character state and resources.
- GameClock day/minute tracking and elapsed-time advancement.
- Location travel and context-sensitive Location Actions.
- Player Actions such as Rest and Sleep.
- Location Events with player-selected outcomes.
- A dedicated deterministic Combat flow and presentation.
- Authored JSON definitions loaded into runtime catalogs.

## 2. Current domain vocabulary

| Concept | Current meaning |
|---|---|
| Character | Runtime participant with character data and mutable state |
| Player | The controlled Character in the current prototype |
| Location | Current place that owns configured actions, exits and event definitions |
| Action | Authored operation with supported duration, costs and effects |
| Event | A configured situation evaluated in a Location context; it is not automatically Combat |
| Effect | A configured operation processed by the responsible gameplay logic |
| State | Current runtime truth such as Health, Fatigue, Stamina, Mood, Gold, time and Location |

Do not infer that the repository already contains a generalized NPC presence, object interaction, quest, schedule, inventory or world simulation system. The conceptual proposals in the former World Model draft are not implementation contracts.

## 3. Character state and time

The current prototype exposes Health, Fatigue, Stamina, Mood and Gold.

- Outside Combat, zero Health prevents ordinary Character actions. During Combat, CombatService tracks Party members at zero Health as Downed, removes them from the Timeline and retains them in the Party roster for possible revival.
- Fatigue is a cumulative daily-life exertion value. It does not currently cause unconsciousness or block ordinary actions.
- Stamina is a separate Combat resource used by defensive actions. Insufficient Stamina rejects the requested action; it does not end Combat.
- Mood is a tracked character value; current effects may change it, but it is not the future party progression system.
- GameClock owns day/minute tracking, elapsed-time advancement and midnight rollover. Sleep is a configured 480-minute Player Action.

Exact current values and action effects must be checked in the relevant JSON definitions and Domain code before changing them. Do not copy tuning values into Design as permanent decisions.

## 4. Location and Exploration flow

The current prototype uses location-based navigation rather than free-roam movement.

- The Player occupies a current Location.
- A Location defines its exits, Actions and Event definitions.
- Available choices are queried for the current context and mapped into presentation-safe choice data.
- Selecting a choice sends an `ActionSelectionRequest` with source kind, source ID, action ID and optional target ID.
- The flow controller validates the current source/context and dispatches to the owning execution path.
- Travel changes the current Location; the destination's configured Event pool may then be evaluated.
- The current implementation does not retain a general persistent Interaction Context for arbitrary NPCs or objects.

The current choice sources include Player Actions, Location Actions, Travel choices and Event choices. Generic target interactions are not a generalized runtime feature.

## 5. Location Events

Location Events represent configured situations associated with a Location. The current Courtyard Goblin Ambush is a narrow deterministic example with Confront and Sneak choices.

An Event is not synonymous with Combat or Encounter. It may offer a choice that starts the dedicated Combat flow or resolves through the normal action path. Keep the Event decision and the specialized Combat resolution separate.

Do not treat the current event selection rules as a finalized world-event scheduler. Verify the relevant `LocationEventService`, content definitions and tests before changing trigger timing or repeat behavior.

## 6. Current Combat implementation

Combat now uses a shared Continuous Timeline driven in real time at 1.0x. `MainController._Process(delta)` forwards each Godot frame delta through `CombatFlowController` and `CombatSessionService` to `CombatService.AdvanceCombatTime`. The service advances all eligible combatants together, clamps each frame step at the next Ready or Stun-expiry event, resolves one Ready Enemy at a time, and stops immediately when a Party member becomes Ready. Time does not advance while the player chooses an action. After an action resolves, its Recovery Cost is applied and the next frame resumes the shared clock. Every party member is player-controlled. Enemy actions are selected from the actions authored for that enemy.

Each combatant has Remaining Progress and Speed. At encounter start, CombatService calculates one Progress Scale from the slowest active Base Speed and the largest Recovery Cost in the active roster. The current target is a maximum 4-second recovery for the slowest combatant. CombatTimeline applies that same scale to Initial Progress, Recovery Cost and Delay; Stun durations remain unscaled Combat Time. This keeps quick and heavy actions proportionate while shortening idle gaps. An action applies its scaled Recovery Cost as the actor's next Remaining Progress. The simulation supports Delay and Stun through the combat effect resolver/dispatcher. Stun preserves progress while freezing the target; its duration is measured in Combat Time.

The presentation pipeline maps combat timing into `TimelineMarkerViewData.TimeToReadySeconds`. For normal combatants this is `RemainingProgress / Speed`; for Stunned combatants it also includes the remaining Stun duration. `TimelineTrack.tscn` displays the next 10 seconds by default, with NOW at the left and the future horizon at the right. The Timeline is refreshed from the current snapshot each runtime frame, so marker movement matches the 1.0x Combat Clock without a lagging fixed-duration Tween. Party markers occupy the upper side of the rail and Enemy markers the lower side; overlapping markers stack within their own side without changing their horizontal time. It creates and reuses `TimelineMarker.tscn` instances by combatant ID. Hidden enemies are filtered from the UI but remain registered in the simulation.

Current limitations:
- The playable `MainController` encounter is composed from Ari, Bram, Mira and Sera through `StartingParty`, plus one Goblin. The helper reuses Ari's Exploration `Character` instance and creates the other members from their character content. All four are passed to the existing Party Roster presentation and remain individually player-controlled.
- Current Timeline markers use faction-colored circles. The marker scene has an icon slot, but no combatant portrait textures are currently supplied.
- A Party member reaching zero HP enters Downed, remains visible in the Party roster and leaves the Timeline. Any active ally can spend 4 Stamina to revive that member once per encounter at 25% maximum HP (rounded up). The revived member and the reviving actor both receive the largest authored Recovery Cost in the encounter; the revived member is re-registered on the Timeline. If every Party member is Downed, the encounter ends in defeat. Enemies at zero HP are defeated rather than Downed.
- Speed modifiers, Acceleration, Interrupt, casting/channeling and Cooldowns are not implemented.
- Enemy behavior remains limited to the currently authored Goblin actions; distinct enemy behavior archetypes and multi-enemy playable encounters remain incomplete.

The intended rules and open decisions are defined in [Design/Combat](../Design/Combat.md). Do not treat numeric balance or explicitly open design decisions as finalized implementation contracts.

## 7. Content and identity constraints

- JSON property names use the repository's documented `camelCase` convention.
- Authored content IDs use `snake_case`.
- Serialized enum-like values use `PascalCase`.
- Definitions may reference other definitions by stable ID; loaders/catalogs validate duplicate IDs and unresolved references.
- Content can select only behavior already supported by the runtime. JSON is not an arbitrary scripting language.

For exact file locations and ownership, see [Cross-Layer Architecture](CrossLayerArchitecture.md). For current screen and ViewData boundaries, see [Presentation User Interface](PresentationUserInterface.md).
