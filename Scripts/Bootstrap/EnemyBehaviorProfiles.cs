using System.Collections.Generic;

/// <summary>Provides predefined enemy behavior profiles for encounter setup.</summary>
public static class EnemyBehaviorProfiles
{
    /// <summary>Creates a profile that favors quick pressure and high-damage impacts.</summary>
    /// <returns>A profile that can select Quick Lunge or Heavy Impact, but not Disorienting Pulse.</returns>
    public static EnemyBehaviorProfile CreateHeavyPressurePresence()
    {
        return new EnemyBehaviorProfile(new Dictionary<string, int>
        {
            ["presence_quick_jab"] = 0,
            ["presence_heavy_swing"] = 1,
            ["presence_staggering_blow"] = 0
        });
    }

    /// <summary>Creates a profile that commits exclusively to the Disorienting Pulse control threat.</summary>
    /// <returns>A profile that selects Disorienting Pulse for every planned action.</returns>
    public static EnemyBehaviorProfile CreateDisruptiveEcho()
    {
        return new EnemyBehaviorProfile(new Dictionary<string, int>
        {
            ["presence_quick_jab"] = 0,
            ["presence_heavy_swing"] = 0,
            ["presence_staggering_blow"] = 1
        });
    }
}
