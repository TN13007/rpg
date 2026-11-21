# C# XML Documentation

This repository uses XML documentation comments (`///`) to make the public C# API discoverable through VS Code IntelliSense and hover tooltips.

## Where documentation belongs

- Add a `<summary>` to every public type and public member. Describe its purpose or contract, not its implementation line by line.
- Add `<param name="...">` for parameters when their meaning is not already obvious from the name and type.
- Add `<returns>` when a method returns a value. Explain what the value means, especially for `bool`, nullable results, counts and calculated values.
- Add `<remarks>` for important preconditions, side effects, state transitions, ownership rules or limitations.
- Add `<exception>` only for exceptions that callers can reasonably encounter as part of the API contract.
- Add `<example>` when a short example makes a non-obvious API easier to use.
- Document private methods only when their behavior, ordering constraints or invariants are non-obvious.

## Example

```csharp
/// <summary>
/// Applies damage to this character and returns the amount actually applied.
/// </summary>
/// <param name="amount">Requested damage. Negative values are treated as zero.</param>
/// <returns>The amount of health removed, which cannot exceed the character's current health.</returns>
/// <remarks>
/// This method changes the character's authoritative health state. It does not trigger
/// presentation effects, combat messages or animation.
/// </remarks>
public int TakeDamage(int amount)
{
    // ...
}
```

## Common XML tags

| Tag | Purpose |
|---|---|
| `<summary>` | Short description of a type or member |
| `<param name="name">` | Meaning and constraints of a method parameter |
| `<returns>` | Meaning of the returned value |
| `<remarks>` | Additional contract details, side effects or limitations |
| `<exception cref="...">` | Documented exception condition |
| `<example>` | Usage example |
| `<see cref="..."/>` | Link to another type or member |
| `<paramref name="..."/>` | Refer to a parameter inside prose |
| `<inheritdoc/>` | Reuse documentation from a base member or interface when the inherited contract is accurate |

## Authoring rules

1. Write documentation in English to match the repository's code identifiers and existing technical documentation.
2. Describe observable behavior and caller expectations. Do not guess intent from a method name.
3. Distinguish validation from mutation, and explain important state changes.
4. Explain units for numeric values (for example, minutes, seconds, combat time or percentage points).
5. For query/read methods, state whether they are read-only when that distinction matters.
6. Do not add comments to every local variable, branch or obvious getter.
7. Keep documentation synchronized with implementation and tests. Update it when the API contract changes.

## VS Code usage

With the Microsoft C# extension and the project's .NET SDK available:

- Hover over a documented symbol to read its summary and remarks.
- Type `///` immediately above a declaration to ask the editor to generate the XML comment skeleton.
- Use `Ctrl+Space` for completion and parameter suggestions.
- Use `Ctrl+Shift+Space` inside an argument list for signature help.
- Use `F12` to go to a definition and `Alt+F12` to peek at it.

Source XML comments are available to the editor while working in the project. Generating a standalone XML documentation file for compiled assemblies is a separate build configuration choice and is not required for ordinary in-solution hover documentation.

## Scope and rollout

Document production APIs under `Scripts/` by layer and feature. Tests, generated Godot `.uid` sidecars and content files are not part of the production API documentation pass. Prefer meaningful contract documentation over mechanically adding empty summaries.
