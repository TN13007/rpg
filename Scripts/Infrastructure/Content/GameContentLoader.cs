using System;
using System.Collections.Generic;
using Godot;

/// <summary>Loads game content data from files or serialized content.</summary>
public static class GameContentLoader
{
    /// <summary>Loads load from the specified content source.</summary>
    /// <param name="dataRoot">The data root used by this operation.</param>
    /// <returns>The resulting game content catalog.</returns>
    public static GameContentCatalog Load(string dataRoot)
    {
        if (string.IsNullOrWhiteSpace(dataRoot))
        {
            throw new ArgumentException("Game data root is required.", nameof(dataRoot));
        }

        string normalizedRoot = dataRoot.TrimEnd('/');
        LocationCatalog locations = new LocationCatalog();
        locations.Load(normalizedRoot + "/Locations.json");

        string charactersPath = normalizedRoot + "/Characters";
        DirAccess characterDirectory = DirAccess.Open(charactersPath);
        if (characterDirectory == null)
        {
            throw new InvalidOperationException("Character data directory was not found: " + charactersPath);
        }

        List<CharacterData> characters = new List<CharacterData>();
        foreach (string fileName in characterDirectory.GetFiles())
        {
            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            characters.Add(CharacterDataLoader.Load(charactersPath + "/" + fileName));
        }

        if (characters.Count == 0)
        {
            throw new InvalidOperationException("At least one character definition is required.");
        }

        string combatActionsPath = normalizedRoot + "/Combat/CombatActions.json";
        if (!FileAccess.FileExists(combatActionsPath))
        {
            throw new InvalidOperationException("Combat action content file was not found: " + combatActionsPath);
        }

        CombatActionCatalog combatActions = new CombatActionCatalog(
            CombatActionJsonReader.Read(FileAccess.GetFileAsString(combatActionsPath)));
        CharacterCatalog characterCatalog = new CharacterCatalog(characters);
        NpcScheduleCatalog npcSchedules = NpcScheduleDataLoader.Load(normalizedRoot + "/NpcSchedules.json");
        NpcInteractionCatalog npcInteractions = NpcInteractionDataLoader.Load(normalizedRoot + "/NpcInteractions.json");

        return new GameContentCatalog(locations, characterCatalog, combatActions, npcSchedules, npcInteractions);
    }
}
