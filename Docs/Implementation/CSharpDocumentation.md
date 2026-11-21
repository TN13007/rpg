# C# XML Documentation and VS Code IntelliSense

**Applies to:** Godot 4.7.2 .NET / C# source in `Scripts/` and public contracts consumed by `RPG.Tests/`.

This guide defines how source code explains its public API. It complements, rather than replaces, the layer and feature documentation in [Cross-Layer Architecture](CrossLayerArchitecture.md).

## 1. What you get in VS Code

With the Microsoft C# tooling installed and the project successfully loaded, IntelliSense can show XML documentation while you work:

| Feature | How to use it | What it does |
|---|---|---|
| Hover | Move the pointer over a symbol | Shows its type/signature and XML documentation |
| Completion | Type a member name or press `Ctrl + Space` | Suggests available types, methods and properties |
| Parameter help | Type `(` after a method name, or press `Ctrl + Shift + Space` inside the argument list | Shows parameter names, types and their documentation |
| Go to Definition | `F12` | Opens the symbol's declaration |
| Peek Definition | `Alt + F12` | Shows the declaration inline |
| Quick Fix / Code Actions | `Ctrl + .` | Offers context-sensitive actions, such as generating a method or implementing an interface |
| Find All References | `Shift + F12` | Finds usages of the selected symbol |

The exact available Code Actions depend on the C# language server and the code context. IntelliSense does not invent missing documentation: the source or referenced assembly must contain it.

## 2. VS Code setup

1. Open the repository root folder, not an isolated `.cs` file.
2. Install the recommended extensions from the Extensions panel. The repository recommends **C# Dev Kit** and **Godot Tools**.
3. Install the **.NET 8 SDK** required by `RPG.csproj`. Godot must be the .NET-enabled 4.7.2 build to run this project.
4. Open the folder in Godot 4.7.2 .NET once and let the editor import the project.
5. In VS Code, open a C# file and wait for the C# language service to finish loading. If IntelliSense is not ready, use the Command Palette and run the C# project/solution reload command exposed by the installed C# extension.
6. Test by hovering over `Character.TakeDamage` or `CombatSessionService.TryStartEncounter`.

The project emits an XML documentation file for its compiled assembly so documentation can also be consumed across project boundaries. Missing-documentation warning CS1591 is temporarily suppressed while existing public APIs are documented incrementally; this does not suppress XML parsing errors or other compiler diagnostics.

## 3. XML documentation syntax

Place three slashes immediately above the declaration:

```csharp
/// <summary>
/// Applies damage to this character, without reducing Health below zero.
/// </summary>
/// <param name="amount">The requested damage. Negative values are treated as zero.</param>
/// <returns>The amount of damage actually applied.</returns>
public int TakeDamage(int amount)
{
    // Implementation...
}
```

The compiler and C# language service understand these XML tags:

| Tag | Use it for |
|---|---|
| `<summary>` | A concise definition of the type or member. This is the main tooltip description. |
| `<param name="...">` | The meaning, valid range and relevant behavior of one parameter. The name must match the method signature. |
| `<returns>` | The meaning of the returned value, including special values such as `false`, `null` or zero when meaningful. |
| `<value>` | The meaning of a property or indexer's value. |
| `<remarks>` | Important preconditions, side effects, ownership rules, timing or non-obvious behavior. |
| `<exception cref="...">` | An exception the member intentionally documents as part of its contract. Do not list speculative exceptions. |
| `<example>` | A short, useful usage example when the API is not self-explanatory. |
| `<see cref="..."/>` / `<seealso cref="..."/>` | Links to related types or members. |
| `<paramref name="..."/>` | Refers to a parameter by name inside documentation text. |

**In short:** `summary` defines what the member is/does; `param` explains each input; `returns` explains the output. A method with no return value (`void`) does not need a `<returns>` tag.

## 4. What must be documented

Document public APIs that another class or layer is expected to use:

- **Class / struct / enum / interface:** responsibility, ownership and important boundaries.
- **Constructor:** what object is created, what each dependency represents and any required preconditions.
- **Public method:** observable behavior, parameters, return value, side effects and important failure conditions.
- **Public property:** what its value represents and whether it is a snapshot, derived value or mutable state.
- **Enum members:** what each option means when the name alone is insufficient.
- **Events and signals exposed through C#:** when they are emitted and what the payload means.

Do not add repetitive comments to every private field, trivial local variable, obvious one-line getter or private helper. Add comments to private members only when they contain a non-obvious invariant or behavior that a maintainer needs to preserve.

Do not merely translate the method name into a sentence. Explain the contract that a caller cannot safely infer from the signature.

## 5. Project-specific writing rules

1. Write documentation in clear English, matching the codebase's identifiers and existing technical documents.
2. Describe **current implemented behavior**, not planned features or desired future behavior.
3. Keep `summary` to one or two sentences. Put edge cases and side effects in `remarks`.
4. Explain units explicitly: seconds, minutes, percentage, game-time minutes, HP, resource points, and so on.
5. For methods returning `bool`, explain what success means and what failure means. If there is an `out string failureReason`, explain when it is populated.
6. For methods returning a number, explain whether it is requested, calculated, capped, restored, spent or actually applied.
7. For query/mapper methods, state whether they are read-only and what kind of data they return.
8. For methods that mutate Domain state, name the state that changes. Do not imply that a UI component owns the mutation.
9. For Godot callbacks such as `_Ready`, `_Process` and `_ExitTree`, document only project-specific behavior; Godot's callback name already describes the engine lifecycle.
10. Use `<see cref="TypeName"/>` for important related APIs when it improves navigation.

### Layer-specific emphasis

| Layer | Explain in documentation |
|---|---|
| Bootstrap | Which dependencies are composed, which scene/nodes are expected and which top-level events are connected |
| Domain | Invariants, state mutation, calculations, validation and deterministic rules |
| Application | Use case, request validation, orchestration order and failure behavior |
| Presentation | Input/output ViewData, UI events, lifecycle and what the component deliberately does not decide |
| Infrastructure | Source format, parsing/validation behavior, IDs/references and failure handling |

## 6. Recommended templates

### Type

```csharp
/// <summary>
/// Owns the runtime state and rules for one character.
/// </summary>
/// <remarks>
/// Character is a Domain object. It does not read JSON or depend on Godot UI.
/// </remarks>
public sealed class Character
{
}
```

### Method with input and output

```csharp
/// <summary>
/// Restores Health without exceeding the character's maximum.
/// </summary>
/// <param name="amount">The requested amount of Health to restore. Negative values are treated as zero.</param>
/// <returns>The amount of Health actually restored; zero when no Health can be restored.</returns>
/// <remarks>
/// A dead character cannot be healed by this method; use <see cref="Revive"/> instead.
/// </remarks>
public int RestoreHealth(int amount)
{
}
```

### Method with a success flag and failure reason

```csharp
/// <summary>
/// Attempts to start an encounter after validating and executing its entry action.
/// </summary>
/// <param name="entryAction">The action used to enter the encounter.</param>
/// <param name="failureReason">Receives the reason when the operation fails.</param>
/// <returns><see langword="true"/> when the encounter starts; otherwise, <see langword="false"/>.</returns>
/// <remarks>
/// A successful call advances the game clock through the entry action and starts the combat session.
/// </remarks>
public bool TryStartEncounter(ActionDefinition entryAction, out string failureReason)
{
}
```

### Property

```csharp
/// <summary>
/// Gets the character's current Health.
/// </summary>
/// <value>A value between zero and <see cref="MaxHealth"/>.</value>
public int Health { get; private set; }
```

## 7. How to generate documentation while coding

When writing a new public member:

1. Write the declaration and its implementation.
2. Move the cursor to the line immediately above the declaration.
3. Type `///`. In supported C# editors, the editor generates the XML tag skeleton from the method signature.
4. Fill in `summary`, every `param`, and `returns` when applicable.
5. Build the project or run the repository validation command to catch malformed XML documentation and compiler errors.
6. Hover over the member from a call site to verify the tooltip is useful.

You can also use `Ctrl + .` on suitable declarations or unresolved calls to inspect available generation actions. Generated method bodies are only stubs; you must still implement and verify the behavior.

## 8. Scope and maintenance

XML documentation is an API guide, not a replacement for:
- implementation comments explaining why a non-obvious algorithm exists;
- Design documents explaining intended player experience;
- Implementation documents explaining architecture and cross-layer flows;
- tests proving actual behavior.

When a public contract changes, update its XML documentation in the same change. When a member is removed or renamed, update its references and documentation together. Prefer documenting the reason and observable contract over narrating each line of code.
