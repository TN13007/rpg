using System;
using System.Linq;
using Xunit;

public sealed class MixedEnemyEncounterTests
{
    [Fact]
    public void EncounterStartsWithFourPartyMembersAndThreeDistinctEnemyIntents()
    {
        CombatActionDefinition partyAction = CreateAction(
            "party_strike", "Party Strike", CombatActionBehavior.Attack, 8, 70);
        CombatActionDefinition[] goblinActions =
        {
            CreateAction("goblin_quick_jab", "Quick Jab", CombatActionBehavior.Attack, 4, 75),
            CreateAction("goblin_heavy_swing", "Heavy Swing", CombatActionBehavior.Attack, 8, 130),
            CreateAction("goblin_staggering_blow", "Staggering Blow", CombatActionBehavior.Stun, 3, 115, stunDuration: 0.5)
        };
        CombatActionDefinition[] bruteActions =
        {
            CreateAction("brute_crushing_slam", "Crushing Slam", CombatActionBehavior.Attack, 13, 155),
            CreateAction("brute_overhead_smash", "Overhead Smash", CombatActionBehavior.Attack, 17, 185)
        };
        CombatActionDefinition[] disruptorActions =
        {
            CreateAction("disruptor_staggering_blow", "Disrupting Pulse", CombatActionBehavior.Stun, 3, 130, stunDuration: 0.85),
            CreateAction("disruptor_quick_jab", "Quick Jab", CombatActionBehavior.Attack, 4, 80)
        };

        CombatantDefinition[] combatants =
        {
            CreatePartyMember("ari", "Ari", partyAction),
            CreatePartyMember("bram", "Bram", partyAction),
            CreatePartyMember("mira", "Mira", partyAction),
            CreatePartyMember("sera", "Sera", partyAction),
            CreateEnemy("goblin-a", "Goblin", goblinActions, EnemyBehaviorProfiles.CreateGoblinSkirmisher()),
            CreateEnemy("brute_a", "Brute", bruteActions, EnemyBehaviorProfiles.CreateBrute()),
            CreateEnemy("disruptor_a", "Disruptor", disruptorActions, EnemyBehaviorProfiles.CreateDisruptor())
        };

        CombatService combat = new CombatService(combatants, new Random(31));

        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);

        CombatSnapshot snapshot = combat.GetSnapshot();
        Assert.Equal(3, snapshot.Enemies.Length);
        Assert.Equal(7, snapshot.Timeline.Length);

        TimelineCombatantStatus[] enemyIntents = snapshot.Timeline
            .Where(entry => entry.IsEnemy)
            .ToArray();

        Assert.Equal(3, enemyIntents.Length);
        Assert.Contains(enemyIntents, entry =>
            entry.Id == "goblin-a" &&
            (entry.IntentDescription.StartsWith("Quick Jab", StringComparison.Ordinal) ||
             entry.IntentDescription.StartsWith("Heavy Swing", StringComparison.Ordinal) ||
             entry.IntentDescription.StartsWith("Staggering Blow", StringComparison.Ordinal)));
        Assert.Contains(enemyIntents, entry =>
            entry.Id == "brute_a" &&
            (entry.IntentDescription.StartsWith("Crushing Slam", StringComparison.Ordinal) ||
             entry.IntentDescription.StartsWith("Overhead Smash", StringComparison.Ordinal)));
        Assert.Contains(enemyIntents, entry =>
            entry.Id == "disruptor_a" &&
            (entry.IntentDescription.StartsWith("Disrupting Pulse", StringComparison.Ordinal) ||
             entry.IntentDescription.StartsWith("Quick Jab", StringComparison.Ordinal)));
    }

    private static CombatantDefinition CreatePartyMember(
        string id,
        string name,
        CombatActionDefinition action)
    {
        return new CombatantDefinition(
            CreateCharacter(id, name, 100, 10, 12),
            CombatantSide.Party,
            new[] { action });
    }

    private static CombatantDefinition CreateEnemy(
        string id,
        string name,
        CombatActionDefinition[] actions,
        EnemyBehaviorProfile profile)
    {
        return new CombatantDefinition(
            CreateCharacter(id, name, 60, 8, 5),
            CombatantSide.Enemy,
            actions,
            behaviorProfile: profile);
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
            label + " resolves.",
            recoveryCost,
            stunDuration: stunDuration);
    }

    private static Character CreateCharacter(
        string id,
        string name,
        int health,
        int agility,
        int luck)
    {
        CharacterData data = new CharacterData(
            id,
            name,
            "Mixed encounter test combatant",
            1,
            health,
            100,
            10,
            100,
            health,
            0,
            10,
            50,
            0,
            new CharacterStats(12, agility, 8, 12, 8, luck));

        return new Character(data);
    }
}
