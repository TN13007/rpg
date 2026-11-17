# Automated Gameplay Tests

## Run

From the repository root:

```powershell
dotnet test .\RPG.Tests\RPG.Tests.csproj --configuration Release
```

The test project targets .NET 8 and references the Godot project. It tests domain/application logic without launching the game window.

## Coverage

The automated suite currently exercises:

- Character state combinations and status precedence across Health, Energy and Mood values.
- Resource boundaries: damage, healing, Energy spending/recovery, Mood changes and Gold addition.
- Attribute-based activity time calculations.
- Action definition loading/validation and configured Location/Self Actions.
- Action execution, failure paths, state mutation and duration/time advancement.
- DayClock rollover and next-day behavior where covered by the relevant tests.
- Combat: Attack, Defend, Dodge, victory, reward, enemy response, invalid targets, insufficient Energy, death, unconsciousness and repeated actions after encounter completion.
- Boundary matrices for Health/Energy combinations and combat resource spending.

The exact test count changes as coverage grows. Use current test output and CI runs as evidence; do not preserve a stale hard-coded action or test count in this document.

## Gameplay rules currently asserted

1. Health = 0 means Dead and takes priority over Unconsciousness.
2. Health > 0 and Energy = 0 means Unconscious.
3. A dead character cannot perform actions, including Rest.
4. An unconscious character cannot perform ordinary actions, Travel or enter Combat.
5. An unconscious character may perform an Action only when its Action Definition explicitly sets `canExecuteWhenUnconscious = true`.
6. The currently configured Rest Self Action sets that flag. It takes 30 minutes, costs 0 Energy, restores up to 3 Energy and adds 2 Mood. A living character at 0 Energy can therefore Rest and become able to act again.
7. The unconscious permission does not waive an Action's Energy cost; an action that still costs Energy is rejected when the character cannot pay it.
8. A valid action may spend the character's final Energy point. That action completes, then the character becomes unconscious and cannot perform another ordinary action.
9. Defend or Dodge spending the final Energy point ends the encounter immediately; the enemy does not receive a response turn.
10. Basic Attack is rejected at Energy = 0 even though its configured Energy cost is zero, because the unconscious state rule still applies.

## Manual Godot checks

Automated tests do not launch or inspect the Godot UI. Run the main scene in Godot 4.7.2 .NET and manually verify:

- The Character Panel refreshes after Health, Energy, Mood and Gold changes.
- A normal action or Travel that spends the final Energy point completes, then displays the collapse message; attempting another ordinary action is rejected.
- At 0 Energy with positive Health, Rest remains available and can be selected from the Exploration UI; verify 30 minutes elapse, Energy increases by up to 3 and Mood increases by 2.
- At Health = 0, Rest is rejected and does not advance time or change resources.
- Ordinary actions and Travel remain unavailable while unconscious.
- Entering a Goblin encounter with exactly 2 Energy consumes the encounter cost and collapses the Player before Combat mode opens.
- Defend with exactly 1 Energy ends combat immediately without a Goblin attack.
- Dodge with exactly 2 Energy ends combat immediately without a Goblin attack.
- The combat result screen displays DEFEAT for unconsciousness and death, and VICTORY only when the enemy is defeated.
- Returning from a completed encounter returns to the same Location.
- Sleep from the Dormitory advances to the next morning and applies its configured recovery Effects once. Session 4 owns the full calendar-boundary acceptance.

Record the tested branch/commit and attach the relevant Godot Output, screenshots or recording. A code-level unit test is not a substitute for a UI/runtime check.

## Current limitations and verification boundaries

- This test project does not launch the Godot scene, inspect rendered controls or validate mouse/input wiring.
- Rest provides the current short-term recovery path for a living unconscious Player. This does not imply that a larger infirmary, collapse consequence or world-level recovery system exists.
- PR #21 and PR #22 report passing GitHub Actions .NET tests, but both PR descriptions leave Godot runtime acceptance pending. Verify the current `main` build before recording those runtime checks as complete.
- A previous owner-marked playable-journey pass is documented in `Docs/QA/PlayableJourneyTestReport.md`; that record explicitly notes that raw logs/screenshots and per-action measurements were not retained.
