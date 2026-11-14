# RPG — Adventurer Academy Prototype

Godot 4.7.2 .NET / C# project.

## Design documents

- [Game Overview](Docs/Design/GAME_OVERVIEW.md) — game concept, core experience and prototype boundaries.
- [UI Architecture](Docs/Design/UI_ARCHITECTURE.md) — main screen, sidebar, Main Panel modes and persistent character state.
- [Combat Panel Contract](Docs/Design/COMBAT_PANEL_REVIEW.md) — combat data boundary, message model, intent lifecycle and UI behavior.
- [Open Design Questions](Docs/Design/OPEN_DESIGN_QUESTIONS.md) — unresolved decisions and resolved design choices.
- [Playable Journey Sprint](Docs/Design/SPRINT_PLAYABLE_JOURNEY.md) — the end-to-end Goblin encounter target for this sprint.

## Run

Open the project in Godot 4.7.2 .NET and run the main scene (Scenes/Main.tscn).

## Current player journey

1. Start in the Dormitory at Day 1, 07:30.
2. Travel between the Dormitory, Courtyard, Classroom and Guild Hall; the base travel time is 10 minutes and Agility can reduce it.
3. Exploration activities advance time by their configured base duration, adjusted by the relevant character stat when a `timeModifier` is defined.
4. Investigate the rustling in the Courtyard to start the Goblin encounter.
5. Choose Attack, Defend or Dodge. The combat service validates and resolves the action, updates HP / Energy and lets the Goblin respond.
6. The completed encounter advances time by 30 minutes. Victory grants 25 G.
7. Return to the Dormitory and choose **Sleep until tomorrow** to advance to the next day at 07:30.

The right Character Panel remains visible and shows Health, Energy, Mood and Gold. The left Sidebar contains Character, Diary and System navigation; location exploration remains in the Main Panel rather than becoming a separate sidebar tab.

## Day and time prototype

- `DayClock` owns the current day and time of day. The prototype starts at Day 1, 07:30. `ActivityTimeCalculator` calculates modified durations; `DayClock` receives only the final number of minutes.
- Each exploration action declares `durationMinutes` in `Data/academy_locations.json`. A `timeModifier` can reduce that duration using a character stat. Travel is configured in the root `travel` section; its base duration is 10 minutes and Agility can reduce it. Studying field maps takes 20 base minutes and Intelligence can reduce it. Completed Goblin encounters still cost 30 minutes.
- The Dormitory provides **Sleep until tomorrow**, which advances the day and resets the clock to 07:30. Sleep currently changes time only; character recovery and daily schedules are not implemented yet.
- The clock stops at 23:59 until the player sleeps. This avoids silently rolling into a new day without the explicit sleep action.

## Data and presentation flow

- `Data/Characters/player.json` and `Data/Characters/goblin.json` define character identity, role, level, maximum values and starting values.
- `CharacterDataLoader` reads a JSON definition and creates immutable `CharacterData`.
- `Character` owns runtime state. It controls damage, resource spending, recovery and reward changes.
- `Data/academy_locations.json` defines location descriptions, exits and available exploration choices.
- `AcademyContent` loads location definitions into `LocationContent` and `StoryChoice` models.
- `StoryPanel` renders the supplied `StoryChoice` collection. It does not define location choices itself.
- `CombatService` creates state-dependent `CombatActionOption` models; `CombatActionPanel` renders those models. Combat rules and action resolution remain in the service.
- `CharacterViewModel` is a read-only presentation model created from `Character`; `CharacterPanel` does not access the domain object directly.
- `CombatPresentationMapper` maps service results into `CombatViewData` for `CombatPanel`.

## Main responsibilities

- `Scripts/Characters/CharacterData.cs`: immutable character configuration, five character attributes (Strength, Agility, Intelligence, Endurance, Wisdom), JSON loader and CharacterViewModel.
- `Scripts/Characters/Character.cs`: runtime character state and state-changing operations.
- `Scripts/Data/AcademyContent.cs`: location and exploration-choice models, activity duration modifiers and JSON content loading.
- `Scripts/Simulation/ActivityTimeCalculator.cs`: calculates final activity duration from its base duration, configured modifier and character stat.
- `Scripts/Services/CombatService.cs`: validates actions, resolves turns and grants the victory reward.
- `Scripts/Data/CombatPresentation.cs`: action request, result, safe snapshot, event and combat view-data contracts.
- `Scripts/Data/CombatPresentationMapper.cs`: maps service results into presentation messages and CombatViewData.
- `Scripts/UI/CharacterPanel.cs`: renders CharacterViewModel.
- `Scripts/UI/StoryPanel.cs`: renders StoryChoice models as exploration buttons.
- `Scripts/UI/Combat/CombatPanel.cs`: coordinates combat status, message log, action selection and the return-to-exploration action.
- `Scripts/UI/Combat/CombatStatusPanel.cs`: current round, enemy HP / condition and observed intent.
- `Scripts/UI/Combat/CombatMessageLog.cs`: bounded history and conditional auto-scroll.
- `Scripts/UI/Combat/CombatMessageEntry.cs`: Player-right, Enemy-left and System-center message presentation.
- `Scripts/UI/Combat/CombatActionPanel.cs`: renders CombatActionOption models and emits action requests.
- `Scripts/MainController.cs`: loads content, creates runtime characters, owns Exploration / Combat mode transitions, applies activity time costs and refreshes the Character Panel.

## Prototype limitations

Combat rules are intentionally small and deterministic: basic attack deals 20 damage, Goblin has 40 HP and deals 4 damage, Defend costs 1 Energy and reduces the next hit to 1, Dodge costs 2 Energy and avoids the next hit. Victory grants 25 G.

Character OOP integration and the original Goblin encounter journey were tested by the project owner in Godot 4.7.2 .NET. The attribute and activity-time modifier changes are committed but still need a fresh build and runtime check. The standalone Combat Preview remains illustrative and is not the main gameplay entry point.
