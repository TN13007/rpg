# Testing and Acceptance

**Purpose:** One place for automated test instructions, manual runtime verification and the repeatable Day 1 gameplay acceptance scenario.

Automated tests validate domain/application behavior without launching Godot. Manual acceptance scenarios validate the actual Godot scene, UI wiring and player journey. A recorded pass must state what evidence exists and what remains unverified.

## Automated Gameplay Tests

### Run

From the repository root:

```powershell
dotnet test .\RPG.Tests\RPG.Tests.csproj --configuration Release
```

The test project targets .NET 8 and references the Godot project. It tests domain/application logic without launching the game window.

### Coverage

The automated suite currently exercises:

- Character state combinations and status precedence across Health, Energy and Mood values.
- Resource boundaries: damage, healing, Energy spending/recovery, Mood changes and Gold addition.
- Attribute-based activity time calculations.
- Action definition loading/validation and configured Location/Self Actions.
- Action execution, failure paths, state mutation and duration/time advancement.
- DayClock rollover and next-day behavior where covered by the relevant tests.
- Combat: Attack, Defend, Dodge, victory, reward, enemy response, invalid targets, insufficient Energy, death, unconsciousness and repeated actions after encounter completion.
- Boundary matrices for Health/Energy combinations and combat resource spending.

The exact test count changes as coverage grows. Use current test output and CI runs as evidence; do not preserve a stale hard-coded action or test count in this document.

### Gameplay rules currently asserted

1. Health = 0 means Dead and takes priority over Unconsciousness.
2. Health > 0 and Energy = 0 means Unconscious.
3. A dead character cannot perform actions, including Rest.
4. An unconscious character cannot perform ordinary actions, Travel or enter Combat.
5. An unconscious character may perform an Action only when its Action Definition explicitly sets `canExecuteWhenUnconscious = true`.
6. The currently configured Rest Self Action sets that flag. It takes 30 minutes, costs 0 Energy, restores up to 3 Energy and adds 2 Mood. A living character at 0 Energy can therefore Rest and become able to act again.
7. The unconscious permission does not waive an Action's Energy cost; an action that still costs Energy is rejected when the character cannot pay it.
8. A valid action may spend the character's final Energy point. That action completes, then the character becomes unconscious and cannot perform another ordinary action.
9. Defend or Dodge spending the final Energy point ends the encounter immediately; the enemy does not receive a response turn.
10. Basic Attack is rejected at Energy = 0 even though its configured Energy cost is zero, because the unconscious state rule still applies.

### Manual Godot checks

Automated tests do not launch or inspect the Godot UI. Run the main scene in Godot 4.7.2 .NET and manually verify:

- The Character Panel refreshes after Health, Energy, Mood and Gold changes.
- A normal action or Travel that spends the final Energy point completes, then displays the collapse message; attempting another ordinary action is rejected.
- At 0 Energy with positive Health, Rest remains available and can be selected from the Exploration UI; verify 30 minutes elapse, Energy increases by up to 3 and Mood increases by 2.
- At Health = 0, Rest is rejected and does not advance time or change resources.
- Ordinary actions and Travel remain unavailable while unconscious.
- Entering a Goblin encounter with exactly 2 Energy consumes the encounter cost and collapses the Player before Combat mode opens.
- Defend with exactly 1 Energy ends combat immediately without a Goblin attack.
- Dodge with exactly 2 Energy ends combat immediately without a Goblin attack.
- The combat result screen displays DEFEAT for unconsciousness and death, and VICTORY only when the enemy is defeated.
- Returning from a completed encounter returns to the same Location.
- Sleep from the Dormitory advances to the next morning and applies its configured recovery Effects once. Session 4 owns the full calendar-boundary acceptance.

Record the tested branch/commit and attach the relevant Godot Output, screenshots or recording. A code-level unit test is not a substitute for a UI/runtime check.

### Current limitations and verification boundaries

- This test project does not launch the Godot scene, inspect rendered controls or validate mouse/input wiring.
- Rest provides the current short-term recovery path for a living unconscious Player. This does not imply that a larger infirmary, collapse consequence or world-level recovery system exists.
- PR #21 and PR #22 report passing GitHub Actions .NET tests, but both PR descriptions leave Godot runtime acceptance pending. Verify the current `main` build before recording those runtime checks as complete.
- A previous owner-marked playable-journey pass is documented in `Docs/Archive/PlayableJourneyTestReport.md`; that record explicitly notes that raw logs/screenshots and per-action measurements were not retained.


## Day 1 — The First Day at Adventurer Academy

**Interaction contract:** Selecting an action executes it immediately. A duration estimate is informational; there is no Preview / Confirm / Cancel step.

**Verification note:** This document defines an acceptance scenario. It is not evidence that the current `main` build has completed Godot runtime acceptance.

**Purpose:** A repeatable narrative and gameplay acceptance scenario for Version 1.  
**Related plan:** [Version 1 Session Plan](Version1Plan.md)  
**Starting point:** Main scene, Dormitory, Day 1 at 07:30.  
**Scenario status:** Proposed test content. Existing behavior and planned Version 1 additions are distinguished below.

### 1. Scenario intent

Ari has just started life as a first-year student at Adventurer Academy. The first day introduces the ordinary rhythm of academy life: reading an instruction, moving between locations, managing Energy, meeting a small danger, attending an introductory briefing and ending the day.

The Goblin incident is deliberately small. It tests the existing Location Event and Combat flow without turning the game into a combat-first experience.

The day must remain completable whether the Courtyard event triggers or not. The event keeps its authored 50% chance; the scenario must not change the live event probability merely to make a test run convenient.

### 2. Initial state

| Field | Value |
|---|---:|
| Day / time | Day 1, 07:30 |
| Location | `dormitory` |
| Character | Ari |
| Health | 20 / 20 |
| Energy | 8 / 10 |
| Mood | 70 / 100 |
| Gold | 20 |

Ari's relevant attributes are Agility 30 and Intelligence 25. With the current travel modifier, a 10-minute journey takes 9 minutes. Studying field maps takes 18 minutes from its 20-minute base duration.

### 3. Narrative and player flow

#### Scene 1 — A letter on the desk

**Location:** Dormitory  
**Opening text:**

> Morning light slips through the curtains. Your room is quiet, and the academy day is about to begin.
>
> A letter has been left on your desk. It reminds new students to attend the academy's morning briefing and report to the instructor before the day ends.

**Player action:** Read the letter (`inspect_letter`).

Current authored result:
- Duration: 10 minutes.
- Energy cost: 1.
- Effects: none.
- Expected clock: 07:40.
- Expected Energy: 7.

The player selects the action and it executes immediately. The duration estimate helps the player understand the time commitment; it does not open a separate preview or confirmation flow.

#### Scene 2 — The courtyard

**Travel:** Dormitory → Courtyard.

Current authored travel result:
- Duration: 9 minutes for Ari.
- Energy cost: 1.
- Expected clock: 07:49.
- Expected Energy: 6.

**Arrival text:**

> Students cross the courtyard beneath old trees. Near the gate, a few students are gathering around the notice board. Mira notices you and gives a quick wave.
>
> Somewhere beyond the gate, something rustles in the bushes.

The player may optionally talk to Mira or inspect the notice board. These are existing actions, not required for the core acceptance path.

##### Event branch A — Goblin Ambush triggers

The existing `courtyard_goblin_ambush` Event has a 50% chance to trigger on the first eligible arrival at the Courtyard each in-game day.

> A goblin is rummaging through a student's dropped bag near the gate. It has not noticed you yet.
>
> You have only a moment to decide what to do.

The player chooses one of the existing Event choices.

**A1. Confront the Goblin**

- The Event delegates to the existing `encounter_goblin` action.
- Encounter entry costs 2 Energy and no additional action time.
- Combat uses the existing deterministic rules: Ari's basic attack deals 20 damage; the Goblin has 40 HP and attacks for 4 damage.
- Two basic attacks defeat the Goblin. The Goblin gets one response between those attacks.
- Expected result: Ari loses 4 Health, the Goblin is defeated, and Ari receives 25 Gold.
- Encounter duration: 30 minutes, applied when Combat ends.
- State after Combat: Health 16/20, Energy 4/10, Mood 70/100, Gold 45.

**A2. Sneak past the Goblin**

- Duration: 10 minutes.
- Energy cost: 1.
- Effects: none.
- Expected result: Ari avoids damage and receives no reward.
- State after the choice: Health 20/20, Energy 5/10, Mood 70/100, Gold 20.
- The Event resolves and the player remains at the Courtyard.

##### Event branch B — No Event triggers

If the 50% roll fails, no Event is shown. The failed attempt is not rerolled by revisiting the Courtyard on the same day.

The player remains in Exploration and can continue with normal location actions. No time, Energy, Health, Mood or Gold is changed by the failed Event roll.

#### Scene 3 — A short rest

**Travel:** Courtyard → Dormitory.

The player returns to the Dormitory to recover before continuing the day.

**Player action:** Take a short rest (`rest`).

Current authored result:
- Duration: 30 minutes.
- Energy cost: 0.
- Effects: +3 Energy and +2 Mood, clamped to their maximums.

Expected states after the rest:

| Event outcome | Clock | Health | Energy | Mood | Gold |
|---|---:|---:|---:|---:|---:|
| Confronted Goblin | 08:58 | 16 | 6 | 72 | 45 |
| Sneaked past | 08:38 | 20 | 7 | 72 | 20 |
| No Event | 08:28 | 20 | 8 | 72 | 20 |

The player can inspect the updated Character Panel and confirm that the action changed only its configured values.

#### Scene 4 — The instructor's briefing

**Travel:** Dormitory → Courtyard → Classroom.

Ari reaches the Practical Classroom.

**Opening text:**

> Field maps cover the walls. Training equipment is arranged neatly beside the instructor's desk.
>
> The instructor looks up from a stack of papers.
>
> "There you are. Your first lesson is simple: learn the academy grounds, understand your limits, and never mistake confidence for preparation."

**Player action:** Talk to the instructor (`talk_instructor`).

Current authored result:
- Duration: 20 minutes.
- Energy cost: 1.
- Effects: none.
- Feedback: the instructor asks Ari to report to the Guild Hall for a first assignment.

The player may also study the field maps (`inspect_maps`), which currently costs 2 Energy and takes 18 minutes for Ari. This is optional; it is not part of the core path because the scenario must preserve enough Energy to return and sleep.

#### Scene 5 — Return before night

**Travel:** Classroom → Courtyard → Dormitory.

Ari returns to the Dormitory. The day is complete; the instructor's words give the player a clear reason to visit the Guild Hall on a later day.

**Closing text:**

> The academy grows quieter as students return to their rooms.
>
> Your first day was not quite what you expected. You have learned where to go, met the instructor, and discovered that even the academy grounds can hold a real danger.
>
> Tomorrow, the Guild Hall will be waiting. For now, you have earned some rest.

#### Scene 6 — Sleep and continue

**Player action:** Sleep until tomorrow (`sleep`).

Current authored result:
- Time mode: `NextDay`.
- Wake-up time: 07:30.
- Effects: restore up to 20 Health, 10 Energy and 10 Mood.
- Effects are applied once by the existing action execution path.

Expected Day 2 state:

| Event outcome | Day / time | Health | Energy | Mood | Gold |
|---|---:|---:|---:|---:|---:|
| Confronted Goblin | Day 2, 07:30 | 20 | 10 | 82 | 45 |
| Sneaked past | Day 2, 07:30 | 20 | 10 | 82 | 20 |
| No Event | Day 2, 07:30 | 20 | 10 | 82 | 20 |

This is the handoff state for Day 2. Day 2 must load this state; it must not create a new Ari or reset the character to the Day 1 starting values.

### 4. Optional Version 1 activity content

These activities enrich the day after their respective Version 1 sessions are implemented. They are not part of the existing baseline and must not be treated as already playable.

| Activity | Target location | Initial proposed data | Planned session |
|---|---|---|---|
| Eat breakfast | New Cafeteria | 20 minutes, 0 Energy cost, +2 Energy | Session 6 |
| Attend introductory lesson | Classroom | 60 minutes, 2 Energy cost, no XP | Session 6 |
| Basic practice | Courtyard | 45 minutes, 3 Energy cost, no skill gain | Session 6 |
| Basic treatment | New Infirmary | 60 minutes, 0 Energy cost, restore up to 5 Health | Session 4 |

These are tuning candidates, not immutable balance values. Add only the location connections and actions needed for these concrete activities. Do not add inventory, shop, NPC schedules, XP or skill progression to support them.

The story text can be expanded when these activities are added, but the core Day 1 route must remain completable without choosing every optional activity.

### 5. Acceptance paths

The scenario has three valid core paths:

1. **Combat path:** the Courtyard Event triggers and the player confronts the Goblin.
2. **Stealth path:** the Courtyard Event triggers and the player sneaks past.
3. **Quiet path:** the Courtyard Event roll fails and the player continues without an encounter.

All three paths must reach Day 2 at 07:30 and preserve their distinct Gold / prior Health history until sleep recovery is applied.

#### Required automated coverage

- Starting state loads with the exact Day 1 values.
- Selecting a valid action executes it immediately and applies duration, Energy cost and configured Effects exactly once.
- A rejected action request changes no character state, time or location.
- Travel duration for Ari is 9 minutes and costs 1 Energy.
- The Event is attempted at most once per Courtyard per day.
- Event chance 0, 0.5 and 1 behave correctly; tests use an injected seeded `Random` for repeatability.
- Confronting starts the existing Goblin Combat and applies the 30-minute encounter duration once.
- The Goblin's first response deals 4 damage; the second player attack defeats it before another enemy response.
- Victory grants exactly 25 Gold.
- Sneaking spends exactly 1 Energy and 10 minutes, without damage or Gold.
- A failed Event roll does not mutate player state.
- Rest restores 3 Energy and 2 Mood without exceeding their maxima.
- Sleep advances to Day 2 at 07:30 and applies recovery exactly once.
- Save / Load preserves the selected branch's resulting Day 2 state without replaying actions or rewards.
- Rejected action requests are not reported as successful actions.

#### Godot runtime acceptance

Run each of the three paths from a fresh game start. Capture:
- Day 1 opening state.
- Inline duration estimate, immediate action selection and resulting clock/resource feedback.
- Courtyard Event or failed-roll Exploration.
- Combat or Stealth result where applicable.
- Rest result.
- Instructor interaction.
- Sleep transition.
- Day 2 state after a fresh application restart and Load.

The Event service uses a random source in the live game. If the event does not trigger, restart the fresh scenario and try again; do not alter the production 50% chance just to force a QA run. Automated tests must cover all outcomes deterministically.

### 6. Known implementation gaps to resolve during Version 1

1. **Grouped Exploration choices:** MainController still combines Location Actions, global Self Actions and Travel into a flat list. Session 3 proposes the contextual Query → Mapper → grouped ViewModel pipeline; this target is not implemented on `main`.
2. **Player-owned actions:** Rest remains in root `selfActions` in `AcademyLocations.json`, and Sleep remains a Dormitory Location Action. Session 3 proposes moving both to Player-owned data and enforcing the Sleep context rule.
3. **Daily activities:** Cafeteria, Infirmary, meal, lesson and practice are not present in the current academy content.
4. **Daily summary:** the current flow does not yet provide the complete activity history and daily summary described by Version 1.
5. **Save / Load:** the current prototype does not persist the complete daily-life state or the Event evaluation state.
6. **Runtime verification:** PR #21 and PR #22 record passing .NET CI but pending Godot runtime acceptance. Run affected flows in Godot 4.7.2 .NET and retain reproducible evidence before marking those runtime checks complete.

**Unconsciousness rule already implemented:** A living Player at 0 Energy may use Rest because that action explicitly sets `canExecuteWhenUnconscious: true`. Rest costs 0 Energy, takes 30 minutes, restores up to 3 Energy and adds 2 Mood. Health = 0 still means Dead and blocks Rest. Ordinary actions and Travel remain unavailable while unconscious. Sleep is not currently configured to execute while unconscious.

**Sleep behavior:** Sleep is currently authored at the Dormitory with `NextDay` and configured Health, Energy and Mood Effects. The action execution path advances the clock and applies those Effects. Session 4 must verify calendar boundaries and exactly-once behavior in runtime; do not add a second recovery implementation.

### 7. Out of scope

- New story / quest framework.
- NPC schedules or relationship progression.
- XP, levels or skill progression.
- Inventory, shops or crafting.
- Dungeon / Tower content.
- Persistent world simulation or autonomous NPC behavior.
- New combat mechanics beyond the existing Goblin encounter.
