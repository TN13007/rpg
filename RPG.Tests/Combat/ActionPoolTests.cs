using System;
using Xunit;

public sealed class ActionPoolTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void NewPoolStartsWithItsFullCapacity(int capacity)
    {
        ActionPool pool = new ActionPool(capacity);

        Assert.Equal(capacity, pool.ActionCapacity);
        Assert.Equal(capacity, pool.CurrentActionPoints);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void CapacityOutsideSupportedRangeIsRejected(int capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ActionPool(capacity));
    }

    [Fact]
    public void SuccessfulSpendReducesTheSharedPool()
    {
        ActionPool pool = new ActionPool(3);

        bool spent = pool.TrySpend(2);

        Assert.True(spent);
        Assert.Equal(1, pool.CurrentActionPoints);
        Assert.Equal(3, pool.ActionCapacity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonPositiveSpendCostIsRejected(int cost)
    {
        ActionPool pool = new ActionPool(2);

        Assert.Throws<ArgumentOutOfRangeException>(() => pool.TrySpend(cost));
        Assert.Equal(2, pool.CurrentActionPoints);
    }

    [Fact]
    public void InsufficientPointsRejectSpendWithoutChangingThePool()
    {
        ActionPool pool = new ActionPool(2);
        Assert.True(pool.TrySpend(1));

        bool spent = pool.TrySpend(2);

        Assert.False(spent);
        Assert.Equal(1, pool.CurrentActionPoints);
    }

    [Fact]
    public void BeginPlayerTurnRestoresThePoolToItsCapacity()
    {
        ActionPool pool = new ActionPool(4);
        Assert.True(pool.TrySpend(3));

        pool.BeginPlayerTurn();

        Assert.Equal(4, pool.CurrentActionPoints);
    }

    [Fact]
    public void QueuedPenaltyReducesTheNextPlayerTurnOnly()
    {
        ActionPool pool = new ActionPool(3);
        Assert.True(pool.TrySpend(2));

        pool.QueueNextPlayerTurnPenalty();
        pool.BeginPlayerTurn();

        Assert.Equal(2, pool.CurrentActionPoints);

        pool.BeginPlayerTurn();

        Assert.Equal(3, pool.CurrentActionPoints);
    }

    [Fact]
    public void MultipleQueuedPenaltiesDoNotStack()
    {
        ActionPool pool = new ActionPool(4);

        pool.QueueNextPlayerTurnPenalty();
        pool.QueueNextPlayerTurnPenalty();
        pool.QueueNextPlayerTurnPenalty();
        pool.BeginPlayerTurn();

        Assert.Equal(3, pool.CurrentActionPoints);
    }

    [Fact]
    public void QueuedPenaltyNeverReducesThePoolBelowOnePoint()
    {
        ActionPool pool = new ActionPool(1);

        pool.QueueNextPlayerTurnPenalty();
        pool.BeginPlayerTurn();

        Assert.Equal(1, pool.CurrentActionPoints);
    }
}
