using System;

/// <summary>Coordinates Combat use cases with player-facing navigation and presentation updates.</summary>
public sealed class CombatFlowController
{
    private readonly StoryPanel _storyPanel;
    private readonly CombatPresentationOrchestrator _presentation;
    private readonly CombatSessionService _combatSession;

    /// <summary>Creates the Combat flow controller with its navigation surface, presentation orchestrator and session service.</summary>
    /// <param name="storyPanel">Displays validation feedback when an encounter cannot be started.</param>
    /// <param name="presentation">Owns Combat UI mode changes and mapping/rendering of snapshots and results.</param>
    /// <param name="combatSession">Coordinates entry-action execution, Domain combat and encounter completion costs.</param>
    public CombatFlowController(
        StoryPanel storyPanel,
        CombatPresentationOrchestrator presentation,
        CombatSessionService combatSession)
    {
        _storyPanel = storyPanel ?? throw new ArgumentNullException(nameof(storyPanel));
        _presentation = presentation ?? throw new ArgumentNullException(nameof(presentation));
        _combatSession = combatSession ?? throw new ArgumentNullException(nameof(combatSession));
    }

    /// <summary>Starts an encounter through the session service and displays its initial snapshot when startup succeeds.</summary>
    /// <param name="action">The authored entry action to validate and execute before Combat begins.</param>
    /// <param name="title">The heading shown by the Combat presentation.</param>
    /// <param name="openingMessage">The initial system message added to the Combat presentation.</param>
    /// <returns><see langword="true"/> when the entry action and Combat startup both succeed; otherwise, <see langword="false"/>.</returns>
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

    /// <summary>Advances the active encounter unless Combat is over or waiting for a player decision, then presents new events or updates the Timeline.</summary>
    /// <param name="elapsedCombatTime">The elapsed frame time in seconds to pass to the shared Combat clock.</param>
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

    /// <summary>Submits the selected player action to the encounter session and refreshes the resulting presentation.</summary>
    /// <param name="request">The selected action, acting Party member and target IDs supplied by the Combat UI.</param>
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

    /// <summary>Returns to Exploration after the encounter outcome has been acknowledged and invokes the supplied Location refresh callback.</summary>
    /// <param name="showLocation">Refreshes the current Location after the Combat panel is closed.</param>
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
