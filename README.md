# Adventurer Academy — UI Prototype

Godot 4.7.2 .NET prototype for the fantasy academy life-simulation UI.

## Run
1. Open `project.godot` with Godot 4.7.2 .NET.
2. Let Godot import the SVG assets.
3. Build the C# project, then run the main scene.

## UI responsibilities
- `MainController.cs`: coordinates the panels and handles navigation choices.
- `Sidebar.cs`: emits selected tab names.
- `StoryPanel.cs`: displays location text and available choices.
- `CharacterPanel.cs`: displays the character portrait and status.
- `AcademyContent.cs`: stores sample location content and choices.
- `UiTheme.cs`: shared panel and button styling.

Each major UI area has its own scene under `Scenes/UI`.
