> **Historical document — superseded.** This records an earlier life-simulation-first direction. The current product direction is in [Design/GameVision](../Design/GameVision.md); current experience rules are in [Design/GameVision](../Design/GameVision.md).

# Game Design — Adventurer Academy RPG

**Purpose:** Canonical product vision and long-term experience direction.

This document combines the early game overview and long-term world vision. It describes product intent, not a promise that every proposed mechanic is implemented. For implementation status, use the current code, linked Issues/PRs and the relevant domain documents.

## Product Overview

**Project:** RPG prototype  
**Engine:** Godot 4.7.2 .NET / C#  
**Design status:** Early concept and UI prototyping  
**Primary audience:** Developers and external AI reviewers who need the product context before evaluating individual systems.

### 1. High Concept

A fantasy life-simulation RPG set in an Adventurer Academy. The player is a student learning to become an adventurer while managing everyday life, studying, resting, building relationships, developing their character and occasionally exploring dangerous places or fighting enemies.

The game is not intended to be a combat-first dungeon crawler. Combat is one activity within a broader student-life and character-growth experience.

### 2. Core Experience

The intended experience combines:

- **Daily life:** Choose activities and spend time at different places in and around the academy.
- **Study and growth:** Develop character capabilities and improve through learning and practice.
- **Relationships:** Meet NPCs, interact with them and build relationships over time.
- **Exploration:** Visit locations, discover people and objects, and choose contextual interactions.
- **Adventure and combat:** Enter dangerous encounters as an occasional but meaningful part of the wider life simulation.

The initial Version 1 rules for Fatigue and Combat Stamina are specified in [Version 1 Plan](Version1PrototypePlan.md) and [Testing](../Implementation/Testing.md). Broader schedule simulation, progression formulas and long-term activity balancing remain open; do not assume they are implemented.

### 3. World Navigation and Interaction

The current prototype direction is **location-based navigation**, not free-roam movement.

The player selects a location or an available interaction. The interface presents the people, objects and activities available there. The prototype does not require a walkable map, collision, pathfinding or real-time character movement.

A location should communicate:
- Where the player currently is.
- What the place looks and feels like.
- The current time or relevant time context.
- Which NPCs and objects are present.
- What meaningful interactions are available.

A map may be useful later, but its role and necessity are still open questions.

### 4. Combat

Combat is planned as **turn-based**.

Combat has a dedicated presentation mode rather than reusing the normal Exploration presentation. The player should be able to understand the current encounter, distinguish actions from outcomes, read enemy reactions and make an informed next decision.

The current main scene contains one playable, deterministic Goblin encounter: the player enters Combat from the Academy Courtyard, chooses Attack / Defend / Dodge, resolves the Goblin's response, receives 25 G on victory and returns to the same location. This is a narrow vertical slice, not the final combat system. See [UI Architecture and Combat Panel Contract](../Implementation/UserInterface.md) and [Playable Journey Sprint](SprintPlayableJourney.md).

### 5. Player State

The current prototype exposes four core character indicators; the exact UI presentation is defined in [UI Architecture](../Implementation/UserInterface.md):

| Indicator | Current initial meaning |
|---|---|
| Health | Survival; reaching zero means the character dies. |
| Fatigue | Daily-life exertion from activity intensity and actual duration. Initial range is 0–100; it does not automatically block actions or cause unconsciousness. |
| Combat Stamina | Short-term Combat resource, initially 0–10, used by defensive actions such as Defend and Dodge. It is not spent by ordinary daily activities. |
| Mood | Represents the character's emotional state. |

These are initial Version 1 rules, not a complete life-simulation specification. Long-term tuning and broader wellbeing mechanics remain open.

### 6. Presentation Direction

The UI is **text-first with illustrative visuals**. Text is used to communicate context, observations, dialogue and outcomes; images support atmosphere and help establish a place or encounter.

The intended player loop is:

1. Understand the current context.
2. Notice available people, objects or threats.
3. Choose an action.
4. Read what happened and what changed.
5. Use the new information to decide what to do next.

The interface should make choices and consequences readable without overwhelming the player with permanent panels or excessive statistics.

### 7. Current Prototype Boundaries

The repository currently contains an integrated Goblin encounter vertical slice, UI scenes and previews. They should not be mistaken for a complete game implementation.

In particular, do not assume that the project already has:
- A functioning day or schedule simulation.
- Complete location navigation or interaction systems.
- A working relationship or diary system.
- A generalized combat system or advanced enemy AI.
- A complete daily-life and wellbeing simulation beyond the initial Fatigue and Combat Stamina rules.
- A finished life-simulation game or complete activity navigation.

### 8. Review Guidance

When reviewing a feature, evaluate it against the wider life-simulation concept rather than treating the game as a conventional combat RPG.

Please distinguish:
- A contradiction with an established design decision.
- A gap that blocks the intended player experience.
- A reasonable unresolved design question.
- An optional enhancement that can wait.

Do not turn every unresolved question into a new system. Prefer the smallest design that supports the intended experience.


## Long-Term World and Evolving Challenges

**Status:** Design direction — early concept, not an implementation specification.  
**Scope:** Long-term world simulation and challenge design for Adventurer Academy RPG.  
**Related:** [Game Overview](../Design/GameVision.md), [Runtime Systems — current prototype](../Implementation/RuntimeSystems.md).

### 1. Core Vision

The player lives through a life in a fantasy world. During that life, the world presents opportunities, relationships, dangers and large-scale challenges. The player decides which matters to pursue, how to approach them, and what to leave alone.

The game should not require the player to follow one fixed heroic objective. Becoming stronger, studying, maintaining relationships, exploring, surviving and confronting major threats can all matter as parts of one lived life.

Challenges are not necessarily short encounters with a single intended solution. Some may take months or years of in-game time to understand, prepare for and resolve.

### 2. Challenges Evolve Over Time

A challenge may develop while the player is occupied with other parts of life. Its situation, scale, available responses and consequences can change over time.

For example, a large dungeon might begin as a newly discovered dangerous place. Monsters may gradually spread beyond it. If the threat is not contained, it could lead to attacks on roads, settlements or the academy. The player may intervene early, prepare over a long period, seek help, ignore it, or fail while attempting to stop it.

Failure or inaction can have lasting consequences. These may include the player's death, the death of an NPC the player cares about, damage or loss of locations, and changes to the surrounding world. These are possible design outcomes, not a promise that every challenge uses all of them.

### 3. Player-Driven Outcomes

A challenge should not necessarily have one predetermined ending. The player's actions can change what the challenge becomes and what role it plays in the world.

A large dungeon, for example, could have several possible directions:

- **Conquer it:** Defeat its major threats and neutralize its source.
- **Partially clear it:** Secure most of the dungeon but deliberately leave its core intact, turning the cleared area into a repeatable place to fight monsters and gain experience.
- **Repurpose it:** Find a way to transform the dungeon into a useful location or another kind of destination.
- **Leave it unresolved:** Allow it to remain dangerous or continue developing, with consequences determined by the world's response and the passage of time.

These are illustrative possibilities, not finalized mechanics or guaranteed features. The same principle may apply to other challenges and threats.

### 4. Consequences and the Continuing World

The world should be able to reflect what the player did, did not do, succeeded at or failed to accomplish. Consequences may affect:

- The player's survival and personal life.
- NPC survival, circumstances and relationships.
- The safety, condition and function of locations.
- The state and future development of a challenge.
- Which opportunities or activities become available later.

Not every consequence needs to be immediate. Some may emerge gradually or become visible only when the player returns to a place or reconnects with an NPC.

### 5. Relationship to the Life-Simulation Loop

The player continues living their life while challenges develop. Daily activities such as studying, resting, earning money and building relationships are not separate from the larger world: they can prepare the player to respond to future situations, provide reasons to care about what is at stake, or compete for the player's limited time.

A high-level loop is:

```text
Live daily life and pursue personal goals
                  |
                  v
       Discover or encounter challenges
                  |
                  v
      Decide whether and how to respond
                  |
                  v
  Prepare, act, retreat, delegate or ignore
                  |
                  v
     Time passes; the world keeps changing
                  |
                  v
      Outcomes alter life and the world
                  |
                  v
       Continue life in the changed world
```

This is a conceptual loop, not a finalized schedule, event scheduler or simulation architecture.

### 6. Design Constraints and Open Questions

- Challenge development should be driven by player actions and the passage of time, rather than forcing one authored outcome.
- A challenge may support multiple resolutions, including partial completion and repurposing.
- Inaction can matter, but not every challenge needs to escalate into catastrophe.
- The game should allow new challenge types and outcomes to be added incrementally as development capacity grows.
- Do not introduce generalized systems, detailed time rules, world simulation architecture or specific mechanics until a concrete playable use case requires them.
- The frequency of world updates, how NPCs independently respond, how challenge stages are authored, and how long-term consequences are persisted remain open design questions.

### 7. Implementation Scope

This document records a long-term design direction. It does not require immediate implementation of:

- A global world simulation or autonomous NPC simulation.
- Multi-year dungeon progression.
- Dungeon ownership, rebuilding or conversion mechanics.
- A generalized challenge framework.
- Permanent death or irreversible NPC death across all content.
- A particular event scheduler, timeline or save-data model.

Implement the smallest concrete experience that supports the current prototype. Expand the simulation only when the project is ready to support the next behavior.

