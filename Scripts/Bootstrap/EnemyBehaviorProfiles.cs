using System.Collections.Generic;

public static class EnemyBehaviorProfiles
{
    public static EnemyBehaviorProfile CreateGoblinSkirmisher()
    {
        return new EnemyBehaviorProfile(new Dictionary<string, int>
        {
            ["goblin_quick_jab"] = 8,
            ["goblin_heavy_swing"] = 1,
            ["goblin_staggering_blow"] = 1
        });
    }

    public static EnemyBehaviorProfile CreateBrute()
    {
        return new EnemyBehaviorProfile(new Dictionary<string, int>
        {
            ["brute_crushing_slam"] = 8,
            ["brute_overhead_smash"] = 2
        });
    }

    public static EnemyBehaviorProfile CreateDisruptor()
    {
        return new EnemyBehaviorProfile(new Dictionary<string, int>
        {
            ["disruptor_staggering_blow"] = 8,
            ["disruptor_quick_jab"] = 2
        });
    }
}
