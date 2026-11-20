using Xunit;

public sealed class CharacterViewModelTests
{
    [Fact]
    public void CharacterViewModelExposesAllCoreAttributes()
    {
        Character character = TestCharacterFactory.CreatePlayer();
        CharacterViewModel viewModel = new CharacterViewModel(character);

        Assert.Equal(20, viewModel.Stats.Strength);
        Assert.Equal(30, viewModel.Stats.Agility);
        Assert.Equal(25, viewModel.Stats.Intelligence);
        Assert.Equal(20, viewModel.Stats.Endurance);
        Assert.Equal(20, viewModel.Stats.Wisdom);
    }
}
