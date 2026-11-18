# Testing and Acceptance

**Purpose:** One place for automated test instructions, manual runtime verification and the repeatable Day 1 gameplay acceptance scenario.

Automated .NET tests validate domain/application behavior without launching a scene. CI also builds the Debug C# assembly, imports the project and launches the main scene plus the Combat preview in Godot 4.7.2 .NET headless mode, catching script/resource startup errors. Repository validation checks local Markdown links, resolvable heading anchors, navigation reachability, changed-path naming and C# style. Manual acceptance scenarios still validate rendered UI, input wiring and the full player journey. A recorded pass must state what evidence exists and what remains unverified.

## Automated Gameplay Tests

### Run

From the repository root:

```powershell
dotnet test .\RPG.Tests\RPG.Tests.csproj --configuration Release
```

The test project targets .NET 8 and tests domain/application logic without launching the Godot window.

### Coverage

The automated suite exercises:

- Character state, Health/death, Fatigue accumulation/recovery, Stamina boundaries, Mood and Gold.
- Activity Fatigue calculation from relative MET and actual duration, including fractional values and boundary inputs.
- Action definition loading, authored activity intensity, duration modifiers, effects and time advancement.
- Rest and Sleep recovery, resource clamping and failure behavior for dead characters.
- Combat Attack, Defend, Dodge, Catch Breath, enemy response, insufficient Stamina, victory, death and invalid requests.

Use current test output and CI runs as evidence; do not preserve stale hard-coded test counts in this document. The CI Godot smoke checks are startup/resource checks, not full gameplay acceptance.

### Gameplay rules currently asserted

1. Health = 0 means Dead and blocks gameplay actions.
2. Fatigue is a cumulative daily-life exertion meter from 0–100. It does not cause unconsciousness or block actions in this initial version.
3. Fatigue is added using `max(0, activityMet - 1.0) * actualDurationMinutes / 15`. The calculation uses actual duration after any stat-based time modifier.
4. A five-minute activity at 2.8 MET adds approximately 0.6 Fatigue; a 30-minute activity at 2.8 MET adds approximately 3.6.
5. Rest takes 30 minutes, restores 5 Fatigue, restores 3 Stamina and adds 2 Mood.
6. Sleep restores Fatigue to 0 and Stamina to maximum, and retains its configured Health/Mood recovery and NextDay transition.
7. Stamina is a separate 0–10 Combat resource. Defend costs 1; Dodge costs 2; Attack has no Stamina cost.
8. Insufficient Stamina disables/rejects only the requested defensive action. It does not make the character unconscious or end Combat.
9. Catch Breath restores up to 3 Stamina and the enemy still resolves its response turn.
10. Completing the configured Combat encounter adds its activity-based Fatigue once. Merely selecting the encounter entry action does not apply the Combat duration's Fatigue.

### Manual Godot checks

Automated tests do not launch or inspect the rendered Godot UI. Run the main scene in Godot 4.7.2 .NET and manually verify:

- The Character Panel shows Health, Fatigue, Stamina, Mood and Gold and refreshes after changes.
- A five-minute walk at 2.8 MET adds approximately 0.6 Fatigue; Travel does not spend Stamina.
- High Fatigue does not block ordinary Location Actions or Travel.
- Rest takes 30 minutes, reduces Fatigue by 5, restores up to 3 Stamina and adds 2 Mood.
- Sleep advances to the next morning and restores Fatigue/Stamina plus its configured Health/Mood values exactly once.
- Combat can start at 0 Stamina. Attack and Catch Breath are available.
- Defend and Dodge show their Stamina costs and are unavailable only when Stamina is insufficient.
- Catch Breath restores up to 3 Stamina and the enemy still attacks.
- Defend/Dodge at the last available Stamina do not end the encounter.
- Death at Health = 0 still ends Combat and prevents further actions.
- Returning from a completed encounter returns to the same Location and the 30-minute Combat Fatigue is applied once.

Record the tested branch/commit and attach the relevant Godot Output, screenshots or recording. A code-level unit test is not a substitute for a UI/runtime check.

### Current limitations and verification boundaries

- Fatigue penalties, exhaustion thresholds, unconsciousness and fatigue-based action blocking are intentionally deferred until the initial accumulation/recovery curve has been playtested.
- Activity MET values and the conversion scale are initial balancing parameters, not a precise physiological prediction.
- Combat Stamina currently has only Defend, Dodge and Catch Breath as resource interactions. Future skills/actions may require additional costs and recovery rules.
- The .NET test project does not launch Godot. CI headless smoke checks load the main scene and Combat preview but do not inspect rendered controls, validate mouse/input wiring or complete the full player journey.

## Day 1 — The First Day at Adventurer Academy

**Interaction contract:** Selecting an action executes it immediately. A duration estimate is informational; there is no Preview / Confirm / Cancel step.

**Verification note:** This document defines an acceptance scenario. It is not evidence that the current `main` build has completed Godot runtime acceptance.

**Purpose:** A repeatable narrative and gameplay acceptance scenario for Version 1.  
**Related plan:** [Version 1 Session Plan](Version1Plan.md)  
**Starting point:** Main scene, Dormitory, Day 1 at 07:30.  
**Scenario status:** Proposed test content. Existing behavior and planned Version 1 additions are disting## Day 1 — The First Day at Adventurer Academy

**Interaction contract:** Selecting an action executes it immediately. Duration estimates are informational; there is no Preview / Confirm / Cancel step.

**Verification note:** This is a repeatable acceptance scenario, not evidence that the current branch has passed Godot runtime verification.

**Purpose:** Validate a complete first-day journey using Fatigue and Combat Stamina.  
**Related plan:** [Version 1 Session Plan](Version1Plan.md)  
**Starting point:** Main scene, Dormitory, Day 1 at 07:30.  
**Scenario status:** Proposed acceptance scenario.

### 1. Initial state

| Field | Value |
|---|---:|
| Day / time | Day 1, 07:30 |
| Location | `dormitory` |
| Character | Ari |
| Health | 20 / 20 |
| Fatigue | 0.0 / 100 |
| Stamina | 8 / 10 |
| Mood | 70 / 100 |
| Gold | 20 |

Ari has Agility 30 and Intelligence 25. Travel takes 9 minutes after the Agility modifier; studying field maps takes 18 minutes.

### 2. Core journey

#### Scene 1 — A letter on the desk

Read the letter (`inspect_letter`) in Dormitory. It takes 10 minutes at 1.3 MET, adds approximately 0.2 Fatigue and has no fixed resource cost. Expected clock: 07:40.

#### Scene 2 — Travel to the Courtyard

Travel takes 9 actual minutes at 2.8 MET, adds approximately 1.08 Fatigue (cumulative 1.28) and does not spend Stamina. Expected clock: 07:49.

The Courtyard Goblin Event retains its authored 50% chance and is attempted at most once per Location per day.

#### Event branch A — Confront the Goblin

The Event delegates to `encounter_goblin`. Entry has zero duration and no fixed resource cost; it does not apply the full Combat Fatigue.

For the deterministic reference path, use Attack twice. Ari deals 20 damage per Attack; the Goblin has 40 Health and deals 4 damage in its response between attacks. The second Attack defeats it before another response. Victory grants 25 Gold; Ari ends Combat with 16 Health. The completed 30-minute Encounter at 5 MET adds 8 Fatigue exactly once. Attack does not consume Stamina, so Ari retains 8 Stamina. Expected clock after Combat: 08:19.

#### Event branch B — Sneak past the Goblin

Sneak takes 10 minutes at 2.0 MET, adds approximately 0.67 Fatigue (cumulative 1.95), and does not spend Stamina. Expected clock: 07:59.

#### Event branch C — No Event

If the 50% roll fails, the player remains in Exploration. The failed Event roll changes no time, Fatigue, Stamina, Health, Mood or Gold.

### 3. Return and Rest

Travel from Courtyard to Dormitory takes 9 minutes and adds approximately 1.08 Fatigue. Rest then takes 30 minutes at 1.0 MET, so it adds no activity Fatigue. Its Effects recover up to 5 Fatigue, restore up to 3 Stamina and add 2 Mood, with values clamped.

Expected state after Rest:

| Event outcome | Clock | Health | Fatigue | Stamina | Mood | Gold |
|---|---:|---:|---:|---:|---:|---:|
| Confronted Goblin (Attack-only path) | 08:58 | 16 | 5.36 | 10 | 72 | 45 |
| Sneaked past | 08:38 | 20 | 0 | 10 | 72 | 20 |
| No Event | 08:28 | 20 | 0 | 10 | 72 | 20 |

These values assume no optional actions before Rest. Combat paths using Defend, Dodge or Catch Breath may produce a different Stamina value but must follow the same resource rules.

### 4. Briefing and optional activities

Travel to the Practical Classroom and talk to the instructor. The 20-minute conversation at 1.5 MET adds approximately 0.67 Fatigue. Studying field maps takes 18 minutes for Ari and accumulates Fatigue using its authored activity intensity.

Optional future activities (Cafeteria, Lesson, Practice and Infirmary treatment) must define `activityMet`; daily activities do not spend Combat Stamina. Their tuning values belong to the Session that introduces them.

### 5. Sleep and next day

Sleep from the Dormitory using `NextDay`. Wake at Day 2, 07:30; restore Fatigue to 0 and Stamina to 10; restore up to 20 Health and apply the configured +10 Mood. Apply recovery exactly once through Action Effects.

| Event outcome | Day / time | Health | Fatigue | Stamina | Mood | Gold |
|---|---|---:|---:|---:|---:|---:|
| Confronted Goblin | Day 2, 07:30 | 20 | 0 | 10 | 82 | 45 |
| Sneaked past | Day 2, 07:30 | 20 | 0 | 10 | 82 | 20 |
| No Event | Day 2, 07:30 | 20 | 0 | 10 | 82 | 20 |

### 6. Required acceptance coverage

- Valid actions apply actual duration, activity-based Fatigue and configured Effects exactly once.
- A rejected action request does not change resources, time or Location.
- Ari's Travel takes 9 minutes and adds approximately 1.08 Fatigue.
- Confront entry does not prematurely apply Combat duration Fatigue.
- Completing the 30-minute Combat adds 8 Fatigue exactly once, on victory or defeat.
- Sneak takes 10 minutes and adds approximately 0.67 Fatigue without spending Stamina.
- Rest recovers up to 5 Fatigue, 3 Stamina and 2 Mood.
- Sleep advances one day to 07:30 and restores Fatigue/Stamina plus configured Health/Mood Effects exactly once.
- The Event's 0, 0.5 and 1 probability cases are tested deterministically; production chance remains 50%.
- Save/Load acceptance is added when that feature is implemented; this scenario does not imply persistence is already shipped.

### 7. Verification boundary

Run the three Event paths in Godot 4.7.2 .NET. Capture opening state, Travel/Fatigue changes, Event outcome, Combat or Sneak result, Rest, Sleep and next-day state. Automated tests do not validate rendered UI, input wiring or scene lifecycle.

### 8. Out of scope

New quests, NPC schedules/relationships, inventory, shops, crafting, XP/skills, dungeons and expanded Combat mechanics remain outside this first-day acceptance scenario.
