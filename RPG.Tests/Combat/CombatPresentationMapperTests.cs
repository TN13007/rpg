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
