# Combat Design

**Status:** Discrete turn-based direction accepted; numeric tuning and selected edge cases remain open  
**Last updated:** 2026-11-22

## 1. Combat identity

Combat is a discrete, party-command turn-based system designed to make decisions easy to read and fast to resolve. The player controls a four-character Party through one shared Action Point pool. The player can chain skills from any active party member before handing the turn to the enemies.

The intended experience is a continuous player-directed combo within a clear turn boundary: choose a character, use a skill, observe the result, then choose the next character or skill while Action Points remain. The player should think about the sequence and cost of actions, not wait for individual characters to recover on a real-time Timeline.

Combat is a specialized response to dangerous situations. It does not replace investigation, retreat or professional help, and it is not the default response to every paranormal incident.

## 2. Accepted decisions

| Dimension | Decision |
|---|---|
| Combat style | Discrete turn-based; no real-time Timeline scheduler |
| Difficulty | Challenging; threats are readable but consequential |
| Round structure | Player Turn followed by Enemy Turn |
| Party size | Four active characters |
| Party control | The player chooses every Party action; no autonomous Party AI |
| Action economy | One shared Action Point pool for the whole Party |
| Skill cost | Each skill use costs 1 AP in the first playable slice |
| Action Capacity | Starts at 2 AP; two training milestones raise it to 3 and then 4 AP |
| Actor selection | Any active Party member may act; each member has a one-Round action cooldown after a successful AP-funded action |
| Turn ending | Player may end the turn early; unused AP does not carry over |
| Enemy behavior | Authored deterministic action patterns evaluated against current conditions during Enemy Turn |
| Enemy actions | Each living enemy resolves one pattern action per Enemy Turn in the first slice |
| Initiative | No per-combatant initiative, Speed, Ready state or Luck tie-break scheduler |
| Positioning | No front/back rows or tactical grid |
| Combat time | No shared real-time Combat Clock; turn boundaries drive combat |
| Enemy information | No committed future Enemy Intent or Intent UI; relevant pattern conditions and current states are visible |
| Resource persistence | Hybrid; HP and selected expedition resources may persist while other resources recover through combat, milestones, consumables or rest |
| Encounter outcomes | Required enemy defeat, objective completion, permitted escape, Party defeat or an authored special condition |
| HP | Reaching 0 HP incapacitates a Party member or defeats an Enemy |
| Arousal / Orgasm | Included in the first playable MVP: Arousal starts at 0, caps at 100, and threshold crossing triggers one Orgasm event, resets Arousal and applies configured HP damage |
| Defeat state | Party members become Downed and may be revived; all Party members Downed ends the encounter |
| World time | GameClock is not advanced per combat action or turn; any encounter time cost is applied once at session completion |

These are target design decisions, not claims that the current runtime already implements them. Action Capacity starts at 2 AP; two fixed training milestones raise it to 3 AP and then 4 AP. The activity costs and exact milestone requirements must be tested in a playable encounter. Arousal/Orgasm is confirmed as part of the first playable MVP; the consequence for Party members reaching the threshold remains an explicit balance/design question.

## 3. Round and turn flow

A Round consists of two phases:

1. **Player Turn:** refresh the shared AP pool to the Party's current Action Capacity. The player chooses an available skill and an eligible Party member, resolves the action, and spends 1 AP. Repeat while AP remains, or end the turn early.
2. **Enemy Turn:** each living Enemy takes its turn in the encounter's authored deterministic order. It evaluates its action pattern against the current combat state and resolves one eligible pattern action.

If the encounter remains active after the Enemy Turn, begin the next Round and refresh the Party's AP. There is no real-time waiting between actions and no combatant independently becomes Ready.

Check victory, defeat, objective completion, permitted escape and other authored terminal conditions immediately after every resolved action. Escape is available only when the encounter definition permits it. If the encounter ends, stop the remaining Player or Enemy actions for that Round. Increment the Round counter exactly once after the Enemy Turn completes, not after each Enemy action.

## 4. Shared Action Points

### 4.1 Action Capacity and the AP pool

Action Capacity determines how many skill uses the Party can make during one Player Turn. It is a single Party-level progression value, not four separate character action bars and not the sum of the four characters' Agility values.

At the beginning of each Player Turn:

- Set current AP to the Party's Action Capacity.
- The player may select any active Party member for each skill use.
- Every skill use consumes 1 AP in the first playable slice.
- AP is shared across the Party; using an action with one character reduces the same pool available to everyone else.
- A Party member may perform at most one AP-funded action per Round. After a successful action, that actor receives a one-Round action cooldown and cannot act again until the next Player Turn begins.
- Current AP cannot exceed the Action Capacity. Unspent AP is discarded when the Player Turn ends and never carries into a later Round.

The first playable slice starts at 2 AP and caps Action Capacity at 4 AP. Training raises the Party's capacity through exactly two milestones: 2 → 3 AP, then 3 → 4 AP. This keeps growth finite and intentional rather than repeatable stat grinding. The milestone requirements and activity costs remain open for playtesting.

For the first combat migration, supply Action Capacity as an authored encounter parameter starting at 2 AP; do not add persistent Party save data yet. The later training/progression issue decides its persistent storage and ownership. Do not duplicate it across four character records or derive it from a sum of individual attributes.

### 4.2 Action resolution and validation

The player chooses an actor, skill and required target. The owning combat service validates the request against the current encounter state before spending AP.

- A Downed, defeated or otherwise ineligible actor cannot act.
- A skill can be used only when its target requirements and other conditions are satisfied.
- Insufficient Stamina or another required resource rejects the action without spending AP.
- An invalid actor, skill or target does not spend AP and does not mutate combat state.
- After a successful action, spend 1 AP, resolve its effects and refresh the available choices.

AP is separate from Stamina, HP and Arousal. AP controls the number of skill uses in the current Player Turn; a skill may also spend Stamina or another gameplay resource. The first-slice cooldown is actor-level: after acting, that Party member cannot act again during the same Round. Do not add separate per-skill cooldowns unless a tested encounter demonstrates a need. Do not recreate Recovery Cost or elapsed-time scheduling through a differently named resource.

### 4.3 Combo sequence

The player distributes AP across different Party members. In the first slice, each character can perform at most one AP-funded action in a Round; this one-Round actor cooldown prevents the shared pool from being concentrated on a single character while preserving player choice over which members participate.

Example with 2 AP:

1. Alex uses Quick Strike (1 AP).
2. Mira uses Disrupting Strike (1 AP).
3. AP reaches 0, so the Enemy Turn begins.

The player may choose any two eligible Party members and any valid skills. The restriction is on repeating an actor within the same Round, not on a fixed character order or a personal AP bar.

### 4.4 Ending the Player Turn

The Player Turn ends when AP reaches 0 or the player explicitly chooses End Turn. Ending early discards the remaining AP and gives the Enemy Turn control of the encounter. The player may therefore trade unused actions for an earlier enemy response, but the game must communicate that consequence clearly.

## 5. Enemy action patterns

Enemies do not have individual Timelines and do not commit a visible future Intent. Their behavior is resolved only when their Enemy Turn arrives.

Each Enemy uses a small authored ordered pattern. A pattern step contains an action, its target rule and optional conditions. When the Enemy becomes active during Enemy Turn:

1. Evaluate the pattern from its current position using the combat state as it exists at that moment.
2. Select the first eligible step in authored priority order.
3. Resolve that action and advance the pattern position according to the authored sequence.
4. If no conditional step is eligible, use an explicit fallback step such as a basic attack, guard or wait.

Each living Enemy resolves at most one pattern action per Enemy Turn in the first playable slice. Multiple effects inside one skill are still one action. Enemies act in the stable order authored by the encounter, not by Agility, Speed, Luck or a real-time readiness calculation.

Conditions may inspect current facts such as an Enemy's HP, a Party member's condition, whether an ally is Downed, or whether a target is valid. Conditions are evaluated at execution time, so an earlier Enemy action or a Player action can change what a later Enemy chooses. The UI must expose relevant condition rules and current states on the Enemy card (for example, “Enraged when HP is below 50%” and whether Enraged is currently active). This communicates how the pattern can change without revealing the next action, pattern step or target.

Enemy targets are selected when the action executes. A pattern must not reveal or lock an upcoming target during the Player Turn. If no valid target exists, the action follows its authored fallback or skip behavior; the exact policy must be deterministic and tested.

Preserve the existing enemy behavior archetypes as design candidates: Skirmisher (fast pressure against vulnerable Party members), Brute (slow, high-impact attacks), Shaman (buffs, healing or support), Guardian (protects allies or punishes careless attacks), and Assassin (threatens support or damage-dealing characters). These are behavioral lenses, not mandatory classes; express them through small authored patterns rather than randomized weighted profiles.

Keep patterns content-driven and small. Do not introduce behavior trees, a general-purpose AI framework or a separate intent/telegraph scheduler for the first slice.

## 6. Skills, resources and cooldowns

Skill selection and action count are separate from skill effects:

- AP pays for using a skill during the current Player Turn.
- Stamina or another resource pays the skill's gameplay cost, if any.
- HP and Arousal are combat meters changed by skill effects.
- Every Party member has a one-Round actor cooldown after a successful AP-funded action; that member cannot act again until the next Player Turn begins.
- Do not add separate skill cooldowns to the first slice unless a concrete encounter demonstrates a need.

A successful skill consumes AP and its configured resource cost. A rejected skill consumes neither. Numeric resource costs are authored and validated by the owning combat rules.

A control skill should have a clearly observable turn-based consequence, such as preventing a specified action, changing a condition or affecting a later pattern check.

When an Enemy control action applies Stun/Delay-like control to the Party, reduce the shared AP available at the next Player Turn by 1, with a minimum of 1 AP. Multiple control sources affecting the same next Player Turn do not stack this penalty. This replaces Timeline progress freezing/delay for Party control and is the planned translation for Unit 213's existing control action.

Repeated control must not create an indefinite stun-lock. Enemy-side control effects must have bounded, deterministic turn consequences; exact resistance/refresh rules remain open until a playable skill requires them. Control cannot be used to permanently deny an Enemy its actions.

Do not add a broad status catalogue, casting phase, interrupt scheduler or timed effect engine unless a concrete encounter requires it.

## 7. HP and Arousal

**MVP requirement:** Arousal/Orgasm is part of the first playable combat slice. Implement the threshold loop below alongside Attack/HP; do not defer it to a later expansion. The consequence when a Party member reaches the threshold remains open and must not be inferred from the Enemy consequence.

HP and Arousal are separate combat meters with separate effects.

### 7.1 HP

- Attack skills reduce the target's HP by their configured amount.
- An Enemy at 0 HP is defeated and no longer takes an Enemy Turn.
- A Party member at 0 HP becomes Downed and cannot perform normal actions.
- Damage from any source, including an Orgasm consequence, uses the same HP and incapacitation rules.

### 7.2 Arousal and Orgasm

- Arousal starts at 0 and is capped at 100.
- Tease skills increase the target's Arousal by a fixed authored amount in the first playable slice.
- When Arousal reaches 100, trigger exactly one Orgasm event for that threshold crossing.
- The Orgasm event resets Arousal to 0 and applies its configured HP damage.
- Excess Arousal from the triggering action is not carried into another threshold crossing in the first slice; one action can trigger at most one Orgasm event.
- Orgasm damage can reduce HP to 0 and cause the normal defeat or Downed transition.

Do not add hidden Arousal resources, body-part multipliers or preference calculations to this MVP. Body Profile, body-region attributes, Exposure/Accessibility and Enemy Preference remain future extensions.

The exact consequence of Arousal reaching 100 for Party members is not decided by this document. Do not assume that the Enemy Orgasm consequence automatically applies to the Party.

## 8. Party, Downed and revival

All four Party members remain individually player-controlled. The player may select any active member for each AP-funded skill use. No autonomous companion behavior or command-ally queue is introduced.

A Party member at 0 HP becomes Downed, remains visible in the Party roster and cannot act or be selected as a normal target. Enemies do not target Downed Party members by default.

Initial revival direction:

- Any active Party member may use Revive on a Downed ally.
- Revive consumes 1 AP and 4 Stamina.
- Revive restores 25% of the target's maximum HP, rounded up to at least 1 HP.
- Each Party member may be revived once per encounter. Being Downed again after revival cannot be reversed a second time in that encounter.
- Revival does not apply Timeline Recovery Cost. The revived member becomes eligible for subsequent actions when the normal Player Turn and AP rules allow it.
- If all Party members are Downed, the encounter ends in defeat immediately.

If the controlled protagonist ends an encounter at 0 HP, the existing Exploration recovery direction remains: offer Request emergency rescue, advance GameClock by its configured cost and restore the protagonist to a small amount of HP. This is outside the in-combat AP system.

Revival numbers are provisional and should be validated in the Unit 213 encounter. Bleed-out, permanent death and special Enemy rules for Downed targets are out of scope.

## 9. World time and encounter completion

Combat has no shared elapsed-time simulation. Player Turns and Enemy Turns are discrete rule phases; they do not advance GameClock minute by minute.

An encounter may apply one authored world-time and activity cost when the Combat session completes. That cost is separate from AP and is applied once, not once per skill or per Round. The encounter/session rules must make the completion cost visible and avoid charging it more than once.

Daily activities continue to use GameClock. Do not merge world time and combat turns into a universal clock.

### 9.1 Hybrid resource persistence

The previously accepted hybrid persistence direction remains in effect. HP and selected expedition resources may persist between encounters. Other resources may recover through combat actions, milestones, consumables, rest or returning to campus or another safe location. Exact resource types, regeneration rates, post-combat recovery and checkpoint rules remain open. Avoid adding overlapping resource bars unless each supports a distinct player decision.

## 10. Combat presentation

The combat interface should make the current decision obvious:

- Show the current Round and whether it is the Player Turn or Enemy Turn.
- During Player Turn, show one shared AP counter and the remaining AP clearly.
- Keep all four Party members visible with HP and only resources that support a current decision; show Arousal only if the product owner retains that feature in the first MVP.
- Allow the player to select any eligible Party member, then choose an available skill and target.
- Show Enemy cards with HP and relevant current conditions; show Arousal only if the product owner retains that feature in the first MVP.
- Show hover, available-target, selected-target and invalid-target states clearly.
- Present skill results, HP changes, Downed/Revive and control conditions as observable feedback. Add Arousal/Orgasm feedback only if that feature is confirmed for the MVP.
- During Enemy Turn, show each resolved Enemy action as it happens; do not display a committed future action or target.

Remove Timeline tracks, Timeline markers, Time-to-Ready, Remaining Progress, Recovery Cost labels and Enemy Intent UI. UI components render supplied presentation data and emit requests; they do not own combat calculations or state mutation.

## 11. Encounter design and difficulty

Combat remains **Challenging**. Difficulty should come from the order and opportunity cost of the Party's limited AP, resource use, target priority, enemy patterns, readable conditions and encounter objectives.

An encounter may end through required enemy defeat, objective completion, escape when the encounter explicitly permits it, Party defeat or an authored special condition. Escape availability and its cost are defined by the encounter; it is not guaranteed in every fight.

An encounter should give the player meaningful choices about:

- Which Party member should act now.
- Which skill is worth spending an AP on.
- Whether to focus damage, control a threat or protect an ally; add Arousal-building choices only if that feature is confirmed for the MVP.
- Whether to spend the remaining AP or end the Player Turn early.
- Which Enemy pattern condition must be disrupted before Enemy Turn.

Do not create difficulty primarily through inflated HP, unavoidable damage, opaque randomness or hidden action-economy exceptions. Action Capacity must create a useful progression choice without making ordinary encounters require repetitive training.

## 12. Validation before balance

A playable encounter must demonstrate:

- The player can understand the Player Turn / Enemy Turn boundary without a Timeline.
- Arousal begins at 0, Tease adds its fixed authored amount, reaching 100 triggers exactly one Orgasm event, Arousal resets to 0, and configured HP damage is applied.
- The Party shares one AP pool; every successful skill use spends exactly 1 AP in the first slice.
- AP refreshes at the beginning of each Player Turn and unused AP does not carry over.
- The player can select any eligible Party member for each action, with each character limited to one AP-funded action per Round by the one-Round actor cooldown.
- Different action sequences create meaningfully different outcomes and using several Party members remains viable.
- The player can end a Player Turn early and understands that the Enemy Turn will begin.
- Enemies resolve deterministic authored patterns in stable order and evaluate conditions against current state while showing relevant condition rules/states without revealing the next action or target.
- Each living Enemy performs at most one pattern action per Enemy Turn in the first slice.
- Invalid actor, skill, target or insufficient-resource requests do not spend AP.
- Attack and HP damage resolve correctly.
- If Arousal/Orgasm is confirmed for the MVP, Tease, threshold crossing, exactly-once Orgasm, reset and Orgasm HP damage resolve correctly.
- Downed, Revive, Party defeat, permitted escape, objectives and other authored terminal conditions stop further actions at the correct point.
- Party control reduces the next Player Turn's AP by 1, never below 1, and multiple control sources do not stack; repeated control cannot create an indefinite stun-lock.
- Hybrid resource persistence creates decisions across encounters without requiring every resource to persist or recover at the same rate.
- GameClock is not advanced per combat action or turn; encounter completion cost is applied at most once.
- The UI communicates AP, active phase, Party state, Enemy meters and action results without Timeline or Intent.

Do not finalize numeric balance before this slice is playable.

## 13. Explicitly open

The following remain open and must be decided when a playable requirement needs them:

- Exact requirements and costs for the two Training milestones that raise Action Capacity from 2 to 3 and then 4 AP.
- Whether a future skill may cost more than 1 AP; the first slice keeps every skill at 1 AP.
- Whether playtests justify changing the one-action-per-character-per-Round cooldown after the first playable slice.
- Exact Enemy pattern cursor behavior when a condition or target is invalid, and the fallback action for each Enemy.
- Enemy-side control resistance/refresh rules, while keeping all control bounded and preventing indefinite stun-lock.
- Exact translation and tuning of Unit 213 Stun/Delay-like Party control into the next-Player-Turn AP penalty.
- What consequence, if any, applies when a Party member's Arousal reaches 100.
- Numeric HP/Arousal values, resource costs, training costs and Orgasm damage.
- Numeric Stamina/resource persistence and regeneration rules; exact escape costs, special encounter objectives and advanced Enemy actions.

Do not reintroduce Timeline, Combat Clock, Remaining Progress, Time-to-Ready or Recovery Cost as a workaround for an open question. Prefer the smallest deterministic rule that supports a visible player decision.

## 14. Attribute compatibility

Agility and Luck remain in serialized character data for compatibility with existing content and saves. They no longer determine combat Speed or initiative/tie-break order in the target turn-based model. Do not delete or repurpose these fields during the combat migration; their future gameplay roles belong to a later Progression decision.

## 15. Implementation boundary

The current implementation documentation describes the Timeline-based runtime on `main`. That remains the implementation baseline until the refactor is merged. The design in this document describes the target behavior; do not update Implementation documentation to claim turn-based behavior exists before the code, tests and Godot runtime are migrated.

Implementation work is tracked in [Issue #91 — Turn-Based Combat, UI Redesign and Arousal MVP](https://github.com/TN13007/rpg/issues/91).
