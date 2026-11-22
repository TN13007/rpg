using System.Collections.Generic;
using Xunit;

public sealed class CharacterDefinitionImmutabilityTests
{
    [Fact]
    public void CharacterDataCopiesAndProtectsItsDefinitionCollections()
    {
        PlayerActionDefinition[] playerActions =
        {
            new PlayerActionDefinition(new ActionDefinition("rest", "Rest", 10))
        };
        string[] combatActionIds = { "quick_strike" };

        CharacterData data = new CharacterData(
            "student", "Student", "Test", 1, 20, 100, 10, 100,
            20, 0, 10, 50, 5,
            playerActions: playerActions,
            combatActionIds: combatActionIds);

        playerActions[0] = new PlayerActionDefinition(
            new ActionDefinition("sleep", "Sleep", 60));
        combatActionIds[0] = "changed";

        Assert.Equal("rest", data.PlayerActions[0].Action.Id);
        Assert.Equal("quick_strike", data.CombatActionIds[0]);
        Assert.Throws<System.NotSupportedException>(() =>
            ((IList<PlayerActionDefinition>)data.PlayerActions)[0] = playerActions[0]);
        Assert.Throws<System.NotSupportedException>(() =>
            ((IList<string>)data.CombatActionIds)[0] = "changed");
    }

    [Fact]
    public void NestedPlayerActionAndActionEffectCollectionsAreDefensiveAndReadOnly()
    {
        string[] availableLocations = { "student_apartment" };
        PlayerActionDefinition playerAction = new PlayerActionDefinition(
            new ActionDefinition("rest", "Rest", 10),
            availableLocations);
        ActionEffectDefinition[] effects =
        {
            new ActionEffectDefinition(ActionEffectType.RestoreHealth, 5)
        };
        ActionDefinition action = new ActionDefinition(
            "recover", "Recover", 10, effects: effects);

        availableLocations[0] = "university_campus";
        effects[0] = new ActionEffectDefinition(ActionEffectType.RestoreHealth, 10);

        Assert.True(playerAction.IsAvailableAt("student_apartment"));
        Assert.False(playerAction.IsAvailableAt("university_campus"));
        Assert.Equal(5, action.Effects[0].Amount);
        Assert.Throws<System.NotSupportedException>(() =>
            ((IList<string>)playerAction.AvailableLocationIds)[0] = "university_campus");
        Assert.Throws<System.NotSupportedException>(() =>
            ((IList<ActionEffectDefinition>)action.Effects)[0] =
                new ActionEffectDefinition(ActionEffectType.RestoreHealth, 10));
    }

    [Fact]
    public void CombatantDefinitionCopiesAndProtectsItsActionCollection()
    {
        Character character = TestCharacterFactory.CreatePlayer();
        CombatActionDefinition[] actions = TestCharacterFactory.CreatePlayerCombatActions();
        CombatantDefinition definition = new CombatantDefinition(
            character, CombatantSide.Party, actions);

        CombatActionDefinition originalFirstAction = definition.Actions[0];
        actions[0] = actions[1];

        Assert.Same(originalFirstAction, definition.Actions[0]);
        Assert.Throws<System.NotSupportedException>(() =>
            ((IList<CombatActionDefinition>)definition.Actions)[0] = actions[1]);
    }
}
