using System;

/// <summary>Coordinates combat flow flow and user interactions.</summary>
public sealed class CombatFlowController
{
    private readonly StoryPanel _storyPanel;
    private readonly CombatPresentationOrchestrator _presentation;
    private readonly CombatSessionService _combatSession;

    /// <summary>Initializes a new instance of <see cref="CombatFlowController"/>.</summary>
    /// <param name="storyPanel">The story panel used to initialize the instance.</param>
    /// <param name="presentation">The presentation used to initialize the instance.</param>
    /// <param name="combatSession">The combat session used to initialize the instance.</param>
    public CombatFlowController(
        StoryPanel storyPanel,
        CombatPresentationOrchestrator presentation,
        CombatSessionService combatSession)
    {
        _storyPanel = storyPanel ?? throw new ArgumentNullException(nameof(storyPanel));
        _presentation = presentation ?? throw new ArgumentNullException(nameof(presentation));
        _combatSession = combatSession ?? throw new ArgumentNullException(nameof(combatSession));
    }

    /// <summary>Starts encounter and initializes its presentation flow.</summary>
    /// <param name="action">The action used by this operation.</param>
    /// <param name="title">The title used by this operation.</param>
    /// <param name="openingMessage">The opening message used by this operation.</param>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
    public bool StartEncounter(ActionDefinition action, string title, string openingMessage)
    {
        if (!_combatSession.TryStartEncounter(action, out string failureReason))
        {
            _storyPanel.ShowFeedback(failureReason);
            return false;
        }

        CombatSnapshot snapshot = _combatSession.GetSnapshot();
        _presentation.BeginEncounter(title, snapshot, openingMessage);
        return true;
    }

    /// <summary>Advances combat time by the supplied elapsed time.</summary>
    /// <param name="elapsedCombatTime">The elapsed combat time used by this operation.</param>
    public void AdvanceCombatTime(double elapsedCombatTime)
    {
        if (_combatSession.IsEncounterOver || _combatSession.IsAwaitingPlayerDecision)
        {
            return;
        }

        CombatResult result = _combatSession.AdvanceCombatTime(elapsedCombatTime);
        if (result == null)
        {
            return;
        }

        if (result.Events.Length > 0 || !string.IsNullOrWhiteSpace(result.Snapshot.ActiveCombatantId))
        {
            _presentation.Present(result);
        }
        else
        {
            _presentation.UpdateTimeline(result.Snapshot);
        }

        if (_combatSession.IsEncounterOver)
        {
            _presentation.CompleteEncounter(_combatSession.IsVictory);
        }
    }

    /// <summary>Handles combat action from the presentation layer.</summary>
    /// <param name="request">The request used by this operation.</param>
    public void HandleCombatAction(CombatActionRequest request)
    {
        if (_combatSession.IsEncounterOver)
        {
            return;
        }

        CombatResult result = _combatSession.ExecuteAction(request);
        if (result == null)
        {
            return;
        }

        _presentation.Present(result);

        if (_combatSession.IsEncounterOver)
        {
            _presentation.CompleteEncounter(_combatSession.IsVictory);
        }
    }

    /// <summary>Returns from to exploration and invokes the supplied navigation callback.</summary>
    /// <param name="showLocation">The show location used by this operation.</param>
    public void ReturnToExploration(Action showLocation)
    {
        if (showLocation == null)
        {
            throw new ArgumentNullException(nameof(showLocation));
        }

        _presentation.EndEncounter();
        showLocation();

        if (_combatSession.TryConsumeTimeAdvance(
            out int startDay,
            out int startMinutes,
            out int endDay,
            out int endMinutes))
        {
            _storyPanel.ShowTimeAdvanceFeedback(
                startDay,
                startMinutes,
                endDay,
                endMinutes);
        }
    }
}
