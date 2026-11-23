# Daily Life Content — First Playable Day

**Status:** Proposed content baseline  
**Last updated:** 2026-11-23  
**Depends on:** [Daily Life](DailyLife.md), [Progression](Progression.md), [Game Vision](GameVision.md)

This document turns the Loop 2 design into one concrete, reviewable content slice. It reuses the current student characters, locations, actions and NPC routines wherever possible. Proposed schedule gates, costs, progression outcomes and the starting location are design targets; they are not implemented runtime behavior.

## 1. Content goal

Prove that a structured Monday and a flexible Saturday give the player different but coherent ways to spend time without requiring a paranormal incident or Combat.

The day must demonstrate:
- A morning route with a real arrival-time decision.
- One scheduled class that can be attended or missed.
- Existing NPCs appearing at useful times and places.
- A flexible study/meal choice.
- An afternoon choice between earning money and a social activity.
- A small, understandable expense and a recovery option.
- A return home and an optional conversation before Sleep.

This is a vertical content slice, not a complete academic timetable or a full week of authored life.

## 2. Starting state

| Field | First-day target | Existing player data |
|---|---|---|
| Campaign day | Day 1, Monday, 1 September 2025 | `GameClock` derives the weekday from the configured Day 1 date |
| Start time | 07:30 | 07:30 |
| Starting Location | `dormitory_room` | The current prototype branch starts Alex in `dormitory_room`; `main` still starts at `university_campus` until Issue #167 is merged |
| Health | 20 / 20 | 20 / 20 |
| Fatigue | 0 / 100 | 0 / 100 |
| Stamina | 8 / 10 | 8 / 10 |
| Mood | 70 / 100 | 70 / 100 |
| Gold | 20 | 20 |

Use the existing player identity `player_student` (Alex). Changing the start Location is a first-day content decision, not a requirement to alter MainController as part of this design document.

## 3. Locations and travel graph

Reuse these existing Location IDs:

| Location ID | Role in this day |
|---|---|
| `dormitory_room` | Private room and first playable starting point; Bed, Laptop and Wardrobe targets |
| `dormitory_bathroom` | Optional bathroom space connected to the private room |
| `dormitory_lobby` | Shared dormitory entrance and route outside to Campus Plaza |
| `student_apartment` | Separate East District apartment location |
| `east_district` | Connector between housing and campus/café |
| `university_campus` | Central plaza, outdoor connector and notice board |
| `campus_library` | Study and Sera encounter |
| `lecture_hall` | Scheduled class |
| `campus_cafe` | Meal and lunchtime social opportunity |
| `corner_cafe` | Paid work shift |
| `student_union` | Club meeting and student social opportunity |
| `sports_field` | Noah's morning training and optional practice |

Each Location represents one concrete, visitable place: a building, facility or outdoor space. The existing `region` field groups those Locations into a broader area such as `NORTH CAMPUS`; it is descriptive grouping metadata, not a separate node the player travels to. The stable ID `university_campus` currently represents the physical Campus Plaza, not the whole university.

The current base Travel duration is 10 minutes per connected edge, before the existing Agility modifier. The opening route from the Campus to the Library and then the Lecture Hall is:

```text
Campus Plaza
  -> Campus Library            10 min
Campus Library
  -> Campus Plaza         10 min
Campus Plaza
  -> Lecture Hall              10 min
```

The Library-to-Lecture-Hall leg therefore takes 20 minutes through Campus Plaza. Keep the existing Location graph; do not add direct exits merely to shorten the sample.

## 4. Monday commitments

These are proposed content records. The schedule system and weekday support do not exist yet.

| Commitment ID | Location | Weekday | Allowed start | Duration | Outcome |
|---|---|---|---|---:|---|
| `monday_research_methods_lecture` | `lecture_hall` | Monday | 09:00–09:15 | 60 min | Attend the introductory lecture; current action adds Mood +1 |
| `monday_corner_cafe_shift` | `corner_cafe` | Monday | 13:00–13:15 | 180 min | Complete the shift; current action adds 30 Gold |
| `monday_club_open_meeting` | `student_union` | Monday | 15:00–15:10 | 60 min | Attend the Campus Sports Club meeting; Mood +2, MET 1.5; completion creates a club-owned membership record |

The class and work shift reuse the existing `attend_lesson` and `work_shift` actions. The 60-minute club meeting uses a proposed `attend_club_meeting` action (Mood +2; MET 1.5) and creates a typed Campus Sports Club membership record on completion. The existing 15-minute `join_club` action only asks about clubs; it is not the meeting and does not grant membership.

The work shift and club meeting intentionally overlap. The player cannot complete both. Missing either commitment has no automatic hidden penalty in this first slice. Membership is granted only when the meeting is completed; it is not granted by merely reaching the Student Union or asking about clubs. The player simply loses that occurrence and can choose another activity.

## 5. Flexible activities and small economy

| Activity | Location | Duration | Cost | Result | Status |
|---|---|---:|---:|---|---|
| `browse_library` | `campus_library` | 30 min | Free | Study; may complete the Knowledge condition in Section 7 | Existing action; progression is proposed |
| `eat_meal` | `campus_cafe` | 20 min | 5 Gold | Stamina +3, Mood +2 | Existing effects; proposed Gold cost |
| `rest` | Any suitable safe Location; prefer apartment | 30 min | Free | Fatigue -5, Stamina +3, Mood +2 | Existing player action |
| `sleep` | `dormitory_room` | 480 min | Free | Health +20, Fatigue -100, Stamina +10, Mood +10, clamped to character limits | Existing Player Action exposed through the Bed target |

The proposed meal price is 5 Gold. With 20 starting Gold, the player can afford ordinary food before working. Completing the existing shift adds 30 Gold, so work creates a clear income opportunity without introducing debt, rent or a general shop economy.

The meal price is a proposed content value, not a current runtime cost. The current `eat_meal` action does not spend Gold.

## 6. NPC presence and opportunities

Reuse the current characters and Location IDs. The proposed weekly schedule migrates the existing Mira, Noah and Sera routine entries from every day to weekdays Monday–Friday (weekday indexes 0–4). Ms. Carter retains her current every-day routine. This is a target schedule; the current `NpcSchedules.json` has no weekday field and repeats entries daily.

### Monday–Friday presence

| NPC | Time | Location | Player-facing opportunity |
|---|---|---|---|
| Mira | 07:00–09:00 | `university_campus` | Short morning conversation; she mentions seeing a neighborhood post |
| Noah | 07:00–08:30 | `sports_field` | Ask about weekend training |
| Sera | 07:00–09:00 | `campus_library` | Ask for a quiet study recommendation |
| Mira | 11:00–12:00 | `campus_cafe` | `join_mira_for_lunch` |
| Noah | 11:00–12:30 | `campus_cafe` | `talk` or ask about weekend training |
| Sera | 11:00–12:00 | `campus_cafe` | `talk` |
| Mira | 15:00–17:00 | `student_union` | Optional conversation during the club meeting |
| Noah | 15:00–18:00 | `student_union` | Optional conversation during the club meeting |
| Mira | 17:00–22:00 | `student_apartment` | Optional evening conversation after returning home |

The proposed `ask_about_weekend_training` interaction is available at `sports_field`, `campus_cafe` or `student_union`, takes 10 minutes and adds Mood +1. It records a weekend invitation owned by Social/Relationship. It is offered only while Noah is actually present; the current interaction schema supports Location restrictions and the presence query, but the invitation mutation needs a typed domain-owned implementation.

### Saturday presence

| NPC | Time | Location | Player-facing opportunity |
|---|---|---|---|
| Noah | 09:00–11:00 | `sports_field` | Present for the invited training session |
| Sera | 10:00–14:00 | `campus_library` | Optional study conversation |
| Noah | 13:00–17:00 | `riverside_park` | Optional conversation during the club outing |
| Mira | 13:00–17:00 | `riverside_park` | Optional conversation during the club outing |

These Saturday entries are additions, not new characters or Locations. Sunday has no authored additions in this slice; do not infer a Sunday routine. Weekday-aware NPC schedules and overlap validation are required before this data can be loaded: the current loader has no weekday field, and the current validator rejects overlapping clock intervals without considering weekday identity. Once weekday support exists, intervals conflict only when their weekday sets intersect.

Keep casual conversations short and contextual. They do not automatically increase an undefined Relationship score. Existing schedules without a weekday restriction continue to mean every day.
## 7. One concrete shared-Progression example

Use one discrete Knowledge entry rather than generic XP:

**Knowledge ID:** `research_methods_basics`

**Description:** The character understands the basics of checking where a claim came from, distinguishing a firsthand account from hearsay, and comparing two sources for contradictions.

**Acquisition condition (proposed):**
1. Complete `monday_research_methods_lecture`.
2. Complete `browse_library` at `campus_library` on the same day; the order of the two activities does not matter.

**Use in a later Loop 3 scenario (proposed):** Reveal a `compare_sources` investigation option when the player examines conflicting witness accounts or archived reports.

This is a concrete Knowledge outcome, not a universal XP reward. Attending class alone, repeating the Library action, or chatting with an NPC does not grant it. The Knowledge definition and Investigation use still require implementation and acceptance; this document does not claim they exist.

The rumor from `lunch_table_conversation.join_the_conversation` or `east_district.browse_forum` may also be recorded as an optional, campaign-persistent Investigation-owned clue (`heard_unit_213_rumor`). It does not gate, trigger or suppress `apartment_213_incident`; the incident remains independently discoverable.

## 8. Example player journey — from the dormitory to the first lecture

This is the current opening route for Issue #167. It is a suggested path, not a mandatory checklist. The player can inspect the room, use the bathroom or leave immediately.

**Starting context:** Monday, 1 September 2025, 07:30, in `dormitory_room`. Alex (`player_student`) keeps the existing starting resources: 20 Gold, Health 20/20, Fatigue 0/100, Stamina 8/10 and Mood 70/100.

### 8.1 Private room

The player sees the room description and chooses a target rather than receiving every furniture action as a flat list.

| Target | Choices | Runtime status |
|---|---|---|
| Bed | Sleep | Reuses the existing 480-minute Player Action |
| Laptop | Course Portal, Email, Web Browser | Selectable placeholders; no application simulation or persistent effects |
| Wardrobe | Look through clothes | Placeholder; outfit selection is not implemented |

Selecting a target opens only the choices belonging to that object. The player can return to the room without spending time. Sleep remains a real action and advances the shared GameClock.

### 8.2 Bathroom, lobby and campus

| Approx. time | Location | Choice / movement | What happens |
|---|---|---|---|
| 07:30–07:40 | Dormitory Room → Dormitory Lobby | `Leave` | Alex leaves the private room and enters the shared lobby. |
| 07:40–07:50 | Dormitory Lobby → Campus Plaza | `Go outside` | Alex exits the dormitory and reaches the campus walkway. |
| 07:50–08:00 | Campus Plaza → Lecture Hall | `Travel to Lecture Hall` | The player can choose the Lecture Hall directly from the existing Campus Plaza exits. |
| 08:00–09:00 | Lecture Hall | `Wait 15 minutes` | The player may wait in short increments until the class window opens. |
| 09:00–10:00 | Lecture Hall | `Attend the lecture` | Attend is available from 09:00 through 09:14. The action is hidden outside that window. |

The Bathroom is an optional branch from the Dormitory Room. Visiting it and returning adds the existing Travel duration for those connections. The Lobby and Campus Plaza remain separate visitable Locations; `region` continues to group Locations and is not itself a travel node.

Travel uses the existing shared Travel action and connected Location exits. From Campus Plaza, the player can choose the Lecture Hall directly or visit the Library, Café, Student Union or Sports Field first. This is direct travel through the existing graph, not a separate fast-travel service.

The table uses the base 10-minute Travel duration for illustration. The existing Agility modifier may reduce actual travel time. The class availability window is a daily time-of-day gate for this opening prototype; weekday-specific commitment scheduling is not introduced here.

### 8.3 Classmates

When an NPC is present at the Lecture Hall, the player can choose `See around`, select a person from the people list and then choose an authored interaction. The existing NPC schedule and interaction queries remain the source of truth; this change does not add relationship progression or autonomous NPC simulation.

## 9. Saturday contrast — invitation, training, work and club outing

Saturday is a second validation slice, not a complete weekly timetable. With Day 1 set to Monday, the first Saturday is Day 6. It tests whether Monday choices can create optional weekend opportunities without turning the week into a checklist.

**Starting context:** Saturday, 08:00, at `dormitory_room`. There is no scheduled lecture. The player may train with Noah, work a shift, join a club outing, study, recover or spend the day elsewhere.

### 9.1 Actions and commitments

These are proposed authored content records. Existing action fields such as duration, effects, activity intensity and `requiredNpcId` are supported by the current Action Definition schema; the commitment schedule, weekday, eligibility, cost and completion-effect fields are not yet implemented.

| Action | Location | Duration | Authored effect / activity intensity | Additional requirement |
|---|---|---:|---|---|
| `noah_training_session` | `sports_field` | 90 min | Mood +3; MET 4 | `requiredNpcId: noah`; also requires the Social-owned weekend invitation |
| `club_riverside_outing` | `riverside_park` | 120 min | Mood +4; MET 2 | Requires club membership; proposed cost 8 Gold |

| Commitment | Action | Allowed start | Duration | Eligibility / outcome |
|---|---|---:|---:|---|
| `saturday_noah_training` | `noah_training_session` | 09:00–09:30 | 90 min | Requires the Social-owned weekend invitation and Noah present; completion records permanent `trained_with_noah` extracurricular activity evidence |
| `saturday_corner_cafe_shift` | `work_shift` | 11:00–11:15 | 180 min | Completion adds 30 Gold |
| `saturday_club_riverside_outing` | `club_riverside_outing` | 13:00–13:15 | 120 min | Requires Campus Sports Club membership and costs a proposed 8 Gold; completion is an optional club outing |

The Saturday training is not membership-gated. It requires an invitation from Noah and Noah's current presence at the Sports Field. The club outing is the membership-gated activity. These are separate eligibility rules owned by their respective domains.

The training and work windows are sequential, but travel matters: after the 09:00–10:30 training session, the base route `sports_field -> university_campus -> east_district -> corner_cafe` takes 30 minutes, reaching the café at 11:00 before applying the existing Agility reduction. If Travel arrives before the 11:00 start window opens, the player can wait until the window begins. The system must not reject an otherwise valid commitment or force an unrelated action because the player arrived early.

The work shift (11:00–14:00) and club outing (13:00–15:00) intentionally overlap. The player can complete one, not both. From the Corner Café, reaching Riverside Park also takes the existing route through East District; the player cannot start the outing inside its allowed window after completing the full shift.

### 9.2 How Monday unlocks Saturday

- Completing `monday_club_open_meeting` at the Student Union creates a permanent Campus Sports Club membership record owned by the Extracurricular domain. The current `join_club` action only asks about clubs and is not a substitute for this 60-minute meeting.
- A conversation with Noah using the proposed `ask_about_weekend_training` interaction (10 minutes, Mood +1) grants a permanent weekend invitation record owned by Social/Relationship. The interaction is available only at `sports_field`, `campus_cafe` or `student_union` while Noah is present.
- The player may skip either Monday opportunity. Without the club membership, Saturday's outing is unavailable; without Noah's invitation, Saturday training is unavailable. Neither missing opportunity applies a hidden penalty.

### 9.3 Saturday NPC presence

The proposed weekday migration restricts the existing Mira, Noah and Sera routine entries to Monday–Friday (weekday indexes 0–4). Ms. Carter keeps her current every-day routine. Saturday adds these presence entries:

| NPC | Time | Location | Purpose |
|---|---|---|---|
| Noah | 09:00–11:00 | `sports_field` | Present for the invited training session |
| Sera | 10:00–14:00 | `campus_library` | Optional study conversation |
| Noah | 13:00–17:00 | `riverside_park` | Optional conversation during the club outing |
| Mira | 13:00–17:00 | `riverside_park` | Optional conversation during the club outing |

Sunday has no authored additions in this slice; do not infer a Sunday routine. Existing schedules without a weekday restriction continue to mean every day until content explicitly migrates them.

These Saturday entries require weekday-aware NPC schedule loading and overlap validation. The current loader has no weekday field, and the current validator rejects overlapping intervals without considering day identity. Once weekdays exist, overlapping time ranges should conflict only when their weekday sets intersect.

### 9.4 Rumor remains optional information

The existing `lunch_table_conversation.join_the_conversation` and `east_district.browse_forum` may record that the player has heard the Unit 213 rumor. This is an Investigation-owned clue/information record, not a global flag. It is optional context and does **not** gate, trigger or suppress `apartment_213_incident`; the player can encounter or ignore that incident independently.

The identifiers `student_union_club_member`, `noah_weekend_invite`, `trained_with_noah` and `heard_unit_213_rumor` name typed domain facts for discussion. They are not permission to implement a generic flag bag or a generic `setFlags` API. The JSON sample's `setFlags` notation is only a draft shorthand for future domain-owned mutations.

The Saturday contrast with Monday is intentional:
- Monday establishes class/study context, creates club membership and offers the chance to receive Noah's invitation.
- Saturday turns those earlier choices into optional training and a club outing, while preserving the work-versus-outing conflict.
- Both days use the same GameClock, Character resources, travel graph and domain-owned state. No weekend clock, global flag manager or universal activity executor is introduced.

## 10. Content validation cases

The Monday slice should pass these cases:

1. Starting at the apartment and following the sample route can reach the lecture within its start window.
2. An extra morning activity can make the player miss the lecture; no hidden penalty is applied.
3. The player can skip class and still use flexible activities.
4. NPC interactions appear only when the NPC is present at the current Location and time.
5. The player can afford the proposed 5-Gold meal with the starting 20 Gold.
6. The meal spends exactly 5 Gold once; insufficient funds reject it without advancing time or applying effects.
7. Work adds 30 Gold once after the full shift resolves.
8. The player cannot complete both the 13:00 work shift and the overlapping 15:00 club meeting.
9. The Knowledge entry is granted only after both the lecture and Library study are completed on the same campaign day.
10. Sleep from 23:00 advances 480 minutes, rolls to Day 2 at 07:00 and applies recovery once.
11. The player can complete the day without visiting `apartment_block` or triggering the Unit 213 incident.
12. The club meeting creates a typed Campus Sports Club membership record; merely asking about clubs does not.
13. The Noah invitation is recorded only after the proposed interaction is selected while Noah is present at an allowed Location.
14. Hearing the Unit 213 rumor is optional and does not gate the incident.

The Saturday contrast should pass these additional cases:

15. Day 6 derives Saturday from the same GameClock when Day 1 is configured as Monday.
16. The player cannot start Noah's training without the invitation, and cannot start it when Noah is absent from `sports_field`; rejection changes no time, Fatigue, resources or progression state.
17. Training runs 09:00–10:30 and records `trained_with_noah` as typed extracurricular activity evidence without inventing a Skill-growth formula.
18. Travel from `sports_field` through Campus and East District reaches `corner_cafe` in 30 base minutes; an Agility-reduced early arrival can wait until the 11:00 start window.
19. The 11:00–14:00 work shift and 13:00–15:00 club outing overlap, so the player cannot complete both.
20. The club outing is unavailable without Campus Sports Club membership and rejects atomically when the player cannot afford the proposed 8 Gold cost.
21. Saturday NPC presence matches weekday, time and Location; weekday-disjoint schedule entries may overlap in clock time without being treated as a conflict.
22. The player can ignore training, work and the club outing and still complete the day through flexible activities.
23. Sunday remains unspecified rather than inheriting an invented weekend routine.
24. The player can complete Saturday without visiting `apartment_block`, triggering Unit 213 or entering Combat.

## 11. Explicitly not part of this content slice

- Full weekly course timetables or exam/grade tracking.
- Multiple jobs, payroll, rent, loans or a city-wide economy.
- Club membership ranks, attendance streaks or club management.
- New NPCs or autonomous NPC movement.
- Generic XP, skill levels or attribute growth.
- Mandatory paranormal incidents, combat encounters or hidden penalties for missed activities.
- A global flag bag or generic `setFlags` mutation API.
- Runtime schema changes. The content model must be reviewed against the actual loaders before any JSON data is added.

The current Action Definition schema already supports the fields used by the three proposed actions, including `requiredNpcId` for Noah's training. The current NPC interaction schema does not provide the proposed invitation mutation. The shared commitment schema, `weekdays`, `startWindow`, `requires`, `costs`, `onComplete` and weekday-aware NPC schedule fields remain proposed. Do not copy the sample JSON into `Data/` until the corresponding implementation Issues land.

## 12. Implementation dependencies

After the design is accepted, implementation should be split into independently testable work:

1. Weekday derivation on the existing GameClock, with Day 1's starting weekday authored explicitly.
2. A shared scheduled-activity data contract and availability query; keep PlayerAction execution, NPC presence and domain outcomes separate.
3. Allowed-start-window handling, including a wait-until-available path when Travel arrives early due to the Agility modifier.
4. Atomic Gold spending with rejection that leaves all state unchanged.
5. Typed domain-owned state for academic completion/Knowledge acquisition, club membership, Social invitations, extracurricular activity evidence and Investigation clues. Do not begin with a global flag bag.
6. Weekday filters for selected NPC schedules while preserving daily-repeat behavior when no weekday is authored; make overlap validation compare weekday intersections as well as time intervals.
7. Monday and Saturday end-to-end playthroughs, including missed commitments, invitation/membership gates, insufficient funds, early arrival, overlapping choices and optional activities.

Do not create a generic ActivityManager, universal content executor, untyped GameState or FlagManager to implement these slices. Reuse current domain owners and add only contracts required by the demonstrated journeys.

## 12. Implementation dependencies

After the design is accepted, implementation should be split into independently testable work:
1. Weekday derivation on the existing GameClock, with Day 1's starting weekday authored explicitly.
2. A shared scheduled-activity data contract and availability query; keep PlayerAction execution, NPC presence and domain outcomes separate.
3. Atomic Gold spending with rejection that leaves all state unchanged.
4. Typed domain-owned state for academic completion/Knowledge acquisition and club membership/Skill practice; do not begin with a global flag bag.
5. Weekday filters for selected NPC schedules while preserving daily-repeat behavior when no weekday is authored.
6. Monday and Saturday end-to-end playthroughs, including missed commitments, eligibility rejection and optional activities.

Do not create a generic ActivityManager, universal content executor, untyped GameState or FlagManager to implement these slices. Reuse current domain owners and add only contracts required by the demonstrated journeys.
