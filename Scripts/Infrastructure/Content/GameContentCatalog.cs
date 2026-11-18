using System;
using System.Collections.Generic;

public sealed class GameContentCatalog
{
    private readonly AcademyContent _academy;

    public CharacterData Player { get; }
    public CharacterData Goblin { get; }
    public CombatActionCatalog CombatActions { get; }

    public IEnumerable<LocationContent> Locations => _academy.All;
    public ActionDefinition Travel => _academy.Travel;
    public ActionDefinition CombatEncounter => _academy.CombatEncounter;

    public GameContentCatalog(
        AcademyContent academy,
        CharacterData player,
        CharacterData goblin,
        CombatActionCatalog combatActions)
    {
        _academy = academy ?? throw new ArgumentNullException(nameof(academy));
        Player = player ?? throw new ArgumentNullException(nameof(player));
        Goblin = goblin ?? throw new ArgumentNullException(nameof(goblin));
        CombatActions = combatActions ?? throw new ArgumentNullException(nameof(combatActions));

        // Resolve references during loading so invalid content fails before gameplay starts.
        CombatActions.Resolve(Player.CombatActionIds);
        CombatActions.Resolve(Goblin.CombatActionIds);
    }

    public LocationContent GetLocation(string id)
    {
        return _academy.Get(id);
    }

    public CombatActionDefinition[] GetPlayerCombatActions()
    {
        return CombatActions.Resolve(Player.CombatActionIds);
    }
}
