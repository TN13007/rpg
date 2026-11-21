using Xunit;

public sealed class CombatPresentationMapperTests
{
    [Fact]
    public void MapsRemainingProgressAndSpeedToExpectedReadyTime()
    {
        CombatPresentationMapper mapper = new CombatPresentationMapper();
        TimelineCombatantStatus ari = new TimelineCombatantStatus(
            "player_student",
            "Alex",
            false,
            20.0,
            10.0,
            15,
            false,
            false,
            true,
            true);

        CombatSnapshot snapshot = CreateSnapshot(ari);
        CombatViewData viewData = mapper.Map(snapshot);

        TimelineMarkerViewData marker = Assert.Single(viewData.Timeline);
        Assert.Equal(2.0, marker.TimeToReadySeconds, 3);
        Assert.True(marker.IsPrimaryPartyMember);
    }

    [Fact]
    public void IncludesRemainingStunDurationInExpectedReadyTime()
    {
        CombatPresentationMapper mapper = new CombatPresentationMapper();
        TimelineCombatantStatus stunnedPresence = new TimelineCombatantStatus(
            "hostile_presence",
            "Unstable Presence",
            true,
            20.0,
            10.0,
            5,
            false,
            true,
            remainingStunTime: 3.0);

        CombatSnapshot snapshot = CreateSnapshot(stunnedPresence);
        CombatViewData viewData = mapper.Map(snapshot);

        TimelineMarkerViewData marker = Assert.Single(viewData.Timeline);
        Assert.Equal(5.0, marker.TimeToReadySeconds, 3);
        Assert.True(marker.IsStunned);
    }

    [Fact]
    public void ReadyCombatantWithStunWaitsUntilStunExpires()
    {
        CombatPresentationMapper mapper = new CombatPresentationMapper();
        TimelineCombatantStatus stunnedReadyPartyMember = new TimelineCombatantStatus(
            "player_student",
            "Alex",
            false,
            0.0,
            10.0,
            15,
            true,
            true,
            true,
            true,
            2.0);

        CombatSnapshot snapshot = CreateSnapshot(stunnedReadyPartyMember);
        CombatViewData viewData = mapper.Map(snapshot);

        TimelineMarkerViewData marker = Assert.Single(viewData.Timeline);
        Assert.Equal(2.0, marker.TimeToReadySeconds, 3);
        Assert.True(marker.IsReady);
        Assert.True(marker.IsStunned);
    }

    [Theory]
    [InlineData("quick_strike", CombatFeedbackType.Attack)]
    [InlineData("heavy_strike", CombatFeedbackType.Attack)]
    [InlineData("disrupting_strike", CombatFeedbackType.Attack)]
    [InlineData("guard", CombatFeedbackType.Guard)]
    [InlineData("evade", CombatFeedbackType.Evade)]
    [InlineData("catch_breath", CombatFeedbackType.Recover)]
    [InlineData("revive_ally", CombatFeedbackType.Revive)]
    public void MapsPlayerActionIdsToSpecificPresentationFeedback(
        string actionId,
        CombatFeedbackType expectedFeedback)
    {
        CombatPresentationMapper mapper = new CombatPresentationMapper();
        CombatSnapshot snapshot = new CombatSnapshot(
            1,
            "PLAYER DECISION",
            System.Array.Empty<EnemyCombatStatus>(),
            null,
            System.Array.Empty<CombatActionOption>());
        CombatEvent actionEvent = new CombatEvent(
            CombatEventType.PlayerAction,
            1,
            actorName: "Alex",
            targetName: "Unstable Presence",
            actionId: actionId);
        CombatResult result = new CombatResult(true, snapshot, new[] { actionEvent });

        CombatFeedbackViewData feedback = Assert.Single(mapper.Map(result).Feedback);

        Assert.Equal("Alex", feedback.ActorName);
        Assert.Equal(expectedFeedback, feedback.Type);
    }

    [Fact]
    public void MapsDamageEventsToHitFeedbackOnTheTarget()
    {
        CombatPresentationMapper mapper = new CombatPresentationMapper();
        CombatSnapshot snapshot = new CombatSnapshot(
            1,
            "PLAYER DECISION",
            System.Array.Empty<EnemyCombatStatus>(),
            null,
            System.Array.Empty<CombatActionOption>());
        CombatEvent damageEvent = new CombatEvent(
            CombatEventType.Damage,
            1,
            targetName: "Unstable Presence",
            amount: 8);
        CombatResult result = new CombatResult(true, snapshot, new[] { damageEvent });

        CombatFeedbackViewData feedback = Assert.Single(mapper.Map(result).Feedback);

        Assert.Equal("Unstable Presence", feedback.TargetName);
        Assert.Equal(CombatFeedbackType.Hit, feedback.Type);
    }

    private static CombatSnapshot CreateSnapshot(TimelineCombatantStatus entry)
    {
        return new CombatSnapshot(
            1,
            "PARTY DECISION",
            System.Array.Empty<EnemyCombatStatus>(),
            null,
            System.Array.Empty<CombatActionOption>(),
            new[] { entry });
    }
}
