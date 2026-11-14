# Combat Panel — Design & AI Review Brief

**Status:** Proposal / UI prototype  
**Engine:** Godot 4.7.2 .NET, C#  
**Scope:** Combat presentation only. This document does not define or implement combat rules.

## 1. Purpose

Combat must use a dedicated presentation mode. It should not reuse the Exploration panel with different text.

Exploration helps the player understand a place and discover possible interactions. Combat helps the player understand:

1. What just happened.
2. What the enemy did in response.
3. What changed as a result.
4. What threat the character has noticed.
5. Which actions are available now.

The Combat Panel is an event-oriented interface, not a scene-description page.

## 2. Screen Structure

The Combat Panel has four areas.

| Area | Responsibility | Update behavior |
|---|---|---|
| Encounter Header | Encounter name / combat context | At encounter start or context change |
| Combat Status | Round, active turn, enemy HP and conditions, detected intent | When combat state or observations change |
| Message Log | Ordered record of actions, reactions, results and observations | Append messages as events occur |
| Action Selection | Actions currently available to the player, including target and cost | Whenever available actions change |

The Character Panel remains outside the Combat Panel and continues to show the player's persistent Health, Energy and Mood. Do not duplicate those permanent player stats inside the message log.

## 3. Message Alignment Contract

Every message has one of three presentation types.

| Type | Alignment | Used for |
|---|---|---|
| Player | Right | Player action / declared intent |
| Enemy | Left | Enemy action, speech and visible reaction |
| System | Center | Damage results, skill checks, status changes, turn changes and tactical environment events |

A message should separate the action from its result.

Example sequence:

1. **Player / right:** "You attack Goblin A with a basic strike."
2. **System / center:** "20 physical damage. Goblin A: HP 60 → 40."
3. **Enemy / left:** "Goblin A snarls and lowers its stance."
4. **System / center:** "PERCEPTION CHECK · SUCCESS — You notice it shifting its weight forward. It appears to be preparing a dangerous lunge."

The message log is not the source of truth for current HP. The status area shows current HP; the log records how it changed.

## 4. Enemy Intent and Information Fairness

Enemy intent is not the same as the enemy AI's internal next action.

- The combat resolver may know the enemy's planned action.
- The character may not know that action.
- The UI may display only information the character has actually observed or inferred.

The caller should pass an **observed intent** to the UI, not the enemy AI's hidden action.

Suggested presentation states:

- **Not detected:** show no intent card.
- **Suspected:** show a cautious, uncertain description.
- **Recognized:** show a more specific description, but do not imply certainty unless the game rules justify it.

Example:
- Low Perception: "The goblin's movements seem unusual."
- Better Perception: "You suspect it is preparing to charge."
- Strong Perception: "You recognize its stance as the beginning of a forward lunge."

Perception should change the information quality, not secretly alter the enemy's action.

## 5. Character Thoughts and Neutral Messages

Internal narration is optional, not mandatory after every action.

Use a centered System message with a clear category label when the game reports:
- Perception or skill check results.
- The character's tactical inference.
- A meaningful emotional reaction.
- An environment event that changes combat conditions.

Do not turn every ordinary action into an internal monologue. Do not present an inference as objective fact.

Example labels:
- PERCEPTION CHECK · SUCCESS
- INSTINCT
- ENVIRONMENT
- STATUS CHANGE
- ROUND 03 · ENEMY TURN

The text should make it clear whether a statement is an observation, an inference, or an objective system result.

## 6. Environment During Combat

Do not keep repeating the normal Exploration description during combat.

Show environment information only when it matters to the encounter:
- A hazard activates.
- Terrain changes movement or attack options.
- Visibility changes.
- An object becomes usable or dangerous.
- A relevant environmental effect begins or ends.

A combat background illustration may remain visible as visual context, but it must not compete with the message log or action choices. The current prototype does not implement a battlefield illustration.

## 7. Action Selection

Each action option should provide enough information to make a decision without opening another screen unnecessarily.

At minimum:
- Action name.
- Target, when relevant.
- Resource cost, when relevant.
- Short effect or purpose.
- Availability state.

Unavailable actions may be disabled, with a reason provided where useful.

Selecting a button emits an action ID. This UI prototype does not resolve the action, calculate damage, spend Energy, or advance the turn.

## 8. Mode Transition

- Exploration → Combat: enter a dedicated Combat Panel when an encounter starts.
- During Combat: update status, append messages and refresh available actions.
- Combat → Exploration: leave the Combat Panel and return to the current location's Exploration presentation.
- A concise encounter result may be transferred to the Diary later. The full combat log does not need to remain in the normal Exploration view.

The Character Panel is persistent across both modes. The Main Controller (not this component) owns the mode switch.

## 9. Prototype Scope

Included:
- Dedicated Combat Panel scene.
- Separate status, message-log and action-selection components.
- Player-right, Enemy-left and System-center message alignment.
- Enemy HP / condition display.
- Optional detected-intent display.
- Action options with cost and description.
- A preview scene with sample Goblin encounter messages.

Not included:
- Turn resolution, enemy AI or damage formulas.
- Real Perception checks or stat calculations.
- Health / Energy mutation.
- Targeting logic, item use or skills.
- Combat animations, VFX or sound.
- Integration into the Exploration flow.
- Save/load or persistent combat history.

## 10. Acceptance Criteria

- [ ] The Combat Panel is a separate scene from the Exploration panel.
- [ ] Player messages are right-aligned.
- [ ] Enemy messages are left-aligned.
- [ ] System messages are center-aligned.
- [ ] The UI distinguishes an action from its result.
- [ ] Current enemy HP is visible without reading the message history.
- [ ] Hidden enemy AI actions are not passed to the UI as known information.
- [ ] Detected intent is visually distinct from confirmed combat state.
- [ ] Action options show relevant costs and descriptions.
- [ ] The log is scrollable and does not grow without a bound.
- [ ] The Character Panel remains a separate, persistent component.
- [ ] Combat ends by returning control to Exploration (integration is future work).

## 11. Questions for External Review

Please critique the design and the prototype code. Do not assume missing combat mechanics are implemented.

1. Is the separation between Combat Status, Message Log and Action Selection clear?
2. Is right / left / center alignment sufficient to distinguish Player, Enemy and System messages, including for color-blind players?
3. Is showing enemy HP in the status area while repeating damage deltas in the log redundant, or is this useful separation of current state and history?
4. Does the Enemy Intent card reveal too much, too little, or the right amount of information?
5. Should internal thoughts use a fourth visual message type, or remain a categorized System message?
6. Does the message log need grouping, timestamps, round separators or a compact mode?
7. Are there accessibility or readability problems with the current layout?
8. Is the component / scene / script split appropriately scoped, or is it over-engineered?
9. What should be changed before integrating the Combat Panel with Exploration?

Please separate:
- Critical defects.
- UX improvements.
- Optional future features.

Avoid adding new gameplay systems unless they are necessary to resolve a demonstrated problem in this UI.
