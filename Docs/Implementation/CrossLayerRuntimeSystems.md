# Cross-Layer Runtime Systems

**Status:** Describes the current repository runtime behavior  
**Boundary:** This document records implemented runtime behavior and the limits of the current model. Desired future behavior belongs in [Design](../Design/README.md).

## 1. Current prototype overview

The current build combines an authored, location-based modern student-life slice, a bounded daily NPC presence schedule and a four-person, shared-AP, turn-based combat prototype. The Campus Day path exercises Location, Travel, Action, Effect and Location Event content. NPC presence is schedule-driven, but NPCs do not yet make autonomous decisions or simulate needs, memory or relationships.

Implemented areas include:
- Character state and resources, plus the Alex/Noah/Mira/Sera StartingParty composition.
- GameClock day/minute tracking, elapsed-time advancement, midnight rollover and authored Sleep/Rest actions.
- A connected modern city route through university campus, lecture hall, library, campus café, student housing, East District and the old apartment block.
- Location travel, context-sensitive Location Actions and source-aware Exploration choices.
- Authored Location Events for a campus conversation and the Unit 213 incident. The first introduces a local rumor; the second offers observation, cautious entry and retreat without forcing Combat.
- Four-person Combat with a shared Party Action Point pool, discrete Player and Enemy Turns, deterministic authored enemy patterns, Downed/Revive and Victory/Defeat.
- Exploration rescue when Alex returns from an encounter at 0 HP.
- Dedicated Combat presentation and authored JSON definitions loaded into runtime catalogs.

## 2. Current domain vocabulary

| Concept | Current meaning |
|---|---|
| Character | Runtime participant with character data and mutable state |
| NPC Schedule | Daily authored time ranges that map a Character ID to a Location and visible activity |
| Player | The controlled Character in the current prototype |
| Location | Current place that owns configured actions, exits and event definitions |
| Action | Authored operation with supported duration, costs and effects |
| Event | A configured situation evaluated in a Location context; it is not automatically Combat |
| Effect | A configured operation processed by the responsible gameplay logic |
| State | Current runtime truth such as Health, Fatigue, Stamina, Mood, Gold, time and Location |

Do not infer that the repository already contains a generalized NPC presence, object interaction, quest, schedule, inventory or world simulation system. The conceptual proposals in the former World Model draft are not implementation contracts.

## 3. Character state and time

The current prototype exposes Health, Fatigue, Stamina, Mood and Gold.

- Outside Combat, zero Health prevents ordinary Character actions. During Combat, CombatService tracks Party members at zero Health as Downed, prevents them from acting and retains them in the Party roster for possible revival.
- Fatigue is a cumulative daily-life exertion value. It does not currently cause unconsciousness or block ordinary actions.
- Stamina is a separate Combat resource used by defensive actions. Insufficient Stamina rejects the requested action; it does not end Combat.
- Mood is a tracked character value; current effects may change it, but it is not the future party progression system.
- GameClock owns day/minute tracking, elapsed-time advancement and midnight rollover. Sleep is a configured 480-minute Player Action.

Exact current values and action effects must be checked in the relevant JSON definitions and Domain code before changing them. Do not copy tuning values into Design as permanent decisions.

## 4. Location and Exploration flow

The current prototype uses location-based navigation rather than free-roam movement.

- The Player occupies a current Location.
- A Location description may be a legacy string or an object containing `firstVisit` and `returnVisit`. On first entry in a game session the controller selects `firstVisit`; after leaving and re-entering it selects `returnVisit`. The selected prose remains stable during the current stay, including after local Actions and Event choices. Visit history is currently in-memory only and resets when the game process restarts.
- A Location defines its exits, Actions and Event definitions.
- Available choices are queried for the current context and mapped into presentation-safe choice data.
- Selecting a choice sends an `ActionSelectionRequest` with source kind, source ID, action ID and optional target ID.
- The flow controller validates the current source/context and dispatches to the owning execution path.
- Travel changes the current Location; the destination's configured Event pool may then be evaluated.
- The current implementation does not retain a general persistent Interaction Context for arbitrary NPCs or objects.

The current choice sources include Player Actions, Location Actions, NPC interactions, Travel choices and Event choices. Character-specific interactions are loaded from `Data/NpcInteractions.json`, queried for the selected NPC and current Location, and displayed after the player chooses `See around` and selects a person. The selection is revalidated against current NPC presence before execution. Event definitions and choices may still declare `requiredNpcId` for authored, situation-specific scenes; those requirements are checked before a choice is offered and again when selected. Location Actions describe the place itself rather than embedding actions for individual NPCs.

### NPC presence and daily schedules

`GameContentLoader` loads `Data/NpcSchedules.json` into `NpcScheduleCatalog`. Each schedule references an existing Character ID and contains non-overlapping half-open time intervals (`startMinute` inclusive, `endMinute` exclusive), a Location ID and an authored activity description. `GameContentCatalog` validates Character, Location and required-NPC references before the runtime starts.

`NpcPresenceQuery` combines the current Location, `GameClock.MinutesOfDay`, schedule definitions and Character data. `ExplorationFlowController` uses the result to offer `See around`, display the people currently present and revalidate the selected NPC before an interaction executes. Events with an event-level NPC requirement are not triggered while that NPC is absent; an unavailable NPC-specific Event does not consume the Location's daily event attempt. Event choice NPC requirements are checked again when selected. Presence is evaluated when the current context is shown; there is no per-frame NPC movement simulation.

The current schedule repeats every in-game day because `GameClock` has a day index but no weekday calendar. An unscheduled time range means the NPC is not present at a player-facing Location. The current content authors daily schedules for Mira, Noah, Sera and Ms. Carter. Each scheduled Character can be discovered through `See around` when present. NPC interactions can apply to every scheduled Location or specify an explicit `availableLocationIds` list.

When the controlled character is at zero Health, the Exploration query suppresses normal Player, Location and Travel choices and exposes only an authored recovery Player Action. The current `Request emergency rescue` action advances GameClock by 180 minutes and revives the character with 5 HP. The action execution service permits this single-effect Revive action only while the character is incapacitated; normal interactions return after recovery.

## 5. Location Events

Location Events represent authored situations associated with a Location. The current modern setting uses Location Events for everyday conversations and the Unit 213 incident. The Unit 213 event offers observation, cautious entry and retreat; it does not force Combat.

An Event is not synonymous with Combat or Encounter. It may offer a choice that starts the dedicated Combat flow or resolves through the normal action path. Keep the Event decision and the specialized Combat resolution separate.

The current Event definition still contains one description and a set of choices; it is not a general multi-page scene engine. Do not treat the current event selection rules as a finalized world-event scheduler. Verify the relevant `LocationEventService`, content definitions and tests before changing trigger timing or repeat behavior.

## 6. Current Combat implementation

Combat uses discrete Player Turns and Enemy Turns. A Round consists of one Player Turn followed by one complete Enemy Turn. CombatService owns the encounter roster, action validation, resource spending, actor eligibility, enemy pattern progression, terminal conditions and the authoritative combat snapshot.

### Party Turn and shared AP

- The encounter starts at Round 1 with 2/2 shared Party Action Points (AP). CombatService accepts an authored capacity from 1 to 4; MainController explicitly supplies the Unit 213 encounter capacity of 2 AP.
- Every successful Party action in the current slice costs exactly 1 AP, including Revive. Defensive and recovery actions may also spend Stamina as authored.
- The player can choose any living, non-Downed Party member who has not acted in the current Round. A Party member may perform only one AP-funded action per Round, even when AP remains.
- Action options are generated for every eligible Party member and carry the required ActorId. The UI labels each option with its actor; it does not choose the actor from Agility, Luck or an initiative order.
- The Domain revalidates actor, action, target, availability and Stamina before spending AP or locking that actor. A rejected request changes neither AP nor actor eligibility.
- When AP reaches zero, the Enemy Turn begins automatically. The player can also explicitly end the Player Turn; unused AP is discarded.

ActionPool owns AP capacity/current AP and the one-point control penalty queued for the next Player Turn. Multiple control effects coalesce into one penalty; the refreshed AP cannot fall below one. The current migration does not persist capacity progression/training milestones.

### Enemy Turn and authored patterns

Each living Enemy resolves at most one action per Enemy Turn, in stable encounter roster order. CombatantDefinition may provide an EnemyPattern, an ordered sequence of action IDs. EnemyPattern advances a per-encounter runtime cursor and wraps deterministically when the sequence ends. If no explicit pattern is supplied, the authored action list is used as the sequence.

Enemy targets are selected at execution time from the currently living Party roster; a Downed member is not a valid target. A missing target, unavailable action or unaffordable Stamina cost causes that Enemy to skip its step rather than pre-commit an action or target. The UI does not expose a future Intent. The Unit 213 playable roster currently gives Unstable Presence a fixed Heavy Impact pattern and Unstable Echo a fixed Disorienting Pulse pattern; the pulse applies damage and queues the bounded -1 AP penalty for the next Player Turn.

After the complete Enemy Turn, and only if the encounter remains active, Round increments once, Party action eligibility resets, and the ActionPool refreshes for the next Player Turn. Enemy actions and resulting events are resolved synchronously in Domain and returned in encounter order.

### Downed, Revive and encounter completion

- A Party member reaching zero HP becomes Downed, remains visible in the Party roster and cannot act or be selected as an Enemy target.
- Revive costs 1 AP and 4 Stamina, restores 25% of maximum HP rounded up, and can be used only once per character per encounter.
- If every Party member is Downed, the encounter ends in defeat. Enemies at zero HP are defeated rather than Downed.
- CombatSessionService coordinates the entry action and encounter lifecycle. GameClock does not advance per Party action, Enemy action or Round. On encounter completion, the configured world-time and Fatigue costs are applied exactly once; the resulting time interval can be consumed once by the return-to-Exploration flow.

### Ownership and current limits

- CombatService mutates the live Character instances retained by the ordered CombatantDefinition[] roster. It partitions that roster into Party and Enemy lists; it does not create Godot scenes.
- Domain contracts (CombatActionRequest, CombatSnapshot, CombatResult, CombatEvent and ICombatService) live in Scripts/Domain/Combat/CombatContracts.cs. Presentation owns only view data, formatted messages and visual feedback models.
- CombatPresentationMapper maps the ordered Domain result into CombatViewData. CombatPanel displays Round, phase, shared AP, Party statuses, Enemy statuses, action options, the event history and an End Player Turn control.
- Enemy cards display current status only. They do not show the next Enemy action or its target.
- The simulation no longer uses CombatTimeline, Remaining Progress, Speed, Recovery Cost, Delay/Stun durations, frame-delta advancement or weighted Enemy Intent.
- The current UI presents the entire ordered result immediately. A dedicated sequential animation/event queue and polished action-selection feedback remain follow-up presentation work.
- Tease/Arousal/Orgasm remains an MVP requirement in Design/Combat, but is not implemented in this turn-based runtime migration yet. Party-side consequences for reaching the Arousal threshold remain an open design decision.
- Persistent Action Capacity training progression and body-region/Exposure/Accessibility/Enemy Preference rules are not implemented in this migration.

The intended rules and open decisions are defined in Design/Combat. Do not treat numeric balance or explicitly open design decisions as finalized implementation contracts.

## 7. Content and identity constraints

- JSON property names use the repository's documented `camelCase` convention. Location `description` supports the original string form or an object containing `firstVisit` and `returnVisit`.
- Authored content IDs use `snake_case`.
- Serialized enum-like values use `PascalCase`.
- Definitions may reference other definitions by stable ID; loaders/catalogs validate duplicate IDs and unresolved references.
- NPC schedules reference Character and Location IDs; `requiredNpcId` on an Action must reference a Character with a loaded schedule.
- Content can select only behavior already supported by the runtime. JSON is not an arbitrary scripting language.

For exact file locations and ownership, see [Cross-Layer Architecture](CrossLayerArchitecture.md). For current screen and ViewData boundaries, see [Presentation User Interface](PresentationUserInterface.md).
