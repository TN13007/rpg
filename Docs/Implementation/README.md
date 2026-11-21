# Implementation

This folder is the authoritative guide to **how the current repository works**. It describes the code and technical contracts on latest `main`; it does not define the intended future game.

## Read by topic

| Document | Owns |
|---|---|
| [Cross-Layer Architecture](CrossLayerArchitecture.md) | Repository map, layers, dependency direction, composition root, runtime flows and code-discovery method |
| [Cross-Layer Runtime Systems](CrossLayerRuntimeSystems.md) | Current domain vocabulary, authored content, Location/Event/Action behavior and the implemented Combat slice |
| [Presentation User Interface](PresentationUserInterface.md) | Current screen composition, presentation data flow and UI/service boundaries |
| [C# XML Documentation](CSharpDocumentation.md) | XML comments, IntelliSense setup, API documentation rules and VS Code navigation |
| [Developer Cookbook](DeveloperCookbook.md) | Step-by-step recipes for changing Exploration Actions, NPC interactions, Combat actions, Domain rules and reusable UI |
| [Testing Strategy](TestingStrategy.md) | Automated test strategy, Godot runtime verification and evidence requirements |

## How to use these documents

1. Read the relevant [Design](../Design/README.md) section to understand the desired player-facing behavior.
2. Read [Cross-Layer Architecture](CrossLayerArchitecture.md) to identify the owning layer and dependency direction.
3. Read the relevant feature in [Cross-Layer Runtime Systems](CrossLayerRuntimeSystems.md) and [Presentation User Interface](PresentationUserInterface.md) to understand current behavior and data flow.
4. Use the [Developer Cookbook](DeveloperCookbook.md) for a concrete change workflow.
5. Trace the actual source and tests before editing. The documents are navigation and contracts, not substitutes for code inspection.
6. If the implementation does not meet the design, describe the gap in the Issue/PR. Do not update Implementation to claim planned behavior exists.

## Ownership

- Product direction, ideas and gameplay rules → [Design](../Design/README.md).
- Current codebase, runtime behavior, data and presentation contracts → this folder.
- Current status and handoff → [WORKFLOW.md](../../WORKFLOW.md).
- Stable collaboration, Git and naming rules → [CONTRIBUTING.md](../../CONTRIBUTING.md).
