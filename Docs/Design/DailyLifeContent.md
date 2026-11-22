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
| Campaign day | Day 1, Monday | GameClock currently has a day index but no weekday |
| Start time | 07:30 | 07:30 |
| Starting Location | `student_apartment` | MainController currently starts at `university_campus` |
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
| `student_apartment` | Start/end point, Rest and Sleep |
| `east_district` | Connector between housing and campus/café |
| `university_campus` | Campus hub and notice board |
| `campus_library` | Study and Sera encounter |
| `lecture_hall` | Scheduled class |
| `campus_cafe` | Meal and lunchtime social opportunity |
| `corner_cafe` | Paid work shift |
| `student_union` | Club meeting and student social opportunity |

The current base Travel duration is 10 minutes per connected edge, before the existing Agility modifier. The sample route from the apartment to the Library is:

```text
Student Apartment
  -> East District             10 min
  -> University Campus         10 min
  -> Campus Library            10 min
```

Do not add direct exits merely to shorten this sample. The route should use the existing Location graph.

## 4. Monday commitments

These are proposed content records. The schedule system and weekday support do not exist yet.

| Commitment ID | Location | Weekday | Allowed start | Duration | Outcome |
|---|---|---|---|---:|---|
| `monday_research_methods_lecture` | `lecture_hall` | Monday | 09:00–09:15 | 60 min | Attend the introductory lecture; current action adds Mood +1 |
| `monday_corner_cafe_shift` | `corner_cafe` | Monday | 13:00–13:15 | 180 min | Complete the shift; current action adds 30 Gold |
| `monday_club_open_meeting` | `student_union` | Monday | 15:00–15:10 | 60 min | Meet club members and receive an optional follow-up opportunity |

The class and work shift reuse the existing `attend_lesson` and `work_shift` actions. The club meeting is new content; do not misrepresent the existing 15-minute `join_club` action (asking about clubs) as a full meeting.

The work shift and club meeting intentionally overlap. The player cannot complete both. Missing either commitment has no automatic hidden penalty in this first slice. The player simply loses that occurrence and can choose another activity.

## 5. Flexible activities and small economy

| Activity | Location | Duration | Cost | Result | Status |
|---|---|---:|---:|---|---|
| `browse_library` | `campus_library` | 30 min | Free | Study; may complete the Knowledge condition in Section 7 | Existing action; progression is proposed |
| `eat_meal` | `campus_cafe` | 20 min | 5 Gold | Stamina +3, Mood +2 | Existing effects; proposed Gold cost |
| `rest` | Any suitable safe Location; prefer apartment | 30 min | Free | Fatigue -5, Stamina +3, Mood +2 | Existing player action |
| `sleep` | `student_apartment` | 480 min | Free | Health +20, Fatigue -100, Stamina +10, Mood +10, clamped to character limits | Existing player action |

The proposed meal price is 5 Gold. With 20 starting Gold, the player can afford ordinary food before working. Completing the existing shift adds 30 Gold, so work creates a clear income opportunity without introducing debt, rent or a general shop economy.

The meal price is a proposed content value, not a current runtime cost. The current `eat_meal` action does not spend Gold.

## 6. NPC presence and opportunities

Reuse the current characters and their existing Location IDs. The following windows are the target Monday schedule; current `NpcSchedules.json` repeats these entries every day and has no weekday field.

| NPC | Time | Location | Player-facing opportunity |
|---|---|---|---|
| Mira | 07:00–09:00 | `university_campus` | Short morning conversation; she mentions seeing a neighborhood post |
| Noah | 07:00–08:30 | `sports_field` | Optional morning conversation about training |
| Sera | 07:00–09:00 | `campus_library` | Ask for a quiet study recommendation |
| Mira | 11:00–12:00 | `campus_cafe` | `join_mira_for_lunch` |
| Noah | 11:00–12:30 | `campus_cafe` | `talk` |
| Sera | 11:00–12:00 | `campus_cafe` | `talk` |
| Mira | 15:00–17:00 | `student_union` | Optional conversation during the club meeting |
| Noah | 15:00–18:00 | `student_union` | Optional conversation during the club meeting |
| Mira | 17:00–22:00 | `student_apartment` | Optional evening conversation after returning home |

These are existing NPCs and mostly existing interactions. Keep casual conversations short and contextual. They do not automatically increase an undefined Relationship score.

## 7. One concrete shared-Progression example

Use one discrete Knowledge entry rather than generic XP:

**Knowledge ID:** `research_methods_basics`

**Description:** The character understands the basics of checking where a claim came from, distinguishing a firsthand account from hearsay, and comparing two sources for contradictions.

**Acquisition condition (proposed):**
1. Complete `monday_research_methods_lecture`.
2. Complete `browse_library` at `campus_library` on the same day; the order of the two activities does not matter.

**Use in a later Loop 3 scenario (proposed):** Reveal a `compare_sources` investigation option when the player examines conflicting witness accounts or archived reports.

This is a concrete Knowledge outcome, not a universal XP reward. Attending class alone, repeating the Library action, or chatting with an NPC does not grant it. The Knowledge definition and Investigation use still require implementation and acceptance; this document does not claim they exist.

## 8. Example player journey

One valid route, not a required sequence:

| Time | Action | State / decision |
|---|---|---|
| 07:30 | Start at `student_apartment` | Day 1 Monday; 20 Gold |
| 07:30–08:00 | Travel via East District to Campus Library | 30 minutes of base travel |
| 08:00–08:30 | `browse_library` | Study before class; no Knowledge yet |
| 08:30–08:40 | Talk with Sera | Optional 10-minute conversation |
| 08:40–09:00 | Travel to Lecture Hall via Campus | Arrive for the class |
| 09:00–10:00 | Attend the lecture | Class commitment completed |
| 10:00–11:00 | Free choice / travel to Campus Café | The player may eat, meet students or use another available action |
| 11:00–12:00 | Lunch opportunity | Mira, Noah and Sera can be encountered |
| 12:00–13:00 | Free choice and travel | Prepare for work, study or another activity |
| 13:00–16:00 | Work shift **or** use the afternoon for other activities | Work grants 30 Gold; the Club Meeting begins at 15:00 and cannot also be attended |
| 15:00–16:00 | Club Meeting if not working | Social opportunity; no mandatory relationship increase |
| 16:00 onward | Return home, Rest, optional conversation with Mira | The player decides when to end the day |
| 23:00–07:00 | Sleep | 480 minutes; wake on Day 2 at 07:00 |

Travel durations in the example use the current base value and ignore the Agility reduction for readability. Actual arrival time must use the existing Travel calculation.

The morning example deliberately leaves little spare time. An extra stop can push the player past the class start window. The player should be able to skip the Library, Sera or the class rather than being forced down this route.

## 9. Saturday contrast — extracurricular and social day

Saturday is a second validation slice, not a complete weekly timetable. With Day 1 set to Monday, the first Saturday is Day 6. Its purpose is to prove that the same weekday and time-window contract supports a day with fewer fixed academic commitments and more player-directed choices.

**Starting context:** Saturday, 08:00, at `student_apartment`. There is no scheduled lecture. The player may spend the day on extracurricular activity, exercise, social time, work, study or recovery.

| Opportunity ID | Location | Weekday / allowed start | Duration | Eligibility / outcome |
|---|---|---|---:|---|
| `saturday_club_open_session` | `student_union` | Saturday, 09:00–09:30 | 60 min | Open to non-members; the player may join the proposed Campus Sports Club. Joining creates a club-owned membership record |
| `saturday_sports_practice` | `sports_field` | Saturday, 10:30–11:00 | 60 min | Requires membership in the Campus Sports Club; records a deliberate practice outcome for the proposed `athletics` Skill |
| `saturday_mira_hangout` | `student_union` | Saturday, 14:00–15:00 | 60 min | Optional social opportunity with Mira; uses Relationship/Social-owned state and does not grant automatic relationship growth |

**Saturday NPC presence target:** Mira is scheduled at `student_union` from 14:00–16:00 on Saturday for this optional hangout. This is a proposed weekday-specific presence entry; it does not change her existing weekday schedule.

These are proposed content records, not existing runtime behavior. The club membership belongs to the extracurricular domain; the `athletics` capability belongs to shared Progression. The practice activity supplies evidence that a valid practice occurred, but no numerical Skill increase, XP reward or growth formula is specified here. That rule requires a separate concrete Progression decision.

The Saturday schedule should not make all three opportunities mandatory. The player may attend only the club session, practice without taking the social opportunity if eligible, spend the day elsewhere, or do nothing beyond recovery. Existing daily NPC schedules remain unchanged unless an explicit Saturday-specific schedule is authored.

The contrast with Monday is intentional:
- Monday asks the player to balance a class window, flexible study and a work-versus-club conflict.
- Saturday offers an open club session, an optional skill-practice context and social time without a required academic commitment.
- Both days use the same GameClock, schedule primitives, Character resources and domain-owned state. No weekend-specific clock or activity executor is introduced.

## 10. Content validation cases

The Monday slice should pass these cases:

1. Starting at the apartment and following the sample route can reach the lecture within its start window.
2. An extra morning activity can make the player miss the lecture; no hidden penalty is applied.
3. The player can skip class and still use flexible activities.
4. NPC interactions appear only when the NPC is present at the current Location and time.
5. The player can afford the 5-Gold meal with the starting 20 Gold.
6. The meal spends exactly 5 Gold once; insufficient funds reject it without advancing time or applying effects.
7. Work adds 30 Gold once after the full shift resolves.
8. The player cannot complete both the 13:00 work shift and the overlapping 15:00 club meeting.
9. The Knowledge entry is granted only after both the lecture and Library study are completed on the same day.
10. Sleep from 23:00 advances 480 minutes, rolls to Day 2 at 07:00 and applies recovery once.
11. The player can complete the day without visiting `apartment_block` or triggering the Unit 213 incident.

The Saturday contrast should pass these additional cases:
12. Day 6 derives Saturday from the same GameClock when Day 1 is configured as Monday.
13. The player can attend the open club session and join the club; membership is stored by the extracurricular domain, not in a global flag bag.
14. A non-member cannot start the membership-gated sports practice; rejection changes no time, Fatigue, resources or progression state.
15. Completing sports practice records a valid practice outcome for the `athletics` Skill without inventing a numerical growth formula.
16. The player can ignore the club, practice and hangout and still complete the day through flexible activities.
17. Saturday NPC opportunities appear only when the corresponding NPC schedule matches the current weekday, time and Location; schedules without weekday restrictions remain daily.

## 11. Explicitly not part of this content slice

- Full weekly course timetables or exam/grade tracking.
- Multiple jobs, payroll, rent, loans or a city-wide economy.
- Club membership ranks, attendance streaks or club management.
- New NPCs or autonomous NPC movement.
- Generic XP, skill levels or attribute growth.
- Mandatory paranormal incidents, combat encounters or hidden penalties for missed activities.
- Runtime schema changes. The content model must be reviewed against the actual loaders before any JSON data is added.

## 12. Implementation dependencies

After the design is accepted, implementation should be split into independently testable work:
1. Weekday derivation on the existing GameClock, with Day 1's starting weekday authored explicitly.
2. A shared scheduled-activity data contract and availability query; keep PlayerAction execution, NPC presence and domain outcomes separate.
3. Atomic Gold spending with rejection that leaves all state unchanged.
4. Typed domain-owned state for academic completion/Knowledge acquisition and club membership/Skill practice; do not begin with a global flag bag.
5. Weekday filters for selected NPC schedules while preserving daily-repeat behavior when no weekday is authored.
6. Monday and Saturday end-to-end playthroughs, including missed commitments, eligibility rejection and optional activities.

Do not create a generic ActivityManager, universal content executor, untyped GameState or FlagManager to implement these slices. Reuse current domain owners and add only contracts required by the demonstrated journeys.
