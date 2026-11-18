# Automated Gameplay Tests

## Run

From the repository root:

```powershell
dotnet test .\RPG.Tests\RPG.Tests.csproj --configuration Release
```

The test project targets .NET 8 and tests domain/application logic without launching the Godot window.

## Coverage

The automated suite exercises:

- Character state, Health/death, Fatigue accumulation/recovery, Stamina boundaries, Mood and Gold.
- Activity Fatigue calculation from relative MET and actual duration, including fractional values and boundary inputs.
- Action definition loading, authored activity intensity, duration modifiers, effects and time advancement.
- Rest and Sleep recovery, resource clamping and failure behavior for dead characters.
- Combat Attack, Defend, Dodge, Catch Breath, enemy response, insufficient Stamina, victory, death and invalid requests.

Use current test output and CI runs as evidence; do not preserve stale hard-coded test counts in this document.

## Gameplay rules currently asserted

1. Health = 0 means Dead and blocks gameplay actions.
2. Fatigue is a cumulative daily-life exertion meter from 0–100. It does not cause unconsciousness or block actions in this initial version.
3. Fatigue is added using `max(0, activityMet - 1.0) * actualDurationMinutes / 15`. The calculation uses actual duration after any stat-based time modifier.
4. A five-minute activity at 2.8 MET adds approximately 0.6 Fatigue; a 30-minute activity at 2.8 MET adds approximately 3.6.
5. Rest takes 30 minutes, restores 5 Fatigue, restores 3 Stamina and adds 2 Mood.
6. Sleep restores Fatigue to 0 and Stamina to maximum, and retains its configured Health/Mood recovery and NextDay transition.
7. Stamina is a separate 0–10 Combat resource. Defend costs 1; Dodge costs 2; Attack has no Stamina cost.
8. Insufficient Stamina disables/rejects only the requested defensive action. It does not make the character unconscious or end Combat.
9. Catch Breath restores up to 3 Stamina and the enemy still resolves its response turn.
10. Completing the configured Combat encounter adds its activity-based Fatigue once. Merely selecting the encounter entry action does not apply the Combat duration's Fatigue.

## Manual Godot checks

Automated tests do not launch or inspect the rendered Godot UI. Run the main scene in Godot 4.7.2 .NET and manually verify:

- The Character Panel shows Health, Fatigue, Stamina, Mood and Gold and refreshes after changes.
- A five-minute walk at 2.8 MET adds approximately 0.6 Fatigue; Travel does not spend Stamina.
- High Fatigue does not block ordinary Location Actions or Travel.
- Rest takes 30 minutes, reduces Fatigue by 5, restores up to 3 Stamina and adds 2 Mood.
- Sleep advances to the next morning and restores Fatigue/Stamina plus its configured Health/Mood values exactly once.
- Combat can start at 0 Stamina. Attack and Catch Breath are available.
- Defend and Dodge show their Stamina costs and are unavailable only when Stamina is insufficient.
- Catch Breath restores up to 3 Stamina and the enemy still attacks.
- Defend/Dodge at the last available Stamina do not end the encounter.
- Death at Health = 0 still ends Combat and prevents further actions.
- Returning from a completed encounter returns to the same Location and the 30-minute Combat Fatigue is applied once.

Record the tested branch/commit and attach the relevant Godot Output, screenshots or recording. A code-level unit test is not a substitute for a UI/runtime check.

## Current limitations and verification boundaries

- Fatigue penalties, exhaustion thresholds, unconsciousness and fatigue-based action blocking are intentionally deferred until the initial accumulation/recovery curve has been playtested.
- Activity MET values and the conversion scale are initial balancing parameters, not a precise physiological prediction.
- Combat Stamina currently has only Defend, Dodge and Catch Breath as resource interactions. Future skills/actions may require additional costs and recovery rules.
- This test project does not launch the Godot scene, inspect rendered controls or validate mouse/input wiring.
