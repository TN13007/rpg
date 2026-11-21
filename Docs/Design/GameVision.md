# Game Vision — Academy Life Simulation RPG

**Status:** Current product direction  
**Last updated:** 2026-11-21

## High concept

A Life Simulation RPG set in a Science-Fantasy world with Renaissance-era civilization and common magic. The player creates a character, joins an academy, explores its campus and nearby town, meets people, chooses activities and gradually shapes their own life.

The game does not require a single central quest, chosen-one identity or predetermined end goal. Its content should invite discovery. The player decides what matters to them: friendships, study, work, exploration, hobbies, combat, romance or other supported activities.

## Experience priorities

1. **Player-directed life:** The player chooses what to do and what goals to pursue. The game offers opportunities rather than prescribing one correct route.
2. **A world worth inhabiting:** Locations, services, schedules and ordinary activities make the setting feel usable beyond quest progression.
3. **People with their own lives:** NPCs have recognizable personalities, routines and relationships. Their simulation should create observable interactions, not complexity for its own sake.
4. **Discovery through play:** The player learns about places, people and opportunities by exploring and interacting with them.
5. **Meaningful development:** Skills, knowledge, resources and relationships expand the player's available choices.
6. **Optional challenge:** Combat, dangerous exploration and other high-intensity activities can exist as choices, not mandatory proof of progress.

## Core pillars

### Live at Your Own Pace

Time advances through activities and the player chooses how to spend it. Daily routines provide structure without turning every hour into a compulsory appointment.

### Explore and Discover

The Academy and nearby town are the initial playable space. New locations and activities are discovered through observation, conversation, access and experimentation.

### Meet People

NPCs have identities, interests and commitments. Relationships grow from shared experiences and repeated interactions rather than a single abstract affection score.

### Choose Your Activities

Study, practice, socialize, shop, work, rest, explore and fight where those activities are supported. No single activity is required to define the player's life.

### A World That Continues

NPCs and world activities should continue according to authored rules when the player chooses to do something else. Simulation depth is added only when it creates visible player value.

## Core gameplay loop

```text
Choose where to go and what to do
        |
        v
Interact with people, places and activities
        |
        v
Time and relevant world state advance
        |
        v
Discover outcomes, opportunities and changes
        |
        v
Choose the next activity — or rest
```

This is an open loop, not a fixed sequence. The player may repeat, interrupt or ignore any optional activity.

## World identity

| Element | Decision |
|---|---|
| Genre identity | Science-Fantasy |
| Civilization aesthetic | Renaissance |
| Magic | Common |
| World scale | Unknown / deliberately undecided |

The world should combine magic, research and practical technology without assuming modern industrial or electronic technology. The source and full metaphysics of magic remain open until a playable need requires them.

## Opening experience

The first vertical slice is **Opening Day**:

1. Create a player character.
2. Travel by horse carriage toward the Academy around 07:00–08:00.
3. Arrive among many new students.
4. Reach the opening ceremony by following the crowd or meeting Mira and walking with her.
5. Attend a concise ceremony and become acquainted with the Academy and a few people.
6. Explore selected Academy and nearby town locations freely during the afternoon.
7. Return to the dormitory and sleep, beginning the next day.

The opening introduces the world through ordinary experiences. It does not assign a mandatory long-term goal or force a crisis.

## Scope rules

- Do not build a full world encyclopedia before the first region is playable.
- Do not build universal NPC simulation, generic quest frameworks or broad content tools without a demonstrated need.
- Prefer authored data for content that uses existing runtime behavior; new behavior still requires code and tests.
- A system is valuable when it creates a visible activity, choice, reaction or consequence for the player.
- Keep optional content optional. Do not make one playstyle a gate to unrelated content.
- A playable day is stronger evidence than a large collection of disconnected systems or JSON files.
