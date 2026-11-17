# Version 1 — Session-by-Session Implementation Plan

**Parent roadmap:** [Game Development Roadmap](GAME_DEVELOPMENT_ROADMAP.md)  
**Version:** 1 — A Day at the Academy  
**Status:** Detailed implementation proposal  
**Engine:** Godot 4.7.2 .NET / C#  
**Delivery rule:** Every session ends with a runnable feature in the Godot main scene.

---

## 1. Version Goal

Version 1 turns the existing academy prototype into a small but complete daily-life loop.

At the end of this version, the player should be able to:

1. Understand the current day, time, location and character condition.
2. Choose an activity after seeing its time and resource costs.
3. Spend and recover Energy through daily activities.
4. Sleep and begin the next day with recovered Health, Energy and Mood.
5. Use a small set of academy activities to make meaningful choices about time and wellbeing.
6. Finish a day, understand what happened and continue.
7. Save and load the current daily-life state.

This version is deliberately not a character-progression release. It does not add skill XP, levels, skill trees, relationship progression, a general NPC schedule simulator, inventory, crafting, quests or a world simulation. Those belong to later versions.

## 2. What Already Exists

The plan must extend the current implementation rather than rebuild it.

The repository currently records:

- DayClock tracks day and minute, rolls over at midnight and supports SleepUntilNextDay().
- Character owns Health, Energy, Mood, Gold and alive/unconscious state.
- Health is bounded by 0..MaxHealth; Energy by 0..MaxEnergy; Mood by 0..MaxMood.
- A character with 0 Health is dead. A living character with 0 Energy is unconscious and cannot act.
- ActionDefinition already contains duration, Energy cost, time modifier, time mode and Effects.
- ActivityTimeCalculator calculates duration after an optional stat modifier.
- ActionEffectDefinition currently describes RestoreHealth, RestoreEnergy, ChangeMood and AddGold.
- academy_locations.json already contains actions such as resting, reading a letter, talking to Mira, inspecting the notice board, studying maps and sleeping.
- The Dormitory rest action is authored as 30 minutes, restoring 3 Energy and 2 Mood.
- The sleep action is authored with NextDay time mode and effects to restore 20 Health, 10 Energy and 10 Mood.
- The current player data starts at 20/20 Health, 8/10 Energy, 70/100 Mood and 20 Gold.
- The UI already shows Health, Energy, Mood and Gold in the persistent Character Panel. The current time is passed into the Main Panel.

### Important baseline discrepancy to resolve

The README says that sleep currently changes time only and does not recover character values. However, academy_locations.json already authors Health, Energy and Mood recovery effects for the sleep action.

Therefore, Session 1 must verify the actual runtime behavior before changing the sleep implementation. The source of truth must be the executed code and a Godot runtime test, not an assumption based on either document. If the effects already work, preserve them and update the stale documentation; do not implement the same behavior twice.

## 3. Player Experience and Initial Rules

The following rules define the proposed Version 1 baseline. Values already present in character data or action content should be preserved unless a runtime test shows they are invalid. New numeric values are initial tuning candidates and must be playtested.

### 3.1 Character indicators

| Indicator | Range | Meaning in Version 1 | Gameplay consequence |
|---|---:|---|---|
| Health | 0–20 for the current player | Physical condition and survival | At 0, the character is dead and cannot perform gameplay actions. |
| Energy | 0–10 for the current player | Capacity to continue activities | At 0 while alive, the character is unconscious and cannot act until Energy is restored. |
| Mood | 0–100 | Emotional wellbeing | Activities can raise or lower it. Mood bands change the displayed condition and feedback; they do not silently change action success or costs in Version 1. |
| Gold | Current integer value | Existing currency | Preserve current behavior. Do not introduce a shop or economy loop in this version. |

The current player values are content configuration, not universal constants. Tests must also use characters with different maximum values.

### 3.2 Mood bands

Use four presentation bands as an initial, easily readable interpretation:

| Mood | Label | Presentation |
|---:|---|---|
| 0–24 | Very Low | Character appears emotionally drained. |
| 25–49 | Low | Character is not feeling well. |
| 50–74 | Steady | Ordinary emotional condition. |
| 75–100 | Positive | Character is in good spirits. |

These bands are descriptive in Version 1. Do not add hidden combat penalties, skill penalties, action lockouts or probability modifiers based on Mood. If playtesting shows that Mood should affect mechanics, define that as a separate, explicit design decision.

### 3.3 Activity execution contract

Every activity must follow the same visible and domain-level sequence:

1. The player sees the activity's description and approximate duration beside its choice.
2. The player selects the activity.
3. The game validates that the character is alive, conscious and has enough Energy.
4. If validation fails, no time, Energy, Health, Mood or Gold is changed and a clear reason is displayed.
5. If validation succeeds, the action's costs, actual calculated time and effects are applied exactly once.
6. The Character Panel and Main Panel refresh from the resulting state.
7. The player receives a clear result message.

The operation must be atomic from the player's perspective. A failed action must never spend Energy but fail to advance time, or advance time but fail to apply the expected effects.

For NextDay actions, the action must use the dedicated next-day transition rather than adding an arbitrary duration and then separately incrementing the day. Recovery effects must be applied exactly once.

---

# 4. Session Plan

## Session 1 — Inline Action Duration Estimates

**Player-facing result:** The player sees an approximate duration beside each action and performs the action immediately by selecting it.

### Why this is first

The game already has timed actions. The player should understand the time commitment without leaving the story flow or opening a separate confirmation screen. A compact estimate beside the action gives useful planning information while keeping interaction immediate.

### Player flow

1. The player opens the Dormitory.
2. The player sees choices such as:
   - Read the letter on your desk (0:10–0:20).
   - Take a short rest (0:30).
   - Sleep until tomorrow (Until next morning).
3. The player selects an action.
4. The action executes immediately. The system validates the character, spends Energy, advances the actual calculated duration and applies effects.
5. The clock, Character Panel and story feedback update.

### Scope

- Remove the separate Action Preview screen and Confirm / Cancel interaction.
- Display a compact duration label directly beside each action choice.
- Support optional authored estimate bounds on ActionDefinition.
- Apply the existing stat-based time modifier to both estimate bounds.
- If an action has no estimate range, display its calculated duration as a single value.
- Display NextDay actions as “Until next morning”; do not display them as zero minutes.
- Keep Energy validation and failure feedback in ActionExecutionService.
- Selecting an unavailable action must show the rejection reason and make no state changes.
- Apply the same choice presentation to normal actions, travel and non-combat Event choices.
- Keep Combat transitions on their existing dedicated flow.
- Do not display predicted Effects, current time, resulting time or a second confirmation panel.

### Estimate semantics

The estimate is presentation guidance, not a second execution rule. Estimate bounds are authored in content and displayed to the player. The actual duration remains calculated by ActivityTimeCalculator using the action's configured base duration and stat modifier. After a successful action, the UI briefly displays the actual elapsed time beside the clock and animates the clock from its previous value to the new value. Do not repeat elapsed minutes in the Story Log.

### Implementation slices

1. Remove the ActionPreview panel, pending-action state and Confirm / Cancel signals.
2. Add optional estimated duration bounds to ActionDefinition and parse them from academy content.
3. Add ActionDurationEstimateMapper to format fixed durations, estimate ranges and NextDay labels.
4. Add ActionChoiceViewData so StoryPanel receives ready-to-render choice labels and duration text.
5. Show the estimate beside each action label and execute the selected action immediately.
6. Update travel and Event choice flows to use the same presentation and direct execution path.
7. Refresh the Main Panel and Character Panel after successful execution.

### Automated tests

- A configured estimate range is displayed with both bounds.
- Both estimate bounds use the same stat modifier as actual action duration.
- A fixed-duration action without estimate bounds displays its calculated duration.
- A NextDay action displays “Until next morning”.
- A zero-duration action displays “Immediate” or omits the duration label consistently.
- Estimate bounds must be provided together.
- Negative bounds and a maximum lower than the minimum are rejected.
- NextDay actions cannot define ordinary duration bounds.
- Mapping choices does not mutate character state or the clock.
- Insufficient Energy, unconsciousness and death are still rejected by ActionExecutionService without mutation.
- Selecting an action executes it once; there is no preview confirmation path.
- Travel and non-combat Event choices execute through their existing rule owners.

### Godot runtime acceptance

- The Dormitory action list shows “Read the letter on your desk (0:10–0:20)”.
- Rest displays its configured fixed duration.
- Sleep displays “Until next morning”.
- Selecting an action executes it immediately and updates the Character Panel.
- The clock rapidly counts from its previous time to the new time.
- The actual elapsed time appears beside the clock for approximately 5–6 seconds, then fades away.
- An unavailable action shows a clear rejection and does not change state or show time feedback.
- Travel updates the location after execution.
- The existing Goblin Event and Combat flow still work.

### Session completion evidence

A short runtime recording or screenshots showing the action list with inline estimates, immediate action execution, updated clock and Character Panel, plus automated test results.

---

## Session 2 — Energy Economy and Rest

**Player-facing result:** The player can spend Energy on activities, recognize exhaustion and recover through rest.

### Why this is second

Energy already limits actions and 0 Energy already means unconsciousness. This session turns that existing rule into a complete, understandable gameplay loop instead of an isolated validation condition.

### Player flow

1. The player begins with 8/10 Energy.
2. The player performs an activity that costs Energy.
3. The Energy display decreases by the configured cost.
4. When Energy is insufficient, the activity is rejected with a clear reason and no state changes.
5. The player returns to the Dormitory and takes a short rest.
6. Energy increases by the authored recovery amount, up to MaxEnergy.
7. The player can continue activities.

### Scope

- Audit and consolidate Energy validation in the existing action execution path.
- Ensure all normal activities use ActionDefinition.EnergyCost.
- Ensure an action with a cost greater than current Energy is rejected before any mutation.
- Preserve the current rule: a living character at 0 Energy is unconscious and cannot act.
- Preserve the current rest action as the first Energy recovery activity, unless runtime verification finds that it does not execute correctly.
- Display current Energy and maximum Energy consistently.
- Make the failure message actionable, e.g. “Not enough Energy. Rest before continuing.”
- Ensure a character who becomes unconscious after an action cannot immediately perform another action.

### Initial tuning

Keep the current authored values as the initial baseline:

- Starting Energy: 8/10.
- Short rest: 30 minutes, 0 Energy cost, +3 Energy, +2 Mood.
- Travel: preserve the current configured cost.
- Other activity costs: preserve existing JSON values initially.

Do not rebalance all costs in this session. First make the rules correct and observable.

### Automated tests

- Cost 0 succeeds when the character can act.
- Cost equal to current Energy succeeds and reaches 0.
- Cost greater than current Energy fails without mutation.
- Negative Energy costs are rejected by content/domain validation.
- Rest at maximum Energy does not exceed maximum.
- Rest from 0 Energy restores the character and makes them able to act.
- A dead character cannot use rest to revive.
- An action that reduces Energy to 0 applies its configured effects/time once, then the character becomes unconscious.
- A second action after unconsciousness is rejected.
- Failure does not change Energy, Health, Mood, Gold or time.
- Very large recovery values are clamped to MaxEnergy.

### Godot runtime acceptance

- Perform an Energy-consuming activity and observe the decrease.
- Attempt another activity without enough Energy and observe a rejection.
- Rest and observe recovery.
- Reach 0 Energy through a valid action and verify that further actions are blocked until recovery.

### Session completion evidence

A reproducible scenario showing Energy decrease → insufficient Energy rejection → rest → recovery → action becomes available again.

---

## Session 3 — Sleep and the Next-Day Transition

**Player-facing result:** The player can end the day, sleep and start the next morning with a recovered character.

### Why this is third

Rest handles short-term recovery. Sleep establishes the daily boundary that makes the academy life loop meaningful. It also becomes the foundation for schedules and world progression in later versions.

### Player flow

1. The player returns to the Dormitory.
2. The player chooses “Sleep until tomorrow”.
3. The UI explains that the action ends the current day and shows the expected recovery.
4. The player confirms.
5. The day advances to the next morning at 07:30.
6. Health, Energy and Mood recover according to the authored sleep effects.
7. The location remains the Dormitory and the Main Panel displays the new day.

### Scope

- Verify and complete the existing NextDay action execution.
- Use DayClock.SleepUntilNextDay() as the single owner of the calendar transition.
- Apply the sleep action's configured effects exactly once.
- Keep the current wake-up time at 07:30 unless a later design decision changes it.
- Correctly handle sleeping before 07:30, at 07:30, after 07:30 and after midnight.
- Ensure sleep cannot be performed from an unavailable location unless the action is explicitly authored there.
- Ensure death cannot be bypassed by sleeping.
- Refresh all UI from the post-sleep state.

### Initial recovery values

Preserve the current JSON configuration as the first candidate:

- Restore up to 20 Health.
- Restore up to 10 Energy.
- Restore up to 10 Mood.
- Wake at 07:30 on the next day.

The actual restored amount is limited by the character's missing amount and maximum value.

### Calendar rules

- If the player sleeps at or after 07:30, move to the next day at 07:30.
- If the clock is before 07:30 because the player has passed midnight, set the clock to 07:30 without incrementing the day a second time.
- Sleeping must not create two day increments.
- A failed sleep action must not advance the day or apply recovery.
- The day number must not silently overflow.

### Automated tests

- Sleep from 07:30 advances exactly one day.
- Sleep from noon advances exactly one day.
- Sleep from 23:59 advances exactly one day.
- Sleep at 02:00 after midnight sets 07:30 without a second increment.
- Sleep at 07:29 follows the documented boundary rule.
- Sleep restores Health, Energy and Mood up to their maxima.
- Sleep at full values does not exceed maxima.
- Sleep does not revive a dead character.
- Sleep recovery effects are not applied twice.
- A failed or invalid sleep action changes nothing.
- The location remains the Dormitory after waking.

### Godot runtime acceptance

Complete one ordinary day, sleep, and verify the day number, 07:30 time, character values and location in the running game.

### Session completion evidence

A before/after state record with day, time, location, Health, Energy and Mood, plus automated boundary test results.

---

## Session 4 — Health, Injury and Recovery

**Player-facing result:** Damage matters beyond the combat screen, and the player has a daily-life way to recover from injury.

### Why this is fourth

Health already represents survival and Combat can reduce it. Without a recovery option outside sleep, a player who survives an encounter may have no meaningful way to manage the injury during the rest of the day. This session closes that gap without adding a medical simulation.

### Player flow

1. The player takes damage during the existing Goblin encounter.
2. The player returns to Exploration with reduced Health.
3. The player travels to the academy's infirmary.
4. The player selects a basic treatment/rest action.
5. The action consumes its configured time and restores a limited amount of Health.
6. The Character Panel shows the updated Health.

### Scope

- Add one small Infirmary Location connected to the existing academy location graph.
- Add one basic treatment action with an authored duration and Health recovery effect.
- Use the existing Action execution and Effect application path.
- Make treatment unavailable to a dead character.
- Do not allow treatment to exceed MaxHealth.
- Keep treatment's Health recovery in the action result feedback; do not add a separate preview screen.
- Keep the existing combat death rule unchanged.
- Keep the feature intentionally small: no doctor NPC, medicine inventory, diagnoses, injuries list or medical minigame.

### Initial tuning candidate

Use a simple fixed treatment action for the first playable implementation, for example:

- Treatment duration: 60 minutes.
- Energy cost: 0.
- Health recovery: up to 5 points.
- No Gold cost in Version 1.

These are prototype tuning values, not final economy decisions. The academy-provided treatment avoids introducing shopping or inventory dependencies before Version 4.

### Automated tests

- Treatment restores the configured amount when injured.
- Treatment is clamped at MaxHealth.
- Treatment at full Health does not create extra Health.
- Dead characters cannot be treated or revived.
- Invalid location/action references are rejected during content loading.
- Treatment advances time exactly once.
- Failed treatment does not advance time or change Health.
- Treatment works after returning from Combat.
- Treatment does not alter Energy, Mood or Gold unless explicitly configured.

### Godot runtime acceptance

Take damage in Combat, return to Exploration, travel to the Infirmary, receive treatment and observe Health recovery and time cost.

### Session completion evidence

A complete injury → return → travel → treatment → recovered-state runtime scenario.

---

## Session 5 — Mood and Daily Wellbeing

**Player-facing result:** Daily activities can improve or worsen the character's emotional condition, and the UI communicates that condition clearly.

### Why this is fifth

Health and Energy describe physical survival and capacity. Mood gives daily choices an emotional dimension. The first implementation should be understandable and descriptive rather than introducing hidden penalties.

### Player flow

1. The player sees the current Mood value and its descriptive band.
2. The player performs activities with authored Mood effects.
3. Mood changes within the 0–100 range.
4. The UI updates the band and contextual description.
5. The player can choose a restorative activity to improve Mood.

### Scope

- Add a Mood presentation mapper or equivalent focused logic so UI labels are not scattered across controllers.
- Use the four documented bands: Very Low, Low, Steady and Positive.
- Preserve Mood effects already authored on rest, sleep and social/context actions.
- Add one simple restorative leisure action in an existing suitable Location if the current content does not provide enough ways to change Mood.
- Show Mood change in the activity result feedback.
- Clamp all Mood changes between 0 and MaxMood.
- Do not let Mood change action success, combat damage, Energy cost or time calculation in this version.

### Initial Mood bands

- 0–24: Very Low.
- 25–49: Low.
- 50–74: Steady.
- 75–100: Positive.

The labels and descriptive copy may be revised after playtesting; the numeric boundaries should be centralized rather than repeated in UI code.

### Automated tests

- Values at 0, 24, 25, 49, 50, 74, 75 and 100 map to the correct band.
- Negative changes clamp at 0.
- Positive changes clamp at MaxMood.
- A character with a MaxMood other than 100 maps correctly if the implementation supports configurable maxima; otherwise validate and document the chosen normalization.
- Action result feedback shows the applied Mood delta.
- Mood band changes refresh immediately after an action.
- Mood does not silently alter action availability or combat calculations.
- Rest and sleep apply Mood effects exactly once.

### Godot runtime acceptance

Lower Mood through an authored activity or test scenario, use a restorative activity, and observe the value and descriptive band change in the running UI.

### Session completion evidence

A runtime example crossing at least one Mood-band boundary in each direction, with tests for all boundaries.

---

## Session 6 — A Small, Complete Academy Routine

**Player-facing result:** The player has several distinct activities and must choose how to spend a day.

### Why this is sixth

The previous sessions establish understandable costs, Energy, sleep, Health recovery and Mood. This session combines them into actual daily choices. It should feel like a day at an academy, not a test menu for character statistics.

### Player flow

The player can choose a routine such as:

1. Wake up in the Dormitory.
2. Eat a meal.
3. Attend a lesson or study in the Classroom.
4. Practice or train in an appropriate academy area.
5. Rest when Energy becomes low.
6. Visit the Infirmary if injured.
7. Return to the Dormitory and sleep.

The player is free to choose another order where location and character state allow it. This is not a fixed timetable or a quest chain.

### Scope

Add a compact set of authored activities that cover different daily decisions:

| Activity | Suggested location | Primary purpose | State interaction |
|---|---|---|---|
| Eat a meal | New Cafeteria Location or an existing suitable academy location | Restore some Energy | Time + Energy recovery |
| Attend a lesson | Classroom | Spend time on study | Time + Energy cost; no XP in Version 1 |
| Practice basic techniques | Courtyard or training area | Spend time on physical practice | Time + Energy cost; optional Mood effect |
| Take a short rest | Dormitory or another explicitly authored resting place | Recover Energy | Existing rest behavior |
| Receive treatment | Infirmary | Recover Health | Session 4 behavior |
| Sleep until tomorrow | Dormitory | End the day and recover | Session 3 behavior |

### Content constraints

- Keep activity definitions in the existing academy content JSON where that is the current project convention.
- Do not add a skill progression reward to lessons or practice. Version 2 owns skills and progression.
- Do not introduce an inventory requirement for meals. For this first version, treat the meal as an academy-provided activity; no item ownership or shopping loop is needed.
- Do not add a full timetable, NPC schedule or attendance system.
- Do not make every activity available at every Location. The current Location's authored choices determine availability.
- Keep each activity's time, Energy cost and Effects explicit in content.

### Initial tuning candidates

Use short, easy-to-test values rather than attempting final balance:

- Meal: 20 minutes; restore 2 Energy; optional small Mood improvement.
- Lesson: 60 minutes; spend 2 Energy; no XP.
- Practice: 45 minutes; spend 3 Energy; no skill gain.
- Short rest: preserve the current 30-minute / +3 Energy / +2 Mood behavior.
- Treatment: preserve Session 4 values.
- Sleep: preserve Session 3 behavior.

These values are starting points for the first playable day. Adjust only after running the same day scenario repeatedly and recording whether the player has meaningful choices.

### Automated tests

- Every activity loads from content with valid IDs, labels, duration, Energy cost and Effects.
- Duplicate activity/action IDs are rejected according to existing validation rules.
- Activities are available only at their authored Location.
- A meal restores Energy without exceeding MaxEnergy.
- A lesson or practice action fails cleanly when Energy is insufficient.
- Each successful activity advances time exactly once.
- Stat modifiers are applied consistently to displayed duration estimates and actual execution.
- Activities do not grant XP, skill levels or inventory items in Version 1.
- A sequence of activities cannot produce Health, Energy or Mood outside their valid ranges.
- Invalid content does not partially replace the already loaded content.

### Godot runtime acceptance

Play a day using at least three different activities, including one restorative activity. The player must be able to choose the order, see costs before confirmation and understand the state changes afterward.

### Session completion evidence

A short playthrough showing at least three distinct activity types and one meaningful trade-off between time and Energy.

---

## Session 7 — Daily Summary and Day Continuity

**Player-facing result:** The player can understand how the day was spent and see the consequences carried into the next morning.

### Why this is seventh

The day loop needs a readable endpoint. A summary gives the player feedback about their choices and makes the transition to the next day feel intentional. It also creates a clean place to verify the day's state before persistence is added.

### Player flow

1. The player chooses to sleep.
2. Before the next morning begins, the game shows a compact day summary.
3. The summary reports the day number, activities completed, time spent and relevant state changes.
4. The player continues to the next morning.
5. The game returns to the Dormitory with the updated state.

### Scope

- Record only the activity history required for the current day's summary.
- Include activity label, Location, time spent and resulting relevant state changes.
- Distinguish successful actions from rejected attempts; rejected actions should not appear as completed activities.
- Keep the summary bounded to the current day. A long-term journal belongs to a later version.
- Provide a clear Continue action to enter the next morning.
- Ensure the summary is generated from actual execution results, not by re-calculating effects in the UI.
- If a player dies, show the existing defeat/death state; do not invent a new resurrection flow.

### Not in scope

- Permanent diary or journal history.
- Quest tracking.
- NPC schedule history.
- Analytics dashboards.
- A calendar UI.

### Automated tests

- Each successful activity is recorded once.
- Rejected actions are not recorded as completed.
- Summary duration matches the actual clock advancement for the day.
- Multiple actions in the same location are recorded in execution order.
- Day rollover starts a new summary record.
- A NextDay action does not appear as an ordinary duration-based action.
- Summary values match the state changes reported by the action execution result.
- Empty-day summary is valid.
- Summary generation does not mutate character state or clock.

### Godot runtime acceptance

Perform several activities, sleep, inspect the summary, continue and verify that the next day starts with a clean daily record and the expected character state.

### Session completion evidence

A screenshot or recording of the summary after a day containing at least three activities, plus automated test output.

---

## Session 8 — Save and Load the Daily-Life Loop

**Player-facing result:** The player can close the game and continue the same academy day later without losing progress.

### Why this is last

Save/load should persist a coherent gameplay loop, not a collection of temporary fields that change every session. Implementing it after the daily activities and day summary are defined reduces repeated save-format changes.

### Player flow

1. The player performs activities and changes location/time/character state.
2. The player saves from the System UI.
3. The game confirms the save.
4. The player exits and starts the game again.
5. The game loads the saved state.
6. The player continues from the saved Location, day, time and character condition.

### Minimum Version 1 save scope

Persist only the state needed to continue the current daily-life loop:

- Save schema version.
- Player character ID.
- Health, Energy, Mood and Gold.
- Current Location ID.
- Day and minute of day.
- Any minimal state required to keep the current day's summary consistent.
- Any existing Location Event evaluation state that must remain consistent for the current day, if the project decides it is part of the save contract.

Do not persist immutable definitions such as character base stats or authored Location content. Reload those from the current game data and validate saved IDs against them.

### Save/load behavior

- Save data is written atomically where practical: write a temporary file, validate the result and replace the previous save only after success.
- A failed save must not report success.
- A missing save starts a new game.
- An invalid or unsupported save must show a clear error and must not partially mutate the live game state.
- A save with an unknown character or Location ID must be rejected safely.
- Clamp or reject out-of-range character values according to one documented policy; never silently accept impossible state.
- Loading must restore the same gameplay state without replaying activity effects.
- Saving during an action selection does not execute an action; actions execute only after the player selects them.
- Saving during Combat is out of scope unless the existing game explicitly supports it; Version 1 save points should be limited to safe Exploration states.

### Automated tests

- New game with no save.
- Save and load the initial state.
- Save and load after multiple activities.
- Save and load after travel.
- Save and load after midnight rollover.
- Save and load after sleep.
- Save and load with low Health, low Energy and low Mood.
- Missing save file.
- Empty, malformed and truncated save file.
- Unsupported schema version.
- Unknown character ID.
- Unknown Location ID.
- Out-of-range Health, Energy, Mood, Gold, Day or minute.
- Interrupted/failed write does not destroy the previous valid save.
- Loading does not apply recovery or action effects a second time.
- Repeated save/load cycles preserve the same state.
- Save/load does not duplicate daily summary entries.

### Godot runtime acceptance

Perform activities, travel to another Location, save, close and restart the game, load, and verify the restored Location, clock, Health, Energy, Mood, Gold and daily summary.

### Session completion evidence

A reproducible save/restart/load scenario with before/after state values and test results.

---

# 5. Session Dependency Map

The sessions are intentionally ordered so every step builds on a playable foundation.

Session 1: Inline action duration estimates
  → Session 2: Energy economy and rest
  → Session 3: Sleep and next-day transition
  → Session 4: Health and recovery
  → Session 5: Mood and wellbeing
  → Session 6: Complete academy routine
  → Session 7: Daily summary and continuity
  → Session 8: Save and load

Session 4 and Session 5 can be developed independently after Session 3, but both must be complete before Session 6 is considered complete.

# 6. Definition of Done for Every Session

A session is complete only when all applicable checks pass:

- [ ] The feature can be reached from the normal Godot main scene.
- [ ] The player can complete the described flow without developer-only shortcuts.
- [ ] The new behavior is visible and understandable in the UI.
- [ ] Existing gameplay flows still work, especially travel, Location Events and Goblin Combat.
- [ ] Domain rules are enforced outside the UI.
- [ ] Failed requests do not partially mutate state.
- [ ] Automated tests cover normal cases, invalid cases, boundaries and important combinations.
- [ ] dotnet test RPG.Tests/RPG.Tests.csproj --configuration Release passes.
- [ ] The project builds and the affected flow is verified in Godot 4.7.2 .NET.
- [ ] Relevant README/design documentation is updated.
- [ ] The issue/PR contains a concise runtime verification record.

A session is not complete when only its service or data model exists. It must have a working player flow.

# 7. Version 1 Final Acceptance Scenario

The complete Version 1 release must support this end-to-end scenario:

1. Start a new game in the Dormitory at Day 1, 07:30.
2. See approximate activity duration beside each choice.
3. Perform an activity and observe the clock and character state change.
4. Perform enough activities to make Energy management relevant.
5. Attempt an activity without sufficient Energy and verify that nothing is spent.
6. Rest and recover Energy.
7. Travel to the Courtyard and complete the existing Event/Combat or non-combat path without regressions.
8. If Health is reduced, travel to the Infirmary and recover.
9. Perform a lesson, practice or meal activity and observe the relevant state changes.
10. Observe Mood and its descriptive band.
11. Sleep in the Dormitory and wake at 07:30 on the next day with the expected recovery.
12. Review the daily summary.
13. Save, close the game, restart and load.
14. Verify that the day, time, Location, Health, Energy, Mood, Gold and daily summary are preserved.

The scenario must be repeatable from a clean save and must not depend on a random Location Event triggering. Where the Goblin Event is probabilistic, use a deterministic test seam or a dedicated test scenario for automated verification; do not weaken the production probability merely to make the test pass.

# 8. Explicitly Deferred to Later Versions

The following are intentionally excluded from Version 1:

- Skill XP, skill levels, attributes progression, skill trees and training rewards — Version 2.
- Persistent NPC population, schedules, relationship values and social progression — Version 3.
- Inventory, equipment, shops, trade, crafting and item consumption — Version 4 or later.
- Quest chains and world exploration outside the academy — Version 4.
- Multiple enemies, party combat, advanced status effects and expanded combat outcomes — Version 5.
- Tower/Floor/Room hierarchy and dungeon residents — Version 6.
- Long-term challenge progression, monster outbreaks and world-wide consequences — Version 7.
- Branching main story, seasonal content and long-term life outcomes — Version 8.

Do not pull these features into Version 1 just because they appear in the academy setting. Version 1 succeeds when one academy day becomes a coherent, replayable loop.

# 9. Open Decisions Before Implementation

These decisions should be confirmed through the first playable sessions and playtesting, not hidden inside implementation:

1. Whether the initial Infirmary treatment values (60 minutes and up to 5 Health) feel appropriate.
2. Whether the initial meal, lesson and practice costs create meaningful choices across one day.
3. Whether Mood should remain descriptive throughout Version 1 or gain one narrowly defined mechanical consequence later.
4. Whether the daily summary should appear automatically after sleep or be a separate confirmation step.
5. Whether the current Location Event evaluation state is part of Version 1 save data or should be reset by a documented new-day rule.

Unless the player experience requires a decision earlier, keep the current proposal and test it in the running game before expanding scope.
