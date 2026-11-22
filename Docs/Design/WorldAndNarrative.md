# World and Narrative Design

**Status:** Accepted setting direction; incident details remain in development  
**Last updated:** 2026-11-23

## World structure

The game takes place in a contemporary city centered around a university and its surrounding districts.

- The university campus is the player's main routine and social anchor.
- Student housing provides a place to rest, study and manage personal life.
- Cafés, shops and part-time workplaces connect daily routines to money and local relationships.
- Nearby streets, parks, apartment blocks and public services create a wider city to explore.
- Paranormal response companies operate as ordinary professional services for reports that appear unusual or dangerous.

This is a modern setting. The world does not use a fantasy academy, guild, magical curriculum or default adventurer premise.

## Ordinary life

The protagonist is an ordinary student. Their immediate concerns can include classes, assignments, attendance, money, work shifts, friends, rest and personal plans. These routines should remain meaningful even when no paranormal event is active.

The player is not a chosen one, and the story does not require them to investigate every rumor. A player may spend several in-game days studying, working or socializing without following an incident.

## Paranormal layer

Unexplained phenomena exist within the same everyday world. People encounter them through rumors, neighborhood forums, inconsistent witness accounts, unusual recordings and personal experiences.

Incidents should vary in scale and tone:
- Harmless or inconvenient oddities.
- Local urban legends and unsettling locations.
- Dangerous anomalies that require caution or professional assistance.
- Rare large-scale events, used only when the story earns that escalation.

Ordinary technology remains ordinary. A phone can record audio, display a forum post or call a service; it does not become a supernatural artifact. A rumor appearing online does not mean the game needs a fully simulated social network.

## Player agency and incident responses

An incident is an opportunity, not a mandatory quest. Depending on the situation, the player may:

- Ignore the rumor and continue daily life.
- Observe from a safe distance.
- Gather information or compare witness accounts.
- Visit the location and investigate.
- Leave when the situation becomes unsafe.
- Contact a professional response service.
- Confront the anomaly only when the situation and implemented combat rules support it.

The game should communicate risk without requiring the player to solve every mystery. Refusing to enter a dangerous place can be a valid and sensible outcome.

## Narrative direction

Stories should emerge from the contrast between recognizable daily routines and unexplained events. A strange incident can interrupt a lecture, become a topic at lunch, appear on a neighborhood forum or affect a part-time shift.

The protagonist's personal relationships and ordinary responsibilities remain central. Paranormal incidents should deepen the world and its characters rather than replace student life with a constant emergency.

Avoid assuming a single world-ending conspiracy, a chosen-one prophecy or a mandatory sequence of escalating bosses. A small event can be complete and satisfying without revealing a grand secret.

## Consequences and continuity

Important incidents may evolve while the player is busy elsewhere. Consequences should be proportionate and understandable:
- A rumor may gain or lose credibility.
- Residents may stop visiting a location.
- A professional service may secure a dangerous site.
- A careless intervention may make the situation harder to resolve.
- A person who asked for help may remember whether the player responded.

These are design possibilities, not claims that persistent incident progression is implemented. The current runtime supports authored Location Events and daily NPC schedules, but does not yet provide durable incident state, delayed progression, a social feed or professional dispatch/payment behavior. Add the smallest state mechanism only when a specific journey requires it.

## NPCs and relationships

NPCs include classmates, friends, coworkers, neighbors and specialists. Their identities should be grounded in ordinary roles and motivations before any supernatural involvement.

The current schedule model is a recurring daily timetable authored as time ranges, Locations and visible activities. It resolves where a scheduled NPC is at the current GameClock time; it does not imply pathfinding, needs, autonomous decision-making or continuous simulation. The player discovers present NPCs through `See around`; character interactions are authored separately from Location actions.

Relationships should be represented through the smallest set of dimensions or flags that produce observable differences. Avoid a large social simulation or relationship framework before a playable interaction needs it.

## Time and simulation boundaries

Daily activities may advance GameClock in minutes or hours. Combat uses discrete Player and Enemy Turns and pauses while the player chooses actions. The rule for translating a completed encounter back into world time must be defined by the relevant scenario; do not assume the two time models are interchangeable.

The game does not need to simulate every NPC or Location continuously. Use authored presence, conditional events or coarse updates where they are sufficient. Add autonomous simulation only when the player can notice it and make a meaningful decision because of it.

## Open questions

- How widely are paranormal incidents known: isolated witnesses, online communities or public institutions?
- Which incident outcomes must persist across days and game sessions?
- Should an ignored incident advance on fixed world time, player visits or authored checkpoints?
- What does contacting a professional response service actually cost and how long does dispatch take?
- Which incidents can be resolved without combat, and which genuinely require confrontation?
- How should the ordinary student schedule remain engaging when the player chooses not to investigate?
