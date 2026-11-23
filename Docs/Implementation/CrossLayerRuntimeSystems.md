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
- GameClock owns the one-based campaign day, Gregorian starting/current date, derived weekday, minute-of-day, elapsed-time advancement and midnight rollover. The initial game composition assigns Monday, 1 September 2025 to campaign Day 1. CurrentDate and Weekday are derived; they are not independently mutable. The calendar uses Gregorian month/year/leap-year rules and rejects advancement beyond 9999-12-31. Sleep remains a configured 480-minute Player Action; it advances the calendar only when midnight is crossed.

Exact current values and action effects must be checked in the relevant JSON definitions and Domain code before changing them. Do not copy tuning values into Design as permanent decisions.

### Character data, base attributes and runtime resources

The current Character model separates authored character definitions from mutable values held by the runtime object, but it does not yet implement a complete Base Stats → Derived Stats → Resources calculation pipeline.

**Authored character data**

- `CharacterData` contains identity fields (`id`, `name`, `role`, `level`), resource limits, starting resource values, `CharacterStats`, Player Actions and Combat Action IDs.
- `CharacterDataLoader` reads these fields from `Data/Characters/*.json`; `CharacterCatalog` provides the loaded definitions to the runtime.
- The serialized `stats` object currently supports six attributes: Strength, Agility, Intelligence, Endurance, Wisdom and Luck. `CharacterStats` is immutable and validates each attribute in the range 0–100.
- These attributes are currently exposed through `Character.Stats` and used by the Character profile presentation to display the character's attributes. The current runtime does not use them to calculate combat power, resource limits or turn order. In particular, Agility and Luck do not determine initiative.

**Runtime values and limits**

- `Character` owns mutable Health, Fatigue, Stamina, Mood and Gold values. Its constructor initializes them from the corresponding `Starting*` fields in `CharacterData`; `Reset()` restores those starting values.
- Maximum Health, Fatigue, Stamina and Mood are read directly from `CharacterData` (`maxHealth`, `maxFatigue`, `maxStamina`, `maxMood`). They are authored per character in JSON, not calculated from Strength, Endurance or another attribute.
- Gold has a starting value and mutable runtime balance, but no maximum value in the current model.
- Combat-specific Downed state, AP eligibility and once-per-encounter Revive usage are owned by the Combat runtime, not by a general Character status-effect framework.

**Not implemented yet**

- There is no `DerivedStats` model or `DerivedStatCalculator` in the current Character Domain.
- There are no runtime Mana/MP or Arousal/Lust resources in `Character` at this revision.
- Equipment, traits, skills and status effects do not yet contribute stat modifiers to a derived-stat calculation.
- The current per-character resource limits must therefore not be described as derived from Base Stats. Any proposed formulas belong in Design until implemented and covered by tests.

When documenting or changing character behavior, keep the distinction explicit: `CharacterStats` holds authored attributes; `CharacterData` holds authored limits and starting values; `Character` holds mutable runtime resources. Do not infer a calculation pipeline from the presence of the attribute data alone.

## 4. Location and Exploration flow

The current prototype uses location-based navigation rather than free-roam movement.

- The Player occupies a current Location.
- A Location description may be a legacy string or an object containing `firstVisit` and `returnVisit`. On first entry in a game session the controller selects `firstVisit`; after leaving and re-entering it selects `returnVisit`. The selected prose remains stable during the current stay, including after local Actions and Event choices. Visit history is currently in-memory only and resets when the game process restarts.
- A Location defines its exits, Actions and Event definitions.
- Available choices are queried for the current context and mapped into presentation-safe choice data.
- Selecting a choice sends an `ActionSelectionRequest` with source kind, source ID, action ID and optional target ID.
- The flow controller validates the current source/context and dispatches to the owning execution path.
- Adjacent Travel follows the current Location's authored exits. Fast Travel is a separate destination-selection page that lists only Locations explicitly marked `fastTravelDestination: true`; it may cross non-adjacent areas but still executes the shared Travel Action, so time and Fatigue rules remain consistent.
- Fast-travel arrival points are outdoor Locations such as Campus Plaza, district/park spaces and authored building entrances. Interior Locations such as lecture rooms, private dormitory rooms, bathrooms and building service areas are not eligible. A player arriving at an entrance must use its normal exit to enter the building.
- Adjacent Travel follows the current Location's authored exits. Fast Travel is a separate destination-selection page that lists only Locations explicitly marked `fastTravelDestination: true`; it may cross non-adjacent areas but still executes the shared Travel Action, so time and Fatigue rules remain consistent.
- Fast-travel arrival points are outdoor Locations such as Campus Plaza, district/park spaces and authored building entrances. Interior Locations such as lecture rooms, private dormitory rooms, bathrooms and building service areas are not eligible. A player arriving at an entrance must use its normal exit to enter the building.
- Travel changes the current Location; the destination's configured Event pool may then be evaluated.
- The current implementation does not retain a general persistent Interaction Context for arbitrary NPCs or objects.

The current choice sources include Player Actions, Location Actions, Location Interactables, NPC interactions, adjacent Travel choices, Fast Travel destinations and Event choices. Character-specific interactions are loaded from `Data/NPC/NpcInteractions.json`, queried for the selected NPC and current Location, and displayed after the player chooses `See around` and selects a person. The selection is revalidated against current NPC presence before execution. Event definitions and choices may still declare `requiredNpcId` for authored, situation-specific scenes; those requirements are checked before a choice is offered and again when selected. Location Actions describe the place itself rather than embedding actions for individual NPCs.

A Location may also author `interactables`: named objects such as a Bed, Laptop or Wardrobe, each with a description, references to existing Player Actions and/or object-owned Actions. The initial Location query exposes only the object targets. Selecting an object queries only that object's actions; the selection request carries the current Location ID and the object ID as its target. The controller revalidates the Location, object, action and any current requirements before execution. A referenced Player Action is executed through the existing Player Action path rather than copied into the object definition. This is a small target-first content contract, not a general object simulation or persistent inventory system.

Actions may optionally declare an inclusive `availableStartMinute` and exclusive `availableEndMinute` in minutes after midnight. The read-only query uses the current GameClock minute to hide actions outside that daily window, and ActionExecutionService checks the window again during preparation and commit to reject stale selections. The opening Research Methods lecture uses this for its 09:00–09:15 start window; the player can use the 15-minute Wait Player Action at the Lecture Hall before it opens. This does not introduce weekday-specific commitments or a general schedule framework.

### NPC presence and daily schedules

`GameContentLoader` loads `Data/NPC/NpcSchedules.json` into `NpcScheduleCatalog`. Each schedule references an existing Character ID and contains non-overlapping half-open time intervals (`startMinute` inclusive, `endMinute` exclusive), a Location ID and an authored activity description. `GameContentCatalog` validates Character, Location and required-NPC references before the runtime starts.

`NpcPresenceQuery` combines the current Location, `GameClock.MinutesOfDay`, schedule definitions and Character data. `ExplorationFlowController` uses the result to offer `See around`, display the people currently present and revalidate the selected NPC before an interaction executes. Events with an event-level NPC requirement are eligible only while that NPC is present; unavailable entries are skipped. Event choice NPC requirements are checked again when selected. Presence is evaluated when the current context is shown; there is no per-frame NPC movement simulation.

The current NPC schedules still repeat every in-game day because `NpcScheduleDefinition` does not yet contain weekday restrictions. `GameClock` exposes a Gregorian CurrentDate and derived Weekday, but schedule content has not yet been migrated to consume them. An unscheduled time range means the NPC is not present at a player-facing Location. The current content authors daily schedules for Mira, Noah, Sera and Ms. Carter. Each scheduled Character can be discovered through `See around` when present. NPC interactions can apply to every scheduled Location or specify an explicit `availableLocationIds` list.

When the controlled character is at zero Health, the Exploration query suppresses normal Player, Location and Travel choices and exposes only an authored recovery Player Action. The current `Request emergency rescue` action advances GameClock by 180 minutes and revives the character with 5 HP. The action execution service permits this single-effect Revive action only while the character is incapacitated; normal interactions return after recovery.

## 5. Location Events

Location Events represent authored situations associated with a Location. The current modern setting uses Location Events for everyday conversations and the Unit 213 incident. The Unit 213 event offers observation, cautious entry and retreat; it does not force Combat.

An Event is not synonymous with Combat or Encounter. It may offer a choice that starts the dedicated Combat flow or resolves through the normal action path. Keep the Event decision and the specialized Combat resolution separate.

In the current prototype, a Location's Event pool is evaluated after a successful arrival. There is no once-per-day attempt limit, cooldown, repeat policy or persistent completion state. A future Event/Quest system will own those rules; `GameClock.Day`, `CurrentDate` and `Weekday` do not currently determine Event eligibility.

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
- Domain contracts (CombatActionRequest, CombatSnapshot, CombatResult, CombatEvent and ICombatService) live in Scripts/Features/Combat/Rules/CombatContracts.cs. Presentation owns only view data, formatted messages and visual feedback models.
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
