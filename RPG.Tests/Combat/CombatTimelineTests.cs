using System;
using Xunit;

public sealed class CombatTimelineTests
{
    [Fact]
    public void AllCombatantsProgressOverTheSameElapsedCombatTime()
    {
        CombatTimeline timeline = new CombatTimeline();
        timeline.AddCombatant("fast", 10, 2, 10);
        timeline.AddCombatant("slow", 5, 1, 10);

        timeline.AdvanceTime(2);

        Assert.Equal(6, timeline.GetCombatant("fast").RemainingProgress);
        Assert.Equal(8, timeline.GetCombatant("slow").RemainingProgress);
        Assert.Equal(2, timeline.CombatTime);
    }

    [Fact]
    public void NextEventAdvancesToEarliestReadyCombatant()
    {
        CombatTimeline timeline = new CombatTimeline();
        timeline.AddCombatant("fast", 10, 2, 10);
        timeline.AddCombatant("slow", 5, 1, 10);

        Assert.True(timeline.AdvanceToNextEvent());

        Assert.Equal(5, timeline.CombatTime);
        Assert.Equal("fast", timeline.GetNextReadyCombatantId());
        Assert.Equal(0, timeline.GetCombatant("fast").RemainingProgress);
        Assert.Equal(5, timeline.GetCombatant("slow").RemainingProgress);
    }

    [Fact]
    public void EqualReadyTimesUseLuckThenStableOrdinalId()
    {
        CombatTimeline timeline = new CombatTimeline();
        timeline.AddCombatant("zeta", 20, 1, 10);
        timeline.AddCombatant("beta", 20, 1, 10);
        timeline.AddCombatant("alpha", 30, 1, 10);

        timeline.AdvanceToNextEvent();

        Assert.Equal("alpha", timeline.GetNextReadyCombatantId());
        timeline.ApplyRecovery("alpha", 10);
        Assert.Equal("beta", timeline.GetNextReadyCombatantId());
    }

    [Fact]
    public void StunFreezesProgressAndPreservesItUntilExpiry()
    {
        CombatTimeline timeline = new CombatTimeline();
        timeline.AddCombatant("goblin", 5, 2, 10);
        timeline.AdvanceTime(2);
        timeline.ApplyStun("goblin", 3);

        timeline.AdvanceTime(2);

        Assert.Equal(6, timeline.GetCombatant("goblin").RemainingProgress);
        Assert.True(timeline.GetCombatant("goblin").IsStunned(timeline.CombatTime));

        timeline.AdvanceToNextEvent();

        Assert.Equal(5, timeline.CombatTime);
        Assert.False(timeline.GetCombatant("goblin").IsStunned(timeline.CombatTime));
        Assert.Equal(6, timeline.GetCombatant("goblin").RemainingProgress);
    }

    [Fact]
    public void StunAppliedToReadyCombatantPreservesReadyProgress()
    {
        CombatTimeline timeline = new CombatTimeline();
        timeline.AddCombatant("goblin", 5, 1, 0);
        timeline.ApplyStun("goblin", 2);

        Assert.Equal(string.Empty, timeline.GetNextReadyCombatantId());
        timeline.AdvanceToNextEvent();

        Assert.Equal("goblin", timeline.GetNextReadyCombatantId());
        Assert.Equal(0, timeline.GetCombatant("goblin").RemainingProgress);
    }

    [Fact]
    public void RecoveryCostBecomesTheActorsNewRemainingProgress()
    {
        CombatTimeline timeline = new CombatTimeline();
        timeline.AddCombatant("player", 10, 2, 0);

        timeline.ApplyRecovery("player", 12);

        Assert.Equal(12, timeline.GetCombatant("player").RemainingProgress);
        Assert.Equal(6, timeline.GetTimeUntilNextEvent());
    }

    [Fact]
    public void InvalidSpeedProgressAndRecoveryAreRejected()
    {
        CombatTimeline timeline = new CombatTimeline();

        Assert.Throws<ArgumentOutOfRangeException>(() => timeline.AddCombatant("bad-speed", 1, 0, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => timeline.AddCombatant("bad-progress", 1, 1, -1));
        timeline.AddCombatant("valid", 1, 1, 10);
        Assert.Throws<ArgumentOutOfRangeException>(() => timeline.ApplyRecovery("valid", -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => timeline.ApplyStun("valid", 0));
    }

    [Fact]
    public void DuplicateCombatantIdsAreRejected()
    {
        CombatTimeline timeline = new CombatTimeline();
        timeline.AddCombatant("same", 1, 1, 10);

        Assert.Throws<ArgumentException>(() => timeline.AddCombatant("same", 2, 1, 10));
    }
}
