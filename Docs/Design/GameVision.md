# Game Vision — Modern Student Life

**Status:** Accepted product direction  
**Last updated:** 2026-11-22

## High concept

A player-directed life-simulation RPG about an ordinary university student living in a contemporary city where unexplained phenomena quietly exist alongside everyday life.

Most days are familiar: attend classes, study, work a part-time job, meet friends, manage money, explore the city or stay home. Somewhere in the same city, strange reports circulate through neighborhood forums and private conversations. A player may investigate them, ignore them, make matters worse, or seek help from a professional response company.

The protagonist is not a chosen one and is not required to save the world. The supernatural is an optional part of a larger everyday life, not a mandatory quest chain. Phones, laptops, messaging apps and the Internet are ordinary tools; they are not magical artifacts.

Combat is a specialized interaction context for situations where confrontation makes sense. It should not define every anomaly or replace investigation, retreat and asking for help.

## Experience priorities

1. **Ordinary life first:** The player has a believable student routine with classes, friends, work, money, rest and personal plans.
2. **Player-directed curiosity:** Rumors and unusual situations create opportunities, not obligations. The player chooses whether to investigate.
3. **A world with two faces:** Everyday places remain recognizable while strange events can occur within them. An incident does not turn the whole setting into a fantasy world.
4. **Meaningful responses:** Observe, document, investigate, leave, ask for professional help or confront a threat when appropriate. Fighting is one possible response, not the default answer.
5. **Social continuity:** Classmates, coworkers, neighbors and specialists have their own schedules and reasons to be where they are.
6. **Proportionate consequences:** Ignoring or mishandling an incident may change what happens later, but not every rumor needs to become a catastrophe.
7. **Incremental simulation:** Build only the state and behavior that creates observable player decisions. Do not build a general-purpose world simulator prematurely.

## Core pillars

### A Student Life Worth Returning To

The university, student apartment, campus facilities, part-time workplace and nearby city districts form the everyday world. The player should have reasons to return to these places even when no paranormal incident is active.

### Optional Paranormal Incidents

Unusual events enter the player's life through rumors, observations, messages and encounters. Some are dangerous, some harmless, some strange or even funny. The player may ignore an incident and continue with ordinary plans.

### Modern Tools, Ordinary Uses

Phones, laptops, cameras, online forums and messaging are normal parts of contemporary life. They may help the player collect information or contact people, but they are not supernatural devices and do not imply a dedicated social-network simulation system already exists.

### People and Places

NPCs should feel like students, friends, coworkers, residents and professionals before they become sources of quests or combat statistics. Their routines and interactions should support recognizable relationships and choices.

### Character-Focused Turn-Based Combat

Combat is a discrete turn-based system centered on the active character, party condition and enemy behavior patterns. The player directly controls the four-person party. HP and Satisfaction provide two distinct dimensions of enemy state; party members also expose HP, Satisfaction and relevant resources. Action frequency is governed by a discrete action-economy rule, while skills use turn-based cooldowns. Combat has no real-time Timeline or advance Enemy Intent display.

A paranormal incident may never reach combat if the player observes, retreats or seeks help.

## Shared world-interaction model

At the design level, the experience follows this loop:

```text
Current life and world context
             |
             v
 Everyday activities and unusual opportunities
             |
             v
       Player chooses a response
             |
             v
  Owning rules validate and resolve it
             |
             v
    Character / incident outcome
             |
             v
 The next day and later situations reflect it
```

Daily-life actions may advance GameClock by minutes or hours. Combat uses its own discrete turn/round flow and pauses for player decisions. Travel changes the current Location through the supported navigation flow. Events may present a situation and choices; a choice may resolve as an ordinary action or start a specialized Combat flow.

This is a shared design model, not a requirement for one universal interaction executor in code. Keep each gameplay domain responsible for its own rules.

## World structure

**Accepted:** One contemporary student-life setting with optional paranormal content.

- The university is the main social and routine anchor, not a fantasy academy.
- The city provides housing, shops, workplaces, public spaces and unusual locations.
- Paranormal response services exist as ordinary professional organizations within the setting.
- Anomalies and rumors can be discovered or ignored; the player is not forced into a central heroic plot.
- Location-based navigation is the current presentation direction; free-roam movement is not required by this vision.

The former fantasy Academy setting is superseded. New player-facing content must use the contemporary setting.

## Content priorities

- Establish a coherent daily student routine across campus, home and the city.
- Give the player at least one optional paranormal incident with multiple understandable responses.
- Keep the ordinary-life loop worthwhile when the player ignores the incident.
- Add durable consequences only when a concrete journey needs them.
- Keep turn-based combat optional and specialized rather than mandatory for every incident.
- Prefer authored content and the smallest supported rules before introducing general systems for quests, incidents, social feeds or autonomous world simulation.

## Scope test

Evaluate a proposed feature by asking:

- Does it strengthen ordinary student life or create a meaningful optional paranormal decision?
- Does the player understand why the situation is happening and what responses are available?
- Can the player ignore, leave or seek help where that makes sense?
- Does the outcome affect the current situation or a later decision in a useful way?
- Can the value be demonstrated in a small playable journey?
- Is a new runtime system genuinely required, or can current Location, Action, NPC, Event and Combat behavior support it?

If a feature adds complexity without creating a meaningful player-facing decision, defer it.
