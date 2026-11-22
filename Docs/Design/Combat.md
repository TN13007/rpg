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
| Round structure | Player Turn followed by Enemy Turn |
| Party size | Four active characters |
| Party control | The player chooses every Party action; no autonomous Party AI |
| Action economy | One shared Action Point pool for the whole Party |
| Skill cost | Each skill use costs 1 AP in the first playable slice |
| Action Capacity | Party-level progression value; AP capacity is bounded to 1–4 in the first slice |
| Actor selection | Any active Party member can be selected for each action; the same member may act more than once in a Player Turn |
| Turn ending | Player may end the turn early; unused AP does not carry over |
| Enemy behavior | Authored deterministic action patterns evaluated against current conditions during Enemy Turn |
| Enemy actions | Each living enemy resolves one pattern action per Enemy Turn in the first slice |
| Initiative | No per-combatant initiative, Speed, Ready state or Luck tie-break scheduler |
| Positioning | No front/back rows or tactical grid |
| Combat time | No shared real-time Combat Clock; turn boundaries drive combat |
| Enemy information | No committed future Enemy Intent or Intent UI |
| HP | Reaching 0 HP incapacitates a Party member or defeats an Enemy |
| Arousal | Starts at 0, is capped at 100 and triggers the configured Orgasm consequence at the threshold |
| Defeat state | Party members become Downed and may be revived; all Party members Downed ends the encounter |
| World time | GameClock is not advanced per combat action or turn; any encounter time cost is applied once at session completion |

These are design decisions, not claims that the current runtime already implements them. Starting Action Capacity, training progression and numeric combat balance must be tested in a playable encounter.

## 3. Round and turn flow

A Round consists of two phases:

1. **Player Turn:** refresh the shared AP pool to the Party's current Action Capacity. The player chooses an available skill and an eligible Party member, resolves the action, and spends 1 AP. Repeat while AP remains, or end the turn early.
2. **Enemy Turn:** each living Enemy takes its turn in the encounter's authored deterministic order. It evaluates its action pattern against the current combat state and resolves one eligible pattern action.

If the encounter remains active after the Enemy Turn, begin the next Round and refresh the Party's AP. There is no real-time waiting between actions and no combatant independently becomes Ready.

Check victory, defeat and other terminal conditions immediately after every resolved action. If the encounter ends, stop the remaining Player or Enemy actions for that Round.

## 4. Shared Action Points

### 4.1 Action Capacity and the AP pool

Action Capacity determines how many skill uses the Party can make during one Player Turn. It is a single Party-level progression value, not four separate character action bars and not the sum of the four characters' Agility values.

At the beginning of each Player Turn:

- Set current AP to the Party's Action Capacity.
- The player may select any active Party member for each skill use.
- Every skill use consumes 1 AP in the first playable slice.
- AP is shared across the Party; using an action with one character reduces the same pool available to everyone else.
- The same character may be selected again while AP remains, provided the character and skill are still eligible.
- Current AP cannot exceed the Action Capacity. Unspent AP is discarded when the Player Turn ends and never carries into a later Round.

The first playable slice bounds Action Capacity to 1–4 AP, matching the four-person Party and limiting the number of decisions in one Player Turn. The initial capacity and the rate at which it grows remain provisional.

### 4.2 Action resolution and validation

The player chooses an actor, skill and required target. The owning combat service validates the request against the current encounter state before spending AP.

- A Downed, defeated or otherwise ineligible actor cannot act.
- A skill can be used only when its target requirements and other conditions are satisfied.
- Insufficient Stamina or another required resource rejects the action without spending AP.
- An invalid actor, skill or target does not spend AP and does not mutate combat state.
- After a successful action, spend 1 AP, resolve its effects and refresh the available choices.

AP is separate from Stamina, HP and Arousal. AP controls the number of skill uses in the current Player Turn; a skill may also spend a gameplay resource or have a turn-based cooldown. Do not recreate Recovery Cost or elapsed-time scheduling through a differently named resource.

### 4.3 Combo sequence

The player can distribute AP across the Party or concentrate several actions on one character. The system does not impose a default once-per-character-per-turn restriction in the first slice.

Example with 3 AP:

1. Alex uses Quick Strike (1 AP).
2. Mira uses Disrupting Strike (1 AP).
3. Alex uses another available skill (1 AP).
4. AP reaches 0, so the Enemy Turn begins.

The example is an action sequence, not a fixed script. Player choices, valid targets, resource costs, skill conditions and encounter state determine which combinations are possible. Playtests must check whether concentrating actions on one character dominates using the whole Party; do not add arbitrary actor locks before evidence shows they are needed.

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

Conditions may inspect current facts such as an Enemy's HP, a Party member's condition, whether an ally is Downed, or whether a target is valid. Conditions are evaluated at execution time, so an earlier Enemy action or a Player action can change what a later Enemy chooses.

Enemy targets are selected when the action executes. A pattern must not reveal or lock an upcoming target during the Player Turn. If no valid target exists, the action follows its authored fallback or skip behavior; the exact policy must be deterministic and tested.

Keep patterns content-driven and small. Do not introduce behavior trees, a general-purpose AI framework or a separate intent/telegraph scheduler for the first slice.

## 6. Skills, resources and cooldowns

Skill selection and action count are separate from skill effects:

- AP pays for using a skill during the current Player Turn.
- Stamina or another resource pays the skill's gameplay cost, if any.
- HP and Arousal are combat meters changed by skill effects.
- Cooldowns, if required in the first slice, are counted in completed Rounds rather than seconds.

A successful skill consumes AP and its configured resource cost. A rejected skill consumes neither. Numeric resource costs are authored and validated by the owning combat rules.

Do not add a broad status catalogue, casting phase, interrupt scheduler or timed effect engine unless a concrete encounter requires it. A control skill should have a clearly observable turn-based consequence, such as preventing a specified action, changing a condition or affecting a later pattern check.

## 7. HP and Arousal

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

## 10. Combat presentation

The combat interface should make the current decision obvious:

- Show the current Round and whether it is the Player Turn or Enemy Turn.
- During Player Turn, show one shared AP counter and the remaining AP clearly.
- Keep all four Party members visible with HP, Arousal and only resources that support a current decision.
- Allow the player to select any eligible Party member, then choose an available skill and target.
- Show Enemy cards with HP, Arousal and relevant current conditions.
- Show hover, available-target, selected-target and invalid-target states clearly.
- Present skill results, HP/Arousal changes, Downed/Revive and Orgasm as observable feedback.
- During Enemy Turn, show each resolved Enemy action as it happens; do not display a committed future action or target.

Remove Timeline tracks, Timeline markers, Time-to-Ready, Remaining Progress, Recovery Cost labels and Enemy Intent UI. UI components render supplied presentation data and emit requests; they do not own combat calculations or state mutation.

## 11. Encounter design and difficulty

Difficulty should come from the order and opportunity cost of the Party's limited AP, resource use, target priority, enemy patterns, readable conditions and encounter objectives.

An encounter should give the player meaningful choices about:

- Which Party member should act now.
- Which skill is worth spending an AP on.
- Whether to focus damage, control a threat, protect an ally or build Arousal.
- Whether to spend the remaining AP or end the Player Turn early.
- Which Enemy pattern condition must be disrupted before Enemy Turn.

Do not create difficulty primarily through inflated HP, unavoidable damage, opaque randomness or hidden action-economy exceptions. Action Capacity must create a useful progression choice without making ordinary encounters require repetitive training.

## 12. Validation before balance

A playable encounter must demonstrate:

- The player can understand the Player Turn / Enemy Turn boundary without a Timeline.
- The Party shares one AP pool; every successful skill use spends exactly 1 AP in the first slice.
- AP refreshes at the beginning of each Player Turn and unused AP does not carry over.
- The player can select any active Party member for each action, including selecting the same member again when AP remains.
- Different action sequences create meaningfully different outcomes and using several Party members remains viable.
- The player can end a Player Turn early and understands that the Enemy Turn will begin.
- Enemies resolve deterministic authored patterns in stable order and evaluate conditions against current state.
- Each living Enemy performs at most one pattern action per Enemy Turn in the first slice.
- Invalid actor, skill, target or insufficient-resource requests do not spend AP.
- Attack, Tease, Arousal threshold, Orgasm reset and Orgasm HP damage resolve correctly.
- Downed, Revive, Party defeat and encounter victory stop further actions at the correct point.
- GameClock is not advanced per combat action or turn; encounter completion cost is applied at most once.
- The UI communicates AP, active phase, Party state, Enemy meters and action results without Timeline or Intent.

Do not finalize numeric balance before this slice is playable.

## 13. Explicitly open

The following remain open and must be decided when a playable requirement needs them:

- Starting Action Capacity and the amount/frequency of gains from Training.
- Whether Action Capacity growth uses fixed thresholds, milestones or diminishing returns; avoid an unlimited grind loop.
- Whether a future skill may cost more than 1 AP; the first slice keeps every skill at 1 AP.
- Whether playtests justify a per-character action limit. The initial rule permits selecting the same active character more than once per Player Turn.
- Exact Enemy pattern cursor behavior when a condition or target is invalid, and the fallback action for each Enemy.
- Cooldown duration and the exact point at which completed Rounds decrement it.
- Turn-based replacements for any existing Stun/Delay skills that are still needed by the first encounter.
- Whether Party Arousal reaching 100 has a consequence, and what that consequence is.
- Numeric HP/Arousal values, resource costs, training costs and Orgasm damage.
- Escape rules, special encounter objectives and advanced Enemy actions.

Do not reintroduce Timeline, Combat Clock, Remaining Progress, Time-to-Ready or Recovery Cost as a workaround for an open question. Prefer the smallest deterministic rule that supports a visible player decision.

## 14. Implementation boundary

The current implementation documentation describes the Timeline-based runtime on main. That remains the implementation baseline until the refactor is merged. The design in this document describes the target behavior; do not update Implementation documentation to claim turn-based behavior exists before the code, tests and Godot runtime are migrated.

Implementation work is tracked in [Issue #91 — Turn-Based Combat, UI Redesign and Arousal MVP](https://github.com/TN13007/rpg/issues/91).