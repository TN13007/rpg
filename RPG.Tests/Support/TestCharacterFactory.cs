using System;
using System.IO;

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
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "Combat", "CombatActions.json");
        return CombatActionJsonReader.Read(File.ReadAllText(path));
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
