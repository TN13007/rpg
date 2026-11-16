# Gameplay Loop and Location Events

**Status:** Prototype decisions implemented on `feat/location-event-game-loop`. The rules below define the current vertical slice; they are not a final random-world simulation specification.

## 1. Core Gameplay Loop

The game uses a location-based loop. It does not require an Expedition, Run or continuous dungeon session abstraction.

```text
Player is at a Location
        |
        v
Choose an available Action or travel
        |
        v
If travel succeeds, update CurrentLocation
        |
        v
Evaluate the destination's Location Events
        |
        +---- No event ----> Show Location Exploration
        |
        +---- Event -------> Show Event choices
                                  |
                     +------------+------------+
                     |                         |
                  Combat                   Other action
                     |                         |
              Dedicated Combat UI       ActionExecutionService
                     |                         |
              CombatService resolves    Apply effects and time
                     |                         |
                     +------------+------------+
                                  |
                                  v
                       Return to same Location
                                  |
                                  v
                         Continue the day
```

An Event is an optional situation introduced during exploration. It does not automatically start an Encounter or Combat. The player must choose a resolution.

## 2. Location Event Ownership

- A `Location` declares the Event Definitions that may occur there.
- An Event Definition is reusable content: identity, title, description, chance and available choices.
- A runtime occurrence exists only when the selection process chooses that definition. The current prototype presents it immediately and keeps the active choice context in `MainController`.
- Event content is stored in `Data/academy_locations.json`; C# owns selection, validation and execution rules.
- An Event is not the same as an Encounter. The Goblin event offers a confrontation choice that starts the existing Combat encounter. Sneaking resolves through the normal Action execution path.

## 3. Prototype Event Selection Rules

1. Evaluate events only after a successful travel action changes the player's current Location.
2. The player must be able to act. If the player is dead or unconscious, the event evaluation is skipped and does not consume the daily attempt.
3. A Location's event pool is evaluated at most once per in-game day.
4. If the Location has no events, no attempt is recorded.
5. Evaluate event definitions in their authored JSON order. The first event whose chance succeeds is selected; at most one event is shown for that Location on that day.
6. A chance is a value from 0 through 1. Zero never triggers; one always triggers.
7. If all event rolls fail, revisiting the same Location on that day does not reroll. The Location can be evaluated again on the next in-game day.
8. Event-attempt state is runtime-only in this prototype. It resets when the game process restarts because save/load is not implemented.

The once-per-location-per-day rule is a deliberately small prototype policy to prevent repeated travel from farming the same event. It is not a decision that every future Location or Event must use this cadence.

## 4. Event Choice Resolution

An Event owns its available choices. A choice contains an Action Definition for presentation and normal action costs/time/effects. It may also reference an existing Location Action through `encounterActionId`.

- A choice with `encounterActionId` delegates to the referenced Location Action, then starts the existing Encounter/Combat flow.
- A choice without `encounterActionId` executes its own Action Definition through `ActionExecutionService`.
- The Event Choice ID must match its Action ID so the UI's emitted choice identifier always resolves to exactly one authored choice.
- A choice that cannot currently execute is rejected without spending resources or time; the Event remains available for another choice.
- Successful non-combat resolution returns to Exploration at the same Location.
- Successful Combat resolution uses the dedicated Combat UI and `CombatService`. The existing victory reward, defeat/death rules and encounter duration remain authoritative.

The first event is `courtyard_goblin_ambush`:
- **Confront the goblin:** references the Courtyard's existing `encounter_goblin` action and enters Combat.
- **Sneak past the goblin:** spends 1 Energy and 10 minutes, then returns to Courtyard Exploration.

## 5. Responsibility Boundaries

| Responsibility | Owner |
|---|---|
| Event and choice definitions, content validation | `AcademyContent` and `LocationEventDefinition` |
| Daily event-pool attempt and random selection | `LocationEventService` |
| Travel, active event context, choice routing and mode transitions | `MainController` (application orchestrator) |
| Action cost, time and effect execution | `ActionExecutionService` |
| Combat validation, turns, enemy response and victory reward | `CombatService` |
| Exploration and Combat rendering | Existing UI panels |

No separate Expedition, Run or general-purpose Event System is introduced.

## 6. Automated Test Coverage

`RPG.Tests/LocationEventLoopTests.cs` verifies:
- Loading the Courtyard Event and its choices from JSON.
- Guaranteed and impossible event chances.
- Once-per-location-per-day behavior, next-day reset and independent locations.
- Skipping event evaluation when the character cannot act.
- Authored event order and one selected event per evaluation.
- Invalid event probabilities and choice definitions.
- Travel followed by event selection and successful Stealth resolution.
- Confrontation linking to the existing encounter action and starting Combat.
- Failed action execution not consuming Energy or advancing time.
- Content validation for duplicate Event IDs and unknown encounter-action references.

These are automated domain/content tests. The Godot scene, actual UI transitions, mouse interaction and a complete run through the main scene still require runtime verification in Godot 4.7.2 .NET.

## 7. Deliberately Not Implemented Yet

- Event conditions beyond the player's ability to act and the Location's event pool.
- Weighted selection, priorities, cooldown variants or multiple events per day.
- Persistent Event history, save/load or world-state consequences across restarts.
- Dialogue, social, stealth checks, investigation and other specialized resolution modes.
- A general Encounter framework for every Event.
- Multiple Event pools and authored events at every Location.
