using System.Collections.Generic;

public static class EnemyBehaviorProfiles
{
    public static EnemyBehaviorProfile CreateUnstablePresence()
    {
        return new EnemyBehaviorProfile(new Dictionary<string, int>
        {
            ["presence_quick_jab"] = 6,
            ["presence_heavy_swing"] = 2,
            ["presence_staggering_blow"] = 2
        });
    }
}
