# Sprint — Playable Goblin Encounter

**Goal:** Deliver one complete player journey in the main game scene, not another isolated UI preview. The gameplay path and UI wiring have been implemented; Godot runtime verification remains pending.

## Player journey

```text
Academy Courtyard (Exploration)
        |
        | Investigate the rustling near the gate
        v
Goblin appears
        |
        v
Main Panel switches to Combat
        |
        | Player selects an action
        v
CombatService validates the request
        |
        +--> Invalid: return failure reason; do not mutate combat state
        |
        v
Resolve player action and apply its result
        |
        v
Goblin turn (if still alive)
        |
        v
Return updated CombatResult
        |
        v
Mapper -> CombatViewData -> CombatPanel
        |
        +--> Goblin HP > 0: next player turn
        |
        +--> Goblin HP = 0: victory + 25 G
                              |
                              v
                    Return to Exploration
                              |
                              v
                 Same Academy Courtyard
```

## Scope and rules

| Item | Sprint behavior |
|---|---|
| Starting location | Academy Courtyard |
| Encounter trigger | Investigate the rustling near the gate |
| Enemy | One Goblin, 40 HP |
| Player | 20 Health, 8 / 10 Energy, 70 Mood, 20 G |
| Attack | 0 Energy; deals 20 damage to Goblin |
| Defend | Costs 1 Energy; reduces the next Goblin hit to 1 damage |
| Dodge | Costs 2 Energy; avoids the next Goblin hit |
| Goblin turn | If alive after the player's action, it attacks for 4 damage |
| Victory | Goblin HP reaches 0 |
| Reward | 25 G, applied to the shared PlayerState |
| Return | Explicit Return to Exploration action; restore the same location view |

These values are prototype rules, not final balance.

## Responsibility boundary

- MainController handles the player journey and mode switching.
- CombatActionRequest carries the requested action and target.
- CombatService validates, resolves and applies combat changes.
- CombatResult returns success / failure, events and a presentation-safe snapshot.
- CombatPresentationMapper converts the result into CombatViewData.
- CombatPanel renders the current status, historical messages and current actions.
- CharacterPanel displays the persistent player state and refreshes after combat results.

The UI may disable actions based on the latest snapshot, but CombatService validates every request again. The UI never receives raw enemy AI state.

## UI requirements

- Left Sidebar: Character, Diary and System groups; no separate Explore tab.
- Main Panel: Exploration and Combat are mutually exclusive modes.
- Right Character Panel: persistent Health, Energy, Mood and Gold.
- Combat messages: Player aligned right, Enemy aligned left, System centered; all have visible speaker labels.
- Combat Status shows current enemy HP. Message Log records damage changes.
- Victory is shown before the player returns to the original Exploration location.

## Acceptance checklist

- [ ] Main scene opens in Academy Courtyard.
- [ ] The encounter choice switches the Main Panel to Combat without replacing Sidebar or Character Panel.
- [ ] Attack, Defend and Dodge are selectable.
- [ ] Invalid requests do not mutate combat state.
- [ ] Player and Goblin HP update correctly.
- [ ] Goblin acts after a player action if it survives.
- [ ] Defeating the Goblin ends combat and grants exactly 25 G once.
- [ ] Character Panel reflects Health, Energy and Gold changes.
- [ ] Return to Exploration restores the same location and its choices.
- [ ] Project builds and the full journey is verified in Godot 4.7.2 .NET.

## Not in scope

- Multiple enemies, random encounters, loot tables or inventory drops.
- Advanced status effects, skill trees, critical hits or final balance.
- Persistent save / load.
- Full responsive layout and screen-reader certification.
- Generalized quest, diary or reward frameworks.
