using System;
using System.Collections.Generic;

/// <summary>Retrieves npc interaction information from the current game state.</summary>
public sealed class NpcInteractionQuery
{
    private readonly NpcInteractionCatalog _catalog;

    /// <summary>Initializes a new instance of <see cref="NpcInteractionQuery"/>.</summary>
    /// <param name="catalog">The catalog used to initialize the instance.</param>
    public NpcInteractionQuery(NpcInteractionCatalog catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    /// <summary>Returns the interactions or presence records matching the supplied criteria.</summary>
    /// <param name="characterId">The character id used by this operation.</param>
    /// <param name="locationId">The location id used by this operation.</param>
    /// <returns>The resulting i read only list available interaction.</returns>
    public IReadOnlyList<AvailableInteraction> Query(string characterId, string locationId)
    {
        IReadOnlyList<NpcInteractionDefinition> definitions = _catalog.GetForCharacter(characterId);
        List<AvailableInteraction> interactions = new List<AvailableInteraction>();

        foreach (NpcInteractionDefinition definition in definitions)
        {
            if (!definition.IsAvailableAt(locationId))
            {
                continue;
            }

            interactions.Add(new AvailableInteraction(
                InteractionSourceKind.Npc,
                characterId,
                definition.Action.Id,
                string.Empty,
                definition.Action));
        }

        return interactions;
    }
}
