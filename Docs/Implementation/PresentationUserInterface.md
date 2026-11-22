# Presentation User Interface and Contracts

**Status:** Describes the current Combat HUD structure and Presentation contracts  
**Boundary:** Player experience goals live in Combat Design; this document describes the current implementation and layer contracts.

## 1. Main screen composition

| Region | Exploration | Combat |
|---|---|---|
| Sidebar | Navigation and location context | Hidden |
| Main Panel | Story and available interactions | Three-region Combat HUD |
| Character Panel | Full Player profile: HP, Fatigue, Stamina, Mood and Gold | Hidden; active member is represented by the Combat portrait |

The Main Panel switches between Exploration and Combat. The Exploration sidebar and full Character Panel are hidden while the Combat HUD is active, then restored when the encounter ends.

## 2. Exploration presentation

Exploration displays current Location context and available choices. Choices are discovered by Application queries and mapped into grouped, presentation-safe data. StoryPanel renders Location prose in a scrollable RichTextLabel, so long first-visit introductions remain readable without pushing choices off-screen. The Exploration flow chooses firstVisit on a Location's first entry during the current process and returnVisit on later entries; the selected text stays stable during the current stay. Visit history is not persisted across restarts.

Current pipeline:

    AvailableInteractionQuery
      -> ActionChoiceMapper
      -> ActionChoiceViewData
      -> StoryPanel
      -> ActionSelectionRequest
      -> ExplorationFlowController

The UI renders the groups and ordering supplied by its view data. It does not decide which actions are valid, inspect JSON or mutate Domain state. The owning service revalidates every request because displayed availability is advisory and state may have changed.

## 3. Combat presentation

Combat uses a dedicated full-area Combat Panel. It does not reuse Exploration choice groups and does not display Timeline markers or future Enemy Intent.

Current composition:

    CombatPanel
      ├── EnemyRegion
      │     └── EnemyCard × N
      ├── BattlefieldRow
      │     └── CharacterPortrait
      ├── PartyRosterPanel (Party Bar)
      │     └── PartyMemberCard × 4
      └── CombatActionPanel
            ├── ActionCategoryList
            ├── ActionList
            └── ActionDetailsPanel

    CombatFlowController
      -> CombatSessionService
      -> CombatService
      -> CombatSnapshot / CombatResult / ordered CombatEvent[]
      -> CombatPresentationMapper
      -> Presentation-owned CombatViewData
      -> CombatPanel

### Three screen regions

- **Enemy region:** reusable Enemy cards show only name, HP and optional Lust. The Enemy Domain contract does not yet provide Arousal/Lust, so that meter is hidden. Enemy condition is used internally to disable defeated targets; it is not displayed as a third resource.
- **Character portrait:** a dedicated portrait scene displays the currently selected Party member. Its view data supports base pose/stance, expression, outfit and status-overlay image layers plus descriptive IDs. The current data mapper uses the generic placeholder portrait because the project does not yet contain separate character art/layer assets.
- **Party and action region:** PartyRosterPanel is the horizontal Party Bar, containing four PartyMemberCard controls. Each card displays name, HP and optional Lust/MP; it does not display a character icon. Lust and MP remain hidden while no runtime values are supplied. CombatActionPanel contains three child scenes: vertical category list, skill/action list and details panel.

### Presentation ownership and mapping

- CombatPresentationMapper converts Domain snapshots/results into Presentation-owned classes in Scripts/Presentation/ViewModels/Combat/CombatPresentation.cs.
- Reusable controls consume EnemyCardViewData, PartyMemberCardViewData, CharacterPortraitViewData, CombatActionCardViewData, CombatMessage and CombatFeedbackViewData. UI controls do not receive Domain EnemyCombatStatus, PartyCombatStatus or CombatActionOption objects directly.
- CombatViewData contains Round, Player Turn state, shared AP values, Presentation-owned Enemy/Party/action arrays, messages and feedback.
- EnemyCard renders Enemy name, HP and optional Lust. PartyMemberCard renders name, HP and optional Lust/MP. Underlying Stamina remains in Party view data because the current Domain action rules spend Stamina, but the Party Bar does not show it.
- CharacterPortrait receives visual data and loads supplied resource paths. It does not infer combat pose or alter Domain state. The mapper currently supplies a generic fallback portrait and condition-based stance labels; dedicated art asset selection remains future content work.
- CombatActionSelection is a Presentation-owned intent containing only ActionId, ActorId and TargetId. MainController converts it into the Domain CombatActionRequest at the composition boundary. CombatService revalidates actor, action, target, AP and Stamina before mutation.
- Action categories are Attack, Defence, Allure, Inventory and Run. Current authored attacks map to Attack; Guard, Evade, Catch Breath and Revive map to Defence. Allure, Inventory and Run are reserved for their authored action IDs and may have no entries yet.
- Selecting a non-target action emits ActionSelected immediately. There is no confirmation step. Enemy-targeted actions request valid target IDs from the action options; CombatPanel forwards these to EnemyRegion, which highlights only those IDs. A valid Enemy click returns the target ID and submits the action. Ally-targeted actions show a short list of valid Party targets in the action-list column.
- Cancelling target selection clears Enemy highlights and restores the action list. Invalid Enemy cards are dimmed, use the normal cursor and ignore clicks.
- The mapper emits distinct cues for Quick Strike, Heavy Strike, Disrupting Strike, Guard, Evade, Catch Breath, Revive and Hit. The cue is visual feedback only; it never changes combat resolution.
- Lust (Domain Arousal) and MP are nullable view-data fields. Current Domain snapshots do not provide these values; labels and bars stay hidden rather than displaying fabricated numbers. Issue #134 owns the Arousal runtime. MP remains a future character-resource integration.

### Component responsibilities

| Component | Responsibility |
|---|---|
| CombatPanel | Composes the three regions, connects UI events, applies CombatViewData and presents Round/AP state |
| EnemyRegion | Owns EnemyCard instances, valid-target highlights and Enemy selection events |
| EnemyCard | Shows Enemy name, HP and optional Lust; exposes target selection and feedback |
| PartyRosterPanel | Owns four PartyMemberCard instances, card ordering and selected actor |
| PartyMemberCard | Shows name, HP and optional Lust/MP; emits eligible actor selection |
| CharacterPortrait | Displays the selected Party member's portrait layers and visual-state labels |
| CombatActionPanel | Coordinates category, action list, details, immediate execution and target selection |
| CombatActionCard | Shows one action and emits its selection |
| CombatMessageEntry | Renders one historical combat message |

The category, list and details columns are separate instanced scenes owned by CombatActionPanel. They do not need C# scripts while their scene structure and parent-owned behavior are sufficient. Components must not look up Character objects, query CombatService, parse content files or decide whether an action is legal.

### Message semantics

Messages retain Player, Enemy or System identity and preserve the order of resolved Domain events. The CombatMessageLog scene remains connected for event history but is hidden from the primary three-region HUD to preserve the agreed screen composition. Completed events are historical facts, while observations/inferences must be presented as such.

## 4. Mode transitions

- Exploration → Combat: show the Combat Panel and hide the Exploration Sidebar and Character Panel.
- During Combat: replace Enemy/Party/action/portrait data from the latest mapped snapshot; append new event messages to the retained combat log.
- Selecting a different eligible Party member updates the portrait and filters the available actions to that actor.
- After a Party member acts, focus moves to another eligible member when one exists. During Enemy Turn, the current living member remains visually focused even though Party actions are disabled.
- Combat → Exploration: hide the Combat Panel, restore the Sidebar and full Character Panel, then present the current Location.

MainController remains the composition root. ExplorationFlowController and CombatFlowController own their respective presentation flows and delegate gameplay rules to Application/Domain services.

## 5. Current limitations

This is a playable prototype interface, not final art. The MainController encounter uses Alex, Noah, Mira and Sera against Unit 213 manifestations. Enemy patterns are fixed authored action sequences. Sequential event/animation playback remains a follow-up; ordered Domain results are currently presented synchronously.

The current Domain contracts provide HP and Stamina for Party members and HP for Enemies. They do not yet provide Lust/Arousal or MP, so those meters remain hidden. Current action costs can still consume Stamina; the skill details display the authored cost. Issue #134 owns the Arousal/Tease/Orgasm runtime. The portrait data contract supports pose, expression, outfit and status layers, but the current game content supplies only a generic placeholder asset.

Interactive Godot acceptance remains required for the full journey: one and multiple Enemies, all four Party cards, portrait changes, optional meter visibility, immediate no-target action, valid/invalid Enemy target selection, Ally target selection, cancelling, action feedback, End Turn and return to Exploration. Responsive narrow-screen behavior and assistive-technology verification remain incomplete.
