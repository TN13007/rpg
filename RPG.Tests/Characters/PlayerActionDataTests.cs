using System;
using System.IO;
using Xunit;

public sealed class PlayerActionDataTests
{
    [Fact]
    public void PlayerData_LoadsRestGloballyAndSleepOnlyInDormitory()
    {
        CharacterData player = LoadPlayerData();

        Assert.Equal(2, player.PlayerActions.Length);
        PlayerActionDefinition rest = Find(player, "rest");
        PlayerActionDefinition sleep = Find(player, "sleep");

        Assert.True(rest.IsAvailableAt("dormitory"));
        Assert.True(rest.IsAvailableAt("courtyard"));
        Assert.True(sleep.IsAvailableAt("dormitory"));
        Assert.False(sleep.IsAvailableAt("courtyard"));
        Assert.Equal(480, sleep.Action.DurationMinutes);
        Assert.Equal("Sleep for 8 hours", sleep.Action.Label);
    }

    [Fact]
    public void PlayerData_DoesNotLoadLegacySelfActions()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "AcademyLocations.json");
        string json = File.ReadAllText(path);

        Assert.DoesNotContain("\"selfActions\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"id\": \"sleep\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void PlayerData_RejectsDuplicatePlayerActionIds()
    {
        string json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Data", "Player.json"));
        string duplicate = json.Replace(
            "\"playerActions\": [",
            "\"playerActions\": [ { \"id\": \"rest\", \"label\": \"Duplicate\", \"durationMinutes\": 1 },");
        Assert.Throws<InvalidOperationException>(() => CharacterDataLoader.LoadJson(duplicate));
    }

    private static CharacterData LoadPlayerData()
    {
        return CharacterDataLoader.LoadJson(
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Data", "Player.json")));
    }

    private static PlayerActionDefinition Find(CharacterData player, string id)
    {
        foreach (PlayerActionDefinition action in player.PlayerActions)
        {
            if (action.Action.Id == id) return action;
        }
        throw new InvalidOperationException("Player action was not found: " + id);
    }
}
