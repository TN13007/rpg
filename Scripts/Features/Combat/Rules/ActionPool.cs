using System;

/// <summary>Owns the Party's shared Action Point capacity and current turn pool.</summary>
public sealed class ActionPool
{
    private const int _minimumActionCapacity = 1;
    private const int _maximumActionCapacity = 4;

    private bool _nextPlayerTurnPenaltyPending;

    /// <summary>Gets the maximum number of Action Points available to the Party each Player Turn.</summary>
    public int ActionCapacity { get; }

    /// <summary>Gets the number of Action Points currently available to the Party.</summary>
    public int CurrentActionPoints { get; private set; }

    /// <summary>Initializes a full Action Point pool with the supplied Party capacity.</summary>
    /// <param name="actionCapacity">The Party's Action Point capacity, from 1 through 4.</param>
    /// <exception cref="ArgumentOutOfRangeException">The capacity is outside the supported range.</exception>
    public ActionPool(int actionCapacity)
    {
        if (actionCapacity < _minimumActionCapacity || actionCapacity > _maximumActionCapacity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(actionCapacity),
                "Action Capacity must be between 1 and 4.");
        }

        ActionCapacity = actionCapacity;
        CurrentActionPoints = actionCapacity;
    }

    /// <summary>Attempts to spend Action Points from the shared Party pool.</summary>
    /// <param name="actionPointCost">The positive number of Action Points to spend.</param>
    /// <returns><see langword="true"/> when the cost is paid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The cost is zero or negative.</exception>
    public bool TrySpend(int actionPointCost)
    {
        if (actionPointCost <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(actionPointCost),
                "Action Point cost must be greater than zero.");
        }

        if (CurrentActionPoints < actionPointCost)
        {
            return false;
        }

        CurrentActionPoints -= actionPointCost;
        return true;
    }

    /// <summary>Queues the one-Action-Point penalty for the next Player Turn.</summary>
    /// <remarks>Repeated calls before the next Player Turn do not increase the penalty.</remarks>
    public void QueueNextPlayerTurnPenalty()
    {
        _nextPlayerTurnPenaltyPending = true;
    }

    /// <summary>Starts a Player Turn by refreshing the pool and consuming any pending control penalty.</summary>
    /// <remarks>The penalty reduces the refreshed pool by one, never below one Action Point, and is consumed once.</remarks>
    public void BeginPlayerTurn()
    {
        CurrentActionPoints = ActionCapacity;

        if (!_nextPlayerTurnPenaltyPending)
        {
            return;
        }

        CurrentActionPoints = Math.Max(1, CurrentActionPoints - 1);
        _nextPlayerTurnPenaltyPending = false;
    }
}
