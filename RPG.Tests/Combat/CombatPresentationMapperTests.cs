using System;
using Xunit;

public sealed class CombatPresentationMapperTests
{
    [Fact]
    public void MapsSharedApAndCombatStatusesIntoPresentationOwnedViewData()
    {
        CombatPresentationMapper mapper = new CombatPresentationMapper();
        CombatSnapshot snapshot = CreateSnapshot();

        CombatViewData viewData = mapper.Map(snapshot);

        Assert.Equal(3, viewData.Round);
        Assert.True(viewData.IsPlayerTurn);
        Assert.Equal(1, viewData.CurrentActionPoints);
        Assert.Equal(3, viewData.ActionCapacity);
        Assert.Equal("PLAYER TURN", viewData.ActiveTurnLabel);
        Assert.Equal("Unstable Presence", Assert.Single(viewData.Enemies).Name);
        PartyMemberCardViewData partyMember = Assert.Single(viewData.Party);
        Assert.Equal("Alex", partyMember.Name);
        Assert.True(partyMember.CanAct);
        Assert.Equal(7, partyMember.CurrentStamina);
        Assert.Equal(10, partyMember.MaxStamina);
        Assert.Null(partyMember.Arousal);
        Assert.Empty(viewData.NewMessages);
    }

    [Fact]
    public void PresentationCardsAcceptOptionalArousalOnlyWhenARealValueIsSupplied()
    {
        EnemyCardViewData enemy = new EnemyCardViewData(
            "enemy-1",
            "Unstable Presence",
            40,
            60,
            "Healthy",
            arousal: 72);
        PartyMemberCardViewData partyMember = new PartyMemberCardViewData(
            "party-1",
            "Alex",
            20,
            20,
            "Healthy",
            true,
            currentStamina: 6,
            maxStamina: 10,
            arousal: 0);

        Assert.Equal(72, enemy.Arousal);
        Assert.Equal(0, partyMember.Arousal);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new EnemyCardViewData("enemy-2", "Invalid", 10, 10, "Healthy", arousal: 101));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PartyMemberCardViewData(
                "party-2", "Invalid", 10, 10, "Healthy", true, 5, 10, arousal: -1));
    }

    [Fact]
    public void MapsOnlyVisibleEnemiesIntoEnemyCardViewData()
    {
        CombatPresentationMapper mapper = new CombatPresentationMapper();
        CombatSnapshot snapshot = CreateSnapshot(
            enemies: new[]
            {
                new EnemyCombatStatus("visible", "Visible Presence", 20, 30, "Healthy"),
                new EnemyCombatStatus("hidden", "Hidden Presence", 30, 30, "Healthy", isVisible: false)
            });

        CombatViewData viewData = mapper.Map(snapshot);

        EnemyCardViewData visible = Assert.Single(viewData.Enemies);
        Assert.Equal("visible", visible.Id);
        Assert.Equal("Visible Presence", visible.Name);
    }

    [Theory]
    [InlineData("quick_strike", CombatFeedbackType.QuickAttack)]
    [InlineData("heavy_strike", CombatFeedbackType.HeavyAttack)]
    [InlineData("disrupting_strike", CombatFeedbackType.DisruptingAttack)]
    [InlineData("guard", CombatFeedbackType.Guard)]
    [InlineData("evade", CombatFeedbackType.Evade)]
    [InlineData("catch_breath", CombatFeedbackType.Recover)]
    [InlineData("revive_ally", CombatFeedbackType.Revive)]
    public void MapsEachPlayerActionToItsOwnPresentationFeedback(
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
    public void MapsActionCategoryAndTargetDataIntoPresentationOwnedActionCard()
    {
        CombatPresentationMapper mapper = new CombatPresentationMapper();
        CombatActionOption attack = new CombatActionOption(
            "quick_strike",
            "player_student",
            "Alex · Quick Strike",
            "1 AP",
            "Deal 8 damage.",
            targets: new[]
            {
                new CombatActionTargetOption("enemy-1", "Unstable Presence")
            });
        CombatActionOption recover = new CombatActionOption(
            "catch_breath",
            "player_student",
            "Alex · Catch Breath",
            "1 AP",
            "Recover 3 Stamina.");

        CombatViewData viewData = mapper.Map(CreateSnapshot(actions: new[] { attack, recover }));
        Assert.Equal(2, viewData.AvailableActions.Length);

        CombatActionCardViewData attackCard = viewData.AvailableActions[0];
        Assert.Equal(CombatActionCategory.Offense, attackCard.Category);
        Assert.Equal(CombatActionTargetKind.Enemy, attackCard.TargetKind);
        Assert.Equal("enemy-1", Assert.Single(attackCard.Targets).Id);

        CombatActionCardViewData recoverCard = viewData.AvailableActions[1];
        Assert.Equal(CombatActionCategory.Support, recoverCard.Category);
        Assert.Equal(CombatActionTargetKind.None, recoverCard.TargetKind);
        Assert.Empty(recoverCard.Targets);
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

    [Fact]
    public void ActionSelectionCarriesOnlyStableIdsAcrossThePresentationBoundary()
    {
        CombatActionSelection selection = new CombatActionSelection(
            "quick_strike",
            "player_student",
            "hostile_presence");

        Assert.Equal("quick_strike", selection.ActionId);
        Assert.Equal("player_student", selection.ActorId);
        Assert.Equal("hostile_presence", selection.TargetId);
    }

    private static CombatSnapshot CreateSnapshot(
        CombatActionOption[] actions = null,
        EnemyCombatStatus[] enemies = null)
    {
        return new CombatSnapshot(
            3,
            CombatPhase.PlayerTurn,
            1,
            3,
            "PLAYER TURN",
            enemies ?? new[]
            {
                new EnemyCombatStatus("hostile_presence", "Unstable Presence", 40, 60, "Wounded")
            },
            new[]
            {
                new PartyCombatStatus(
                    "player_student",
                    "Alex",
                    20,
                    20,
                    "Healthy",
                    true,
                    currentStamina: 7,
                    maxStamina: 10)
            },
            actions ?? Array.Empty<CombatActionOption>());
    }
}
