# Application Architecture

**Purpose:** Application-level orchestration boundaries and MainController responsibilities.

## MainController Responsibilities

### Purpose

`MainController` is the application's composition root and orchestrator. It connects Godot UI input to application use cases and coordinates high-level transitions between Exploration and Combat.

It is not the place where gameplay rules, availability discovery or presentation mapping are implemented.

### Responsibilities

1. **Initialize and connect dependencies**
   - Load the required character and world content through their loaders.
   - Create runtime objects such as Player, DayClock and gameplay services.
   - Subscribe to UI events.

2. **Handle player-facing requests**
   - Receive a selected Exploration choice request.
   - Receive Combat commands from CombatPanel.
   - Receive Sidebar navigation requests.

3. **Orchestrate use cases**
   - Delegate Exploration choice discovery to AvailableInteractionQuery.
   - Delegate choice execution to the responsible handler/service.
   - Delegate Combat rules to CombatService.
   - Delegate Exploration/Combat visibility to GameModeController.

4. **Coordinate world context**
   - Keep the current Location ID and active Event context.
   - Supply current runtime state/context to queries.
   - Coordinate successful Travel with the responsible Location rule owner.

5. **Refresh presentation**
   - Request the current ExplorationViewModel after relevant state changes.
   - Update CharacterPanel from CharacterViewModel.
   - Pass Combat results through CombatPresentationMapper to CombatPanel.

### Responsibilities that do not belong here

| Concern | Owner |
|---|---|
| Reading Player and Location JSON | CharacterDataLoader, AcademyContent |
| Choosing contextually available Exploration choices | AvailableInteractionQuery |
| Mapping choices to grouped ViewModels | ActionChoiceMapper |
| Action duration estimates | ActionDurationEstimateMapper |
| Action validation, activity-based Fatigue accumulation and common action execution | ActionExecutionService |
| Applying Health, Fatigue, Stamina, Mood and Gold Effects | ActionEffectExecutor |
| Calculating action duration | ActivityTimeCalculator |
| Advancing the calendar | DayClock |
| Travel rules and location-state update | Location/travel rule owner |
| Combat rules and encounter state | CombatService |
| Exploration/Combat visibility | GameModeController |
| Rendering UI and clock feedback | StoryPanel, CombatPanel, CharacterPanel |

### Target Exploration request flow

```text
Loaders
  -> CharacterData / LocationContent
Runtime state + context
  -> AvailableInteractionQuery
  -> ActionChoiceMapper
  -> ExplorationViewModel
  -> StoryPanel
  -> ActionSelectionRequest (source/action/optional target)
  -> MainController
  -> responsible handler/service
  -> update runtime state
  -> query + mapper again
  -> render refreshed ExplorationViewModel
```

#### Current prototype behavior

The current implementation is transitional:
- AcademyContent loads root `selfActions` from `AcademyLocations.json`.
- MainController.ShowLocation combines Location Actions, global Self Actions and derived Travel choices.
- MainController maps that flat collection into ActionChoiceViewData and calls StoryPanel.ShowContent.
- HandleChoice resolves by action ID across Location Actions and Academy Self Actions.

This is the behavior Session 3 replaces. Rest moves to PlayerData in `Player.json`; the new query composes Player, Location and Travel choices; the Mapper creates grouped ViewModels; StoryPanel only renders those ViewModels.

#### Event context

The current Event flow is focused. MainController supplies the active Event context, the query/presentation path renders its choices, and a selected choice is routed to the Event resolution owner. Do not add normal Location/Travel choices to an unresolved Event view in Version 1.

#### Combat

`UI request → MainController → CombatService → CombatResult/Snapshot → CombatPresentationMapper → CombatPanel`

CombatService owns validation, resolution and state mutation. MainController only starts/ends the mode and refreshes the persistent Player presentation.

### Design boundary

MainController may decide **which use case/handler should receive a request** and **what high-level flow follows**. It must not decide **which world objects are present**, **whether a chest is unlocked**, **how a puzzle is solved**, **how activity-based Fatigue is calculated or Combat Stamina is spent**, **how an Effect mutates a Character**, or **how Combat damage is calculated**.

Add orchestration only when a use case needs to connect existing responsibility owners. Do not move business rules into MainController to avoid creating a new focused owner.
