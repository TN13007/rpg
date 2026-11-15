# Sprint — Playable Academy Day and Goblin Encounter

**Goal:** Deliver a text-first academy day with location travel, timed activities, sleep and one deterministic Goblin encounter. Player and enemy are represented by domain Character objects. A fresh Godot build and runtime verification are still required for this revision.

## Player journey

```text
Dormitory — Day 1, 07:30
        |
        +--> Read, rest or sleep
        |
        v
Travel to Academy Courtyard
        |
        +--> Social and exploration activities
        |
        +--> Investigate the rustling
                    |
                    v
              Goblin encounter
                    |
          CombatService validates
          and resolves each action
                    |
          +---------+---------+
          |                   |
       Victory              Defeat
          |                   |
       +25 G              Player HP = 0
          |                   |
          +---------+---------+
                    |
             Return to location
                    |
          Continue the day or sleep
                    |
             Next morning, 07:30
```

## Scope and rules

| Item | Prototype behavior |
|---|---|
| Starting location | Dormitory |
| Starting time | Day 1, 07:30 |
| Locations | Dormitory, Courtyard, Classroom and Guild Hall |
| Travel | 10 base minutes; Agility can reduce the duration |
| Courtyard event | First arrival each day rolls once; 50% chance to reveal the Goblin event |
| Event resolution | Confront the Goblin (Combat mode) or sneak past it (10 minutes and 1 Energy) |
| Event repeat | The same event cannot be rolled again that day, whether it appears or not |
| Activities | Each location action declares its duration in academy content JSON |
| Study modifier | Intelligence can reduce the field-map study duration |
| Sleep | Available in the Dormitory; moves the clock to 07:30 on the next morning |
| Midnight | The clock rolls over automatically and carries remaining minutes into the next day |
| Enemy | One Goblin, 40 HP |
| Player | 20 Health, 8 / 10 Energy, 70 / 100 Mood, 20 G |
| Attack | 0 Energy; deals 20 damage to the enemy |
| Defend | Costs 1 Energy; reduces the next enemy hit to 1 damage |
| Dodge | Costs 2 Energy; avoids the next enemy hit |
| Enemy turn | If alive after the player's action, it attacks for 4 damage |
| Encounter duration | 30 minutes, configured in academy content |
| Victory | Enemy HP reaches 0; grants 25 G |
| Death | Player HP reaches 0; the encounter ends and the character cannot perform further actions |
| Return | Explicit Return to Exploration action; restore the same location view |

These values are prototype rules, not final balance. Sleep currently changes time only; character recovery and daily schedules are not implemented.

## Responsibility boundaries

- `CharacterData` stores immutable character configuration; `Character` owns runtime state and state-changing operations.
- `CombatService` validates combat requests, prevents dead characters from acting, resolves turns and grants the victory reward.
- `ActivityDefinition` owns the duration and optional stat modifier of an activity. `StoryChoice` owns the choice identity, label, feedback and activity definition.
- `ActivityTimeCalculator` calculates the final duration from one ActivityDefinition and the acting character.
- `DayClock` only advances calendar time and handles midnight rollover. It does not decide which gameplay action should consume time.
- `AcademyContent` loads location and activity definitions, then validates duplicate IDs and location exit references before publishing the loaded content.
- `MainController` coordinates UI modes and gameplay flow. It reads activity definitions and delegates duration calculation rather than owning each activity's feedback text or time formula.
- `CharacterViewModel` is a read-only presentation model; `CharacterPanel` does not access the domain Character.
- `CombatPresentationMapper` maps combat results into presentation messages and safe CombatViewData.

## UI requirements

- Left Sidebar: Character, Diary and System groups; no separate Explore tab.
- Main Panel: Exploration and Combat are mutually exclusive modes.
- Right Character Panel: persistent Health, Energy, Mood and Gold.
- Combat messages: Player aligned right, Enemy aligned left, System centered; all have visible speaker labels.
- Combat Status shows current enemy HP. Message Log records damage changes.
- Combat ends immediately when the enemy dies or the player's HP reaches zero.

## Acceptance checklist

- [ ] Main scene opens in the Dormitory at Day 1, 07:30.
- [ ] Travel only succeeds when the destination is listed in the current location's exits.
- [ ] Entering the Courtyard rolls the configured once-per-day Goblin event chance.
- [ ] A failed event roll does not reroll on repeated visits that same day.
- [ ] A triggered event offers both Combat and Stealth resolutions.
- [ ] Sneaking past the Goblin spends 1 Energy, advances 10 minutes and returns to the Courtyard.
- [ ] Confronting the Goblin enters Combat and returns to the same location after the encounter.
- [ ] Exploration activities and travel advance time using configured durations.
- [ ] Time modifiers use the configured character attribute.
- [ ] Advancing past midnight increments the day and preserves remaining minutes.
- [ ] Sleeping before midnight advances to the next day at 07:30.
- [ ] Sleeping after midnight but before 07:30 does not increment the day twice.
- [ ] Academy content rejects duplicate location IDs, duplicate action IDs and unknown exit references.
- [ ] A character with 0 HP cannot start an encounter or perform an action.
- [ ] Attack, Defend and Dodge resolve through CombatService.
- [ ] Invalid combat requests do not mutate character resources.
- [ ] Defeating the enemy grants exactly 25 G once.
- [ ] CharacterPanel receives CharacterViewModel, not Character.
- [ ] Return to Exploration restores the same location and its choices.
- [ ] Project builds and the full journey is verified in Godot 4.7.2 .NET.

## Not in scope

- Multiple enemies, additional random location events, loot tables or inventory drops.
- Advanced status effects, skill trees, critical hits or final balance.
- Persistent save / load.
- Character recovery, daily schedules and a complete death/rebirth or ghost-play flow.
- Full responsive layout and screen-reader certification.
