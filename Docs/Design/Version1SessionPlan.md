# Version 1 — Session-by-Session Implementation Plan

**Parent roadmap:** [Game Development Roadmap](GameDevelopmentRoadmap.md)  
**Architecture contract:** [Exploration Action Choice Pipeline](ExplorationActionPipeline.md)  
**Version:** 1 — A Day at the Academy  
**Engine:** Godot 4.7.2 .NET / C#  
**Delivery rule:** Every session ends with a runnable feature in the Godot main scene.

## 1. Version Goal

**Learning-target approval:** All Learning objectives, Independent practices and Knowledge checks below are AI-drafted proposals. Each is marked **Proposed** and requires explicit user approval before its Session begins. They are not evidence that the learner has already demonstrated the skill.


Version 1 turns the current academy prototype into a small, complete daily-life loop. The player can choose what to do, understand the time/resource commitment, manage Health/Fatigue/Stamina/Mood, experience the current academy event/combat slice, sleep, review the day and continue after restarting the game.

Version 1 also establishes a modest, reusable Exploration choice pipeline. It must support Player Actions, Location Actions, Travel and Event Choices without making StoryPanel depend on their data source.

At the end of Version 1, the player can:
1. Understand the current day, time, Location and character condition.
2. See contextual, grouped choices and their duration/cost.
3. Perform Player Actions such as Rest and Sleep under explicit context rules.
4. Perform Location Actions and travel along valid Location exits.
5. Resolve the existing Event through a non-combat or Combat path.
6. Accumulate and recover Fatigue; manage Combat Stamina; understand death.
7. Recover Health and Mood through a small set of academy activities.
8. Complete a day, inspect a summary and continue.
9. Save and load the current daily-life state.

Version 1 is not a character-progression, social-simulation, inventory or world-exploration release. NPC schedules/relationships, item ownership and use, puzzles, containers, skill progression, towers and world simulation remain outside its implementation scope.

## 2. Status Ownership

Current implementation, active PRs, Session progress, learning evidence and documentation mismatches are tracked only in [WORKFLOW.md](../../WORKFLOW.md). This plan defines intended behavior, Session scope and acceptance criteria; it is not a second progress dashboard.

## 3. Shared Player Rules

### 3.1 Character indicators

| Indicator | Initial range | Version 1 meaning |
|---|---:|---|
| Health | 0–20 for current Player | Physical survival; 0 means dead |
| Fatigue | 0–100 | Cumulative daily-life exertion; no automatic action lock in Version 1 |
| Stamina | 0–10 | Short-term resource used by defensive Combat Actions |
| Mood | 0–100 | Emotional wellbeing; descriptive bands only in Version 1 |
| Gold | Non-negative integer | Preserve existing reward value; no shop/economy loop in Version 1 |

These values remain content-configurable. Tests must include characters with different maximums.

### 3.2 Mood bands

| Mood | Label |
|---:|---|
| 0–24 | Very Low |
| 25–49 | Low |
| 50–74 | Steady |
| 75–100 | Positive |

Mood bands are descriptive only. Do not introduce hidden penalties or action probability modifiers in Version 1.

### 3.3 Action execution contract

1. The player sees a choice and its available duration/cost information.
2. The player selects the choice.
3. The authoritative handler/service validates current state and context again.
4. A failed request changes no Health, Fatigue, Stamina, Mood, Gold, time, location or target state.
5. A successful request applies configured costs, time and Effects once.
6. Runtime state updates.
7. Query and Mapper regenerate the current choices.
8. UI renders the updated state and clear result feedback.

Presentation availability is advisory; the owning service must revalidate.

---

# 4. Session Plan

<a id="session-1"></a>

## Session 1 — Inline Action Duration Estimates


**Player-facing result:** The player sees an approximate duration beside each choice and executes it immediately by selecting it.

### Learning objective

**Approval status:** Proposed — pending user approval.



Explain the difference between a displayed duration estimate and actual execution duration; trace ActionDefinition → ActionDurationEstimateMapper → ActionChoiceViewData → StoryPanel.

### Independent practice

**Approval status:** Proposed — pending user approval.



Independently add or adjust one duration-estimate test, predict the displayed value for a stat-modified action, then compare with the test result.

### Knowledge check

**Approval status:** Proposed — pending user approval.



Explain why the mapper must not mutate state and why execution recalculates actual duration.


### Existing implementation
- Removed the full Action Preview / Confirm / Cancel screen.
- Added optional authored estimate bounds and `ActionDurationEstimateMapper`.
- Reused the character-stat duration modifier for both estimate bounds.
- Displayed NextDay as “Until next morning” and zero-duration as “Immediate”.
- Executed selected normal actions, travel and non-combat Event choices immediately.
- Added actual elapsed-time feedback beside the clock for approximately 5–6 seconds and clock count-up animation.
- Removed duplicate elapsed-minute narration from Story Log.

### Verification expected
- Fixed duration, range, stat modifier, NextDay and Immediate labels.
- Invalid estimate bounds rejected.
- Rejected actions do not advance time or show success feedback.
- Travel, Event and Goblin Combat flow remain functional.
- Godot runtime evidence should accompany the implementation PR/issue; automated tests alone do not prove rendered UI behavior.


<a id="session-2"></a>

## Session 2 — Fatigue Accumulation, Rest and Combat Stamina

**Player-facing result:** Daily activities build cumulative Fatigue according to their duration and physical intensity. Rest and Sleep recover Fatigue. Stamina is a separate resource used only by Combat Actions.

### Learning objective

**Approval status:** Proposed — pending user approval.

Trace an activity from its authored MET value through actual duration calculation to Fatigue accumulation; distinguish long-term Fatigue from short-term Combat Stamina.

### Independent practice

**Approval status:** Proposed — pending user approval.

Independently add or adjust one activity-intensity test, predict Fatigue for a short walk and a longer activity, then compare the results.

### Knowledge check

**Approval status:** Proposed — pending user approval.

Explain why a five-minute walk must not consume a fixed combat resource, and why reaching 0 Stamina must not make a living character unconscious.

### Scope

- Replace the shared Energy resource with Fatigue (0–100) and Stamina (0–10).
- Fatigue starts at 0, accumulates from activity duration and relative intensity, and is not an action lock or unconsciousness trigger in this initial version.
- Use the first-pass formula: `max(0, activityMet - 1.0) * actualDurationMinutes / 15`.
- Store Fatigue with fractional precision. Calculate it after the action's actual duration modifier is applied.
- Author `activityMet` for current Location Actions and Travel. Use MET as a relative intensity reference, not as a direct physiological prediction.
- Rest takes 30 minutes, restores 5 Fatigue, restores 3 Stamina and adds 2 Mood.
- Sleep restores Fatigue to 0 and Stamina to maximum while preserving its authored Health/Mood recovery.
- Combat uses Stamina: Defend costs 1, Dodge costs 2, Attack has no Stamina cost, and Catch Breath restores 3 Stamina while still allowing the enemy response.
- Completing the configured Combat duration adds its configured activity Fatigue exactly once.
- Keep Health = 0 as the only character state that blocks all actions. Do not add Fatigue thresholds or penalties yet.

### Automated tests

- Five minutes at 2.8 MET adds approximately 0.6 Fatigue; 30 minutes adds approximately 3.6.
- Resting-level activity (1.0 MET) and zero-duration actions add no Fatigue.
- Actual duration modifiers are applied before Fatigue is calculated.
- Fatigue and Stamina clamp at their minimum and maximum values.
- Rest and Sleep recover the configured resources exactly once.
- A living character at 100 Fatigue or 0 Stamina can still perform ordinary non-Combat actions.
- Combat can start at 0 Stamina; Attack and Catch Breath remain available.
- Insufficient Stamina for Defend/Dodge rejects only that request without consuming a turn.
- Catch Breath restores Stamina, consumes the player's turn and resolves the enemy response.
- Health = 0 still blocks actions and ends Combat on death.

### Godot runtime acceptance

- Travel between Locations does not require a fixed resource point and a short walk does not force immediate Rest.
- Observe Fatigue accumulation for Travel, reading, conversation and Study activities.
- Rest for 30 minutes and verify Fatigue -5, Stamina +3 and Mood +2, with values clamped.
- Sleep advances to the next day and restores Fatigue/Stamina and the configured Health/Mood values.
- Enter Combat with 0 Stamina; Attack and Catch Breath are still usable.
- Defend and Dodge show their Stamina requirements and become unavailable only when the current Stamina is insufficient.
- Catch Breath restores Stamina and the enemy still responds.
- Confirm that high Fatigue does not silently disable Exploration actions.

### Completion evidence

Automated test output plus a Godot recording/screenshots showing short Travel, Fatigue accumulation, Rest/Sleep recovery and Combat at 0 Stamina.

### Migration note

The original Session 2 Energy economy from PR #22 is superseded by Issue #46. Do not preserve the old rule that spending the final Energy point causes unconsciousness.

<a id="session-3"></a>

## Session 3 — Exploration Action Choice Pipeline

**Player-facing result:** Exploration presents choices in clear groups, while the same UI can render choices supplied by different data sources.

### Learning objective

**Approval status:** Proposed — pending user approval.



Trace a choice from Player/Location JSON through loading, contextual availability query, mapper, grouped ViewModel and StoryPanel; distinguish source identity from presentation group.

### Independent practice

**Approval status:** Proposed — pending user approval.



Add one small Player Action or query test, predict where it appears in Dormitory versus Courtyard, and debug the result without placing discovery logic in StoryPanel.

### Knowledge check

**Approval status:** Proposed — pending user approval.



Explain the distinct responsibilities of Loader, Query, Mapper, ViewModel, UI and execution handler.

### Why this is third
Sessions 1 and 2 already introduced action duration, costs and execution. Before adding more activities, the game needs a clear data-to-UI path. Otherwise each new action source increases controller and UI coupling. This session corrects the temporary Self Actions implementation before later sessions add more Player Actions.

### Player flow
1. At any normal Location, the player sees Personal choices such as Rest.
2. The player sees actions belonging to the current Location.
3. The player sees Travel choices derived from that Location's exits.
4. If an Event is active, its choices are presented in the focused Event context.
5. Selecting a choice routes to the responsible rule owner.
6. After a successful state change, the choice list is rebuilt from current state.

### Data sources in Version 1

| Source | Definition location | Version 1 example |
|---|---|---|
| Player Actions | `Data/Characters/Player.json` → `playerActions` | Rest, Sleep |
| Location Actions | `Data/AcademyLocations.json` → `locations[].actions` | Read letter, Inspect board, Study maps |
| Travel | Derived from `locations[].exits` plus shared Travel configuration | Go to Courtyard |
| Event Choices | `locations[].events[].choices` | Confront Goblin, Sneak past |
| NPC / Item / Interactable | Future source-specific content | Not implemented in this session |
| Combat Actions | CombatService and its own snapshot | Remain separate |

### Scope
- Add `playerActions` parsing and validation to `CharacterDataLoader` / `CharacterData`.
- Move Rest from root `selfActions` into Player Actions. Preserve 30 minutes, restore 5 Fatigue, restore 3 Stamina and add 2 Mood.
- Move Sleep's definition into Player Actions as a Player-owned operation; give it a context requirement that initially permits it only in Dormitory. Preserve NextDay and its current authored recovery Effects.
- Remove root `selfActions` after migration; avoid two competing ownership sources.
- Introduce a small Action Choice source/request contract that carries source kind, source ID, action ID and optional target/destination ID. Keep identity unambiguous.
- Introduce an Available Interaction Query for the implemented sources: Player, Location, Travel and active Event.
- Query is read-only. It distinguishes choices absent from the current context from contextual choices temporarily disabled by a resource/state condition.
- Introduce an Exploration Mapper and grouped `ExplorationViewModel` / `ActionGroupViewModel` / `ActionChoiceViewModel` (names may follow repository conventions).
- Render groups in StoryPanel. Initial groups: Personal, At This Location and Travel. Event uses a focused Current Situation group/view.
- Keep MainController as orchestrator: gather current context, submit selection request, delegate and refresh. Remove action-source merging and flat-list mapping from MainController.
- Preserve the existing ActionExecutionService, ActivityTimeCalculator, Effects, Location Event behavior and Combat pipeline.
- Document future extension points for Nearby NPC/Object interactions, but do not implement their loaders or services.

### Availability rules
- Rest is Personal and available at every Location for a living Player. Fatigue does not block it.
- Sleep is Player-owned but only appears in the Dormitory.
- Location Actions appear only at their owning Location.
- Travel appears only for valid exits.
- Event Choices appear only while their Event is active.
- Fatigue and Stamina do not block ordinary Exploration actions; a dead Player cannot execute gameplay actions.

### Automated tests
- Player Action JSON loads, validates and retains all action fields.
- Missing/malformed `playerActions`, duplicate IDs and invalid context references are handled safely.
- Rest comes from PlayerData, is visible at every Location, reduces Fatigue and restores Stamina; death still blocks it.
- Sleep comes from PlayerData and is not offered outside its allowed context.
- Location Actions are only returned for the current Location.
- Travel choices are derived only from current valid exits and retain destination identity.
- Active Event choices are mapped with stable identity and do not accidentally include ordinary Location/Travel choices.
- Query does not mutate character, clock, location or target state.
- Mapper does not read JSON or mutate runtime state.
- Empty groups are omitted and deterministic group order is preserved.
- Different sources may reuse an Action ID without causing request routing collisions.
- Invalid/stale requests are revalidated at execution and fail without partial mutation.
- Successful action refresh reflects the resulting state.

### Godot runtime acceptance
- Rest appears under Personal in Dormitory, Courtyard, Classroom and Guild Hall.
- Sleep appears only in Dormitory.
- Location-specific choices change when moving between Locations.
- Travel options reflect each Location's exits.
- Current Event screen displays Event choices using the shared presentation contract.
- Select Rest, observe 30 minutes, Fatigue -5, Stamina +3 up to maximum and Mood +2.
- Reach maximum Fatigue and 0 Stamina; confirm ordinary Exploration actions and Travel remain usable.
- Existing Goblin Event and Combat flow still work.
- UI group labels and choice ordering are readable and no stale choices remain after an action or travel.

### Session completion evidence
Automated test output plus a Godot recording/screenshots showing at least two Locations, grouped Personal/Location/Travel choices, Rest at maximum Fatigue and the Event flow.

<a id="session-4"></a>

## Session 4 — Sleep and the Next-Day Transition

**Player-facing result:** The player can end the day in the Dormitory and wake at 07:30 on the next morning with recovered character values.

### Learning objective

**Approval status:** Proposed — pending user approval.



Explain how DayClock's NextDay transition differs from ordinary minute advancement and how authored Sleep Effects are applied.

### Independent practice

**Approval status:** Proposed — pending user approval.



Independently write or extend one calendar-boundary test and verify the expected day/time.

### Knowledge check

**Approval status:** Proposed — pending user approval.



Explain how the implementation prevents double day increments and duplicate recovery.

### Scope
- Use `DayClock.SleepUntilNextDay()` as the only owner of the calendar transition.
- Use Player-owned Sleep from Session 3; enforce Dormitory context through availability and authoritative validation.
- Apply authored Sleep Effects exactly once: restore up to 20 Health, reduce Fatigue to 0, restore Stamina to maximum and restore 10 Mood.
- Keep wake-up time at 07:30.
- Keep the Player in Dormitory and refresh Character Panel and Exploration ViewModel after waking.
- A dead character cannot sleep to revive.
- Do not add schedules or world simulation.

### Calendar rules and tests
- Sleep from 07:30, noon and 23:59 advances exactly one day and wakes at 07:30.
- Sleep at 02:00 after midnight does not increment the day a second time.
- Verify 07:29 boundary behavior against the documented DayClock rule.
- Recovery clamps to each maximum.
- Full values do not exceed maxima.
- Effects are applied once; failed sleep changes nothing.
- Sleep outside Dormitory is hidden and direct stale/forged request is rejected.
- Day counter overflow is handled explicitly.

### Godot runtime acceptance
Complete a normal day, sleep and verify day, 07:30 time, Dormitory location, Health, Fatigue, Stamina and Mood.

### Completion evidence
Before/after state record plus calendar-boundary automated test output.

<a id="session-5"></a>

## Session 5 — Health, Injury and Recovery

**Player-facing result:** Combat damage has a meaningful daily-life consequence and the Player can recover at the academy Infirmary.

### Learning objective

**Approval status:** Proposed — pending user approval.



Identify the owner of Health state, trace damage/healing through the responsible rules and explain maximum-value clamping and the death boundary.

### Independent practice

**Approval status:** Proposed — pending user approval.



Add a focused Health recovery test at a boundary and explain the expected state transition.

### Knowledge check

**Approval status:** Proposed — pending user approval.



Explain why restoring Health must not silently revive a dead character unless a rule explicitly allows it.

### Scope
- Add one Infirmary Location connected to the existing Location graph.
- Add one basic treatment Location Action.
- Initial tuning candidate: 60 minutes, activity-based Fatigue gain, restore up to 5 Health, no Gold or Stamina cost.
- Reuse common action duration/cost/effect execution where applicable.
- Death cannot be bypassed by treatment.
- No doctor NPC, medicine inventory, diagnosis, injury list or medical minigame.

### Automated tests
- Treatment restores configured Health and clamps to MaxHealth.
- Full Health receives no additional Health.
- Dead Player cannot be treated/revived.
- Treatment advances time exactly once.
- Failed treatment changes nothing.
- Invalid Location/action references are rejected.
- Treatment does not alter Fatigue, Stamina, Mood or Gold unless explicitly configured.

### Godot runtime acceptance
Take damage in Goblin Combat, return to Exploration, travel to Infirmary, receive treatment and observe Health recovery/time cost.

### Completion evidence
A complete injury → return → travel → treatment → recovered-state run.

<a id="session-6"></a>

## Session 6 — Mood and Daily Wellbeing

**Player-facing result:** The Player sees a clear description of their emotional condition and understands how daily activities affect Mood.

### Learning objective

**Approval status:** Proposed — pending user approval.



Explain Mood as a runtime value, distinguish descriptive Mood bands from gameplay modifiers, and trace a Mood Effect to updated Character state.

### Independent practice

**Approval status:** Proposed — pending user approval.



Add or adjust a boundary test for Mood minimum/maximum or one authored Mood activity; predict the clamped result before running it.

### Knowledge check

**Approval status:** Proposed — pending user approval.



Explain which Mood behavior is data-driven and which remains descriptive in Version 1.

### Scope
- Centralize Mood band mapping: Very Low (0–24), Low (25–49), Steady (50–74), Positive (75–100).
- Show the band and update it after actions.
- Preserve Mood Effects on Rest, Sleep and existing suitable actions.
- Add one restorative leisure action only if current content does not provide enough meaningful Mood variation.
- Show the actual Mood change in result feedback.
- Mood remains descriptive; it does not affect action success, costs, duration or Combat in Version 1.

### Automated tests
- Test every boundary: 0, 24, 25, 49, 50, 74, 75 and 100.
- Negative/positive changes clamp to valid range.
- Configurable MaxMood is handled correctly or explicitly validated.
- UI mapping is pure and does not mutate Character.
- Rest/Sleep Mood Effects apply once.
- Mood does not silently change action availability, Fatigue calculation, Stamina or Combat calculations.

### Godot runtime acceptance
Perform actions that cross at least one Mood boundary in both directions and verify updated display and feedback.

### Completion evidence
Runtime examples plus complete Mood-boundary test results.

<a id="session-7"></a>

## Session 7 — A Small, Complete Academy Routine

**Player-facing result:** The Player has several distinct activities and meaningful decisions about how to spend one day.

### Learning objective

**Approval status:** Proposed — pending user approval.



Trace one academy activity from authored content to availability, execution, Effects, elapsed time and refreshed UI; identify each rule owner.

### Independent practice

**Approval status:** Proposed — pending user approval.



Add one concrete daily activity using existing patterns and tests, without adding a generic system or putting rules in UI.

### Knowledge check

**Approval status:** Proposed — pending user approval.



Explain why the activity belongs in its current source/service and what would justify a separate responsibility.

### Scope
Add a small set of data-authored activities:
- Meal: suitable academy Location; 20 minutes; low-intensity activity; optional small Mood increase.
- Lesson: Classroom; 60 minutes; low-intensity activity; no XP.
- Practice: Courtyard/training area; 45 minutes; higher-intensity activity; no skill gain.
- Rest: preserve Session 2 behavior.
- Treatment: preserve Session 5 behavior.
- Sleep: preserve Session 4 behavior.

Keep initial values as tuning candidates, not final balance. Do not add inventory requirements for food, XP/skills, shopping, full timetables, NPC schedules or quest chains.

### Automated tests
- All activity definitions load and validate.
- Duplicate IDs are rejected within the appropriate source scope.
- Each Location Action appears only at its authored Location.
- Activity-based Fatigue accumulation remains within 0–100.
- Lesson/Practice add Fatigue based on authored activity intensity and actual duration.
- Successful activities advance time and apply Effects once.
- Displayed duration estimates and executed duration use consistent stat modifiers.
- No XP, skill level or inventory items are granted in Version 1.
- Sequences never push Health/Fatigue/Stamina/Mood outside valid bounds.

### Godot runtime acceptance
Play a day with at least three activity types and a meaningful time/Fatigue trade-off. Player can choose the order allowed by location and state.

### Completion evidence
A playable day recording and test results.

<a id="session-8"></a>

## Session 8 — Daily Summary and Day Continuity

**Player-facing result:** The Player can review how the day was spent before continuing to the next morning.

### Learning objective

**Approval status:** Proposed — pending user approval.



Distinguish transient UI feedback, action history and a persisted daily summary; explain how the summary derives from completed gameplay events/state.

### Independent practice

**Approval status:** Proposed — pending user approval.



Add a focused test for one summary rule such as ordering, empty-day behavior or exclusion of a rejected action.

### Knowledge check

**Approval status:** Proposed — pending user approval.



Explain how the summary avoids treating failed requests or temporary clock feedback as completed activities.

### Scope
- Record successful completed activities only.
- Include label, source/context, Location, actual elapsed time and relevant state changes.
- Rejected choices are not recorded as completed activities.
- Keep the summary bounded to the current day; no permanent journal or quest tracker.
- Generate the summary from actual execution results, not by replaying Effects in the UI.
- Decide and document whether the summary is shown immediately before or after the next-day transition, then keep the flow consistent.

### Automated tests
- Each successful action is recorded once and in execution order.
- Rejected actions are not recorded.
- Total summary duration matches actual clock advancement.
- Multiple activities in one Location are recorded correctly.
- New day starts a new summary.
- NextDay is represented as a day transition, not a normal elapsed duration.
- Summary matches execution results and does not mutate state.
- Empty-day summary is valid.

### Godot runtime acceptance
Perform several activities, sleep, inspect the summary and continue. Verify next day begins with a clean daily record.

### Completion evidence
Screenshot/recording of a day containing at least three activities plus test output.

<a id="session-9"></a>

## Session 9 — Save and Load the Daily-Life Loop

**Player-facing result:** The Player can close the game and continue the same academy day later.

### Learning objective

**Approval status:** Proposed — pending user approval.



Explain serialization boundaries, save schema validation and the difference between restoring state and replaying gameplay Effects.

### Independent practice

**Approval status:** Proposed — pending user approval.



Independently add a Save/Load regression test for one persisted field or repeated load cycle, then compare the full before/after state.

### Knowledge check

**Approval status:** Proposed — pending user approval.



Explain why loading a save must not grant the same reward or recovery Effect again.

### Minimum save scope
- Schema version.
- Player ID.
- Health, Fatigue, Stamina, Mood and Gold.
- Current Location ID.
- Day and minute of day.
- Current-day summary data.
- Minimal Location Event evaluation state required to preserve the intended current-day behavior.

Do not persist immutable definitions. Reload JSON content and validate saved IDs against current definitions.

### Save/load behavior
- Write atomically where practical; do not destroy the last valid save on a failed write.
- Missing save starts a new game.
- Empty, malformed, truncated or unsupported save shows a clear failure and does not partially mutate live state.
- Unknown Player/Location IDs and out-of-range values are rejected according to a documented policy.
- Loading restores state without reapplying action Effects or duplicating summary entries.
- Save only at safe Exploration states; Combat save is out of scope.

### Automated tests
- New game without a save.
- Save/load initial state and after multiple activities.
- Save/load after travel, midnight rollover, sleep and low Health/Fatigue/Stamina/Mood.
- Missing, empty, malformed, truncated and unsupported schema files.
- Unknown IDs and out-of-range values.
- Failed/interrupted write preserves previous valid save.
- Repeated save/load cycles preserve identical state.
- Loading does not replay effects or duplicate daily summary entries.

### Godot runtime acceptance
Perform activities, travel, save, close/restart, load and verify Location, clock, Health, Fatigue, Stamina, Mood, Gold and daily summary.

### Completion evidence
Reproducible before/after save record and automated test results.

---

# 5. Dependency Map

```text
Session 1: Inline Action Duration Estimates
        |
Session 2: Fatigue Accumulation, Rest and Combat Stamina
        |
Session 3: Exploration Action Choice Pipeline
        |
Session 4: Sleep and Next-Day Transition
        |
Session 5: Health, Injury and Recovery
        |
Session 6: Mood and Daily Wellbeing
        |
Session 7: Complete Academy Routine
        |
Session 8: Daily Summary and Day Continuity
        |
Session 9: Save and Load
```

The first two sessions provide existing behavior. Session 3 corrects data ownership and creates the grouped choice pipeline that later activity additions will use. Sessions 4–9 then build the complete daily-life loop on that foundation.

## 6. Definition of Done for Every Session

- [ ] Reachable from the normal Godot main scene.
- [ ] Player can complete the described flow without developer-only shortcuts.
- [ ] New behavior is visible and understandable in the UI.
- [ ] Existing travel, Location Event and Goblin Combat flows still work.
- [ ] Gameplay rules are enforced outside the UI.
- [ ] Failed requests do not partially mutate state.
- [ ] Automated tests cover normal behavior, invalid inputs, boundaries and important combinations.
- [ ] `dotnet test RPG.Tests/RPG.Tests.csproj --configuration Release` passes.
- [ ] Project builds and affected flow is verified in Godot 4.7.2 .NET.
- [ ] Relevant README/design documentation is updated.
- [ ] Issue/PR contains a concise runtime verification record.

Code-only output is not session completion.

## 7. Version 1 Final Acceptance Scenario

1. Start a new game in Dormitory at Day 1, 07:30.
2. See grouped Personal, Location and Travel choices.
3. See duration estimates and execute a normal action.
4. Rest after accumulating Fatigue and recover the authored Fatigue/Stamina amounts.
5. Accumulate high Fatigue and spend all Stamina in Combat; verify Exploration remains available and Combat permits Attack/Catch Breath.
6. Travel between the academy Locations; confirm choices update from the current Location.
7. Complete the Courtyard Event through either Sneak or the dedicated Goblin Combat path.
8. If injured, visit the Infirmary and receive treatment.
9. Complete a meal, lesson or practice activity and observe Health/Fatigue/Stamina/Mood rules.
10. Sleep in Dormitory and wake at 07:30 on the next day.
11. Review the daily summary.
12. Save, close, restart and load.
13. Verify day, time, Location, Health, Fatigue, Stamina, Mood, Gold and summary are preserved.

The scenario must be repeatable from a clean save and must not require a random Event to trigger. Use deterministic test seams for automated Event-path verification without changing production probability.

## 8. Explicitly Deferred

- NPC population, schedules, dialogue and relationship progression — Version 3.
- Inventory, equipment, item consumption, shops and crafting — Version 4 or later.
- NPC/Item/Interactable action discovery and the Nearby group — implement when their gameplay systems are scheduled; Session 3 only establishes the contract.
- Generic puzzle framework, containers/chests and puzzle progression — not part of Version 1.
- Skill XP, levels, skill trees and Combat Action unlocks — Version 2.
- Expanded Combat, multiple enemies and party tactics — Version 5.
- Tower/dungeon hierarchy — Version 6.
- Long-term world challenge simulation — Version 7.
- Long-term stories and branching life outcomes — Version 8.

## 9. Open Tuning Decisions

1. Whether Infirmary treatment at 60 minutes / up to 5 Health feels appropriate.
2. Whether meal, lesson and practice costs produce meaningful choices across one day.
3. Whether Mood remains descriptive throughout Version 1.
4. Whether the summary appears immediately before or after the wake-up transition.
5. Which Location Event evaluation state must persist in the Version 1 save.

Keep these as explicit tuning decisions. Do not introduce hidden rules to resolve them.
