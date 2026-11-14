using System.Collections.Generic;

public sealed class StoryChoice
{
    public string Id { get; }
    public string Label { get; }

    public StoryChoice(string id, string label)
    {
        Id = id;
        Label = label;
    }
}

public sealed class LocationContent
{
    public string Id { get; }
    public string Name { get; }
    public string Region { get; }
    public string Description { get; }
    public string[] Exits { get; }
    public StoryChoice[] Actions { get; }

    public LocationContent(string id, string name, string region, string description, string[] exits, StoryChoice[] actions)
    {
        Id = id;
        Name = name;
        Region = region;
        Description = description;
        Exits = exits;
        Actions = actions;
    }
}

public static class AcademyContent
{
    private static readonly LocationContent[] Locations =
    {
        new LocationContent(
            "dormitory", "Dormitory", "STUDENT QUARTERS",
            "Morning light slips through the curtains. Your room is quiet, and the academy day is about to begin.",
            new[] { "courtyard" },
            new[] { new StoryChoice("rest", "Take a short rest"), new StoryChoice("inspect_letter", "Read the letter on your desk") }),

        new LocationContent(
            "courtyard", "Academy Courtyard", "ASTERIA ACADEMY",
            "Students cross the courtyard beneath old trees. A rustling comes from the gate. Something small is moving in the bushes.",
            new[] { "dormitory", "classroom", "guild_hall" },
            new[]
            {
                new StoryChoice("talk_mira", "Talk to Mira"),
                new StoryChoice("inspect_board", "Read the notice board"),
                new StoryChoice("encounter_goblin", "Investigate the rustling near the gate")
            }),

        new LocationContent(
            "classroom", "Practical Classroom", "EAST WING",
            "Field maps cover the walls. Training equipment is arranged neatly beside the instructor's desk.",
            new[] { "courtyard" },
            new[] { new StoryChoice("talk_instructor", "Talk to the instructor"), new StoryChoice("inspect_maps", "Study the field maps") }),

        new LocationContent(
            "guild_hall", "Guild Hall", "EXPEDITION QUARTER",
            "A broad counter separates the students from the expedition clerks. Reports and requests are pinned behind them.",
            new[] { "courtyard" },
            new[] { new StoryChoice("talk_clerk", "Talk to the guild clerk"), new StoryChoice("inspect_requests", "Browse expedition requests") })
    };

    public static IEnumerable<LocationContent> All => Locations;

    public static LocationContent Get(string id)
    {
        foreach (LocationContent location in Locations)
        {
            if (location.Id == id)
            {
                return location;
            }
        }

        return Locations[0];
    }
}
