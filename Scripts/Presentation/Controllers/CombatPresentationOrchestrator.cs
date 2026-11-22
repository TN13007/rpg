using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Coordinates combat presentation across the panels and view models.</summary>
public sealed class CombatPresentationOrchestrator
{
    private readonly GameModeController _gameModeController;
    private readonly Sidebar _sidebar;
    private readonly CombatPanel _combatPanel;
    private readonly CharacterPanel _characterPanel;
    private readonly CombatPresentationMapper _mapper;
    private readonly Character[] _partyMembers;
    private readonly Character _primaryCharacter;

    /// <summary>Initializes a new instance of <see cref="CombatPresentationOrchestrator"/>.</summary>
    /// <param name="gameModeController">The game mode controller used to initialize the instance.</param>
    /// <param name="sidebar">The sidebar used to initialize the instance.</param>
    /// <param name="combatPanel">The combat panel used to initialize the instance.</param>
    /// <param name="characterPanel">The character panel used to initialize the instance.</param>
    /// <param name="mapper">The mapper used to initialize the instance.</param>
    /// <param name="partyMembers">The party members used to initialize the instance.</param>
    public CombatPresentationOrchestrator(
        GameModeController gameModeController,
        Sidebar sidebar,
        CombatPanel combatPanel,
        CharacterPanel characterPanel,
        CombatPresentationMapper mapper,
        IEnumerable<Character> partyMembers)
    {
        _gameModeController = gameModeController ?? throw new ArgumentNullException(nameof(gameModeController));
        _sidebar = sidebar ?? throw new ArgumentNullException(nameof(sidebar));
        _combatPanel = combatPanel ?? throw new ArgumentNullException(nameof(combatPanel));
        _characterPanel = characterPanel ?? throw new ArgumentNullException(nameof(characterPanel));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        if (partyMembers == null)
        {
            throw new ArgumentNullException(nameof(partyMembers));
        }

        _partyMembers = partyMembers.ToArray();
        if (_partyMembers.Length == 0 || _partyMembers.Any(character => character == null))
        {
            throw new ArgumentException("The presentation roster requires at least one valid party member.", nameof(partyMembers));
        }

        _primaryCharacter = _partyMembers[0];
    }

    /// <summary>Begins encounter and initializes the corresponding UI state.</summary>
    /// <param name="title">The title used by this operation.</param>
    /// <param name="snapshot">The snapshot used by this operation.</param>
    /// <param name="openingMessage">The opening message used by this operation.</param>
    public void BeginEncounter(string title, CombatSnapshot snapshot, string openingMessage)
    {
        if (snapshot == null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        _gameModeController.ShowCombat();
        _combatPanel.BeginEncounter(title);
        _sidebar.EnterCombat(snapshot.Enemies);

        if (!string.IsNullOrWhiteSpace(openingMessage))
        {
            _combatPanel.AppendMessage(new CombatMessage(
                Guid.NewGuid().ToString("N"),
                snapshot.Round,
                CombatMessageType.System,
                CombatMessageCategory.Environment,
                "SYSTEM",
                openingMessage));
        }

        Present(_mapper.Map(snapshot));
    }

    /// <summary>Presents the latest combat result and its feedback.</summary>
    /// <param name="result">The result used by this operation.</param>
    public void Present(CombatResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        Present(_mapper.Map(result));
    }

    /// <summary>Updates timeline to reflect the latest game state.</summary>
    /// <param name="snapshot">The snapshot used by this operation.</param>
    public void UpdateTimeline(CombatSnapshot snapshot)
    {
        if (snapshot == null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        CombatViewData viewData = _mapper.Map(snapshot);
        _combatPanel.SetTimeline(viewData.Timeline);
    }

    /// <summary>Presents the completion state for encounter.</summary>
    /// <param name="isVictory">The is victory used by this operation.</param>
    public void CompleteEncounter(bool isVictory)
    {
        _combatPanel.ShowEncounterComplete(isVictory ? "VICTORY" : "DEFEAT");
        _characterPanel.SetCharacter(new CharacterViewModel(_primaryCharacter));
    }

    /// <summary>Ends encounter and clears its presentation state.</summary>
    public void EndEncounter()
    {
        _sidebar.ExitCombat();
        _gameModeController.ShowExploration();
        _characterPanel.SetCharacter(new CharacterViewModel(_primaryCharacter));
    }

    private void Present(CombatViewData viewData)
    {
        _sidebar.UpdateCombatEnemies(viewData.Enemies);
        _combatPanel.ApplyViewData(viewData);
        foreach (CombatFeedbackViewData feedback in viewData.Feedback)
        {
            if (!string.IsNullOrWhiteSpace(feedback.ActorName))
            {
                _combatPanel.PlayPartyFeedback(feedback.ActorName, feedback.Type);
                _sidebar.PlayEnemyFeedback(feedback.ActorName, feedback.Type);
            }

            if (!string.IsNullOrWhiteSpace(feedback.TargetName))
            {
                _combatPanel.PlayPartyFeedback(feedback.TargetName, feedback.Type == CombatFeedbackType.Hit
                    ? CombatFeedbackType.Hit
                    : feedback.Type);
                _sidebar.PlayEnemyFeedback(feedback.TargetName, feedback.Type == CombatFeedbackType.Hit
                    ? CombatFeedbackType.Hit
                    : feedback.Type);
            }
        }

        Character activeCharacter = _partyMembers.FirstOrDefault(
            character => character.Id == viewData.ActiveCombatantId);

        if (activeCharacter == null)
        {
            activeCharacter = _primaryCharacter;
        }

        _characterPanel.SetCharacter(new CharacterViewModel(activeCharacter));
    }
}
