# Combat Balance Specification v1

**Status:** Prototype specification — provisional values, not final balance  
**Date:** 2026-11-20  
**Related design:** [Issue #11 — Long-term gameplay and system design](https://github.com/TN13007/rpg/issues/11)

## 1. Purpose

This document defines a first testable model for character attributes, progression, damage, HP/resources, Recovery, and Stagger. Its purpose is to make combat balance measurable and adjustable while preserving distinct character builds and encounter identities.

The model is based on these accepted design decisions:

- Combat supports multiple allies and enemies.
- Turn scheduling uses a Timeline / ATB model.
- Each combatant becomes eligible according to its `NextActionTime`.
- Actions execute immediately after validation and selection.
- Recovery Time determines when the combatant can act again.
- Cooldown is time-based and independent of Recovery Time.
- Combat simulation time pauses while waiting for the Player to choose an action.
- No battlefield positions, formation, universal Cast Time, or interruption system in v1.
- Combat outcomes are separate from their consequences.
- Resource persistence after combat is hybrid.
- Damage has both sustained and burst sources.
- Encounter duration varies by enemy and encounter design; there is no universal required number of Combat Cycles.

## 2. Design principles

1. **Design from player experience, then derive numbers.** Balance around action value, damage per Combat Cycle, time to kill, resource pressure, and encounter behavior—not isolated Attack or HP values.
2. **Every attribute needs a gameplay responsibility.** Avoid attributes that improve too many unrelated systems.
3. **Attributes, equipment, and skills contribute differently.** Attributes establish character capability; equipment shapes build and scaling; skills determine tactical options and action efficiency.
4. **Sustained damage must matter.** Players should make progress during ordinary engagement, not only during a Stagger window.
5. **Burst damage rewards opportunity creation.** Stagger and enemy-specific mechanics should create meaningful punish windows.
6. **Stagger is not identical to Vulnerability.** A target can be unable to act without necessarily taking extra damage, and can have a weakness without being Staggered.
7. **Prototype values are data, not constants embedded in code.** Keep tunable values in definitions/configuration.
8. **Do not generalize before a playable encounter proves the need.**

## 3. Combat time units

The following terms must remain distinct:

- **Action:** One selected combat action.
- **Combo / Burst:** A sequence of actions performed during an opportunity window.
- **Combat Cycle:** A broader exchange of engagement, defense, opportunity creation, punish/burst, and re-engagement.

A Combat Cycle is not a Timeline turn. In a Souls-like example, the player may perform several actions during a boss's Stagger window, then return to an engagement phase. Damage per Combat Cycle is a useful balancing metric, but the cycle is a design/analysis unit rather than a mandatory runtime turn.

## 4. Character progression

Character power comes from three complementary sources:

| Source | Responsibility | Typical contribution |
|---|---|---|
| Attributes | Character's underlying capabilities | HP, resources, weapon/magic scaling, Recovery |
| Equipment | Build specialization | Weapon Power, scaling profile, armor/resistance, modifiers |
| Skills | Tactical expression | New actions, multipliers, costs, effects, Posture pressure |

Do not make all three sources grant generic Attack increases. Their contributions should create different decisions.

### 4.1 Hybrid attribute growth

Attributes can grow through two channels:

- **Level-up allocation:** the primary, player-controlled method. The player receives points and chooses where to invest them.
- **Activity-based growth:** a secondary, bounded method tied to meaningful training, milestones, or authored progression.

Activity growth must not be infinitely farmable by repeating a low-cost action. Use explicit caps, milestones, training opportunities, or diminishing/eligibility rules.

Keep **Attribute** separate from **Mastery**:
- Attribute is a broad character capability and participates in shared calculations.
- Mastery represents proficiency with a weapon family or specific technique and affects only relevant actions.

Mastery is a proposed extension, not required for the first damage prototype.

## 5. Base attributes

Initial attribute set:

| Attribute | Primary responsibility | Explicitly not granted by default |
|---|---|---|
| Strength | Physical weapon/action scaling | HP, Critical Chance |
| Intelligence | Magic scaling | Magic Resistance |
| Agility | Recovery Time modifier | Evasion, Critical Chance |
| Endurance | Max HP and Max Stamina | Damage |
| Luck | Critical Chance and selected luck-specific mechanics | Universal probability bonuses |

Reference scale: 0–100 for design/data compatibility. Initial prototype tests should focus around values 10–30, with 20 as the reference character. The 0–100 range is not yet a final cap or progression promise.

### 5.1 Attribute formulas — provisional

```text
MaxHP      = BaseHP + Endurance * HPPerEndurance
MaxStamina = BaseStamina + Endurance * StaminaPerEndurance
MaxMana    = BaseMana + Intelligence * ManaPerIntelligence
```

Prototype values:

| Derived value | Base | Per attribute point | Example at attribute 20 |
|---|---:|---:|---:|
| Max HP | 100 | 10 per Endurance | 300 at Endurance 20 |
| Max Stamina | 50 | 5 per Endurance | 150 at Endurance 20 |
| Max Mana | 50 | 5 per Intelligence | 150 at Intelligence 20 |

These are test values only. Revisit the HP scale if damage and enemy HP do not scale coherently.

Agility controls Recovery Time through a bounded modifier:

```text
AgilityModifier = Clamp(0.8, 1.2, 1 + (Agility - 20) / 100)
RecoveryTime    = BaseRecoveryTime / AgilityModifier
```

Illustration for a 2.0-second base recovery:

| Agility | Modifier | Recovery |
|---:|---:|---:|
| 0 | 0.8 | 2.50 s |
| 10 | 0.9 | 2.22 s |
| 20 | 1.0 | 2.00 s |
| 30 | 1.1 | 1.82 s |
| 40+ | 1.2 | 1.67 s |

This deliberately limits Agility's action-frequency advantage. Agility does not automatically increase Evasion in v1.

Luck's provisional Critical Chance formula:

```text
CriticalChance = Clamp(5%, 35%, 5% + Luck * 0.2%)
CriticalDamage = 150% of pre-mitigation damage
```

Critical Damage does not scale with Luck in this prototype. Luck must not become a universal modifier for every random outcome.

## 6. Damage model

Damage is calculated from the action, attack source, target defenses, and current target state.

### 6.1 Attack Power

Physical action:

```text
AttackPower = WeaponPower + Strength * WeaponScaling
RawDamage   = AttackPower * SkillMultiplier
```

Magic action:

```text
AttackPower = MagicPower + Intelligence * MagicScaling
RawDamage   = AttackPower * SkillMultiplier
```

For an action that does not use a weapon, its definition may provide a BasePower instead of WeaponPower/MagicPower.

The scaling coefficient belongs to the weapon or action definition. Different weapons may have different Strength scaling; do not assume one universal coefficient.

### 6.2 Damage pipeline

Apply operations in this order:

1. Validate action and target.
2. Calculate Attack Power.
3. Apply Skill Multiplier and flat action-specific additions, if any.
4. Apply Critical multiplier when a critical hit is confirmed.
5. Apply the target's relevant Resistance.
6. Apply Vulnerability or other target-state modifiers.
7. Apply shield/absorption.
8. Reduce target HP and produce a DamageResult.
9. Resolve separately defined Posture Damage and secondary effects.

Critical multiplier is applied before Resistance and Vulnerability. Posture Damage is a separate output and does not have to be proportional to HP Damage.

### 6.3 Resistance

Use separate Physical and Magic Resistance values. Provisional mitigation:

```text
ResistanceModifier = K / (K + Resistance)
MitigatedDamage    = RawDamage * ResistanceModifier
```

Prototype K = 100:

| Resistance | Damage received |
|---:|---:|
| 0 | 100% |
| 25 | 80% |
| 50 | 66.7% |
| 100 | 50% |
| 200 | 33.3% |

K is a tuning parameter, not a universal truth. Do not allow negative Resistance or immunity semantics to emerge accidentally; define those rules explicitly if a future encounter needs them.

### 6.4 Sustained and Burst damage

```text
TotalEncounterDamage = SustainedDamage + BurstDamage
BurstRatio = BurstDamage / TotalEncounterDamage
DamagePerCycle = Damage dealt across one defined Combat Cycle
```

- **Sustained Damage:** damage dealt during ordinary engagement and smaller opportunities.
- **Burst Damage:** damage dealt during a high-value window such as Stagger or an exposed weakness.

Prototype test target: in encounters with regular Stagger opportunities, try a Burst contribution around 35–50% of total player damage. This is an experiment range, not a universal balance target. If Burst dominates too much, ordinary engagement feels like waiting; if it contributes too little, creating an opportunity feels unrewarding.

## 7. Skills and action prototypes

Initial physical action values for a reference weapon:

| Action | Skill Multiplier | Base Recovery | Stamina Cost |
|---|---:|---:|---:|
| Quick Attack | 0.8 | 1.0 s | 0 |
| Heavy Attack | 1.5 | 2.0 s | 15 |
| Basic Skill | 1.2 | 1.5 s | 0 or action-specific |
| Powerful Skill | 2.0 | 2.5 s | 25 |

These values are only for initial comparisons. Cooldowns are defined independently per skill and are not included in Recovery Time.

Example resource costs:

| Action | Stamina |
|---|---:|
| Dodge | 20 |
| Parry | 10 |
| Heavy Attack | 15 |
| Powerful physical skill | 25 |

Example magic costs:

| Spell tier | Mana |
|---|---:|
| Basic | 15 |
| Advanced | 30 |
| Powerful | 45 |

Basic actions should not all consume resources by default. Resource regeneration rates and out-of-combat recovery are still open balance decisions.

## 8. Stagger model

Use a shared Posture foundation plus encounter-specific opportunity mechanics.

### 8.1 Shared Posture state

A combatant may have:

- CurrentPosture
- MaxPosture
- PostureRecoveryRate
- PostureRecoveryDelay
- StaggerDuration

When Posture Damage is applied:

```text
CurrentPosture = Max(0, CurrentPosture - PostureDamage)
```

At zero Posture, the target enters Stagger if its encounter profile allows it.

Prototype starting values:

| Action | Posture Damage |
|---|---:|
| Quick Attack | 8 |
| Heavy Attack | 18 |
| Basic Skill | 12 |
| Powerful Skill | 20 |
| Successful Parry | 22 |

Prototype recovery:
- Recovery delay after the last Posture Damage: 3 seconds.
- Posture recovery rate: 8 points/second.
- Stagger duration: 3 seconds.

The exact behavior during Stagger, including whether Posture resets, partially restores, or becomes temporarily locked, must be explicitly implemented and tested. Do not infer it from the duration alone.

### 8.2 Encounter-specific mechanics

Examples:
- Knight: successful parry applies a large Posture hit.
- Golem: breaking armor changes its Posture profile or opens a weakness.
- Beast: attacking a specific exposed point after evasion creates an opportunity.
- Mage: interrupting a defined special action creates an opportunity.

These mechanics should feed the shared Posture/opportunity model where practical. A boss may require a specific condition before ordinary Posture Damage can Stagger it.

### 8.3 Stagger and Vulnerability are separate

Provisional target-state modifiers:

| Target state | Vulnerability modifier |
|---|---:|
| Normal | 1.0 |
| Vulnerable | 1.25 |
| Staggered | 1.5 |

These modifiers are examples for testing. Stagger means the target cannot act for its defined duration; Vulnerability means it receives increased damage. A target may have either state independently.

## 9. Enemy HP and encounter pacing

Do not force all enemies or bosses to use the Player's HP formula. Author enemy HP, Resistance, Posture, behavior, and opportunity rules according to encounter role and expected experience.

Initial enemy profile data for the reference character:

| Tier | HP | Resistance | Max Posture |
|---|---:|---:|---:|
| Normal | 60 | 0 | 30 |
| Elite | 180 | 15 | 70 |
| Mini-boss | 350 | 20 | 100 |
| Boss | 600 | 25 | 100 |

These are test fixtures, not progression formulas or final content balance.

Example reference character:
- Strength 20
- Weapon Power 12
- Weapon Scaling 0.8
- Attack Power = 12 + 20 × 0.8 = 28

Before Resistance:
- Quick Attack: 28 × 0.8 = 22.4
- Heavy Attack: 28 × 1.5 = 42
- Basic Skill: 28 × 1.2 = 33.6
- Powerful Skill: 28 × 2.0 = 56

Against Resistance 25, multiply by 100/125 = 0.8. Against a Staggered target with Vulnerability 1.5, multiply the mitigated result by 1.5.

Encounter duration is variable. Tune it using:
- Sustained DPS
- Burst Damage and Burst Ratio
- Damage per Combat Cycle
- Time to Kill
- Stagger frequency and opportunity duration
- Player mistakes, defensive downtime, and resource availability
- Enemy phase changes and behavior

A Combat Cycle is an analysis unit, not a fixed number of Timeline turns. Do not require every boss to die in three, four, or five cycles.

## 10. Prototype encounter identities

Initial examples:
- **Aggressive Knight:** frequent pressure; parry is an effective way to build Posture; large punish window.
- **Ancient Golem:** high defense; ordinary attacks build Posture slowly; breaking armor opens a more effective damage opportunity.

The bosses should differ in behavior and opportunity creation, not merely in HP and Resistance.

## 11. Resource persistence

Keep the previously accepted hybrid persistence policy:
- HP is not automatically restored to full after every encounter.
- Stamina may recover faster than HP.
- Mana has its own recovery rules.
- Some effects may persist beyond combat when their design requires it.
- Items, rest, or specific locations may restore resources.

Exact recovery rates and rules remain open until the first complete encounter loop is playable.

## 12. Balance telemetry and acceptance tests

Track at least:

| Metric | Purpose |
|---|---|
| Sustained DPS | Measures ordinary engagement effectiveness |
| Burst Damage | Measures punish-window reward |
| Burst Ratio | Detects over-reliance on Stagger |
| Damage per Combat Cycle | Measures progress across an exchange |
| Time to Kill | Measures encounter duration |
| Stagger Frequency | Measures opportunity creation |
| Recovery Downtime | Measures action cadence |
| Resource Efficiency | Measures damage/effectiveness per resource |
| Damage Taken per Cycle | Measures defensive pressure |

Prototype acceptance:
- Two builds with different attribute investments demonstrate different combat strengths.
- Physical and magic scaling use their intended attributes.
- Agility improves action cadence without unbounded speed growth.
- Endurance improves survivability/resource capacity without directly increasing damage.
- Luck affects Critical Chance only as defined; it does not modify every random system.
- Sustained attacks make meaningful progress outside Stagger windows.
- Creating a Stagger opportunity provides a noticeable reward.
- Two enemy profiles create different opportunity patterns.
- Cooldown and Recovery Time advance independently on combat simulation time.
- Damage, Posture, critical, resistance, vulnerability, shield, and resource calculations have unit tests for normal, boundary, and invalid cases.
- A manual runtime encounter verifies that the combat loop is understandable and satisfying.

## 13. Deferred and unresolved

Do not finalize these without prototype evidence:
- Final attribute caps, point budget, and level-up curve.
- Activity-growth rate, limits, and exact training rules.
- Mastery system implementation.
- Weapon-specific scaling tables.
- Final Skill Multipliers and damage coefficients.
- Resistance K and resistance caps.
- Critical chance caps and any future Critical Damage scaling.
- Stamina/Mana regeneration and post-combat recovery rates.
- Exact Posture reset/lock behavior after Stagger.
- Boss-specific Stagger exceptions and phase transitions.
- Final HP values and target encounter duration.
- Evasion/Accuracy model.
- Advanced combo, interruption, casting, positional, and formation mechanics.

## 14. Implementation boundary

This specification does not authorize implementing every listed system at once. Build the smallest playable combat slice first:
1. One Player combatant and one enemy.
2. Attributes, one weapon, and two actions (Quick and Heavy).
3. Damage pipeline, HP, Resistance, Recovery, and independent cooldown.
4. Sustained combat loop and one Stagger opportunity.
5. One encounter-specific mechanic.
6. Unit tests and manual runtime verification.
7. Only then expand to multiple builds, enemy profiles, resources, and additional skills.

The implementation should preserve the existing Timeline/ATB and action lifecycle decisions. Do not add universal Cast Time, interruption, positions, or formation to v1 as part of this work.
