# Scripts Source Map

The source tree is organized by **shared game foundations and gameplay feature**. A feature folder keeps its rules, flow coordination, presentation and feature-specific content close together. Logical boundaries between rules, use cases, UI and content loading still apply; they are no longer mirrored as four global technical-layer directories.

## Where to start

| Work to do | Start here |
|---|---|
| Shared Character state/resources | `Core/Characters/` |
| Shared Action definitions/effect execution | `Core/Actions/` |
| Activity time/Fatigue calculations | `Core/Activities/` |
| World time/date | `Core/Time/` |
| Location navigation/content | `Features/Exploration/Navigation/` and `Features/Exploration/Locations/` |
| Location choices | `Features/Exploration/Interactions/` |
| NPC presence/interactions | `Features/Exploration/NPC/` |
| Location Event rules | `Features/Exploration/Events/` |
| Combat rules/turn resolution | `Features/Combat/Rules/` |
| Encounter lifecycle/party setup | `Features/Combat/Encounter/` and `Features/Combat/Setup/` |
| Feature UI/presentation | That feature's `UI/` or `Presentation/` |
| Shared JSON loading/reference validation | `Content/` |
| Shared game shell controls | `UI/Shell/` or `UI/Shared/` |

## Directory ownership

```text
Scripts/
├── Bootstrap/                 # Godot entry point and dependency composition
├── Core/                      # Shared, feature-independent game rules/state
│   ├── Actions/               # Definitions, effects and execution
│   ├── Activities/
│   ├── Characters/
│   └── Time/
├── Features/
│   ├── Exploration/
│   │   ├── Navigation/
│   │   ├── Locations/Content/
│   │   ├── Interactions/
│   │   ├── NPC/Content/
│   │   ├── Events/
│   │   ├── Presentation/
│   │   └── UI/
│   └── Combat/
│       ├── Rules/
│       ├── Encounter/
│       ├── Setup/
│       ├── Content/
│       ├── Presentation/
│       ├── Preview/
│       └── UI/
├── Content/                   # Cross-feature loading/reference validation
│   ├── Actions/
│   └── Characters/
└── UI/
    ├── Shell/                 # Character panel, sidebar and game-mode shell
    └── Shared/                # Reusable UI styles and formatters
```

The tree lists responsibilities that currently exist. Do not create empty folders or placeholder classes for future features such as DailyLife.

## Content lookup

| Change | Files/folders to inspect |
|---|---|
| Add a Location using the existing schema | Only its record in `Data/Locations/Locations.json` |
| Change Location schema/parsing/validation | `Data/Locations/Locations.json`, `Features/Exploration/Locations/Content/LocationDataLoader.cs`, `Features/Exploration/Locations/LocationContent.cs` and loader tests |
| Change Location lookup/indexing | `Features/Exploration/Locations/Content/LocationCatalog.cs` |
| Change NPC schedule/interaction schema | `Data/NPC/`, `Features/Exploration/NPC/Content/`, NPC definitions and tests |
| Change Character definition schema | `Data/Characters/`, `Content/Characters/`, `Core/Characters/CharacterData.cs` and tests |
| Change Combat action schema | `Data/Combat/CombatActions.json`, `Features/Combat/Content/` and content tests |
| Add a top-level content category | Its loader/catalog plus `Content/GameContentLoader.cs` and `Content/GameContentCatalog.cs` |

A data-only content addition should not require editing a loader when it uses the existing schema and supported behavior. A schema change normally touches the data example, definition, loader and focused tests. Cross-category reference validation belongs in `Content/GameContentCatalog.cs`.

## Tests

`RPG.Tests/` mirrors ownership: `Core/` tests shared state/rules; `Features/Exploration/` and `Features/Combat/` test gameplay responsibilities; `Content/` tests cross-feature loading; `Integration/` tests composed content or journeys; `UI/` tests shared presentation utilities; `Support/` contains shared fixtures.

For new code, put a test beside the responsibility it protects. Do not create a separate test folder for every technical layer.

See [AGENTS.md](../AGENTS.md) for discovery and verification rules, and [Cross-Layer Architecture](../Docs/Implementation/CrossLayerArchitecture.md) for logical ownership and dependency constraints.
