using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public sealed class CombatEnemyIntentTests
{
    [Fact]
    public void Unit213PresenceCommitsAnActionAndTargetAtEncounterStart()
    {
        Character player = TestCharacterFactory.CreatePlayer();
        Character presence = TestCharacterFactory.CreateEnemy();
        CombatService combat = new CombatService(
            new[]
            {
                new CombatantDefinition(player, CombatantSide.Party, TestCharacterFactory.CreatePlayerCombatActions()),
                new CombatantDefinition(presence, CombatantSide.Enemy, TestCharacterFactory.CreateEnemyCombatActions())
            },
            new Random(7));

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);

        TimelineCombatantStatus intent = Assert.Single(
            combat.GetSnapshot().Timeline,
            entry => entry.Id == presence.Id);

        Assert.False(string.IsNullOrWhiteSpace(intent.IntentDescription));
        Assert.Contains(player.Name, intent.IntentDescription);
        Assert.NotNull(combat.GetSnapshot().Intent);
        Assert.Equal(presence.Id, combat.GetSnapshot().Intent.EnemyId);
        Assert.Equal(intent.IntentDescription, combat.GetSnapshot().Intent.Description);
        Assert.Equal(IntentConfidence.Recognized, combat.GetSnapshot().Intent.Confidence);
    }

    [Fact]
    public void UnstablePresenceProfileCanDeterministicallyCommitItsWeightedAction()
    {
        Character player = TestCharacterFactory.CreatePlayer();
        Character presence = TestCharacterFactory.CreateEnemy();
        CombatActionDefinition[] actions = TestCharacterFactory.CreateEnemyCombatActions();
        EnemyBehaviorProfile profile = new EnemyBehaviorProfile(new Dictionary<string, int>
        {
            ["presence_quick_jab"] = 1,
            ["presence_heavy_swing"] = 0,
            ["presence_staggering_blow"] = 0
        });
        CombatService combat = new CombatService(
            new[]
            {
                new CombatantDefinition(player, CombatantSide.Party, TestCharacterFactory.CreatePlayerCombatActions()),
                new CombatantDefinition(presence, CombatantSide.Enemy, actions, behaviorProfile: profile)
            },
            new Random(11));

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);

        TimelineCombatantStatus intent = Assert.Single(
            combat.GetSnapshot().Timeline,
            entry => entry.Id == presence.Id);

        Assert.StartsWith("Quick Lunge", intent.IntentDescription);
        Assert.Contains(player.Name, intent.IntentDescription);
    }
}
