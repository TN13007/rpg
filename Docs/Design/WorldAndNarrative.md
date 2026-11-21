# World and Narrative Design

**Status:** Current direction; world details remain open  
**Last updated:** 2026-11-21

## World foundation

| Element | Decision |
|---|---|
| Genre identity | Science-Fantasy |
| Civilization aesthetic | Renaissance |
| Magic | Common |
| World scale | Unknown |

The setting combines everyday magic, research and practical technology with Renaissance-era social and material culture. The exact source of magic, geography, political structure, history and total world scale remain open. Decide them only when needed to create a coherent playable location or activity.

## Player and world relationship

The player is a person living in the world, not a chosen one required to resolve a central plot. The world offers places to visit, people to meet and activities to discover. The player chooses which opportunities to pursue and may ignore others.

The world does not need a mandatory main storyline to give the player direction. Personal goals can emerge from interests, relationships, discoveries and consequences.

## Narrative approach

Narrative is discovered through location descriptions, authored events, choices, conversations and repeated encounters. A story scene does not need to become a quest. Small everyday moments can be meaningful without escalating into a crisis.

For the current prototype, authored narrative content should fit the existing interaction model:
- Locations describe where the player is and connect to other places.
- Actions represent activities and advance time.
- Effects change supported player state.
- Location Events present a scene and its choices.
- A sequential passage feature may be added narrowly if required for a multi-part scene such as the opening ceremony.

## NPC principles

NPCs should be understandable as people before they become simulation systems.

For the first playable day, named NPCs such as Mira can be represented by authored location interactions and events. This is enough to test first meetings and short conversations.

Add presence, schedules, memory and relationship rules later, only when the intended player experience requires the player to observe NPCs moving or reacting outside a scripted interaction. Do not implement universal needs, planning or autonomous decision frameworks before a concrete scenario requires them.

## Time and events

Time provides structure for daily activities and NPC availability. Actions advance the GameClock according to their authored duration. Events may be triggered by entering a Location or choosing an interaction.

Events should:
- Have a clear trigger and state.
- Resolve consistently when the player participates or ignores them.
- Produce proportionate and understandable outcomes.
- Avoid blocking unrelated player activities without a clear reason.

## Opening Day

The first authored day introduces the Academy as the player's new environment. No character creator is required for this prototype; use the existing protagonist data.

- Morning: begin at a horse-carriage location around 07:00–08:00 and travel to the Academy.
- Arrival: the crowd is established through description and content, not hundreds of simulated NPCs.
- Route choice: follow the students or encounter Mira and walk with her. Both paths reach the opening ceremony.
- Ceremony: an authored event describes the hall, the headmaster and the address, with readable passages and simple choices.
- Canteen: eat to restore Stamina and interact with Mira or a few students through authored actions/events.
- Afternoon: visit a small set of connected Academy and town locations, with carriage/travel connections where appropriate.
- Evening: return to the Dormitory and use the existing eight-hour Sleep action.

This is a welcoming introduction, not a mandatory quest chain. Mira is an optional early connection, not a companion who must follow the player all day.

## Content boundaries

- Do not assume a fixed central conflict, final villain or required endgame.
- Do not predefine the complete geography or history while world scale remains unknown.
- Do not equate common magic with unlimited capability.
- Keep combat, romance, work, study and other activities as optional paths unless a later design decision explicitly changes this.
- Keep the world internally consistent, but leave unused details open rather than inventing lore without a gameplay purpose.
