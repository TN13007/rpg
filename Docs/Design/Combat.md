# Combat Design

**Status:** Core direction accepted; detailed values and edge cases remain provisional  
**Last updated:** 2026-11-20

## 1. Combat identity

Combat is a challenging, turn-based Tactical Timeline system. The player directly controls a four-character party and wins by manipulating action order, coordinating skills, managing resources and responding to enemy threats.

The intended fantasy is to create deliberate sequences in which one character delays or disables a threat, another protects an ally, and another exploits the opening to finish the target. The player should feel like a tactician controlling the flow of battle, not a spectator waiting for automated allies.

## 2. Accepted combat decisions

| Dimension | Decision |
|---|---|
| Combat style | Turn-based, Timeline-based |
| Difficulty | Challenging |
| Party size | Four characters active in combat |
| Party control | Full control; player chooses every party member's action |
| Positioning | None; no front/back rows or tactical grid |
| Timeline visibility | Show the expected action order and timing for both sides |
| Resource persistence | Hybrid; selected resources persist between encounters while some recover through combat or rest |
| Defeat state | Characters become Downed and may be revived |
| World structure | Hybrid Academy hub and adventure areas |

These are design decisions. They do not assert that the current code implements them.

## 3. Timeline

The Timeline is the primary tactical interface. It answers **who is expected to act next and when**.

It should:
- Display the expected action order and timing for all active party members and enemies.
- Update when actions, recovery, control effects or state changes alter the schedule.
- Make the consequences of a selected action understandable before confirmation where practical.
- Use deterministic tie-breaking for equal action times.
- Distinguish expected timing from uncertain outcomes.

Enemy intent is a related but separate information layer. Timeline visibility is accepted; the exact amount of enemy intent/telegraph information is still open. The design should communicate enough to support informed decisions without revealing every hidden behavior.

## 4. Action timing vocabulary

These terms must not be treated as synonyms:

| Term | Design meaning |
|---|---|
| Action Cost | Time or scheduling cost associated with performing an action |
| Recovery | Time until the acting character can act again |
| Delay | Moves a target's expected next action later |
| Stun | Prevents a target from taking an eligible action |
| Interrupt | Stops an action that is being prepared or executed |
| Cooldown | Restricts reuse of a skill until its cooldown condition is met |
| Casting | A preparation period during which an action may be interrupted |

The exact timing formula, whether actions are instantaneous or have casting phases, and the interaction between Action Cost and Recovery are **open**. Avoid double-counting time by adding every timing field indiscriminately.

## 5. Control effects

- **Delay** changes when a target can act; it does not automatically cancel its action.
- **Stun** removes or blocks an action opportunity; it should be distinct from merely moving the target later.
- **Interrupt** cancels a defined in-progress action; not every action needs to be interruptible.
- **Counter** creates a response under a defined defensive condition.
- **Stagger / Break** may create an opportunity window, but its exact relationship to Stun and vulnerability needs to be explicit.

Control should be powerful enough to enable the intended combo fantasy, but repeated control must not become a universal stun-lock strategy. Enemies should have distinct control susceptibility and encounter-specific responses. Bosses should not be universally immune to every form of control.

## 6. Party

All four party members participate in combat. The player selects actions for every member; companions do not autonomously spend resources or override the player's plan.

Character identity and build determine tactical contribution. Roles such as Controller, Defender, Striker and Support are useful design lenses, not mandatory classes or fixed party slots. A character may combine roles, and multiple party compositions should remain viable.

No spatial formation is used. Target restrictions, area effects, protection and range may exist as explicit skill rules, but must not silently depend on a hidden front/back row or grid position.

## 7. Enemy encounter design

Enemies should differ by tactical problem, not only by HP and damage. Example archetypes:
- **Skirmisher:** fast pressure against vulnerable party members.
- **Brute:** slow, high-impact attacks that create a visible response window.
- **Shaman:** buffs, healing or other support actions that may be interrupted.
- **Guardian:** protects allies or punishes careless attacks.
- **Assassin:** threatens support or damage-dealing characters.

An encounter should combine enemy behaviors to create competing priorities. There should usually be more than one viable response.

## 8. Party survival and Downed

When HP reaches zero, a character enters **Downed** rather than immediately causing total party defeat.

Design intent:
- A Downed character cannot take normal actions.
- The character leaves the normal action order while Downed.
- An ally skill or a limited-use item may revive them.
- Revival should return the character with limited HP and/or a temporary drawback, so it is a meaningful recovery rather than a free extra turn.
- If all four party members are Downed, the party loses the encounter.
- Some enemies or bosses may threaten Downed characters as a special encounter rule.

Whether Downed characters can be targeted, how bleed-out/death is handled, revival costs and post-revival timing are **open** and require playtesting.

## 9. Resource persistence

The accepted direction is **hybrid persistence**. It should create decisions across an adventure without making every encounter feel like a punishment for the previous one.

HP and selected expedition resources may persist between encounters. Other resources may recover through combat actions, milestones, consumables, rest or returning to the Academy. The exact resource list, regeneration rates, post-combat recovery and checkpoint rules are **open**.

Avoid adding multiple overlapping resource bars unless each one supports a distinct decision.

## 10. Difficulty principles

Challenging combat should come from:
- Threats with readable but consequential actions.
- Action-order trade-offs and recovery windows.
- Resource costs and opportunity costs.
- Different enemy behaviors and control resistances.
- The need to protect and revive party members.
- Encounter objectives that sometimes reward something other than maximum damage.

Do not create difficulty primarily through inflated enemy HP, unavoidable damage or hidden rules.

## 11. Encounter success and failure

An encounter may end through:
- Defeating all required enemies.
- Completing a defined objective.
- Escaping where the encounter permits it.
- Party defeat when all four characters are Downed.
- A special encounter-specific success or failure condition.

Escape availability and its costs are **open**.

## 12. Combat acceptance questions

Before numeric balance is finalized, a playable encounter must demonstrate:
- The Timeline is understandable and meaningfully changes after actions.
- Four active characters create interesting choices without excessive decision overhead.
- Different party members have reasons to act beyond repeating their highest-damage skill.
- Delay, Stun and Interrupt produce distinct tactical outcomes.
- Enemies can threaten the party without relying on opaque randomness.
- Downed and revival create tension without producing an endless revive loop.
- Resource persistence encourages expedition decisions without excessive attrition.

## 13. Explicitly not decided

Do not treat the following as final:
- Exact action-time formula and time units.
- Exact Action Cost / Recovery / casting interaction.
- Number of actions per character before an enemy acts.
- Full enemy intent disclosure.
- Exact status duration, stacking, resistance and diminishing returns.
- Exact resource types and regeneration.
- Downed timer, targeting, revival costs and post-revival penalties.
- Escape rules.
- Exact party roles, character identities, skill lists and numerical balance.
- Whether Stagger is a shared universal system or encounter-specific.

These require a playable combat slice and observed results.
