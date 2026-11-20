    [Fact]
    public void EncounterStartsAtCombatTimeZeroAndReachesReadyAfterElapsedTime()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        Assert.True(combat.TryStartEncounter(out string reason));
        Assert.False(combat.IsAwaitingPlayerDecision);
        Assert.Equal(0, combat.CombatTime);

        combat.AdvanceCombatTime(12.4);
        Assert.False(combat.IsAwaitingPlayerDecision);
        Assert.Equal(12.4, combat.CombatTime, 3);

        combat.AdvanceCombatTime(0.1);
        Assert.True(combat.IsAwaitingPlayerDecision);
        Assert.Equal(12.5, combat.CombatTime, 3);

        TimelineCombatantStatus playerEntry = combat.GetSnapshot().Timeline.Single(entry => entry.Id == player.Id);
        TimelineCombatantStatus enemyEntry = combat.GetSnapshot().Timeline.Single(entry => entry.Id == enemy.Id);
        Assert.Equal(0, playerEntry.RemainingProgress, 3);
        Assert.Equal(27.5, enemyEntry.RemainingProgress, 3);
    }

    [Fact]
    public void CombatClockDoesNotAdvanceWhilePlayerDecisionIsOpen()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        StartAtPlayerDecision(combat);
        double decisionTime = combat.CombatTime;

        combat.AdvanceCombatTime(2.0);

        Assert.True(combat.IsAwaitingPlayerDecision);
        Assert.Equal(decisionTime, combat.CombatTime, 6);
    }

;

        Assert.True(combat.IsAwaitingPlayerDecision);
        Assert.Equal(decisionTime, combat.CombatTime, 6);
    }

    [Fact]
    public void QuickStrikeResolvesAndEnemyActsOnItsOwnTimelineEvent()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        StartAtPlayerDecision(combat);
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("quick_strike", enemy.Id, player.Id));
        Assert.True(result.Success);
        Assert.Equal(32, enemy.Health);
        Assert.DoesNotContain(result.Events, item => item.Type == CombatEventType.EnemyAction);
        Assert.Equal(12.5, combat.CombatTime, 3);

        CombatResult enemyResult = combat.AdvanceCombatTime(1000);
        Assert.Contains(enemyResult.Events, item => item.Type == CombatEventType.EnemyAction);
        Assert.True(combat.IsAwaitingPlayerDecision);
        Assert.True(combat.CombatTime > 12.5);
    }

    [Fact]
    public void HeavyStrikeHasGreaterRecoveryThanQuickStrike()
    {
        CombatService quick = TestCharacterFactory.CreateCombat(out Character quickPlayer, out Character quickEnemy);
        CombatService heavy = TestCharacterFactory.CreateCombat(out Character heavyPlayer, out Character heavyEnemy);
        StartAtPlayerDecision(quick);
        StartAtPlayerDecision(heavy);
        quick.ExecuteAction(new CombatActionRequest("quick_strike", quickEnemy.Id));
        heavy.ExecuteAction(new CombatActionRequest("heavy_strike", heavyEnemy.Id));
        quick.AdvanceCombatTime(1000);
        heavy.AdvanceCombatTime(1000);
        Assert.True(heavy.CombatTime > quick.CombatTime);
    }

    [Fact]
    public void DisruptingStrikeAppliesDelayThroughEffectDispatcher()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        StartAtPlayerDecision(combat);
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("disrupting_strike", enemy.Id, player.Id));
        Assert.True(result.Success);
        Assert.Contains(result.Events, item => item.Detail.Contains("delayed"));
    }

    [Fact]
    public void InsufficientStaminaDoesNotConsumeDecisionWindow()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy, playerStamina: 1);
        StartAtPlayerDecision(combat);
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("evade", string.Empty, player.Id));
        Assert.False(result.Success);
        Assert.True(combat.IsAwaitingPlayerDecision);
        Assert.Equal(1, player.Stamina);
        Assert.Equal(40, enemy.Health);
    }

    [Fact]
    public void GuardSpendsStaminaAndReducesTheNextGoblinHit()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        StartAtPlayerDecision(combat);
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("guard", string.Empty, player.Id));
        Assert.True(result.Success);
        Assert.Equal(7, player.Stamina);
        CombatResult enemyResult = combat.AdvanceCombatTime(1000);
        Assert.Contains(enemyResult.Events, item => item.Type == CombatEventType.EnemyAction);
        Assert.InRange(player.Health, 12, 19);
    }

    [Fact]
    public void EvadeSpendsStaminaAndAvoidsTheNextGoblinHit()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy);
        StartAtPlayerDecision(combat);
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("evade", string.Empty, player.Id));
        Assert.True(result.Success);
        Assert.Equal(6, player.Stamina);
        Assert.Contains(result.Events, item => item.Type == CombatEventType.Check);
        Assert.Equal(20, player.Health);

        CombatResult enemyResult = combat.AdvanceCombatTime(1000);
        Assert.Contains(enemyResult.Events, item => item.Type == CombatEventType.EnemyAction);
        Assert.Equal(20, player.Health);
    }

    [Fact]
    public void CatchBreathRestoresStaminaAndTimelineContinues()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy, playerStamina: 1);
        StartAtPlayerDecision(combat);
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("catch_breath", string.Empty, player.Id));
        Assert.True(result.Success);
        Assert.Equal(4, player.Stamina);
        CombatResult enemyResult = combat.AdvanceCombatTime(1000);
        Assert.Contains(enemyResult.Events, item => item.Type == CombatEventType.EnemyAction);
    }

    [Fact]
    public void DefeatingGoblinEndsEncounterWithoutCounterattack()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy, enemyHealth: 8);
        StartAtPlayerDecision(combat);
        CombatResult result = combat.ExecuteAction(new CombatActionRequest("quick_strike", enemy.Id, player.Id));
        Assert.True(result.Success);
        Assert.True(combat.IsEncounterOver);
        Assert.True(combat.IsVictory);
        Assert.Equal(0, enemy.Health);
        Assert.Equal(45, player.Gold);
        Assert.DoesNotContain(result.Events, item => item.Type == CombatEventType.EnemyAction);
    }

    [Fact]
    public void GoblinChoosesOnlyFromItsAuthoredActionSet()
    {
        CombatService combat = TestCharacterFactory.CreateCombat(out Character player, out Character enemy, randomSeed: 17);
        StartAtPlayerDecision(combat);
        combat.ExecuteAction(new CombatActionRequest("quick_strike", enemy.Id, player.Id));
        CombatResult result = combat.AdvanceCombatTime(1000);
        string[] allowed =
        {
            "The goblin snaps out a quick jab.",
            "The goblin commits to a heavy swing.",
            "The goblin lands a staggering blow."
        };
        Assert.Contains(result.Events, item => item.Type == CombatEventType.EnemyAction && allowed.Contains(item.Detail));
    }

    [Fact]
    public void EncounterRegistersEveryCombatantAndLetsThePlayerCommandEachPartyMember()
    {
        CombatActionDefinition[] partyActions = TestCharacterFactory.CreatePlayerCombatActions();
        CombatActionDefinition[] enemyActions = TestCharacterFactory.CreateEnemyCombatActions();
        Character[] party =
        {
            CreateRosterCharacter("party-a", "Party A", 20, 30, 20),
            CreateRosterCharacter("party-b", "Party B", 15, 30, 20),
            CreateRosterCharacter("party-c", "Party C", 10, 30, 20),
            CreateRosterCharacter("party-d", "Party D", 5, 30, 20)
        };
        Character enemyA = CreateRosterCharacter("enemy-a", "Enemy A", 5, 8, 100);
        Character enemyB = CreateRosterCharacter("enemy-b", "Enemy B", 5, 8, 100);

        List<CombatantDefinition> roster = new List<CombatantDefinition>();
        foreach (Character member in party)
        {
            roster.Add(new CombatantDefinition(member, CombatantSide.Party, partyActions));
        }

        roster.Add(new CombatantDefinition(enemyA, CombatantSide.Enemy, enemyActions));
        roster.Add(new CombatantDefinition(enemyB, CombatantSide.Enemy, enemyActions));

        CombatService combat = new CombatService(roster, new Random(23));
        StartAtPlayerDecision(combat);

        CombatSnapshot openingSnapshot = combat.GetSnapshot();
        Assert.Equal(4, openingSnapshot.Party.Length);
        Assert.Equal(2, openingSnapshot.Enemies.Length);
        Assert.Equal(6, openingSnapshot.Timeline.Length);
        Assert.Equal("party-a", openingSnapshot.ActiveCombatantId);
        Assert.True(openingSnapshot.Timeline.Single(entry => entry.Id == "party-a").IsPrimaryPartyMember);
        Assert.False(openingSnapshot.Timeline.Single(entry => entry.Id == enemyA.Id).IsPrimaryPartyMember);

        CombatResult lastResult = null;
        foreach (Character member in party)
        {
            if (!combat.IsAwaitingPlayerDecision)
            {
                combat.AdvanceCombatTime(1000);
            }

            CombatSnapshot snapshot = combat.GetSnapshot();
            Assert.Equal(member.Id, snapshot.ActiveCombatantId);

            CombatActionOption action = Assert.Single(snapshot.AvailableActions.Where(option =>
                option.Id == "quick_strike" && option.TargetId == enemyA.Id));
            Assert.Equal(member.Id, action.ActorId);

            lastResult = combat.ExecuteAction(new CombatActionRequest(
                action.Id,
                action.TargetId,
                action.ActorId));
            Assert.True(lastResult.Success);
        }

        CombatResult enemyResult = combat.AdvanceCombatTime(1000);
        Assert.Contains(enemyResult.Events, item => item.Type == CombatEventType.EnemyAction);
        Assert.Contains(enemyResult.Events, item =>
            item.Type == CombatEventType.Damage &&
            party.Any(member => member.Name == item.TargetName));
        Assert.Equal(4, combat.GetSnapshot().Party.Length);
    }

    [Fact]
    public void HiddenEnemyRemainsInSimulationButIsMarkedInvisibleForPresentation()
    {
        Character party = CreateRosterCharacter("party-hidden-test", "Scout", 15, 0, 500);
        Character hiddenEnemy = CreateRosterCharacter("hidden-enemy", "Hidden Enemy", 5, 100, 40);
        CombatActionDefinition[] enemyActions = TestCharacterFactory.CreateEnemyCombatActions();
        CombatService combat = new CombatService(
            new[]
            {
                new CombatantDefinition(party, CombatantSide.Party, TestCharacterFactory.CreatePlayerCombatActions()),
                new CombatantDefinition(hiddenEnemy, CombatantSide.Enemy, enemyActions, false)
            },
            new Random(3));

        StartAtPlayerDecision(combat);
        Assert.True(party.Health < party.MaxHealth);

        CombatSnapshot snapshot = combat.GetSnapshot();
        Assert.False(Assert.Single(snapshot.Enemies).IsVisible);
        TimelineCombatantStatus hiddenEntry = Assert.Single(snapshot.Timeline.Where(entry => entry.Id == hiddenEnemy.Id));
        Assert.False(hiddenEntry.IsVisible);
    }

    private static void StartAtPlayerDecision(CombatService combat)
    {
        Assert.True(combat.TryStartEncounter(out string reason), reason);
        combat.AdvanceCombatTime(1000);
        Assert.True(combat.IsAwaitingPlayerDecision);
    }

    private static Character CreateRosterCharacter(string id, string name, int luck, int agility, int health)
    {
        CharacterData data = new CharacterData(
            id,
            name,
            "Encounter test combatant",
            1,
            health,
            100,
            10,
            100,
            health,
            0,
            10,
            50,
            0,
            new CharacterStats(20, agility, 20, 20, 20, luck));
        return new Character(data);
    }
}
