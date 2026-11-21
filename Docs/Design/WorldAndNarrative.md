# World and Narrative Design

**Status:** Current direction; world details remain open  
**Last updated:** 2026-11-22

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

## Discovering opportunities and quests

The world should offer several ways for the player to discover an activity or a longer story. A quest is not synonymous with a mission marker or a dialogue option; it is an authored progression that may span multiple interactions, events and Locations. Not every conversation or story scene needs to become a quest.

Use these authored discovery channels where they suit the content:

- **Known contact:** The player deliberately visits or contacts an NPC they already know. The NPC may offer a task, request help or reveal a new opportunity when its conditions are met.
- **Discovery encounter:** The player notices an unfamiliar person, unusual situation, clue or activity while visiting a Location. Investigating or responding may reveal a quest; the player should be able to ignore it when appropriate.
- **Communication:** A letter, message, notice or other in-world communication informs the player about an opportunity or advances an existing thread. It should not become a substitute for every in-person interaction.
- **World event:** A situation becomes available because of the current time, Location or world state. It may be temporary, optional and independent of the player's quest log.

These are presentation and content patterns, not four separate quest systems. Each can lead to an existing Action or Location Event, or to a quest progression when the content genuinely needs multiple stages. Reuse the current Location, Action, Event and Effect model wherever it expresses the experience clearly.

### Quest progression

A quest tracks a meaningful chain of player-facing stages across contexts. A simple authored quest may follow this conceptual lifecycle:

```text
Undiscovered opportunity
        |
        v
Discovered / offered
        |
        v
Accepted or started
        |
        v
One or more stages / changing circumstances
        |
        v
Resolved -> completed, failed, or abandoned
```

This is a design vocabulary, not a requirement that every quest use every state. Some content starts when the player agrees to help; other content begins when the player investigates or acts without knowing it belongs to a larger thread. The player may decline, ignore or leave optional content unresolved.

Keep **quest progress** distinct from **player awareness**. The world may already satisfy a condition before the player learns about it. When the player later meets the relevant NPC or discovers the clue, the content should acknowledge what has already happened instead of forcing an unnecessary repeat. Only track awareness separately when it changes what the player can understand or choose.

A stage should define, in authored content:
- What makes it available and what starts it.
- The relevant interaction, event or objective.
- The choices or supported ways to resolve it.
- The state changes and narrative feedback caused by each outcome.
- What becomes available next, including whether the player can stop or leave the thread.

Quest outcomes may change NPC circumstances, Location access, available activities, relationships, information or later opportunities. Consequences should be visible when the player has a reasonable way to notice them. Do not require every quest to branch extensively or produce world-scale consequences.

### Quest and event boundaries

A **Location Event** presents a particular situation, scene and its choices. A **Quest** tracks progression when a story or activity spans multiple situations or requires durable stage state. An **Effect** applies a supported state change. These concepts may connect, but they are not interchangeable.

A single event can start or advance a quest. A quest can refer to several events at different Locations. An event can also be a self-contained moment with no quest at all.

The quest log, map markers, notification feed and automatic mission tracking are presentation choices, not prerequisites for the underlying experience. The player should be able to discover content through the world itself; use explicit reminders or tracking only when they improve clarity without turning optional life activities into a checklist.

### Scope and implementation guardrails

- Start with a small authored example: one Location, a few NPCs or points of interest, one discoverable multi-stage thread and at least one observable consequence on return.
- Prefer explicit content conditions and state transitions over a general-purpose quest scripting language.
- Evaluate availability at meaningful boundaries such as entering a Location, viewing its contents, interacting with an NPC or applying a relevant state change. Do not poll every possible trigger continuously without a player-facing need.
- Do not introduce a universal Quest Manager, generic dialogue framework, autonomous mission director or large dependency graph solely to support the patterns above.
- Do not assume every NPC is a quest giver, every interaction creates a relationship score, or every world event must be recorded as a quest.
- Add persistence and recovery rules for a quest only when the actual player journey needs progress to survive leaving a Location, sleeping, reloading or changing context.

These are design intentions. They do not claim that the current runtime already supports generic quests, arbitrary triggers, communication channels or all listed state transitions.

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
