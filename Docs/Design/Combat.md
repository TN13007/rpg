# Combat Design

**Status:** Discrete shared-AP combat accepted; numerical progression and advanced effects remain provisional  
**Last updated:** 2026-11-22

## 1. Combat identity

Combat is a challenging, discrete turn-based tactical encounter. The player directly controls all four Party members and chooses each action. The tactical challenge comes from deciding which member should spend the Party's limited AP, selecting targets, using defensive/support actions at the right time and responding to deterministic enemy patterns.

Combat is a specialized optional interaction in the modern student-life RPG. It does not replace ordinary exploration, social interaction or non-combat responses to paranormal incidents.

The former Continuous Timeline, Combat Clock, Recovery Cost scheduling and committed Enemy Intent design is superseded. Those concepts must not be reintroduced under renamed fields or hidden behind the current UI.

## 2. Round and turn model

One Round consists of:

1. **Player Turn:** the player spends the Party's shared AP on eligible Party members, or explicitly ends the turn.
2. **Enemy Turn:** every living Enemy resolves at most one authored pattern action in stable encounter roster order.
3. **Next Round:** if the encounter remains active, the Round increments, Party action eligibility resets and the AP pool refreshes.

Enemy actions resolve synchronously as one ordered Domain result. The Presentation layer may later animate those ordered events, but animation timing does not determine combat rules.

## 3. Shared Party Action Points

The Party owns one shared AP pool. The first playable encounter begins with an authored capacity of **2 AP**. Capacity is bounded from 1 to 4 by the current Domain ActionPool.

- Each successful Party action costs 1 AP, including Revive.
- The acting Party member must be named explicitly in the request.
- Any living, eligible Party member may act regardless of AGI or Luck.
- Each Party member may perform at most one AP-funded action per Round.
- An action request is validated before AP or Stamina is spent. A rejected request does not spend AP or mark the actor as having acted.
- At 0 AP, the Enemy Turn starts automatically. The player may also end the Player Turn early; unused AP is discarded.
- AP refreshes at the start of the next Player Turn.

AGI and Luck remain as character data but do not determine initiative, turn order or AP in this combat slice. Persistent Action Capacity training/progression is deferred.

### 3.1 Enemy control penalty

An Enemy control effect can queue a one-point AP penalty for the next Player Turn.

- Multiple control sources before that Player Turn coalesce into one penalty.
- The penalty is consumed once when the next Player Turn begins.
- The resulting AP cannot fall below 1.
- The penalty does not stack into later turns or create an indefinite stun-lock.

## 4. Party action eligibility

A Party member can act when the encounter is in Player Turn, the member is alive, not Downed, has not already acted in the current Round, the Party has AP and the requested action's Stamina/target requirements are valid.

The roster always includes all four Party members. It communicates each member's current HP, condition and whether they can act. Selecting an eligible member filters the action list to that actor. After the selected member has acted, the UI moves focus to another eligible member.

## 5. Enemy patterns and target selection

Each Enemy follows an authored, ordered, repeating `EnemyPattern`.

- A living Enemy resolves at most one pattern step per Enemy Turn.
- Enemies are processed in stable encounter roster order.
- The target is selected at execution time from currently valid Party members.
- The UI does not display a future Enemy action or pre-committed target.
- Pattern progression, unavailable actions, insufficient Stamina and missing valid targets must remain deterministic.
- Downed Party members are not valid Enemy targets.

Enemy visibility affects presentation only. Hiding an Enemy from the player must not remove it from combat resolution.

## 6. Combat actions and meters

The first action set includes Attack, Guard, Evade, Recover Stamina, Control and Revive. Tease/Arousal/Orgasm is also an MVP requirement tracked separately in Issue #134.

HP and Arousal are separate meters:

- **Attack** reduces HP.
- **Tease** increases the target's Arousal by a fixed authored amount.
- Arousal starts at 0 and is capped at 100.
- Reaching 100 triggers exactly one Orgasm event for that threshold crossing, resets Arousal to 0 and applies the configured HP damage.
- Excess Arousal beyond the threshold is discarded; one action can trigger at most one Orgasm.
- HP reaching 0 incapacitates the target regardless of whether the HP loss came from Attack or Orgasm.

The exact Party-side consequence when Arousal reaches its threshold remains undecided. Do not assume that Party and Enemy consequences are identical. Body Profile progression, body-region attributes, exposure/accessibility and Enemy Preference remain deferred until the basic loop is playable.

## 7. Downed and revival

A Party member at zero HP becomes Downed, remains visible in the roster and cannot act or be targeted by ordinary Enemy actions.

- Revive costs 1 AP and 4 Stamina.
- Revive restores 25% of the target's Max HP, rounded up.
- Each Party member can be revived only once per encounter.
- If every Party member is Downed, the encounter ends in defeat.
- Enemy characters at zero HP are defeated.

Revival does not restore an additional action opportunity to the revived member in the Round in which they are revived.

## 8. Combat UI direction

The combat interface is organized around the character the player is currently directing.

**Screen composition**

- **Combat header:** encounter title, Round, current phase and shared AP.
- **Enemy area:** compact reusable Enemy cards. Each card shows identity, HP, condition and Arousal when the runtime supplies a real Arousal value. No Timeline or future Intent is displayed.
- **Party roster:** four compact cards remain visible. Each communicates HP, condition, action eligibility and relevant resources. Arousal is shown only when real data is available.
- **Active Party member panel:** visually focuses the selected actor. During combat it shows combat-relevant values such as HP and Stamina; it does not display the full Exploration profile, Fatigue, Mood or Gold.
- **Action area:** category filters, action list, selected-action details, cost, availability/disabled reason and an explicit confirmation step.
- **Combat log:** preserves the order of resolved events and identifies Player, Enemy and System messages.
- **End Player Turn:** remains available throughout the Player Turn.

**Interaction behavior**

- Eligible Party cards have clear hover, selected and can-act states. Already-acted and Downed members are visibly distinct and cannot be selected as the actor.
- When an action requires an Enemy target, valid targets receive a visible hover/available treatment and a pointing-hand cursor. Invalid targets are dimmed and do not accept selection.
- The chosen target receives clear selected feedback. Cancelling target selection clears target states and returns to the action details.
- Revive uses a separate ally-target selection state.
- Attack variants, Guard, Evade, Recover Stamina, Revive and Hit each receive a deliberate feedback cue. Messages and meter changes remain grounded in resolved events.
- Components render Presentation-owned view data and emit selection IDs/events. They do not calculate damage, spend AP/Stamina, decide target validity or mutate combat state.

The combat UI must remain readable with one Enemy, multiple Enemies, four Party members, long names and changing HP/Arousal/Stamina values.

## 9. Encounter lifecycle and world time

Combat remains behind the existing Exploration → Combat → Exploration flow. `CombatSessionService` coordinates entry and completion. GameClock does not advance for each combat action or Round; encounter completion applies the configured world-time and Fatigue consequences once.

Victory, defeat, escape and encounter objectives remain terminal outcomes owned by the combat/session rules, not by presentation.

## 10. Difficulty and balance

Difficulty remains **Challenging**. Difficulty should come from AP opportunity cost, readable enemy patterns, defensive timing, target priorities, Stamina management and consequences of Downed members—not inflated HP, opaque randomness or hidden scheduling rules.

The first playable Unit 213 encounter uses 2 AP. Numerical tuning and later capacity progression require playable evidence; they are not reasons to add a generic combat framework.

## 11. Deferred work

- Sequential event/animation playback beyond the current ordered result.
- Persistent Action Capacity training and progression.
- Party-side Orgasm consequence.
- Body Profile, body-region attributes, exposure/accessibility and Enemy Preference.
- Broad skill cooldown catalogue, elaborate status resistance/stacking and advanced Orgasm variants.
- Full responsive layout and assistive-technology verification.

These items must not block the current AP turn loop or the first usable character-focused combat interface.
