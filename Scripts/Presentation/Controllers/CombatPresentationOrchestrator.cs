using System;
using System.Collections.Generic;
using System.Linq;

public sealed class CombatPresentationOrchestrator
{
    private readonly GameModeController _gameModeController;
    private readonly Sidebar _sidebar;
    private readonly CombatPanel _combatPanel;
    private readonly CharacterPanel _characterPanel;
    private readonly CombatPresentationMapper _mapper;
    private readonly Character[] _partyMembers;
    private readonly Character _primaryCharacter;

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

    public void Present(CombatResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        Present(_mapper.Map(result));
    }

    public void CompleteEncounter(bool isVictory)
    {
        _combatPanel.ShowEncounterComplete(isVictory ? "VICTORY" : "DEFEAT");
        _characterPanel.SetCharacter(new CharacterViewModel(_primaryCharacter));
    }

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

        Character activeCharacter = _partyMembers.FirstOrDefault(
            character => character.Id == viewData.ActiveCombatantId);

        if (activeCharacter == null)
        {
            activeCharacter = _primaryCharacter;
        }

        _characterPanel.SetCharacter(new CharacterViewModel(activeCharacter));
    }
}
