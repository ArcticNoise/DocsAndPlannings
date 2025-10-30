---
name: code-review-task-creator-SKILL
description: Thorough code review ensuring quality, standards adherence, and correctness. Use after code is written to verify mandatory rules, architectural patterns, performance requirements, and create Linear tasks for findings. Reviews C++/C# against project standards.
---

# Code Review and Task Creation Skill

## When to Use This Skill

Use this skill whenever code has been written or modified and needs review for quality, standards adherence, and correctness. This includes:

- After implementing new features or components
- After refactoring existing code
- After adding or modifying P/Invoke interop code
- After writing performance-critical systems
- After making changes to data structures or queries
- Before creating pull requests
- When proactive code quality checks are needed

**Key Indicators**: User mentions "review this code", "does this look good", "check my implementation", has just completed a feature, or you've completed writing/modifying code that needs validation.

## Core Review Responsibilities

### 1. Standards Enforcement

Review code against project standards defined in CLAUDE.md or equivalent project documentation:

**Mandatory Rules (🔒)** - These must be followed:
- Unit test coverage for all non-trivial changes
- Proper use of Debug.Assert for critical logic validation
- Nullable reference types enabled and properly annotated
- TreatWarningsAsErrors enabled (no warnings tolerated)
- No prohibited constructs (avoid if specified)
- Thread-safe static code (document thread safety)
- Proper error handling (especially file I/O and interop)
- Correct file naming and organization
- P/Invoke calls have tests validating marshaling and error handling
- Tests genuinely fail when defects are introduced
- Unsafe code properly isolated and wrapped

**Style Guidelines (📝)** - Follow for consistency:
- Naming conventions (PascalCase, camelCase, conventions)
- Proper use of language features (var, expression bodies, etc.)
- Code formatting (indentation, braces, spacing)
- Documentation standards (XML comments for public APIs)
- Error handling patterns (specific exceptions, narrow catch blocks)

### 2. Architecture-Specific Validation

Review code against the project's documented architectural patterns. Common patterns include:

**Data-Oriented Design Patterns** (e.g., ECS, DOD):

**Data Structure Rules**:
```csharp
// ✅ Data structures contain only data
public struct TransformData {
    public float X;
    public float Y;
    public float Rotation;
}

// ❌ Data structures should NOT have methods (except constructors/equality)
public struct TransformData {
    public float X;
    public float Y;

    public void Rotate(float angle) { }  // WRONG: Logic in data structure
}

// ✅ Use IDs for cross-references
public struct ParentData {
    public int ParentId;  // Correct: Reference by ID
}

// ❌ Don't store direct references in data structures
public struct ParentData {
    public Transform Parent;  // WRONG: Direct reference
}
```

**Processing System Rules**:
```csharp
// ✅ Processing systems are stateless
public sealed class MovementSystem {
    public void Update(DataContext context, float deltaTime) {
        var items = context.Query<TransformData, VelocityData>();
        foreach (var (id, transform, velocity) in items) {
            transform.X += velocity.X * deltaTime;
            transform.Y += velocity.Y * deltaTime;
        }
    }
}

// ❌ Processing systems should NOT maintain state
public sealed class MovementSystem {
    private float _accumulatedTime;  // WRONG: Stateful system
}
```

**Performance Rules** (for data-oriented designs):
- Data storage prioritizes memory locality (struct of arrays)
- No allocations in hot paths (pre-allocate, use object pools)
- Queries compiled and cached (not created per frame)
- Explicit execution order for dependencies

**For Other Architectural Patterns**:
- Verify adherence to documented architectural patterns
- Check separation of concerns
- Validate dependency injection usage
- Ensure proper abstraction boundaries

### 3. C# Specific Review Points

**Null Safety**:
```csharp
// ❌ Missing nullable annotation
public string GetName(User user) {  // WRONG: user could be null
    return user.Name;
}

// ✅ Proper nullable handling
public string GetName(User? user) {
    return user?.Name ?? "Unknown";
}

// ❌ Nullable warning suppression without justification
public void Process(string? input) {
    var length = input!.Length;  // WRONG: Unjustified suppression
}

// ✅ Proper null check or justification
public void Process(string? input) {
    if (input == null) throw new ArgumentNullException(nameof(input));
    var length = input.Length;
}
```

**Resource Management**:
```csharp
// ❌ Missing disposal
public void ProcessFile(string path) {
    var stream = File.OpenRead(path);
    // WRONG: Not disposed
}

// ✅ Proper disposal with using
public void ProcessFile(string path) {
    using var stream = File.OpenRead(path);
    // Automatically disposed
}

// ❌ IDisposable not implemented for unmanaged resources
public class NativeWrapper {
    private IntPtr _handle;  // WRONG: No disposal pattern
}

// ✅ Proper IDisposable implementation
public class NativeWrapper : IDisposable {
    private IntPtr _handle;
    private bool _disposed;
    
    public void Dispose() {
        if (!_disposed) {
            if (_handle != IntPtr.Zero) {
                NativeMethods.FreeHandle(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
    }
}
```

**Async Patterns**:
```csharp
// ❌ Async void (except event handlers)
public async void ProcessData() {  // WRONG: Can't be awaited
    await Task.Delay(100);
}

// ✅ Return Task
public async Task ProcessDataAsync() {
    await Task.Delay(100);
}

// ❌ Blocking on async
public void Execute() {
    var result = GetDataAsync().Result;  // WRONG: Deadlock risk
}

// ✅ Async all the way
public async Task ExecuteAsync() {
    var result = await GetDataAsync();
}
```

### 4. C++ Specific Review Points

**Memory Safety**:
```cpp
// ❌ Raw pointer with manual management
char* CreateBuffer(size_t size) {
    return new char[size];  // WRONG: Caller must remember to delete
}

// ✅ Smart pointer or RAII
std::unique_ptr<char[]> CreateBuffer(size_t size) {
    return std::make_unique<char[]>(size);
}

// ❌ Use after free
void ProcessData() {
    auto* data = new Data();
    delete data;
    data->Process();  // WRONG: Use after free
}

// ❌ Dangling reference
const std::string& GetName() {
    std::string name = "Test";
    return name;  // WRONG: Returns reference to local
}

// ✅ Return by value or use output parameter
std::string GetName() {
    return "Test";
}
```

**Thread Safety**:
```cpp
// ❌ Race condition
class Counter {
    int value = 0;
public:
    void Increment() { ++value; }  // WRONG: Not thread-safe
};

// ✅ Proper synchronization
class Counter {
    std::atomic<int> value{0};
public:
    void Increment() { ++value; }
};

// ❌ Lock not released on exception
void Process() {
    mutex.lock();
    DoWork();  // May throw
    mutex.unlock();  // WRONG: Not reached if exception
}

// ✅ RAII lock guard
void Process() {
    std::lock_guard<std::mutex> lock(mutex);
    DoWork();  // Exception safe
}
```

### 5. Interop Review Points

**P/Invoke Declarations**:
```csharp
// ❌ Missing error handling
[DllImport("NativeLib.dll")]
private static extern IntPtr CreateObject();  // WRONG: No error check

// ✅ Error handling with return code
[DllImport("NativeLib.dll")]
private static extern int CreateObject(out IntPtr handle);

public static IntPtr CreateObjectSafe() {
    int result = CreateObject(out IntPtr handle);
    if (result != 0) {
        throw new InvalidOperationException($"Failed to create object: {result}");
    }
    return handle;
}

// ❌ Incorrect calling convention
[DllImport("NativeLib.dll")]  // WRONG: Missing CallingConvention
private static extern void DoWork();

// ✅ Explicit calling convention
[DllImport("NativeLib.dll", CallingConvention = CallingConvention.Cdecl)]
private static extern void DoWork();
```

**Structure Marshaling**:
```csharp
// ❌ Ambiguous layout
public struct NativeConfig {
    public int Value1;
    public bool Flag;  // WRONG: Size/padding unclear
    public int Value2;
}

// ✅ Explicit layout and marshaling
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct NativeConfig {
    public int Value1;
    [MarshalAs(UnmanagedType.I1)]
    public bool Flag;
    public int Value2;
}
```

**Memory Ownership**:
```cpp
// C++ side - Document ownership
/// @brief Creates a new processor instance
/// @return Pointer to processor. Caller must call DestroyProcessor() to free.
extern "C" __declspec(dllexport)
ProcessorHandle* CreateProcessor();

/// @brief Destroys a processor instance
/// @param handle Processor to destroy (must not be null)
extern "C" __declspec(dllexport)
void DestroyProcessor(ProcessorHandle* handle);
```

```csharp
// C# side - Implement SafeHandle
public class ProcessorSafeHandle : SafeHandle {
    public ProcessorSafeHandle() : base(IntPtr.Zero, true) { }
    
    protected override bool ReleaseHandle() {
        NativeMethods.DestroyProcessor(handle);
        return true;
    }
    
    public override bool IsInvalid => handle == IntPtr.Zero;
}
```

## Code Review Process

### Step 1: Initial Assessment

```
1. Understand the scope
   - What files were changed?
   - What is the purpose of the changes?
   - What areas of the system are affected?

2. Identify risk areas
   - Performance-critical code (data queries, hot paths)
   - Security-sensitive code (P/Invoke, file I/O, user input)
   - Complex algorithms or data structures
   - Thread synchronization code

3. Check for test coverage
   - Are there unit tests for the changes?
   - Do tests cover edge cases?
   - Are P/Invoke calls tested?
```

### Step 2: Mandatory Rules Verification

**Checklist for every review**:

- [ ] **Unit Tests**: All non-trivial code has tests
  - Tests pass on first run
  - Tests fail when defects introduced (validated)
  - Tests cover happy path and edge cases
  - P/Invoke marshaling validated

- [ ] **Null Safety**: Nullable reference types properly used
  - Public APIs annotated correctly
  - Null checks before dereference
  - No unjustified null-forgiving operators (!)

- [ ] **Error Handling**: Exceptions and errors properly handled
  - File I/O has error handling
  - P/Invoke return codes checked
  - Specific exception types used
  - Resources cleaned up on error paths

- [ ] **Thread Safety**: Static/shared state is thread-safe
  - Documented thread safety guarantees
  - Proper synchronization (locks, atomics)
  - No data races

- [ ] **Build Configuration**:
  - TreatWarningsAsErrors enabled
  - No warnings in output
  - Nullable reference types enabled

- [ ] **Prohibited Constructs** (if specified):
  - No global usings
  - No singletons (if prohibited)
  - No local functions (if prohibited)
  - No dynamic keyword
  - No LINQ query syntax (if prohibited)

### Step 3: Architecture Validation

**For Data-Oriented Architectures**:

- [ ] **Data Structures**:
  - Only contain data fields (POD)
  - No methods except constructors/equality
  - Value types (struct) for better performance
  - Immutable where possible

- [ ] **Processing Systems**:
  - Stateless (no instance fields)
  - Query/iterate by data signature
  - No direct data storage
  - Execution order documented

- [ ] **Queries/Iteration**:
  - Compiled and cached (not recreated per loop)
  - Efficient iteration patterns
  - No allocations in hot paths

- [ ] **Cross-References**:
  - Use IDs, not direct references
  - Validity checks before access

**For General Architectures**:

- [ ] **Separation of Concerns**: Each class has single responsibility
- [ ] **Dependency Injection**: Proper use of DI patterns
- [ ] **SOLID Principles**: Code adheres to SOLID where applicable
- [ ] **Performance**: No obvious performance issues

### Step 4: Style Guidelines Check

- [ ] **Naming Conventions**:
  - Classes/Methods: PascalCase
  - Fields/Parameters: camelCase (private fields may use _prefix)
  - Constants: CONSTANT_CASE or PascalCase
  - Interfaces: IPrefixed

- [ ] **Code Organization**:
  - File per type (unless nested types)
  - Correct file naming matches type name
  - Proper namespace organization
  - Member ordering (constants, fields, properties, methods)

- [ ] **Language Features**:
  - Proper use of var (obvious types)
  - Expression bodies for simple properties only
  - Null-coalescing operators (??, ?.)
  - Collection initializers where appropriate
  - Pattern matching for type checks

- [ ] **Formatting**:
  - Consistent indentation (4 spaces or tabs as per project)
  - Braces on new lines (C# Allman style) or same line (C++ K&R)
  - File-scoped namespaces (C# 10+)
  - No trailing whitespace

- [ ] **Documentation**:
  - XML comments on public APIs
  - Complex logic has explanatory comments
  - TODO/HACK/NOTE properly marked
  - No commented-out code committed

### Step 5: Test Quality Assessment

**Test Coverage**:
```csharp
// ❌ Test doesn't actually verify behavior
[Fact]
public void ProcessData_Works() {
    var processor = new DataProcessor();
    processor.Process(testData);
    // WRONG: No assertion!
}

// ✅ Test verifies expected behavior
[Fact]
public void ProcessData_TransformsInputCorrectly() {
    var processor = new DataProcessor();
    var input = new byte[] { 1, 2, 3 };
    
    var result = processor.Process(input);
    
    Assert.NotNull(result);
    Assert.Equal(3, result.Length);
    Assert.All(result, b => Assert.InRange(b, 0, 255));
}

// ❌ Test won't fail if implementation broken
[Fact]
public void ValidateInput_ChecksForNull() {
    var validator = new InputValidator();
    validator.Validate(null);
    // WRONG: Should expect exception or return false
}

// ✅ Test genuinely validates behavior
[Fact]
public void ValidateInput_ThrowsOnNull() {
    var validator = new InputValidator();
    
    Assert.Throws<ArgumentNullException>(() => validator.Validate(null));
}
```

**Test Organization**:
- [ ] Tests organized by scenario (happy path, edge cases, errors)
- [ ] Test names clearly describe what they test
- [ ] Arrange-Act-Assert pattern used
- [ ] No test interdependencies
- [ ] Mock only external dependencies

## Task Creation Guidelines

### Task Format

For each issue found, create a task with this structure:

```markdown
**Title**: [Action Verb] [Component/Area]: [Specific Issue]

Example: "Fix MovementSystem: Missing null check for velocity component"
Example: "Add unit tests for NativeMemory P/Invoke marshaling"
Example: "Refactor PlayerController: Remove stateful system design"

**Priority**: [Critical / High / Medium / Low]
- Critical: Security vulnerability, data corruption, crashes
- High: Mandatory rule violation, architectural issue
- Medium: Style guideline violation, missing tests
- Low: Documentation, minor refactoring

**Category**: [Code Quality / Testing / Architecture / Performance / Documentation]

**Description**:

## Problem
[Clearly describe what is wrong, with specific file and line references]

File: `src/Systems/MovementSystem.cs`
Lines: 45-52

The MovementSystem queries for VelocityComponent but doesn't check if the component is valid before accessing its properties, leading to potential NullReferenceException.

## Why This Is An Issue
[Explain why this violates standards or causes problems]

This violates the mandatory rule requiring null checks before dereferencing (CLAUDE.md section 3.2). It can cause crashes during gameplay when entities have partial component sets.

## How To Fix
[Provide specific, actionable steps with code examples]

Add a null check before accessing the velocity component:

**Current (Incorrect)**:
```csharp
public void Update(World world, float deltaTime) {
    var query = world.Query<TransformComponent, VelocityComponent>();
    foreach (var (entity, transform, velocity) in query) {
        transform.X += velocity.X * deltaTime;  // Unsafe access
    }
}
```

**Fixed (Correct)**:
```csharp
public void Update(World world, float deltaTime) {
    var query = world.Query<TransformComponent, VelocityComponent>();
    foreach (var (entity, transform, velocity) in query) {
        if (!velocity.IsValid) continue;  // Add safety check
        
        transform.X += velocity.X * deltaTime;
        transform.Y += velocity.Y * deltaTime;
    }
}
```

## Additional Context
[Related issues, references, or side effects]

- Similar issue exists in RotationSystem.cs line 38
- Add test case for entities with incomplete component sets
- Reference: CLAUDE.md section 3.2.1 "Component Access Safety"

## Acceptance Criteria
- [ ] Null check added before velocity access
- [ ] Unit test added for missing component scenario
- [ ] Test validates exception is NOT thrown
- [ ] Similar patterns in other systems reviewed
```

### Task Quality Standards

**Every task must be**:
- ✅ **Autonomous**: Can be completed independently without dependencies
- ✅ **Specific**: References exact files, lines, and code
- ✅ **Actionable**: Provides clear steps to fix
- ✅ **Educational**: Explains why and how to fix correctly
- ✅ **Verifiable**: Has acceptance criteria to validate completion

**Avoid**:
- ❌ Vague descriptions ("improve code quality")
- ❌ Missing context (no file/line references)
- ❌ No solution provided (just points out problem)
- ❌ Unclear priority or category
- ❌ Dependencies on other tasks

### Task Prioritization Guide

**Critical** - Fix immediately:
- Security vulnerabilities (buffer overflows, injection, auth bypass)
- Data loss or corruption bugs
- System crashes or deadlocks
- Memory corruption in P/Invoke

**High** - Fix this sprint:
- Mandatory rule violations (missing tests, warnings, nullable issues)
- Architectural violations (stateful systems, logic in components)
- Major performance issues (allocations in hot paths)
- Missing error handling in file I/O or P/Invoke

**Medium** - Fix soon:
- Style guideline violations
- Missing documentation on public APIs
- Minor performance improvements
- Code organization issues

**Low** - Nice to have:
- Cosmetic improvements
- Additional code comments
- Refactoring for clarity (without functional change)

## Review Output Format

Provide your review in this structure:

```markdown
# Code Review Summary

**Date**: [Current Date]
**Reviewer**: Claude
**Files Reviewed**: [List of files]
**Lines Changed**: [Approximate count]

## Executive Summary

**Overall Assessment**: [Excellent / Good / Needs Work / Critical Issues]

**Tasks Created**: [Total Count]
- 🔴 Critical: [Count]
- 🟠 High: [Count]
- 🟡 Medium: [Count]
- 🟢 Low: [Count]

**Key Findings**:
- [Most important issue #1]
- [Most important issue #2]
- [Most important issue #3]

**Positive Aspects**:
- [What was done well]
- [Good patterns observed]

## Detailed Findings

### Critical Issues (Fix Immediately)

#### Issue #1: [Title]
[Full task description as formatted above]

---

### High Priority Issues

#### Issue #2: [Title]
[Full task description]

---

### Medium Priority Issues

#### Issue #3: [Title]
[Full task description]

---

### Low Priority Issues

#### Issue #4: [Title]
[Full task description]

---

## Recommendations

### Immediate Actions
1. [Critical fixes to make now]
2. [High priority items to address]

### Process Improvements
1. [Suggestions to prevent similar issues]
2. [Additional checks to add to workflow]

### Follow-Up Items
1. [Areas that need further review]
2. [Documentation to update]

## Test Coverage Analysis

**Current Coverage**: [Percentage if known]

**Missing Tests**:
- [Test scenario #1]
- [Test scenario #2]

**Test Quality**: [Assessment of existing tests]

## Compliance Checklist

✅ / ❌ Unit tests present and passing
✅ / ❌ Nullable reference types properly used
✅ / ❌ No warnings (TreatWarningsAsErrors)
✅ / ❌ Error handling complete
✅ / ❌ Thread safety documented
✅ / ❌ P/Invoke calls tested
✅ / ❌ Architecture follows project patterns
✅ / ❌ Style guidelines followed
✅ / ❌ Documentation complete
```

## Common Review Patterns

### Pattern 1: Missing Test Coverage

```markdown
**Title**: Add Unit Tests for ComponentArray<T> Insertion Logic

**Priority**: High

**Description**:

## Problem
The `ComponentArray<T>.Add()` method in `src/Storage/ComponentArray.cs` (lines 67-82) lacks unit tests validating edge cases.

## Why This Is An Issue
Violates mandatory rule: "All non-trivial changes require unit tests" (CLAUDE.md 2.1). Component storage is critical infrastructure that must be thoroughly tested.

## How To Fix
Create test class `ComponentArrayTests.cs` with these scenarios:

```csharp
public class ComponentArrayTests {
    [Fact]
    public void Add_IncreasesCount() {
        var array = new ComponentArray<TransformComponent>();
        var component = new TransformComponent { X = 10 };
        
        array.Add(0, component);
        
        Assert.Equal(1, array.Count);
    }
    
    [Fact]
    public void Add_WithSameEntityTwice_Throws() {
        var array = new ComponentArray<TransformComponent>();
        array.Add(0, new TransformComponent());
        
        Assert.Throws<InvalidOperationException>(() => 
            array.Add(0, new TransformComponent()));
    }
    
    [Fact]
    public void Add_ExceedingCapacity_ResizesArray() {
        var array = new ComponentArray<TransformComponent>(initialCapacity: 2);
        
        array.Add(0, new TransformComponent());
        array.Add(1, new TransformComponent());
        array.Add(2, new TransformComponent());  // Should trigger resize
        
        Assert.Equal(3, array.Count);
        Assert.True(array.Capacity >= 3);
    }
}
```

## Acceptance Criteria
- [ ] Test class created with 3+ test methods
- [ ] All tests pass
- [ ] Edge cases covered (empty, duplicate, capacity)
- [ ] Tests actually fail when implementation broken
```

### Pattern 2: P/Invoke Missing Error Handling

```markdown
**Title**: Fix NativeMemory: Add Error Handling to P/Invoke Calls

**Priority**: Critical

**Description**:

## Problem
`NativeMemory.Allocate()` in `src/Interop/NativeMemory.cs` (line 23) doesn't check the return code from native `Alloc()` function.

```csharp
[DllImport("MemoryLib.dll")]
private static extern IntPtr Alloc(int size);

public static IntPtr Allocate(int size) {
    return Alloc(size);  // No error checking!
}
```

## Why This Is An Issue
Violates mandatory rule: "P/Invoke calls must check error codes" (CLAUDE.md 4.3). If native allocation fails, returning IntPtr.Zero will cause crashes when dereferenced.

## How To Fix
Update P/Invoke to return error code and check it:

```csharp
[DllImport("MemoryLib.dll", CallingConvention = CallingConvention.Cdecl)]
private static extern int Alloc(int size, out IntPtr handle);

public static IntPtr Allocate(int size) {
    if (size <= 0) {
        throw new ArgumentOutOfRangeException(nameof(size));
    }
    
    int result = Alloc(size, out IntPtr handle);
    if (result != 0) {
        throw new OutOfMemoryException(
            $"Native allocation failed with code: {result}");
    }
    
    return handle;
}
```

Also update native side to return error codes:

```cpp
extern "C" __declspec(dllexport)
int Alloc(int size, void** handle) {
    if (size <= 0) return -1;  // Invalid size
    if (handle == nullptr) return -2;  // Invalid parameter
    
    *handle = malloc(size);
    return (*handle != nullptr) ? 0 : -3;  // 0 = success, -3 = out of memory
}
```

## Additional Context
- Add test: `Allocate_WithZeroSize_ThrowsArgumentOutOfRangeException`
- Add test: `Allocate_WhenNativeFails_ThrowsOutOfMemoryException`
- Review all P/Invoke calls in NativeMemory class for similar issues

## Acceptance Criteria
- [ ] P/Invoke returns error code, not pointer
- [ ] C# wrapper throws appropriate exceptions
- [ ] Native code returns error codes
- [ ] Unit tests verify error handling
- [ ] All tests pass
```

### Pattern 3: Data Structure Architecture Violation

```markdown
**Title**: Refactor PlayerData: Remove Method Logic from Data Structure

**Priority**: High

**Description**:

## Problem
`PlayerData` in `src/Data/PlayerData.cs` contains methods with business logic:

```csharp
public struct PlayerData {
    public int Health;
    public int MaxHealth;

    // WRONG: Logic in data structure
    public void TakeDamage(int amount) {
        Health = Math.Max(0, Health - amount);
    }

    public bool IsAlive() => Health > 0;
}
```

## Why This Is An Issue
Violates data-oriented design principle: "Data structures contain only data fields, no methods except constructors/equality" (project architectural guidelines). Methods in data structures break separation of concerns and prevent effective optimization.

## How To Fix
1. Remove methods from data structure:

```csharp
public struct PlayerData {
    public int Health;
    public int MaxHealth;
}
```

2. Move logic to processing system:

```csharp
public sealed class HealthProcessor {
    public void ApplyDamage(DataContext context, int playerId, int damage) {
        if (!context.TryGetData<PlayerData>(playerId, out var player)) {
            return;
        }

        player.Health = Math.Max(0, player.Health - damage);
        context.UpdateData(playerId, player);

        // Emit event if dead
        if (player.Health == 0) {
            context.EmitEvent(new PlayerDeathEvent { PlayerId = playerId });
        }
    }
}
```

## Additional Context
- Create `HealthProcessor` class in `src/Processors/`
- Update all call sites to use processor instead of data methods
- Consider adding event system for state changes

## Acceptance Criteria
- [ ] All methods removed from PlayerData
- [ ] HealthProcessor created with damage logic
- [ ] All usages updated to call processor
- [ ] Unit tests for HealthProcessor added
- [ ] No regression in functionality
```

## Special Considerations

### Performance-Critical Code

When reviewing hot paths (data queries, update loops):

```csharp
// ❌ Allocation in iteration loop
foreach (var item in collection) {
    var list = new List<int>();  // WRONG: Allocation per item
    // ...
}

// ✅ Pre-allocate and reuse
private List<int> _reusableList = new List<int>();

foreach (var item in collection) {
    _reusableList.Clear();
    // ... use _reusableList
}

// ❌ Query/filter creation per iteration
public void Update() {
    var filtered = data.Where(x => x.Active);  // WRONG: Creates new enumerable
    foreach (var item in filtered) { }
}

// ✅ Cached query/filter
private IEnumerable<T> _cachedFilter;

public void Initialize(DataContext context) {
    _cachedFilter = context.CreateFilter(x => x.Active);
}

public void Update() {
    foreach (var item in _cachedFilter) { }
}
```

### Security-Sensitive Code

Extra scrutiny for:
- User input validation
- File path handling
- SQL/command injection vectors
- Authentication/authorization logic
- Cryptographic operations

### Third-Party Code

If code appears to be from external sources:
- Verify licensing compatibility
- Check for security vulnerabilities
- Ensure proper attribution
- Validate that it follows project standards

## Self-Verification Checklist

Before completing your review, verify:

- [ ] Checked all mandatory rules (🔒)
- [ ] Verified project-specific architectural constraints
- [ ] All tasks are autonomous and actionable
- [ ] Each task has specific fix instructions with code examples
- [ ] Issues prioritized correctly
- [ ] Referenced specific files and line numbers
- [ ] Explained why issues matter, not just what's wrong
- [ ] Provided positive feedback on good patterns
- [ ] Created comprehensive summary with metrics

## Escalation Scenarios

Create high-priority tasks and recommend discussion for:

- **Fundamental architecture conflicts**: Code contradicts documented design
- **Missing critical infrastructure**: No build system, test framework missing
- **Ambiguous requirements**: Multiple valid interpretations possible
- **External dependencies**: Unclear licensing or security concerns
- **Performance cliffs**: Algorithmic complexity issues (O(n²) where O(n) needed)

## Final Notes

Remember:
- **Be thorough but constructive**: Frame issues as improvement opportunities
- **Be specific**: Vague feedback isn't actionable
- **Be educational**: Explain the "why" behind each issue
- **Be consistent**: Apply standards uniformly
- **Be practical**: Focus on impactful issues, not nitpicking

Your role is to ensure code quality, maintainability, and adherence to project standards. Every issue you catch prevents technical debt and improves the codebase's reliability. You are a guardian of quality, helping the team build better software.
