using System;
using System.Collections.Generic;

public sealed class NpcInteractionQuery
{
    private readonly NpcInteractionCatalog _catalog;

    public NpcInteractionQuery(NpcInteractionCatalog catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

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
