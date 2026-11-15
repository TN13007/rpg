# Automated Gameplay Tests

## Run

From the repository root:

```powershell
dotnet test .\RPG.Tests\RPG.Tests.csproj
```

The test project targets .NET 8 and references the Godot project. It tests domain/application logic without launching the game window.

## Coverage

- Character state combinations: every Health value from 0 to MaxHealth, every Energy value from 0 to MaxEnergy, and every Mood value from 0 to MaxMood.
- Status precedence: Health = 0 means Dead; Energy = 0 with positive Health means Unconscious; a character with no Energy resource is not automatically unconscious.
- Resource boundaries: damage, healing, Energy spending/recovery, Mood changes and Gold addition.
- Attributes: every Strength, Agility, Intelligence, Endurance and Wisdom value from 0 to 100 in time-modifier calculations.
- Action execution: every configured location action across every Health/Energy combination, and across every Mood value.
- Action data: all 10 configured actions, their Energy costs, effects and durations; Travel modifier; separate combat encounter duration.
- Combat: Attack, Defend, Dodge, victory, reward, enemy response, invalid targets, insufficient Energy, death, unconsciousness, and repeated actions after encounter completion.
- Combat boundary matrices: every player Health/Energy combination at encounter start, every Energy value for Defend/Dodge, and every positive Health value when receiving an enemy response.

## Gameplay rules currently asserted

1. Health = 0 is Death and takes priority over Unconsciousness.
2. Health > 0 and Energy = 0 is Unconscious.
3. An unconscious character cannot perform any action, including a zero-cost action, and cannot enter combat.
4. If Defend or Dodge spends the final Energy point, the encounter ends immediately. The enemy does not receive a response turn.
5. An Attack cannot be submitted at Energy = 0, even though the basic Attack has no Energy cost.
6. A valid action may spend the character's final Energy point. That action completes, then the character becomes unconscious and cannot take another action.

## Manual Godot checks

Automated tests do not launch or inspect the Godot UI. Manually verify:

- The Character Panel refreshes after Health, Energy, Mood and Gold changes.
- Entering a Goblin encounter with exactly 2 Energy consumes the encounter cost and collapses the player before Combat mode opens.
- Defend with exactly 1 Energy ends combat immediately without a Goblin attack.
- Dodge with exactly 2 Energy ends combat immediately without a Goblin attack.
- The combat result screen displays DEFEAT for unconsciousness and death, and VICTORY only when the enemy is defeated.
- Returning from a completed encounter returns to the same location.

## Current limitation

There is not yet a world-level recovery/consequence system for an unconscious character. The tests enforce that the character cannot continue acting at zero Energy, but the game still needs a future recovery flow (for example, being taken to the academy infirmary). This is intentionally not simulated by these unit tests.
