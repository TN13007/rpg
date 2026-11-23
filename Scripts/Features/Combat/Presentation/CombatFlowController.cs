using System;

/// <summary>Coordinates combat session calls with the encounter presentation and exploration navigation.</summary>
public sealed class CombatFlowController
{
    private readonly StoryPanel _storyPanel;
    private readonly CombatPresentationOrchestrator _presentation;
    private readonly CombatSessionService _combatSession;

    /// <summary>Creates the Combat flow with its navigation surface, presentation orchestrator and session service.</summary>
    public CombatFlowController(
        StoryPanel storyPanel,
        CombatPresentationOrchestrator presentation,
        CombatSessionService combatSession)
    {
        _storyPanel = storyPanel ?? throw new ArgumentNullException(nameof(storyPanel));
        _presentation = presentation ?? throw new ArgumentNullException(nameof(presentation));
        _combatSession = combatSession ?? throw new ArgumentNullException(nameof(combatSession));
    }

    /// <summary>Starts an encounter through the session service and presents its initial snapshot.</summary>
    public bool StartEncounter(ActionDefinition action, string title, string openingMessage)
    {
        if (!_combatSession.TryStartEncounter(action, out string failureReason))
        {
            _storyPanel.ShowFeedback(failureReason);
            return false;
        }

        _presentation.BeginEncounter(title, _combatSession.GetSnapshot(), openingMessage);
        return true;
    }

    /// <summary>Submits one Party action and presents the resolved result.</summary>
    public void HandleCombatAction(CombatActionRequest request)
    {
        if (_combatSession.IsEncounterOver)
        {
            return;
        }

        CombatResult result = _combatSession.ExecuteAction(request);
        PresentResult(result);
    }

    /// <summary>Ends the Player Turn, resolves the Enemy Turn and presents its ordered events.</summary>
    public void HandleEndPlayerTurn()
    {
        if (_combatSession.IsEncounterOver)
        {
            return;
        }

        CombatResult result = _combatSession.EndPlayerTurn();
        PresentResult(result);
    }

    /// <summary>Returns to Exploration after the encounter outcome has been acknowledged.</summary>
    public void ReturnToExploration(Action showLocation)
    {
        if (showLocation == null)
        {
            throw new ArgumentNullException(nameof(showLocation));
        }

        _presentation.EndEncounter();
        showLocation();

        if (_combatSession.TryConsumeTimeAdvance(
            out DateOnly startingDate,
            out int startDay,
            out int startMinutes,
            out int endDay,
            out int endMinutes))
        {
            _storyPanel.ShowTimeAdvanceFeedback(
                startDay,
                startMinutes,
                endDay,
                endMinutes,
                startingDate);
        }
    }

    private void PresentResult(CombatResult result)
    {
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
}