# Developer Cookbook

**Applies to:** The current Godot 4.7.2 .NET / C# prototype.  
**Purpose:** Practical starting points for common changes. This guide describes where to work and how to trace a change; it does not replace source inspection or tests.

Before using a recipe, read [Cross-Layer Architecture](CrossLayerArchitecture.md), then the relevant feature section in [Cross-Layer Runtime Systems](CrossLayerRuntimeSystems.md). For UI contracts, also read [Presentation User Interface](PresentationUserInterface.md).

## 1. Trace a feature before editing

Use this sequence rather than opening files at random:

1. Find the player-facing label, content ID, signal or type with repository search.
2. Find the authored definition in Data/ and its reader/catalog in Scripts/Infrastructure/Content/.
3. Find the Domain type that owns the state or rule.
4. Find the Application query/service that exposes or executes the behavior.
5. Find the Presentation mapper, ViewData and UI component.
6. Find the Bootstrap wiring in Scripts/Bootstrap/MainController.cs.
7. Find the matching tests under RPG.Tests/.

A typical request path is:

    Input / UI signal
      -> Presentation flow controller
      -> Application query or use-case service
      -> Domain rule and state mutation
      -> result / snapshot
      -> Presentation mapper
      -> ViewData
      -> UI component

Not every feature uses every step. Content loading normally enters through Infrastructure during bootstrap; a pure Domain rule may not need a UI change.

## 2. Add or change an Exploration Action

Current examples are Player Actions in character data and Location Actions in Data/Locations.json.

1. Check the action model and supported effect types in Scripts/Domain/Actions/ActionDefinition.cs.
2. Check the corresponding JSON reader and loader in Scripts/Infrastructure/Content/.
3. Add or update the authored record using the existing ID and JSON naming conventions. Do not invent a new effect type in JSON; content can only select behavior implemented by code.
4. Check how the choice is discovered in Scripts/Application/Exploration/AvailableInteractionQuery.cs.
5. Check execution and validation in Scripts/Application/Actions/ActionExecutionService.cs and the effect executor. The service must remain the authority; a visible choice is not proof that a request is still valid.
6. Update the relevant tests in RPG.Tests/Actions/ or the feature-specific test folder.
7. Run the repository validation command and verify the affected action in Godot.

**Important distinction:** adding a new record that uses an existing effect may be data-only. Adding a new effect or changing its rules requires Domain/Application implementation and tests as well.

## 3. Add or change an NPC interaction

NPC presence and NPC-specific actions are separate concerns.

1. Define the Character in Data/Characters/.
2. Define its daily presence intervals in Data/NpcSchedules.json. Intervals use minutes of day, start inclusive and end exclusive.
3. Define NPC-specific actions in Data/NpcInteractions.json; use availableLocationIds only when the interaction is location-restricted.
4. Check NpcScheduleDataLoader, NpcInteractionDataLoader and GameContentCatalog for loading and reference validation.
5. Check NpcPresenceQuery and NpcInteractionQuery for read-only discovery.
6. Check ExplorationFlowController for the See around → select person → select interaction flow.
7. Preserve execution-time presence validation. A schedule query can become stale if the game clock or location changes before the action is executed.
8. Add or update schedule/interaction tests and verify at least one present and one absent time/location case in Godot.

Do not put NPC-specific actions into LocationContent.Actions; those actions belong to the place itself.

## 4. Add or change a Combat action

Combat actions are defined separately from Exploration Actions.

1. Inspect Scripts/Domain/Combat/CombatActionDefinition.cs and the supported behavior/effect handling.
2. Add the authored action to Data/Combat/CombatActions.json, using the existing serialized enum values and fields.
3. Assign the action ID to the intended Character definition in Data/Characters/.
4. Check CombatActionCatalog and GameContentCatalog for lookup and reference validation.
5. Check CombatService for action availability, target validation, resolution and state mutation. Do not put damage, stamina spending or turn rules in UI code.
6. Check CombatPresentationMapper and the Combat ViewData if the action needs new player-facing information.
7. Add tests for success, invalid actor/target, insufficient resources and the resulting Timeline/state changes that apply to the action.
8. Verify the action through the Combat UI and confirm that messages and visual feedback describe the result without becoming the source of truth.

Enemy behavior is currently selected from the actions authored for that enemy and its behavior profile. Do not add an enemy decision rule to CombatPanel or EnemyCard.

## 5. Add or change a reusable UI component

Presentation components render ViewData and emit intent; they do not own gameplay decisions.

1. Define or reuse a presentation contract under Scripts/Presentation/ViewModels/.
2. Map Domain/Application results into that contract in the feature mapper.
3. Create the scene under Scenes/ and its C# script under Scripts/Presentation/UI/.
4. Give the component a focused responsibility: render the supplied data, expose selection/interaction events and manage its own visual state.
5. Let the parent panel own the component collection and lifecycle. Use stable entity IDs when creating, updating or removing repeated cards/markers.
6. Let the flow controller or composition root connect the UI event to the owning Application use case.
7. Verify empty, normal, selected, unavailable and updated states as relevant.

A UI component should not load JSON, look up Domain entities, call gameplay services directly or decide whether an action is legal. For current Combat examples, see EnemyCard, PartyMemberCard, CombatActionCard and TimelineMarker in Presentation User Interface.

## 6. Add or change a Domain rule

1. Identify the state owner and invariant before changing code.
2. Put deterministic rules and state transitions in the Domain feature folder.
3. Keep the rule independent of Godot controls, JSON parsing and Presentation ViewData.
4. Make invalid operations explicit: return a result/boolean or throw only when the existing contract calls for it.
5. Add focused unit tests for normal behavior, boundaries and rejected operations without unintended mutation.
6. Update Application orchestration only if the use case needs to call the changed rule.
7. Update XML documentation to explain observable behavior, preconditions, side effects and return semantics.

Do not create a generic framework or interface for a single use case without a demonstrated need.

## 7. Debug a behavior that appears in the wrong place

Trace the value in both directions:

- **Input direction:** UI signal → request identity → flow controller → Application validation → Domain operation.
- **Output direction:** Domain result/snapshot → mapper → ViewData → parent panel → child component.

At each boundary, inspect the actual object/value rather than relying on a label or method name. For a stale or invalid choice, compare the identity and state used when the choice was displayed with the state revalidated at execution time.

For Combat timing, trace MainController._Process(delta) → CombatFlowController → CombatSessionService → CombatService → snapshot → CombatPresentationMapper → TimelineTrack. Keep simulation time and presentation time calculations separate.

## 8. Verification checklist

- [ ] The change is in the layer that owns the responsibility.
- [ ] Authored IDs and references resolve through the content catalog.
- [ ] Application revalidates requests before mutation.
- [ ] Domain tests cover the changed rule and important failure cases.
- [ ] ViewData contains only presentation-safe values.
- [ ] UI components render and emit intent without owning gameplay rules.
- [ ] XML documentation matches the implementation and explains non-obvious contracts.
- [ ] The repository validation command passes.
- [ ] The affected player journey is manually checked in Godot when scenes, input, wiring or runtime behavior changed.
- [ ] The PR records what was tested and what was not tested.

For exact commands and evidence expectations, see Testing Strategy and CONTRIBUTING.
