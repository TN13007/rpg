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
   - Receive selected action choices from StoryPanel.
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
| Action duration estimate mapping | ActionDurationEstimateMapper |
| Action Energy validation and spending | ActionExecutionService |
| Applying HP, Energy, Mood, and Gold Effects | ActionEffectExecutor |
| Calculating action duration | ActivityTimeCalculator |
| Advancing the calendar | DayClock |
| Combat rules and encounter state | CombatService |
| Exploration/Combat UI visibility | GameModeController |
| Reading JSON content | AcademyContent |
| Rendering UI elements and clock feedback animation | StoryPanel, CombatPanel, CharacterPanel |

## Current request flow

### Normal Action

`UI choice with estimated duration → MainController → ActionExecutionService → DayClock / ActionEffectExecutor → UI refresh`

ActionDurationEstimateMapper converts authored estimate bounds into a compact label beside each choice. It reuses ActivityTimeCalculator so stat modifiers affect both ends of the displayed range. The mapper does not change gameplay state. Selecting a choice executes it immediately; ActionExecutionService remains responsible for validating the actor, spending Energy, advancing time and applying effects. After success, MainController passes the DayClock values from before and after execution to StoryPanel. StoryPanel briefly displays the elapsed time beside the clock and animates the clock display to its new value. The elapsed-time feedback is not added to the Story Log.

Travel uses the same choice presentation and executes immediately after MainController validates that the destination is connected. Its actual duration still comes from the shared travel definition and ActivityTimeCalculator.

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
