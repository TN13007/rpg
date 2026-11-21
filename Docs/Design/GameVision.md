# Game Vision — Academy Life Simulation RPG

**Status:** Current product direction  
**Last updated:** 2026-11-21

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
