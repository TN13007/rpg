using System;

internal static class TestCharacterFactory
{
    public static Character CreatePlayer(int health = 20, int energy = 8, int mood = 70, int gold = 20, int maxHealth = 20, int maxEnergy = 10, int maxMood = 100, int agility = 30, int intelligence = 25)
    {
        CharacterData data = new CharacterData(
            "test-player", "Test Player", "Test", 1,
            maxHealth, maxEnergy, maxMood,
            health, energy, mood, gold,
            new CharacterStats(20, agility, intelligence, 20, 20));
        return new Character(data);
    }

    public static Character CreateEnemy(int health = 40)
    {
        CharacterData data = new CharacterData(
            "test-enemy", "Test Enemy", "Training enemy", 1,
            health, 0, 0, health, 0, 0, 0);
        return new Character(data);
    }

    public static CombatService CreateCombat(out Character player, out Character enemy, int playerHealth = 20, int playerEnergy = 8, int enemyHealth = 40)
    {
        player = CreatePlayer(health: playerHealth, energy: playerEnergy);
        enemy = CreateEnemy(enemyHealth);
        return new CombatService(player, enemy);
    }
}
