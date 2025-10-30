---
name: api-docs-writer-SKILL
description: Creates comprehensive API documentation including XML comments, markdown docs, usage examples, and interop guides. Use when documenting REST/GraphQL endpoints, C++/C# APIs, P/Invoke interfaces, user guides, or technical documentation.
---

# API Documentation Writer Skill

## When to Use This Skill

Use this skill whenever you need to create or update API documentation, user guides, or technical documentation for code. This includes:

- Documenting new REST/GraphQL endpoints, functions, classes, or modules
- Creating user guides for libraries or frameworks
- Updating documentation after code changes or refactoring
- Documenting breaking changes and migration paths
- Writing integration guides and getting-started tutorials
- Creating comprehensive API reference documentation

**Key Indicators**: User mentions "document this", "create docs for", "update the documentation", "write API docs", or you've just completed implementing/modifying code that affects the public interface.

## Core Documentation Principles

### 1. Documentation Structure

Every piece of documentation should follow this hierarchy:

```
# Component/API Name

## Overview
Brief description (2-3 sentences) of what it does and why it exists

## Installation/Prerequisites (if applicable)
Setup requirements, dependencies, environment configuration

## Quick Start
Minimal working example to get users started immediately

## API Reference
Detailed documentation of all public interfaces

## Usage Examples
Comprehensive examples covering common and edge cases

## Error Handling
Common errors, status codes, and resolution steps

## Advanced Topics (if applicable)
Complex patterns, performance tips, best practices

## Troubleshooting
FAQ and common issues

## Changelog (if updating)
Version history and migration notes
```

### 2. API Reference Standards

For every endpoint/function/method, document:

**Functions/Methods**:
```
### functionName(param1, param2, options)

**Description**: What it does in one clear sentence.

**Parameters**:
- `param1` (type, required/optional): Description, constraints, defaults
- `param2` (type, required/optional): Description, constraints, defaults
- `options` (object, optional): Configuration object
  - `option1` (type): Description
  - `option2` (type): Description

**Returns**: 
- Type: Return type description
- Example return value

**Throws/Errors**:
- `ErrorType`: When this occurs and how to handle it

**Example**:
[Complete, runnable code example]
```

**REST API Endpoints**:
```
### POST /api/resource

**Description**: What this endpoint does.

**Authentication**: Required/Not required, auth type

**Request Headers**:
- `Authorization`: Bearer token format
- `Content-Type`: application/json

**Request Body**:
{
  "field1": "string (required) - description",
  "field2": "number (optional) - description, default: 0",
  "nested": {
    "subfield": "boolean - description"
  }
}

**Response**:

Success (200):
{
  "id": "string - resource ID",
  "status": "string - status value",
  "created_at": "ISO 8601 timestamp"
}

Error (400):
{
  "error": "string - error code",
  "message": "string - human-readable message",
  "details": {}
}

**Status Codes**:
- 200: Success
- 400: Bad Request - invalid parameters
- 401: Unauthorized - missing or invalid token
- 404: Not Found - resource doesn't exist
- 429: Rate Limited - exceeded request quota
- 500: Internal Server Error

**Rate Limits**: X requests per minute/hour

**Example Request**:
[curl command or code example]

**Example Response**:
[Full JSON response]
```

### 3. Code Example Standards

Every code example must:

1. **Be Complete**: Include all necessary headers/using statements, initialization, and setup
2. **Be Runnable**: Should compile/run if copied directly (with placeholders for paths/keys)
3. **Handle Errors**: Show proper error handling patterns (exceptions, error codes, RAII)
4. **Use Realistic Data**: Meaningful variable names and realistic example values
5. **Include Comments**: Explain non-obvious logic inline
6. **Show Both Languages**: For interop scenarios, provide both C++ and C# examples

**Language-Specific Requirements**:

**C++ Examples**:
- Include necessary `#include` directives
- Show proper memory management (RAII, smart pointers, manual cleanup)
- Demonstrate exception handling with try-catch
- Use modern C++ features (C++11/14/17) unless targeting older standards
- Show proper resource cleanup (destructors, finally patterns)
- Include namespace declarations

**C# Examples**:
- Include necessary `using` statements
- Show proper disposal patterns (using statements, IDisposable)
- Demonstrate exception handling with try-catch-finally
- Use async/await patterns for asynchronous operations
- Show nullable reference type handling (C# 8.0+)
- Include namespace declarations

**Interop Examples**:
- Always show BOTH C++ and C# sides
- Demonstrate proper memory management across the boundary
- Show structure marshaling with correct layout attributes
- Include P/Invoke declarations with proper attributes
- Demonstrate string marshaling (UTF-8, UTF-16)
- Show how to handle callbacks from native to managed code

**Example Template for C++**:

```cpp
// Example: Creating a new user with error handling in C++

#include <string>
#include <stdexcept>
#include <memory>

namespace UserManagement {

/// @brief Result structure for user creation
struct UserResult {
    std::string userId;
    std::string email;
    int64_t createdAt;
};

/// @brief Creates a new user account
/// @param email User's email address (must be valid format)
/// @param name User's full name
/// @param role User role (default: "user", options: "user", "admin", "moderator")
/// @return UserResult containing user ID and creation timestamp
/// @throws std::invalid_argument If email format is invalid
/// @throws std::runtime_error If API request fails
UserResult CreateUser(
    const std::string& email,
    const std::string& name,
    const std::string& role = "user"
) {
    // Validate email format before making request
    if (email.find('@') == std::string::npos) {
        throw std::invalid_argument("Invalid email format: " + email);
    }
    
    // Validate role
    if (role != "user" && role != "admin" && role != "moderator") {
        throw std::invalid_argument("Invalid role: " + role);
    }
    
    try {
        // Call internal API implementation
        auto response = InternalApi::PostUser(email, name, role);
        
        UserResult result;
        result.userId = response.GetId();
        result.email = email;
        result.createdAt = response.GetTimestamp();
        
        return result;
    }
    catch (const ApiException& e) {
        if (e.GetStatusCode() == 400) {
            throw std::runtime_error("Invalid request: " + e.GetMessage());
        }
        else if (e.GetStatusCode() == 409) {
            throw std::runtime_error("User already exists: " + email);
        }
        throw;
    }
}

} // namespace UserManagement

// Usage example
int main() {
    try {
        auto user = UserManagement::CreateUser(
            "alice@example.com",
            "Alice Smith",
            "admin"
        );
        std::cout << "Created user with ID: " << user.userId << std::endl;
        return 0;
    }
    catch (const std::exception& e) {
        std::cerr << "Failed to create user: " << e.what() << std::endl;
        return 1;
    }
}
```

**Example Template for C#**:

```csharp
// Example: Creating a new user with error handling in C#

using System;
using System.Threading.Tasks;

namespace UserManagement
{
    /// <summary>
    /// Result of user creation operation
    /// </summary>
    public class UserResult
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public long CreatedAt { get; set; }
    }

    /// <summary>
    /// Service for managing user accounts
    /// </summary>
    public class UserService
    {
        /// <summary>
        /// Creates a new user account
        /// </summary>
        /// <param name="email">User's email address (must be valid format)</param>
        /// <param name="name">User's full name</param>
        /// <param name="role">User role (default: "user", options: "user", "admin", "moderator")</param>
        /// <returns>UserResult containing user ID and creation timestamp</returns>
        /// <exception cref="ArgumentException">Thrown when email format is invalid</exception>
        /// <exception cref="InvalidOperationException">Thrown when API request fails</exception>
        public async Task<UserResult> CreateUserAsync(
            string email,
            string name,
            string role = "user")
        {
            // Validate email format before making request
            if (!email.Contains("@"))
            {
                throw new ArgumentException($"Invalid email format: {email}", nameof(email));
            }

            // Validate role
            var validRoles = new[] { "user", "admin", "moderator" };
            if (Array.IndexOf(validRoles, role) == -1)
            {
                throw new ArgumentException($"Invalid role: {role}", nameof(role));
            }

            try
            {
                // Call internal API implementation
                var response = await InternalApi.PostUserAsync(email, name, role);

                return new UserResult
                {
                    UserId = response.Id,
                    Email = email,
                    CreatedAt = response.Timestamp
                };
            }
            catch (ApiException ex) when (ex.StatusCode == 400)
            {
                throw new InvalidOperationException($"Invalid request: {ex.Message}", ex);
            }
            catch (ApiException ex) when (ex.StatusCode == 409)
            {
                throw new InvalidOperationException($"User already exists: {email}", ex);
            }
        }
    }

    // Usage example
    class Program
    {
        static async Task Main(string[] args)
        {
            var service = new UserService();
            
            try
            {
                var user = await service.CreateUserAsync(
                    email: "alice@example.com",
                    name: "Alice Smith",
                    role: "admin"
                );
                Console.WriteLine($"Created user with ID: {user.UserId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create user: {ex.Message}");
            }
        }
    }
}
```

### 4. Writing Style Guidelines

**Voice and Tone**:
- Use active voice: "The function returns..." not "The value is returned..."
- Use present tense: "This method validates..." not "This method will validate..."
- Be direct and concise: "Required parameter" not "This parameter is required"
- Be specific: "Must be between 1-100" not "Should be a reasonable number"

**Terminology**:
- Define acronyms on first use: "JWT (JSON Web Token)"
- Use consistent terms throughout (don't alternate between "parameter" and "argument")
- Prefer standard terms: "authentication" over "auth" in formal documentation

**Clarity Checklist**:
- [ ] Can a developer unfamiliar with the code use it successfully?
- [ ] Are all parameters and return values explained?
- [ ] Are constraints and validation rules clear?
- [ ] Are error scenarios documented?
- [ ] Are examples complete and runnable?

### 5. C++/C# Interop Documentation

When documenting C++ code that will be consumed from C#, provide comprehensive interop guidance:

**C++ Native Interface Documentation**:

```cpp
// Native C++ interface that will be exported for C# consumption

/// @brief Configuration for the processing engine
/// @note This structure is marshaled to C# as a value type
struct ProcessConfig {
    int maxThreads;        // Maximum number of worker threads (1-32)
    bool enableLogging;    // Enable detailed logging
    double timeout;        // Operation timeout in seconds (0 = no timeout)
};

/// @brief Processes data using the native engine
/// @param data Pointer to input data buffer
/// @param dataSize Size of input data in bytes
/// @param config Configuration settings for processing
/// @param resultSize [out] Receives the size of result data
/// @return Pointer to result buffer, or nullptr on error. Caller must free using FreeResult()
/// @throws None - this is a C-style interface for interop
/// @note Thread-safe. Multiple threads can call this simultaneously.
extern "C" __declspec(dllexport) 
const char* ProcessData(
    const char* data,
    int dataSize,
    const ProcessConfig* config,
    int* resultSize
);

/// @brief Frees memory allocated by ProcessData
/// @param result Pointer returned from ProcessData
extern "C" __declspec(dllexport)
void FreeResult(const char* result);
```

**Corresponding C# Interop Documentation**:

```csharp
using System;
using System.Runtime.InteropServices;

namespace NativeInterop
{
    /// <summary>
    /// Configuration for the native processing engine
    /// </summary>
    /// <remarks>
    /// This structure matches the native ProcessConfig layout.
    /// All fields are blittable for efficient marshaling.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct ProcessConfig
    {
        /// <summary>
        /// Maximum number of worker threads (valid range: 1-32)
        /// </summary>
        public int MaxThreads;

        /// <summary>
        /// Enable detailed logging (true = enabled, false = disabled)
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool EnableLogging;

        /// <summary>
        /// Operation timeout in seconds (0 = no timeout)
        /// </summary>
        public double Timeout;
    }

    /// <summary>
    /// P/Invoke wrapper for native processing library
    /// </summary>
    public static class NativeProcessor
    {
        private const string DllName = "ProcessingEngine.dll";

        /// <summary>
        /// Processes data using the native C++ engine
        /// </summary>
        /// <param name="data">Input data to process</param>
        /// <param name="config">Configuration settings</param>
        /// <returns>Processed result data</returns>
        /// <exception cref="InvalidOperationException">Thrown when native processing fails</exception>
        /// <remarks>
        /// Thread-safe. The native library handles concurrent calls.
        /// Memory is automatically managed through SafeHandle.
        /// </remarks>
        public static byte[] ProcessData(byte[] data, ProcessConfig config)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            int resultSize;
            IntPtr resultPtr;

            // Pin the input data during the native call
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                resultPtr = ProcessDataNative(
                    dataHandle.AddrOfPinnedObject(),
                    data.Length,
                    ref config,
                    out resultSize
                );

                if (resultPtr == IntPtr.Zero)
                    throw new InvalidOperationException("Native processing failed");

                // Copy result to managed array
                byte[] result = new byte[resultSize];
                Marshal.Copy(resultPtr, result, 0, resultSize);

                return result;
            }
            finally
            {
                dataHandle.Free();
                if (resultPtr != IntPtr.Zero)
                    FreeResultNative(resultPtr);
            }
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "ProcessData")]
        private static extern IntPtr ProcessDataNative(
            IntPtr data,
            int dataSize,
            ref ProcessConfig config,
            out int resultSize
        );

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "FreeResult")]
        private static extern void FreeResultNative(IntPtr result);
    }

    // Usage example
    class Program
    {
        static void Main()
        {
            var config = new ProcessConfig
            {
                MaxThreads = 4,
                EnableLogging = true,
                Timeout = 30.0
            };

            byte[] inputData = System.Text.Encoding.UTF8.GetBytes("Sample data");
            
            try
            {
                byte[] result = NativeProcessor.ProcessData(inputData, config);
                Console.WriteLine($"Processed {result.Length} bytes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Processing failed: {ex.Message}");
            }
        }
    }
}
```

**Interop Documentation Checklist**:
- [ ] Native calling convention documented (Cdecl, Stdcall)
- [ ] Memory ownership and lifetime clearly specified
- [ ] Marshaling requirements for structures documented
- [ ] Thread safety characteristics stated
- [ ] Error handling approach explained (return codes, exceptions, etc.)
- [ ] Example showing proper memory management on both sides
- [ ] Performance considerations noted (pinning, marshaling overhead)
- [ ] Platform requirements specified (Windows, Linux, architecture)
- [ ] DLL/SO naming and location requirements
- [ ] Version compatibility information

## Special Scenarios

### Documenting Breaking Changes

When code has breaking changes:

1. **Mark Deprecated Features**:
```markdown
### oldMethod() [DEPRECATED]

⚠️ **Deprecated in v2.0**: Use `newMethod()` instead. This will be removed in v3.0.

**Migration Guide**:
[Show before/after code examples]
```

2. **Provide Migration Path**:
```markdown
## Migrating from v1.x to v2.0

### Breaking Changes

1. **Authentication Method Changed**
   - **Old**: API key in query parameter
   - **New**: Bearer token in Authorization header
   - **Action Required**: Update all API calls to use header authentication

**Before (v1.x)**:
[code example]

**After (v2.0)**:
[code example]
```

### Documenting Complex Workflows

For multi-step processes:

1. **Create a Flow Diagram** (in text or suggest diagram):
```
User Registration Flow:
1. POST /api/auth/register → Receive confirmation token
2. User clicks email link
3. POST /api/auth/verify → Account activated
4. POST /api/auth/login → Receive JWT
5. Use JWT for authenticated requests
```

2. **Provide End-to-End Example**:
```python
# Complete workflow example showing all steps
```

### Handling Missing Information

When you can't determine something from code:

```markdown
**Note**: [Parameter/behavior] is not explicitly defined in the code. 
Based on the implementation, this appears to [your inference], but please 
verify this assumption. Questions for clarification:
1. What should happen when [scenario]?
2. Are there any rate limits or quotas?
3. What are the valid values for [parameter]?
```

## Quality Checklist

Before finalizing documentation:

### Completeness
- [ ] All public methods/endpoints are documented
- [ ] All parameters have type and description
- [ ] Return values are fully specified
- [ ] Error cases are documented with status codes
- [ ] Authentication requirements are clear
- [ ] Rate limits and quotas are specified (if applicable)

### Accuracy
- [ ] Examples match actual code behavior
- [ ] Type signatures are correct
- [ ] Default values are accurate
- [ ] Error messages match what code produces

### Usability
- [ ] Quick start example works out of the box
- [ ] Common use cases are covered
- [ ] Error handling examples are included
- [ ] Code examples are complete and runnable
- [ ] Complex concepts have additional explanation

### Consistency
- [ ] Terminology is consistent throughout
- [ ] Formatting follows established patterns
- [ ] Code style matches project conventions
- [ ] Related docs use same structure

## Output Format Recommendations

### For Markdown Documentation:

```markdown
# API Name

> Brief tagline describing the API

## Table of Contents
[Auto-generated or manual TOC]

## Overview
[Content]

## [Sections following structure above]
```

### For OpenAPI/Swagger:

Generate or update OpenAPI 3.0 specification:

```yaml
openapi: 3.0.0
info:
  title: API Name
  version: 1.0.0
  description: |
    Detailed description
paths:
  /endpoint:
    post:
      summary: Brief description
      description: Detailed description
      parameters: []
      requestBody: {}
      responses: {}
```

### For C++ Doxygen Comments:

Ensure inline documentation follows Doxygen conventions:

```cpp
/// @brief Processes a batch of images with specified transformations
///
/// This function applies transformations to multiple images in parallel.
/// The input images are not modified; results are written to output directory.
///
/// @param[in] inputPaths Vector of paths to input image files
/// @param[in] transforms Transformations to apply (rotation, scaling, filters)
/// @param[in] outputDir Directory where processed images will be saved
/// @param[out] processedCount Number of successfully processed images
/// @return true if all images processed successfully, false if any failed
///
/// @throws std::invalid_argument if outputDir doesn't exist or isn't writable
/// @throws std::runtime_error if image decoding fails
///
/// @note This function is thread-safe and uses all available CPU cores
/// @warning Large images may consume significant memory
///
/// @code
/// std::vector<std::string> inputs = {"img1.jpg", "img2.jpg"};
/// TransformParams transforms;
/// transforms.rotation = 90;
/// transforms.scale = 0.5;
/// 
/// size_t count = 0;
/// if (ProcessImageBatch(inputs, transforms, "./output", &count)) {
///     std::cout << "Processed " << count << " images\n";
/// }
/// @endcode
///
/// @see TransformParams, Image::Load(), Image::Save()
/// @since Version 2.0
bool ProcessImageBatch(
    const std::vector<std::string>& inputPaths,
    const TransformParams& transforms,
    const std::string& outputDir,
    size_t* processedCount
);
```

### For C# XML Documentation Comments:

Ensure inline documentation follows C# conventions:

```csharp
/// <summary>
/// Processes a batch of images with specified transformations
/// </summary>
/// <remarks>
/// <para>
/// This method applies transformations to multiple images asynchronously.
/// The input images are not modified; results are written to the output directory.
/// </para>
/// <para>
/// This method is thread-safe and automatically manages parallelism based on
/// available CPU cores. Large images may consume significant memory.
/// </para>
/// </remarks>
/// <param name="inputPaths">Collection of paths to input image files</param>
/// <param name="transforms">Transformations to apply (rotation, scaling, filters)</param>
/// <param name="outputDir">Directory where processed images will be saved</param>
/// <param name="cancellationToken">Cancellation token to abort processing</param>
/// <returns>
/// A task that represents the asynchronous operation. 
/// The task result contains the number of successfully processed images.
/// </returns>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="inputPaths"/> or <paramref name="outputDir"/> is null
/// </exception>
/// <exception cref="DirectoryNotFoundException">
/// Thrown when <paramref name="outputDir"/> doesn't exist
/// </exception>
/// <exception cref="UnauthorizedAccessException">
/// Thrown when the output directory isn't writable
/// </exception>
/// <exception cref="ImageProcessingException">
/// Thrown when image decoding or processing fails
/// </exception>
/// <example>
/// <code>
/// var processor = new ImageProcessor();
/// var inputs = new[] { "img1.jpg", "img2.jpg" };
/// var transforms = new TransformParams 
/// { 
///     Rotation = 90, 
///     Scale = 0.5 
/// };
/// 
/// int count = await processor.ProcessImageBatchAsync(
///     inputs, 
///     transforms, 
///     "./output",
///     CancellationToken.None
/// );
/// 
/// Console.WriteLine($"Processed {count} images");
/// </code>
/// </example>
/// <seealso cref="TransformParams"/>
/// <seealso cref="ProcessSingleImageAsync"/>
public async Task<int> ProcessImageBatchAsync(
    IEnumerable<string> inputPaths,
    TransformParams transforms,
    string outputDir,
    CancellationToken cancellationToken = default
)
{
    // Implementation
}
```

## Best Practices

1. **Start with Why**: Begin documentation by explaining the purpose and value
2. **Progressive Disclosure**: Basic example first, then advanced features
3. **Show, Don't Tell**: Prefer code examples over lengthy explanations
4. **Anticipate Questions**: Address common confusion points proactively
5. **Keep It Updated**: Documentation should be updated with every code change
6. **Link Generously**: Cross-reference related functionality
7. **Version Everything**: Tag documentation with version numbers
8. **Test Examples**: Ensure all code examples actually work

## Common Pitfalls to Avoid

❌ **Don't**:
- Write incomplete examples that won't run
- Use vague descriptions like "various options available"
- Forget to document error cases
- Assume knowledge your audience doesn't have
- Let documentation drift from actual code behavior
- Document internal implementation details users shouldn't rely on

✅ **Do**:
- Provide complete, working examples
- Be specific about types, constraints, and defaults
- Document every error code and its meaning
- Define terms and explain concepts clearly
- Keep docs in sync with code
- Focus on the public API contract

## Tools and Automation

When creating documentation files for C++ and C#:

1. **Choose the Right Format**:
   - README.md for project overview and quick start
   - Separate docs for C++ API reference and C# interop guide
   - Doxygen-compatible comments in C++ headers
   - XML documentation comments in C# code
   - Markdown files in docs/ directory for detailed guides

2. **C++ Documentation Tools**:
   - **Doxygen**: Generate HTML/PDF from specially formatted comments
   - Use `/// @brief`, `/// @param`, `/// @return` style comments
   - Configure Doxygen to extract all public APIs
   - Include call graphs and class diagrams when helpful

3. **C# Documentation Tools**:
   - **XML Documentation Comments**: Use `///` style comments
   - **DocFX**: Generate static documentation websites
   - **Sandcastle**: Create MSDN-style documentation
   - Use `<summary>`, `<param>`, `<returns>`, `<exception>` tags

4. **Interop Documentation**:
   - Maintain parallel documentation showing both sides
   - Use consistent terminology between C++ and C# docs
   - Cross-reference between native and managed documentation
   - Document marshaling behavior explicitly

5. **Use Templates**: 

**C++ Header Template**:
```cpp
/// @file MyClass.h
/// @brief Brief description of what this header provides
/// @author Your Name
/// @version 1.0
/// @date 2025-01-15

#pragma once

namespace MyNamespace {

/// @brief Brief class description
/// @details Detailed description with usage notes, thread safety, etc.
class MyClass {
public:
    /// @brief Brief method description
    /// @param paramName Description of parameter
    /// @return Description of return value
    /// @throws std::exception Description of when thrown
    /// @note Additional important information
    int MyMethod(const std::string& paramName);
};

} // namespace MyNamespace
```

**C# Documentation Template**:
```csharp
/// <summary>
/// Brief class description
/// </summary>
/// <remarks>
/// Detailed description with usage notes, performance characteristics, etc.
/// </remarks>
public class MyClass
{
    /// <summary>
    /// Brief method description
    /// </summary>
    /// <param name="paramName">Description of parameter</param>
    /// <returns>Description of return value</returns>
    /// <exception cref="ArgumentException">Thrown when parameter is invalid</exception>
    /// <example>
    /// <code>
    /// var obj = new MyClass();
    /// int result = obj.MyMethod("test");
    /// </code>
    /// </example>
    public int MyMethod(string paramName)
    {
        // Implementation
    }
}
```

6. **Generate When Possible**: 
   - Doxygen XML → Documentation website
   - C# XML comments → IntelliSense and generated docs
   - Keep inline docs and external docs in sync

7. **Validate**:
   - Check markdown rendering in docs
   - Build and test all code examples
   - Verify Doxygen/DocFX builds without warnings
   - Test P/Invoke declarations actually work
   - Ensure structure layouts match between C++ and C#

## Example Documentation Scenarios

### Scenario 1: C++ Native Library Function

User implements:
```cpp
namespace DataProcessing {
    std::vector<uint8_t> CompressData(
        const uint8_t* data, 
        size_t length, 
        CompressionLevel level = CompressionLevel::Medium
    );
}
```

You should document:
- Full function signature with parameter types and defaults
- Memory ownership (who allocates, who frees)
- Valid ranges for size_t length parameter
- CompressionLevel enum values and their effects
- Return value characteristics (empty vector on error vs exception)
- Thread safety guarantees
- Performance characteristics for different compression levels
- Complete usage example showing memory management
- C# interop example if this will be called from C#

### Scenario 2: C# Wrapper for Native Library

User implements:
```csharp
public class ImageProcessor : IDisposable
{
    [DllImport("ImageLib.dll")]
    private static extern IntPtr CreateProcessor(int width, int height);
    
    [DllImport("ImageLib.dll")]
    private static extern bool ProcessImage(IntPtr processor, byte[] data);
    
    [DllImport("ImageLib.dll")]
    private static extern void DestroyProcessor(IntPtr processor);
}
```

You should document:
- Class purpose and relationship to native library
- Constructor parameters and their mapping to native calls
- IDisposable pattern and why it's critical (unmanaged resources)
- Thread safety considerations (native library may not be thread-safe)
- Error handling approach (return codes vs exceptions)
- Memory management best practices
- Complete usage example with using statement
- Native DLL deployment requirements
- Platform-specific considerations (x86 vs x64)

### Scenario 3: C++ Class with C# Interop

User implements:
```cpp
class DatabaseConnection {
public:
    static DatabaseConnection* Create(const char* connectionString);
    bool ExecuteQuery(const char* sql, QueryResult** result);
    void Destroy();
private:
    DatabaseConnection();
};

extern "C" {
    __declspec(dllexport) DatabaseConnection* DB_Create(const char* connStr);
    __declspec(dllexport) bool DB_ExecuteQuery(DatabaseConnection* conn, 
                                                 const char* sql, 
                                                 QueryResult** result);
    __declspec(dllexport) void DB_Destroy(DatabaseConnection* conn);
}
```

You should document:

**For C++ Developers**:
- Class design pattern (factory, opaque pointer)
- Why C-style exports are needed for interop
- Memory management requirements
- Thread safety model
- Exception handling within the library

**For C# Developers**:
- Complete P/Invoke declarations with proper marshaling
- SafeHandle implementation for automatic cleanup
- Managed wrapper class that hides interop complexity
- RAII-style usage pattern in C# (using statements)
- Connection string format and validation
- Error handling approach
- Complete end-to-end example

Example dual documentation:

```markdown
## DatabaseConnection Class (C++)

### Native Interface

The `DatabaseConnection` class provides thread-safe database connectivity.

#### Creation

```cpp
static DatabaseConnection* Create(const char* connectionString);
```

Creates a new database connection. Returns nullptr on failure.

**Parameters**:
- `connectionString`: Connection string in format "host=X;port=Y;db=Z"

**Returns**: Pointer to DatabaseConnection, or nullptr on error

**Memory**: Caller must call `Destroy()` when done

#### Usage (C++)

```cpp
auto* conn = DatabaseConnection::Create("host=localhost;port=5432;db=mydb");
if (conn) {
    QueryResult* result = nullptr;
    if (conn->ExecuteQuery("SELECT * FROM users", &result)) {
        // Process result
        result->Free();
    }
    conn->Destroy();
}
```

### C# Interop

Safe managed wrapper for the native library:

```csharp
public class DatabaseConnection : SafeHandle
{
    [DllImport("DatabaseLib.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr DB_Create(
        [MarshalAs(UnmanagedType.LPStr)] string connectionString);

    [DllImport("DatabaseLib.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool DB_ExecuteQuery(
        IntPtr connection,
        [MarshalAs(UnmanagedType.LPStr)] string sql,
        out IntPtr result);

    [DllImport("DatabaseLib.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void DB_Destroy(IntPtr connection);

    public DatabaseConnection(string connectionString) 
        : base(IntPtr.Zero, true)
    {
        SetHandle(DB_Create(connectionString));
        if (IsInvalid)
            throw new InvalidOperationException("Failed to create database connection");
    }

    public QueryResult ExecuteQuery(string sql)
    {
        IntPtr resultPtr;
        if (!DB_ExecuteQuery(handle, sql, out resultPtr))
            throw new InvalidOperationException("Query execution failed");
        
        return new QueryResult(resultPtr);
    }

    protected override bool ReleaseHandle()
    {
        DB_Destroy(handle);
        return true;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;
}

// Usage (C#)
using (var conn = new DatabaseConnection("host=localhost;port=5432;db=mydb"))
{
    var result = conn.ExecuteQuery("SELECT * FROM users");
    // Process result - automatic cleanup on dispose
}
```
```

## Final Reminders

Your documentation is the bridge between code and users. Every sentence should help someone successfully use the API. When in doubt:

1. **Be more specific** rather than more general
2. **Include more examples** rather than fewer
3. **Over-explain edge cases** rather than under-explain
4. **Ask clarifying questions** rather than make assumptions
5. **Update related docs** rather than just the immediate request

The goal is documentation so good that users rarely need to read the source code or ask for help.
