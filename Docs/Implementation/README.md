# Implementation

This folder is the authoritative guide to **how the current repository works**. It describes the code and technical contracts on latest `main`; it does not define the intended future game.

## Read by topic

| Document | Owns |
|---|---|
| [Architecture](Architecture.md) | Repository map, layers, dependency direction, composition root, runtime flows and code-discovery method |
| [Runtime Systems](RuntimeSystems.md) | Current domain vocabulary, authored content, Location/Event/Action behavior and the implemented Combat slice |
| [User Interface](UserInterface.md) | Current screen composition, presentation data flow and UI/service boundaries |
| [Testing](Testing.md) | Automated test strategy, Godot runtime verification and evidence requirements |

## How to use these documents

1. Read the relevant [Design](../Design/README.md) section to understand the desired player-facing behavior.
2. Read the relevant Implementation document to understand what exists and where the behavior belongs.
3. Trace the actual source and tests before editing. The documents are navigation and contracts, not substitutes for code inspection.
4. If the implementation does not meet the design, describe the gap in the Issue/PR. Do not update Implementation to claim planned behavior exists.

## Ownership

- Product direction, ideas and gameplay rules → [Design](../Design/README.md).
- Current codebase, runtime behavior, data and presentation contracts → this folder.
- Current status and handoff → [WORKFLOW.md](../../WORKFLOW.md).
- Stable collaboration, Git and naming rules → [CONTRIBUTING.md](../../CONTRIBUTING.md).
