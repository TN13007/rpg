# World and Narrative Design

**Status:** Long-term direction; details remain in development  
**Last updated:** 2026-11-21

## World structure

The world uses a **hybrid, location-based structure**:

- The Academy is a recurring place to study, train, rest, work toward personal goals and meet people.
- Nearby towns, roads, wilderness and adventure areas provide other places to visit, investigate and interact with.
- Dangerous situations and combat occur in this same world. They are not a disconnected game mode.
- The wider world may change in response to time, player actions and unresolved situations.

The Academy is important because of the people, activities and opportunities found there. It should not be reduced to a preparation menu, but it also does not need to become a detailed school-management simulator.

## World interaction and consequence

The player experiences the world through contextual interactions. A Location, social situation, discovery or encounter presents relevant choices. The player selects an interaction; the responsible gameplay rules resolve it; the result changes relevant character or world state.

At the design level, this creates a continuing loop:

```text
World state -> situation/context -> available interactions
     ^                                  |
     |                                  v
updated state <- outcome/effects <- chosen interaction
```

Daily activities, social interactions, travel, exploration and combat share this conceptual pattern. They do not need identical costs, execution rules or presentation.

Combat is a high-stakes interaction context with its own Tactical Timeline and combat resolution. An Event may introduce a threat and offer a choice to fight, flee, negotiate or use another supported response; the Event itself is not the Combat system. When Combat ends, its relevant outcome should feed back into the continuing world.

The exact persistence and scope of each consequence must be defined by the scenario. Not every action needs to change the whole world, and not every choice needs a long-term consequence.

## Narrative direction

The story should give emotional and thematic meaning to the player's current life, relationships and adventures. The larger narrative may explore reincarnation, recurring souls, changed circumstances, responsibility, redemption and the meaning of a finite life.

The player should first have reasons to care about the people and events of the current life. The reincarnation mystery should deepen that investment rather than replace it.

The narrative should create context, people, mysteries and consequences without forcing every player into one fixed heroic sequence. A player may follow a major plot, focus on personal goals, or move between them.

## Evolving challenges and consequences

Major threats may change while the player pursues other goals. The player may intervene early, prepare, seek allies, negotiate, fight, partially resolve a threat, ignore it or fail. The available responses depend on the situation and on what the player has developed or discovered.

Outcomes may affect:

- The player's health, resources, capabilities and immediate circumstances.
- NPC circumstances, safety and survival.
- Locations, routes and the activities available there.
- Relationships, trust and reputation.
- Future encounters, discoveries and opportunities.
- The state and scale of a larger threat.

Not every challenge must escalate into catastrophe. Consequences should be legible, proportionate and connected to the action or inaction that caused them.

## NPCs and relationships

NPCs should have their own motivations, capabilities and circumstances where those details create meaningful player-facing choices. Schedules, memories and autonomous decisions are possible tools, not mandatory features for every NPC.

The first schedule model is a recurring daily timetable authored as time ranges, Locations and visible activities. It answers where a scheduled NPC is at the current GameClock time; it does not imply pathfinding, needs, autonomous decision-making or continuous simulation. A gap means the NPC is not scheduled at a known player-facing Location. The player discovers present NPCs through `See around`; their interactions are authored separately from Location actions and are available only while the NPC is present.

A desire to do something is not the same as being able to do it: events, resources, relationships, danger and the player's actions may prevent or redirect an NPC's plans.

Relationships should be represented through the smallest set of dimensions or flags that produce observable differences. Avoid a large social simulation or relationship framework before a playable interaction needs it.

## Time and simulation boundaries

Daily life and Combat have different time semantics:

- Daily activities may advance GameClock in minutes or hours.
- Combat runs on its own Combat Timeline and pauses while the player chooses an action.
- The rule for translating the elapsed duration or consequences of a completed encounter back into world time must be defined by the relevant gameplay scenario; do not assume the two clocks are interchangeable.

The game does not need to simulate every NPC or every Location continuously. Use authored presence, conditional events or coarse updates where they are sufficient. Add autonomous simulation only when the player can notice it and make a meaningful decision because of it.

## Content boundaries

Optional personal-interest content, including NSFW material, is secondary to the core world experience. It should not be required to access central exploration, character growth, combat or narrative content.

## Open questions

- Which character and world facts must persist across Location and Combat contexts?
- Which consequences should be immediate, delayed or visible only when the player revisits a Location or NPC?
- How should encounter duration and recovery interact with GameClock?
- Which NPCs need authored presence, simple state-driven behavior or bounded autonomy?
- How much of the wider world changes autonomously, and at what level of detail?
- How do challenge stages, relationships, reincarnation and story branches affect one another without creating an unmanageable simulation?

## Prototype under validation: modern student life and paranormal incidents

**Status:** Proposed prototype for [Issue #110](https://github.com/TN13007/rpg/issues/110); this does not replace the accepted product direction or the existing Academy slice.

### Experience premise

The protagonist is an ordinary university student living in a modern city. They have classes, friends, a part-time job, limited money and a normal social life. Supernatural phenomena exist alongside that routine, but the protagonist is not chosen, obligated to investigate, or automatically responsible for saving anyone.

The player can notice a rumor, follow it, ignore it, report it, exploit it, seek help or become involved by accident. Most days should still be allowed to remain ordinary. Paranormal incidents are opportunities and complications within the player's life, not a mandatory mission list.

Phones, laptops and the Internet are ordinary tools. They can eventually provide leads and social contact, but this prototype does not assume a phone UI or social-network system exists.

### Small setting slice

The prototype uses one compact city district rather than an open world:

| Location | Purpose | Example ordinary / unusual activity |
|---|---|---|
| University campus | Daily anchor and social hub | Attend class, study, meet classmates |
| Part-time café | Earn money and meet locals | Work a shift, hear neighborhood gossip |
| Student apartment | Rest and return to routine | Sleep, prepare for the next day |
| East District street | Connect daily-life places to the incident | Travel, observe police activity |
| Abandoned apartment block | Investigation location | Inspect rooms, listen, retreat |
| Paranormal response office | Optional source of specialist help | Request an assessment or service |

The location list is a proposed content map, not a claim that these places are already loaded by the game.

### Small cast

| Character | Role in the slice | Authored presence / interaction |
|---|---|---|
| Linh | Classmate; shares a rumor after class | Campus schedule; talk and ask about the post |
| Minh | Café shift supervisor | Café schedule; start or finish a shift |
| Ms. An | Dispatcher at a specialist response company | Office availability; request a consultation |

Characters should be introduced through what they do and what the player can ask them. They do not need relationship scores, memory simulation or autonomous AI for this first journey.

### Incident: The crying apartment

**Rumor:** A neighborhood group has several posts about a vacant apartment where a child can be heard crying at approximately 2:13 a.m. One post claims the building has been empty for years. Another says a tenant moved out after hearing someone call their name from inside a locked room. The posts disagree and none is reliable proof.

**Truth for the authored scenario:** A presence is repeating fragments of a former resident's nightly routine. It reacts to sound and to people entering the bedroom. The player should be able to infer a pattern from observations; the first contact should be unsettling, not an obligatory boss fight.

#### Player journey

1. **Morning — ordinary commitment.** The player travels to campus, attends class or studies. Linh mentions the neighborhood posts after class. The player can ask for details or end the conversation.
2. **Afternoon — personal choice.** The player can go to a café shift, study, return home or travel toward the East District. Choosing the shift earns money and consumes time; the incident does not pause the rest of life.
3. **Evening — investigate or ignore.** If the player visits the abandoned building, the Location description establishes its condition. An Event presents the sound from the upper floor. The player can listen from a distance, inspect the hallway, enter the bedroom or leave.
4. **Discovery — learn before acting.** Listening reveals that the crying repeats at a fixed interval. Entering the bedroom causes the presence to call the player's name and approach. The scene communicates danger and gives the player a chance to retreat; combat is not the default answer.
5. **Response — choose a level of risk.** The player can leave and contact the specialist company, attempt to document the phenomenon, or continue investigating. A reckless approach can trigger a Combat encounter only if the prototype deliberately supports that outcome.
6. **Aftermath — return to life.** The player returns to their apartment or next-day commitments. The authored feedback makes the immediate outcome clear. A later visit can show a changed description only if a persistent outcome flag is implemented; it must not be faked with a different first-visit paragraph.

#### What makes the choices meaningful

- **Ignore it:** The player keeps their normal schedule. No penalty is applied simply for not accepting an optional incident.
- **Observe and leave:** The player learns a useful clue while avoiding the most dangerous room.
- **Enter immediately:** The player gets more information but risks injury or a forced retreat.
- **Call specialists:** The player exchanges money or time for professional help, if the relevant payment and dispatch behavior is implemented.
- **Keep investigating:** The player accepts greater risk for more information and possible direct resolution.

The first playable version does not need all five options. It must, however, preserve at least one non-combat response and make leaving a valid choice.

### Mapping to the current repository

| Journey requirement | Existing contract | Current fit |
|---|---|---|
| Move between campus, café, home and incident site | Location exits and Travel | The Location schema can author the route, but the current loader loads only one `AcademyLocations.json` graph; a parallel city graph is not selectable yet |
| Spend time at class or work | Player Actions / Location Actions and GameClock | Supported for time advancement; the current effect set is limited |
| Have Linh and Minh appear at authored times | NPC schedules and `See around` | Supported by daily time ranges |
| Ask an NPC about a rumor | NPC interactions | Supported for authored interactions and feedback |
| Present the apartment scene and choices | Location Event and Event Choices | Supported for a bounded scene |
| Trigger a specialized fight | Event choice referencing a Location encounter action; Combat flow | Supported as a separate combat context |
| Retreat or choose a narrative response | Event Choice | Can present feedback, but does not by itself create durable incident state |
| Pay a company and dispatch a responder | Current effects include `AddGold`, but no service-call / incident-dispatch effect | Not fully supported |
| Remember that the apartment was investigated or resolved | No general persistent world-flag contract in the current implementation | Missing |
| Advance the incident while the player ignores it | No incident-stage scheduler or conditional world-state update | Missing |
| Show a social feed / rumor board on a phone | No phone or feed presentation/content contract | Missing; use an NPC conversation for this prototype |

The current action effect types are `RestoreHealth`, `RestoreFatigue`, `RestoreStamina`, `ChangeMood`, `AddGold` and `Revive`. Do not invent additional JSON effect names and assume they will load. Location Events currently support an authored description, chance, optional required NPC and choices; they are not a general multi-stage quest or conditional branching engine.

### Prototype boundary and likely minimum gap

The Location, NPC schedule, NPC interaction and Event definitions are reusable, but the current composition is not yet content-only for a second setting: `GameContentLoader.Load()` explicitly loads `AcademyLocations.json`, and `AcademyContent` validates one connected Location graph. A separate modern-city JSON file cannot currently be selected at runtime. Keeping the Academy intact while running this alternative setting therefore requires a small, explicit content-root/scenario-selection change, or a deliberate replacement of the existing graph. The latter would overwrite the current slice and is not part of this prototype.

Once the setting is loadable, the existing contracts can demonstrate the everyday route, discovery, investigation choices and a non-combat retreat without a generalized quest framework. A genuinely reactive specialist call or persistent resolved/unresolved aftermath needs at least one small, explicit piece of world state and a supported way to change and query it. The next implementation decision should be made only after the authored journey is wired and the exact limitation is reproduced. Do not build a generic Quest, Incident, Social Network or World Simulation framework for this one scenario.

### Open decisions after the prototype

- Is the modern university/city setting intended to replace the Academy fantasy setting, or is this an alternative concept being evaluated in the same project?
- Should the protagonist be able to work for a paranormal response company, merely hire one, or both?
- Which incident outcomes must persist between visits, and which can remain narrative feedback?
- Should supernatural incidents advance on fixed world-time milestones, only when the player visits relevant places, or through a small number of authored checkpoints?
- How visible should the supernatural world be to ordinary people?

