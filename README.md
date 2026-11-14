# RPG — Adventurer Academy Prototype

Godot 4.7.2 .NET / C# project.

## Design documents

- [Game Overview](Docs/Design/GAME_OVERVIEW.md) — game concept, core experience and prototype boundaries.
- [UI Architecture](Docs/Design/UI_ARCHITECTURE.md) — main screen, sidebar, Main Panel modes and persistent character state.
- [Combat Panel Contract](Docs/Design/COMBAT_PANEL_REVIEW.md) — combat data boundary, message model, intent lifecycle and UI behavior.
- [Open Design Questions](Docs/Design/OPEN_DESIGN_QUESTIONS.md) — unresolved questions and resolved decisions.
- [Playable Journey Sprint](Docs/Design/SPRINT_PLAYABLE_JOURNEY.md) — the end-to-end academy day and Goblin encounter target.

## Run

Open the project in Godot 4.7.2 .NET and run the main scene (Scenes/Main.tscn).

## Current player journey

1. Start in the Dormitory at Day 1, 07:30.
2. Travel between the Dormitory, Courtyard, Classroom and Guild Hall. Travel has a configured base duration; Agility can reduce it.
3. Explore, rest, read and interact. Each action gets its duration and feedback from academy content data. A configured character stat can reduce its duration.
4. Investigate the rustling in the Courtyard to start the Goblin encounter.
5. Choose Attack, Defend or Dodge. CombatService validates and resolves the action, updates HP / Energy and lets the Goblin respond.
6. The completed encounter advances time by its configured duration. Victory grants 25 G.
7. Return to the Dormitory and choose **Sleep until tomorrow** to move to 07:30 on the next morning.

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
- `Data/academy_locations.json` defines locations, exits, actions, feedback and activity durations.
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

Character OOP integration and the original Goblin encounter journey were tested by the project owner in Godot 4.7.2 .NET. The current responsibility and day-time revisions still need a fresh build and runtime check. The standalone Combat Preview remains illustrative and is not the main gameplay entry point.
