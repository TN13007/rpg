# Combat Panel — Design & Implementation Contract

**Status:** Integrated playable Goblin encounter slice; broader combat system remains prototype-level  
**Engine:** Godot 4.7.2 .NET / C#  
**Scope:** Combat presentation and its integration contract. The current preview does not implement combat rules.

## 1. Purpose

Combat uses a dedicated presentation mode. It must not reuse the Exploration panel with different text.

Exploration communicates a place and its available interactions. Combat communicates the current encounter, completed actions, outcomes, observed threats and currently available choices.

## 2. Screen Structure

| Area | Responsibility | Update behavior |
|---|---|---|
| Encounter Header | Encounter name and context | Encounter start or context change |
| Combat Status | Round, active turn, enemy HP / condition and current observed intent | Replace from latest view data |
| Message Log | Ordered history of actions, reactions, results and observations | Append new messages |
| Action Selection | Current actions, target, cost, description and availability | Replace the full action list |

The Character Panel remains outside Combat Panel and persistently displays Health, Energy, Mood and Gold. It remains visible during Combat.

## 3. Architecture and Data Flow

The intended gameplay boundary is:

    Actor / UI
        |
        | CombatActionRequest
        v
    CombatService
        |-- Validate action and current turn
        |-- Resolve action using combat rules
        |-- Apply valid result to combat state
        |-- Return CombatResult with safe CombatSnapshot
        v
    CombatPresentationMapper
        |-- Convert public combat events into messages
        |-- Pass through presentation-safe status only
        |-- Build CombatViewData
        v
    CombatPanel
        |-- Replace current Status
        |-- Replace available Actions
        |-- Append new Messages
        v
    Status / Log / Action components

### Ownership rules

- CombatService owns action validation, resolution and mutation of combat state. It does not reference Godot UI nodes.
- CombatResult reports success or failure, a failure reason when applicable, the resulting presentation-safe snapshot and events produced by the request.
- CombatSnapshot is a safe read model. It may contain current enemy HP, conditions, active turn, available actions and observed intent. It must not contain raw enemy AI state, planned action IDs, hidden targets or internal decision data.
- CombatPresentationMapper converts the result into CombatViewData and maps event types to message type, category, speaker and text.
- CombatPanel consumes CombatViewData. It does not calculate damage, spend resources, validate game rules or inspect enemy AI state.

The UI's enabled / disabled action state is advisory. CombatService.ExecuteAction must validate again because the state may have changed since the view was produced. If validation fails, the service returns a failure result and leaves combat state unchanged. If validation succeeds, it resolves the action, applies the result, then returns the updated safe snapshot and produced events.

### Data contracts

| Type | Responsibility |
|---|---|
| CombatActionRequest | Action ID and optional target ID requested by the actor |
| CombatResult | Success / failure, failure reason, safe snapshot and produced events |
| CombatEvent | Completed gameplay event with type, round and relevant values |
| CombatSnapshot | Current presentation-safe encounter state |
| CombatViewData | Data the Combat Panel can render |
| CombatMessage | One historical, semantically typed message |
| IntentObservation | Information the character has actually observed or inferred |
| CombatActionOption | Player-facing action and its current availability |

ICombatService defines the application boundary. CombatService is the current concrete implementation for one deterministic Goblin encounter; it is intentionally not a generalized combat framework.

## 4. Message Contract

Every message has one of three semantic types. Alignment is a visual aid, not the only identity cue.

| Type | Alignment | Required visible identity |
|---|---|---|
| Player | Right | Speaker label such as YOU |
| Enemy | Left | Stable enemy label such as GOBLIN A |
| System | Center | SYSTEM label and category when applicable |

Each message contains a stable ID, Round, Type, Category, Speaker and Text.

Categories are None, Damage, Check, Turn, Environment, Inference, Emotion, StatusChange and Round.

Do not add a fourth message type for thoughts. Use a categorized System message. A Round separator is also a System message with the Round category.

Completed events use past tense. Intent and available-action descriptions may use present or future wording. Use stable target names consistently.

The current status is the source of truth for HP. The log records damage delta only; it must not repeat resulting current HP.

Example:
- Status: Goblin A — 40 / 60 HP.
- Log: SYSTEM · DAMAGE — Goblin A loses 20 HP.

## 5. Enemy Intent and Information Fairness

Enemy intent is an observation, not the enemy AI's internal next action.

- CombatService may know the planned action internally.
- The character may not know it.
- CombatSnapshot may expose only an IntentObservation produced by a valid observation / inference rule.
- IntentObservation contains enemy identity, player-facing description and confidence (Suspected or Recognized). It must not expose a hidden action ID or raw AI decision.

No observation means no intent card. Sending a snapshot with Intent = null clears the card.

### Lifecycle

1. A valid observation creates or updates the intent.
2. A new observation replaces the previous observation for that enemy.
3. When the observed threat starts executing, is interrupted or otherwise becomes invalid, the next snapshot clears or replaces it.
4. A new encounter starts with no intent unless an observation has already occurred in that encounter.

Gameplay rules decide when an observation becomes invalid. The UI must not infer expiry from elapsed time or guess enemy behavior.

### Information precedence

- Intent card: temporary, current actionable expectation.
- Message Log: historical record of what the character observed or what actually happened.
- Enemy action message: describes the visible action after it occurs.
- Inference message: explicitly describes the character's interpretation, not objective fact.

Do not repeat the same claim in all four places.

## 6. Action Availability

The latest snapshot replaces the entire available-action list; it is not appended to the previous list.

The service refreshes that list whenever the player turn begins or a relevant condition changes, including resources, target availability, status effects or encounter phase.

Each option contains an ID, label, optional target context, cost, description, availability and optional unavailable reason.

The UI disables unavailable actions and may display the reason. The service still validates every submitted request before applying it.

## 7. Status, Log and Encounter Lifecycle

- Status is current state; Log is historical sequence.
- Enemy HP is exact in this prototype.
- Log records damage deltas, not duplicate current HP.
- A new encounter clears the previous encounter log and intent.
- The log is bounded to 60 messages.
- Auto-scroll only when the reader was already at the bottom before a new message arrived.
- If the reader is inspecting older messages, preserve their scroll position.
- A round separator is a styled System message with category Round. Timestamps are not required for the prototype.

## 8. Accessibility and Layout

- Message identity remains visible in text labels, not only alignment or color.
- Action buttons are keyboard-focusable and operable.
- Unavailable actions expose a readable reason.
- Message bodies wrap when the window is resized.
- Maintain sufficient contrast between text, labels and message backgrounds.
- The preview is desktop-first. Exact narrow-screen stacking rules remain open and must be tested before mobile-sized layouts are claimed as supported.

## 9. Mode Transition

The Main Controller owns the transition between Exploration and Combat.

- Exploration → Combat: show the dedicated Combat Panel.
- During Combat: apply the latest view data.
- Combat → Exploration: return to the current location's Exploration presentation.
- Character Panel remains persistent.

The Main Controller currently implements this transition for the Goblin encounter. The standalone Combat Preview remains separate.

## 10. Prototype Reality Check

Implemented in the main playable slice:
- Dedicated Combat Panel and separate Status, Message Log and Action components.
- Typed messages with visible speaker and category labels.
- Presentation-safe CombatSnapshot, CombatResult, CombatViewData and mapper contracts.
- Intent observation model without a hidden AI action field.
- Bounded log and conditional auto-scroll.
- Keyboard focus enabled for action buttons.
- Main Screen integration with mutually exclusive Exploration and Combat modes.
- Deterministic CombatService for Attack, Defend and Dodge, including validation, enemy response, HP / Energy mutation and victory / defeat.
- Victory reward of 25 G applied to the shared PlayerState.
- Persistent Character Panel showing Health, Energy, Mood and Gold.
- Return from Combat to the same Exploration location.

Not implemented:
- Multiple enemies, advanced enemy AI, perception checks or randomized combat.
- Skills, items, status effects, VFX, audio or save/load.
- Full activity simulation, relationship, diary or quest behavior.
- Responsive narrow-screen layout verification or full assistive-technology testing.
- Runtime build and end-to-end verification in Godot 4.7.2 .NET.

Combat rules are deliberately small and deterministic for this sprint. The standalone Combat Preview still uses illustrative values and is not the main gameplay entry point.

## 11. Acceptance Criteria

- [ ] Combat is a dedicated scene, separate from Exploration.
- [ ] CombatService is the only owner of action validation, resolution and combat-state mutation when gameplay integration is implemented.
- [x] UI presentation contracts do not expose raw enemy AI state.
- [x] Message data includes stable ID, round, semantic type, category, speaker and text.
- [x] Player, Enemy and System messages have visible text identities in addition to alignment.
- [x] Status shows current enemy HP; log records damage deltas.
- [x] Intent is represented as an observation and can be cleared by sending null.
- [x] Action availability is replaced from latest view data; service-side revalidation is specified.
- [x] Log is bounded to 60 messages.
- [x] Auto-scroll follows new messages only when the reader was already at the bottom.
- [x] Action buttons support keyboard focus.
- [x] Concrete CombatService validates, resolves and applies the current sprint actions.
- [x] Main Controller integrates Combat and Exploration modes for the Goblin encounter.
- [ ] Narrow-screen layout and assistive-technology behavior are tested.

## 12. External Review Questions

1. Is the Service → Result / Snapshot → Mapper → ViewData → UI boundary clear?
2. Does the safe snapshot contain exactly the information presentation needs?
3. Are message categories sufficient without a fourth message type?
4. Is the intent lifecycle consistent with the game's observation rules?
5. Are action availability refresh conditions complete?
6. Does the log preserve reading position correctly?
7. What responsive layout should be used below desktop width?
