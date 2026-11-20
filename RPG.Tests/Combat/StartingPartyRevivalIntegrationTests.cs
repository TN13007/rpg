using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

public sealed class StartingPartyRevivalIntegrationTests
{
    [Fact]
    public void OneDownedStartingMemberLeavesThreeAlliedMembersAbleToTakeTheirTurns()
    {
        EncounterContext context = CreateEncounter();
        context.Ari.TakeDamage(context.Ari.Health);

        Assert.True(context.Combat.TryStartEncounter(out string failureReason), failureReason);
        CombatSnapshot openingSnapshot = context.Combat.GetSnapshot();

        Assert.Equal(4, openingSnapshot.Party.Length);
        Assert.Equal(
            "Downed",
            openingSnapshot.Party.Single(member => member.Id == context.Ari.Id).Condition);
        Assert.Equal(3, openingSnapshot.Timeline.Count(entry => !entry.IsEnemy));

        string[] expectedActorIds = { "sera", "mira", "bram" };
        foreach (string expectedActorId in expectedActorIds)
        {
            AdvanceUntilPlayerDecision(context.Combat);
            Assert.Equal(expectedActorId, context.Combat.ActiveCombatantId);

            CombatResult action = ExecuteAvailableAttack(context);
            Assert.True(action.Success, action.FailureReason);
        }

        Assert.False(context.Combat.IsEncounterOver);
        Assert.Equal(
            3,
            context.Combat.GetSnapshot().Timeline.Count(entry => !entry.IsEnemy));
    }

    [Fact]
    public void StartingPartyCanReviveADownedMemberAndBothActorsReceiveRecovery()
    {
        EncounterContext context = CreateEncounter();
        context.Ari.TakeDamage(context.Ari.Health);

        StartAtPlayerDecision(context.Combat);
        Assert.Equal("sera", context.Combat.ActiveCombatantId);

        CombatActionOption reviveOption = Assert.Single(
            context.Combat.GetSnapshot().AvailableActions.Where(option =>
                option.Id == "revive_ally" &&
                option.TargetId == context.Ari.Id));

        Assert.True(reviveOption.IsAvailable);

        CombatResult reviveResult = context.Combat.ExecuteAction(
            new CombatActionRequest(
                reviveOption.Id,
                context.Ari.Id,
                context.Combat.ActiveCombatantId));

        Assert.True(reviveResult.Success, reviveResult.FailureReason);
        Assert.Equal(5, context.Ari.Health);
        Assert.Equal(5, context.Combat.PartyMembers.Single(member => member.Id == "sera").Stamina);
        Assert.False(context.Combat.IsAwaitingPlayerDecision);

        CombatSnapshot snapshot = context.Combat.GetSnapshot();
        TimelineCombatantStatus revivedMember = Assert.Single(
            snapshot.Timeline.Where(entry => entry.Id == context.Ari.Id));
        TimelineCombatantStatus revivingMember = Assert.Single(
            snapshot.Timeline.Where(entry => entry.Id == "sera"));

        Assert.True(revivedMember.RemainingProgress > 0);
        Assert.False(revivedMember.IsReady);
        Assert.True(revivingMember.RemainingProgress > 0);
        Assert.False(revivingMember.IsReady);
        Assert.Equal("Wounded", snapshot.Party.Single(member => member.Id == context.Ari.Id).Condition);
    }

    [Fact]
    public void EnemyTargetsTheFirstLivingPartyMemberAndNeverTargetsADownedMember()
    {
        EncounterContext context = CreateEncounter();
        context.Ari.TakeDamage(context.Ari.Health);
        StartAtPlayerDecision(context.Combat);

        CombatEvent[] events = AdvanceUntilFirstEnemyAction(context);
        CombatEvent enemyAction = Assert.Single(
            events.Where(item => item.Type == CombatEventType.EnemyAction));

        Assert.Equal("Bram", enemyAction.TargetName);
        Assert.Equal(0, context.Ari.Health);
        Assert.DoesNotContain(
            context.Combat.GetSnapshot().Timeline,
            entry => entry.Id == context.Ari.Id);
    }

    [Fact]
    public void DefeatOccursWhenTheOnlyRemainingStartingMemberIsDowned()
    {
        EncounterContext context = CreateEncounter(useLethalEnemy: true);
        Character bram = context.Combat.PartyMembers.Single(member => member.Id == "bram");
        Character mira = context.Combat.PartyMembers.Single(member => member.Id == "mira");
        Character sera = context.Combat.PartyMembers.Single(member => member.Id == "sera");

        context.Ari.TakeDamage(context.Ari.Health);
        bram.TakeDamage(bram.Health);
        mira.TakeDamage(mira.Health);
        sera.TakeDamage(sera.Health - 1);

        StartAtPlayerDecision(context.Combat);
        Assert.Equal("sera", context.Combat.ActiveCombatantId);

        CombatResult playerAction = ExecuteAvailableAttack(context);
        Assert.True(playerAction.Success, playerAction.FailureReason);

        AdvanceUntilEncounterOver(context.Combat);

        Assert.True(context.Combat.IsEncounterOver);
        Assert.False(context.Combat.IsVictory);
        Assert.All(
            context.Combat.GetSnapshot().Party,
            member => Assert.Equal("Downed", member.Condition));
        Assert.DoesNotContain(
            context.Combat.GetSnapshot().Timeline,
            entry => !entry.IsEnemy);
        Assert.Empty(context.Combat.GetSnapshot().AvailableActions);
    }

    [Fact]
    public void RevivalAllowanceResetsWhenTheSameFourPersonEncounterRestarts()
    {
        EncounterContext context = CreateEncounter(useLethalEnemy: true);
        context.Ari.TakeDamage(context.Ari.Health);

        StartAtPlayerDecision(context.Combat);
        Assert.Equal("sera", context.Combat.ActiveCombatantId);

        // Keep the first encounter alive long enough to perform the revival,
        // then let the next party member finish the nearly defeated enemy.
        context.Enemy.TakeDamage(context.Enemy.Health - 1);

        CombatResult firstRevive = context.Combat.ExecuteAction(
            new CombatActionRequest("revive_ally", context.Ari.Id, "sera"));
        Assert.True(firstRevive.Success, firstRevive.FailureReason);
        Assert.Equal(5, context.Ari.Health);

        AdvanceUntilPlayerDecision(context.Combat);
        CombatResult firstVictoryAttack = ExecuteAvailableAttack(context);
        Assert.True(firstVictoryAttack.Success, firstVictoryAttack.FailureReason);
        Assert.True(context.Combat.IsVictory);
        Assert.Equal(45, context.Ari.Gold);

        // HP persists between encounters; the once-per-encounter revival flag does not.
        context.Ari.TakeDamage(context.Ari.Health - 1);
        Assert.Equal(1, context.Ari.Health);
        Assert.True(context.Combat.TryStartEncounter(out string restartFailure), restartFailure);

        CombatEvent[] secondEncounterEvents = AdvanceUntilFirstEnemyAction(context);
        CombatEvent secondEnemyAction = Assert.Single(
            secondEncounterEvents.Where(item => item.Type == CombatEventType.EnemyAction));

        Assert.Equal("Ari", secondEnemyAction.TargetName);
        Assert.Equal(0, context.Ari.Health);
        Assert.Equal(
            "Downed",
            context.Combat.GetSnapshot().Party.Single(member => member.Id == context.Ari.Id).Condition);

        AdvanceUntilPlayerDecision(context.Combat);
        CombatActionOption secondEncounterRevive = Assert.Single(
            context.Combat.GetSnapshot().AvailableActions.Where(option =>
                option.Id == "revive_ally" &&
                option.TargetId == context.Ari.Id));

        CombatResult secondRevive = context.Combat.ExecuteAction(
            new CombatActionRequest(
                secondEncounterRevive.Id,
                context.Ari.Id,
                context.Combat.ActiveCombatantId));

        Assert.True(secondRevive.Success, secondRevive.FailureReason);
        Assert.Equal(5, context.Ari.Health);
    }

    private static void StartAtPlayerDecision(CombatService combat)
    {
        Assert.True(combat.TryStartEncounter(out string failureReason), failureReason);
        AdvanceUntilPlayerDecision(combat);
        Assert.True(combat.IsAwaitingPlayerDecision);
    }

    private static void AdvanceUntilPlayerDecision(CombatService combat)
    {
        for (int frame = 0;
             frame < 10000 && !combat.IsAwaitingPlayerDecision && !combat.IsEncounterOver;
             frame++)
        {
            combat.AdvanceCombatTime(0.1);
        }

        Assert.True(combat.IsAwaitingPlayerDecision || combat.IsEncounterOver);
        Assert.False(combat.IsEncounterOver);
    }

    private static CombatEvent[] AdvanceUntilFirstEnemyAction(EncounterContext context)
    {
        List<CombatEvent> events = new List<CombatEvent>();

        for (int frame = 0;
             frame < 10000 &&
             !events.Any(item => item.Type == CombatEventType.EnemyAction) &&
             !context.Combat.IsEncounterOver;
             frame++)
        {
            if (context.Combat.IsAwaitingPlayerDecision)
            {
                CombatResult playerAction = ExecuteAvailableAttack(context);
                Assert.True(playerAction.Success, playerAction.FailureReason);
                events.AddRange(playerAction.Events);
            }
            else
            {
                CombatResult timeAdvance = context.Combat.AdvanceCombatTime(0.1);
                events.AddRange(timeAdvance.Events);
            }
        }

        Assert.Contains(events, item => item.Type == CombatEventType.EnemyAction);
        return events.ToArray();
    }

    private static void AdvanceUntilEncounterOver(CombatService combat)
    {
        for (int frame = 0; frame < 10000 && !combat.IsEncounterOver; frame++)
        {
            combat.AdvanceCombatTime(0.1);
        }

        Assert.True(combat.IsEncounterOver);
    }

    private static CombatResult ExecuteAvailableAttack(EncounterContext context)
    {
        CombatActionOption action = context.Combat.GetSnapshot().AvailableActions
            .Where(option =>
                option.IsAvailable &&
                option.TargetId == context.Enemy.Id &&
                (option.Id == "quick_strike" || option.Id == "heavy_strike"))
            .OrderBy(option => option.Id == "quick_strike" ? 0 : 1)
            .First();

        return context.Combat.ExecuteAction(new CombatActionRequest(
            action.Id,
            context.Enemy.Id,
            context.Combat.ActiveCombatantId));
    }

    private static EncounterContext CreateEncounter(bool useLethalEnemy = false)
    {
        GameContentCatalog content = LoadContent();
        Character ari = new Character(content.GetCharacter("player-ari"));
        CombatantDefinition[] partyDefinitions = StartingParty.CreateCombatants(content, ari);
        Character enemy = CreateEnemy();

        CombatActionDefinition[] enemyActions = useLethalEnemy
            ? new[] { CreateLethalAttack() }
            : content.GetCombatActionsForCharacter("goblin-a");

        List<CombatantDefinition> encounter = new List<CombatantDefinition>(partyDefinitions)
        {
            new CombatantDefinition(enemy, CombatantSide.Enemy, enemyActions)
        };

        return new EncounterContext(
            ari,
            enemy,
            partyDefinitions.Select(definition => definition.Character).ToArray(),
            new CombatService(encounter, new FirstIndexRandom()));
    }

    private static Character CreateEnemy()
    {
        CharacterData data = new CharacterData(
            "integration-goblin",
            "Goblin",
            "Starting-party integration test enemy",
            1,
            500,
            0,
            0,
            0,
            500,
            0,
            0,
            0,
            0,
            new CharacterStats(12, 8, 3, 10, 3, 5));

        return new Character(data);
    }

    private static CombatActionDefinition CreateLethalAttack()
    {
        return new CombatActionDefinition(
            "integration_lethal_attack",
            "Lethal Attack",
            "Enemy Action",
            "A deterministic test attack.",
            CombatActionBehavior.Attack,
            0,
            10,
            0,
            string.Empty,
            "The enemy strikes.",
            130);
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
            new AcademyContent(),
            new CharacterCatalog(characters),
            new CombatActionCatalog(TestCharacterFactory.CreateCombatActions()));
    }

    private sealed class EncounterContext
    {
        public Character Ari { get; }
        public Character Enemy { get; }
        public Character[] Party { get; }
        public CombatService Combat { get; }

        public EncounterContext(
            Character ari,
            Character enemy,
            Character[] party,
            CombatService combat)
        {
            Ari = ari;
            Enemy = enemy;
            Party = party;
            Combat = combat;
        }
    }

    private sealed class FirstIndexRandom : Random
    {
        public override int Next(int maxValue)
        {
            return 0;
        }
    }
}
