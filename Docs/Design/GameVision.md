# Game Vision — Adventurer Academy

**Status:** Current product direction  
**Last updated:** 2026-11-21

## High concept

Adventurer Academy is a player-directed fantasy life-simulation RPG about living in and around an academy in a wider, changing world. The player chooses how to spend time, whom to know, what to learn, where to go and which opportunities or dangers to pursue.

Daily activities, social interactions, exploration, adventures and tactical combat are not separate games competing for priority. They are different ways of interacting with the same continuing world. Combat is a specialized, high-stakes interaction with its own tactical rules; its outcomes can affect the player's ongoing life and the world around them.

The game should make room for self-directed play. A player may spend a session studying, meeting people, exploring, preparing for danger, fighting, or following a situation that connects several of these activities. A single mandatory heroic sequence should not dictate every session.

## Experience priorities

1. **Player-directed life:** The player decides what matters to them and how to spend their time. The game presents understandable opportunities, constraints and consequences rather than forcing one fixed activity loop.
2. **Meaningful world interactions:** Players can act through daily activities, conversation, travel, investigation, trade, exploration or force when the situation allows. Each interaction communicates its context, requirements, costs and outcome.
3. **Continuity and consequence:** Relevant character and world state carries between contexts. A choice made during ordinary life may shape a later encounter; an encounter may change the player's health, resources, relationships, reputation or available opportunities.
4. **Specialized tactical combat:** Combat remains a deep, deliberate experience built around party decisions and the Tactical Timeline. It is part of the world, not a disconnected game mode, but it does not need to use the same execution rules as ordinary actions.
5. **Character and narrative investment:** People, places and personal goals give meaning to the player's decisions. Narrative should deepen the current life and its relationships, not dictate every action.
6. **Incremental simulation:** Simulate the people, places and long-term changes that create observable player decisions. Do not simulate every character or system merely to make the world appear technically complete.

## Core pillars

### A World to Live In

The Academy is a recurring place to study, train, rest, meet people and make plans. The wider world provides other locations, opportunities, discoveries and dangers. The world is the shared context for these activities, not a wrapper around a separate combat game.

### Contextual Interactions and Outcomes

The player encounters a situation in a current context and chooses from the interactions that make sense there. An interaction may have requirements, costs, time, risks, effects and narrative feedback. The responsible gameplay domain validates and resolves it; the result changes the relevant character or world state.

This is a shared design model, not a requirement for one universal interaction executor in code.

### Tactical Timeline Combat

Combat is a specialized interaction context for dangerous confrontations. The player commands a party, reads threats and controls the order and timing of actions. It keeps its own Combat Timeline, combat rules and execution flow.

### Characters and Relationships

Characters should be worth knowing as people, not just as combat statistics. Interactions may reveal information, change familiarity or trust, create opportunities, provide help, or produce conflict. Implement relationship depth only when it changes observable player choices or outcomes.

### Discovery, Adventure and Consequence

The player may discover a threat, opportunity or mystery while pursuing another goal. They can prepare, intervene, seek help, partially resolve it, ignore it or fail. The world should reflect important outcomes in proportion to what happened.

## Shared world-interaction model

At the design level, the experience follows this loop:

```text
Current world state and context
              |
              v
   Available interactions appear
              |
              v
       Player chooses one
              |
              v
 Owning rules validate and resolve it
              |
              v
     Outcome and effects are applied
              |
              v
 Relevant character/world state changes
              |
              v
   The next context offers new choices
```

Examples of interaction contexts include:

| Context | Example interactions | Time/rules |
|---|---|---|
| Daily life at a Location | Eat, rest, study, train, inspect | Uses the relevant daily-life action rules and GameClock |
| Travel and exploration | Move between Locations, investigate a place | Uses location/navigation rules |
| Social situations | Talk, ask, negotiate, help, refuse | Uses the authored social situation and its supported conditions/outcomes |
| Combat | Attack, defend, dodge, delay, revive | Uses CombatService and Combat Timeline rules |

These examples describe the intended experience; they do not claim that every listed interaction or system is already implemented.

The model must not be mistaken for a universal software pipeline. The conceptual interaction is shared, while each domain keeps the rules it needs:

- Daily-life actions may advance GameClock by minutes.
- Combat advances its own Combat Time and pauses for player decisions.
- Travel changes the current Location through the supported navigation flow.
- Events may present a situation and choices; a choice may resolve as an ordinary action or start a specialized Combat flow.

Do not merge CombatService into ActionExecutionService or collapse different time domains into one clock merely to make the concepts look uniform. Share code or effect contracts only when a concrete behavior genuinely has the same semantics in more than one domain.

## How life and combat affect each other

The following are design examples, not promises that every consequence is currently implemented:

- Studying or training may improve capabilities used in later exploration or combat.
- Relationships may provide information, assistance, access or additional choices.
- Injury or resource loss from combat may change which daily activities are sensible or available.
- Resolving a threat may make a Location or route safer and open new opportunities.
- Ignoring a developing danger may change the people, places or encounters the player meets later.
- Using force against a person may resolve an immediate obstacle while creating social, legal or reputational consequences.

A combat result should return to the continuing world context. Victory is not automatically a reset, and defeat does not have to be just a separate screen; the appropriate consequences depend on the situation and the rules being designed.

## World structure

**Accepted:** Hybrid structure.

- The Academy is a recurring hub and social anchor, but not merely a support menu for combat.
- Adventure areas are Locations in the same world where exploration, encounters, objectives and other interactions occur.
- The wider world may change in response to time, player actions and unresolved situations.
- Location-based navigation is the current presentation direction; free-roam movement is not required by this vision.

The player moves between contexts as their choices and circumstances require. This is not a mandatory Academy → Adventure → Combat → Academy sequence.

## Content priorities

- Prove complete player journeys that connect ordinary interactions, meaningful choices and their outcomes.
- Preserve the depth and distinct identity of Tactical Timeline combat.
- Expand social, daily-life and exploration content when it creates choices the player can understand and care about.
- Let progression support a wider range of actions and play styles, not only larger combat numbers.
- Keep optional personal-interest content, including NSFW material, optional; it must not become a mandatory barrier to the wider experience.
- Prefer authored content and the smallest supported rules before introducing general simulation frameworks.

## Scope test

Evaluate a proposed feature by asking:

- Does it create a meaningful choice in a recognizable world context?
- Does the player understand its requirements, costs, risks and likely consequences?
- Does its outcome affect the current situation or a later decision in a useful way?
- Does it connect naturally to existing daily-life, social, exploration or combat experiences?
- Can the value be demonstrated in a small playable journey?
- Is a new system genuinely required, or can the existing domain rules support it?

If the feature adds complexity without creating a meaningful player-facing decision, defer it.
