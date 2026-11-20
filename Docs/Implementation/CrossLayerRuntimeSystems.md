# Cross-Layer Runtime Systems

**Status:** Describes the current prototype on latest `main`  
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

- Health reaching zero means Dead; the current resource model has no Downed/revival state.
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

The current integrated Combat prototype is a deterministic single-Goblin encounter. Its supported actions include Attack, Defend, Dodge and Catch Breath. It uses the existing player resource model and returns to the same Exploration Location after resolution.

Current limitations:
- One player-controlled Character and one deterministic Goblin, not a four-member party.
- No visible action Timeline or action-time manipulation.
- No general Delay, Stun, Interrupt, Downed/revival or party-synergy system.
- No advanced enemy AI, broad skill system, equipment progression or multiple enemy archetypes.

The intended future rules are defined only in [Design/Combat](../Design/Combat.md). Do not extend this prototype by assuming the target design has already been implemented.

## 7. Content and identity constraints

- JSON property names use the repository's documented `camelCase` convention.
- Authored content IDs use `snake_case`.
- Serialized enum-like values use `PascalCase`.
- Definitions may reference other definitions by stable ID; loaders/catalogs validate duplicate IDs and unresolved references.
- Content can select only behavior already supported by the runtime. JSON is not an arbitrary scripting language.

For exact file locations and ownership, see [Cross-Layer Architecture](CrossLayerArchitecture.md). For current screen and ViewData boundaries, see [Presentation User Interface](PresentationUserInterface.md).
