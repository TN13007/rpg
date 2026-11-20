# Combat Design

**Status:** Core direction accepted; detailed values and edge cases remain provisional  
**Last updated:** 2026-11-20

## 1. Combat identity

Combat is a challenging, turn-based Tactical Timeline system. The player directly controls a four-character party and wins by manipulating action order, coordinating skills, managing resources and responding to enemy threats.

The intended fantasy is to create deliberate sequences in which one character delays or disables a threat, another protects an ally, and another exploits the opening to finish the target. The player should feel like a tactician controlling the flow of battle, not a spectator waiting for automated allies.

## 2. Accepted decisions

| Dimension | Decision |
|---|---|
| Combat style | Turn-based, Timeline-based |
| Difficulty | Challenging |
| Party size | Four active characters |
| Party control | Full control; player chooses every party member's action |
| Positioning | None; no front/back rows or tactical grid |
| Timeline | Show expected action order and timing for both sides |
| Resource persistence | Hybrid; selected resources persist while others recover through combat or rest |
| Defeat state | Characters become Downed and may be revived |
| World structure | Hybrid Academy hub and adventure areas |

These are design decisions. They do not assert that the current code implements them.

## 3. Timeline

The Timeline is the primary tactical interface. It answers **who is expected to act next and when**.

It should:
- Display expected action order and timing for active party members and enemies.
- Update when actions, recovery, control effects or state changes alter the schedule.
- Make the consequences of a selected action understandable before confirmation where practical.
- Use deterministic tie-breaking for equal action times.
- Distinguish expected timing from uncertain outcomes.

Enemy intent is a related but separate information layer. Timeline visibility is accepted; the exact amount of intent/telegraph information remains open. Communicate enough to support informed decisions without revealing every hidden behavior.

## 4. Action timing vocabulary

| Term | Design meaning |
|---|---|
| Action Cost | Time or scheduling cost associated with performing an action |
| Recovery | Time until the acting character can act again |
| Delay | Moves a target's expected next action later |
| Stun | Prevents a target from taking an eligible action |
| Interrupt | Stops an action being prepared or executed |
| Cooldown | Restricts reuse of a skill until its cooldown condition is met |
| Casting | A preparation period during which an action may be interrupted |

These terms are not synonyms. The exact timing formula, whether actions are instantaneous or have casting phases, and the interaction between Action Cost and Recovery are **open**. Avoid double-counting time by adding every timing field indiscriminately.

## 5. Control effects

- **Delay** changes when a target can act; it does not automatically cancel the action.
- **Stun** removes or blocks an action opportunity; it is distinct from merely moving the target later.
- **Interrupt** cancels a defined in-progress action; not every action needs to be interruptible.
- **Counter** creates a response under a defined defensive condition.
- **Stagger / Break** may create an opportunity window, but its relationship to Stun and vulnerability needs to be explicit.

Control should be powerful enough to enable combo play, but repeated control must not become a universal stun-lock strategy. Enemies should have distinct control susceptibility and encounter-specific responses. Bosses should not be universally immune to every form of control.

## 6. Party

All four party members participate in combat. The player selects actions for every member; companions do not autonomously spend resources or override the player's plan.

Character identity and build determine tactical contribution. Controller, Defender, Striker and Support are useful design lenses, not mandatory classes or fixed party slots. Characters may combine roles, and multiple party compositions should remain viable.

No spatial formation is used. Target restrictions, area effects, protection and range may exist as explicit skill rules, but must not silently depend on a hidden front/back row or grid position.

## 7. Enemy encounter design

Enemies should differ by tactical problem, not only HP and damage. Candidate archetypes include:
- **Skirmisher:** fast pressure against vulnerable party members.
- **Brute:** slow, high-impact attacks that create a visible response window.
- **Shaman:** buffs, healing or support actions that may be interrupted.
- **Guardian:** protects allies or punishes careless attacks.
- **Assassin:** threatens support or damage-dealing characters.

An encounter should combine behaviors to create competing priorities. There should usually be more than one viable response.

## 8. Downed and revival

When HP reaches zero, a character enters **Downed** rather than immediately causing total party defeat.

Design intent:
- A Downed character cannot take normal actions and leaves the normal action order.
- An ally skill or limited-use item may revive them.
- Revival returns the character with limited HP and/or a temporary drawback, making recovery consequential.
- If all four party members are Downed, the party loses the encounter.
- Some enemies or bosses may threaten Downed characters as a special encounter rule.

Whether Downed characters can be targeted, how bleed-out/death is handled, revival costs and post-revival timing remain **open** and require playtesting.

## 9. Resource persistence

The accepted direction is **hybrid persistence**. It should create decisions across an adventure without making every encounter feel like punishment for the previous one.

HP and selected expedition resources may persist between encounters. Other resources may recover through combat actions, milestones, consumables, rest or returning to the Academy. Exact resource types, regeneration rates, post-combat recovery and checkpoint rules remain **open**.

Avoid adding overlapping resource bars unless each supports a distinct decision.

## 10. Difficulty and encounter outcomes

Challenging combat should come from readable but consequential threats, action-order trade-offs, recovery windows, resource and opportunity costs, distinct enemy behavior, control resistance, party protection and encounter objectives beyond maximum damage.

Do not create difficulty primarily through inflated HP, unavoidable damage or hidden rules.

An encounter may end through defeating required enemies, completing an objective, escaping where permitted, party defeat or a special encounter condition. Escape availability and costs remain **open**.

## 11. Validation before balance

A playable encounter must demonstrate:
- The Timeline is understandable and meaningfully changes after actions.
- Four active characters create interesting choices without excessive decision overhead.
- Different party members have reasons to act beyond repeating their highest-damage skill.
- Delay, Stun and Interrupt produce distinct tactical outcomes.
- Enemies threaten the party without relying on opaque randomness.
- Downed and revival create tension without an endless revive loop.
- Resource persistence encourages expedition decisions without excessive attrition.

Do not finalize numeric balance before this slice is playable.

## 12. Explicitly open

Exact action-time formula and units; Action Cost / Recovery / casting interaction; number of actions before an enemy acts; enemy intent disclosure; status duration, stacking and resistance; resource types and regeneration; Downed timer, targeting, revival costs and penalties; escape rules; final party roles, character identities and skill lists; and whether Stagger is universal or encounter-specific.
