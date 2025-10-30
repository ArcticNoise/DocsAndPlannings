---
name: csharp-SKILL
description: Mandatory C# coding standards and best practices ensuring consistency, maintainability, and quality. Covers testing requirements, project configuration, code safety, file organization, and naming conventions.
---

# C# Coding Standards Skill

## Overview
This skill defines mandatory coding standards and best practices for C# development. All code must comply with these rules to ensure consistency, maintainability, and quality.

## Mandatory Rules (🔒 = Non-negotiable)

### Testing & Quality Assurance
1. **Unit tests first** 🔒 — Every non-trivial change MUST add or update tests before implementation
2. **Assert critical logic & parameters** 🔒 — Use `Debug.Assert` or unit-test assertions for validation
3. **Tests must genuinely reflect reality** 🔒 — Tests must fail for any defect; missing dependencies must NOT yield passing tests
4. **P/Invoke & external DLLs** 🔒 — Every interop call MUST be covered by unit tests validating marshaling and error handling

### Project Configuration
5. **Enable required features** 🔒:
   - `nullable` reference types
   - `implicit usings`
   - `TreatWarningsAsErrors` (default warning level)
6. **Forbidden features** 🔒 — NO:
   - Global usings
   - Singletons
   - Local functions
   - Dynamic types
   - File-scope `internal`/`private protected` modifiers
   - LINQ query syntax (use method syntax only)

### Code Safety & Architecture
7. **Static code must be thread-safe and side-effect-free** 🔒
8. **All file IO wrapped in try/catch with narrowest possible scope** 🔒
9. **Unsafe code must be only in dedicated projects** 🔒 — Wrap with binding classes; only use those wrappers
10. **Verify external source links** 🔒 — When integrating code from other projects, ensure exact match or justify updates

### File Organization
11. **Filenames = top-level type; one top-level type per file** 🔒
12. **Usings → Namespace → Members** 🔒 (strict order)
13. **Class/struct member order** 🔒:
    1. Constants
    2. Events
    3. Fields
    4. Properties & Indexers
    5. Constructors
    6. Methods
14. **Interface member order** 🔒: Properties, then Methods

### Comments & Documentation
15. **Code comments only in English** 🔒

## Coding & Style Guidelines

### General Principles
- **Remove redundancy**: Eliminate redundant code, keywords, and whitespace
- **Expression-bodied members**: ONLY for properties; methods and switches use braces
- **Early returns**: Prefer early returns to reduce nesting depth
- **One action per line**: Keep logical actions separated
- **Simple usings**: Prefer simple `using` statements; avoid type aliases

### Type & Member Safety

#### Immutability & Access
- Prefer `const` or `readonly` when possible
- `sealed` every class unless inheritance is explicitly required
- Always declare explicit and narrowest access modifiers
- Use `in` on large value-type parameters (NOT on primitives)

#### Best Practices
- Use `nameof(...)` for member name references
- Use `out` ONLY in Try-Parse style methods (max one per method)
- For multiple return values, use tuples or dedicated types instead of `out`
- Always use `var` where type is obvious
- Never use `dynamic`
- Avoid `ref` on reference types

### Naming Conventions

| Category | Convention | Example |
|----------|------------|---------|
| Types, events, methods, namespaces | **PascalCase** | `CustomerService`, `DataLoaded` |
| Variables, parameters | **camelCase** | `firstName`, `itemCount` |
| Constants | **CONSTANT_CASE** | `MAX_RETRY_COUNT` |
| Static fields | **s_PascalCase** | `s_Instance`, `s_DefaultTimeout` |
| Private instance fields | **m_PascalCase** | `m_ConnectionString`, `m_Items` |
| Interfaces | prefix **I** | `IService`, `IRepository` |
| Boolean members | **Is/Can/Has** + affirmative | `IsEnabled`, `CanExecute`, `HasPermission` |
| Acronyms | Capitalize first letter only | `XmlDocument`, `HttpClient` |
| Attributes | suffix **Attribute** | `ValidationAttribute` (use without suffix) |
| View-models | suffix **ViewModel** | `UserProfileViewModel` |

### Formatting Rules

#### Structure
- **Indentation**: 4 spaces (no tabs)
- **Braces**: On new lines for all blocks
- **Namespaces**: File-scoped; path mirrors folder structure
- **Blank lines**: Single blank line between logical blocks; no consecutive empty lines
- **Spacing**: Space after keywords: `if (condition)` not `if(condition)`

#### Organization
```csharp
using System;
using System.Collections.Generic;

namespace MyCompany.MyProject;

public sealed class MyClass
{
    // 1. Constants
    private const int MAX_SIZE = 100;
    
    // 2. Events
    public event EventHandler? DataLoaded;
    
    // 3. Fields
    private static readonly string s_DefaultValue = "default";
    private readonly ILogger m_Logger;
    
    // 4. Properties & Indexers
    public string Name { get; init; }
    public int Count => m_Items.Count;
    
    // 5. Constructors
    public MyClass(ILogger logger)
    {
        m_Logger = logger;
    }
    
    // 6. Methods
    public void ProcessData()
    {
        // Implementation
    }
}
```

### Nullability & Expressions
- Use null-coalescing (`??`) and null-conditional (`?.`) operators to shorten null checks
- Return `Array.Empty<T>()` or `[]` instead of `null` for empty collections
- Keep properties fast — no heavy computations in getters

### Collections & LINQ

#### Collection Types
- **Fixed size**: Use arrays
- **Mutable**: Use `IList<T>`
- **Immutable/Read-only**: Use `IReadOnlyList<T>` or `IEnumerable<T>`

#### LINQ Best Practices
- Avoid enumerating `IEnumerable<T>` multiple times
- Materialize with `ToList()` or `ToArray()` when multiple enumerations needed
- Prefer `foreach` loops over `ForEach()` extension (unless single-line lambda)
- Use `Any()` to test emptiness, NOT `Count() == 0`
- Use method syntax only (NO query syntax)

```csharp
// ✅ GOOD
var result = items
    .Where(x => x.IsActive)
    .Select(x => x.Name)
    .ToList();

// ❌ BAD - Query syntax
var result = (from item in items
              where item.IsActive
              select item.Name).ToList();
```

### String Handling
- Use `string.IsNullOrWhiteSpace()` for validation
- **Interpolation**: Use for readable expressions
- **Concatenation**: Use `+` for exactly two strings
- **StringBuilder**: Use for many concatenations or loops
- **String Comparison**: 🔒 NEVER use `ToLower()`/`ToUpper()` with `==` for comparison

#### String Comparison Rules 🔒
**Always use explicit `StringComparison` parameter:**

```csharp
// ✅ GOOD - Case-insensitive comparison
if (string.Equals(email1, email2, StringComparison.OrdinalIgnoreCase))
{
    // ...
}

// ✅ GOOD - Case-sensitive comparison
if (string.Equals(password, hashedPassword, StringComparison.Ordinal))
{
    // ...
}

// ✅ GOOD - For collections/LINQ
var user = users.FirstOrDefault(u =>
    string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

// ✅ GOOD - Using StringComparer
var emailComparer = StringComparer.OrdinalIgnoreCase;
if (emailComparer.Equals(email1, email2))
{
    // ...
}

// ❌ BAD - ToLower with == (inefficient, culture-dependent)
if (email1.ToLower() == email2.ToLower())  // DON'T DO THIS
{
    // ...
}

// ❌ BAD - Using == directly (case-sensitive, unclear intent)
if (email1 == email2)  // Unclear if case matters
{
    // ...
}
```

**Common `StringComparison` values:**
- `Ordinal` - Case-sensitive, culture-invariant (fastest, use for identifiers)
- `OrdinalIgnoreCase` - Case-insensitive, culture-invariant (use for emails, usernames, file paths)
- `CurrentCulture` - Culture-aware, case-sensitive (rare, only for UI display)
- `InvariantCulture` - Culture-invariant but slower than Ordinal (use when sorting matters)

**When to use each:**
- **Ordinal**: API tokens, passwords (hashed), IDs, XML/JSON keys
- **OrdinalIgnoreCase**: Emails, usernames, file paths, URLs, database keys
- **CurrentCulture**: User-facing text sorting/comparison
- **InvariantCulture**: Persistent data that needs culture-independent sorting

```csharp
// ✅ GOOD - General formatting and concatenation
string message = $"User {userName} logged in at {timestamp}";
string path = basePath + fileName;

// ✅ GOOD - Many concatenations
var builder = new StringBuilder();
foreach (var item in items)
{
    builder.AppendLine(item.ToString());
}
```

### Events
- Name events with verbs: `Closing`, `Closed`, `DataLoading`, `DataLoaded`
- Unsubscribe when publisher lifetime exceeds subscriber
- NO anonymous handlers that cannot be removed

```csharp
// ✅ GOOD
public event EventHandler? ConnectionClosed;
public event EventHandler<DataEventArgs>? DataReceived;

// In subscriber
dataSource.DataReceived += OnDataReceived;
// Later...
dataSource.DataReceived -= OnDataReceived;
```

### Records
Use `record` types where possible and reasonable, especially for:
- DTOs and data transfer objects
- Immutable data structures
- Value-based equality scenarios

```csharp
public sealed record UserDto(string Name, string Email, int Age);
```

### Structs
Use `struct` ONLY if ALL conditions met:
1. Will never be boxed (except when intentionally wrapped)
2. Has well-defined constructors
3. Remains small (< 16 bytes) and immutable

```csharp
public readonly struct Point
{
    public int X { get; init; }
    public int Y { get; init; }
    
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
}
```

### Exception Handling
- **Narrow scope**: `try/catch` only minimal lines that may throw
- **Specific exceptions**: Catch specific exception types when feasible
- **Preserve stack trace**: Re-throw with `throw;` unless new context required
- **File IO**: ALWAYS wrap in try/catch with narrowest scope

```csharp
// ✅ GOOD
try
{
    var content = File.ReadAllText(filePath);
    return ParseContent(content);
}
catch (FileNotFoundException ex)
{
    m_Logger.LogError(ex, "File not found: {FilePath}", filePath);
    throw;
}
catch (IOException ex)
{
    m_Logger.LogError(ex, "IO error reading file: {FilePath}", filePath);
    throw;
}
```

### Equality & Operators
When overloading equality:
- Implement `IEquatable<T>`
- Override `Equals(object?)`
- Override `GetHashCode()`
- Overload `operator ==`
- Overload `operator !=` as `!(a == b)`

```csharp
public sealed class Customer : IEquatable<Customer>
{
    public int Id { get; init; }
    
    public bool Equals(Customer? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }
    
    public override bool Equals(object? obj) => Equals(obj as Customer);
    
    public override int GetHashCode() => Id.GetHashCode();
    
    public static bool operator ==(Customer? left, Customer? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }
    
    public static bool operator !=(Customer? left, Customer? right) => !(left == right);
}
```

## Pre-Implementation Checklist

Before writing code, verify:
- [ ] Unit tests planned or written first
- [ ] Nullable reference types enabled
- [ ] TreatWarningsAsErrors enabled
- [ ] No forbidden features will be used
- [ ] File organization matches standards
- [ ] Naming conventions understood
- [ ] Exception handling strategy planned for file IO
- [ ] Task tracking system configured (if applicable)

## Common Patterns

### Constructor Dependency Injection
```csharp
public sealed class UserService
{
    private readonly ILogger<UserService> m_Logger;
    private readonly IUserRepository m_Repository;
    
    public UserService(ILogger<UserService> logger, IUserRepository repository)
    {
        m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }
}
```

### Try-Parse Pattern
```csharp
public bool TryParse(string input, out Result result)
{
    result = default;
    
    if (string.IsNullOrWhiteSpace(input))
    {
        return false;
    }
    
    // Parsing logic...
    result = new Result(/* ... */);
    return true;
}
```

### Early Return Pattern
```csharp
public void ProcessOrder(Order order)
{
    if (order is null)
    {
        m_Logger.LogWarning("Order is null");
        return;
    }
    
    if (!order.IsValid)
    {
        m_Logger.LogWarning("Order {OrderId} is invalid", order.Id);
        return;
    }
    
    // Main processing logic without nesting
    order.Process();
    m_Logger.LogInformation("Order {OrderId} processed", order.Id);
}
```

## Verification Steps

After writing code:
1. ✅ All unit tests pass
2. ✅ No compiler warnings
3. ✅ All file IO has try/catch
4. ✅ Member ordering correct
5. ✅ Naming conventions followed
6. ✅ No forbidden features used
7. ✅ Documentation updated (if applicable)
8. ✅ Task tracking updated (if applicable)

## Summary

This skill ensures consistent, maintainable, and high-quality C# code by enforcing strict standards around testing, safety, organization, and style. Always prioritize test-driven development, explicit type safety, and clear code structure.
