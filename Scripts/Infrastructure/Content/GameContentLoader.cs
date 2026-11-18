using System;
using Godot;

public static class GameContentLoader
{
    public static GameContentCatalog Load(string dataRoot)
    {
        if (string.IsNullOrWhiteSpace(dataRoot))
        {
            throw new ArgumentException("Game data root is required.", nameof(dataRoot));
        }

        string normalizedRoot = dataRoot.TrimEnd('/');
        AcademyContent.Load(normalizedRoot + "/AcademyLocations.json");

        CharacterData player = CharacterDataLoader.Load(normalizedRoot + "/Characters/Player.json");
        CharacterData goblin = CharacterDataLoader.Load(normalizedRoot + "/Characters/Goblin.json");

        string combatActionsPath = normalizedRoot + "/Combat/CombatActions.json";
        if (!FileAccess.FileExists(combatActionsPath))
        {
            throw new InvalidOperationException("Combat action content file was not found: " + combatActionsPath);
        }

        string combatActionsJson = FileAccess.GetFileAsString(combatActionsPath);
        CombatActionCatalog combatActions = new CombatActionCatalog(
            CombatActionJsonReader.Read(combatActionsJson));

        return new GameContentCatalog(player, goblin, combatActions);
    }
}
