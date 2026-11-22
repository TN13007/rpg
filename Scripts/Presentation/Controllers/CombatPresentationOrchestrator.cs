using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Coordinates combat presentation across the panels and presentation view models.</summary>
public sealed class CombatPresentationOrchestrator
{
    private readonly GameModeController _gameModeController;
    private readonly Sidebar _sidebar;
    private readonly CombatPanel _combatPanel;
    private readonly CharacterPanel _characterPanel;
    private readonly CombatPresentationMapper _mapper;
    private readonly Character[] _partyMembers;
    private readonly Character _primaryCharacter;

    /// <summary>Creates the orchestrator with the presentation components and ordered Party roster.</summary>
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
            throw new ArgumentException("The presentation roster requires at least one valid Party member.", nameof(partyMembers));
        }

        _primaryCharacter = _partyMembers[0];
        _combatPanel.PartyMemberSelected += HandlePartyMemberSelected;
    }

    /// <summary>Enters combat mode, shows the initial statuses and adds the opening message.</summary>
    public void BeginEncounter(string title, CombatSnapshot snapshot, string openingMessage)
    {
        if (snapshot == null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        _gameModeController.ShowCombat();
        _combatPanel.BeginEncounter(title);
        CombatViewData initialViewData = _mapper.Map(snapshot);
        _sidebar.EnterCombat(initialViewData.Enemies);

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

        Present(initialViewData);
    }

    /// <summary>Maps and presents the latest Domain result, including its ordered events.</summary>
    public void Present(CombatResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        Present(_mapper.Map(result));
    }

    /// <summary>Displays the terminal outcome after the Domain encounter has ended.</summary>
    public void CompleteEncounter(bool isVictory)
    {
        _combatPanel.ShowEncounterComplete(isVictory ? "VICTORY" : "DEFEAT");
        _characterPanel.SetCharacter(new CharacterViewModel(_primaryCharacter));
    }

    /// <summary>Leaves combat mode and restores the primary Party member in the character panel.</summary>
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
                CombatFeedbackType targetFeedback = feedback.Type == CombatFeedbackType.Hit
                    ? CombatFeedbackType.Hit
                    : feedback.Type;
                _combatPanel.PlayPartyFeedback(feedback.TargetName, targetFeedback);
                _sidebar.PlayEnemyFeedback(feedback.TargetName, targetFeedback);
            }
        }

    }

    private void HandlePartyMemberSelected(string characterId)
    {
        Character selectedCharacter = _partyMembers.FirstOrDefault(
            character => character.Id == characterId);

        _characterPanel.SetCharacter(
            new CharacterViewModel(selectedCharacter ?? _primaryCharacter));
    }
}