# Combat Design

**Status:** Turn-based redesign accepted; action economy, numeric balance and advanced effects remain provisional  
**Last updated:** 2026-11-22

## 1. Combat identity

Combat is a character-focused, turn-based RPG encounter. The player directly controls a four-character party and chooses each party member's actions. Combat is built around three interacting concerns:

- **Physical survival:** manage HP, damage and incapacitation.
- **Satisfaction:** build or manage Satisfaction through actions such as Tease and Allure. Satisfaction is a combat state with consequences, not merely a decorative meter.
- **Action economy:** decide how often each combatant can act, manage resources and use skills when their turn-based cooldowns permit.

The presentation prioritizes the active character and the party's condition. Combat should communicate important state through character presentation, animation and compact meters rather than requiring the player to read a large amount of text.

## 2. Accepted decisions

| Dimension | Decision |
|---|---|
| Combat style | Turn-based; no real-time Timeline |
| Timeline | Removed from the intended combat design |
| Enemy Intent | Removed; no advance intent markers or intent UI |
| Party size | Four active characters |
| Party control | Full control; player chooses every party member's action |
| Positioning | No front/back rows or tactical grid |
| Enemy behavior | Enemies execute authored, repeating or otherwise fixed action patterns |
| Action frequency | AGI or another selected attribute determines how many actions a combatant can take in a turn/round; exact rule is open |
| Enemy visible meters | HP and Satisfaction only |
| Party visible meters | HP, Satisfaction and relevant resources such as Mana or Stamina |
| Skill cooldown | Measured in turns, not elapsed time |
| Enemy physical defeat | HP reaching zero incapacitates the enemy; it can no longer act |
| Satisfaction threshold | Satisfaction begins at 0 and reaching 100 triggers Orgasm |
| Orgasm consequence | Each occurrence removes a configured amount of HP and may apply Fatigue, Stun or another authored consequence |
| Resource persistence | Hybrid; exact resource types and recovery rules remain open |
| World structure | University and city locations with optional paranormal incidents |

These are design decisions, not claims about current runtime implementation. Numeric values and unresolved rules below remain provisional.

## 3. Turn structure and action economy

Combat no longer advances through a shared real-time clock. There is no Remaining Progress, Time-to-Ready, Recovery Cost, real-time pause or Timeline scheduling.

Combat is resolved through discrete turns. The exact definition of a **turn** versus a **round** must be made explicit before implementation. In particular, the design must define when all eligible party members and enemies have had their action opportunities and when turn-based effects/cooldowns advance.

### 3.1 AGI and action frequency

AGI is the leading candidate for determining how many times a combatant may act during a turn/round. The attribute, formula, breakpoints, caps and tie-breaking rules have not yet been selected.

The action-frequency rule must satisfy these constraints:

- Higher AGI should provide a meaningful advantage in action frequency.
- A faster combatant must not gain unlimited actions or make slower party members irrelevant.
- The rule must be predictable enough for players to plan.
- Enemy patterns must remain understandable when an enemy gains multiple action opportunities.
- The system must define whether an action opportunity can be saved, lost or carried into the next turn/round.

Do not reuse the old Timeline Speed/Progress formula under new names. This is a discrete action-economy system.

## 4. Enemy behavior patterns

Enemies no longer announce their next action through Enemy Intent. They follow authored behavior patterns and execute the next eligible pattern step when their action opportunity arrives.

A pattern may contain multiple action types, for example:

`Attack → Attack → Special → Recover → Repeat`

This is an illustrative pattern, not a committed pattern for a specific enemy.

Pattern definitions must eventually specify:

- Ordered actions and repeat behavior.
- Conditions that can skip or replace a step, if conditional branches are needed.
- What happens when an action has no valid target.
- Whether control effects pause, skip or otherwise alter pattern progression.
- How multiple action opportunities in one turn/round interact with pattern steps.

**Open:** whether the pattern advances after each enemy action or after each round. This must be decided before implementing the pattern runner.

Patterns should be authored per enemy or enemy archetype. Avoid replacing the removed Intent system with a new UI that exposes the full future pattern; the player should understand behavior through what the enemy does and through consistent animation/feedback.

## 5. Combat meters and defeat states

### 5.1 Enemy meters

Each enemy exposes exactly two primary meters in the combat UI:

| Meter | Meaning |
|---|---|
| HP | Physical survivability. At 0, the enemy is incapacitated and can no longer act. |
| Satisfaction | Starts at 0 and increases when affected by actions such as Tease or Allure. At 100, it triggers Orgasm. |

Enemy cards should not display a collection of secondary combat stats or a persistent intent description. Other data may exist in the simulation, but it should not be added to the enemy's primary UI without a demonstrated player decision that requires it.

### 5.2 Satisfaction and Orgasm

Satisfaction is accumulated from actions and effects that explicitly contribute to it. When Satisfaction reaches 100, the enemy triggers an Orgasm event.

The event has two accepted consequences:

1. A configured amount of HP is removed, representing physical exhaustion.
2. The event may apply additional conditions such as Fatigue or Stun.

The following details remain open and must not be silently hard-coded as design decisions:

- Whether Satisfaction resets to 0, retains overflow or follows another rule after Orgasm.
- Whether HP loss is fixed, percentage-based or scaled by other factors.
- Whether Fatigue and Stun are guaranteed, conditional or selected by enemy-specific rules.
- Whether repeated Orgasm events become less effective or create escalating consequences.
- Whether an enemy can be incapacitated by the HP loss caused by Orgasm.

Orgasm is a combat event, not automatically an instant victory. The enemy becomes unable to act when its HP reaches 0, regardless of which effect caused the HP loss.

### 5.3 Party meters

Party members expose:

- **HP:** physical survivability.
- **Satisfaction:** the character's current Satisfaction state.
- **Relevant resources:** for example Mana or Stamina, according to the character's abilities and available actions.

The UI should not show resource meters that have no meaningful gameplay use for that character. Party Satisfaction may have consequences distinct from enemy Satisfaction; those consequences are not yet defined and must not be copied from enemy rules by assumption.

### 5.4 Incapacitation and recovery

Enemy HP reaching 0 means the enemy is incapacitated and cannot perform further actions in the encounter.

The party's zero-HP state, Downed/revival behavior and post-encounter recovery need to be reconciled with the new turn-based model before implementation. Existing Timeline-specific revival timing and Recovery Cost rules are superseded and must not be carried forward unchanged.

## 6. Body profile, exposure and arousal interaction

**Status:** Body Profile progression direction selected; individual attributes, growth sources, formulas and balance remain proposed/open.

This system adds a hidden body-state simulation layer to combat. It must remain separate from the two primary combat meters (HP and Satisfaction) and must not turn the combat UI into a display of body-part statistics.

### 6.1 Body Profile

Each character may have a persistent Body Profile containing data for the following regions:

- Mouth
- Hands
- Chest
- Genitals
- Anus
- Skin

The selected progression direction is **B: body attributes can develop over the course of the game**. These attributes are not assumed to be fixed character constants. Growth may be influenced by authored experiences, training, milestones or other progression sources, but the exact sources and rates are not yet decided.

Candidate per-region attributes:

| Attribute | Intended meaning | Design status |
|---|---|---|
| Sensitivity | How strongly the region responds to a relevant interaction | Proposed |
| Resilience | How well the region resists or mitigates relevant effects | Proposed |
| Familiarity | How accustomed the character is to a category of interaction | Proposed |

These are candidates, not a requirement that every region must have all three attributes. Do not add attributes unless they produce distinct, testable gameplay outcomes. Growth must not be assumed to be universally beneficial: increasing one capability may create a trade-off, change a response profile or have no effect outside relevant interactions.

### 6.2 Current Body State

Persistent Body Profile data must be distinguished from temporary encounter state.

| State | Intended meaning | Typical source |
|---|---|---|
| Exposure | Whether and how much a region is currently exposed | Clothing, actions, encounter events |
| Accessibility | Whether an opponent can currently reach or affect the region | Position/context, restraints or other encounter conditions |
| Temporary Effects | Short-lived modifiers to relevant body interactions | Skills, enemy actions, status effects |

Exposure and Accessibility are not synonyms. A region can be exposed but not reachable, or reachable while covered. The system must not infer one automatically from the other.

### 6.3 Enemy Preferences and target selection

Enemy Preference Profile is owned by each enemy, not by the target character. It describes which regions or interaction categories that enemy tends to prioritize.

An enemy must not select a target solely because its preference value is high. Candidate selection should account for at least:

- Preference compatibility.
- Current Exposure.
- Current Accessibility.
- Whether the enemy has an available action capable of affecting that region.
- Encounter-specific constraints and valid targets.

For example, an enemy with a strong preference for the chest may prioritize that region when it is exposed and accessible. If it is covered or inaccessible, the enemy should select another valid behavior rather than repeatedly attempting an impossible action.

This is a behavioral tendency, not a guarantee that every enemy action targets its favorite region. Enemy patterns remain the authority for which action types are available; preference helps choose among valid options and must not silently override the authored pattern.

### 6.4 Arousal effects and combat resolution

Arousal-related effects are a separate resolution channel from physical HP damage. They must not be added to HP damage as if they were the same resource.

The design must define how an action's base effect interacts with the target region's attributes, current body state and the enemy's preference. A multiplicative expression such as:

`ArousalEffect = BaseEffect × Sensitivity × Exposure × Accessibility × Preference`

is only an illustrative hypothesis. It is not an accepted formula. In particular, the factors need defined scales, bounds, zero behavior and diminishing returns before implementation. Avoid a formula in which multiplying several loosely normalized values creates extreme or unintuitive outcomes.

The combat model currently defines Enemy Satisfaction as a visible meter that reaches its threshold and triggers Orgasm. The relationship between ArousalEffect and Satisfaction gain must be explicit: do not create a second hidden arousal meter that duplicates Satisfaction. If ArousalEffect contributes to Satisfaction, document the conversion and feedback clearly. Party Satisfaction remains a separate unresolved design question; enemy consequences must not be copied to party members by assumption.

### 6.5 UI and information boundaries

Body Profile attributes are simulation data and are not displayed as combat meters. The combat UI continues to prioritize:

- Enemy: HP and Satisfaction.
- Party member: HP, Satisfaction and relevant resources.
- Active character and readable action feedback.

Body-related information may be communicated through contextual animation, presentation or concise event feedback when it helps the player understand why an outcome occurred. Do not expose a dense numeric body-stat panel merely to make the hidden simulation inspectable.

### 6.6 Progression and implementation boundaries

Body Profile is persistent character progression data; Exposure, Accessibility and Temporary Effects belong to current state. Enemy Preference belongs to enemy data. These ownership boundaries must remain distinct.

The combat resolver may request the relevant values through a dedicated domain rule, but UI components must not calculate Arousal effects, enemy preference scores or body progression. Progression rules own permanent growth; encounter/combat rules own temporary state and action resolution.

Before implementation, validate the system in a small encounter and answer these open questions:

- Which body attributes are genuinely necessary, and do they have distinct effects?
- What activities or milestones change each attribute? Can attributes decrease?
- Is Familiarity a per-region value, a per-interaction-category value or unnecessary?
- How are Exposure and Accessibility represented and updated?
- How does Enemy Preference interact with fixed enemy patterns without making behavior random or opaque?
- Does ArousalEffect directly produce Satisfaction, modify it, or resolve through a separate effect?
- What are the bounds, diminishing returns and counterplay options?
- How can the player understand cause and effect without seeing hidden numeric attributes?

## 7. Skills, resources and cooldowns

Skill use is governed by resource costs and turn-based cooldowns. Cooldowns do not use real-world seconds or Combat Time.

A cooldown value represents the number of turns that must pass before the skill can be used again. The implementation must define one consistent decrement point (for example, at the start or end of a turn) and make the displayed remaining value match the actual availability rule.

Resource Cost and Cooldown are separate:

- **Resource Cost** spends Mana, Stamina or another defined resource.
- **Cooldown** temporarily prevents reuse of the skill.
- Neither value determines action frequency; that belongs to the action-economy rule.

The exact cooldown counting convention, whether cooldowns advance on the owner's turns or global rounds, and whether a skill can have both a resource cost and cooldown remain to be finalized.

## 8. Party and character presentation

All four party members participate in combat and remain player-controlled. The player chooses each member's action; companions do not autonomously override the player's decisions.

The combat screen's visual priority is the character currently taking an action. The main presentation area should focus on that one active character rather than showing four large portraits simultaneously.

The party roster remains visible as a compact status overview so the player can monitor all four members and make decisions based on team-wide conditions. Each member's status presentation includes HP, Satisfaction and relevant resources. Status effects may be shown beneath each member, with a bounded, independently scrollable area so long effect lists do not distort the layout.

Enemy presentation is secondary to the active character. Enemies are represented in a compact vertical list; each enemy exposes only its HP and Satisfaction meters as primary combat information.

Action selection should group available choices by category. The selected action's details should be shown separately, including relevant cost, cooldown, effect, target requirements and description. UI components render supplied presentation data; they do not calculate combat outcomes or own combat rules.

## 9. Control effects and statuses

Statuses such as Fatigue and Stun may be applied by combat events, skills or enemy patterns. Their exact behavior and duration rules must be defined in turn-based terms.

Do not retain the old assumption that Stun freezes Timeline progress. In the new model, Stun must specify which action opportunities it prevents and how its duration is counted.

The status catalogue, stacking, resistance, refresh rules and anti-lockout safeguards remain open. The first implementation should support only the statuses needed by a playable encounter.

## 10. Encounter outcomes and difficulty

An encounter may end through defeating required enemies, completing an objective, escaping where permitted, party defeat or a special encounter condition.

Difficulty should come from readable enemy patterns, action-economy trade-offs, resource and cooldown management, Satisfaction pressure, party protection and meaningful consequences. Do not create difficulty primarily through inflated HP, unavoidable damage or hidden rules.

## 11. Validation before balance

A playable encounter using the new model must demonstrate:

- No real-time Timeline or Enemy Intent dependency remains in the intended combat flow.
- Party members and enemies receive action opportunities according to the discrete turn/action-economy rules.
- AGI affects action frequency without creating runaway extra turns.
- Enemy patterns execute deterministically and remain understandable.
- Enemy HP and Satisfaction are sufficient to communicate the two primary enemy defeat/progression paths.
- Tease and Allure can increase Satisfaction through explicit, testable effects.
- Satisfaction reaching 100 triggers Orgasm exactly once according to the selected threshold/reset rule.
- Orgasm applies HP loss and any configured secondary consequences.
- Party HP, Satisfaction and relevant resources are visible and update after actions.
- Skill cooldowns advance by turns according to one documented counting convention.
- Stun and other statuses use turn-based duration semantics.
- Four party members create meaningful decisions without excessive decision overhead.

## 12. Superseded design

The previous Continuous Timeline model is no longer the intended combat design. Its shared Combat Clock, Remaining Progress, Speed-based readiness, Initial Progress, Recovery Cost, Delay/Acceleration scheduling, real-time Stun duration, Timeline markers and committed Enemy Intent are superseded.

Historical implementation documentation may still describe those systems as they exist in the current repository. Such descriptions are implementation snapshots, not the current design direction. Update implementation documents only when the runtime is actually migrated.
