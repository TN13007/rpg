> **Historical delivery roadmap — superseded.** This plan belongs to the earlier reincarnation life-simulation-first product direction. Current player-facing direction is in [Design/Roadmap](../Design/Roadmap.md). Current implementation status is in [WORKFLOW.md](../../WORKFLOW.md). Retained for traceability to earlier Issues and planning decisions.

# Development Roadmap — Adventurer Academy: Reincarnation RPG

**Project:** TN13007/rpg  
**Engine:** Godot 4.7.2 .NET / C#  
**Roadmap status:** Proposed long-term product plan; implementation priority updated 2026-11-20 to establish a text-first playable Academy life before visual production  
**Last reviewed:** 2026-11-20  
**Canonical planning issue:** [#69 — Deliver the Reincarnation RPG vision through playable milestones](https://github.com/TN13007/rpg/issues/69)  
**Narrative idea:** [#68 — Reincarnation narrative](https://github.com/TN13007/rpg/issues/68)

> **Planning principle:** Build one complete, emotionally meaningful life before building many lives. Every milestone must produce a playable and observable result; architecture exists to support that result, not replace it.

## 1. Product Vision

Adventurer Academy is evolving from a fantasy academy life-simulation RPG into a **Reincarnation Life-Simulation RPG**. The player lives through distinct lives, builds relationships with people who have independent agency, discovers consequences that may cross lifetimes, and eventually faces the choice of whether to end the cycle of reincarnation.

The game combines three narrative pillars:

1. **People who meet again:** Familiar souls may reappear in different lives as people with different identities, roles, personalities, abilities, memories and relationships. A shared Soul Identity does not make two incarnations the same person in terms of lived experience.
2. **Redemption and responsibility:** Past choices can create lasting consequences. The player must face what they have done; other characters are not required to forgive, remember, follow or love the player.
3. **The meaning of a life:** Each life must matter in its own right. Ordinary routines, friendship, affection, humor, ambition, loss and small accomplishments are not filler between reincarnations. A finite and imperfect life can still be sufficiently happy.

**Central question:** *When there is no next life in which to try again, how will we choose to live—and can one finite, imperfect life be enough?*

### 1.1 Player experience pillars

- **Daily life:** Spend time, manage wellbeing, study, work, rest and pursue personal interests.
- **Character identity and growth:** See the character, shape appearance and capabilities, and understand how choices affect what they can do.
- **People and relationships:** Meet recurring NPCs who have their own goals, schedules, boundaries, memories and decisions.
- **Exploration and adventure:** Visit meaningful places, investigate situations, face danger and choose how to resolve challenges.
- **A living world:** Selected people, locations and challenges continue to change when the player is elsewhere.
- **Reincarnation and consequence:** Distinguish a Soul from each Incarnation; let selected memories, echoes and consequences cross lives without erasing the distinct identity of each life.
- **A finite ending:** The player may discover a way to confront the force behind reincarnation and earn a final life. The ending must be earned by prior choices and established story payoffs.

### 1.2 Narrative principles

Use these principles when designing characters, scenes, quests and multi-life story arcs:

- **Promise – Progress – Payoff:** Introduce mysteries, symbols, relationships and promises early; develop them; deliver a payoff supported by prior evidence.
- **Cause and Effect:** Prefer consequences that follow from choices and established conditions over a sequence of unrelated incidents.
- **Want – Need – Flaw – Stakes:** Important characters need their own wants, deeper needs, flaws and something meaningful to lose.
- **Objective – Obstacle – Action – Turning Point – Payoff:** A scene should change a relationship, belief, plan, resource, knowledge or world state.
- **NPC Agency:** NPCs may refuse, disagree, leave, fail, forgive, betray or choose a life that does not include the player.
- **Emotional Contrast:** Give the player peaceful and joyful moments as well as hardship. Do not use repeated tragedy as a substitute for character development.
- **Consequences without omnipotence:** Reincarnation is not a universal undo button. Knowledge, memory and outcomes have explicit scope and limits.
- **Earned Ending:** Do not resolve the final conflict with an unforeshadowed twist, arbitrary power or automatic restoration of every loss.

## 2. Development and Scope Principles

### 2.1 Player-facing milestones

A version is not complete because code, data or documentation exists. It must produce a coherent gameplay outcome that can be demonstrated in Godot.

Each milestone must define:
- A player-facing promise and a bounded scenario.
- Explicit in-scope and out-of-scope behavior.
- Relevant domain rules and state ownership.
- Automated tests for normal, invalid, boundary and important combined cases.
- Godot runtime acceptance for the actual player journey.
- Persistence requirements for every state that must survive a transition or restart.
- Product completion separately from any learning outcome.

### 2.2 Build in dependency order

The intended progression is:

**Daily-life foundation → Character identity and growth → Recurring people → One complete first life → Wider world → Adventure and dungeons → Living-world simulation → Reincarnation across lives → Full narrative campaign and final life.**

Later systems depend on earlier player experiences. For example:
- NPC autonomy is only meaningful when the player can meet NPCs and observe their behavior.
- Long-term relationships need recurring interactions and a working daily-life/time loop.
- World simulation needs meaningful locations, persistent state and consequences.
- Reincarnation needs at least one complete life and a proven content/persistence model.
- A multi-life story should not be produced at scale until one life has demonstrated its narrative and production cost.

### 2.3 Prefer concrete behavior over speculative frameworks

- Do not add a general-purpose system without a concrete gameplay scenario that needs it.
- Keep authored narrative beats and systemic NPC behavior complementary. Use authored content for important story promises and controlled outcomes; use simulation for bounded, understandable variations in daily life and responses.
- A Schedule is an intention, not a guaranteed sequence of successful actions. Goals express desired outcomes; decisions select what to do now; the shared Action System validates and executes.
- Events communicate things that have actually happened. A failed or interrupted action must not be treated as a completed goal.
- Do not introduce a full GOAP planner, LLM NPC, universal Event Bus, generic quest engine or continuous simulation unless a proven scenario requires it.
- Use coarse off-screen updates for distant or ambient entities; reserve more detailed decision-making for important NPCs and active situations.
- Keep location-based navigation unless a concrete player-facing feature justifies a walkable real-time world.
- Reuse the existing Layered Architecture and Action Pipeline where they fit. Refactor only when a new vertical slice exposes a real boundary problem.

### 2.4 Keep the product scope honest

The following quantities are deliberately **not fixed yet**:
- Number of lives/worlds in the final release.
- Total playtime.
- Number of major NPCs and supporting NPCs.
- Number of major branches and endings.
- Final art style and character-rendering technology.
- Target platforms, localization and release channel.

Use the first complete-life vertical slice to estimate content production cost. Do not treat earlier illustrative estimates as approved scope.

## 3. Current Baseline — Version 0 (Historical Foundation)

The current repository is no longer an empty project. It contains a narrow, integrated Academy/Goblin prototype and a maintainable technical foundation.

### Implemented on current main

- Godot 4.7.2 .NET / C# project, main scene and explicit Bootstrap, Domain, Application, Presentation and Infrastructure layers.
- Character definitions loaded from JSON; runtime Health, Fatigue, Stamina, Mood and Gold.
- Character and Combat Action catalogs with ID-based content references and validation.
- Action execution, activity-time calculation, activity-based Fatigue and authored Effects.
- GameClock with elapsed-minute advancement and midnight rollover.
- Location travel among Dormitory, Courtyard, Practical Classroom, Guild Hall and Infirmary.
- Source-aware Exploration choice query and presentation mapping.
- Location Event selection and a Courtyard Goblin Event with Confront/Sneak choices.
- Dedicated turn-based single-Goblin Combat flow and presentation.
- Rest, Sleep, Combat resource use and Infirmary treatment.
- Automated .NET tests and GitHub Actions validation.
- Manual user confirmation that the Infirmary journey works.

### Important current limitations

- There is no persistent NPC entity population. A static `talk_mira` Location Action is not an NPC instance.
- Character UI includes a static portrait asset and resource panel; it is not a configurable character-rendering pipeline.
- There is no implemented skill progression, inventory/equipment loop or appearance customization.
- Relationship, diary and Journal/Goals pages are not full gameplay systems.
- NPC Schedule, autonomous decision-making, Memory and off-screen simulation are not implemented.
- Save/Load for the daily-life loop is not complete.
- Location Event attempt state is runtime-only; this is not a persistent world event history.
- The current Combat is a small prototype, not the final adventure/party combat system.
- Soul Identity, Incarnation, cross-life Memory, Life transition and Reincarnation are narrative/design ideas only.

**Baseline evidence:** [WORKFLOW.md](../../WORKFLOW.md), [Architecture](../Implementation/Architecture.md), [Gameplay](../Implementation/RuntimeSystems.md), [Testing](../Implementation/Testing.md), and the current source tree. The last main snapshot reviewed for this roadmap is commit `98c438f32a22ff1b738a58102ca20d000d264a4f` (2026-11-19). Re-check main before each implementation Session.

## 4. Version Roadmap

## Version 1 — A Day at the Academy

**Status:** In progress. Accepted scope is owned by [Version1Plan.md](Version1PrototypePlan.md); this roadmap does not silently change it.

**Player-facing promise:** The player can live through a meaningful Academy day, understand choices available in each context, manage Health/Fatigue/Stamina/Mood and continue into the next day.

### Scope

- Source-aware Exploration Action Choice Pipeline.
- Separate Player Actions, Location Actions, Travel Choices and Event Choices.
- Explicit action costs, time advancement, activity Fatigue and relevant resource Effects.
- Health, Fatigue, Combat Stamina and Mood rules.
- A small set of Academy activities and recovery options.
- Sleep and next-day transition.
- Daily Summary and minimum Save/Load for the daily-life loop.

NPC entities, relationship gameplay, autonomous NPC simulation, item/container interactions and reincarnation remain out of scope for Version 1. The current Combat pipeline remains bounded to its existing prototype.

### Current progress

- Sessions 1–5 have merged implementations on main.
- Session 5 (Infirmary treatment) has user-confirmed interactive runtime acceptance.
- Session 4 implementation and automated tests are merged, but manual Sleep runtime evidence remains tracked by [Issue #62](https://github.com/TN13007/rpg/issues/62).
- Sessions 6–9 (Mood and Daily Wellbeing, Complete Academy Routine, Daily Summary, Save/Load) remain planned and are not implemented.
- Earlier runtime evidence for some sessions is incomplete or ambiguous; do not infer complete acceptance from merged code or CI alone.

### Exit criteria

- The complete day can be played from morning through the next morning.
- Player, Location, Travel and Event choices remain correctly separated by source and context.
- Action costs, availability, state changes and failure behavior are authoritative and testable.
- Health, Fatigue, Stamina and Mood have documented behavior and visible feedback.
- The daily summary reflects successful actions only.
- Save/Load restores the supported day state without replaying Effects or duplicating history.
- Automated tests and Godot runtime acceptance cover the complete daily-life journey.

**Detailed implementation plan:** [Version 1 — Session-by-Session Implementation Plan](Version1PrototypePlan.md).

## Version 2 — Character Identity and Gameplay Growth (Text-First)

**Player-facing promise:** The Player is a distinct, developing Character whose attributes, traits and learned capabilities create meaningful differences in what they can do. This version uses the existing text/profile UI; visual customization is not a prerequisite.

### Scope

- Establish a clear shared Character definition/runtime model usable by Player and NPCs without embedding Player input or NPC decision policy in the shared Domain model.
- Give the existing Strength, Agility, Intelligence, Endurance and Wisdom attributes explicit, tested gameplay meaning; remove or defer any stat that has no concrete use.
- Add a small skill/proficiency model grounded in actual activities such as lessons, practice, social interaction or combat.
- Let the Player make at least two meaningful growth choices that affect Action availability, requirements or outcomes.
- Define a small trait/personality vocabulary while keeping personality distinct from raw attributes and skills.
- Show identity, capabilities, traits and growth through the existing text-based Character Profile.
- Extend persistence for progression only when the relevant save boundary is implemented.

### Visible gameplay change

The Player can develop in at least two different directions and observe those choices changing available or executable Actions. The same Character foundation can represent Player and NPCs.

### Exit criteria

- Player and NPC share the same core Character capability/state model without Player-only assumptions in Domain.
- Every retained core attribute has a documented and tested gameplay use.
- At least one skill improves through a real in-game activity.
- At least two growth paths create different gameplay options or outcomes.
- Capability requirements are validated at query and execution time.
- Character identity and progression are communicated in the existing text UI and preserved at the required persistence boundary.

**Out of scope:** Character art, body/face customization, modular outfits, rigging, animation, large skill trees and universal equipment systems. Visual presentation is a separate deferred production track tracked by [Issue #76](https://github.com/TN13007/rpg/issues/76).

## Version 3 — People at the Academy

**Player-facing promise:** The Academy contains recurring people with their own circumstances and choices, not static dialogue prompts.

### Scope

- Represent NPCs as Character instances with stable instance identity, role, capability and current state.
- Place/query NPC presence at Locations; distinguish authored presence from current runtime presence.
- Add contextual social Actions and dialogue choices that use the common Action pipeline where appropriate.
- Add a small relationship model with observable outcomes; do not reduce every relationship to one affection number without a design reason.
- Give one named NPC (Mira) a bounded Schedule, a small set of Goals and a decision policy.
- Treat Schedule as a revisable plan. NPC Agent chooses an intention; Action System validates/execut es it; Events report actual outcomes. A blocked or interrupted action causes re-evaluation rather than false completion.
- Add selective Life Memory for meaningful witnessed/learned events. Respect knowledge boundaries: NPCs only know what they observe or are told.
- Reuse the Character model for Player and NPC. Separate shared capability/state from Player input and NPC decision policy.
- Keep autonomy bounded and understandable. A simple priority/utility rule and authored fallback is sufficient for this milestone; full GOAP is not required.
- Present NPC identity, presence, dialogue, schedule changes and relationship outcomes through the existing text UI and static portrait. Visual customization is deferred.

### Visible gameplay change

The player meets Mira in the Academy, sees her at different places/times, can build or damage a relationship, and observes her change plans when Player interaction or world conditions interrupt her day.

### Exit criteria

- Mira is a real NPC instance, not a Location Action with authored text.
- NPC Character capability constrains the Actions she can perform.
- Schedule, Goal, Decision and Action Execution have distinct state and ownership.
- At least one interruption results in a visible replan or fallback.
- At least two relationship outcomes are observable and persist.
- Memory records a meaningful event without storing unlimited raw dialogue.
- Tests cover unavailable NPCs, knowledge boundaries, interruption, action failure, repeated events and state ownership.
- A manual Godot scenario proves the full Player–Mira interaction journey.

**Out of scope:** A large NPC population, general-purpose planner, LLM dialog, simulated economy or complex social graph.

## Version 4 — The First Life: Narrative Vertical Slice

**Player-facing promise:** The player can live through one complete, emotionally meaningful Academy life with a beginning, development, turning point and ending.

### Scope

- Design one bounded first-life story arc using the narrative principles in Section 1.2.
- Establish the player's life goals, Mira's independent goals and at least one additional character perspective.
- Connect daily activities, locations, social interactions and one consequential event to the story.
- Extend the existing Location Event and Action pipeline only as needed for authored story beats, conditions, choices, flags and Effects. Keep story-critical Events condition-driven and ambient Events bounded; do not create a generic quest/event framework without a demonstrated need.
- Add the minimum Narrative State/Dialogue capability needed for authored conditions, choices and Effects. Evaluate tools such as Ink through a small integration proof before adopting a dependency.
- Allow prior actions, relationship state and NPC knowledge to influence dialogue/opportunities/outcomes.
- Include a meaningful personal choice with a consequence that cannot be erased by a routine reset.
- Introduce one recurring symbol, object, phrase, song or reaction that can later serve as a potential Echo. Do not explain the full reincarnation mystery yet.
- Deliver a complete ending for this life (happy, bittersweet or unresolved) that is satisfying on its own while leaving a clear future question.

### Visible gameplay change

The player plays a compact Academy story from arrival through key relationships and a major turning point to a visible ending. The player has a reason to care about Mira and about the life that is ending.

### Exit criteria

- The life has a playable beginning, middle, turning point and ending.
- At least one authored story beat has meaningful player-dependent variation.
- NPC agency and relationship changes affect later scenes.
- At least one ordinary-life moment has a deliberate narrative payoff.
- The recurring motif is established with enough context to be recognized later.
- Story conditions and Effects are testable; invalid content and unreachable critical beats are detected.
- The complete life can be played in Godot and survives save/load.

**Gate:** Do not scale to multiple lives until this slice demonstrates that a complete life is enjoyable, understandable and affordable to produce.

## Version 5 — Beyond the Academy

**Player-facing promise:** The player can leave the Academy and explore a small connected world whose places, objects and opportunities support the same life and story systems.

### Scope

- Extend Location content beyond the Academy to a town, wilderness area and one adventure destination.
- Generalize Academy-specific location loading only when the non-Academy use case proves the shared Location contract; coordinate with [Issue #61](https://github.com/TN13007/rpg/issues/61).
- Add persistent Object instances for a small set of meaningful interactions (door, container, notice, resource or device).
- Add a bounded inventory/item acquisition/use loop.
- Add a small multi-step quest or goal that can be resolved through more than one valid approach.
- Reuse Character capabilities, interaction, relationships, narrative state and time.
- Make return travel and time spent away meaningful.
- Keep world hierarchy/containment distinct from navigation connections.

### Visible gameplay change

The player accepts an opportunity, travels outside the Academy, explores and interacts with a new place, resolves a situation and returns to persistent changes and new information.

### Exit criteria

- One connected non-Academy region is playable end to end.
- Each included location has a distinct gameplay reason to visit.
- Objects have identity/state and contextual interactions.
- Items can be acquired and used in at least one complete loop.
- At least one quest has multiple steps and meaningful resolution alternatives.
- Returning to locations preserves intended state.
- Tests cover navigation, presence, object state, ownership/consumption and quest outcomes.

## Version 6 — Adventure, Combat and Dungeons

**Player-facing promise:** The player can undertake a dangerous expedition-like adventure within the existing Location model, use developed capabilities and companions, and make tactical choices.

### Scope

- Expand the existing CombatService and presentation only where a concrete scenario requires it; preserve its working rules otherwise.
- Add multi-target/multi-enemy combat and a small set of skills/status effects where they create distinct decisions.
- Add one companion/NPC participation flow using shared Character and Action concepts.
- Add item use in combat and clear action ownership.
- Support retreat, surrender, negotiation or other non-victory outcomes where appropriate.
- Build one compact tower/dungeon represented through Location hierarchy, with distinct floors/rooms and actual navigation edges.
- Persist explored rooms, opened routes, collected items, defeated/moved threats and NPC presence.
- Keep dungeon as a location/world structure, not a new generic Run/Expedition wrapper without proven need.

### Visible gameplay change

The player explores a small multi-area dungeon, meets threats and people, uses skills/items/companions, can retreat and later return to a world that remembers important changes.

### Exit criteria

- A complete small dungeon can be explored to a meaningful endpoint.
- At least one companion has a useful, understandable role.
- At least one encounter supports a non-victory resolution.
- The player can retreat and return without losing intended progress.
- Combat, Character, inventory and location state integrate without duplicate rules.
- Tests cover combat transitions, participants, target validity, retreat, rewards and room persistence.
- Manual Godot gameplay proves the complete adventure loop.

## Version 7 — A Living World

**Player-facing promise:** Selected people and world challenges continue to develop while the player is elsewhere, and the player can observe the consequences on return.

### Scope

- Add bounded NPC autonomy beyond the authored Mira schedule: goals, priorities, interruptions, fallback and replanning for a small named cast.
- Define the difference between intention, decision, accepted action, running action and actual outcome.
- Use the shared Action System for player and NPC actions; NPC Agent proposes actions but does not directly mutate Character or World state.
- Introduce event communication for meaningful domain outcomes. Use Query for reading, Command for requesting work and Event for facts that have happened; do not create a generic bus without a concrete integration requirement.
- Simulate selected off-screen activity using coarse time/event boundaries and catch-up rules, not frame-by-frame execution for every entity.
- Let selected world challenges progress over days; Player intervention may accelerate, delay, redirect or prevent outcomes.
- Preserve important world, NPC and location consequences across travel, day changes and restart.
- Provide enough journal/notification/debug information for the player and developer to understand important changes and their causes.
- Use simulation detail levels: detailed for active/important NPCs, coarse for distant NPCs, abstract for ambient populations.

### Visible gameplay change

The player leaves an area for several days, returns and observes that NPC plans, relationships or a world challenge have changed. Different interventions produce different outcomes.

### Exit criteria

- A named NPC schedule can be interrupted by Player or world events and replanned coherently.
- NPCs do not complete goals when their actions fail or are interrupted.
- Off-screen progression produces deterministic, testable outcomes for a bounded scenario.
- At least two player interventions create visibly different world outcomes.
- Catch-up does not duplicate Effects, miss required transitions or process the same event twice.
- Save/load restores enough information to continue simulation consistently.
- Tests cover time jumps, event ordering, interruption, failed action, sequential catch-up and persistence.

**Out of scope:** Continuous simulation of every entity, autonomous economy at full detail or emergent behavior with no player-facing purpose.

## Version 8 — Reincarnation Core and Two-Life Proof

**Player-facing promise:** The player experiences two distinct lives and recognizes that a familiar soul can return as a different person with a trace of the past.

### Scope

- Define a campaign-level lifecycle and distinguish Campaign, Soul Identity, Life, Incarnation and Character runtime state.
- Define what is retained, reset, transformed or discarded when a Life ends.
- Separate Life Memory, Soul Memory/Echoes and Player knowledge/memory. Do not copy all prior dialogue or life state into a new Incarnation.
- Define soul-to-incarnation links and rules for recurring souls; do not assume every character has a cross-life identity.
- Implement a Life transition that closes one life and creates a new one without overwriting its history.
- Extend persistence/versioning to save multiple life records, current incarnation, campaign progression and selected cross-life data.
- Reuse the same Character, Action, Interaction, NPC and presentation foundations while authoring a different background, role, skills, personality and relationships for the new Incarnation.
- Implement at least one observable Echo from the previous life (for example, an unexplained preference, dream, phrase or emotional reaction).
- Preserve NPC agency: the recurring character does not owe the Player the same relationship in the new life.

### Visible gameplay change

The player completes Life 1, begins Life 2 and meets a character with the same Soul Identity but a new Incarnation. The character's current-life memories and relationships are fresh, while a controlled Echo creates a recognizable connection.

### Exit criteria

- Two playable life contexts can be entered and distinguished.
- A recurring Soul has two different Incarnations with separate Character and Life state.
- Life 1 history remains intact after transition.
- Life Memory does not leak into the new Incarnation unless an explicit mechanic permits it.
- Selected Echoes and Player knowledge cross the transition exactly as designed.
- A recurring NPC can make a different decision and build a different relationship in Life 2.
- Save/load and migration tests cover transition boundaries, interrupted transitions and repeated loads.
- A complete Godot runtime journey demonstrates the cross-life result.

**Gate:** Do not scale content to many lives until the two-life proof is understandable, emotionally effective and technically reliable.

## Version 9 — Full Narrative Campaign, Final Life and Release

**Player-facing promise:** The player experiences a complete multi-life story about reunion, responsibility, meaning and the choice to end reincarnation or continue the cycle.

### Scope

- Determine the final number and variety of lives from the proven production cost of Version 4 and Version 8; do not assume all lives need the same content density.
- Produce the main storyline and selected personal/side arcs across the approved lives.
- Develop the mystery of the reincarnation cycle through established clues, recurring motifs, conflicting testimony and consequences.
- Give major recurring souls different motivations and opinions: wanting reunion, seeking release, pursuing revenge, refusing to be saved or choosing an ordinary life.
- Make prior actions, relationships, memories, Echoes and world state meaningfully affect later scenes and available paths.
- Establish the origin and rules of reincarnation through foreshadowed story payoffs. The idea that a past Player wish may have caused the cycle remains a candidate twist, not accepted canon.
- Build the route to meeting/confronting the deity or force behind reincarnation. The confrontation may combine combat, exploration, preparation, social choices and narrative decisions; a boss fight alone is not sufficient.
- Let the player decide whether/how to end the cycle, with understandable consequences for other souls.
- Deliver a final finite life in which there is no next-life retry. The ending may be happy without restoring everyone or removing every consequence.
- Complete art, animation, audio, UI/UX, accessibility, performance, save compatibility, localization decision, credits, packaging and release QA for the selected target platforms.

### Visible gameplay change

The player can complete the full campaign, resolve the central mystery, make the final decision and experience an earned ending. A final-life ending demonstrates the game's central theme through play, not only exposition.

### Exit criteria

- The main story has a beginning, progressive revelations, a climax and a resolved ending.
- The reunion, redemption/responsibility and meaning-of-life themes are all expressed through character choices and consequences.
- Major recurring characters have distinct arcs and independent outcomes.
- Critical story promises and motifs receive supported payoffs.
- The final decision has clear, tested consequences and more than one valid outcome where intended.
- The final life is mechanically distinct because the cycle cannot be used as an ordinary retry.
- All required save/load, content validation, narrative branch and simulation regression tests pass.
- The complete campaign is playable from a clean start through each supported ending on target release platforms.
- Release candidate passes performance, accessibility, UX, audio/visual, packaging and regression acceptance.
- Store/release materials and known limitations match the actual shipped scope.

## 5. Dependency Map

The high-level dependency sequence is:

- **V1 → V2:** Daily time/resource loop supports character development and presentation.
- **V2 → V3:** Reusable Character identity and capability support Player/NPC instances.
- **V3 → V4:** Recurring NPCs, interaction, relationships and Life Memory support a complete personal story.
- **V4 → V5:** A proven first-life narrative and authoring process provide a reason to expand the world.
- **V5 → V6:** Locations, objects, inventory and travel support adventure and dungeon gameplay.
- **V6 → V7:** Persistent places, characters and challenges provide meaningful targets for off-screen simulation.
- **V4 + V7 → V8:** A complete life plus stable persistence/world behavior support a reliable life transition.
- **V8 → V9:** The two-life proof validates the core promise before full multi-life content production.

The phases are product gates, not rigid waterfall. A small technical spike may be run early to retire a risk, but it must stay disposable and must not be mistaken for a completed version.

## 6. Cross-Cutting Production Tracks

These tracks evolve alongside the version that needs them.

| Track | Responsibility |
|---|---|
| Character art and presentation (deferred) | After the text-first playable life is established, compare rendering pipelines, define model/rig contract, full-body/portrait reuse, supported outfit changes, animation and visual QA. Tracked by [Issue #76](https://github.com/TN13007/rpg/issues/76). |
| Narrative and content pipeline | Create reusable authoring conventions for scenes, dialogue, conditions, effects, story flags, motifs and content validation; adopt Ink or another tool only after a prototype. |
| Save/Load and lifecycle | Version save data as new state scopes are introduced; test migrations, safe writes, recovery and no duplicate Effects. |
| NPC and world simulation | Keep Schedule, Goal, Decision, Action Execution and Event outcomes distinct; define simulation level of detail and catch-up behavior. |
| Presentation and UX | Show context, NPC presence, emotional/relationship changes, story consequences and life transitions clearly. |
| Audio and atmosphere | Establish a coherent audio direction and use recurring motifs where they strengthen recognition across lives. |
| Testing and observability | Unit tests, content validation, narrative reachability, deterministic simulation tests, runtime acceptance and developer-facing debug views. |
| Accessibility and localization | Make text, dialogue, controls, visual signals and localization constraints part of production rather than last-minute additions. |
| Release engineering | Reproducible builds, target platform checks, crash/error handling, save compatibility, packaging and release QA. |

## 7. Definition of Done for Every Version

A version is complete only when all applicable conditions are met:

- [ ] The player-facing promise is fulfilled in a coherent gameplay journey.
- [ ] The change is observable in the running Godot game.
- [ ] Domain ownership and dependency boundaries are clear.
- [ ] Authored content is validated and invalid references/conditions are reported.
- [ ] Automated tests cover normal, invalid, boundary and important combined cases.
- [ ] Save/load and lifecycle behavior are tested at the required scope.
- [ ] Manual Godot runtime acceptance is recorded against the tested commit.
- [ ] Player feedback explains important outcomes and state changes.
- [ ] Performance and simulation cost are reasonable for the demonstrated content.
- [ ] Documentation, issues and current handoff reflect actual implementation.
- [ ] Known limitations and deferred scope are explicit.

## 8. Production Scope and Risk Gates

Do not lock final content quantities before the first-life and two-life proofs. Use the following gates:

1. **After V2:** Estimate character art/rigging cost per character, portrait/full-body integration, outfit authoring effort and animation needs.
2. **After V3:** Estimate the cost of creating and maintaining one named NPC with schedule, relationship, dialogue, memory and visual presentation.
3. **After V4:** Estimate the time/content cost of a complete life, including scenes, choices, art, audio, testing and revision. Use this to choose the number and density of later lives.
4. **After V7:** Decide which world simulation details create visible value and which should remain abstract.
5. **After V8:** Estimate cross-life narrative branching, save complexity and recurring-character content cost before locking the final campaign.
6. **Before V9 release production:** Lock target platforms, language/localization plan, release scope, expected playtime and supported ending count.

### Explicit non-goals until justified

- LLM-driven NPC conversation or autonomous agents.
- Full GOAP for every NPC.
- Simulating every world entity continuously.
- Infinite/procedural lives or worlds.
- Arbitrary modular clothing combinations.
- A universal quest, event or story engine before multiple concrete uses prove the need.
- Rebuilding the current architecture simply because the product vision has expanded.

## 9. Current Next Steps

**Current implementation priority (user-approved 2026-11-20):** deliver a text-first playable Academy life before investing in character artwork. This reprioritizes implementation order; it does not mark the accepted Version 1 plan complete or silently change its formal scope.

1. Start with [Issue #78 — Text-First Academy Life](https://github.com/TN13007/rpg/issues/78), using its child issues in dependency order:
   - [#79 — Shared Character Foundation and Meaningful Growth](https://github.com/TN13007/rpg/issues/79).
   - [#80 — NPC Runtime Presence, Social Interaction and Relationships](https://github.com/TN13007/rpg/issues/80).
   - [#81 — Bounded NPC Agency: Schedule, Goals, Decisions and Memory](https://github.com/TN13007/rpg/issues/81).
   - [#82 — Conditional Story Events and Consequence Pipeline](https://github.com/TN13007/rpg/issues/82).
   - [#83 — First Playable Academy Life](https://github.com/TN13007/rpg/issues/83).
2. Reuse the existing Character Panel, static portrait, StoryPanel and location/action UI. Do not block gameplay on art, animation or Character Creator work.
3. Keep [Issue #75 — Daily Summary](https://github.com/TN13007/rpg/issues/75) deferred until day skipping or off-screen NPC simulation makes a summary materially useful. Preserve the accepted Version 1 plan until it is explicitly revised.
4. Add the minimum save/load boundary required before the first-life slice needs relationships, NPC memory and Event outcomes to survive restart; do not build persistence speculatively.
5. Keep [Issue #62](https://github.com/TN13007/rpg/issues/62) as outstanding manual Sleep runtime evidence and verify it when the day/time loop is next touched.
6. Resume [Issue #76 — Character Visual Pipeline](https://github.com/TN13007/rpg/issues/76) only after the text-first playable life has a coherent loop and the user is ready to learn/produce character art.

Do not start Soul/Incarnation implementation merely because the idea is exciting. One complete, playable life and reliable state persistence are prerequisites for a meaningful reincarnation prototype.

## 10. Relationship to Other Sources of Truth

- [WORKFLOW.md](../../WORKFLOW.md) owns current implementation status, verification evidence, learning evidence and handoff.
- [Version1Plan.md](Version1PrototypePlan.md) owns the accepted Version 1 Session scope and acceptance criteria.
- [Architecture.md](../Implementation/Architecture.md) owns current layer boundaries and runtime dependency rules.
- [Gameplay.md](../Implementation/RuntimeSystems.md) owns the implemented Location/Event and Exploration action-choice behavior.
- [GameDesign.md](../Design/GameVision.md) owns product concept and long-term world design; accepted narrative canon should be moved there after the narrative prototype is approved.
- [World.md](../Implementation/RuntimeSystems.md) owns provisional world/location modeling vocabulary.
- [Issue #11](https://github.com/TN13007/rpg/issues/11) owns long-term system design questions and baseline.
- [Issue #68](https://github.com/TN13007/rpg/issues/68) owns the reincarnation narrative idea.
- [Issue #69](https://github.com/TN13007/rpg/issues/69) tracks this end-to-end delivery roadmap.
- [Issue #62](https://github.com/TN13007/rpg/issues/62) tracks outstanding Session 4 runtime acceptance.

If this roadmap conflicts with an already accepted, more specific implementation contract, preserve that contract and update the roadmap through review. An idea or roadmap entry does not automatically authorize an implementation or change Version 1 scope.
