---
name: dev-plan-validator-SKILL
description: Validates development plans, technical specifications, and implementation proposals before work begins. Use to verify completeness, detect conflicts, analyze data requirements, map dependencies, and assess risks in planning documents.
---

# Development Plan Validator Skill

## When to Use This Skill

Use this skill when a development plan, technical specification, or implementation proposal needs validation before work begins. This includes:

- After drafting a development plan for a new feature
- Before starting implementation on a technical design document
- When reviewing sprint planning documents
- After completing the planning phase of a project
- Before committing to a major refactoring or architectural change
- When evaluating proposals from team members
- Before presenting plans to stakeholders or management

**Key Indicators**: User mentions "review my plan", "does this look complete", "check my design doc", "planning is done", "ready to start coding", or shares a technical specification document.

## Core Validation Responsibilities

### 1. Completeness Verification

Ensure the plan contains all necessary information for successful implementation.

**Technical Specifications Checklist**:

```
□ Architecture & Design
  ├─ System architecture diagram or description
  ├─ Component interactions and boundaries
  ├─ Data flow through the system
  ├─ Design patterns employed
  └─ Technology stack with versions

□ Data Models
  ├─ Database schemas (tables, columns, types)
  ├─ Entity relationships (one-to-many, many-to-many)
  ├─ Indexes and constraints
  ├─ Data validation rules
  └─ Migration strategy (if modifying existing data)

□ API Specifications
  ├─ Endpoint definitions (routes, HTTP methods)
  ├─ Request/response formats
  ├─ Authentication/authorization requirements
  ├─ Rate limiting and throttling
  └─ Error response formats

□ Component Interfaces (C++/C#)
  ├─ Public API surface (classes, methods, functions)
  ├─ Interface contracts and abstractions
  ├─ P/Invoke signatures for interop
  ├─ Structure layouts for marshaling
  └─ Callback definitions

□ ECS-Specific (if applicable)
  ├─ Component data structures defined
  ├─ System responsibilities outlined
  ├─ Query patterns specified
  ├─ Entity lifecycle management
  └─ System execution order documented
```

**Requirements & Acceptance Criteria**:

```
□ Functional Requirements
  ├─ User stories or use cases
  ├─ Input/output specifications
  ├─ Business logic rules
  └─ User interaction flows

□ Non-Functional Requirements
  ├─ Performance targets (response time, throughput)
  ├─ Scalability requirements (concurrent users, data volume)
  ├─ Reliability targets (uptime, error rates)
  ├─ Security requirements (authentication, encryption)
  └─ Compliance needs (GDPR, HIPAA, etc.)

□ Acceptance Criteria
  ├─ Measurable success criteria
  ├─ Definition of "done"
  ├─ Quality gates
  └─ Sign-off requirements
```

**Error Handling & Edge Cases**:

```
□ Error Scenarios
  ├─ Expected error conditions
  ├─ Error handling strategies
  ├─ User-facing error messages
  ├─ Error logging and monitoring
  └─ Recovery procedures

□ Edge Cases
  ├─ Boundary conditions (empty, null, max values)
  ├─ Concurrent access scenarios
  ├─ Network failure handling
  ├─ Resource exhaustion (memory, disk, connections)
  └─ Invalid input handling
```

**Testing & Quality Assurance**:

```
□ Testing Strategy
  ├─ Unit test approach
  ├─ Integration test scenarios
  ├─ Performance test plan
  ├─ Security testing approach
  └─ User acceptance testing criteria

□ Quality Metrics
  ├─ Code coverage targets
  ├─ Performance benchmarks
  ├─ Reliability metrics
  └─ Security scan requirements
```

**Deployment & Operations**:

```
□ Deployment Plan
  ├─ Deployment steps and sequence
  ├─ Environment configuration
  ├─ Database migration scripts
  ├─ Feature flags or rollout strategy
  └─ Rollback procedures

□ Monitoring & Observability
  ├─ Metrics to track
  ├─ Logging requirements
  ├─ Alerting rules
  ├─ Dashboards needed
  └─ Health check endpoints
```

**Dependencies & Prerequisites**:

```
□ External Dependencies
  ├─ Third-party libraries (with versions)
  ├─ External APIs or services
  ├─ Infrastructure requirements
  ├─ Development tools needed
  └─ License considerations

□ Internal Dependencies
  ├─ Prerequisite tasks or features
  ├─ Required team members or skills
  ├─ Shared resources or services
  └─ Documentation dependencies
```

### 2. Conflict Detection

Identify contradictions, inconsistencies, and conflicts that would cause implementation problems.

**Conflict Categories**:

**Requirement Conflicts**:
```
❌ Example Conflict:
Section A: "User authentication tokens expire after 24 hours"
Section B: "Sessions remain active indefinitely for convenience"
→ Contradiction: Token expiration vs. indefinite sessions

✅ Resolution Needed:
Clarify: Should tokens refresh automatically, or should sessions expire?
Recommend: Token refreshing mechanism OR session timeout policy
```

**Naming Conflicts**:
```
❌ Example Conflict:
Component Design: "UserComponent contains userId field"
Database Schema: "User table has user_id column"
C# Code: "public int UserId" (PascalCase)
C++ Code: "int user_id" (snake_case)
→ Inconsistent naming conventions across layers

✅ Resolution Needed:
Establish naming convention mapping:
- C#: PascalCase (UserId)
- C++: snake_case (user_id)
- Database: snake_case (user_id)
- Document conversion rules for interop
```

**Technology Conflicts**:
```
❌ Example Conflict:
Plan specifies: "Use .NET 6 for compatibility"
Dependency requires: ".NET 8 for latest features"
Existing codebase: ".NET 7 currently deployed"
→ Version incompatibility

✅ Resolution Needed:
Decision required:
- Upgrade to .NET 8 (assess migration cost)
- Stay on .NET 7 (find alternative to dependency)
- Use .NET 6 (verify all dependencies support it)
```

**Architectural Conflicts**:
```
❌ Example Conflict:
ECS Principle: "Components contain only data, no methods"
Plan includes: "PlayerComponent.TakeDamage() method"
→ Violates architectural pattern

✅ Resolution Needed:
Refactor design:
- Move TakeDamage logic to HealthSystem
- Keep PlayerComponent as pure data structure
```

**Dependency Conflicts**:
```
❌ Example Conflict:
Task A: "Implement user authentication" (depends on database schema)
Task B: "Create database schema" (scheduled after Task A)
→ Circular or reverse dependency

✅ Resolution Needed:
Reorder tasks:
1. Task B: Create database schema first
2. Task A: Implement authentication using schema
```

**Resource Conflicts**:
```
❌ Example Conflict:
System 1: "Acquires EntityManager lock, then ResourcePool lock"
System 2: "Acquires ResourcePool lock, then EntityManager lock"
→ Potential deadlock

✅ Resolution Needed:
Establish lock ordering:
- Always acquire EntityManager before ResourcePool
- Document lock hierarchy
- Add lock order validation in debug builds
```

**Performance Conflicts**:
```
❌ Example Conflict:
Requirement: "Support 10,000 concurrent users"
Design: "Synchronous blocking I/O for all requests"
→ Design cannot meet performance requirement

✅ Resolution Needed:
Architectural change:
- Use async I/O operations
- Implement connection pooling
- Add load balancing
- Benchmark to validate approach
```

### 3. Data Requirement Analysis

Ensure all data needs are identified and properly specified.

**Data Source Validation**:

```
For each data requirement, verify:

□ Source Identification
  ├─ Where does the data come from? (database, API, file, user input)
  ├─ Is the source reliable and available?
  ├─ What's the data update frequency?
  └─ Who owns/maintains the data source?

□ Access Patterns
  ├─ Read vs write frequency
  ├─ Query patterns and filters
  ├─ Pagination requirements
  ├─ Caching strategy
  └─ Concurrent access handling

□ Data Volume
  ├─ Current data size
  ├─ Expected growth rate
  ├─ Storage requirements
  └─ Archival strategy
```

**Schema Specification**:

```cpp
// C++ Example - Validate schema completeness

❌ Incomplete Schema:
struct UserData {
    char* name;      // MISSING: Length? Null-terminated? Encoding?
    int age;         // MISSING: Valid range? Optional?
    // MISSING: User ID, email, timestamps?
};

✅ Complete Schema:
struct UserData {
    uint64_t user_id;                    // Unique identifier
    char name[256];                      // UTF-8, null-terminated, max 255 chars
    uint8_t age;                         // 0-255, required
    char email[320];                     // RFC 5321 max length
    int64_t created_at;                  // Unix timestamp (seconds since epoch)
    int64_t updated_at;                  // Unix timestamp
    bool is_active;                      // Account status
};
```

```csharp
// C# Example - Validate schema completeness

❌ Incomplete Schema:
public class User {
    public string Name { get; set; }     // MISSING: Max length? Required?
    public int Age { get; set; }         // MISSING: Validation? Range?
    // MISSING: ID, email, timestamps, constraints
}

✅ Complete Schema:
public class User {
    [Required]
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Range(0, 150)]
    public int Age { get; set; }
    
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
}
```

**Data Validation Rules**:

```
□ Input Validation
  ├─ Required vs optional fields
  ├─ Data type validation
  ├─ Format validation (email, phone, date)
  ├─ Range validation (min/max values)
  ├─ Length validation (string, array)
  └─ Pattern validation (regex)

□ Business Rules
  ├─ Cross-field validation
  ├─ Referential integrity
  ├─ State transition rules
  └─ Authorization rules

□ Sanitization
  ├─ SQL injection prevention
  ├─ XSS prevention
  ├─ Path traversal prevention
  └─ Command injection prevention
```

**Data Migration**:

```
If modifying existing data:

□ Migration Strategy
  ├─ Backwards compatibility approach
  ├─ Data transformation logic
  ├─ Rollback procedure
  ├─ Migration validation
  └─ Zero-downtime migration plan

□ Migration Risks
  ├─ Data loss possibilities
  ├─ Performance impact during migration
  ├─ Inconsistent state handling
  └─ Migration failure recovery
```

**Privacy & Compliance**:

```
□ Data Privacy
  ├─ PII (Personally Identifiable Information) identified
  ├─ Encryption at rest requirements
  ├─ Encryption in transit requirements
  ├─ Data retention policies
  └─ Right to deletion (GDPR, CCPA)

□ Compliance Requirements
  ├─ GDPR compliance (EU users)
  ├─ CCPA compliance (California users)
  ├─ HIPAA compliance (health data)
  ├─ PCI-DSS compliance (payment data)
  └─ Industry-specific regulations
```

### 4. Dependency Mapping

Create a complete picture of all dependencies and their relationships.

**Dependency Categories**:

**External Libraries & Frameworks**:
```
For each external dependency:

Library: [Name]
├─ Version: [Specific version or range]
├─ Purpose: [Why it's needed]
├─ License: [MIT, Apache, GPL, etc.]
├─ Compatibility: [.NET version, C++ standard]
├─ Alternatives: [Other options considered]
├─ Risk Assessment: [Maintenance status, community size]
└─ Update Strategy: [How to keep it current]

Example - Complete Dependency:
Library: Newtonsoft.Json
├─ Version: 13.0.3
├─ Purpose: JSON serialization for API responses
├─ License: MIT (commercial-friendly)
├─ Compatibility: .NET 6+, .NET Standard 2.0+
├─ Alternatives: System.Text.Json (considered but needs custom converters)
├─ Risk: Widely used, actively maintained, stable
└─ Update: Review quarterly, test before upgrading

Example - Incomplete Dependency (Needs Detail):
❌ Library: "Some JSON library"
   Version: "Latest"
   → MISSING: Specific version, license, compatibility
```

**Native Library Dependencies (P/Invoke)**:
```
For each native library:

Native Library: [Name]
├─ Platform: [Windows x64, Linux ARM, etc.]
├─ Version: [Specific version]
├─ Calling Convention: [Cdecl, Stdcall]
├─ Runtime Dependencies: [MSVC Runtime, glibc, etc.]
├─ Distribution: [How to deploy - bundled, system-wide, etc.]
├─ Fallback Strategy: [What if library missing/load fails]
└─ Testing Strategy: [How to test interop]

Example:
Native Library: PhysicsEngine.dll
├─ Platform: Windows x64, Linux x64
├─ Version: 2.1.0 (exact match required)
├─ Calling Convention: Cdecl
├─ Runtime: MSVC 2019 Redistributable
├─ Distribution: Copy to output directory
├─ Fallback: Graceful degradation (disable physics features)
└─ Testing: Integration tests for all P/Invoke functions
```

**Service Dependencies**:
```
For each external service:

Service: [Name]
├─ Provider: [Company/team]
├─ Endpoint: [URL or connection string]
├─ Authentication: [API key, OAuth, cert]
├─ Rate Limits: [Requests per second/minute]
├─ Availability SLA: [Uptime guarantee]
├─ Fallback: [What if service unavailable]
├─ Cost: [Per request, monthly, etc.]
└─ Monitoring: [How to detect failures]

Example:
Service: Authentication API
├─ Provider: Auth0
├─ Endpoint: https://myapp.auth0.com
├─ Authentication: OAuth2 client credentials
├─ Rate Limits: 100 req/sec, 10,000/hour
├─ Availability: 99.9% SLA
├─ Fallback: Cached credentials (5 min), degraded mode
├─ Cost: $0.10 per 1000 requests
└─ Monitoring: Health check every 30 sec, alert on 3 failures
```

**Task Dependencies**:
```
Dependency Graph:

Task A: Create database schema
  └─ BLOCKS → Task B: Implement data access layer
              └─ BLOCKS → Task C: Build API endpoints
                          └─ BLOCKS → Task D: Create UI components

Task E: Set up CI/CD pipeline
  └─ PARALLEL WITH → Tasks B, C, D

Validation:
✅ No circular dependencies
✅ Critical path identified (A → B → C → D)
✅ Parallelizable work marked (E)
✅ All blocking relationships explicit
```

**Version Compatibility Matrix**:
```
Component         | Version | Requires           | Compatible With
------------------|---------|--------------------|-----------------
.NET Runtime      | 8.0     | -                  | C# 12
Entity Framework  | 8.0.x   | .NET 8.0+          | SQL Server 2019+
React             | 18.2    | Node.js 18+        | TypeScript 5.0+
Native Library    | 2.1.0   | MSVC 2019+ Runtime | x64 architecture

Validation:
✅ All version dependencies satisfied
✅ No conflicting requirements
❌ WARNING: React 18 end-of-life in 6 months, plan upgrade
```

### 5. Risk Assessment

Identify potential issues before they become problems.

**Risk Categories**:

**Technical Risks**:
```
Risk: Ambiguous or Underspecified Areas
├─ Impact: Implementation delays, rework
├─ Probability: [High/Medium/Low]
├─ Examples:
│   ├─ "Optimize performance" (What metric? To what target?)
│   ├─ "Handle errors gracefully" (Which errors? How?)
│   └─ "User-friendly interface" (What are the UX requirements?)
└─ Mitigation:
    └─ Request clarification on all ambiguous specifications

Risk: Technical Debt Introduction
├─ Impact: Slower future development, increased maintenance
├─ Probability: [High/Medium/Low]
├─ Examples:
│   ├─ Skipping test coverage
│   ├─ Hardcoding configuration values
│   ├─ "TODO: Optimize later" comments
│   └─ Incomplete error handling
└─ Mitigation:
    ├─ Flag all "quick and dirty" solutions
    ├─ Create technical debt tickets
    └─ Set a "Technical Debt Sprint" in roadmap
```

**Integration Risks**:
```
Risk: External API Breaking Changes
├─ Impact: Application breaks in production
├─ Probability: Medium (depends on API stability)
├─ Example: Third-party authentication service changes endpoints
└─ Mitigation:
    ├─ Version-pin API SDK
    ├─ Abstract API behind interface
    ├─ Implement circuit breaker pattern
    ├─ Monitor API deprecation notices
    └─ Have fallback authentication method

Risk: Data Migration Failure
├─ Impact: Data loss, extended downtime
├─ Probability: Medium-High (complex migrations)
├─ Example: Database schema change fails mid-migration
└─ Mitigation:
    ├─ Test migration on production-like dataset
    ├─ Implement incremental migration (not big bang)
    ├─ Create database backup before migration
    ├─ Have rollback script ready and tested
    └─ Monitor migration progress in real-time
```

**Performance Risks**:
```
Risk: Algorithm Doesn't Scale
├─ Impact: Slow performance under real-world load
├─ Probability: Medium
├─ Example: O(n²) algorithm works with 100 items, fails with 10,000
└─ Mitigation:
    ├─ Analyze algorithmic complexity
    ├─ Benchmark with realistic data volumes
    ├─ Identify and optimize hot paths
    └─ Consider algorithmic alternatives

Risk: Memory Leak in Long-Running Process
├─ Impact: Application crashes after hours/days
├─ Probability: Medium (especially with native interop)
├─ Example: Unfreed native memory allocated via P/Invoke
└─ Mitigation:
    ├─ Use memory profilers (valgrind, dotMemory)
    ├─ Implement proper disposal patterns
    ├─ Add memory leak detection tests
    ├─ Run 24-hour stress tests
    └─ Monitor production memory metrics
```

**Security Risks**:
```
Risk: Insufficient Input Validation
├─ Impact: SQL injection, XSS, code execution vulnerabilities
├─ Probability: High (if not explicitly addressed)
├─ Example: User input directly concatenated into SQL queries
└─ Mitigation:
    ├─ Use parameterized queries always
    ├─ Implement input sanitization
    ├─ Add security testing (OWASP Top 10)
    ├─ Code review focusing on input boundaries
    └─ Use static analysis security tools

Risk: Insecure Data Storage
├─ Impact: Data breach, compliance violations
├─ Probability: Medium
├─ Example: Passwords stored in plain text
└─ Mitigation:
    ├─ Hash passwords with bcrypt/Argon2
    ├─ Encrypt PII at rest
    ├─ Use TLS for data in transit
    ├─ Implement key rotation strategy
    └─ Conduct security audit
```

## Validation Methodology

**Step-by-Step Validation Process**:

```
STEP 1: INITIAL SCAN (5-10 minutes)
├─ Read entire plan start to finish
├─ Understand scope and context
├─ Note initial impressions
└─ Identify major sections present/absent

STEP 2: COMPLETENESS CHECK (15-20 minutes)
├─ Go through completeness checklist systematically
├─ Mark what's present ✓
├─ Mark what's missing ✗
├─ Note partial/unclear items ~
└─ Prioritize missing items by criticality

STEP 3: CONFLICT DETECTION (15-20 minutes)
├─ Cross-reference related sections
├─ Check naming consistency
├─ Verify technology compatibility
├─ Look for contradictory requirements
├─ Validate dependency ordering
└─ Check resource access patterns

STEP 4: DATA ANALYSIS (10-15 minutes)
├─ Review all data models
├─ Check schema completeness
├─ Verify validation rules
├─ Assess migration needs
└─ Evaluate privacy/compliance

STEP 5: DEPENDENCY MAPPING (10-15 minutes)
├─ List all external dependencies
├─ Check version specifications
├─ Verify compatibility
├─ Map task dependencies
└─ Identify critical path

STEP 6: RISK ASSESSMENT (10-15 minutes)
├─ Identify ambiguous areas
├─ Flag potential technical debt
├─ Assess integration risks
├─ Evaluate performance concerns
└─ Note security considerations

STEP 7: REPORT GENERATION (15-20 minutes)
├─ Organize findings by severity
├─ Write actionable feedback
├─ Provide specific recommendations
├─ Create summary checklist
└─ Highlight critical blockers
```

## Output Format

Structure your validation report as follows:

```markdown
# Development Plan Validation Report

**Plan**: [Name of plan/feature]
**Reviewed**: [Date]
**Reviewer**: Claude
**Validation Status**: [✅ Ready / ⚠️ Needs Revision / 🚨 Critical Issues]

---

## Executive Summary

[2-3 sentence overview of the plan's readiness]

**Key Findings**:
- [Most important finding #1]
- [Most important finding #2]
- [Most important finding #3]

**Recommendation**: [Proceed with implementation / Address issues before starting / Major revision needed]

---

## ✅ Strengths

What's well-defined in this plan:

- [Strength #1]: [Specific example]
- [Strength #2]: [Specific example]
- [Strength #3]: [Specific example]

---

## 🚨 Critical Issues (Must Fix Before Development)

### Issue #1: [Title]
**Category**: [Completeness / Conflict / Data / Dependency / Risk]
**Severity**: Critical

**Problem**:
[Detailed explanation of the issue]

**Impact**:
[What will happen if this isn't addressed]

**Recommendation**:
[Specific, actionable steps to fix]

**Example** (if applicable):
```[code or specification example]
```

---

### Issue #2: [Title]
[Same structure as above]

---

## ⚠️ Important Gaps (Should Address)

### Gap #1: [Title]
**Category**: [Completeness / Conflict / Data / Dependency / Risk]
**Severity**: High

**Problem**:
[What's missing or unclear]

**Impact**:
[Likely consequences during development]

**Recommendation**:
[What needs to be added or clarified]

---

## 💡 Recommendations (Consider Addressing)

### Recommendation #1: [Title]
**Category**: [Enhancement / Best Practice / Future-Proofing]
**Severity**: Medium-Low

**Suggestion**:
[What could be improved]

**Benefit**:
[Why this improvement would help]

**Optional**: [Note if this can be deferred]

---

## 📋 Validation Checklist

### Completeness
- [x] Architecture defined
- [ ] Error handling specified
- [x] Data models documented
- [ ] Testing strategy outlined
- [~] Deployment plan included (partial)
- [x] Dependencies identified
- [ ] Security considerations addressed
- [ ] Performance requirements specified

### Conflicts
- [x] No requirement contradictions
- [ ] Naming consistency verified
- [x] Technology compatibility confirmed
- [ ] Dependency ordering validated
- [x] Resource conflicts checked

### Data Requirements
- [x] Data sources identified
- [ ] Schemas completely specified
- [ ] Validation rules defined
- [~] Migration strategy outlined (needs detail)
- [ ] Privacy/compliance addressed

### Dependencies
- [x] External libraries listed with versions
- [ ] Service dependencies documented
- [x] Task dependencies mapped
- [ ] Version compatibility verified

### Risk Assessment
- [x] Ambiguous areas identified
- [x] Technical debt risks flagged
- [ ] Integration risks assessed
- [ ] Performance risks evaluated
- [ ] Security risks noted

**Overall Readiness**: [X] / [Y] items complete ([Z]%)

---

## Detailed Analysis

[Optional: Provide deeper analysis of specific areas if needed]

### Architecture Review
[Comments on architectural decisions]

### Data Model Review
[Detailed feedback on schemas and data design]

### Dependency Analysis
[Detailed breakdown of dependency chain]

---

## Next Steps

### Immediate Actions Required
1. [Action #1] - Assignee: [Name], Due: [Date]
2. [Action #2] - Assignee: [Name], Due: [Date]
3. [Action #3] - Assignee: [Name], Due: [Date]

### Before Starting Implementation
- [ ] All critical issues resolved
- [ ] Important gaps addressed
- [ ] Plan re-validated (if major changes)
- [ ] Team reviewed and approved plan
- [ ] Stakeholders signed off

### Questions for Clarification
1. [Question #1]
2. [Question #2]
3. [Question #3]

---

## Validation Confidence

**Confidence Level**: [High / Medium / Low]
**Reason**: [Explanation of confidence level]

**Additional Review Needed**:
- [ ] Architecture review by [senior architect]
- [ ] Security review by [security team]
- [ ] Performance review by [performance engineer]
- [ ] None - plan is sufficiently detailed
```

## Common Validation Scenarios

### Scenario 1: Complete and Ready Plan

```markdown
# Validation Report: User Authentication System

**Validation Status**: ✅ Ready to Implement

## Executive Summary
The authentication system plan is comprehensive, well-structured, and ready for implementation. All critical components are specified with appropriate detail.

**Key Findings**:
- Architecture clearly defined with security best practices
- Data models complete with validation rules
- Testing strategy covers security scenarios
- Deployment includes rollback procedures

**Recommendation**: Proceed with implementation

## ✅ Strengths
- JWT token implementation follows RFC 7519 standard
- Password hashing uses Argon2id (current best practice)
- Rate limiting specified for login attempts
- Comprehensive error handling for all auth flows
- Clear P/Invoke interface for native crypto library

## 💡 Recommendations (Optional Improvements)
- Consider adding MFA support in roadmap
- Document session revocation strategy
- Add metrics for failed login attempts

**Overall Readiness**: 28/30 items complete (93%)
```

### Scenario 2: Plan with Critical Issues

```markdown
# Validation Report: Payment Processing Module

**Validation Status**: 🚨 Critical Issues - Do Not Start

## Executive Summary
The payment processing plan has several critical security and compliance issues that must be addressed before implementation can begin.

**Key Findings**:
- Missing PCI-DSS compliance requirements
- No encryption specified for payment data
- Incomplete error handling for transaction failures

**Recommendation**: Major revision needed

## 🚨 Critical Issues

### Issue #1: PCI-DSS Compliance Not Addressed
**Category**: Compliance / Security
**Severity**: Critical

**Problem**:
Plan does not mention PCI-DSS compliance requirements for storing and processing payment card data. This is a legal requirement for handling credit card information.

**Impact**:
- Regulatory violations and potential fines
- Inability to process payments legally
- Security vulnerabilities exposing customer data

**Recommendation**:
1. Determine PCI-DSS SAQ (Self-Assessment Questionnaire) level needed
2. Use payment processor API (Stripe, PayPal) instead of storing card data
3. If storing cards: Implement tokenization, encryption at rest, restricted access
4. Document compliance measures in plan
5. Schedule security audit before launch

### Issue #2: Payment Data Stored in Plain Text
**Category**: Security / Data
**Severity**: Critical

**Problem**:
Database schema shows `credit_card_number VARCHAR(16)` with no encryption mentioned.

**Impact**:
- Massive security vulnerability
- PCI-DSS violation
- Liability in case of data breach

**Recommendation**:
**DO NOT store credit card numbers**. Instead:
```csharp
public class PaymentMethod {
    public Guid Id { get; set; }
    public string PaymentProcessorToken { get; set; } // Stripe token, not actual card
    public string Last4Digits { get; set; }           // For display only
    public string CardBrand { get; set; }             // Visa, Mastercard, etc.
    public DateTime ExpiryMonth { get; set; }
    // NEVER store full card number or CVV
}
```

**Overall Readiness**: 15/30 items complete (50%) - Critical gaps
```

### Scenario 3: Plan Needing Clarification

```markdown
# Validation Report: Real-Time Notification System

**Validation Status**: ⚠️ Needs Clarification

## Executive Summary
The notification system plan has a solid foundation but contains several ambiguous specifications that need clarification before implementation.

**Key Findings**:
- Core architecture is sound (WebSocket-based)
- Technology choices are appropriate
- Missing specific requirements for scalability and error handling

**Recommendation**: Address clarifications, then proceed

## ⚠️ Important Gaps

### Gap #1: Unclear Scalability Requirements
**Severity**: High

**Problem**:
Plan states "handle multiple users" without specifying:
- How many concurrent connections expected?
- What's the message throughput requirement?
- Are there any latency requirements?

**Impact**:
Cannot make informed decisions about:
- Server infrastructure sizing
- Need for load balancing
- Message queue selection
- WebSocket server choice (Socket.IO, SignalR, native)

**Recommendation**:
Specify:
- Target: [X] concurrent connections
- Target: [Y] messages per second
- Target: [Z]ms latency p95
- Growth projection: [expected increase over time]

### Gap #2: Error Handling Strategy Incomplete
**Severity**: High

**Problem**:
Plan mentions "handle disconnections" but doesn't specify:
- Reconnection strategy (exponential backoff? max retries?)
- Message queue behavior when user offline
- What happens to messages sent during disconnection?

**Recommendation**:
Define:
```csharp
public class ReconnectionPolicy {
    public int MaxRetries { get; set; } = 5;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);
    public BackoffStrategy Strategy { get; set; } = BackoffStrategy.Exponential;
}

public class MessageQueuePolicy {
    public int MaxQueuedMessages { get; set; } = 100;
    public TimeSpan MessageTTL { get; set; } = TimeSpan.FromHours(24);
    public QueueBehavior WhenFull { get; set; } = QueueBehavior.DropOldest;
}
```

## Questions for Clarification
1. What are the expected concurrent user numbers?
2. Should messages be persisted (message history)?
3. What's the budget for infrastructure (affects scalability approach)?
4. Are push notifications (mobile) also required?

**Overall Readiness**: 22/30 items complete (73%) - Needs detail
```

## Best Practices

### Be Constructive, Not Critical

❌ **Poor Feedback**:
"This plan is incomplete and poorly thought out."

✅ **Good Feedback**:
"The plan has a solid architectural foundation. To make it implementation-ready, we need to add three key elements: [1] specific performance requirements, [2] error handling for network failures, and [3] database migration strategy."

### Be Specific and Actionable

❌ **Vague Feedback**:
"Error handling needs improvement."

✅ **Specific Feedback**:
"Add error handling for these scenarios:
1. Database connection timeout (retry 3x with exponential backoff)
2. Invalid user input (return 400 with specific error messages)
3. External API failure (circuit breaker pattern, fallback to cached data)"

### Provide Examples

❌ **Abstract Feedback**:
"The data model needs validation rules."

✅ **Concrete Feedback**:
"Add validation to UserModel:
```csharp
[Required]
[EmailAddress]
public string Email { get; set; }

[Required]
[MinLength(8)]
[RegularExpression(@"^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*])", 
    ErrorMessage = "Password must contain uppercase, number, and special character")]
public string Password { get; set; }
```"

### Acknowledge Good Work

Always include a "Strengths" section highlighting what's done well. This:
- Provides positive reinforcement
- Helps team understand what good looks like
- Balances criticism with recognition
- Builds trust in your feedback

## Final Reminders

### Core Principles

1. **Thoroughness**: Check all five core areas (completeness, conflicts, data, dependencies, risks)
2. **Clarity**: Make every issue actionable with specific recommendations
3. **Prioritization**: Clearly distinguish critical blockers from nice-to-haves
4. **Constructiveness**: Frame feedback as opportunities to improve
5. **Pragmatism**: Consider project context (prototype vs production)

### You Are Successful When

- ✅ Development team has confidence to start implementation
- ✅ All critical blockers identified before coding begins
- ✅ Ambiguous specifications clarified
- ✅ Potential conflicts prevented
- ✅ Risk awareness established
- ✅ Plan quality measurably improved

Your role is to be the final checkpoint before implementation, catching issues in the planning phase where they're cheapest and easiest to fix. A thorough validation now prevents costly rework later.
