using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

public sealed class ExplorationInteractionQueryTests
{
    [Fact]
    public void Query_OffersRestEverywhereAndSleepOnlyInDormitory()
    {
        LoadContent();
        Character player = TestCharacterFactory.CreatePlayer();
        CharacterData playerData = LoadPlayerData();
        AvailableInteractionQuery query = new AvailableInteractionQuery();

        IReadOnlyList<AvailableInteraction> dormitory = query.Query(
            player, playerData.PlayerActions, AcademyContent.Get("dormitory"));
        IReadOnlyList<AvailableInteraction> courtyard = query.Query(
            player, playerData.PlayerActions, AcademyContent.Get("courtyard"));

        Assert.Contains(dormitory, item => item.SourceKind == InteractionSourceKind.Player && item.ActionId == "rest");
        Assert.Contains(dormitory, item => item.SourceKind == InteractionSourceKind.Player && item.ActionId == "sleep");
        Assert.Contains(courtyard, item => item.SourceKind == InteractionSourceKind.Player && item.ActionId == "rest");
        Assert.DoesNotContain(courtyard, item => item.SourceKind == InteractionSourceKind.Player && item.ActionId == "sleep");
    }

    [Fact]
    public void Query_ExcludesEventEncounterActionsAndCreatesTravelTargets()
    {
        LoadContent();
        Character player = TestCharacterFactory.CreatePlayer();
        CharacterData playerData = LoadPlayerData();
        IReadOnlyList<AvailableInteraction> interactions = new AvailableInteractionQuery().Query(
            player, playerData.PlayerActions, AcademyContent.Get("courtyard"));

        Assert.DoesNotContain(interactions, item => item.SourceKind == InteractionSourceKind.Location && item.ActionId == "encounter_goblin");
        Assert.Contains(interactions, item =>
            item.SourceKind == InteractionSourceKind.Travel &&
            item.SourceId == "courtyard" &&
            item.TargetId == "dormitory");
    }

    [Fact]
    public void Query_DoesNotMutatePlayerState()
    {
        LoadContent();
        Character player = TestCharacterFactory.CreatePlayer(fatigue: 45, stamina: 3);
        CharacterData playerData = LoadPlayerData();

        new AvailableInteractionQuery().Query(player, playerData.PlayerActions, AcademyContent.Get("dormitory"));

        Assert.Equal(45, player.Fatigue);
        Assert.Equal(3, player.Stamina);
    }

    [Fact]
    public void Mapper_PreservesSourceIdentityAndPresentationGroups()
    {
        LoadContent();
        Character player = TestCharacterFactory.CreatePlayer();
        CharacterData playerData = LoadPlayerData();
        IReadOnlyList<AvailableInteraction> interactions = new AvailableInteractionQuery().Query(
            player, playerData.PlayerActions, AcademyContent.Get("dormitory"));
        ActionChoiceMapper mapper = new ActionChoiceMapper(
            new ActionDurationEstimateMapper(new ActivityTimeCalculator()));

        List<ActionChoiceViewData> choices = mapper.Map(interactions, player);

        Assert.Contains(choices, choice => choice.GroupTitle == "PERSONAL");
        Assert.Contains(choices, choice => choice.GroupTitle == "AT THIS LOCATION");
        Assert.Contains(choices, choice => choice.GroupTitle == "TRAVEL");
        Assert.Contains(choices, choice =>
            ActionSelectionRequest.TryParse(choice.Id, out ActionSelectionRequest request) &&
            request.SourceKind == InteractionSourceKind.Player &&
            request.ActionId == "sleep" &&
            request.SourceId == player.Id);
    }

    [Fact]
    public void SelectionRequest_RoundTripsAndRejectsMalformedValues()
    {
        ActionSelectionRequest request = new ActionSelectionRequest(
            InteractionSourceKind.Travel, "courtyard", "travel", "guild_hall");

        Assert.True(ActionSelectionRequest.TryParse(request.Encode(), out ActionSelectionRequest parsed));
        Assert.Equal(request.SourceKind, parsed.SourceKind);
        Assert.Equal(request.SourceId, parsed.SourceId);
        Assert.Equal(request.ActionId, parsed.ActionId);
        Assert.Equal(request.TargetId, parsed.TargetId);
        Assert.False(ActionSelectionRequest.TryParse("travel:courtyard", out _));
    }

    private static void LoadContent()
    {
        AcademyContent.LoadJson(File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Data", "AcademyLocations.json")));
    }

    private static CharacterData LoadPlayerData()
    {
        return CharacterDataLoader.LoadJson(File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Data", "Player.json")));
    }
}
