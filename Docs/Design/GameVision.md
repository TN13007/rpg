# Game Vision — Academy Life Simulation RPG

**Status:** Current product direction  
**Last updated:** 2026-11-22

## High concept

A Life Simulation RPG set in a Science-Fantasy world with Renaissance-era civilization and common magic. The player inhabits an academy and its nearby town, meets people, chooses activities and discovers content at their own pace.

The game does not require a single central quest, chosen-one identity or predetermined end goal. The player decides what matters to them: friendships, study, work, exploration, hobbies, combat, romance or other supported activities.

## Experience priorities

1. **Player-directed life:** The player chooses what to do and what goals to pursue. The game offers opportunities rather than prescribing one correct route.
2. **A world worth inhabiting:** Locations, services, schedules and ordinary activities make the setting usable beyond quest progression.
3. **People to meet:** NPCs have recognizable identities and can be encountered through locations, events and activities.
4. **Discovery through play:** The player learns about places, people and opportunities by exploring and interacting.
5. **Meaningful development:** Skills, knowledge, resources and relationships expand the player's available choices.
6. **Optional challenge:** Combat and dangerous activities can exist as choices, not mandatory proof of progress.

## Core pillars

### Live at Your Own Pace

Time advances through activities. Daily routines provide structure without turning every hour into a compulsory appointment.

### Explore and Discover

The Academy and nearby town are the initial playable space. New locations and activities are discovered through travel, observation, conversation and experimentation.

### Meet People

NPCs are introduced through authored encounters and interactions first. More simulation depth is added only when it creates visible player value.

### Choose Your Activities

Study, practice, socialize, shop, work, rest, explore and fight where those activities are supported. No single activity is required to define the player's life.

### A World That Continues

World activities should continue according to authored rules when the player chooses something else. Simulation depth is added only when it creates visible player value.

## Core gameplay loop

```text
Choose a location
    |
    v
Read its description and available choices
    |
    v
Travel, perform an action or enter an event
    |
    v
Time advances and effects update state
    |
    v
Discover the result and choose what to do next
```

This is an open loop, not a fixed sequence. The player may repeat, interrupt or ignore any optional activity.

## Design principles

These principles describe the intended player experience. They do not imply that every supporting system already exists in the runtime.

### 1. Offer activities, not only quests

A Location should give the player understandable reasons to visit and choices about what to do there. Ordinary activities can be valuable without belonging to a quest. Use the current Location, Action, Event and Effect model before introducing a new content system.

### 2. Let world elements connect

Prefer interactions where an action, item, piece of knowledge, NPC or Location can affect more than one situation. Start with explicit authored conditions and outcomes. Add more flexible combinations only when repeated content exposes a real limitation.

### 3. Make time create choices

Activities may consume different amounts of GameClock time. Time should make some opportunities available, unavailable or more costly, but should not turn every hour into a mandatory appointment. Start with action durations and a few meaningful time conditions; add broader scheduling only when the player can observe its value.

### 4. Make NPCs recognizable before simulating them

Start important NPCs with authored interactions and clear places to encounter them. Add simple schedules only for characters whose presence matters to a playable situation. A schedule is a default, not an unbreakable script: authored events or player actions may justify exceptions. Do not simulate needs, autonomous planning or memory for every NPC without a demonstrated need.

### 5. Make relationships two-way and consequential

Relationships should eventually support more than a numerical score. They may affect available interactions, invitations, assistance, information and later events. NPCs may initiate contact, agree, refuse or suggest another time according to their authored circumstances. A high relationship value does not override an NPC's boundaries or consent.

Start with explicit relationship state and a small number of visible consequences. Add messaging, invitations, complex relationship progression or combat synergy only when a concrete player journey needs them.

### 6. Make progression expand choices

Stats, skills and knowledge should do more than act as pass/fail gates. Where appropriate, they can reveal information, offer another approach, improve an activity or change a combat option. A stat does not need to affect every domain; each use should make sense in its context.

Start with a small set of attributes and a few authored checks or alternative actions. Expand their uses when playtests show that growth is not noticeable or meaningful.

### 7. Give equipment and items a reason to exist

Equipment should create understandable differences or trade-offs, not just cosmetic variety or tiny numerical increases. Items should solve a recognizable problem, enable an activity or support a useful choice. Some equipment or items may connect multiple contexts, such as combat, exploration and social interaction, when that connection is natural.

Start with a few items and equipment pieces that have clear uses. Add durability, set bonuses, complex tags or broad cross-system effects only when the simple inventory fails to support meaningful decisions.

### 8. Keep specialized activities distinct but connected

Combat, daily actions, travel, social interactions and exploration are different interaction contexts in the same continuing world. Each context may have its own rules, costs, timing and feedback. Their outcomes should update the relevant continuing character or world state.

Do not force every activity through one universal executor. In particular, keep Combat Timeline time distinct from GameClock time and preserve specialized combat execution. Improve combat speed and presentation when actual encounters show unnecessary waiting; do not add combo, overkill or auto-resolve systems before their value is demonstrated.

## Shared interaction model

Use this conceptual flow without assuming a universal runtime framework:

```text
Current context presents available interactions
                  |
                  v
Player chooses an interaction
                  |
                  v
The responsible system validates and resolves it
                  |
                  v
Relevant character/world state is updated
                  |
                  v
The player sees the result and new opportunities
```

An ordinary Action, a Location Event and a Combat encounter may follow different execution rules. They are connected by the continuing world state and the consequences the player can observe, not by requiring identical implementation.

## Incremental complexity rule

Always begin with the smallest implementation that can demonstrate the intended player experience. Add complexity only after a specific limitation is observed in a playable journey, test or content-authoring workflow.

| Observed problem | Possible next step — only when needed |
|---|---|
| A Location feels empty | Add a small number of authored activities or points of interest |
| Days feel repetitive | Add time-based opportunities or a few scheduled events |
| An important NPC is hard to encounter consistently | Give that NPC a simple, testable schedule |
| NPC behavior feels implausibly rigid | Add explicit schedule exceptions for relevant events or player actions |
| Relationships feel like a score with no impact | Add one visible interaction or event consequence |
| Stats do not make character growth noticeable | Add a new approach or useful information check |
| Items are ignored | Give selected items a clear use in an actual situation |
| Equipment choices feel interchangeable | Add a meaningful trade-off to a small number of pieces |
| Combat encounters contain unnecessary waiting | Improve action pacing, animation speed or encounter resolution based on playtest evidence |

Do not build a general-purpose manager, simulator, scheduler, scripting language or content framework to anticipate hypothetical future needs. Prefer explicit rules and authored content until repetition, inconsistency or maintenance cost demonstrates that a reusable abstraction is necessary.

A proposed feature should answer four questions:

1. What new or improved player experience does it deliver?
2. What meaningful choice or consequence does it create?
3. Which existing context or system does it connect to?
4. What observed problem makes its implementation necessary now?

If these questions cannot be answered, defer the feature.

## World identity

| Element | Decision |
|---|---|
| Genre identity | Science-Fantasy |
| Civilization aesthetic | Renaissance |
| Magic | Common |
| World scale | Unknown / deliberately undecided |

The world combines magic, research and practical technology without assuming modern industrial or electronic technology. The source and full metaphysics of magic remain open until a playable need requires them.

## Opening experience

The first vertical slice is **Opening Day**. It uses the existing content model wherever possible:

- Start around 07:00–08:00 at a horse-carriage location.
- Choose the Academy as the destination; the existing travel flow changes the current Location.
- Arrive among many students, represented through the location description and authored content rather than a simulated crowd.
- Reach the opening ceremony either by following students or by meeting Mira and going with her. Both routes converge on the ceremony.
- An authored event introduces the hall, the headmaster and the opening address through readable story passages and choices.
- Go to the canteen, eat to restore Stamina and interact with Mira or other students through authored actions/events.
- Explore a small set of connected Academy and town locations.
- Return to the Dormitory and use the existing eight-hour Sleep action.

The prototype uses the existing protagonist data. Character creation is not required for this slice. The opening introduces the world through ordinary experiences and does not assign a mandatory long-term goal or force a crisis.

## Content model and scope

- Use **Location** for places and their connections.
- Use the existing **Travel** flow for movement between connected Locations.
- Use **Action** for an activity with duration, feedback and effects.
- Use **Effect** for supported state changes, such as restoring Stamina.
- Use **Location Event** for authored scene text and choices.
- Add a small sequential-passage capability only if one event description cannot present the ceremony clearly.
- Prefer authored JSON when the runtime already supports the behavior. JSON configures content; it does not create new runtime behavior by itself.
- Do not build a full world encyclopedia, universal NPC simulator, generic quest framework or broad content tools without a demonstrated need.
- A playable day is stronger evidence than a large collection of disconnected systems or JSON files.
