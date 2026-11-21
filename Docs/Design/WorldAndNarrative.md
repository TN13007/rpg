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
