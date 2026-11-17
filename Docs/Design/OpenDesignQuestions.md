# Open Design Questions

**Purpose:** Keep undecided choices visible so reviewers do not mistake assumptions for requirements.  
**Status:** Living document. Update a question when a decision is made.

## 1. Main Screen and Navigation

| ID | Question | Current position | Status |
|---|---|---|---|
| UI-01 | Is a dedicated Map page necessary when the Main Panel already presents the current Location and interactions? | No separate Explore page; Map page remains undecided. | Open |
| UI-02 | Should the Sidebar groups (Character, Diary, System) be kept as-is? | Current taxonomy is implemented; review after more content exists. | Open |
| UI-03 | Should the Sidebar show labels permanently, collapse, or support both modes? | Not specified. | Open |
| UI-04 | How should study, rest, social interaction and other non-combat activities be presented? | Version 1 uses grouped Exploration choices and result feedback; specialized activity views remain possible later. | Decided for V1 / future open |

## 2. Exploration Choices and World Context

| ID | Question | Current position | Status |
|---|---|---|---|
| EXP-01 | How much should a Location use illustration versus text? | Text-first with illustrative visuals. Exact balance remains open. | Open |
| EXP-02 | How should NPCs and objects appear as interactions? | Their choices will be discovered contextually and rendered in the Nearby group; exact card/list treatment can be refined when NPC/Object content is implemented. | Decided direction |
| EXP-03 | How are action definitions separated from the context that makes them available? | Player, Location, Travel, Target and Event are distinct sources; Available Interaction Query determines contextual choices. | Decided |
| EXP-04 | Should every action source use a different Action class? | Use common definition fields for shared data, while specialized rule owners handle unique behavior. Do not make a universal executor. | Decided |
| EXP-05 | Does Loader own action availability or does Mapper decide what appears? | Loaders parse definitions; Query resolves contextual availability; Mapper creates presentation data. | Decided |
| EXP-06 | Should Action IDs be globally unique? | No global uniqueness assumption. Choice identity includes source kind, source ID, action ID and optional target ID. | Decided |
| EXP-07 | Should an active Event show ordinary Location and Travel choices? | No for V1; keep Event as a focused situation until resolved. | Decided for V1 |
| EXP-08 | Should target/NPC/Object action systems be implemented in Version 1? | No. Session 3 creates only a clean extension point; NPC, Item, Container and Puzzle systems remain deferred. | Decided |
| EXP-09 | Should Location exits immediately become a separate Connection model? | No. Keep current exits for V1; introduce independent Connection data only when route-specific metadata/rules require it. | Decided for V1 |

## 3. Character State and Diary

| ID | Question | Current position | Status |
|---|---|---|---|
| CHAR-01 | What numerical ranges, recovery rules and thresholds should Health, Energy and Mood use? | V1 starts with Health 0–20, Energy 0–10, Mood 0–100 for the current Player; tune in playtests. | Initial baseline |
| CHAR-02 | Which values belong on Profile or Skills rather than the persistent panel? | Persistent panel stays minimal: Health, Energy, Mood and Gold. | Decided for current UI |
| DIARY-01 | Which events should become journal entries, and which should remain transient UI history? | V1 summary records only the current day's completed activities; permanent Diary history remains future work. | Decided for V1 |

## 4. Combat

| ID | Question | Current position | Status |
|---|---|---|---|
| COM-01 | How should intent confidence be shown? | Suspected / Recognized labels; exact visual treatment can be playtested. | Open |
| COM-02 | What narrow-screen layout should Combat use? | Current preview is desktop-first; stacking/minimum widths are not finalized. | Open |
| COM-03 | What action-target flow is required for multi-target actions? | Not implemented or specified. | Open |
| COM-04 | How should combat rules, costs, damage and turns be balanced? | Deterministic single-enemy prototype exists; final balance remains open. | Open |
| COM-05 | Which accessibility checks and assistive-technology targets are required? | Keyboard focus exists; screen-reader/text-scaling behavior needs testing. | Open |
| COM-06 | How do Skills relate to Combat Actions? | Skills represent learned capabilities; Combat Actions are operations unlocked or modified by them. Combat remains a separate pipeline. | Decided direction |

## 5. Resolved Decisions

| ID | Decision | Source |
|---|---|---|
| UI-R01 | Keep the Character, Diary and System Sidebar taxonomy; the Main Panel owns Location exploration. | [UI Architecture](UiArchitecture.md) |
| COM-R01 | Keep three message types: Player, Enemy and System. Thoughts and inferences are categorized System messages. | [Combat Panel Contract](CombatPanelReview.md) |
| COM-R02 | Use visible speaker and category labels; alignment and color are not the only identity cues. | [Combat Panel Contract](CombatPanelReview.md) |
| COM-R03 | Show exact enemy HP in the current prototype. Status shows current HP; log records damage deltas. | [Combat Panel Contract](CombatPanelReview.md) |
| COM-R04 | Use a styled System message for round separators; timestamps are not required. | [Combat Panel Contract](CombatPanelReview.md) |
| COM-R05 | Bound the message log to 60 messages and auto-scroll only when already at the bottom. | [Combat Panel Contract](CombatPanelReview.md) |
| COM-R06 | UI receives only a presentation-safe snapshot; hidden AI decisions are never passed to UI. | [Combat Panel Contract](CombatPanelReview.md) |
| COM-R07 | Combat action availability is replaced from the latest snapshot; CombatService revalidates every request. | [Combat Panel Contract](CombatPanelReview.md) |
| COM-R08 | A character with 0 HP is dead and cannot start an encounter or perform gameplay actions. | [Playable Journey Sprint](SprintPlayableJourney.md) |
| LOOP-R01 | The core loop uses Location travel and optional Location Events; no Expedition or Run wrapper is required. | [Gameplay Loop and Location Events](GameLoopAndLocationEvents.md) |
| LOOP-R02 | A Location owns its Event Definitions. An Event is not automatically an Encounter or Combat; the player chooses a resolution. | [Gameplay Loop and Location Events](GameLoopAndLocationEvents.md) |
| LOOP-R03 | Evaluate a Location's Event pool after successful arrival, at most once per in-game day, only while the Player can act. | [Gameplay Loop and Location Events](GameLoopAndLocationEvents.md) |
| LOOP-R04 | Event choices execute through the normal action path or reference an action that starts the relevant Encounter/Combat flow. | [Gameplay Loop and Location Events](GameLoopAndLocationEvents.md) |
| ACT-R01 | Exploration choices come from distinct sources (Player, Location, Travel, Target and Event) and are grouped for presentation. | [Exploration Action Choice Pipeline](ExplorationActionPipeline.md) |
| ACT-R02 | Loaders parse content; Query selects contextually available choices; Mapper creates ViewModels; UI renders them. | [Exploration Action Choice Pipeline](ExplorationActionPipeline.md) |
| ACT-R03 | Combat Actions and Skills remain separate from the Exploration action-choice pipeline. | [Exploration Action Choice Pipeline](ExplorationActionPipeline.md) |

## 6. Review Priorities

1. **Critical usability:** Can the Player understand context, status and available choices?
2. **Information fairness:** Does the UI reveal only what the character can know?
3. **Information hierarchy:** Are current state, history, observation, inference and outcome distinguishable?
4. **Navigation clarity:** Does the Main Panel own exploration without redundant sidebar destinations?
5. **Implementation scope:** Are boundaries useful without unnecessary abstractions?

## 7. Rules for Updating This Document

When a question is resolved, move the decision into its owning design document and mark it Decided here. Do not silently decide an open question on the user's behalf.
