using System;

internal static class TestCharacterFactory
{
    public static Character CreatePlayer(
        int health = 20,
        double fatigue = 0,
        int stamina = 8,
        int mood = 70,
        int gold = 20,
        int maxHealth = 20,
        double maxFatigue = 100,
        int maxStamina = 10,
        int maxMood = 100,
        int agility = 30,
        int intelligence = 25)
    {
        CharacterData data = new CharacterData(
            "test-player", "Test Player", "Test", 1,
            maxHealth, maxFatigue, maxStamina, maxMood,
            health, fatigue, stamina, mood, gold,
            new CharacterStats(20, agility, intelligence, 20, 20));
        return new Character(data);
    }

    public static Character CreateEnemy(int health = 40)
    {
        CharacterData data = new CharacterData(
            "test-enemy", "Test Enemy", "Training enemy", 1,
            health, 0, 0, 0, health, 0, 0, 0, 0);
        return new Character(data);
    }

    public static CombatActionDefinition[] CreateCombatActions()
    {
        return new[]
        {
            new CombatActionDefinition("attack", "Attack", "No Stamina cost", "Deal 20 damage to the enemy.",
                CombatActionBehavior.Attack, 0, 20, 0, "", "You attack the enemy with a basic strike."),
            new CombatActionDefinition("defend", "Defend", "1 Stamina", "Reduce the next enemy hit to 1 damage.",
                CombatActionBehavior.Defend, 1, 0, 0, "Not enough Stamina.", "You take a guarded stance."),
            new CombatActionDefinition("dodge", "Dodge", "2 Stamina", "Avoid the next enemy attack.",
                CombatActionBehavior.Dodge, 2, 0, 0, "Not enough Stamina.", "You prepare to evade the next attack."),
            new CombatActionDefinition("catch_breath", "Catch Breath", "Recover 3 Stamina",
                "Recover your composure; the enemy still gets its turn.",
                CombatActionBehavior.RecoverStamina, 0, 0, 3, "Stamina is already full.",
                "You catch your breath and recover {amount} Stamina.")
        };
    }

    public static CombatService CreateCombat(
        out Character player,
        out Character enemy,
        int playerHealth = 20,
        int playerStamina = 8,
        int enemyHealth = 40)
    {
        player = CreatePlayer(health: playerHealth, stamina: playerStamina);
        enemy = CreateEnemy(enemyHealth);
        return new CombatService(player, enemy, CreateCombatActions());
    }
}
