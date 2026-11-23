using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xunit;

public sealed class NpcScheduleTests
{
    [Fact]
    public void GetEntryAt_UsesInclusiveStartAndExclusiveEnd()
    {
        NpcScheduleEntryDefinition morning = new NpcScheduleEntryDefinition(
            420, 510, "university_campus", "Greeting students.");
        NpcScheduleDefinition schedule = new NpcScheduleDefinition("mira", new[] { morning });

        Assert.Same(morning, schedule.GetEntryAt(420));
        Assert.Same(morning, schedule.GetEntryAt(509));
        Assert.Null(schedule.GetEntryAt(510));
    }

    [Fact]
    public void GetEntryAt_ReturnsNullDuringAnUnscheduledGap()
    {
        NpcScheduleDefinition schedule = new NpcScheduleDefinition(
            "mira",
            new[]
            {
                new NpcScheduleEntryDefinition(420, 510, "university_campus", "Greeting students."),
                new NpcScheduleEntryDefinition(570, 750, "canteen", "Having lunch.")
            });

        Assert.Null(schedule.GetEntryAt(510));
        Assert.Null(schedule.GetEntryAt(569));
        Assert.Equal("canteen", schedule.GetEntryAt(570).LocationId);
    }

    [Fact]
    public void Constructor_RejectsOverlappingEntries()
    {
        Assert.Throws<ArgumentException>(() => new NpcScheduleDefinition(
            "mira",
            new[]
            {
                new NpcScheduleEntryDefinition(420, 600, "university_campus", "Greeting students."),
                new NpcScheduleEntryDefinition(599, 720, "canteen", "Having lunch.")
            }));
    }

    [Fact]
    public void Entry_RejectsInvalidTimeRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new NpcScheduleEntryDefinition(600, 600, "canteen", "Having lunch."));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new NpcScheduleEntryDefinition(-1, 600, "canteen", "Having lunch."));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new NpcScheduleEntryDefinition(600, 1441, "canteen", "Having lunch."));
    }

    [Fact]
    public void PresenceQuery_ReturnsNpcOnlyAtScheduledLocationAndTime()
    {
        CharacterData mira = CreateCharacter("mira", "Mira");
        NpcScheduleDefinition schedule = new NpcScheduleDefinition(
            "mira",
            new[]
            {
                new NpcScheduleEntryDefinition(570, 750, "canteen", "Sharing lunch with students."),
                new NpcScheduleEntryDefinition(750, 900, "library", "Reading course material.")
            });
        NpcPresenceQuery query = new NpcPresenceQuery(new[] { schedule }, new[] { mira });

        IReadOnlyList<NpcPresence> atCanteen = query.Query("canteen", 570);
        IReadOnlyList<NpcPresence> atLibrary = query.Query("library", 800);

        Assert.Single(atCanteen);
        Assert.Equal("mira", atCanteen[0].CharacterId);
        Assert.Equal("Mira", atCanteen[0].Name);
        Assert.Equal("Sharing lunch with students.", atCanteen[0].ActivityDescription);
        Assert.Single(atLibrary);
        Assert.Equal("library", atLibrary[0].LocationId);
        Assert.Empty(query.Query("canteen", 800));
        Assert.Empty(query.Query("canteen", 750));
        Assert.True(query.IsPresent("mira", "canteen", 600));
        Assert.False(query.IsPresent("mira", "library", 600));
    }

    [Fact]
    public void PresenceQuery_RejectsUnknownCharacterReferences()
    {
        NpcScheduleDefinition schedule = new NpcScheduleDefinition(
            "unknown_npc",
            new[] { new NpcScheduleEntryDefinition(420, 510, "courtyard", "Waiting.") });

        Assert.Throws<InvalidOperationException>(() =>
            new NpcPresenceQuery(new[] { schedule }, new[] { CreateCharacter("mira", "Mira") }));
    }

    [Fact]
    public void DataLoader_ReadsAuthoredMiraSchedule()
    {
        string json = File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory, "Data", "NpcSchedules.json"));
        NpcScheduleCatalog catalog = NpcScheduleDataLoader.LoadJson(json);
        NpcScheduleDefinition miraSchedule = catalog.Get("mira");

        Assert.Equal(6, miraSchedule.Entries.Length);
        Assert.Equal("university_campus", miraSchedule.GetEntryAt(450).LocationId);
        Assert.Equal("university_campus", miraSchedule.GetEntryAt(500).LocationId);
        Assert.Equal("lecture_hall", miraSchedule.GetEntryAt(550).LocationId);
        Assert.Equal("campus_cafe", miraSchedule.GetEntryAt(690).LocationId);
        Assert.Equal("campus_library", miraSchedule.GetEntryAt(800).LocationId);
        Assert.Equal("student_union", miraSchedule.GetEntryAt(950).LocationId);
    }

    [Fact]
    public void ActionDefinitionJsonReader_ReadsRequiredNpcId()
    {
        using JsonDocument document = JsonDocument.Parse(
            "{\"durationMinutes\":10,\"requiredNpcId\":\"mira\"}");

        ActionDefinition action = ActionDefinitionJsonReader.ReadActionDefinition(
            "talk_to_mira", "Talk with Mira", document.RootElement);

        Assert.Equal("mira", action.RequiredNpcId);
    }

    private static CharacterData CreateCharacter(string id, string name)
    {
        return new CharacterData(
            id,
            name,
            "Student",
            1,
            20,
            100,
            10,
            100,
            20,
            0,
            10,
            60,
            0);
    }
}
