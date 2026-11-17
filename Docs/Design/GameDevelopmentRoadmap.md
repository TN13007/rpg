# Game Development Roadmap — Adventurer Academy RPG

**Project:** RPG — Adventurer Academy  
**Engine:** Godot 4.7.2 .NET / C#  
**Document status:** Proposed development roadmap  
**Planning principle:** Every version must deliver a playable, observable improvement.

## 1. Product Direction

The game is a fantasy life-simulation RPG about living as a student in an Adventurer Academy and gradually building a life in a wider fantasy world. The player studies, rests, works, develops skills, meets people, builds relationships, explores locations and sometimes faces dangerous situations.

The game is **not** a combat-first dungeon crawler. Combat, towers and dungeons are important parts of the world, but they serve the wider life-simulation experience.

The intended experience is built around five connected pillars:

1. **Daily life:** Spend time, manage Health, Energy and Mood, and choose how to live each day.
2. **Character development:** Learn, practice, gain capabilities and shape a character build.
3. **People and relationships:** Meet NPCs, understand their circumstances and develop relationships over time.
4. **World exploration:** Visit locations, discover opportunities, gather resources and encounter situations with multiple possible resolutions.
5. **Long-term consequences:** The world and its challenges can change while the player pursues other goals.

## 2. Development Principles

### 2.1 Every version must be visible and playable

A version is not complete merely because code has been written. Each version must introduce a change that the player can experience in the running game.

Every version must satisfy these criteria:

- **Playable feature:** The player can use the new feature in a real gameplay flow.
- **Visible change:** The UI, available choices, world state or player experience clearly demonstrates the addition.
- **Gameplay value:** The feature creates a meaningful decision, capability or consequence.
- **Persistence:** State that is expected to survive a scene transition, day change or restart is retained at the appropriate scope.
- **Automated tests:** Cover normal behavior, invalid inputs, boundary conditions and important combinations.
- **Acceptance evidence:** Record how the feature was verified, including automated test results and runtime evidence where relevant.

A version should end with a small, coherent playable milestone, not a collection of disconnected systems.

### 2.2 Build dependencies in a deliberate order

The development order is:

**Playable foundation → Daily life → Character growth → Social life → Wider world → Expanded encounters and combat → Tower and dungeon → Evolving world → Long-term stories.**

This order ensures that later content has something meaningful to affect. For example, NPC schedules and relationships become valuable when the player already has a daily routine; a tower outbreak becomes meaningful when the world has locations, residents and persistent consequences.

### 2.3 Prefer concrete gameplay over speculative frameworks

- Do not create a general-purpose system solely because it might be useful later.
- Validate new concepts against a concrete player scenario before introducing new abstractions.
- Keep content data-driven where appropriate: JSON defines authored content; C# owns gameplay rules and runtime behavior.
- Keep an Event distinct from an Encounter or Combat. An Event is a situation; the player may resolve it through different actions or specialized flows.
- Do not introduce an Expedition or Run wrapper around location exploration.
- Keep location-based navigation. A free-roam movement, collision or pathfinding layer is not a roadmap requirement.
- Do not simulate every NPC or monster continuously when a coarse time-step and state evaluation can produce the intended result.

### 2.4 Preserve responsibility boundaries

Use the project's responsibility-oriented design approach. Requests and UI input should be routed through clear handling and orchestration; guards and validators protect operations; queries read data; calculators compute results; maps convert data between representations; factories create objects; lifecycle, state and persistence own their respective concerns; apply operations perform domain changes.

This is a way to keep responsibilities understandable, not a requirement to create a class or system for every label.

## 3. Current Baseline — Version 0

The repository already contains a narrow playable vertical slice. Version 0 is therefore a **baseline audit and stabilization milestone**, not a claim that the game starts from zero.

### Existing capabilities recorded in the current README

- Godot 4.7.2 .NET / C# project and main scene.
- Location-based travel between Dormitory, Courtyard, Classroom and Guild Hall.
- Location actions, authored durations and feedback from JSON content.
- DayClock with day/minute tracking, midnight rollover and a next-morning sleep action.
- Character definitions loaded from JSON; runtime Health, Energy, Mood, Gold and life state.
- Activity duration calculation with a character-stat modifier.
- Location Event pools evaluated after successful arrival, at most once per Location per in-game day.
- A Courtyard Goblin Event with a choice to confront or sneak past.
- Dedicated turn-based Combat mode with Attack, Defend and Dodge.
- Combat action validation, enemy response, victory reward and death rule.
- Character Panel, exploration choices and a dedicated Combat presentation.
- Automated .NET test project and recorded owner verification of the playable journey.

### Known gaps and limits

- Sleep has authored recovery Effects in academy content; next-day boundary behavior and exactly-once runtime application are verified in Version 1 Session 4.
- A complete daily activity simulation and grouped Exploration choice pipeline are not implemented.
- Health, Energy and Mood do not yet form a complete life simulation.
- Skill progression, levels, a full inventory and equipment loop are not complete.
- NPCs are not yet implemented as a persistent population with schedules and relationships.
- Save/load and persistent world history are not established as complete gameplay features.
- Location Event attempts are runtime-only and reset when the process restarts.
- The current Combat is a deterministic single-Goblin prototype, not the final combat system.
- There is no tower hierarchy, long-term challenge simulation or world-wide consequence loop.

### Version 0 deliverable

1. Audit the current main branch and open design PRs before changing code.
2. Keep the existing playable journey working while reducing known structural friction, including the Location/Combat flow extraction tracked by Issue #15.
3. Define the minimum save/load scope needed by the next gameplay milestone; do not build a speculative universal save framework.
4. Establish reliable automated test and runtime verification evidence for the existing journey.
5. Update README and design documents so implemented behavior, intended behavior and open decisions are clearly distinguished.

**Visible result:** The existing academy-to-Goblin journey remains playable and is easier to maintain and verify. This milestone is primarily stabilization; it is not intended to introduce a large new gameplay feature.

**Exit criteria:** Existing journey acceptance criteria pass; automated tests pass; the main controller's responsibilities are clear; the baseline state and persistence gaps are documented.

## 4. Version Roadmap

## Version 1 — A Day at the Academy

**Player-facing promise:** The Player can live through a meaningful academy day, understand the choices available in each context, manage Health/Energy/Mood and continue into the next day.

### Scope

- Establish the Exploration Action Choice Pipeline: content Loaders → runtime context → Available Interaction Query → Mapper → grouped ViewModel → UI.
- Separate Player Actions, Location Actions, derived Travel Choices and Event Choices by source/ownership.
- Make activity costs explicit: time, Energy and relevant state changes.
- Establish complete initial rules for Health, Energy and Mood.
- Add a small set of academy activities and recovery options.
- Complete sleep/next-day transition, daily summary and minimum save/load.

NPC interactions, item/container interactions and puzzles are designed as future extensions, not implemented in this version. Combat retains its separate Service/Mapper/UI pipeline.

### Visible gameplay change

The Player starts a day, sees Personal, Location and Travel choices grouped in Exploration, performs activities, handles the Goblin Event/Combat slice, manages wellbeing, sleeps, reviews the day and can load the same state after restarting.

### Why this comes first

Daily life needs both a meaningful time/resource loop and a reliable way to discover contextually valid choices. Establishing the action-choice pipeline before adding more activities avoids coupling every new action to MainController and StoryPanel.

### Exit criteria

- A complete day can be played from morning to the next morning.
- Player Actions are loaded from Player data; Location Actions and Travel are sourced from the current Location; Event Choices remain contextual.
- Query determines choice availability; Mapper produces grouped ViewModels; UI only renders them.
- Actions have validated costs and predictable results.
- Health, Energy and Mood have documented rules.
- State changes are visible and persist at the intended scope.
- Tests cover insufficient resources, availability/context, invalid/stale requests, time rollover, recovery, summary and save/load.

**Detailed implementation plan:** [Version 1 — Session-by-Session Implementation Plan](Version1SessionPlan.md). The revised plan contains nine sessions. Sessions 1 and 2 have merged implementations; Session 3 establishes the grouped Exploration choice pipeline before the remaining daily-life features. Every session requires automated tests and Godot runtime acceptance; code-only output is not completion.

## Version 2 — Character Development

**Player-facing promise:** The player's choices shape what their character becomes capable of doing.

### Scope

- Define the initial attribute and skill model.
- Add experience and level progression only where they support the chosen gameplay.
- Allow learning through appropriate activities: lessons, books, practice, training and mentorship.
- Make skill or attribute changes affect relevant actions through clear rules.
- Add a Character/Skills view that explains current capabilities and progress.
- Add a small set of equipment or character-build choices only if they create meaningful differences in play.

### Visible gameplay change

Two characters who spend their time differently can develop different capabilities and unlock different options.

### Why this follows Version 1

Growth needs repeatable activities and a time economy. Implementing progression before daily activities would produce disconnected numbers without a meaningful player decision.

### Exit criteria

- At least two distinct development paths are playable.
- Progress is earned through understandable actions.
- Updated capabilities affect gameplay, not only UI numbers.
- Progress survives the relevant transitions and save/load.
- Tests cover progression thresholds, invalid rewards, repeated gains and action requirements.

## Version 3 — People at the Academy

**Player-facing promise:** The academy feels inhabited by recurring people, not a list of one-time dialogue prompts.

### Scope

- Represent NPCs as Characters with identity and relevant runtime state.
- Give a small initial cast distinct roles, goals or preferences.
- Add contextual dialogue and social actions.
- Add a first relationship model with clearly defined values and effects.
- Let selected NPCs appear at relevant locations and times.
- Add a minimal diary or journal record for important interactions.
- Support authored NPC movement or schedule transitions where needed for the first scenarios; do not require full autonomous simulation.

### Visible gameplay change

The player meets recurring NPCs, chooses how to interact, builds or damages relationships, and notices that people are not always in the same place.

### Why this follows Version 2

NPC interactions become more interesting when the player has capabilities, activities and a daily routine. The player can then choose how to spend time with people and use developed skills in social contexts.

### Exit criteria

- A small cast can be encountered repeatedly.
- At least two relationship outcomes are observable.
- NPC location/state changes are coherent and persistent.
- Dialogue choices can apply validated effects.
- Tests cover relationship boundaries, unavailable NPCs, repeated interactions and time/location conditions.

## Version 4 — Beyond the Academy

**Player-facing promise:** The player can leave the academy, visit different places and discover opportunities in a wider fantasy world.

### Scope

- Add a small connected world containing a town, wilderness area and an adventure location.
- Extend the existing Location model and navigation rather than introducing a separate travel abstraction without need.
- Add contextual actions, objects, resources and authored Events.
- Add a basic inventory and item acquisition/use loop.
- Add a small set of quests or goals that direct the player toward meaningful activities.
- Allow situations to have more than one resolution, using normal Actions or a specialized flow where justified.
- Add return travel and consequences for time spent away.

### Visible gameplay change

The player accepts an opportunity, travels outside the academy, explores a new location, gathers or earns something, resolves a situation and returns with changed state or new information.

### Why this follows Version 3

The wider world should contain people and opportunities, not merely a larger list of locations. NPCs, inventory, character capabilities and time make exploration meaningful.

### Exit criteria

- The player can travel through a small connected world.
- Each new location offers a distinct reason to visit.
- Items/resources can be acquired and used in at least one gameplay loop.
- At least one quest or goal can be completed through multiple steps.
- Tests cover travel constraints, item ownership/consumption, event resolution and return-state consistency.

## Version 5 — Combat and Companions

**Player-facing promise:** Dangerous situations become tactical decisions, with more than one participant and more than one way to resolve the danger.

### Scope

- Expand the existing CombatService and Combat presentation; do not replace the working vertical slice without a demonstrated need.
- Add target selection and support for multiple enemies.
- Add a small set of character skills and enemy behaviors.
- Add status effects only when they create distinct tactical decisions.
- Add party/companion participation and clear action ownership.
- Add item use in combat using the inventory rules from Version 4.
- Support outcomes such as victory, retreat, surrender or negotiated resolution where appropriate to the situation.
- Keep the Event → player choice → selected resolution relationship.

### Visible gameplay change

The player can enter a multi-participant tactical encounter, use developed capabilities and companions, and choose whether to fight or pursue another available resolution.

### Why this follows Version 4

Combat should consume and interact with the systems already built: character growth, inventory, companions, locations and authored situations. Expanding combat earlier would risk making the entire game revolve around a feature that is only one part of the intended experience.

### Exit criteria

- At least one multi-enemy encounter is playable.
- At least one companion can participate meaningfully.
- Actions, targets, costs and outcomes are readable.
- Retreat or another non-victory outcome is supported in a concrete scenario.
- Tests cover turn order, target validity, defeated participants, resource costs, status effects and outcome transitions.

## Version 6 — The Tower and Dungeon

**Player-facing promise:** The player can explore a substantial multi-floor location whose rooms contain different threats, discoveries, people and rewards.

### Scope

- Model the tower/dungeon as a root Location containing smaller Floor and Room Locations.
- Keep containment separate from navigation: being a child Location does not automatically define every route.
- Author a small tower with multiple floors and rooms, different layouts, connections, difficulty and rewards.
- Place monsters, items, resources, objects and NPCs in specific locations.
- Allow the player to explore, interact, fight, retreat and return later.
- Persist meaningful room state: discovered areas, collected items, defeated or moved threats, opened paths and relevant NPC presence.
- Introduce a tower-specific core or central source of danger only when the basic tower is already playable.
- Do not make every floor or room a separate scene if data-driven Location content can represent it cleanly.

### Visible gameplay change

The player can enter a tower, navigate multiple floors and rooms, discover residents and threats, leave, and later return to a world that remembers important changes.

### Why this follows Version 5

A multi-floor tower needs stable location navigation, character participation, inventory, encounters and persistence. Building it earlier would force several foundational systems to be improvised inside tower-specific code.

### Exit criteria

- A complete small tower can be explored from entry to a meaningful endpoint.
- Floors and rooms have distinct authored content.
- The player can leave and return without losing intended progress.
- NPCs and objects have coherent location presence.
- Tests cover hierarchy, route validity, room state, item persistence, NPC movement and retreat/re-entry.

## Version 7 — A World That Changes

**Player-facing promise:** The tower and other challenges continue to develop while the player spends time elsewhere.

### Scope

- Introduce coarse time-based progression for selected world challenges.
- Evaluate changes at deliberate time boundaries (initially daily), not by continuously simulating every creature.
- Use a hybrid progression model:
  - Time provides baseline pressure and progression.
  - World state and player actions modify, accelerate, delay or redirect that progression.
- Track only the state needed for concrete challenge behavior, such as core condition, monster population/pressure, escape routes and control of important areas.
- Allow monsters to spread or threaten nearby locations when defined conditions are met.
- Allow NPCs to evacuate or move through explicit actions/events; full autonomous NPC AI is not a prerequisite.
- Present consequences through location changes, reports, events, quests and visible world state.
- Preserve consequences after the immediate threat is prevented. Stopping an outbreak does not automatically restore everything already damaged.
- Keep destruction of a danger source distinct from destruction of the physical location.

### Visible gameplay change

The player leaves the tower for several days and returns to find that its state has changed. Their earlier actions can delay, accelerate, prevent or redirect an outbreak, and consequences can reach other locations.

### Why this follows Version 6

World evolution needs a persistent challenge with meaningful state, a place hierarchy, NPC presence and consequences. The tower provides one bounded scenario to validate the model before applying it to the wider world.

### Exit criteria

- A challenge progresses across multiple in-game days.
- Time and state/action modifiers both affect its progression.
- At least two different player interventions produce different outcomes.
- At least one consequence occurs outside the tower.
- NPC evacuation/movement is represented coherently.
- Tests cover time jumps, sequential daily processing, thresholds, intervention combinations, prevented events and persistent consequences.

## Version 8 — Stories and a Long Life

**Player-facing promise:** The player's daily choices, relationships and world interventions combine into a personal long-term story.

### Scope

- Add authored main and side storylines that use existing world, character, NPC and challenge state.
- Add story flags and prerequisites only where needed by concrete branching content.
- Let relationships and prior actions influence dialogue, opportunities and outcomes.
- Add seasonal or calendar-based content if the time model supports it.
- Expand economy, trade, crafting and resource loops where they reinforce the player's life and goals.
- Add longer-term goals that can span months or years without forcing a single heroic objective.
- Support multiple valid outcomes: resolve, contain, exploit, rebuild around or ignore selected challenges.
- Add more world challenges and locations using proven patterns from the tower, rather than creating a universal challenge framework prematurely.

### Visible gameplay change

The player can pursue different long-term priorities and see their decisions shape relationships, opportunities, locations and story outcomes over an extended in-game life.

### Why this comes last

Long-form stories depend on stable daily life, progression, NPCs, world state, exploration and persistence. Building them earlier would make narrative branches depend on systems that are still changing.

### Exit criteria

- At least one substantial storyline has meaningful branches.
- Relationships and world state influence its available paths or outcomes.
- Long-term consequences persist and are reflected in the world.
- The player can pursue more than one valid life goal.
- Tests cover branch prerequisites, conflicting state, missed opportunities and outcome persistence.

## 5. Cross-Cutting Work

These areas are not isolated versions. They must evolve alongside the feature that needs them.

| Area | Ongoing responsibility |
|---|---|
| UI/UX | Make context, available actions, costs, state changes and consequences understandable. Add focused presentation modes when the gameplay requires them. |
| Save/Load | Persist only the state required by the current and upcoming playable loops. Expand the schema deliberately and test compatibility where needed. |
| Content data | Keep authored locations, characters, activities, events and items separate from C# gameplay rules where appropriate. Validate content before publishing it to runtime. |
| Automated tests | Add tests with each feature: normal cases, invalid cases, boundaries and important combinations. |
| Runtime verification | Test the actual Godot flow in addition to .NET unit tests. Keep evidence such as logs, screenshots or a concise verification record. |
| Documentation | Update README, owning design documents, open questions and acceptance criteria as behavior changes. |
| Audio and visuals | Add presentation that strengthens atmosphere and communicates gameplay; do not block a playable feature on final art. |
| Balance | Tune time, costs, rewards, progression and challenge pressure using repeatable scenarios rather than intuition alone. |

## 6. Version Delivery Checklist

A version can be marked complete only when all applicable items are satisfied:

- [ ] The player-facing change is described in one sentence.
- [ ] The feature is connected to an existing playable flow.
- [ ] The UI communicates its state, costs, options and outcomes.
- [ ] Domain rules and responsibility boundaries are documented.
- [ ] Normal, invalid, boundary and combination cases have automated tests.
- [ ] The relevant Godot runtime flow has been verified.
- [ ] Required state persists across the transitions specified by the feature.
- [ ] README and owning design documents reflect the actual implementation.
- [ ] Known limitations and deferred scope are recorded.
- [ ] Acceptance evidence is attached to the issue or QA record.

## 7. Planning and Scope Control

The roadmap is a sequence of product outcomes, not a promise that every feature must be implemented exactly as currently written. Details can change after a playable prototype reveals better solutions.

For each version:

1. Review the current repository, open issues, pull requests and design documents.
2. Select the smallest player-facing milestone that fulfills the version promise.
3. Write acceptance criteria and test scenarios before implementation.
4. Break work into small issues grouped by dependency.
5. Implement one coherent vertical slice at a time.
6. Verify automated tests and the actual Godot runtime.
7. Update documentation and close the milestone only when its visible result is demonstrated.

Do not start a later version merely because its code seems interesting. Start it when the previous version's exit criteria are met or when a deliberately scoped prototype is needed to resolve a documented design risk.

## 8. Relationship to Existing Design Documents

This roadmap organizes delivery order. It does not replace the detailed rules in the owning documents.

- [Game Overview](GameOverview.md) defines the product concept and experience.
- [Gameplay Loop and Location Events](GameLoopAndLocationEvents.md) defines the current location/event flow.
- [World Model Foundation](WorldModelFoundation.md) defines provisional domain vocabulary and modeling constraints.
- [Open Design Questions](OpenDesignQuestions.md) tracks decisions that are not yet settled.
- [Long-Term World and Challenges](LongTermWorldAndChallenges.md), if/when merged, records the direction for evolving world challenges.

If this roadmap conflicts with a more specific, already accepted design decision, preserve the accepted decision and update the roadmap. A proposal in an unmerged branch is not automatically an accepted requirement.
