# Presentation User Interface and Contracts

**Status:** Describes the current repository UI composition and presentation contracts  
**Boundary:** This document owns current UI composition and presentation contracts. Product experience goals belong in [Design](../Design/README.md).

## 1. Main screen composition

| Region | Current responsibility |
|---|---|
| Sidebar | Navigation between available pages |
| Main Panel | Current Location exploration or dedicated activity mode |
| Character Panel | Persistent display of current Player state |

The Main Panel changes mode between Exploration and Combat. The Character Panel remains visible across both.

## 2. Exploration presentation

Exploration displays current Location context and available choices. Choices are discovered by Application queries and mapped into grouped, presentation-safe data. `StoryPanel` renders Location prose in a scrollable `RichTextLabel`, so long first-visit introductions remain readable without pushing choices off-screen. The Exploration flow chooses `firstVisit` on a Location's first entry during the current process and `returnVisit` on later entries; the selected text stays stable during the current stay. Visit history is not persisted across restarts.

Current pipeline:

```text
AvailableInteractionQuery
  -> ActionChoiceMapper
  -> ActionChoiceViewData
  -> StoryPanel
  -> ActionSelectionRequest
  -> ExplorationFlowController
```

The UI renders the groups and ordering supplied by its view data. It does not decide which actions are valid, inspect JSON or mutate Domain state.

The current request identifies:
- Source kind.
- Source ID.
- Action ID.
- Optional target/destination ID.

Action IDs alone are not globally unique. The owning flow/service revalidates each request because UI availability is advisory and state may have changed.

## 3. Combat presentation

Combat uses a dedicated Combat Panel rather than reusing the Exploration choice groups.

The combat flow is:

CombatPanel → CombatFlowController → CombatSessionService → CombatService → CombatSnapshot / CombatResult / ordered CombatEvent[] → CombatPresentationMapper → CombatViewData → CombatPanel.

The combat panel presents:
- Current Round, Player/Enemy phase and shared Party AP.
- The current Party roster, including Downed state and which members are eligible to act this Round.
- Current Enemy HP and condition only; no future Enemy action or target is displayed.
- Select an eligible Party member from the roster, then view only that member's actions. Every action option carries an ActorId; the UI submits that ID with the selected action. If that member has already acted, the roster selection moves to another eligible member.
- A target-selection state for Enemy-targeting actions and a dedicated ally-target list for Revive.
- An explicit End Player Turn control. When AP reaches zero, the Domain resolves the Enemy Turn automatically.

The Domain owns authoritative combat state. CombatSnapshot contains statuses and action options suitable for display; it does not expose mutable Character objects. The presentation mapper converts ordered Domain events into messages and visual feedback cues. The UI renders those values and does not calculate damage, spend AP/Stamina or decide Enemy behavior.

### Ownership

- CombatService owns Player/Enemy Turn rules, action validation, AP spending, one-action-per-Party-member-per-Round eligibility, EnemyPattern resolution and terminal conditions.
- CombatSessionService coordinates the entry action, encounter completion costs and one-time world-time feedback. GameClock does not advance per action or Round.
- CombatSnapshot, CombatActionRequest, CombatResult, CombatEvent and ICombatService are Domain contracts in Scripts/Domain/Combat/CombatContracts.cs. Presentation depends on these lower-level contracts; Domain does not depend on Presentation.
- CombatPresentationMapper converts Domain results into CombatViewData. CombatPanel renders that data and emits actor/action/target requests.
- The latest snapshot replaces current status and available actions; new messages append to history. Events remain in the exact order resolved by Domain.

### Reusable presentation components

Combat UI components consume presentation contracts and do not access Domain entities or services directly:

| Component | Input contract | Responsibility |
|---|---|---|
| EnemyCard | EnemyCombatStatus | Render Enemy name, HP and condition; expose target-selection state and selection event |
| PartyMemberCard | PartyCombatStatus | Render one Party member's status and current-round action eligibility |
| CombatActionCard | CombatActionOption | Render an actor-labelled action and emit the selected option |
| CombatMessageEntry | CombatMessage | Render one historical combat message |

Parent panels own component collections and lifecycle. They create, update, reorder and release instances by stable entity ID. A component receives a presentation object through Set... methods; it must not look up Character, query CombatService, parse content files or decide whether an action is legal. Components may expose UI events, but the owning controller/service revalidates the request.

CombatPresentationMapper also maps resolved action events into CombatFeedbackViewData. The orchestrator routes those cues to the matching PartyMemberCard or EnemyCard. Attack, Guard, Evade, Catch Breath, Revive and Hit currently use short visual color pulses; these are presentation-only and do not affect combat resolution.

### Message semantics

Messages use Player, Enemy or System identity. Speaker/category labels remain visible; alignment or color is not the only identity cue. Completed events are historical facts, while observations/inferences must be presented as such.

The current prototype uses exact Enemy HP in status and records damage deltas in the log. The log is bounded and preserves reading position when the user scrolls older messages. Target selection is confirmed before execution; invalid targets are not selectable and the Sidebar cursor changes only for valid EnemyCards.

## 4. Character Panel

The current Character Panel displays Health, Fatigue, Stamina, Mood and Gold. These are current prototype resources, not a commitment to the final game HUD.

The panel presents state; it does not own resource rules or mutate the Character.

## 5. Mode transitions

- Exploration → Combat: show the dedicated Combat Panel.
- During Combat: replace status/action data from the latest view.
- Combat → Exploration: return to the current Location's Exploration presentation.
- Character Panel remains persistent.

MainController composes the screen and top-level signals. ExplorationFlowController and CombatFlowController own their respective presentation flows and delegate gameplay rules to Application/Domain services.

## 6. Current limitations

The current UI is a prototype, not a final interface specification. The playable MainController encounter is wired with Alex, Noah, Mira and Sera against the Unit 213 manifestations. The current Enemy patterns are fixed authored action sequences; richer conditional behavior rules, sequential animation/event playback and polished Party actor selection remain incomplete. Interactive Godot acceptance is still required to verify target selection, action feedback, End Player Turn and the complete encounter flow. Arousal/Tease/Orgasm is still an MVP runtime follow-up. Broad skill selection, inventory, relationships, diary, responsive narrow-screen behavior and full assistive-technology verification remain incomplete.

When the intended UI changes, first update the relevant Design document if the player experience changes; update this document only when the current presentation contract changes.
