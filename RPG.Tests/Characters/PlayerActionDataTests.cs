using System;
using System.IO;
using Xunit;

public sealed class PlayerActionDataTests
{
    [Fact]
    public void PlayerData_LoadsRestGloballyAndSleepOnlyInStudentApartment()
    {
        CharacterData player = LoadPlayerData();

        Assert.Equal(3, player.PlayerActions.Length);
        PlayerActionDefinition rest = Find(player, "rest");
        PlayerActionDefinition sleep = Find(player, "sleep");
        PlayerActionDefinition rescue = Find(player, "request_rescue");

        Assert.True(rest.IsAvailableAt("student_apartment"));
        Assert.True(rest.IsAvailableAt("university_campus"));
        Assert.True(sleep.IsAvailableAt("student_apartment"));
        Assert.False(sleep.IsAvailableAt("university_campus"));
        Assert.Equal(480, sleep.Action.DurationMinutes);
        Assert.Equal("Sleep for 8 hours", sleep.Action.Label);
        Assert.True(rescue.IsAvailableAt("university_campus"));
        Assert.Equal(180, rescue.Action.DurationMinutes);
        ActionEffectDefinition reviveEffect = Assert.Single(rescue.Action.Effects);
        Assert.Equal(ActionEffectType.Revive, reviveEffect.Type);
        Assert.Equal(5, reviveEffect.Amount);
    }

    [Fact]
    public void PlayerData_DoesNotLoadLegacySelfActions()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "Locations.json");
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
