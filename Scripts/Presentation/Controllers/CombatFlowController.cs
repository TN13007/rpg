using System;

public sealed class CombatFlowController
{
    private readonly StoryPanel _storyPanel;
    private readonly CombatPresentationOrchestrator _presentation;
    private readonly CombatSessionService _combatSession;

    public CombatFlowController(
        StoryPanel storyPanel,
        CombatPresentationOrchestrator presentation,
        CombatSessionService combatSession)
    {
        _storyPanel = storyPanel ?? throw new ArgumentNullException(nameof(storyPanel));
        _presentation = presentation ?? throw new ArgumentNullException(nameof(presentation));
        _combatSession = combatSession ?? throw new ArgumentNullException(nameof(combatSession));
    }

    public bool StartGoblinEncounter(ActionDefinition action)
    {
        return StartEncounter(
            action,
            "Goblin Ambush",
            "A goblin jumps out from the bushes and blocks your way.");
    }

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
