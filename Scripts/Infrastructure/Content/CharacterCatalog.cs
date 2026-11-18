using System;
using System.Collections.Generic;

public sealed class CharacterCatalog
{
    private readonly Dictionary<string, CharacterData> _characters;

    public IEnumerable<CharacterData> All => _characters.Values;

    public CharacterCatalog(IEnumerable<CharacterData> characters)
    {
        if (characters == null) throw new ArgumentNullException(nameof(characters));

        _characters = new Dictionary<string, CharacterData>(StringComparer.Ordinal);
        foreach (CharacterData character in characters)
        {
            if (character == null) throw new InvalidOperationException("Character definitions cannot contain null values.");
            if (!_characters.TryAdd(character.Id, character))
            {
                throw new InvalidOperationException("Duplicate character ID: " + character.Id);
            }
        }
    }

    public CharacterData Get(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Character ID is required.", nameof(id));
        if (!_characters.TryGetValue(id, out CharacterData character))
        {
            throw new KeyNotFoundException("Character was not found: " + id);
        }
        return character;
    }
}
