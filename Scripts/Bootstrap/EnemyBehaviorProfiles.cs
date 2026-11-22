using System.Collections.Generic;

/// <summary>Provides predefined enemy behavior profiles for encounter setup.</summary>
public static class EnemyBehaviorProfiles
{
    /// <summary>Creates the configured behavior profile for an unstable enemy presence.</summary>
    /// <returns>The resulting enemy behavior profile.</returns>
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
