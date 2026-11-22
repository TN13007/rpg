# Character Progression Design

**Status:** Directional; Action Capacity growth direction accepted, detailed progression rules remain open  
**Last updated:** 2026-11-22

## Purpose

Progression should expand what the student and their companions can do across ordinary life, investigation and dangerous encounters. A stronger character should not merely deal a larger number; they should gain new options, knowledge, combinations or resilience.

## Candidate dimensions

| Dimension | Design responsibility | Desired player impact |
|---|---|---|
| Attributes | Broad character capabilities | Change underlying strengths and weaknesses |
| Action Capacity | Party-wide number of skill uses available in a Player Turn | Unlock longer Party combos and more flexible action sequences |
| Equipment | Practical tools and build specialization | Change action properties, defense or tactical trade-offs |
| Skills | Practical and tactical options | Add or modify actions, effects and combinations |
| Mastery / proficiency | Focused expertise | Reward deliberate use of a capability where appropriate |
| Knowledge | Understanding of people, places and anomalies | Help the player make better-informed decisions |
| Relationships / support | Social and character development | Create meaningful assistance, conflicts or opportunities |

These are candidate dimensions, not a commitment to implement seven separate systems. Avoid overlap: each system must own a distinct player-facing outcome.

## Principles

1. Prefer new capabilities over raw stat inflation.
2. Make equipment and builds involve meaningful trade-offs.
3. Preserve distinct character identity.
4. Support multiple viable approaches to an incident.
5. Make growth readable: the player should understand what changed and how to use it.
6. Introduce only the progression dimensions required by the current playable slice.

## Action Capacity and team training

Combat uses one shared Action Point (AP) pool for the entire Party. **Action Capacity** is the Party-level progression value that sets the AP available at the start of each Player Turn. It is not a separate AP bar for each character, is not the sum of individual Agility values and is not a personal action quota for each Party member.

In the first playable slice, Action Capacity starts at 2 AP and is capped at 4 AP. Two fixed training milestones raise it from 2 to 3 AP, then from 3 to 4 AP. Each successful skill use costs 1 AP. The player may spend the shared pool on any eligible Party member, but each character may perform at most one AP-funded action per Round because of a one-Round actor cooldown. This makes the shared pool support combinations across different Party members rather than repeated actions from one character.

Training should provide a clear in-world way to improve Action Capacity. Suitable activities include team drills, sparring and tactical coordination practice. They should use the existing daily-life activity model where possible, with visible costs such as GameClock time and exertion. Avoid creating a separate training minigame or universal training framework before a playable scenario needs one.

Training must not become an unlimited, consequence-free grind. The two milestone requirements and their activity costs should make practice a meaningful choice against study, work, rest and other activities. The starting value and number of milestones are fixed for this slice; exact training activities, milestone requirements, exertion and GameClock costs remain open until tested.

Action Capacity is intentionally a Party-level value. Its eventual storage/ownership in runtime data is an implementation decision; do not duplicate the same capacity across all four character records or derive it from a sum of their attributes without a demonstrated design need.

## Student life and personal development

University activities may provide learning, practice, research, recovery and access to classmates or mentors. Part-time work may provide money and contacts. Personal routines can create opportunities to develop capabilities, but they should not become a separate management game.

## Paranormal knowledge

Knowledge may help the player recognize patterns, assess risk or choose a safer response. Do not turn every anomaly into a combat stat check. Information, preparation and professional assistance should remain viable alternatives where appropriate.

## Open decisions

Which individual attributes are active and what each changes; the exact activity requirements and costs for the Action Capacity milestones; whether later skills can spend more than 1 AP; whether levels, activity-linked growth or milestones drive other progression; how practical skills are learned and equipped; whether Mastery is necessary; how equipment changes action options; and how relationships mechanically affect investigation or combat.
