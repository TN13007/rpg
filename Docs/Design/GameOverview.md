# Game Overview — Adventurer Academy RPG

**Project:** RPG prototype  
**Engine:** Godot 4.7.2 .NET / C#  
**Design status:** Early concept and UI prototyping  
**Primary audience:** Developers and external AI reviewers who need the product context before evaluating individual systems.

## 1. High Concept

A fantasy life-simulation RPG set in an Adventurer Academy. The player is a student learning to become an adventurer while managing everyday life, studying, resting, building relationships, developing their character and occasionally exploring dangerous places or fighting enemies.

The game is not intended to be a combat-first dungeon crawler. Combat is one activity within a broader student-life and character-growth experience.

## 2. Core Experience

The intended experience combines:

- **Daily life:** Choose activities and spend time at different places in and around the academy.
- **Study and growth:** Develop character capabilities and improve through learning and practice.
- **Relationships:** Meet NPCs, interact with them and build relationships over time.
- **Exploration:** Visit locations, discover people and objects, and choose contextual interactions.
- **Adventure and combat:** Enter dangerous encounters as an occasional but meaningful part of the wider life simulation.

The exact schedule model, time costs, progression formulas and activity economy have not been finalized. Do not assume they are implemented.

## 3. World Navigation and Interaction

The current prototype direction is **location-based navigation**, not free-roam movement.

The player selects a location or an available interaction. The interface presents the people, objects and activities available there. The prototype does not require a walkable map, collision, pathfinding or real-time character movement.

A location should communicate:
- Where the player currently is.
- What the place looks and feels like.
- The current time or relevant time context.
- Which NPCs and objects are present.
- What meaningful interactions are available.

A map may be useful later, but its role and necessity are still open questions.

## 4. Combat

Combat is planned as **turn-based**.

Combat has a dedicated presentation mode rather than reusing the normal Exploration presentation. The player should be able to understand the current encounter, distinguish actions from outcomes, read enemy reactions and make an informed next decision.

The current main scene contains one playable, deterministic Goblin encounter: the player enters Combat from the Academy Courtyard, chooses Attack / Defend / Dodge, resolves the Goblin's response, receives 25 G on victory and returns to the same location. This is a narrow vertical slice, not the final combat system. See [Combat Panel Review](CombatPanelReview.md) and [Playable Journey Sprint](SprintPlayableJourney.md).

## 5. Player State

Three persistent character indicators are currently intended to remain visible:

| Indicator | Intended meaning |
|---|---|
| Health | Survival; reaching zero means the character dies. |
| Energy | Limits available actions and contributes to challenge and progression. |
| Mood | Represents the character's emotional state. |

These are current design intentions, not a complete simulation specification. Their numerical ranges, recovery rules, thresholds and exact effects are undecided.

## 6. Presentation Direction

The UI is **text-first with illustrative visuals**. Text is used to communicate context, observations, dialogue and outcomes; images support atmosphere and help establish a place or encounter.

The intended player loop is:

1. Understand the current context.
2. Notice available people, objects or threats.
3. Choose an action.
4. Read what happened and what changed.
5. Use the new information to decide what to do next.

The interface should make choices and consequences readable without overwhelming the player with permanent panels or excessive statistics.

## 7. Current Prototype Boundaries

The repository currently contains an integrated Goblin encounter vertical slice, UI scenes and previews. They should not be mistaken for a complete game implementation.

In particular, do not assume that the project already has:
- A functioning day or schedule simulation.
- Complete location navigation or interaction systems.
- A working relationship or diary system.
- A generalized combat system or advanced enemy AI.
- A complete Health, Energy and Mood simulation.
- A finished life-simulation game or complete activity navigation.

## 8. Review Guidance

When reviewing a feature, evaluate it against the wider life-simulation concept rather than treating the game as a conventional combat RPG.

Please distinguish:
- A contradiction with an established design decision.
- A gap that blocks the intended player experience.
- A reasonable unresolved design question.
- An optional enhancement that can wait.

Do not turn every unresolved question into a new system. Prefer the smallest design that supports the intended experience.
