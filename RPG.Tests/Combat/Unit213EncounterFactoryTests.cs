using System;
using System.IO;
using System.Linq;
using Xunit;

public sealed class Unit213EncounterFactoryTests
{
    [Fact]
    public void RuntimeEncounterComposesFourPartyMembersAndTwoDistinctUnit213Threats()
    {
        GameContentCatalog content = LoadContent();
        Character primaryCharacter = new Character(content.GetCharacter("player_student"));

        CombatantDefinition[] roster =
            Unit213EncounterFactory.CreateCombatants(content, primaryCharacter);

        Assert.Equal(6, roster.Length);
        Assert.Equal(4, roster.Count(item => item.Side == CombatantSide.Party));
        Assert.Equal(2, roster.Count(item => item.Side == CombatantSide.Enemy));
        Assert.Same(primaryCharacter, roster[0].Character);

        CombatantDefinition heavyPresence = Assert.Single(
            roster,
            item => item.Character.Id == "hostile_presence");
        CombatantDefinition disruptiveEcho = Assert.Single(
            roster,
            item => item.Character.Id == "hostile_echo");

        Assert.Equal(1, heavyPresence.BehaviorProfile.GetWeight("presence_heavy_swing"));
        Assert.Equal(0, heavyPresence.BehaviorProfile.GetWeight("presence_staggering_blow"));
        Assert.Equal(0, disruptiveEcho.BehaviorProfile.GetWeight("presence_heavy_swing"));
        Assert.Equal(1, disruptiveEcho.BehaviorProfile.GetWeight("presence_staggering_blow"));

        CombatService combat = new CombatService(roster, new Random(17));
        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);

        CombatSnapshot snapshot = combat.GetSnapshot();
        TimelineCombatantStatus heavyIntent = Assert.Single(
            snapshot.Timeline,
            item => item.Id == "hostile_presence");
        TimelineCombatantStatus controlIntent = Assert.Single(
            snapshot.Timeline,
            item => item.Id == "hostile_echo");

        Assert.StartsWith("Heavy Impact", heavyIntent.IntentDescription);
        Assert.StartsWith("Disorienting Pulse", controlIntent.IntentDescription);
        Assert.Contains("→", heavyIntent.IntentDescription);
        Assert.Contains("→", controlIntent.IntentDescription);
    }

    private static GameContentCatalog LoadContent()
    {
        string charactersDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "Characters");

        CharacterData[] characters = Directory.GetFiles(charactersDirectory, "*.json")
            .Select(path => CharacterDataLoader.LoadJson(File.ReadAllText(path), path))
            .ToArray();

        return new GameContentCatalog(
            new LocationCatalog(),
            new CharacterCatalog(characters),
            new CombatActionCatalog(TestCharacterFactory.CreateCombatActions()),
            new NpcScheduleCatalog(Array.Empty<NpcScheduleDefinition>()));
    }
}
