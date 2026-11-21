using System;
using System.Collections.Generic;
using Xunit;

public sealed class CombatEnemyBehaviorProfileTests
{
    [Fact]
    public void BruteProfileCommitsToItsHeavyAttack()
    {
        CombatSnapshot snapshot = StartProfiledEnemy(
            new Dictionary<string, int>
            {
                ["quick_jab"] = 0,
                ["heavy_swing"] = 1,
                ["staggering_blow"] = 0
            });

        Assert.Equal("Heavy Swing → Test Party", Assert.Single(
            snapshot.Timeline, entry => entry.IsEnemy).IntentDescription);
    }

    [Fact]
    public void DisruptorProfileCommitsToItsControlAction()
    {
        CombatSnapshot snapshot = StartProfiledEnemy(
            new Dictionary<string, int>
            {
                ["quick_jab"] = 0,
                ["heavy_swing"] = 0,
                ["staggering_blow"] = 1
            });

        Assert.Equal("Staggering Blow → Test Party", Assert.Single(
            snapshot.Timeline, entry => entry.IsEnemy).IntentDescription);
    }

    [Fact]
    public void SkirmisherProfileCommitsToItsFastAttack()
    {
        CombatSnapshot snapshot = StartProfiledEnemy(
            new Dictionary<string, int>
            {
                ["quick_jab"] = 1,
                ["heavy_swing"] = 0,
                ["staggering_blow"] = 0
            });

        Assert.Equal("Quick Jab → Test Party", Assert.Single(
            snapshot.Timeline, entry => entry.IsEnemy).IntentDescription);
    }

    private static CombatSnapshot StartProfiledEnemy(IDictionary<string, int> weights)
    {
        Character party = CreateCharacter("profile-party", "Test Party");
        Character enemy = CreateCharacter("profile-enemy", "Test Enemy");
        CombatActionDefinition[] actions =
        {
            CreateAction("quick_jab", "Quick Jab", CombatActionBehavior.Attack, 4, 75),
            CreateAction("heavy_swing", "Heavy Swing", CombatActionBehavior.Attack, 12, 130),
            CreateAction("staggering_blow", "Staggering Blow", CombatActionBehavior.Stun, 3, 115, stunDuration: 0.5)
        };

        CombatService combat = new CombatService(
            new[]
            {
                new CombatantDefinition(
                    party,
                    CombatantSide.Party,
                    new[] { CreateAction("party_strike", "Party Strike", CombatActionBehavior.Attack, 8, 70) }),
                new CombatantDefinition(
                    enemy,
                    CombatantSide.Enemy,
                    actions,
                    behaviorProfile: new EnemyBehaviorProfile(weights))
            },
            new Random(17));

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);
        return combat.GetSnapshot();
    }

    private static CombatActionDefinition CreateAction(
        string id,
        string label,
        CombatActionBehavior behavior,
        int damage,
        double recoveryCost,
        double stunDuration = 0)
    {
        return new CombatActionDefinition(
            id,
            label,
            "Enemy action",
            label,
            behavior,
            0,
            damage,
            0,
            string.Empty,
            "The enemy uses " + label + ".",
            recoveryCost,
            stunDuration: stunDuration);
    }

    private static Character CreateCharacter(string id, string name)
    {
        CharacterData data = new CharacterData(
            id,
            name,
            "Enemy behavior profile test combatant",
            1,
            100,
            100,
            10,
            100,
            100,
            0,
            10,
            50,
            0,
            new CharacterStats(20, 8, 20, 20, 20, 5));

        return new Character(data);
    }
}
