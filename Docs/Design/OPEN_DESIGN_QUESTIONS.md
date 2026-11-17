# Open Design Questions

**Project/session status and handoff:** Maintained centrally in [WORKFLOW.md](../../WORKFLOW.md). This file remains the owner of detailed gameplay, UI and world-design questions.

**Purpose:** Keep undecided choices visible so reviewers do not mistake assumptions for requirements.  
**Status:** Living document. Update a question when a decision is made.

## 1. Main Screen and Navigation

| ID | Question | Current position | Status |
|---|---|---|---|
| UI-01 | Is a dedicated Map page necessary when the Main Panel already presents the current location and its available interactions? | Map is still being considered; no separate Explore page is planned. | Open |
| UI-02 | Should the proposed Sidebar groups (Character, Diary, System) be kept as-is? | Current taxonomy is a proposal. | Open |
| UI-03 | Should the sidebar show navigation labels permanently, collapse, or support both modes? | Not specified. | Open |
| UI-04 | How should the Main Panel present study, rest, social interaction and other non-combat activities? | Each activity may need a focused view; no common pattern has been selected. | Open |

## 2. Exploration Experience

| ID | Question | Current position | Status |
|---|---|---|---|
| EXP-01 | How much of the location should be represented by illustration versus text? | Text-first with illustrative visuals. Exact balance is undecided. | Open |
| EXP-02 | Should NPCs and objects appear as inline choices, cards, lists or another interaction pattern? | They must be visible and discoverable; exact control style is undecided. | Open |
| EXP-03 | How should time be displayed and how clearly should actions communicate time cost? | The clock, activity durations and stat modifiers are implemented. Schedules and broader time-based world events remain undecided; location-arrival Event selection has a separate prototype rule. | Open |

## 3. Character State and Diary

| ID | Question | Current position | Status |
|---|---|---|---|
| CHAR-01 | What numerical ranges, recovery rules and thresholds should Health, Energy and Mood use? | The three indicators remain visible; detailed recovery rules are undecided. | Open |
| CHAR-02 | Which additional character values belong on Profile or Skills pages rather than the persistent panel? | Persistent panel should stay minimal. | Open |
| DIARY-01 | Which events should become journal entries, and which should remain transient UI history? | Diary includes Current Goals and Journal Entries; recording rules are undecided. | Open |

## 4. Combat

| ID | Question | Current position | Status |
|---|---|---|---|
| COM-01 | How should intent confidence be communicated visually? | Use Suspected / Recognized labels; exact visual treatment can be playtested. | Open |
| COM-02 | What narrow-screen layout should Combat use? | Current preview is desktop-first; stacking and minimum widths are not finalized. | Open |
| COM-03 | What exact action-target selection flow is required for multi-target actions? | Not implemented or specified. | Open |
| COM-04 | How should combat rules, action costs, damage formulas and turn progression be balanced and expanded? | A deterministic single-enemy prototype is implemented; final balance and expanded mechanics remain undecided. | Open |
| COM-05 | Which accessibility checks and assistive-technology targets must be supported? | Keyboard focus is enabled; full screen-reader and text-scaling behavior needs testing. | Open |

## 5. Resolved Decisions

| ID | Decision | Source |
|---|---|---|
| COM-R01 | Keep three message types: Player, Enemy and System. Thoughts and inferences are categorized System messages. | [Combat Panel Contract](COMBAT_PANEL_REVIEW.md) |
| COM-R02 | Use visible speaker and category labels; alignment and color are not the only identity cues. | [Combat Panel Contract](COMBAT_PANEL_REVIEW.md) |
| COM-R03 | Show exact enemy HP in the current prototype. Status shows current HP; log records damage deltas. | [Combat Panel Contract](COMBAT_PANEL_REVIEW.md) |
| COM-R04 | Use a styled System message for round separators; timestamps are not required. | [Combat Panel Contract](COMBAT_PANEL_REVIEW.md) |
| COM-R05 | Bound the message log to 60 messages and auto-scroll only when already at the bottom. | [Combat Panel Contract](COMBAT_PANEL_REVIEW.md) |
| COM-R06 | UI receives only a presentation-safe snapshot; hidden AI decisions are never passed to UI. | [Combat Panel Contract](COMBAT_PANEL_REVIEW.md) |
| COM-R07 | Action availability is replaced from the latest snapshot; gameplay service revalidates every request. | [Combat Panel Contract](COMBAT_PANEL_REVIEW.md) |
| COM-R08 | A character with 0 HP is dead and cannot start an encounter or perform gameplay actions. CombatService and MainController enforce this rule. | [Playable Journey Sprint](SPRINT_PLAYABLE_JOURNEY.md) |
| LOOP-R01 | The core loop uses Location travel and optional Location Events; no Expedition or Run wrapper is required. | [Gameplay Loop and Location Events](GAME_LOOP_AND_LOCATION_EVENTS.md) |
| LOOP-R02 | A Location owns its Event Definitions. An Event is not automatically an Encounter or Combat; the player chooses a resolution. | [Gameplay Loop and Location Events](GAME_LOOP_AND_LOCATION_EVENTS.md) |
| LOOP-R03 | For this prototype, evaluate a Location's Event pool after successful arrival, at most once per in-game day, only while the player can act. The first successful chance in authored order is selected. | [Gameplay Loop and Location Events](GAME_LOOP_AND_LOCATION_EVENTS.md) |
| LOOP-R04 | Event choices execute through ActionExecutionService or reference an existing Location Action that starts the relevant Encounter/Combat flow. | [Gameplay Loop and Location Events](GAME_LOOP_AND_LOCATION_EVENTS.md) |

## 6. Review Priorities

1. **Critical usability:** Can a player understand current context, status and available action?
2. **Information fairness:** Does the UI reveal only what the character can know?
3. **Information hierarchy:** Are current state, history, observation, inference and outcome distinguishable?
4. **Navigation clarity:** Does the Main Panel own exploration without redundant sidebar destinations?
5. **Implementation scope:** Are component boundaries useful without unnecessary complexity?

## 7. Rules for Updating This Document

When a question is resolved, move the decision into its owning design document and mark it Decided here. Do not silently decide an open question on the user's behalf.
