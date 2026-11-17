# RPG — Adventurer Academy Prototype

Godot 4.7.2 .NET / C# project.

## Design documents

- [Game Overview](Docs/Design/GAME_OVERVIEW.md) — game concept, core experience and prototype boundaries.
- [Game Development Roadmap](Docs/Design/GAME_DEVELOPMENT_ROADMAP.md) — versioned product roadmap, visible player-facing milestones and exit criteria.
- [Version 1 Session Plan](Docs/Design/VERSION_1_SESSION_PLAN.md) — detailed playable sessions, implementation scope, test cases and runtime acceptance.
- [World Model Foundation](Docs/Design/WORLD_MODEL_FOUNDATION.md) — provisional gameplay taxonomy, object inventory, conceptual ERD, modeling rules and incremental plan.
- [World Model Review](Docs/Design/WORLD_MODEL_REVIEW.md) — review of NPC, Object and Location against three concrete prototype scenarios.
- [UI Architecture](Docs/Design/UI_ARCHITECTURE.md) — main screen, sidebar, Main Panel modes and persistent character state.
- [Combat Panel Contract](Docs/Design/COMBAT_PANEL_REVIEW.md) — combat data boundary, message model, intent lifecycle and UI behavior.
- [Open Design Questions](Docs/Design/OPEN_DESIGN_QUESTIONS.md) — unresolved questions and resolved decisions.
- [Playable Journey Sprint](Docs/Design/SPRINT_PLAYABLE_JOURNEY.md) — the end-to-end academy day and Goblin encounter target.
- [Gameplay Loop and Location Events](Docs/Design/GAME_LOOP_AND_LOCATION_EVENTS.md) — Event ownership, selection rules, choice resolution and prototype boundaries.
- [Playable Journey QA Record](Docs/QA/PLAYABLE_JOURNEY_TEST_REPORT.md) — owner-recorded runtime verification status and evidence limitations.
- [Day 1 Playable Scenario](Docs/QA/DAY_1_PLAYABLE_SCENARIO.md) — narrative, expected state transitions and acceptance paths for the first complete academy day.

## Run

Open the project in Godot 4.7.2 .NET and run the main scene (Scenes/Main.tscn).

## Current player journey

1. Start in the Dormitory at Day 1, 07:30.
2. Travel between the Dormitory, Courtyard, Classroom and Guild Hall. Travel has a configured base duration; Agility can reduce it.
3. Explore, rest, read and interact. Each action gets its duration and feedback from academy content data. A configured character stat can reduce its duration.
4. After a successful arrival at a Location, evaluate its authored Event pool if the player can act. Each Location is evaluated at most once per in-game day; the first successful event chance in JSON order is selected.
5. At the Courtyard, the Goblin Ambush has a 50% chance to appear. Choose to confront the Goblin or sneak past it. Confronting starts the dedicated Combat mode; sneaking spends 1 Energy and 10 minutes.
6. Combat offers Attack, Defend or Dodge. CombatService validates and resolves the action, updates HP / Energy and lets the Goblin respond.
7. The completed encounter advances time by its configured duration. Victory grants 25 G; defeat ends the encounter and the character cannot continue while dead.
8. Return to Exploration at the same location, continue the day, or return to the Dormitory and choose **Sleep until tomorrow** to move to 07:30 on the next morning.

A character with 0 HP is dead and cannot start an encounter or perform gameplay actions. The current prototype does not yet implement rebirth, ghost play or switching to another character.

The right Character Panel remains visible and shows Health, Energy, Mood and Gold. The left Sidebar contains Character, Diary and System navigation; location exploration remains in the Main Panel rather than becoming a separate sidebar tab.

## Day and time prototype

- `DayClock` owns the calendar day and minute of day. The prototype starts at Day 1, 07:30.
- `ActivityDefinition` stores a base duration and optional stat modifier. `ActivityTimeCalculator` calculates the final duration for an activity.
- Location actions, travel and combat encounter duration are configured in `Data/academy_locations.json`.
- The clock rolls over automatically at midnight, increments the day and preserves remaining minutes.
- Sleeping moves the player to 07:30 on the next morning. If midnight has already passed, sleeping does not increment the day a second time.
- Sleep currently changes time only; character recovery and daily schedules are not implemented.

## Data and presentation flow

- `Data/Characters/player.json` and `Data/Characters/goblin.json` define character identity, role, level, maximum values, starting values and attributes.
- `CharacterDataLoader` reads a JSON definition and creates immutable `CharacterData`.
- `Character` owns runtime state and state-changing operations.
- `Data/academy_locations.json` defines locations, exits, actions, feedback, activity durations and each Location's Event pool.
- `LocationEventService` evaluates a Location's Event pool once per day and selects the first successful event in authored order. Event definitions and choices are validated while AcademyContent loads.
- `AcademyContent` loads location and activity definitions, then validates duplicate IDs and location exit references before publishing the content.
- `StoryPanel` renders the supplied `StoryChoice` collection. It does not define location choices itself.
- `CombatService` validates combat actions, enforces the alive/dead rule, resolves turns and grants the victory reward.
- `CharacterViewModel` is a read-only presentation model created from `Character`; `CharacterPanel` does not access the domain object directly.
- `CombatPresentationMapper` maps service results into `CombatViewData` for `CombatPanel`.

## Main responsibilities

- `Scripts/Characters/CharacterData.cs`: immutable character configuration, character attributes and JSON loading.
- `Scripts/Characters/Character.cs`: runtime character state and state-changing operations.
- `Scripts/UI/CharacterViewModel.cs`: read-only character data for UI presentation.
- `Scripts/Data/AcademyContent.cs`: activity definitions, location and choice models, and academy content loading/validation.
- `Scripts/Simulation/ActivityTimeCalculator.cs`: calculates final activity duration from an ActivityDefinition and character stats.
- `Scripts/Simulation/DayClock.cs`: advances calendar time and handles midnight rollover.
- `Scripts/Services/LocationEventService.cs`: evaluates Location Event pools and selects an eligible Event.
- `Scripts/Data/LocationEventDefinition.cs`: immutable Location Event and choice definitions.
- `Scripts/Services/CombatService.cs`: validates actions, enforces character life state, resolves turns and grants rewards.
- `Scripts/Data/CombatPresentation.cs`: action request, result, safe snapshot, event and combat view-data contracts.
- `Scripts/Data/CombatPresentationMapper.cs`: maps combat events and snapshots into presentation messages and CombatViewData.
- `Scripts/UI/CharacterPanel.cs`: renders CharacterViewModel.
- `Scripts/UI/StoryPanel.cs`: renders StoryChoice models as exploration buttons.
- `Scripts/UI/Combat/CombatPanel.cs`: coordinates combat status, message log, action selection and the return-to-exploration action.
- `Scripts/UI/Combat/CombatStatusPanel.cs`: current round, enemy HP / condition and observed intent.
- `Scripts/UI/Combat/CombatMessageLog.cs`: bounded history and conditional auto-scroll.
- `Scripts/UI/Combat/CombatMessageEntry.cs`: Player-right, Enemy-left and System-center message presentation.
- `Scripts/UI/Combat/CombatActionPanel.cs`: renders CombatActionOption models and emits action requests.
- `Scripts/MainController.cs`: initializes game systems and orchestrates Exploration / Combat mode transitions and player actions.

## Prototype limitations

Combat rules are intentionally small and deterministic: basic attack deals 20 damage, Goblin has 40 HP and deals 4 damage, Defend costs 1 Energy and reduces the next hit to 1, Dodge costs 2 Energy and avoids the next hit. Victory grants 25 G.

Automated tests run with `dotnet test RPG.Tests/RPG.Tests.csproj --configuration Release`. The project owner closed [Issue #10](https://github.com/TN13007/rpg/issues/10) with all playable-journey runtime acceptance criteria marked complete in Godot 4.7.2 .NET. The QA record documents that owner-reported result and notes that the original Issue did not retain logs, screenshots or a run-by-run result table. The broader Sprint checklist remains a separate, wider verification scope. The standalone Combat Preview remains illustrative and is not the main gameplay entry point.
