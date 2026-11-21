# Combat Design

**Status:** Core direction accepted; numeric balance and advanced edge cases remain provisional  
**Last updated:** 2026-11-20

## 1. Combat identity

Combat is a challenging, turn-based Tactical Timeline system. The player directly controls a four-character party and wins by manipulating action order, coordinating skills, managing resources and responding to enemy threats.

The intended fantasy is to create deliberate sequences in which one character delays or disables a threat, another protects an ally, and another exploits the opening to finish the target. The player should feel like a tactician controlling the flow of battle, not a spectator waiting for automated allies.

## 2. Accepted decisions

| Dimension | Decision |
|---|---|
| Combat style | Turn-based, Continuous Timeline |
| Difficulty | Challenging |
| Party size | Four active characters |
| Party control | Full control; player chooses every party member's action |
| Positioning | None; no front/back rows or tactical grid |
| Timeline | Shared time axis; each combatant has independent progress |
| Time while choosing | Combat Clock pauses while the player selects an action |
| Recovery | Each action defines its own Recovery Cost |
| Tie-break | Higher Luck acts first; stable ID breaks a remaining tie |
| Stun | Freezes Timeline progress; it does not consume or skip the target's pending progress |
| Information | Partial; invisible/hidden enemies are omitted from the Timeline until revealed |
| Resource persistence | Hybrid; selected resources persist while others recover through combat or rest |
| Defeat state | Characters become Downed and may be revived |
| World structure | University and city locations with optional paranormal incidents |

These are design decisions. They do not assert that the current code implements them.

## 3. Timeline model

The Timeline is a visualization of a shared Combat Clock and each combatant's independent progress. It answers **who is expected to act next and when**. It is not a fixed turn queue and does not advance only the currently selected character.

### 3.1 Progress and Speed

Each eligible combatant has:

- **Remaining Progress:** the amount of progress required before their next action.
- **Effective Speed:** the rate at which they complete that progress.
- **Ready:** a derived state reached when Remaining Progress reaches zero.

While Combat Time advances, every eligible combatant progresses simultaneously:

`RemainingProgress = max(0, RemainingProgress - EffectiveSpeed * ElapsedCombatTime)`

For a combatant that is not Stunned or otherwise prevented from progressing:

`TimeToReady = RemainingProgress / EffectiveSpeed`

The Combat Clock advances in real time at **1.0x** in the current implementation: one real-world second contributes one Combat second. The runtime supplies elapsed frame time to the simulation. Each frame's elapsed time is clamped at the earliest relevant event: a combatant becoming Ready or a timed combat effect reaching its scheduled time. This preserves event ordering without jumping over Ready events. There is no time-acceleration control in this slice. All eligible combatants progress over the same elapsed interval. Every active, non-Stunned combatant must have a strictly positive Effective Speed; Stun is represented as an eligibility/progress-freeze state, not by assigning zero Speed.

A combatant with zero Remaining Progress is Ready. Ready is not an action by itself; it means the combatant is eligible to be considered for the next action opportunity, subject to statuses and encounter state.

### 3.2 Initialization

At encounter start, each active combatant receives the same configured Initial Progress. Effective Speed determines who reaches Ready first. This makes Speed relevant from the opening of combat without granting a side an arbitrary first-turn advantage.

The Initial Progress value and all numeric tuning are provisional. They must be tuned in a playable encounter, not treated as permanent design constants.

The current pacing target is that the slowest active combatant takes no more than **4 seconds** to recover from the largest Recovery Cost available in the encounter. Progress values use one shared scale:

`ProgressScale = (4 seconds × SlowestBaseSpeed) / LargestRecoveryCost`

The same scale applies to Initial Progress, Recovery Cost and Delay so their relative timing remains consistent. Stun duration is already measured in Combat Time and is not scaled. This is a tuning target, not a player-facing timer or a limit on how long Stun can prevent an action.

### 3.3 Action recovery

Actions resolve instantaneously in the first Timeline implementation. Each action defines its own **Recovery Cost**, expressed in progress units rather than seconds.

After an action resolves, the acting combatant's Remaining Progress is set to that action's Recovery Cost. Their next action time is therefore determined by both Recovery Cost and current Effective Speed:

`TimeToNextAction = RecoveryCost / EffectiveSpeed`

A quick action can have a lower Recovery Cost; a heavy action can have a higher one. Other combatants retain their current progress and continue from where they were. Recovery is not a separate real-time animation duration and must not be counted twice.

**Resource Cost** is separate from Recovery Cost. Resource Cost spends a gameplay resource; Recovery Cost changes when the actor can act again.

Casting time, channeling and interruptible action execution are not part of the first implementation. If introduced later, they must be modeled as a distinct action phase rather than silently added to Recovery.

### 3.4 Player decision pause

When a player-controlled party member becomes Ready, the Combat Clock pauses and the player chooses an action. No combatant progresses and no Combat-Time-based effect advances while the player is deciding. There is no real-world decision timer.

After the action is confirmed and resolved, the actor receives its Recovery Cost. The Combat Clock resumes on the next runtime frame. Any other combatants already Ready at that same Combat Time are resolved before elapsed time advances again.

Enemy actions are selected and resolved by enemy behavior when an enemy becomes Ready. They do not require the player to wait through a real-time decision timer.

### 3.5 Multiple combatants Ready at the same time

When multiple combatants reach Ready at the same Combat Time, resolve them in this order:

1. Higher Luck acts first.
2. If Luck is equal, use stable combatant ID in ascending ordinal order.

There is no random roll in this tie-break rule and no separate Action Priority in the first implementation.

Resolve one action at a time. After each action, immediately check encounter terminal conditions; if the encounter has ended, stop processing the timestamp. Otherwise, re-evaluate the remaining Ready combatants against the updated combat state. This matters because the first action may Stun, Down, Delay or otherwise affect another combatant that was waiting at the same timestamp.

A combatant that remains Ready does not receive artificial extra Recovery merely because another combatant acted first.

## 4. Timeline control and timed effects

### 4.1 Speed modifiers

Speed determines the rate of progress, not the amount of progress required. Effects such as Slow and Haste modify Effective Speed:

`EffectiveSpeed = BaseSpeed * ActiveSpeedModifiers`

The exact modifier composition, stacking, caps and rounding are open balance/design details. The first implementation must define one deterministic policy and test it; it must not allow invalid negative or non-finite speeds.

A Speed change takes effect when the effect is applied or expires. It changes the rate from that point onward; it does not retroactively rewrite progress already completed.

### 4.2 Delay and acceleration

Delay and Acceleration directly modify Remaining Progress:

- **Delay** adds progress that the target must complete before becoming Ready.
- **Acceleration** removes progress, but Remaining Progress cannot become negative.
- Neither effect is automatically a Stun or an Interrupt.

Exact values and stacking rules are provisional. At least one meaningful action-order control mechanic must be demonstrated in the first playable encounter; the full catalogue is not required.

### 4.3 Stun — accepted freeze behavior

Stun prevents the target from gaining Timeline progress for the duration of the effect. Combat Time continues for everyone else, and Stun duration is measured in Combat Time.

- If the target is partway through progress, that progress is preserved while Stunned.
- When Stun expires, the target resumes from the preserved progress.
- If the target was already Ready when Stun was applied, it remains Ready but cannot act until Stun expires. It may then act without having to rebuild progress.
- Stun does not itself add Recovery or erase the target's progress.
- Stun is distinct from Delay, which changes the amount of progress remaining.

The exact Stun duration, resistance, refresh and stacking rules remain open. The first implementation must avoid indefinite or unbounded stun-lock behavior.

### 4.4 Effect durations and event ordering

Timed status durations use Combat Time, not the number of turns taken by a particular combatant. A duration is active on the half-open interval `[StartTime, EndTime)`: it is no longer active at its exact EndTime.

When the Combat Clock reaches an event timestamp, process the timestamp as one deterministic event batch:

1. Advance all eligible combatants' progress by the elapsed Combat Time.
2. Resolve periodic effects due at that timestamp.
3. Apply resulting HP changes and Downed/removal transitions.
4. Expire effects whose EndTime is this timestamp.
5. Re-evaluate readiness and action eligibility.
6. Resolve the next eligible Ready combatant using the tie-break rule.
7. Immediately check encounter victory, defeat and other terminal conditions. If the encounter has ended, stop processing this timestamp; otherwise re-evaluate readiness and continue with the next eligible Ready combatant.

This ordering means an effect that expires at the same timestamp as a Ready event is expired before the action eligibility check. Periodic damage due at that timestamp is applied before a character can act; a character reduced to zero HP cannot act afterward.

If an action or event creates a new effect at the current timestamp, resolve its immediate consequences before selecting the next Ready combatant.

## 5. Timeline information and visibility

Timeline simulation and Timeline presentation are separate responsibilities. Hiding a combatant from the UI never removes it from the scheduler or pauses its actual progress.

- Party members are shown with their actual Timeline state and expected timing.
- Detected enemies may be shown with their current Timeline state. Their action intent is a separate information layer and is not automatically revealed by showing their Timeline icon.
- An invisible/hidden enemy is omitted entirely from the Timeline. No placeholder, question mark or estimated position is shown.
- A hidden enemy continues progressing, can become Ready and can act normally.
- When an enemy is revealed, its icon is added using its actual current progress. It does not restart or catch up from the moment of reveal.

The exact detection/reveal rules and whether ordinary visible enemies' timing can be uncertain remain open. The first implementation should keep visibility filtering in the presentation layer and must not let UI knowledge alter combat simulation.

## 6. Action timing vocabulary

| Term | Design meaning |
|---|---|
| Combat Time | Shared elapsed time inside an encounter |
| Remaining Progress | Progress units a combatant still needs before becoming Ready |
| Speed | Rate of progress per unit of Combat Time |
| Initial Progress | Shared starting progress assigned at encounter start |
| Recovery Cost | Progress assigned to an actor after its action resolves |
| Resource Cost | Gameplay resource spent to use an action |
| Delay | Adds to a target's Remaining Progress |
| Acceleration | Reduces a target's Remaining Progress |
| Stun | Freezes progress and action eligibility for a duration |
| Slow / Haste | Modifies Effective Speed |
| Interrupt | Cancels a defined action phase that is currently in progress |
| Casting | A preparation phase before an action resolves |
| Cooldown | Restricts reuse of an action until its cooldown condition is met |

These terms are not synonyms. The first implementation uses Combat Time, Remaining Progress, Speed, Initial Progress, Recovery Cost, Resource Cost, Stun and at least one action-order control mechanic. Casting, Interrupt and Cooldown behavior are not required for the first Timeline slice.

## 7. Control-effect design constraints

Control should be powerful enough to enable combo play, but repeated control must not become a universal stun-lock strategy. Enemies should have distinct control susceptibility and encounter-specific responses. Bosses should not be universally immune to every form of control.

- Delay changes when a target can act; it does not automatically cancel the action.
- Stun freezes progress and action eligibility; it is not merely a larger Delay.
- Interrupt cancels a defined in-progress action; it is not implemented as a synonym for Stun.
- Counter creates a response under a defined defensive condition.
- Stagger / Break may create an opportunity window, but its relationship to Stun and vulnerability needs to be explicit.

The complete status catalogue and numeric balance are outside the first encounter's scope.

## 8. Party

All four party members participate in combat. The player selects actions for every member; companions do not autonomously spend resources or override the player's plan.

Character identity and build determine tactical contribution. Controller, Defender, Striker and Support are useful design lenses, not mandatory classes or fixed party slots. Characters may combine roles, and multiple party compositions should remain viable.

No spatial formation is used. Target restrictions, area effects, protection and range may exist as explicit skill rules, but must not silently depend on a hidden front/back row or grid position.

## 9. Enemy encounter design

Enemies should differ by tactical problem, not only HP and damage. Candidate archetypes include:

- **Skirmisher:** fast pressure against vulnerable party members.
- **Brute:** slow, high-impact attacks that create a visible response window.
- **Shaman:** buffs, healing or support actions that may be interrupted.
- **Guardian:** protects allies or punishes careless attacks.
- **Assassin:** threatens support or damage-dealing characters.

An encounter should combine behaviors to create competing priorities. There should usually be more than one viable response.

### 9.1 Committed enemy intent

At encounter start, each living enemy commits to one action and (when required) one living party target. The committed action and target remain fixed while that enemy recovers, while the player chooses an action, and while the enemy is affected by Stun. The enemy plans its next intent only after resolving its current action. If its committed target is no longer a valid target when the action resolves, the action is lost rather than silently retargeted.

The Timeline marker exposes the committed action and target in its tooltip. This is a readable preview of the next threat, not a second action-selection interface. Enemy behavior profiles may weight the actions already assigned to that enemy; they do not add actions or change action execution. The Unit 213 Unstable Presence uses a lightweight authored profile over its existing presence_* actions. No Academy encounter or additional enemy archetype is required.

## 10. Downed and revival

When HP reaches zero, a Party member enters **Downed** rather than immediately causing total party defeat. Enemies at zero HP are defeated in this first implementation.

Rules for the first playable encounter:

- A Downed Party member remains in the Party roster at zero HP but is removed from the Timeline and cannot take normal actions.
- Enemy actions cannot target a Downed Party member.
- Any active ally can use the built-in **Revive** action on a Downed ally. Revival costs 4 Stamina.
- Revival restores 25% of maximum HP, rounded up to at least 1 HP.
- Each Party member can be revived only once per encounter. A member who is Downed again cannot be revived a second time.
- The revived member re-enters the Timeline with a Recovery Cost equal to the largest authored Recovery Cost in the active encounter roster. The ally performing Revive pays that same Recovery Cost. This makes revival consequential without allowing the revived character to act immediately.
- If every Party member is Downed, the party loses the encounter immediately. No bleed-out timer is used in this slice.
- If the controlled protagonist is still at zero HP after an encounter ends, Exploration offers only **Request emergency rescue**. It advances Game Time by 180 minutes and restores 5 HP; normal Player, Location and Travel interactions become available again afterward. This is a provisional recovery fallback, separate from in-combat Revive, and it does not restore other Party members.

These are provisional encounter rules chosen to prove a finite, understandable revival loop. Downed targeting by special enemies, bleed-out/death and broader revival costs remain open for future encounters.

## 11. Resource persistence

The accepted direction is **hybrid persistence**. It should create decisions across an adventure without making every encounter feel like punishment for the previous one.

HP and selected expedition resources may persist between encounters. Other resources may recover through combat actions, milestones, consumables, rest or returning to campus or another safe location. Exact resource types, regeneration rates, post-combat recovery and checkpoint rules remain open.

Avoid adding overlapping resource bars unless each supports a distinct decision.

## 12. Difficulty and encounter outcomes

Challenging combat should come from readable but consequential threats, action-order trade-offs, recovery windows, resource and opportunity costs, distinct enemy behavior, control resistance, party protection and encounter objectives beyond maximum damage.

Do not create difficulty primarily through inflated HP, unavoidable damage or hidden rules.

An encounter may end through defeating required enemies, completing an objective, escaping where permitted, party defeat or a special encounter condition. Escape availability and costs remain open.

## 13. Validation before balance

A playable encounter must demonstrate:

- The Timeline is understandable and meaningfully changes after actions.
- Four active characters create interesting choices without excessive decision overhead.
- Different party members have reasons to act beyond repeating their highest-damage skill.
- Action-specific Recovery Cost and Speed produce predictable next-action timing.
- Equal Ready times are resolved deterministically by Luck and stable ID.
- Stun freezes progress and preserves it until the effect expires.
- At least one action-order control mechanic creates a meaningful tactical outcome.
- Hidden enemies are omitted from the Timeline without being removed from combat simulation.
- Enemies threaten the party without relying on opaque randomness.
- Downed and revival create tension without an endless revive loop.
- Resource persistence encourages expedition decisions without excessive attrition.

Do not finalize numeric balance before this slice is playable.

## 14. Explicitly open

The following are intentionally not finalized:

- Numeric values and units for Initial Progress, Base Speed, Recovery Cost and control effects.
- Exact Speed modifier stacking, caps and rounding.
- Delay / Acceleration stacking and resistance.
- Stun duration, resistance, refresh, stacking and anti-stun-lock rules.
- Detection, reveal and the precision of enemy timing information.
- Enemy intent / telegraph disclosure.
- Casting, channeling, Interrupt and Cooldown rules.
- Luck range, growth and whether any future effect modifies tie-break Luck.
- Bleed-out/death and special enemy rules for targeting Downed characters.
- Resource types and regeneration, escape rules, final party roles, character identities and skill lists.
- Whether Stagger is universal or encounter-specific.

These open questions must not block the first implementation unless the encounter directly requires one. Resolve them only when a concrete playable requirement makes the decision necessary.
