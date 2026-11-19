using Xunit;

public sealed class CharacterTests
{
    [Fact]
    public void CharacterWithPositiveHealthCanActRegardlessOfFatigueOrStamina()
    {
        for (int health = 1; health <= 20; health++)
        {
            for (int stamina = 0; stamina <= 10; stamina++)
            {
                Character character = TestCharacterFactory.CreatePlayer(
                    health: health,
                    fatigue: 100,
                    stamina: stamina);

                Assert.True(character.IsAlive);
                Assert.False(character.IsDead);
                Assert.True(character.CanAct);
            }
        }
    }

    [Fact]
    public void DeathPreventsCharacterFromActing()
    {
        Character character = TestCharacterFactory.CreatePlayer(health: 0, fatigue: 100, stamina: 0);

        Assert.False(character.IsAlive);
        Assert.True(character.IsDead);
        Assert.False(character.CanAct);
    }

    [Fact]
    public void DamageAndHealingAreClampedAtMinimumAndMaximum()
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
    public void FatigueAccumulationAndRecoveryRespectBoundaries()
    {
        Character character = TestCharacterFactory.CreatePlayer(fatigue: 95);

        Assert.Equal(5, character.AddFatigue(20));
        Assert.Equal(100, character.Fatigue);
        Assert.Equal(0, character.AddFatigue(-5));
        Assert.Equal(0, character.AddFatigue(double.NaN));
        Assert.Equal(25, character.RestoreFatigue(25));
        Assert.Equal(75, character.Fatigue);
        Assert.Equal(75, character.RestoreFatigue(100));
        Assert.Equal(0, character.Fatigue);
        Assert.Equal(0, character.RestoreFatigue(1));
    }

    [Fact]
    public void StaminaSpendingAndRecoveryRespectBoundaries()
    {
        Character character = TestCharacterFactory.CreatePlayer(stamina: 5);

        Assert.False(character.SpendStamina(-1));
        Assert.False(character.SpendStamina(6));
        Assert.Equal(5, character.Stamina);
        Assert.True(character.SpendStamina(5));
        Assert.Equal(0, character.Stamina);
        Assert.True(character.CanAct);
        Assert.Equal(0, character.RestoreStamina(-5));
        Assert.Equal(10, character.RestoreStamina(100));
        Assert.Equal(10, character.Stamina);
    }

    [Fact]
    public void MoodIsClampedAtZeroAndMaximum()
    {
        Character character = TestCharacterFactory.CreatePlayer(mood: 50);
        character.ChangeMood(int.MinValue);
        Assert.Equal(0, character.Mood);
        character.ChangeMood(int.MaxValue);
        Assert.Equal(100, character.Mood);
        character.ChangeMood(1);
        Assert.Equal(100, character.Mood);
    }

    [Fact]
    public void GoldCannotDecreaseAndCannotOverflow()
    {
        Character character = TestCharacterFactory.CreatePlayer(gold: 20);
        character.AddGold(-100);
        Assert.Equal(20, character.Gold);
        character.AddGold(int.MaxValue);
        Assert.Equal(int.MaxValue, character.Gold);
    }

    [Fact]
    public void ResetRestoresConfiguredFatigueAndStamina()
    {
        Character character = TestCharacterFactory.CreatePlayer(fatigue: 12.5, stamina: 4);

        character.AddFatigue(20);
        character.SpendStamina(2);
        character.Reset();

        Assert.Equal(12.5, character.Fatigue);
        Assert.Equal(4, character.Stamina);
    }
}
