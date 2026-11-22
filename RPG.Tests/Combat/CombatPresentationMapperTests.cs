using System;
using Xunit;

public sealed class CombatPresentationMapperTests
{
    [Fact]
    public void MapsSharedApPhaseAndRoundIntoPresentationViewData()
    {
        CombatPresentationMapper mapper = new CombatPresentationMapper();
        CombatSnapshot snapshot = CreateSnapshot();

        CombatViewData viewData = mapper.Map(snapshot);

        Assert.Equal(3, viewData.Round);
        Assert.Equal(CombatPhase.PlayerTurn, viewData.Phase);
        Assert.Equal(1, viewData.CurrentActionPoints);
        Assert.Equal(3, viewData.ActionCapacity);
        Assert.Equal("PLAYER TURN", viewData.ActiveTurnLabel);
        Assert.Equal("Alex", viewData.Party[0].Name);
        Assert.Empty(viewData.NewMessages);
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
        CombatEvent actionEvent = new CombatEvent(
            CombatEventType.PlayerAction,
            2,
            actorName: "Alex",
            targetName: "Unstable Presence",
            actionId: actionId);
        CombatResult result = new CombatResult(true, CreateSnapshot(), new[] { actionEvent });

        CombatFeedbackViewData feedback = Assert.Single(mapper.Map(result).Feedback);

        Assert.Equal("Alex", feedback.ActorName);
        Assert.Equal(expectedFeedback, feedback.Type);
    }

    [Fact]
    public void MapsEnemyActionAndDamageEventsWithoutChangingTheirOrder()
    {
        CombatPresentationMapper mapper = new CombatPresentationMapper();
        CombatEvent enemyAction = new CombatEvent(
            CombatEventType.EnemyAction,
            2,
            actorName: "Unstable Echo",
            targetName: "Alex",
            detail: "The echo releases a pulse.");
        CombatEvent damage = new CombatEvent(
            CombatEventType.Damage,
            2,
            targetName: "Alex",
            amount: 3);
        CombatEvent control = new CombatEvent(
            CombatEventType.StatusChanged,
            2,
            targetName: "Alex",
            detail: "The next Player Turn loses 1 AP.");
        CombatResult result = new CombatResult(
            true,
            CreateSnapshot(),
            new[] { enemyAction, damage, control });

        CombatViewData viewData = mapper.Map(result);

        Assert.Equal(3, viewData.NewMessages.Length);
        Assert.Equal("Unstable Echo", viewData.NewMessages[0].Speaker);
        Assert.Contains("releases a pulse", viewData.NewMessages[0].Text);
        Assert.Equal("Alex loses 3 HP.", viewData.NewMessages[1].Text);
        Assert.Equal(CombatMessageCategory.StatusChange, viewData.NewMessages[2].Category);
    }

    [Fact]
    public void MapsDamageEventsToHitFeedbackOnTheTarget()
    {
        CombatPresentationMapper mapper = new CombatPresentationMapper();
        CombatEvent damageEvent = new CombatEvent(
            CombatEventType.Damage,
            1,
            targetName: "Unstable Presence",
            amount: 8);
        CombatResult result = new CombatResult(true, CreateSnapshot(), new[] { damageEvent });

        CombatFeedbackViewData feedback = Assert.Single(mapper.Map(result).Feedback);

        Assert.Equal("Unstable Presence", feedback.TargetName);
        Assert.Equal(CombatFeedbackType.Hit, feedback.Type);
    }

    private static CombatSnapshot CreateSnapshot()
    {
        return new CombatSnapshot(
            3,
            CombatPhase.PlayerTurn,
            1,
            3,
            "PLAYER TURN",
            new[]
            {
                new EnemyCombatStatus("hostile_presence", "Unstable Presence", 40, 60, "Wounded")
            },
            new[]
            {
                new PartyCombatStatus("player_student", "Alex", 20, 20, "Healthy", true)
            },
            Array.Empty<CombatActionOption>());
    }
}