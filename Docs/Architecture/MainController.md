# MainController Responsibilities

## Purpose

`MainController` is the application's composition root and orchestrator. It connects the Godot UI to the game systems and coordinates the high-level flow between exploration and combat.

It is not the place where game rules are implemented.

## Responsibilities

1. **Initialize and connect dependencies**
   - Load academy and character content.
   - Create runtime objects such as the Player, DayClock, and game services.
   - Subscribe to UI events.

2. **Handle player-facing requests**
   - Receive selected choices and action-preview confirmation / cancellation from StoryPanel.
   - Receive combat commands from CombatPanel.
   - Receive sidebar navigation requests.

3. **Orchestrate use cases**
   - Resolve the selected action in the current location.
   - Delegate action validation, Energy costs, time advancement, and Effects to ActionExecutionService.
   - Delegate combat rules to CombatService.
   - Delegate exploration/combat visibility changes to GameModeController.

4. **Coordinate world context**
   - Keep the current location ID.
   - Verify that a requested travel destination is connected.
   - Ask AcademyContent for the relevant location data.

5. **Refresh presentation**
   - Update CharacterPanel after a completed action or combat result.
   - Ask StoryPanel to render the current location and feedback.
   - Pass combat results through CombatPresentationMapper to CombatPanel.

## Responsibilities that do not belong here

| Concern | Owner |
|---|---|
| Action preview mapping and readable cost / effect presentation | ActionPreviewMapper |
| Action Energy validation and spending | ActionExecutionService |
| Applying HP, Energy, Mood, and Gold Effects | ActionEffectExecutor |
| Calculating action duration | ActivityTimeCalculator |
| Advancing the calendar | DayClock |
| Combat rules and encounter state | CombatService |
| Exploration/Combat UI visibility | GameModeController |
| Reading JSON content | AcademyContent |
| Rendering UI elements | StoryPanel, CombatPanel, CharacterPanel |

## Current request flow

### Normal Action

`UI choice → MainController → ActionPreviewMapper → StoryPanel preview → Confirm → MainController → ActionExecutionService → DayClock / ActionEffectExecutor → UI refresh`

ActionPreviewMapper reuses ActivityTimeCalculator for the exact stat-adjusted duration and ActionExecutionService for the current availability check. It maps authored effects into readable presentation text and does not mutate gameplay state. Cancel returns to the current choice list. Confirm sends the original ActionDefinition to ActionExecutionService, which validates again before spending Energy, advancing time and applying effects.

Travel follows the same preview flow, using the shared travel definition for its duration, Agility modifier and Energy cost. The destination is still validated by MainController before execution.

### Travel

`UI choice → MainController → validate connected exit → ActionExecutionService → update current location → render location`

Travel uses the shared travel definition in `academy_locations.json`, including its Energy cost and Agility time modifier.

### Combat encounter

`UI choice → MainController → ActionExecutionService / CombatService → GameModeController → CombatPanel`

The current goblin encounter is still selected through the `encounter_goblin` action ID because the prototype has only one encounter. This is a temporary routing rule. A future event/encounter definition can replace it without moving combat rules into MainController.

### Return from Combat

`CombatPanel request → MainController → GameModeController → refresh CharacterPanel and location`

## Design boundary

MainController may decide **which system should handle a request** and **what should happen next**. It should not decide **how an action costs Energy**, **how an Effect changes a Character**, **how combat damage is calculated**, or **how a mode changes panel visibility**.

When adding a feature, first add its rule to the responsible system. Add orchestration to MainController only when the high-level flow needs to connect that system to another one.
