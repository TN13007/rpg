# Daily Life — Loop 2 Design

**Status:** Proposed design baseline for review  
**Last updated:** 2026-11-23  
**Owner:** Loop 2 — Daily Life  
**Depends on:** [Game Vision](GameVision.md), [Progression](Progression.md), [World and Narrative](WorldAndNarrative.md)

## 1. Purpose

Loop 2 gives the player a repeatable reason to live in the world between specialized activities. It turns the shared Foundation into an ordinary student routine: choosing commitments, spending free time, earning and using money, maintaining personal condition, meeting people and deciding which opportunities deserve attention.

Loop 2 is not a calendar checklist or a complete real-world life simulator. The player chooses priorities. The game makes the time, resource and opportunity costs of those choices understandable.

**Core promise:** Every in-game day presents a limited amount of time and a changing set of opportunities. The player decides what matters today, accepts the consequences and carries the resulting character/world state into the next day and into Loop 3.

## 2. Core player loop

```text
Review current day and situation
          |
          v
Choose a commitment or free-time activity
          |
          v
Check context, time window, requirements and costs
          |
          v
Resolve the activity
          |
          v
Advance shared time and update shared character state
          |
          v
Refresh location, NPC presence and available opportunities
          |
          +------> Choose another activity
          |
          v
Return home / choose Sleep
          |
          v
Advance into the next day and repeat
```

The player may leave this loop for a Loop 3 activity at any point when its context permits. On returning, the same GameClock, Character resources, Location and progression state remain authoritative. A specialized activity owns its own resolution rules; it does not create a parallel daily-life clock or duplicate Character state.

### Player decisions that must matter

- **Time:** attending one activity means less time for another; a missed time window may not return until its next occurrence.
- **Money:** earning income creates spending options, while purchases reduce the balance available for later choices.
- **Condition:** physical activity accumulates Fatigue; rest and sleep recover it. Current rules do not turn Fatigue into an automatic action lock.
- **People:** NPC presence and their current context determine whom the player can meet. A casual interaction consumes time but is not automatically a full relationship-progression scene.
- **Opportunity:** an activity may reveal information, create access or close a time-limited opportunity. Important consequences must be authored and visible, not hidden generic penalties.

## 3. Activity types

Loop 2 uses three activity types. They share the same time and character-state foundation but do not need one universal activity executor.

| Type | Purpose | Examples | Availability |
|---|---|---|---|
| Scheduled commitment | A recurring or one-off activity tied to a time window | Class, part-time shift, appointment, club meeting | A specific weekday/day and allowed start window, usually at a Location |
| Flexible activity | An ordinary action the player can choose when its context permits | Study, meal, rest, exercise, household task, hobby | Current Location, character state and affordability |
| Optional opportunity | A contextual chance that can be accepted, ignored or revisited | Talk to a present NPC, join a meal, read a notice, respond to a local event | Location, NPC presence, current time and authored conditions |

Travel remains a Foundation action used to move between Locations. It consumes time and adds its configured Fatigue; it is not a fourth daily-life activity type.

### 3.1 Scheduled commitments

Use schedules only when timing is part of the activity's meaning. Classes and work shifts are the initial examples.

A scheduled commitment has:
- A stable activity/content ID.
- A weekday or one-off day condition.
- A Location requirement when applicable.
- An allowed start window.
- An authored activity duration and outcome.
- Optional costs or requirements.

The start window defines when the player may begin the activity. Once accepted, the activity consumes its authored duration. If the player arrives before the window opens (for example, because Agility reduces Travel time), the player must be able to wait until the start time without being rejected or forced into an unrelated action. The initial design does not simulate partial attendance or prorated wages. Content may provide a small late-start grace window; after the latest allowed start, the commitment is unavailable until its next occurrence.

Missing a commitment does not automatically apply a hidden penalty. The immediate consequence is that the opportunity is missed. Attendance, grades, job standing or other long-term consequences must be explicitly designed for the relevant activity rather than inferred from a generic schedule system.

Overlapping optional commitments may create a genuine choice. Content must not accidentally schedule two mandatory commitments for the same player at the same time.

### 3.2 Flexible activities

Flexible activities fill the time between commitments. They should be concise actions with clear duration and effects, not simulations of every real-world step.

Examples:
- Study at a suitable Location.
- Eat a meal or buy a basic necessity.
- Rest or Sleep.
- Exercise or practice a hobby.
- Complete a household task through one abstract action.
- Browse ordinary information or handle a simple administrative task.

The same activity may be available at more than one Location when that makes sense. Avoid duplicating nearly identical actions solely to change their label.

### 3.3 Optional opportunities

NPC interactions and Location Events can create opportunities without becoming mandatory daily tasks. The player may accept, ignore or leave them.

A casual conversation belongs to Loop 2 when it is a short contextual action. A consequential dialogue with relationship stages, negotiation or substantial branching belongs to the relevant Loop 3 domain.

### 3.4 Shared activity data contract, domain-owned behavior

Scheduled activities should share a small descriptive contract so classes, work shifts, club sessions and sports practice can be authored and queried consistently. This is a data contract, not a universal activity system.

A scheduled activity record may identify:
- A stable commitment ID and the existing action/content ID it invokes.
- A recurrence rule: selected weekdays, or one specific campaign day.
- An optional required Location.
- An allowed start window expressed in GameClock minutes.
- References to the activity's existing duration, costs and authored outcomes.
- Optional domain-owned eligibility conditions.

The shared schedule contract owns only when and where an opportunity is available. It must not duplicate duration/effects already owned by an ActionDefinition, nor encode academic attendance, club membership, skill practice, payroll or relationship outcomes as generic fields. The owning domain validates its own conditions and resolves its own result.

NPC presence is related but distinct: `NpcScheduleDefinition` describes where an NPC is available, not an activity the player commits to. It may reuse the same weekday and time-window primitives, while retaining its own NPC-presence semantics. Existing schedules with no weekday restriction continue to mean every day. Weekday-aware validation must compare both time intervals and weekday intersections; two entries at the same clock time on disjoint weekdays do not overlap.

Flexible actions do not need a schedule record unless a concrete player-facing case requires one. Do not migrate every `PlayerActionDefinition` into the scheduled-activity contract.

## 4. Representative weekday journey

This is a target example used to validate the design, not a mandatory route or final content schedule.

**Starting context:** Monday, 07:30, at the student apartment. The character is alive, has ordinary starting resources and has no urgent incident requiring attention.

| Period | Situation | Player choice | Immediate result / trade-off |
|---|---|---|---|
| Morning | Prepare and travel to campus | Leave early, eat first, or spend time studying at home | Different arrival time and Fatigue; a scheduled class may be missed if its start window closes |
| Morning | Class window | Attend the lecture or skip it | Attendance opportunity versus free time; no hidden penalty is applied by the calendar itself |
| Late morning | Free campus time | Study in the library, eat, or meet a present classmate | Time and possible currency cost versus knowledge/practice or social opportunity |
| Afternoon | Work shift or club activity | Work a scheduled café shift, attend a club meeting, or use the time freely | Work can earn money; club participation can create social opportunities; either consumes time |
| Evening | Return to housing | Study, relax, socialize, handle a small chore or explore the district | The player chooses which short-term outcome is worth the remaining time and condition |
| Night | End the day | Sleep now or spend more time awake | Sleep advances 480 minutes and applies authored recovery; staying awake may cause the next day's opportunities to be missed |

### Example branch: work versus study

At the same afternoon decision point:
- **Work:** travel to the Corner Café and complete the existing 180-minute shift. Current sample content rewards 30 Gold. The player gains money but gives up that block of free time and accumulates activity Fatigue.
- **Study:** spend the same period in the Library. The player forgoes the income and creates a learning/practice opportunity. Exact skill or knowledge growth remains governed by the Progression design, not by an invented generic XP reward.
- **Club / social activity:** spend time with people who are present. This can reveal a new opportunity or provide authored Mood feedback; it does not automatically increase an undefined relationship score.

These branches should remain viable for different player priorities. No single activity should be the universally optimal choice for every day.

A concrete first-day content pack reusing the current student characters, locations, actions and NPC routines is specified in [Daily Life Content — First Playable Day](DailyLifeContent.md). It separates existing data from proposed schedule, economy and Knowledge additions.

The content pack also includes a small Saturday contrast: invitation-gated training with Noah, a paid work shift versus a membership-gated club outing, and optional social time. It is deliberately smaller than a full weekly timetable and validates the same schedule and state-ownership contracts under a less structured day.

## 5. Current implementation boundary

The design target must not be confused with the current playable prototype:

| Area | Current `main` behavior | Loop 2 target |
|---|---|---|
| Starting context | `GameClock` starts at Day 1, 07:30; `MainController` starts Exploration at `university_campus` | The representative journey starts at student housing; the initial Location can be changed when the playable journey is implemented |
| Calendar | Campaign day index, weekday and minute-of-day derived from one GameClock; no Gregorian date yet | A readable Gregorian calendar; the initial campaign starts Monday, 1 September 2025 |
| NPC schedules | Authored daily time ranges repeat every day | Optional weekday restrictions for recurring NPC presence when content needs them |
| Class | `attend_lesson` takes 60 minutes and changes Mood by +1 | A scheduled commitment with a weekday and allowed start window; academic progression still needs its own rules |
| Study | `browse_library` takes 30 minutes and currently has no progression outcome | A flexible activity with a defined learning/practice outcome once Progression rules are approved |
| Work | `work_shift` takes 180 minutes and currently adds 30 Gold at the Corner Café | A scheduled commitment with a start window and explicit income outcome |
| Meal | `eat_meal` takes 20 minutes, restores 3 Stamina and adds 2 Mood; it currently has no Gold cost | A small paid/low-cost choice once spending is implemented and prices are authored |
| Currency | Character can receive Gold; no spending operation exists on the current main baseline | Signed Gold balance with explicit increases/reductions; a reduction may create debt, without a separate loan system |

The current work action is not yet schedule-gated, the current class action does not record attendance, and the current study action does not grant Knowledge or Skill growth. These are design gaps, not implemented features. The sample starting Location and weekday calendar are also target decisions, not current runtime behavior.

## 6. Activity resolution contract

An ordinary Loop 2 activity follows this conceptual flow:

1. **Discover:** the query checks current Location, GameClock, weekday/day, NPC presence, character state, requirements and affordability.
2. **Present:** the UI shows the action, expected duration, explicit costs and relevant outcome information. Estimates are informational, not authorization.
3. **Select:** the player chooses the activity.
4. **Revalidate:** the owning Application/domain flow checks the current source, time window, requirements and resource balance again.
5. **Resolve:** if valid, apply the explicit costs, advance GameClock by the actual duration, calculate activity Fatigue and apply authored effects/outcomes exactly once.
6. **Refresh:** rebuild choices and NPC presence from the new state.

A rejected action must not partially spend money, advance time, add Fatigue or apply effects. This is an execution invariant, not a promise that all activity domains must share one universal executor. Each domain retains ownership of its rules and delegates only genuinely shared calculations/state operations.

## 7. Shared-system contracts required by Loop 2

### 7.1 Time and lightweight weekly calendar

**Existing owner:** `GameClock` in the Domain layer.

The shared `GameClock` owns the one-based campaign day, minute-of-day, elapsed-minute advancement and midnight rollover. The initial campaign assigns Monday, 1 September 2025 to Day 1.

- `StartingDate` is the authored Gregorian date for campaign Day 1.
- `CurrentDate` is derived from `StartingDate + (Day - 1)`; `Weekday` is derived from that date.
- Month lengths, year boundaries and leap years follow the Gregorian calendar. The .NET `DateOnly` range (0001-01-01 through 9999-12-31) is the explicit calendar boundary; advancement beyond it is rejected before state changes.
- Keep the GameClock as the only world-time owner. Do not add a second CalendarClock, time zones, seasons or a real-time clock.
- Midnight rollover changes campaign Day, CurrentDate and Weekday together. There is no separate NextDay command or automatic daily resource reset.
- Sleep remains an authored 480-minute Player Action. It advances the date only when its elapsed time crosses midnight; the player can also cross midnight through other time-consuming actions.
- Allow selected commitments to define recurring weekdays and an allowed start window.
- Keep one-off appointments/events able to reference a specific campaign day.
- Extend selected NPC presence schedules with weekday restrictions while preserving omitted-weekday daily behavior; validate interval overlap only when entries share at least one weekday.
- Keep ordinary flexible actions independent of a weekly schedule.

The schedule data describes when an opportunity is available; GameClock remains responsible only for current time and rollover. The query that owns the relevant activity decides whether its schedule matches.

**Not included:** weather, traffic simulation, full academic timetable management, automatic NPC travel, or continuous world simulation.

### 7.2 Currency and resource transactions

**Existing owner:** `Character` owns the current Gold balance. The Action effect contract supports explicit `AddGold` and `ReduceGold` adjustments.

**Proposed rule:**
- Gold is a signed shared balance. A positive value is money available; a negative value represents debt.
- A successful work shift or explicit reward may add Gold.
- A paid activity declares an explicit Gold reduction. The reduction may cross zero and create debt; insufficient balance alone does not reject the activity.
- Debt is a balance state, not a separate loan system. Do not add automatic borrowing, interest, repayment schedules or a city-wide economy.
- The Character remains the authoritative owner of the balance. Do not add a separate ResourceManager or wallet object for the current scope.
- A cost is applied exactly once. Any action rejected for another validation reason must leave Gold, time and other resources unchanged.
- Prices and rewards are authored content values. Do not invent a global pricing formula or balance every shop in this design pass.

The first playable economy only needs a small number of meaningful examples, such as earning money from a work shift and choosing whether to pay for a meal or necessity. Shopping/inventory ownership remains a separate design dependency; spending Gold alone does not imply that a full inventory system exists.

### 7.3 Character state

| State | Loop 2 role | Current rule / boundary |
|---|---|---|
| Health | Survival and recovery after dangerous activities | Not a daily energy meter; zero Health blocks ordinary actions under current rules |
| Fatigue | Accumulated exertion and a planning signal for rest | Calculated from activity intensity and actual duration; no automatic action lock or threshold penalty is currently approved |
| Stamina | Primarily a Combat resource | Do not turn it into a second daily work-energy bar |
| Mood | Readable emotional condition affected by authored activities | Current value/bands may be displayed; no hidden success modifier is implied |
| Gold | Signed balance for authored income and expenses; negative values represent debt | Action effects apply explicit increases/reductions; affordability and purchase rules remain owned by the relevant activity domain |
| Attributes | Broad character characteristics | Preserve authored base values; do not invent new formulas or universal checks |

The existing Agility-based Travel duration modifier remains unchanged. Other attribute uses should be designed through concrete scenarios and coordinated with [Issue #114](https://github.com/TN13007/rpg/issues/114), not added as arbitrary bonuses to every activity.

### 7.4 Progression as a shared cross-layer system

Progression is not owned by Loop 2 or Loop 3. Both may create opportunities to develop the same character capabilities.

The current design direction distinguishes:
- **Attributes:** broad character characteristics; not automatically increased by repeating an ordinary action.
- **Skills:** practical capabilities that can unlock actions, change how an action resolves or create new combinations.
- **Knowledge:** discrete information or understanding that can reveal options, improve interpretation or support later decisions.
- **Relationships:** social state and reciprocal opportunities; casual conversation alone does not imply an automatic relationship increase.

Loop 2 can provide instruction, study and deliberate practice. Loop 3 can provide specialized practice, research and consequential use. The activity domain decides whether its context qualifies; the shared Progression rules own the resulting capability state.

No universal XP pool, level formula, skill-point award, anti-farming rule or attribute-growth formula is approved by this document. Those rules remain open in [Progression Design](Progression.md). Do not implement a generic progression event/API until at least one complete activity demonstrates the required data and behavior.

### 7.5 Persistent state and ownership

Loop 2 and Loop 3 need durable facts, but that does not justify one untyped global flag bag. A shared persistence mechanism may eventually serialize domain records, while each domain remains the authority for the meaning, validation, lifecycle and mutation of its own state.

| State / fact | Owning domain | Required shape for the current examples |
|---|---|---|
| Class or study completion | Academic / learning activity domain | Typed completion record with activity ID and campaign-day identity; enough to evaluate same-day acquisition conditions |
| Learned Knowledge | Shared Progression — Knowledge | Character-owned Knowledge entry; acquisition is granted only when the relevant domain confirms its requirements |
| Club membership | Club / extracurricular domain | Campaign-persistent membership record keyed by character and club; joining/leaving rules belong to that domain |
| Weekend invitation from Noah | Social / Relationship domain | Campaign-persistent typed invitation/opportunity record; an interaction may grant it, and the training activity queries it |
| Training with Noah completed | Extracurricular / activity domain | Campaign-persistent typed activity-completion evidence; it is not itself a Skill increase |
| Unit 213 rumor heard | Investigation domain | Optional campaign-persistent clue/information record; it does not gate the incident |
| Skill capability | Shared Progression — Skills | Character-owned Skill state; the activity domain supplies a valid practice/learning context |
| First meeting and relationship state | Relationship / Social domain | NPC-specific relationship/history state; a casual interaction does not automatically imply relationship growth |
| Incident outcome | Incident domain | Typed outcome for that incident; only expose facts to other domains through an explicit query/contract |

Important boundaries:
- A common storage format does not make these records interchangeable. Avoid arbitrary string keys, untyped values or generic counters without a demonstrated consumer.
- The domain that owns a record is the only domain that may authoritatively change it. Other domains may query it through an explicit read contract.
- Distinguish day-scoped evidence from campaign-persistent state. For example, lecture and library completion must be associated with the same campaign day; the resulting Knowledge, once granted, is persistent until a domain rule changes it.
- Do not clear or overwrite durable state at midnight merely because a daily activity record is day-scoped. Define retention per record type.
- The example identifiers `student_union_club_member`, `noah_weekend_invite`, `trained_with_noah` and `heard_unit_213_rumor` name typed domain facts, not entries in a global string-keyed flag bag. A draft `setFlags` field is not an approved API.
- Do not create a generic `GameState`, `FlagManager`, Quest State or Incident State framework in this design pass. First prove the concrete records and queries through the Monday/Saturday journeys and one later Loop 3 use.

## 8. Loop 2 activity scope

| Activity group | Initial treatment | Reason |
|---|---|---|
| Attend class / lecture | Core scheduled activity | Creates a recurring time decision and academic context |
| Independent study | Core flexible activity | Lets the player use free time deliberately |
| Part-time work shift | Core scheduled activity | Connects time commitment with income and Fatigue |
| Meals | Core flexible activity | Supports daily routine and a small currency decision |
| Rest / Sleep | Core shared actions | Connects daily activity costs to recovery and day rollover |
| Travel | Reuse Foundation | Connects the campus, housing and city; no duplicate travel system |
| Casual NPC interaction | Core contextual opportunity | Uses existing NPC presence and interaction query |
| Club meeting / campus event | Abstract scheduled activity | Provides recurring social/extracurricular context without club management |
| Exercise / hobby | Abstract flexible activity | One authored action can represent a session |
| Household chores / hygiene | Abstract flexible activity | Resolve as a short action only when it creates a meaningful choice |
| Shopping / services | Small authored transactions | Add only when a concrete purchase/use outcome exists |
| Course registration / grades / exams | Defer specialized detail | Requires academic records and evaluation rules not yet justified by the daily slice |
| Job search / career management | Defer specialized detail | A full employment-management system is not required to prove one part-time shift |
| Full social network / autonomous NPC life | Defer | Not needed for a player-directed daily routine |
| Detailed hobby, sport or volunteer simulation | Defer | Use Loop 3 only when a specific activity earns dedicated rules |

## 9. Loop 2 and Loop 3 interaction

Loop 2 supplies the shared context in which specialized activities happen:
- The current GameClock time and weekday.
- The current Location and available NPC presence.
- Character Health, Fatigue, Stamina, Mood and Gold.
- Shared progression capabilities and discovered knowledge.
- Relevant authored commitments or opportunities.

Loop 3 may consume time or resources, change Character state, create knowledge or affect later opportunities. It must return to the same shared state when its own resolution finishes. Combat retains its own turn/AP rules; Investigation retains its own evidence and decision rules; Social Interaction retains its own dialogue/relationship rules.

A specialized activity does not automatically consume a fixed daily-life duration while it is running. Its world-time cost must be authored and applied at the appropriate domain boundary. Do not advance GameClock once per turn or once per frame.

## 10. Player-facing acceptance journey

Loop 2's first complete playable slice should demonstrate:

- A player can inspect the current day/time, Location, condition and available opportunities.
- At least one recurring class or equivalent commitment is available only within an authored weekly start window.
- At least one flexible activity remains available before and after that commitment.
- The player can deliberately miss a commitment without the game applying an undocumented penalty.
- The player can choose between a paid work shift and at least one meaningful alternative.
- Gold can be earned and reduced by a small, understandable paid activity; the balance may become negative and visibly represent debt.
- Travel and activities advance the same GameClock and calculate Fatigue once from actual duration.
- NPC presence and interactions refresh as time advances.
- Sleep advances exactly 480 minutes, handles midnight rollover and applies its authored recovery once.
- The player can complete the day without entering a paranormal event or Combat.
- A Loop 3 activity can be entered and exited without duplicating or resetting shared Character/time state.

## 11. Explicit non-goals

- A mandatory morning-to-night quest checklist.
- A full real-world calendar with months, years, holidays or time zones.
- A comprehensive timetable/course/grade/tuition simulator.
- A full job-search, promotion, payroll or career-management simulator.
- A city-wide market, loan products, automatic interest/repayment, investments, rent simulation or dynamic prices.
- A universal activity dispatcher or universal interaction executor.
- Continuous autonomous NPC needs, movement or decision-making.
- A generic Quest, Event, Schedule, Resource or World Simulation framework.
- New XP, skill, attribute, relationship or fatigue formulas without a separately validated design decision.

## 12. Open decisions

The following require a concrete playable scenario or balance test before implementation:
- Which weekdays and time windows the first class and work shift use.
- Whether a late-start grace period is needed for each commitment type.
- The first few meal/necessity prices and whether any recurring expense is necessary.
- Which specific study/practice outcomes should create Knowledge or Skill progression.
- Whether Fatigue ever needs a consequence beyond being a visible planning signal.
- Which social interactions should remain casual Loop 2 actions and which deserve the Loop 3 Social Interaction domain.
