# Long-Term World and Evolving Challenges

**Status:** Design direction — early concept, not an implementation specification.  
**Scope:** Long-term world simulation and challenge design for Adventurer Academy RPG.  
**Related:** [Game Overview](GAME_OVERVIEW.md), [Gameplay Loop and Location Events](GAME_LOOP_AND_LOCATION_EVENTS.md).

## 1. Core Vision

The player lives through a life in a fantasy world. During that life, the world presents opportunities, relationships, dangers and large-scale challenges. The player decides which matters to pursue, how to approach them, and what to leave alone.

The game should not require the player to follow one fixed heroic objective. Becoming stronger, studying, maintaining relationships, exploring, surviving and confronting major threats can all matter as parts of one lived life.

Challenges are not necessarily short encounters with a single intended solution. Some may take months or years of in-game time to understand, prepare for and resolve.

## 2. Challenges Evolve Over Time

A challenge may develop while the player is occupied with other parts of life. Its situation, scale, available responses and consequences can change over time.

For example, a large dungeon might begin as a newly discovered dangerous place. Monsters may gradually spread beyond it. If the threat is not contained, it could lead to attacks on roads, settlements or the academy. The player may intervene early, prepare over a long period, seek help, ignore it, or fail while attempting to stop it.

Failure or inaction can have lasting consequences. These may include the player's death, the death of an NPC the player cares about, damage or loss of locations, and changes to the surrounding world. These are possible design outcomes, not a promise that every challenge uses all of them.

## 3. Player-Driven Outcomes

A challenge should not necessarily have one predetermined ending. The player's actions can change what the challenge becomes and what role it plays in the world.

A large dungeon, for example, could have several possible directions:

- **Conquer it:** Defeat its major threats and neutralize its source.
- **Partially clear it:** Secure most of the dungeon but deliberately leave its core intact, turning the cleared area into a repeatable place to fight monsters and gain experience.
- **Repurpose it:** Find a way to transform the dungeon into a useful location or another kind of destination.
- **Leave it unresolved:** Allow it to remain dangerous or continue developing, with consequences determined by the world's response and the passage of time.

These are illustrative possibilities, not finalized mechanics or guaranteed features. The same principle may apply to other challenges and threats.

## 4. Consequences and the Continuing World

The world should be able to reflect what the player did, did not do, succeeded at or failed to accomplish. Consequences may affect:

- The player's survival and personal life.
- NPC survival, circumstances and relationships.
- The safety, condition and function of locations.
- The state and future development of a challenge.
- Which opportunities or activities become available later.

Not every consequence needs to be immediate. Some may emerge gradually or become visible only when the player returns to a place or reconnects with an NPC.

## 5. Relationship to the Life-Simulation Loop

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

## 6. Design Constraints and Open Questions

- Challenge development should be driven by player actions and the passage of time, rather than forcing one authored outcome.
- A challenge may support multiple resolutions, including partial completion and repurposing.
- Inaction can matter, but not every challenge needs to escalate into catastrophe.
- The game should allow new challenge types and outcomes to be added incrementally as development capacity grows.
- Do not introduce generalized systems, detailed time rules, world simulation architecture or specific mechanics until a concrete playable use case requires them.
- The frequency of world updates, how NPCs independently respond, how challenge stages are authored, and how long-term consequences are persisted remain open design questions.

## 7. Implementation Scope

This document records a long-term design direction. It does not require immediate implementation of:

- A global world simulation or autonomous NPC simulation.
- Multi-year dungeon progression.
- Dungeon ownership, rebuilding or conversion mechanics.
- A generalized challenge framework.
- Permanent death or irreversible NPC death across all content.
- A particular event scheduler, timeline or save-data model.

Implement the smallest concrete experience that supports the current prototype. Expand the simulation only when the project is ready to support the next behavior.
