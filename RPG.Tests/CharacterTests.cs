using Xunit;

public sealed class CharacterTests
{
    [Fact]
    public void HealthEnergyAndMood_StateGrid_HasCorrectLifeAndActionState()
    {
        for (int health = 0; health <= 20; health++)
        {
            for (int energy = 0; energy <= 10; energy++)
            {
                for (int mood = 0; mood <= 100; mood++)
                {
                    Character character = TestCharacterFactory.CreatePlayer(health: health, energy: energy, mood: mood);
                    bool expectedDead = health == 0;
                    bool expectedUnconscious = health > 0 && energy == 0;

                    Assert.Equal(!expectedDead, character.IsAlive);
                    Assert.Equal(expectedDead, character.IsDead);
                    Assert.Equal(expectedUnconscious, character.IsUnconscious);
                    Assert.Equal(!expectedDead && !expectedUnconscious, character.CanAct);
                    Assert.InRange(character.Mood, 0, character.MaxMood);
                }
            }
        }
    }

    [Fact]
    public void Death_TakesPriorityWhenHealthAndEnergyAreBothZero()
    {
        Character character = TestCharacterFactory.CreatePlayer(health: 0, energy: 0);
        Assert.True(character.IsDead);
        Assert.False(character.IsUnconscious);
        Assert.False(character.CanAct);
    }

    [Fact]
    public void ZeroEnergyWithPositiveHealth_IsUnconscious()
    {
        Character character = TestCharacterFactory.CreatePlayer(health: 1, energy: 0);
        Assert.True(character.IsAlive);
        Assert.True(character.IsUnconscious);
        Assert.False(character.CanAct);
    }

    [Fact]
    public void CharacterWithNoEnergyResource_IsNotAutomaticallyUnconscious()
    {
        Character enemy = TestCharacterFactory.CreateEnemy();
        Assert.True(enemy.IsAlive);
        Assert.False(enemy.IsUnconscious);
        Assert.True(enemy.CanAct);
    }

    [Fact]
    public void DamageAndHealing_AreClampedAtMinimumAndMaximum()
    {
        Character character = TestCharacterFactory.CreatePlayer(health: 5);
        Assert.Equal(0, character.TakeDamage(-10));
        Assert.Equal(5, character.Health);
        Assert.Equal(2, character.TakeDamage(2));
        Assert.Equal(3, character.Health);
        Assert.Equal(0, character.RestoreHealth(-10));
        Assert.Equal(17, character.RestoreHealth(100));
        Assert.Equal(20, character.Health);
        Assert.Equal(0, character.RestoreHealth(1));

        character.TakeDamage(100);
        Assert.Equal(0, character.Health);
        Assert.Equal(0, character.TakeDamage(100));
        Assert.Equal(0, character.RestoreHealth(100));
        Assert.True(character.IsDead);
    }

    [Fact]
    public void EnergySpendingAndRecovery_RespectBoundaries()
    {
        Character character = TestCharacterFactory.CreatePlayer(energy: 5);
        Assert.False(character.SpendEnergy(-1));
        Assert.False(character.SpendEnergy(6));
        Assert.Equal(5, character.Energy);
        Assert.True(character.SpendEnergy(5));
        Assert.Equal(0, character.Energy);
        Assert.True(character.IsUnconscious);
        Assert.Equal(0, character.RestoreEnergy(-5));
        Assert.Equal(10, character.RestoreEnergy(100));
        Assert.Equal(10, character.Energy);
        Assert.False(character.IsUnconscious);
    }

    [Fact]
    public void Mood_IsClampedAtZeroAndMaximum()
    {
        Character character = TestCharacterFactory.CreatePlayer(mood: 50);
        character.ChangeMood(-100);
        Assert.Equal(0, character.Mood);
        character.ChangeMood(100);
        Assert.Equal(100, character.Mood);
        character.ChangeMood(1);
        Assert.Equal(100, character.Mood);
    }

    [Fact]
    public void Gold_CannotDecreaseAndCannotOverflow()
    {
        Character character = TestCharacterFactory.CreatePlayer(gold: 20);
        character.AddGold(-100);
        Assert.Equal(20, character.Gold);
        character.AddGold(int.MaxValue);
        Assert.Equal(int.MaxValue, character.Gold);
    }
}
