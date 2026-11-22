# Presentation User Interface and Contracts

**Status:** Describes current UI composition and presentation contracts  
**Boundary:** Player experience goals live in [Combat Design](../Design/Combat.md); this document describes the current implementation and layer contracts.

## 1. Main screen composition

| Region | Exploration | Combat |
|---|---|---|
| Sidebar | Navigation and location context | Compact Enemy cards and target selection |
| Main Panel | Story and available interactions | Combat header, Party roster, AP status, action flow and combat log |
| Character Panel | Full Player profile: HP, Fatigue, Stamina, Mood and Gold | Active Party member focus: HP, Stamina and Arousal only when real data is available |

The Main Panel switches between Exploration and Combat. The persistent Character Panel switches its display mode rather than showing the full Exploration profile during combat.

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

The UI renders the groups and ordering supplied by its view data. It does not decide which actions are valid, inspect JSON or mutate Domain state. The owning service revalidates every request because displayed availability is advisory and state may have changed.

## 3. Combat presentation

Combat uses a dedicated Combat Panel. It does not reuse Exploration choice groups and does not display Timeline markers or future Enemy Intent.

Current flow:

```text
CombatPanel
  -> CombatFlowController
  -> CombatSessionService
  -> CombatService
  -> CombatSnapshot / CombatResult / ordered CombatEvent[]
  -> CombatPresentationMapper
  -> Presentation-owned CombatViewData
  -> CombatPanel + Sidebar + CharacterPanel
```

The combat interface presents:

- **Header:** encounter title, Round, Player/Enemy phase and shared Party AP.
- **Enemy area:** compact Enemy cards with name, HP and condition. Each card has a target-selection state; an optional Arousal meter is displayed only when a real value is supplied.
- **Party roster:** all four Party members remain visible. Cards display HP, condition, Stamina and current-Round action eligibility. Arousal is optional and stays hidden while the runtime provides no value.
- **Active character:** the Character Panel focuses the selected Party member. In Combat mode it hides Exploration-only Fatigue, Mood and Gold, and displays HP, Stamina and Arousal only when available. During the Enemy Turn it shows a Party-on-standby state.
- **Action flow:** category filters → action list → selected-action details → confirmation → target selection when required. Unavailable actions remain inspectable so their disabled reason can be read.
- **Combat log:** newly mapped messages append in the exact order of resolved Domain events.
- **End Player Turn:** remains available throughout the Player Turn; the Domain also starts Enemy Turn automatically when AP reaches zero.

### Presentation ownership and mapping

- `CombatPresentationMapper` converts Domain snapshots/results into Presentation-owned classes in `Scripts/Presentation/ViewModels/Combat/CombatPresentation.cs`.
- Reusable controls consume `EnemyCardViewData`, `PartyMemberCardViewData`, `CombatActionCardViewData`, `CombatMessage` and `CombatFeedbackViewData`. UI controls do not receive Domain `EnemyCombatStatus`, `PartyCombatStatus` or `CombatActionOption` objects directly.
- `CombatViewData` contains a boolean `IsPlayerTurn`, Round, AP values, Presentation-owned Enemy/Party/action arrays, messages and feedback. Domain enums and contracts do not leak into the reusable UI controls.
- `EnemyCard`, `PartyMemberCard`, `CombatActionCard` and `CombatActionButton` render supplied values and emit selection events/IDs only. Parent panels own instance lifetimes and selected state.
- `CombatActionSelection` is a Presentation-owned intent containing only `ActionId`, `ActorId` and `TargetId`. `MainController` converts it into the Domain `CombatActionRequest` at the composition boundary. CombatService revalidates actor, action, target, AP and Stamina before mutation.
- `CombatActionCardViewData` contains category, target kind, valid targets, cost, description and availability reason. The mapper groups the currently authored actions: Attack variants under Offense, Guard/Evade under Defense and Recover/Revive under Support.
- Target validity comes from the current action options returned by Domain. During selection the Sidebar highlights only valid target IDs; valid cards use a pointing-hand cursor, invalid cards are dimmed and ignore clicks. A successful target selection produces a short selected-state cue. Cancelling clears the visual state.
- The mapper emits distinct cues for Quick Strike, Heavy Strike, Disrupting Strike, Guard, Evade, Catch Breath, Revive and Hit. The cue is visual feedback only; it never changes combat resolution.
- Presentation view data reserves nullable Arousal values. Current Enemy/Party combat contracts do not yet supply Arousal; therefore the Arousal labels/bars remain hidden rather than displaying fabricated placeholder values. Issue #134 owns the runtime meter and events.

### Component responsibilities

| Component | Input | Responsibility |
|---|---|---|
| `EnemyCard` | `EnemyCardViewData` | Render Enemy identity, HP, condition and optional Arousal; expose target hover/selected states and a selection event |
| `PartyMemberCard` | `PartyMemberCardViewData` | Render compact Party status, relevant Stamina/Arousal and action eligibility; expose actor selection |
| `PartyRosterPanel` | `PartyMemberCardViewData[]` | Own Party card collection, stable ordering and selected eligible actor |
| `CharacterPanel` | `CharacterViewModel` or `PartyMemberCardViewData` | Show the full Exploration profile or the active combat member's relevant state |
| `CombatActionCard` | `CombatActionCardViewData` | Render an action and emit selection for the details panel |
| `CombatActionPanel` | `CombatActionCardViewData[]` | Manage categories, action list, details, confirmation and target-selection states |
| `CombatMessageEntry` | `CombatMessage` | Render one historical combat message |

Components must not look up Character objects, query CombatService, parse content files or decide whether an action is legal. The owning flow/service revalidates interactions. Do not add a generic UI framework or duplicate combat state.

### Message semantics

Messages use Player, Enemy or System identity. Speaker/category labels remain visible; alignment or color is not the only identity cue. Completed events are historical facts, while observations/inferences must be presented as such.

The current prototype uses exact Enemy HP in status and records damage deltas in the log. The log is bounded and preserves reading position when the user scrolls older messages. Enemy-targeting actions are confirmed before target selection; invalid targets are not selectable.

## 4. Character Panel modes

**Exploration mode:** displays Health, Fatigue, Stamina, Mood and Gold from the current `CharacterViewModel`.

**Combat mode:** displays only the selected Party member's name, condition, HP and Stamina. Arousal appears only when the combat runtime supplies a value. The generic portrait is hidden because current Party combat view data does not provide separate portraits for every member. During Enemy Turn, the panel shows that the Party is on standby.

The panel presents state; it does not own resource rules or mutate the Character.

## 5. Mode transitions

- Exploration → Combat: show the dedicated Combat Panel, Enemy cards and active-Party focus.
- During Combat: replace status/action data from the latest view; append new event messages.
- Player → Enemy Turn: clear actor selection and show the Party-on-standby state.
- Enemy Turn → next Player Turn: select the first eligible Party member unless the player has already selected another eligible member.
- Combat → Exploration: return to the current Location's Exploration presentation and restore the full Character Panel.

MainController remains the composition root. ExplorationFlowController and CombatFlowController own their respective presentation flows and delegate gameplay rules to Application/Domain services.

## 6. Current limitations

This is a functional prototype interface, not a final visual specification. The MainController encounter uses Alex, Noah, Mira and Sera against the Unit 213 manifestations. The current Enemy patterns are fixed authored action sequences. Sequential event/animation playback is still a follow-up; ordered Domain results are currently presented synchronously.

Arousal/Tease/Orgasm remains an MVP runtime task under Issue #134. The UI can display an optional Arousal value once the Domain snapshot and mapper supply it, but it does not invent values or implement threshold behavior. Interactive Godot acceptance is still required for action confirmation, target hover/selection/cancel, card focus, feedback cues, End Player Turn and the full Unit 213 encounter.

Broad skill selection, inventory, relationships, diary, responsive narrow-screen behavior and full assistive-technology verification remain incomplete.

When the intended player experience changes, update the relevant Design document first; update this document when the current UI contract or implementation changes.
