# Gameplay Loop

**Status:** Design direction  
**Last updated:** 2026-11-20

## 1. Primary loop

The game alternates between the Academy hub and adventure areas.

```text
Academy
  Prepare party, train, recover, interact and choose an expedition
       |
       v
Adventure
  Explore, discover objectives, encounter enemies and make decisions
       |
       v
Combat / Events
  Fight, resolve threats, protect allies and earn rewards
       |
       v
Consequences and Rewards
  Gain resources, equipment, knowledge, story changes and character growth
       |
       v
Return to Academy
  Recover, develop builds, deepen relationships and prepare again
```

This is a player-experience loop, not a technical state machine or a required fixed sequence for every mission.

## 2. Academy

The Academy is a supporting hub. Its activities should serve one or more of these purposes:

- Prepare the party for upcoming adventures.
- Train characters and unlock or improve combat options.
- Recover from injuries and resource depletion.
- Recruit or meet party members.
- Develop relationships and reveal character stories.
- Receive quests, intelligence and world updates.

Avoid expanding daily-life simulation or schedules unless they create meaningful decisions or support a character/narrative experience.

## 3. Adventure

An adventure is a player-facing expedition into a distinct location or situation. It may contain:

- Exploration and discoveries.
- Combat encounters with different tactical problems.
- Objectives that can be completed in different ways.
- Resource and survival decisions.
- Story events and consequences.
- Optional rewards or risks.

Not every adventure must be a long dungeon. A short mission can be valuable if it offers a complete, memorable experience.

## 4. Combat loop

1. Read the encounter and visible Timeline.
2. Identify imminent threats and opportunities.
3. Select an action for the next eligible party member.
4. Resolve the action and update the Timeline.
5. Respond to enemy actions and changing conditions.
6. Coordinate the party to defeat enemies or satisfy the encounter objective.
7. Resolve victory, defeat, escape or objective-specific outcomes.

Combat should reward intentional action sequencing rather than repetitive menu selection.

## 5. Between-encounter decisions

HP and selected resources may persist between encounters. This creates expedition-level decisions about whether to continue, recover, spend consumables or return to the Academy.

The exact resource set, recovery rates, rest rules and expedition structure remain subject to playtesting. Do not create additional resource bars without a clear decision they enable.

## 6. Player-facing quality bar

A complete loop should let the player understand:
- Why they are going on an adventure.
- What they can do before leaving.
- What makes each encounter tactically distinct.
- What they gained or lost.
- How the result changes their next decision.
