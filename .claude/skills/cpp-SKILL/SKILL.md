---
name: cpp-SKILL
description: Comprehensive C++ coding standards covering testing, safety, warnings, naming, formatting, RAII, move semantics, and best practices. Enforces mandatory rules for quality C++ development with modern C++ features and strict safety requirements.
---

# C++ Coding Standards Skill

## Overview
This skill defines comprehensive C++ coding standards covering testing, safety, naming conventions, formatting, and best practices. These standards ensure consistent, maintainable, and high-quality C++ code across all projects.

## Build Targets

### Standard Build Configurations

| Target | Optimizations | Asserts | Console |
|--------|--------------|---------|---------|
| Debug | No | Yes | Yes |
| Release | Yes | Yes | Yes |
| Shipping | Yes | No | Yes |
| Gold | Full (LTCG/PGO) | No | No |

**Configuration Guidelines:**
- **Debug**: Unoptimized builds for active development with full debugging support
- **Release**: Optimized builds for testing with assertions enabled
- **Shipping**: Fully optimized production builds without assertions
- **Gold**: Maximum optimization with Link-Time Code Generation (LTCG) and Profile-Guided Optimization (PGO)

## Asserts

### Assertion Philosophy
**In all cases, avoid adding asserts in the first place.** Fail as early as possible through:
1. Compile-time checks
2. Data validation at import/input stage
3. Type system enforcement

### Assertion Hierarchy (Best to Worst)
1. **Compile-time validation**: Use `static_assert` when possible
2. **Type system**: Use references instead of pointers when null is invalid; use unsigned types when negative values are invalid
3. **Return error codes**: Return error results that can be handled by callers
4. **Asserts** (last resort only)

### When to Use Asserts
Asserts should only be used as a **last resort** when:
- Compile-time checks are impossible
- Type system cannot enforce the constraint
- Error handling would make code significantly more complex

### Assertion Best Practices

**Provide maximum information:**
```cpp
// ❌ BAD - Minimal information
LS_ASSERT(character, "Invalid character");

// ✅ GOOD - Rich context
LS_ASSERT(sourceCharacter, 
    "Invalid source character (UUID: %s - Name: %s)!", 
    sourceCharacter->GetUUID(), 
    sourceCharacter->GetName());
```

**Assert classification by department:**
```cpp
LS_ASSERT(condition, message, ...);              // Code department
LS_ASSERT_CRITICAL(condition, message, ...);     // Critical code assert
LS_SCRIPT_ASSERT(condition, message, ...);       // Script department
LS_ART_ASSERT(condition, message, ...);          // Art department
```

**Alternative: LOG for already-checked conditions:**
```cpp
if (!isValid)
{
    LOG(ls::LOG_AssertProgrammer, 
        "Plugin %ls is an older version: %s\nPlease update the plugin.", 
        path, version.ToString());
    return;
}
```

### Critical Requirements
- **No side effects in asserts**: Asserts may be compiled out in certain builds
- **Rich context**: Include all relevant information (IDs, names, values, context)
- **Consider ls::Result**: For validations that need error propagation

```cpp
// ✅ Prefer returning error results
ls::Result<Data> LoadData(const std::string& path)
{
    if (!FileExists(path))
    {
        return ls::ErrorLogString("File not found: %s", path.c_str());
    }
    // ... load data
}
```

## Warnings

**All code must pass warning-free on:**
- Visual Studio (MSVC)
- Clang
- GCC

**Static analysis requirements:**
- PVS-Studio
- CPPCheck

**Configuration:**
- Enable all reasonable warnings
- Treat warnings as errors in CI/CD pipelines

## Includes

### Include Rules

1. **Don't include what you don't use**
2. **Use forward declarations where possible**
3. **No platform-specific includes in headers**
4. **Include with full path**: `#include <GameEngine/Decal.h>`
5. **Case-sensitive paths**: Critical for cross-platform compatibility
6. **Include what you use**: Don't depend on transitive includes
7. **Actively remove unused includes**: They impact build times

### Include Order and Formatting

**Include types:**
- `""` for project-wide includes
- `<>` for STL and 3rd party libraries

**File structure:**
```cpp
#include "ProjectPCH.h"           // PCH first
#include "Module/ThisFile.h"      // Associated header second

#include "CoreLib/Optional.h"     // Project includes
#include "GameEngine/PhysicsObject.h"
#include "Module/IncludeA.h"
#include "Module/IncludeB.h"
#include "Shared/IncludeA.h"

#include <imgui.h>                // 3rd party and STL
#include <type_traits>
```

**Sorting rules:**
- Sort each path group alphabetically (case-insensitive)
- Single blank line between logical groups
- No consecutive empty lines

**Conditional includes:**
```cpp
#include "Module/Include1.h"
#include "Module/Include3.h"

#ifdef PLATFORM_WINDOWS
    #include "Module/Include2.h"
    #include "Module/Include4.h"
#endif
```

### Header Guards
Use `#pragma once` for header guards.

## Nesting

**FORBIDDEN: Nesting public classes inside other classes**

This breaks forward declarations and forces users to include the entire header.

```cpp
// ❌ BAD - Nested public class
class Outer
{
public:
    class Inner  // Cannot be forward declared!
    {
        // ...
    };
};

// ✅ GOOD - Separate classes
class Inner
{
    // ...
};

class Outer
{
    Inner* m_Inner;  // Can be forward declared
};
```

## Namespaces

### Namespace Guidelines

1. **Keep names short**: Prefer abbreviations where clear
2. **Include module name**: Namespace should reflect the module/library
3. **Use snake_case**: `ls::my_module::longer_namespace`
4. **Never use `using namespace`**: Always fully scope declarations (critical for unity builds)
5. **Prefer namespace to module name**: Or use clear abbreviation

### Hiding Implementation Details

**Use anonymous namespaces for file-local items:**
```cpp
// ✅ GOOD - Anonymous namespace for file-local functions
namespace
{
    void InternalHelper()
    {
        // Only visible in this translation unit
    }
    
    constexpr int INTERNAL_CONSTANT = 42;
}

void PublicFunction()
{
    InternalHelper();
}
```

**Why anonymous namespaces are superior:**
- Better than named namespaces (which don't truly hide)
- Better than `static` (more C++-idiomatic)
- Prevents name clashes in unity builds
- Clear intent: "This is internal"

## Defines

### Preprocessor Guidelines

1. **Avoid compiler-specific #pragma's**
   - Handle through CMake when possible
   - If unavoidable, wrap in compiler detection:
   ```cpp
   #ifdef _MSC_VER
       #pragma warning(disable: 4251)
   #endif
   ```

2. **Use `#pragma once` for header guards**

3. **Avoid environment-specific defines in cross-platform code**
   - Editor code should use editor-specific systems, not `#ifdef LS_EDITOR`

## Headers

### Header Best Practices

1. **Keep headers simple**: Minimal dependencies, clear structure
2. **Add Doxygen documentation**: Brief description of purpose

```cpp
#pragma once

/// @brief Manages physics simulation for game objects
/// Provides rigid body dynamics and collision detection
class PhysicsManager
{
    // ...
};
```

## Naming Conventions

### General Rules

**Pointer and reference modifiers:**
```cpp
// ✅ GOOD - Modifier immediately after type
MyType* pointer;
MyType& reference;

// ❌ BAD
MyType * pointer;
MyType *pointer;
```

**Const specifiers:**
```cpp
// ✅ GOOD - Space before const
void MyFunction() const;

// ❌ BAD
void MyFunction()const;
```

### Member Variables

**Prefix with `m_` followed by PascalCase:**
```cpp
struct MyStruct
{
    int32_t m_MyMember;
    float m_Speed;
    std::string m_Name;
};
```

**Do NOT use Hungarian notation:**
```cpp
// ❌ BAD
int m_iCount;
float m_fSpeed;

// ✅ GOOD
int m_Count;
float m_Speed;
```

### Functions and Methods

**PascalCase starting with capital:**
```cpp
// ✅ GOOD
void MyFunction();
int CalculateDistance();

// ❌ BAD
void myFunction();
void my_function();
```

**Always use `override` and `final` keywords:**
```cpp
class Base
{
public:
    virtual void Update();
    virtual void Render() = 0;
};

class Derived final : public Base  // final - cannot be inherited
{
public:
    void Update() override;         // override - makes intent clear
    void Render() override;
};
```

### Classes and Structs

**PascalCase starting with capital:**
```cpp
// ✅ GOOD
class MyClass {};
struct DataContainer {};

// ❌ BAD - Don't prefix with C, T, or S
class CMyClass {};      // NO
class TMyClass {};      // NO
struct SDataContainer; // NO
```

**Exception: Template parameters start with T:**
```cpp
template <class TObject, class TFactory>
class Manager {};
```

### Typedefs and Enums

**Type aliases with T prefix (prefer using over typedef):**
```cpp
// ✅ GOOD
using TMyMap = std::map<int, std::string>;

// Acceptable but prefer 'using'
typedef std::map<int, std::string> TMyMapOld;
```

**Enum declarations prefixed with E:**
```cpp
// ✅ GOOD
enum EMyEnum
{
    eMyEnum_FirstValue,
    eMyEnum_SecondValue
};

// Enum class (preferred for strong typing)
enum class EState
{
    Idle,
    Running,
    Paused
};
```

**Enum values prefixed with e:**
```cpp
enum EMyEnum
{
    eAllYourBase_BelongsToUs,  // Optional: prefix with type name
    eValue2,
    eValue3
};
```

### Template Arguments

**PascalCase with T prefix:**
```cpp
template <class TObject, class TObjectFactory>
class Factory
{
    // ...
};
```

### Macros

**ALL_CAPS with underscores:**
```cpp
#define MY_MACRO 42
#define CALCULATE_SIZE(x) ((x) * sizeof(int))
```

**Prefer constexpr over macros:**
```cpp
// ✅ PREFER
constexpr int MAX_SIZE = 100;

// ❌ AVOID
#define MAX_SIZE 100
```

### Local Variables

**camelCase starting with lowercase:**
```cpp
void MyFunction()
{
    int localVariable = 0;
    float mySpeed = 1.0f;
    std::string userName = "player";
}
```

### Globals and Statics

**Avoid globals when possible!**

When necessary:
- **Globals**: `g_` prefix + PascalCase
- **Statics and constexpr**: `s_` prefix + PascalCase

```cpp
// Global (avoid!)
int g_GlobalCounter = 0;

// Static
static std::string s_DefaultName = "Unknown";

// constexpr static
static constexpr float s_Pi = 3.14159f;

// In anonymous namespace (preferred over static)
namespace
{
    constexpr float s_Gravity = 9.81f;
}
```

### Function Parameters

**camelCase for regular parameters:**
```cpp
void MyFunction(float speed, int count);
```

**Input/output parameters prefixed with `inout_` + PascalCase:**
```cpp
void MyFunction(float& inout_Speed, int& out_Count);
```

**Use structs for long parameter lists:**
```cpp
// ❌ BAD - Too many parameters
void CreateObject(bool flag, float x, float y, float z, 
                  int id, const std::string& name, 
                  float speed, int health);

// ✅ GOOD - Use parameter struct
struct CreateObjectParams
{
    bool flag;
    glm::vec3 position;
    int id;
    std::string name;
    float speed;
    int health;
};

void CreateObject(const CreateObjectParams& params);
```

### Boolean Members

**Prefix with Is/Can/Has, use affirmative terms:**
```cpp
bool IsEnabled;
bool CanMove;
bool HasPermission;

// ❌ Avoid negative names
bool IsNotDisabled;  // Confusing!
```

## Formatting

### Indentation

**Use tabs with 4-space width for C++:**
- Set your IDE tab width to 4 spaces
- Use actual tab characters, not spaces

**Visual Studio:** Tools → Options → Text Editor → All Languages → Tabs → Tab size: 4  
**Sublime Text:** Preferences → Settings → `"tab_size": 4`  
**Notepad++:** Preferences → Settings → Language → Tab Settings → Tab size: 4

### Bracketing

**Avoid Egyptian brackets:**
```cpp
// ✅ GOOD
if (condition)
{
    DoSomething();
}

// ❌ BAD - Egyptian brackets
if (condition) {
    DoSomething();
}
```

**Single-line return/break/continue:**
Brackets are optional for single-line `return`, `break`, and `continue`:
```cpp
// ✅ Both acceptable
if (x)
    return;

if (x)
{
    return;
}
```

**ALWAYS scope MACROs:**
```cpp
// ❌ DANGEROUS - Macro not scoped
if (error)
    LOG_ERROR("Error occurred");
    CleanUp();  // Always executes if LOG_ERROR is multi-line!

// ✅ GOOD - Always use braces with macros
if (error)
{
    LOG_ERROR("Error occurred");
}
CleanUp();
```

### RAII and Scoping

**Use brackets to force scoping:**
```cpp
void ProcessData()
{
    // Scope guards lifetime
    {
        std::lock_guard<std::mutex> lock(m_Mutex);
        // Protected operations
    }  // lock released here
    
    // Continue with unprotected operations
}
```

## Const Correctness

### Const Guidelines

1. **Don't cast away const** (except for const-incorrect external APIs)
2. **Make temporary variables const**:
   ```cpp
   const size_t len = vector.size();  // Clearly won't change
   ```
3. **Don't use const on return values of value types**:
   ```cpp
   // ❌ BAD
   const int GetValue() const;
   
   // ✅ GOOD
   int GetValue() const;
   const int& GetValue() const;  // Returning reference is fine
   ```
4. **Always prefer const for input parameters**:
   ```cpp
   void Process(const std::string& input);
   ```
5. **Functions should be logically const**

### Method Const Correctness

**Returning pointers to internals:**
```cpp
class Container
{
private:
    Data* m_Data;
    
public:
    // ❌ BAD - Allows modification through const method
    Data* GetData() const { return m_Data; }
    
    // ✅ GOOD - Const pointer from const method
    const Data* GetData() const { return m_Data; }
    
    // ✅ GOOD - Non-const overload for non-const access
    Data* GetData() { return m_Data; }
};
```

## Mutable Keyword

### Mutable is PROHIBITED

The `mutable` keyword is **forbidden** in the codebase with only two exceptions:

**Why mutable is bad:**
- Breaks const correctness guarantees
- Prevents data-race detection
- Usually indicates poor design
- Hides true intent

**Exception 1: Mutex in const methods**
```cpp
class ThreadSafeContainer
{
private:
    mutable std::mutex m_Mutex;  // ✅ Allowed
    std::vector<int> m_Data;
    
public:
    int GetSize() const
    {
        std::lock_guard<std::mutex> lock(m_Mutex);
        return m_Data.size();
    }
};
```

**Exception 2: Debug-only tracking**
```cpp
class Tracker
{
private:
#ifndef NDEBUG
    mutable int m_DebugAccessCount;  // ✅ Allowed behind debug define
#endif
    
public:
    void DoWork() const
    {
#ifndef NDEBUG
        ++m_DebugAccessCount;
#endif
    }
};
```

## Auto Keyword

### Use Auto with Caution

**We've seen many bugs where containers were copied instead of referenced.**

**When to use auto:**

1. **Very long template types** (consider using alias first):
   ```cpp
   using TComplexMap = std::unordered_map<std::string, std::vector<MyClass>>;
   ```

2. **For loops / iterators**:
   ```cpp
   for (const auto& item : container)
   {
       // ...
   }
   ```

3. **When type is truly unknown** (rare, usually template metaprogramming)

### Auto Requirements

**When using auto, ALWAYS specify const, *, or & explicitly:**
```cpp
// ✅ GOOD - Explicit qualifiers
const auto& item = container.front();
auto* pointer = GetPointer();
const auto* constPointer = GetConstPointer();

// ❌ BAD - Implicit, could cause copies
auto item = container.front();
```

**Exceptions for readability:**
```cpp
auto a = static_cast<Type>(b);
const auto* a = dynamic_cast<const Type*>(b);
auto a = new Type();
```

**Avoid verbose casting:**
```cpp
// ❌ TOO VERBOSE
SceNpMatching2GetRoomDataExternalListResponseA* respData = 
    static_cast<const SceNpMatching2GetRoomDataExternalListResponseA*>(data);

// ✅ BETTER
auto* respData = static_cast<const SceNpMatching2GetRoomDataExternalListResponseA*>(data);
```

**In short: Avoid using auto. When you must use it, be explicit!**

## Initialization

### Constructor Initialization

**Always initialize members in constructor:**

**Single member:**
```cpp
Foo() : m_Var(nullptr)
{
}
```

**Struct initialization:**
```cpp
Foo() : m_Var{0, 0, 0}
{
}
```

**Default constructor OK:**
```cpp
class Foo
{
private:
    std::string m_Name;  // Default constructor is fine
    
public:
    Foo() {}  // No need to initialize m_Name
};
```

### Inline Initialization

**Alternative: Inline declarations (don't mix with constructor initialization):**
```cpp
struct Foo
{
    void* m_Var = nullptr;
    int m_Count = 0;
    // Do NOT also initialize these in constructor!
};
```

## Virtual, Final, and Override

### Class Finality

**Mark classes `final` if not intended for inheritance:**
```cpp
// ✅ GOOD - Clear intent
class MyClass final
{
    // Cannot be inherited
};
```

**Benefits:**
- Future developer knows class needs review before inheriting
- Compiler can devirtualize calls (optimization)
- Example: If inherited, need to make destructor virtual

### Function Overriding

**Always add `override` or `final` to overriding functions:**
```cpp
class Base
{
public:
    virtual void Update();
    virtual void Render() = 0;
};

class Derived final : public Base
{
public:
    void Update() override;  // ✅ Clear this overrides
    void Render() final;     // ✅ Cannot be overridden further
};
```

**Only add `virtual` to functions that will be overridden:**
```cpp
// Prevents unnecessary vtables and virtual dispatch overhead
class MyClass
{
public:
    // ❌ If never overridden, don't use virtual
    void UtilityFunction();
    
    // ✅ Only if designed for override
    virtual void ExtensionPoint();
};
```

## C++ vs C Style

### Prefer C++ Style

1. **Use `nullptr` over `NULL`**:
   ```cpp
   // ✅ GOOD
   void* ptr = nullptr;
   
   // ❌ BAD
   void* ptr = NULL;
   ```

2. **Use `constexpr` functions over macros**:
   ```cpp
   // ✅ PREFER
   constexpr int Square(int x) { return x * x; }
   
   // ❌ AVOID
   #define SQUARE(x) ((x) * (x))
   ```

3. **Use range-based for loops**:
   ```cpp
   // ✅ GOOD
   for (const auto& item : container)
   {
       Process(item);
   }
   
   // ❌ AVOID (unless performance-critical)
   for (size_t i = 0; i < container.size(); ++i)
   {
       Process(container[i]);
   }
   ```

4. **Use custom C++-style casts over C-style casts**:
   ```cpp
   // Project-specific checked casts
   auto* derived = checked_cast<Derived*>(base);
   auto value = checked_numcast<int>(largeValue);
   
   // Standard C++ casts
   auto* derived = static_cast<Derived*>(base);
   auto* shared = dynamic_cast<Shared*>(base);
   auto value = reinterpret_cast<intptr_t>(ptr);
   ```

**Benefits:**
- Compiler can check for errors
- Better optimization opportunities
- Clear intent

## Move Semantics

### When to Implement Move Operations

**For complex classes (containing allocations, locks, etc.):**
- Write move constructor
- Write move assignment operator
- Consider deleting copy operations if appropriate

```cpp
class Resource
{
private:
    void* m_Data;
    size_t m_Size;
    
public:
    // Move constructor
    Resource(Resource&& other) noexcept
        : m_Data(other.m_Data)
        , m_Size(other.m_Size)
    {
        other.m_Data = nullptr;
        other.m_Size = 0;
    }
    
    // Move assignment
    Resource& operator=(Resource&& other) noexcept
    {
        if (this != &other)
        {
            delete[] m_Data;
            m_Data = other.m_Data;
            m_Size = other.m_Size;
            other.m_Data = nullptr;
            other.m_Size = 0;
        }
        return *this;
    }
    
    // Delete copy operations (if move-only)
    Resource(const Resource&) = delete;
    Resource& operator=(const Resource&) = delete;
};
```

**Critical for container storage:**
- Prevents hundreds of copies during vector growth
- Dramatically improves performance

### Pass by Value + Move

**Prefer pass-by-value + move for moveable types:**
```cpp
// ✅ GOOD - Works with both copy and move
class MyClass
{
private:
    std::vector<int> m_Values;
    
public:
    MyClass(std::vector<int> values)  // Pass by value
        : m_Values(std::move(values))  // Move into member
    {
    }
};

// Usage
std::vector<int> data = {1, 2, 3};
MyClass obj1(data);              // Copy when lvalue
MyClass obj2(std::move(data));   // Move when rvalue

// ❌ LESS FLEXIBLE - Always copies
MyClass(const std::vector<int>& values)
    : m_Values(values)
{
}
```

**Why this is better:**
- Caller can choose to copy or move
- Single implementation
- Optimal performance in both cases

## Common Mistakes

### Const References vs Const Values

```cpp
// ❌ BAD - Unnecessary copy
void Process(const std::string value);

// ✅ GOOD - Reference avoids copy
void Process(const std::string& value);
```

### Use Dedicated Functions

```cpp
// ❌ BAD - strlen is cheaper than creating std::string
std::string temp(charPtr);
size_t len = temp.length();

// ✅ GOOD
size_t len = strlen(charPtr);

// ❌ BAD - Empty() is cheaper than Length()
if (str.Length() != 0)

// ✅ GOOD
if (!str.Empty())
```

### Choose the Right Constructor

```cpp
// ❌ BAD - Unnecessary temporary
std::string test = std::string("test");

// ✅ GOOD - Direct construction
std::string test("test");

// ✅ GOOD - Default construction
std::string test{};

// ❌ BAD
std::string test("");
```

### Check Expensive Conditions Last

```cpp
// ❌ BAD - Expensive function called even when flags is false
if (ReallyExpensiveFunction() || flags)
{
    // ...
}

// ✅ GOOD - Short-circuit evaluation
if (flags || ReallyExpensiveFunction())
{
    // ...
}
```

### Container Contains vs Find

```cpp
// ❌ BAD - Two lookups
if (container.Contains(key))
{
    auto value = container.Find(key);
    // Use value
}

// ✅ GOOD - Single lookup
if (auto it = container.Find(key); it != container.end())
{
    // Use it
}
```

### Inline Keyword

The `inline` keyword is usually ignored by compilers.

**If you really need to force inline:**
- Use `forceinline` macro (project-specific)
- Profile to prove it's faster
- Function must be fully declared in .h/.inl files, NOT .cpp

### Always Use Braces with Macros

```cpp
// ❌ DANGEROUS
if (error)
    SOME_MACRO;
    CleanUp();  // Might not execute as expected!

// ✅ SAFE
if (error)
{
    SOME_MACRO;
}
CleanUp();
```

## Operator Overloading

### Operator Overloading Rules

1. **Operators should do what users expect**: No magic side effects
2. **Keep operators symmetric**: Important for paired operators
3. **Enforce symmetry through negation**:

```cpp
// ✅ GOOD - Symmetric operators
bool operator==(const Obj& v1, const Obj& v2)
{
    return v1.a == v2.a && v1.b == v2.b;
}

bool operator!=(const Obj& v1, const Obj& v2)
{
    return !(v1 == v2);  // Defined in terms of ==
}
```

**Benefits:**
- Consistency guaranteed
- Single source of truth
- Less error-prone

## Function Parameters

### Long Parameter Lists

**When parameters exceed ~200 characters, use one of these approaches:**

**Approach 1: Wrap each parameter:**
```cpp
void MyFunction(
    bool somethingA,
    float somethingB,
    const glm::vec3& somethingC,
    const glm::vec3& pos,
    Character* src,
    Character* targetCharacter,
    const glm::vec3& target,
    Item* item)
{
    // ...
}
```

**Approach 2: Use parameter struct (preferred for many parameters):**
```cpp
struct MyFunctionParams
{
    bool somethingA;
    float somethingB;
    glm::vec3 somethingC;
    glm::vec3 pos;
    Character* src;
    Character* target;
    glm::vec3 targetPos;
    Item* item;
};

void MyFunction(const MyFunctionParams& params)
{
    // ...
}
```

### Use Appropriate Parameter Types

**Make parameters match actual needs:**
```cpp
// ❌ BAD - Converting between incompatible types
void GetLength(const char* str);

// ✅ GOOD - Use string_view for flexibility
void GetLength(std::string_view str);

// Can now accept: string, char*, string_view, etc.
```

## Code Review

### Code Review Requirements

1. **All code must be reviewed** by at least one other programmer
2. **Assign reviews** to someone with relevant expertise
3. **Check thoroughly**: Even small reviews can contain mistakes
4. **No stupid questions**: If unclear, the code needs improvement or comments
5. **Learning opportunity**: Share knowledge and best practices
6. **Post publicly**: Use task groups or review channels, not DMs
7. **Keep it simple**: Split large changes into multiple reviews

### Review Mindset

**For reviewers:**
- Does the code do what it claims?
- Is it clear and maintainable?
- Are there simpler approaches?
- Can you understand it now? (Will the author in 6 months?)
- No stupid questions - ask for clarification

**For authors:**
- Reviews are learning opportunities
- Simpler code is better
- Multiple small reviews > one large review

## Unit Tests

### Unit Testing Requirements

1. **All code should have corresponding unit tests**
2. **Test file naming**: Use `Test.cpp` postfix (e.g., `StringUtilsTest.cpp`)
3. **Add tests for bugs**: When fixing bugs, add tests to prevent regression
4. **Legacy code**: Add tests when touching untested legacy code

### Test Quality

**Tests must genuinely reflect reality:**
- Tests must fail for actual defects
- Missing dependencies must cause test failures
- Don't write tests that always pass

## Undefined Behavior

### Avoiding Undefined Behavior

**Read about undefined behavior**: https://blog.llvm.org/2011/05/what-every-c-programmer-should-know.html

**Common sources:**
- Signed integer overflow
- Dereferencing null pointers
- Accessing out-of-bounds array elements
- Using uninitialized variables
- Data races
- Violating object lifetime rules

**Prevention:**
- Enable sanitizers (ASan, UBSan, TSan)
- Use static analysis tools
- Follow coding standards strictly
- Test thoroughly

## Violations

### Handling Guideline Violations

**If you encounter code violating these guidelines:**
- Update the code to comply
- These guidelines evolved over time
- Improve code when you touch it
- Don't propagate bad patterns

**Boy Scout Rule:** Leave code better than you found it.

## Summary

### Core Principles

1. **Fail early**: Compile-time > Runtime, Type system > Asserts
2. **Be explicit**: Clear code > Clever code
3. **Const correctness**: Default to const, carefully add mutability
4. **Move semantics**: Implement for complex types
5. **Modern C++**: Prefer C++ features over C-style
6. **Test everything**: Unit tests are mandatory
7. **Review carefully**: Code review catches bugs
8. **Simplify**: Remove complexity, improve clarity

### Quick Reference Checklist

**Before committing:**
- [ ] Code passes all warnings (MSVC, Clang, GCC)
- [ ] Static analysis clean (PVS, CPPCheck)
- [ ] Unit tests written and passing
- [ ] Code reviewed by peer
- [ ] Follows naming conventions
- [ ] No unnecessary includes
- [ ] Proper const correctness
- [ ] Move semantics for complex types
- [ ] Documentation added for public APIs
- [ ] No undefined behavior

### Key Takeaways

- **Type safety first**: Use the type system to enforce constraints
- **Const everywhere**: Default to const, relax only when needed
- **No mutable**: Except mutexes and debug code
- **Careful with auto**: Always explicit with const/&/*
- **Move, don't copy**: Implement move semantics for complex types
- **Modern C++**: Prefer C++ features over C-style code
- **Test thoroughly**: Unit tests are not optional
- **Review seriously**: Code review is crucial for quality
