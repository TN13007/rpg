# Open Design Questions

**Purpose:** Keep undecided choices visible so reviewers do not mistake assumptions for requirements.  
**Status:** Living document. Update a question when a decision is made.

For external review, prioritize questions that affect the current UI prototype. Do not assume a proposed answer is already approved.

## 1. Main Screen and Navigation

| ID | Question | Current position | Status |
|---|---|---|---|
| UI-01 | Is a dedicated Map page necessary when the Main Panel already presents the current location and its available interactions? | Map is still being considered; no separate Explore page is currently planned. | Open |
| UI-02 | Should the proposed Sidebar groups (Character, Diary, System) be kept as-is? | Current taxonomy is a proposal. | Open |
| UI-03 | Should the sidebar show navigation labels permanently, collapse, or support both modes? | Not specified. | Open |
| UI-04 | How should the Main Panel present study, rest, social interaction and other non-combat activities? | Each activity may need a focused view; no common pattern has been selected. | Open |

## 2. Exploration Experience

| ID | Question | Current position | Status |
|---|---|---|---|
| EXP-01 | How much of the location should be represented by illustration versus text? | Text-first with illustrative visuals. Exact balance is undecided. | Open |
| EXP-02 | Should NPCs and objects appear as inline choices, cards, lists or another interaction pattern? | They must be visible and discoverable; exact control style is undecided. | Open |
| EXP-03 | How should time be displayed and how clearly should actions communicate time cost? | Time is relevant context, but the schedule and time-cost rules are not finalized. | Open |

## 3. Character State and Diary

| ID | Question | Current position | Status |
|---|---|---|---|
| CHAR-01 | What numerical ranges, recovery rules and thresholds should Health, Energy and Mood use? | The three indicators are intended to remain visible; detailed rules are undecided. | Open |
| CHAR-02 | Which additional character values belong on Profile or Skills pages rather than the persistent panel? | Persistent panel should stay minimal. | Open |
| DIARY-01 | Which events should become journal entries, and which should remain transient UI history? | Diary includes Current Goals and Journal Entries; recording rules are undecided. | Open |

## 4. Combat Presentation

| ID | Question | Current position | Status |
|---|---|---|---|
| COM-01 | Should internal thoughts have a fourth message style or remain categorized System messages? | Current proposal uses categorized System messages. | Open |
| COM-02 | Does the log need round separators, timestamps or grouped events? | Not decided. |
| COM-03 | Should enemy HP be exact, approximate or hidden under some conditions? | Current preview shows HP as an example; actual information rules are undecided. | Open |
| COM-04 | How should target selection work when an action can affect multiple enemies? | Not implemented or specified. | Open |
| COM-05 | How should intent confidence be communicated without revealing hidden AI actions? | Proposed states: Not detected, Suspected, Recognized. Visual treatment needs review. | Open |
| COM-06 | Should the log auto-scroll when a new event arrives if the player is reading older messages? | Not decided. | Open |
| COM-07 | How should message types remain distinguishable for color-blind players and different accessibility needs? | Alignment is proposed, but alignment alone may not be sufficient. | Open |

## 5. Review Priorities

For the current prototype, review in this order:

1. **Critical usability:** Can a player understand the current context, status and available action?
2. **Information fairness:** Does the UI reveal only what the character can know?
3. **Information hierarchy:** Are current state, history, observation, inference and outcome distinguishable?
4. **Navigation clarity:** Does the Main Panel own exploration without redundant sidebar destinations?
5. **Implementation scope:** Are component boundaries useful without creating unnecessary complexity?

## 6. Rules for Updating This Document

When a question is resolved:
- Move the decision into the relevant design document.
- Replace the question's current position with the approved decision.
- Change its status to **Decided** and link to the decision location where practical.

When reviewing the project:
- Do not silently decide an open question on the user's behalf.
- Explain the trade-off and recommend one option when useful.
- Separate a necessary decision from an optional enhancement.
