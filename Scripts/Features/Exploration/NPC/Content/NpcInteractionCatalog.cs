using System;
using System.Collections.Generic;

/// <summary>Provides indexed access to the configured npc interaction definitions.</summary>
public sealed class NpcInteractionCatalog
{
    private readonly List<NpcInteractionDefinition> _all;
    private readonly Dictionary<string, List<NpcInteractionDefinition>> _byCharacter;

    /// <summary>Gets the all.</summary>
    public IEnumerable<NpcInteractionDefinition> All => _all;

    /// <summary>Initializes a new instance of <see cref="NpcInteractionCatalog"/>.</summary>
    /// <param name="interactions">The interactions used to initialize the instance.</param>
    public NpcInteractionCatalog(IEnumerable<NpcInteractionDefinition> interactions)
    {
        if (interactions == null)
        {
            throw new ArgumentNullException(nameof(interactions));
        }

        _all = new List<NpcInteractionDefinition>();
        _byCharacter = new Dictionary<string, List<NpcInteractionDefinition>>(StringComparer.Ordinal);
        Dictionary<string, HashSet<string>> actionIdsByCharacter =
            new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        foreach (NpcInteractionDefinition interaction in interactions)
        {
            if (interaction == null)
            {
                throw new InvalidOperationException("NPC interaction definitions cannot contain null values.");
            }

            if (!_byCharacter.TryGetValue(interaction.CharacterId, out List<NpcInteractionDefinition> characterInteractions))
            {
                characterInteractions = new List<NpcInteractionDefinition>();
                _byCharacter.Add(interaction.CharacterId, characterInteractions);
                actionIdsByCharacter.Add(interaction.CharacterId, new HashSet<string>(StringComparer.Ordinal));
            }

            if (!actionIdsByCharacter[interaction.CharacterId].Add(interaction.Action.Id))
            {
                throw new InvalidOperationException(
                    "Duplicate NPC interaction ID '" + interaction.Action.Id
                    + "' for Character '" + interaction.CharacterId + "'.");
            }

            characterInteractions.Add(interaction);
            _all.Add(interaction);
        }
    }

    /// <summary>Returns for character.</summary>
    /// <param name="characterId">The character id used by this operation.</param>
    /// <returns>The resulting i read only list npc interaction definition.</returns>
    public IReadOnlyList<NpcInteractionDefinition> GetForCharacter(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            throw new ArgumentException("NPC interaction Character ID is required.", nameof(characterId));
        }

        if (!_byCharacter.TryGetValue(characterId, out List<NpcInteractionDefinition> interactions))
        {
            return Array.Empty<NpcInteractionDefinition>();
        }

        return interactions.ToArray();
    }
}
