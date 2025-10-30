---
name: bug-hunter-tester-SKILL
description: Comprehensive testing and bug detection for C++/C# code. Use after implementation or refactoring to validate functionality, find bugs through systematic testing, verify P/Invoke marshaling, validate architecture adherence, and ensure quality standards.
---

# Bug Hunter and Tester Skill

## When to Use This Skill

Use this skill whenever you need comprehensive bug detection, testing, and quality assurance. This includes:

- After implementing new features or functionality
- After refactoring or modifying existing code
- When investigating intermittent or hard-to-reproduce bugs
- Before deploying changes to production
- When experiencing unexplained failures or crashes
- After making changes to C++/C# interop layer
- When performance issues or memory leaks are suspected
- After dependency updates or toolchain changes

**Key Indicators**: User mentions "test this", "find bugs", "something's wrong", "intermittent failure", "crashes sometimes", "memory leak", "not working as expected", or you've just completed implementing or modifying code that needs validation.

## Core Bug Hunting Principles

### 1. Systematic Analysis Approach

Follow this methodology for every code review:

```
1. UNDERSTAND
   ├─ Read the code's purpose and intended behavior
   ├─ Identify critical paths and high-risk areas
   ├─ Review existing tests and documentation
   └─ Note project-specific patterns and standards

2. STATIC ANALYSIS
   ├─ Read code line by line for logic errors
   ├─ Check for common anti-patterns
   ├─ Verify memory management (C++)
   ├─ Check resource disposal (C#)
   └─ Look for security vulnerabilities

3. DYNAMIC TESTING
   ├─ Run existing test suites
   ├─ Test with valid inputs
   ├─ Test with invalid inputs
   ├─ Test boundary conditions
   ├─ Test concurrent scenarios
   └─ Profile for performance issues

4. ANALYZE & REPORT
   ├─ Document all findings
   ├─ Classify by severity
   ├─ Provide root cause analysis
   └─ Recommend solutions
```

### 2. Bug Severity Classification

**Critical**: 
- Security vulnerabilities (buffer overflows, injection attacks)
- Data loss or corruption
- System crashes or deadlocks
- Complete feature failure
- Memory corruption in C++/C# interop

**High**: 
- Major functionality broken
- Significant performance degradation (>50% slower)
- Memory leaks (especially in long-running processes)
- Race conditions causing data inconsistency
- Incorrect P/Invoke marshaling causing corruption

**Medium**: 
- Feature partially broken
- Minor memory leaks
- Poor error messages
- Suboptimal performance
- Missing null checks

**Low**: 
- Cosmetic issues
- Minor inefficiencies
- Code quality concerns
- Missing documentation

## C++ Specific Bug Patterns

### Memory Management Issues

**Common Patterns to Check**:

```cpp
// ❌ Memory Leak - forgot to delete
void ProcessData() {
    char* buffer = new char[1024];
    // ... processing
    // Missing: delete[] buffer;
}

// ✅ RAII with smart pointers
void ProcessData() {
    auto buffer = std::make_unique<char[]>(1024);
    // ... processing
    // Automatically cleaned up
}

// ❌ Double Delete
MyClass* obj = new MyClass();
delete obj;
// ... later
delete obj;  // BUG: Double delete

// ❌ Use After Free
char* data = new char[100];
delete[] data;
strcpy(data, "test");  // BUG: Use after free

// ❌ Dangling Pointer
char* GetTemporaryString() {
    char buffer[100];
    return buffer;  // BUG: Returns pointer to stack memory
}

// ❌ Array Bounds Violation
int arr[10];
arr[10] = 5;  // BUG: Out of bounds access

// ❌ Null Pointer Dereference
MyClass* obj = nullptr;
obj->DoSomething();  // BUG: Dereferencing null
```

**Testing Strategy**:
- Run with AddressSanitizer (ASan) to detect memory errors
- Use Valgrind to find leaks and invalid memory access
- Test with MemorySanitizer (MSan) for uninitialized reads
- Enable debug allocators to catch heap corruption

### Undefined Behavior

```cpp
// ❌ Integer Overflow
int32_t a = INT32_MAX;
int32_t b = a + 1;  // BUG: Signed overflow is UB

// ❌ Uninitialized Variable
int value;
if (value > 0) { }  // BUG: Reading uninitialized variable

// ❌ Data Race
std::vector<int> shared_data;
// Thread 1
shared_data.push_back(1);
// Thread 2
shared_data.push_back(2);  // BUG: Data race without synchronization

// ❌ Iterator Invalidation
std::vector<int> vec = {1, 2, 3};
for (auto it = vec.begin(); it != vec.end(); ++it) {
    vec.push_back(*it);  // BUG: Invalidates iterator
}
```

**Testing Strategy**:
- Compile with `-fsanitize=undefined` to catch UB
- Use ThreadSanitizer (TSan) for race condition detection
- Initialize all variables to catch uninitialized reads
- Test with different optimization levels

### Resource Management

```cpp
// ❌ File Handle Leak
void ProcessFile(const char* path) {
    FILE* f = fopen(path, "r");
    if (!f) return;
    // ... processing
    // Missing: fclose(f);
}

// ✅ RAII Resource Management
void ProcessFile(const char* path) {
    std::ifstream file(path);
    if (!file.is_open()) return;
    // ... processing
    // Automatically closed by destructor
}

// ❌ Lock Not Released
void ThreadUnsafeOperation() {
    mutex.lock();
    if (error_condition) {
        return;  // BUG: Lock not released
    }
    // ... work
    mutex.unlock();
}

// ✅ RAII Lock Guard
void ThreadSafeOperation() {
    std::lock_guard<std::mutex> lock(mutex);
    if (error_condition) {
        return;  // Lock automatically released
    }
    // ... work
}
```

## C# Specific Bug Patterns

### Null Reference Issues

**Common Patterns to Check**:

```csharp
// ❌ Null Reference Exception
public string GetUserName(User user) {
    return user.Name.ToUpper();  // BUG: user or Name could be null
}

// ✅ Null Check
public string GetUserName(User? user) {
    return user?.Name?.ToUpper() ?? "Unknown";
}

// ❌ Unhandled Null in LINQ
var result = users.Select(u => u.Name.Length).ToList();  
// BUG: Throws if any Name is null

// ✅ Filter Nulls First
var result = users
    .Where(u => u.Name != null)
    .Select(u => u.Name!.Length)
    .ToList();
```

### Resource Disposal Issues

```csharp
// ❌ Missing Disposal
public void ProcessFile(string path) {
    var stream = File.OpenRead(path);
    // ... processing
    // Missing: stream.Dispose();
}

// ✅ Using Statement
public void ProcessFile(string path) {
    using var stream = File.OpenRead(path);
    // ... processing
    // Automatically disposed
}

// ❌ Disposing Too Early
public Stream GetDataStream() {
    using var stream = new MemoryStream();
    // ... fill stream
    return stream;  // BUG: Already disposed!
}

// ❌ Missing IDisposable Implementation
public class DatabaseConnection {
    private IntPtr nativeHandle;
    
    // BUG: Should implement IDisposable
    // Native resource will leak
}
```

### Async/Await Pitfalls

```csharp
// ❌ Async Void (except event handlers)
public async void ProcessData() {  // BUG: Can't be awaited, swallows exceptions
    await Task.Delay(100);
}

// ✅ Return Task
public async Task ProcessDataAsync() {
    await Task.Delay(100);
}

// ❌ Blocking on Async
public void DoWork() {
    var result = GetDataAsync().Result;  // BUG: Can cause deadlock
}

// ✅ Async All The Way
public async Task DoWorkAsync() {
    var result = await GetDataAsync();
}

// ❌ Fire and Forget
public void StartBackgroundWork() {
    ProcessDataAsync();  // BUG: Exceptions will be lost
}

// ✅ Await or Handle
public async Task StartBackgroundWorkAsync() {
    await ProcessDataAsync();
}

// ❌ ConfigureAwait Misuse in Library Code
public async Task<string> GetDataAsync() {
    await Task.Delay(100);  // BUG: May cause deadlock in UI apps
    return "data";
}

// ✅ ConfigureAwait(false) in Library Code
public async Task<string> GetDataAsync() {
    await Task.Delay(100).ConfigureAwait(false);
    return "data";
}
```

### Collection Modification Issues

```csharp
// ❌ Collection Modified During Enumeration
foreach (var item in list) {
    if (item.ShouldRemove) {
        list.Remove(item);  // BUG: InvalidOperationException
    }
}

// ✅ Remove After Enumeration
var toRemove = list.Where(item => item.ShouldRemove).ToList();
foreach (var item in toRemove) {
    list.Remove(item);
}
```

## C++/C# Interop Bug Patterns

### Memory Management Across Boundaries

**Critical Issues to Check**:

```cpp
// C++ Side - ❌ Wrong Memory Management
extern "C" __declspec(dllexport) 
char* GetString() {
    std::string str = "Hello";
    return str.c_str();  // BUG: Dangling pointer!
}

// ✅ Proper Memory Management
extern "C" __declspec(dllexport) 
char* GetString() {
    const char* str = "Hello";
    size_t len = strlen(str) + 1;
    char* result = new char[len];
    strcpy_s(result, len, str);
    return result;  // Caller must free
}

extern "C" __declspec(dllexport)
void FreeString(char* str) {
    delete[] str;
}
```

```csharp
// C# Side - ❌ Memory Leak
[DllImport("MyLib.dll")]
private static extern IntPtr GetString();

public string GetManagedString() {
    IntPtr ptr = GetString();
    return Marshal.PtrToStringAnsi(ptr);  // BUG: Native memory leaked!
}

// ✅ Proper Cleanup
[DllImport("MyLib.dll")]
private static extern IntPtr GetString();

[DllImport("MyLib.dll")]
private static extern void FreeString(IntPtr str);

public string GetManagedString() {
    IntPtr ptr = GetString();
    try {
        return Marshal.PtrToStringAnsi(ptr) ?? string.Empty;
    }
    finally {
        FreeString(ptr);
    }
}
```

### Marshaling Issues

```csharp
// ❌ Incorrect Structure Layout
[StructLayout(LayoutKind.Sequential)]
public struct MyStruct {
    public int Value1;
    public bool Flag;      // BUG: Size may differ from C++
    public int Value2;     // BUG: Padding may be different
}

// ✅ Explicit Layout and Marshaling
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct MyStruct {
    public int Value1;
    [MarshalAs(UnmanagedType.I1)]
    public bool Flag;      // Explicitly 1 byte
    public int Value2;
}

// ❌ String Marshaling Without Specification
[DllImport("MyLib.dll")]
private static extern void ProcessString(string str);  // BUG: Encoding ambiguous

// ✅ Explicit String Marshaling
[DllImport("MyLib.dll", CharSet = CharSet.Ansi)]
private static extern void ProcessString(
    [MarshalAs(UnmanagedType.LPStr)] string str);
```

### Callback Issues

```cpp
// C++ - Callback typedef
typedef void (*Callback)(const char* message);

extern "C" __declspec(dllexport)
void RegisterCallback(Callback cb) {
    g_callback = cb;
}

extern "C" __declspec(dllexport)
void TriggerCallback() {
    if (g_callback) {
        g_callback("Hello from C++");
    }
}
```

```csharp
// ❌ Callback GC Issue
public class CallbackManager {
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void NativeCallback(IntPtr message);
    
    public void Setup() {
        // BUG: Delegate may be garbage collected!
        RegisterCallback(OnCallback);
    }
    
    private void OnCallback(IntPtr message) {
        // ...
    }
}

// ✅ Keep Callback Alive
public class CallbackManager : IDisposable {
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void NativeCallback(IntPtr message);
    
    private NativeCallback? _callback;  // Keep reference
    
    public void Setup() {
        _callback = OnCallback;
        RegisterCallback(_callback);
    }
    
    private void OnCallback(IntPtr message) {
        // ...
    }
    
    public void Dispose() {
        _callback = null;
    }
}
```

## Testing Tools and Frameworks

### C++ Testing Tools

**Unit Testing Frameworks**:
- **Google Test (gtest)**: Industry standard
- **Catch2**: Header-only, BDD-style
- **doctest**: Fastest compile times
- **Boost.Test**: Part of Boost ecosystem

**Memory and Safety Tools**:
- **AddressSanitizer (ASan)**: Memory errors
- **MemorySanitizer (MSan)**: Uninitialized memory
- **ThreadSanitizer (TSan)**: Data races
- **UndefinedBehaviorSanitizer (UBSan)**: Undefined behavior
- **Valgrind**: Memory leaks, profiling
- **Dr. Memory**: Windows memory debugger

**Example Test Setup**:

```cpp
#include <gtest/gtest.h>

// Test fixture for common setup
class DataProcessorTest : public ::testing::Test {
protected:
    void SetUp() override {
        processor = std::make_unique<DataProcessor>();
    }
    
    void TearDown() override {
        processor.reset();
    }
    
    std::unique_ptr<DataProcessor> processor;
};

// Test cases
TEST_F(DataProcessorTest, ProcessValidData) {
    std::vector<uint8_t> input = {1, 2, 3};
    auto result = processor->Process(input);
    
    ASSERT_FALSE(result.empty());
    EXPECT_EQ(result.size(), 3);
}

TEST_F(DataProcessorTest, ProcessEmptyData) {
    std::vector<uint8_t> input;
    auto result = processor->Process(input);
    
    EXPECT_TRUE(result.empty());
}

TEST_F(DataProcessorTest, ProcessNullPointer) {
    EXPECT_THROW(processor->Process(nullptr, 0), std::invalid_argument);
}

// Death tests for crashes
TEST(DataProcessorDeathTest, CrashOnInvalidInput) {
    ASSERT_DEATH({
        DataProcessor processor;
        processor.ProcessUnsafe(nullptr, 100);
    }, ".*");
}

// Parameterized tests
class DataProcessorParamTest : public ::testing::TestWithParam<size_t> {};

TEST_P(DataProcessorParamTest, ProcessVariousSizes) {
    size_t size = GetParam();
    std::vector<uint8_t> input(size, 0xFF);
    
    DataProcessor processor;
    auto result = processor.Process(input);
    
    EXPECT_EQ(result.size(), size);
}

INSTANTIATE_TEST_SUITE_P(
    SizeVariations,
    DataProcessorParamTest,
    ::testing::Values(0, 1, 10, 100, 1000, 10000)
);
```

### C# Testing Tools

**Unit Testing Frameworks**:
- **xUnit**: Modern, extensible
- **NUnit**: Feature-rich, mature
- **MSTest**: Microsoft official

**Code Analysis Tools**:
- **Roslyn Analyzers**: Static analysis
- **FxCop / Code Analysis**: Built-in analysis
- **SonarQube**: Comprehensive quality tool
- **ReSharper**: IDE-integrated analysis

**Example Test Setup**:

```csharp
using Xunit;
using FluentAssertions;
using Moq;

public class DataProcessorTests {
    private readonly DataProcessor _processor;
    private readonly Mock<ILogger> _mockLogger;
    
    public DataProcessorTests() {
        _mockLogger = new Mock<ILogger>();
        _processor = new DataProcessor(_mockLogger.Object);
    }
    
    [Fact]
    public void ProcessValidData_ReturnsExpectedResult() {
        // Arrange
        var input = new byte[] { 1, 2, 3 };
        
        // Act
        var result = _processor.Process(input);
        
        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }
    
    [Fact]
    public void ProcessNullData_ThrowsArgumentNullException() {
        // Arrange
        byte[]? input = null;
        
        // Act & Assert
        Action act = () => _processor.Process(input!);
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*input*");
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void ProcessVariousSizes_HandlesCorrectly(int size) {
        // Arrange
        var input = new byte[size];
        
        // Act
        var result = _processor.Process(input);
        
        // Assert
        result.Should().HaveCount(size);
    }
    
    [Fact]
    public async Task ProcessAsync_WithCancellation_ThrowsOperationCanceledException() {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        
        // Act
        Func<Task> act = async () => await _processor.ProcessAsync(
            new byte[100], 
            cts.Token);
        
        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
    
    [Fact]
    public void Process_LogsCorrectly() {
        // Arrange
        var input = new byte[] { 1, 2, 3 };
        
        // Act
        _processor.Process(input);
        
        // Assert
        _mockLogger.Verify(
            x => x.Log(LogLevel.Information, It.IsAny<string>()), 
            Times.Once);
    }
}
```

### Interop Testing

```csharp
public class InteropTests {
    [Fact]
    public void NativeFunction_ReturnsExpectedValue() {
        // Test basic P/Invoke call
        int result = NativeMethods.Add(2, 3);
        result.Should().Be(5);
    }
    
    [Fact]
    public void NativeFunction_HandlesErrorCorrectly() {
        // Test error handling
        int errorCode = NativeMethods.ProcessData(null, 0);
        errorCode.Should().Be(-1);  // Error code
    }
    
    [Fact]
    public void StructMarshaling_MaintainsDataIntegrity() {
        // Test structure marshaling
        var config = new NativeConfig {
            Value1 = 42,
            Value2 = 3.14,
            Flag = true
        };
        
        int result = NativeMethods.ProcessConfig(ref config);
        result.Should().Be(0);  // Success
        
        // Verify native code modified the struct correctly
        config.Value1.Should().BeGreaterThan(42);
    }
    
    [Fact]
    public void MemoryManagement_NoLeaks() {
        // Test that memory is properly freed
        for (int i = 0; i < 1000; i++) {
            IntPtr ptr = NativeMethods.AllocateBuffer(1024);
            ptr.Should().NotBe(IntPtr.Zero);
            NativeMethods.FreeBuffer(ptr);
        }
        // Monitor memory usage to ensure no leaks
    }
    
    [Fact]
    public void StringMarshaling_PreservesUnicodeCharacters() {
        // Test string encoding
        string input = "Hello 世界 🌍";
        IntPtr ptr = NativeMethods.ConvertString(input);
        
        try {
            string output = Marshal.PtrToStringUni(ptr)!;
            output.Should().Be(input);
        }
        finally {
            NativeMethods.FreeString(ptr);
        }
    }
}
```

## Bug Report Format

For every bug found, use this structured format:

```markdown
### Bug #[N]: [Descriptive Title]

**Severity**: [Critical / High / Medium / Low]

**Location**: 
- File: `path/to/file.cpp` or `path/to/file.cs`
- Function: `FunctionName()`
- Lines: 123-145

**Category**: [Memory Leak / Null Reference / Race Condition / etc.]

**Description**:
[Clear, concise explanation of what's wrong]

**Steps to Reproduce**:
1. [Exact steps needed to trigger the bug]
2. [Include specific input values]
3. [Expected environment or conditions]

**Expected Behavior**:
[What should happen]

**Actual Behavior**:
[What actually happens, including error messages]

**Root Cause**:
[Technical explanation of why this occurs, trace through the code execution]

**Proof of Concept** (if applicable):
```[cpp/csharp]
// Minimal code demonstrating the issue
```

**Recommended Solution**:
```[cpp/csharp]
// Before (buggy code)
[original code]

// After (fixed code)
[corrected code]
```

**Additional Testing Needed**:
- [ ] Unit test for this scenario
- [ ] Integration test
- [ ] Performance test
- [ ] Memory leak test

**Impact if Not Fixed**:
[Consequences: data loss, crashes, security breach, etc.]

**Related Issues**:
- Similar pattern in `other_file.cpp:456`
- May affect `RelatedClass` as well

**References**:
- [Link to documentation]
- [OWASP guideline]
- [Stack Overflow discussion]
```

## Comprehensive Bug Hunting Workflow

### Phase 1: Preparation

```bash
# Set up testing environment
# C++
cmake -DCMAKE_BUILD_TYPE=Debug \
      -DENABLE_ASAN=ON \
      -DENABLE_UBSAN=ON \
      -DENABLE_TESTS=ON \
      ..

# Build with sanitizers
make -j$(nproc)

# Run tests with memory checking
valgrind --leak-check=full --show-leak-kinds=all ./tests

# C#
dotnet build --configuration Debug
dotnet test --logger "console;verbosity=detailed"
```

### Phase 2: Static Analysis

**C++ Checklist**:
- [ ] All pointers checked for null before dereferencing
- [ ] All allocated memory is freed (check with smart pointers)
- [ ] No use-after-free scenarios
- [ ] No buffer overflows (bounds checking)
- [ ] All resources released (files, sockets, mutexes)
- [ ] Thread synchronization correct (no data races)
- [ ] Exception safety (no leaks on exception paths)
- [ ] Integer overflow checks for arithmetic
- [ ] Proper const correctness
- [ ] Virtual destructors in base classes

**C# Checklist**:
- [ ] All nullable types handled correctly
- [ ] All IDisposable objects disposed (use using statements)
- [ ] Async methods return Task, not void (except event handlers)
- [ ] No blocking on async code (.Result, .Wait())
- [ ] Collections not modified during enumeration
- [ ] Proper exception handling (catch specific exceptions)
- [ ] String comparisons use proper culture (StringComparison)
- [ ] Resources properly disposed in exception paths
- [ ] No implicit synchronization context capture (ConfigureAwait)
- [ ] Async streams properly disposed (await using)

**Interop Checklist**:
- [ ] Calling conventions match (Cdecl, Stdcall)
- [ ] Structure layouts identical (size, padding, alignment)
- [ ] Strings marshaled with correct encoding
- [ ] Memory ownership clearly defined
- [ ] Native memory properly freed from C#
- [ ] Callbacks kept alive (not garbage collected)
- [ ] Thread safety across boundaries
- [ ] Error codes properly propagated
- [ ] Platform-specific code handled (x86/x64)

### Phase 3: Dynamic Testing

**Test Categories**:

1. **Happy Path Testing**:
   - Valid inputs with expected usage patterns
   - Verify core functionality works

2. **Boundary Testing**:
   - Empty inputs (null, empty strings, zero-length arrays)
   - Maximum values (INT_MAX, max string length)
   - Minimum values (INT_MIN, negative numbers)
   - Off-by-one cases (array[size-1], array[size])

3. **Error Path Testing**:
   - Invalid inputs (null pointers, invalid enum values)
   - Resource exhaustion (out of memory, disk full)
   - Network failures (timeouts, connection refused)
   - Verify exceptions thrown correctly

4. **Concurrency Testing**:
   - Multiple threads accessing shared data
   - Race conditions and deadlocks
   - Thread pool exhaustion
   - Async/await edge cases

5. **Performance Testing**:
   - Large input sizes
   - Repeated operations (memory leaks)
   - CPU and memory profiling
   - Scalability under load

### Phase 4: Exploratory Testing

Think creatively about edge cases:

- What if the user does X twice in a row?
- What if the system is under heavy load?
- What if the input is in an unexpected format?
- What if external dependencies fail?
- What if the object is used after disposal?
- What if the callback is called on a different thread?

## Testing Anti-Patterns to Avoid

❌ **Don't**:
- Only test happy paths
- Write tests that depend on execution order
- Use hard-coded timestamps or file paths
- Ignore flaky tests
- Test implementation details instead of behavior
- Write tests without assertions
- Catch exceptions without asserting on them
- Mock everything (test real code when possible)

✅ **Do**:
- Test both success and failure cases
- Make tests independent and isolated
- Use test data generators for varied inputs
- Investigate and fix flaky tests immediately
- Test public contracts and observable behavior
- Assert expected outcomes explicitly
- Let exceptions propagate in tests (unless testing exception handling)
- Mock only external dependencies

## Executive Summary Format

After completing analysis, provide:

```markdown
## Bug Hunting Report: [Component Name]

### Executive Summary

**Analysis Date**: [Date]
**Code Analyzed**: [Files/Modules]
**Lines of Code**: ~[Count]
**Test Coverage**: [Percentage if available]

**Findings**:
- 🔴 Critical Issues: [Count]
- 🟠 High Priority: [Count]
- 🟡 Medium Priority: [Count]
- 🟢 Low Priority: [Count]

**Overall Assessment**: [Healthy / Needs Attention / Critical Issues Found]

**Top Concerns**:
1. [Most critical issue summary]
2. [Second most critical]
3. [Third most critical]

**Immediate Actions Required**:
- [ ] Fix critical security vulnerability in [location]
- [ ] Address memory leak in [location]
- [ ] Add error handling for [scenario]

### Testing Summary

**Tests Run**: [Passed/Total]
**New Tests Written**: [Count]
**Test Failures**: [Count]

**Coverage Gaps**:
- [Area lacking tests]
- [Another untested area]

### Detailed Findings

[Detailed bug reports follow...]

### Recommendations

**Short Term** (Fix immediately):
- [Critical fixes]

**Medium Term** (Fix this sprint):
- [High priority improvements]

**Long Term** (Technical debt):
- [Refactoring suggestions]
- [Architecture improvements]

**Prevention Strategies**:
- [Code review checklist items]
- [Additional automated checks]
- [Documentation needs]
```

## Common Testing Scenarios

### Scenario 1: Testing Memory Management in C++

```cpp
// Test that objects are properly cleaned up
TEST(ResourceManagementTest, NoMemoryLeaks) {
    // Run under Valgrind or ASan
    for (int i = 0; i < 10000; i++) {
        auto processor = std::make_unique<DataProcessor>();
        processor->Process(testData);
        // Verify no leaks when processor destroyed
    }
}

// Test RAII behavior
TEST(ResourceManagementTest, ExceptionSafety) {
    EXPECT_THROW({
        FileProcessor processor("test.dat");
        // Force exception during processing
        processor.ProcessWithError();
    }, std::runtime_error);
    
    // File should be closed despite exception
    EXPECT_FALSE(IsFileOpen("test.dat"));
}
```

### Scenario 2: Testing Async Behavior in C#

```csharp
[Fact]
public async Task ProcessAsync_CancelsGracefully() {
    var cts = new CancellationTokenSource();
    var processor = new DataProcessor();
    
    var task = processor.ProcessLongRunningAsync(cts.Token);
    
    // Cancel after brief delay
    await Task.Delay(100);
    cts.Cancel();
    
    // Should throw OperationCanceledException
    await Assert.ThrowsAsync<OperationCanceledException>(
        () => task);
}

[Fact]
public async Task ProcessAsync_HandlesTimeouts() {
    var processor = new DataProcessor();
    
    var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
    
    await Assert.ThrowsAsync<OperationCanceledException>(
        () => processor.ProcessSlowAsync(cts.Token));
}
```

### Scenario 3: Testing Interop Boundary

```csharp
[Fact]
public void InteropCall_PreservesMemory() {
    // Test that data survives the interop boundary
    var input = new byte[1024];
    new Random().NextBytes(input);
    
    var result = NativeMethods.ProcessBuffer(input, input.Length);
    
    result.Should().NotBeNull();
    result.Should().HaveCount(1024);
    
    // Verify first and last bytes to ensure no corruption
    result[0].Should().NotBe(0);
    result[1023].Should().NotBe(0);
}

[Fact]
public void InteropStructure_MarshalCorrectly() {
    var config = new NativeConfig {
        IntValue = 42,
        DoubleValue = 3.14159,
        BoolValue = true
    };
    
    // Verify size matches native expectations
    int size = Marshal.SizeOf(typeof(NativeConfig));
    size.Should().Be(16);  // Expected native size
    
    // Test round-trip marshaling
    IntPtr ptr = Marshal.AllocHGlobal(size);
    try {
        Marshal.StructureToPtr(config, ptr, false);
        var result = Marshal.PtrToStructure<NativeConfig>(ptr);
        
        result.IntValue.Should().Be(42);
        result.DoubleValue.Should().BeApproximately(3.14159, 0.00001);
        result.BoolValue.Should().BeTrue();
    }
    finally {
        Marshal.FreeHGlobal(ptr);
    }
}
```

## Advanced Debugging Techniques

### Using Sanitizers (C++)

```bash
# Address Sanitizer - Memory errors
ASAN_OPTIONS=detect_leaks=1:symbolize=1 ./app

# Memory Sanitizer - Uninitialized reads
MSAN_OPTIONS=poison_in_dtor=1 ./app

# Thread Sanitizer - Race conditions
TSAN_OPTIONS=history_size=7:second_deadlock_stack=1 ./app

# Undefined Behavior Sanitizer
UBSAN_OPTIONS=print_stacktrace=1:halt_on_error=1 ./app
```

### Common Sanitizer Error Patterns

```
// AddressSanitizer Output
ERROR: AddressSanitizer: heap-use-after-free
→ Check for use after delete/free
→ Look for dangling pointers

ERROR: AddressSanitizer: heap-buffer-overflow
→ Check array bounds
→ Look for off-by-one errors

ERROR: AddressSanitizer: SEGV
→ Null pointer dereference
→ Invalid memory access

// ThreadSanitizer Output
WARNING: ThreadSanitizer: data race
→ Missing synchronization
→ Add mutex or atomic operations

WARNING: ThreadSanitizer: lock-order-inversion (potential deadlock)
→ Reorder lock acquisition
→ Use std::lock for multiple locks
```

## Quality Metrics to Report

After bug hunting, provide these metrics:

1. **Bug Density**: Bugs per 1000 lines of code
2. **Critical Bug Ratio**: Critical bugs / Total bugs
3. **Test Coverage**: Percentage of code covered by tests
4. **Code Complexity**: Cyclomatic complexity scores
5. **Memory Safety**: Clean ASan/Valgrind runs
6. **Thread Safety**: Clean TSan runs
7. **API Stability**: Breaking vs non-breaking changes

## Final Checklist

Before completing bug hunting:

**C++ Code**:
- [ ] Compiled with -Wall -Wextra -Werror
- [ ] All tests pass
- [ ] Clean run under AddressSanitizer
- [ ] Clean run under Valgrind
- [ ] No compiler warnings
- [ ] Static analysis clean (clang-tidy, cppcheck)

**C# Code**:
- [ ] Compiled with warnings as errors
- [ ] All tests pass
- [ ] No nullable warnings
- [ ] All IDisposable objects disposed
- [ ] Async methods properly used
- [ ] Code analysis clean

**Interop Code**:
- [ ] Structure layouts verified
- [ ] Memory ownership documented
- [ ] Callbacks tested
- [ ] Error handling validated
- [ ] Platform compatibility checked

Remember: The goal is to find bugs before they reach production. Be thorough, systematic, and constructive in your findings. Every bug caught during development is a potential production incident prevented.
