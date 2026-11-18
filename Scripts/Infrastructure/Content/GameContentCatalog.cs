using System;
using System.Collections.Generic;

public sealed class GameContentCatalog
{
    public CharacterData Player { get; }
    public CharacterData Goblin { get; }
    public CombatActionCatalog CombatActions { get; }

    public IEnumerable<LocationContent> Locations => AcademyContent.All;
    public ActionDefinition Travel => AcademyContent.Travel;
    public ActionDefinition CombatEncounter => AcademyContent.CombatEncounter;

    public GameContentCatalog(
        CharacterData player,
        CharacterData goblin,
        CombatActionCatalog combatActions)
    {
        Player = player ?? throw new ArgumentNullException(nameof(player));
        Goblin = goblin ?? throw new ArgumentNullException(nameof(goblin));
        CombatActions = combatActions ?? throw new ArgumentNullException(nameof(combatActions));

        // Resolve references during loading so invalid content fails before gameplay starts.
        CombatActions.Resolve(Player.CombatActionIds);
        CombatActions.Resolve(Goblin.CombatActionIds);
    }

    public LocationContent GetLocation(string id)
    {
        return AcademyContent.Get(id);
    }

    public CombatActionDefinition[] GetPlayerCombatActions()
    {
        return CombatActions.Resolve(Player.CombatActionIds);
    }
}
