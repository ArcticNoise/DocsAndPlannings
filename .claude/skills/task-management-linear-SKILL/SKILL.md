---
name: task-management-linear-SKILL
description: Task management workflow using Linear for project tracking, issue management, status transitions, parent-child synchronization, and quality gates. Mandatory for all work tracking with proper state management through Linear MCP.
allowed-tools: mcp__linear-server__list_issues, mcp__linear-server__update_issue, mcp__linear-server__get_issue, mcp__linear-server__create_issue, mcp__linear-server__list_projects, mcp__linear-server__get_project, mcp__linear-server__create_project, mcp__linear-server__create_comment, mcp__linear-server__list_issue_statuses
---

# Linear Task Management Skill

## Overview
This skill defines task management workflow using Linear for project tracking, including issue management, status transitions, parent-child synchronization, and quality gates. All work must be tracked through Linear with proper state management.

## Critical Prerequisites

### Linear MCP Server Requirement

**MANDATORY:** The `linear-server` MCP must be available and functional.

**If linear-server MCP is unavailable:**
1. **STOP your current task immediately**
2. **Report the issue to the user**
3. **Do not proceed with any work**

**Verification before starting any task:**
```
Check: Is linear-server MCP available?
Yes → Proceed with workflow
No  → STOP and REPORT to user
```

**Why this is critical:**
- All work must be tracked in Linear
- Task state management is mandatory
- Ensures project visibility and progress tracking
- Maintains team coordination
- Provides audit trail

**Error message to user:**
```
"linear-server MCP is unavailable. Cannot proceed with task management.
Please check MCP configuration and ensure linear-server is properly set up."
```

## Linear Issue Structure

### Autonomous Issue Principle

**Rule 1: Each issue must be autonomous and independently completable**

An autonomous issue means:
- Can be completed without waiting for other issues to finish
- Has all necessary context and requirements
- Includes clear acceptance criteria
- Specifies all dependencies explicitly
- Is self-contained and actionable

**Examples:**

```
✅ GOOD - Autonomous Issue
Title: "Implement user registration API endpoint"
Description: 
- Create POST /api/auth/register endpoint
- Validate email format and password strength
- Hash password using bcrypt
- Store user in database
- Return JWT token on success
- Return appropriate errors for invalid input
Acceptance Criteria:
- Endpoint accepts email and password
- Returns 201 with token on success
- Returns 400 for invalid input
- Returns 409 for duplicate email
- Unit tests covering all cases

✅ GOOD - Autonomous Issue
Title: "Add password reset functionality"
Description: Complete password reset flow including email sending
Dependencies: Email service already implemented
Acceptance Criteria: [specific criteria]

❌ BAD - Not Autonomous
Title: "Add login button to UI"
Description: Add button that calls login endpoint
Issues: 
- Depends on login endpoint being completed first
- Not self-contained
- Should be part of larger issue or clarify dependencies

❌ BAD - Too Vague
Title: "Fix authentication"
Description: Make authentication work better
Issues:
- No clear scope
- No acceptance criteria
- Too broad
```

**Benefits of autonomous issues:**
- Parallel work possible
- Clear completion definition
- Easier estimation
- Better progress tracking
- Reduces blocking

### Milestone Issues and Sub-Issues

**Rule 2: Break down large features into milestone issues with sub-issues**

When creating a large feature or milestone:
1. Create main milestone issue in Linear
2. Break down into smaller autonomous sub-issues
3. Link all related issues as **sub-issues** to main issue

**Structure:**
```
📋 [MILESTONE] User Authentication System
Description: Complete authentication system for the application
  
  Sub-issues:
  ├─ 📋 Design authentication database schema
  ├─ 📋 Implement JWT token service
  ├─ 📋 Create user registration endpoint
  ├─ 📋 Create login endpoint
  ├─ 📋 Add password hashing utility
  ├─ 📋 Implement password reset flow
  ├─ 📋 Add session management
  └─ 📋 Write authentication integration tests
```

**Sub-issue requirements:**
- Each sub-issue is autonomous
- Each has clear acceptance criteria
- Each can be completed independently (or dependencies are explicit)
- Together they complete the milestone

**How to create in Linear:**
```
1. Create main milestone issue
2. For each sub-task:
   - Create new issue
   - Set parent to milestone issue
   - Add appropriate labels/tags
   - Set priority if needed
3. Verify all sub-issues link to parent
```

**Benefits:**
- Clear progress tracking (e.g., "3 of 8 sub-issues complete")
- Logical work breakdown
- Maintains relationships between related tasks
- Easier to understand project scope
- Can assign different sub-issues to different people

## Issue Status Management

### Available Statuses

Linear issues flow through these states:
- **Backlog**: Not yet ready for active work
- **TODO**: Ready to be worked on immediately
- **In Progress**: Currently being worked on
- **Done**: Completed and verified
- **Cancelled**: Will not be completed

### Status Transition Rules

#### Working from TODO Status

**Rule 3: All active work comes from TODO status**

**Workflow:**
```
1. Check Linear for issues in TODO status
2. Select an issue to work on
3. Move issue from TODO → In Progress
4. Begin work
```

**If no issues in TODO:**
```
Action: STOP and REPORT
Message: "No issues available in TODO status. Please move issues 
         from Backlog to TODO or create new issues before proceeding."
         
Do NOT:
- Start working on anything
- Take issues from Backlog without permission
- Create new issues without approval
```

**Example dialog:**
```
User: "Start working on the project"
Assistant: "I've checked Linear and there are no issues in TODO status.
           I cannot proceed without a task assigned in TODO. 
           Would you like me to:
           1. Wait while you move an issue to TODO
           2. Check Backlog for specific issues to move
           3. Create a new issue for specific work"
```

#### Backlog Issues

**Rule 4: Never take issues from Backlog without explicit permission**

**Backlog is for:**
- Issues not yet ready for implementation
- Tasks awaiting more information
- Ideas under consideration
- Work that depends on incomplete features
- Future enhancements

**To move from Backlog to TODO:**
```
Requirements:
✅ User explicitly requests it
✅ All requirements are complete
✅ All dependencies are resolved
✅ Acceptance criteria are clear

Process:
1. User identifies Backlog issue
2. User confirms it's ready
3. Move to TODO
4. Then begin work
```

**Do NOT:**
- Assume Backlog issues are ready
- Move Backlog to TODO automatically
- Work on Backlog issues directly

#### Cancelled Issues

**Rule 5: NEVER work on Cancelled issues**

**Cancelled means:**
- Explicitly marked as not to be completed
- Obsolete or no longer needed
- Superseded by other work
- Decided against during planning

**If you see a Cancelled issue that seems relevant:**
```
Action: Ask the user
Message: "I found a Cancelled issue that might be relevant: [issue].
         Should this be reopened and moved to TODO, or is there
         a different issue I should work on instead?"
         
Do NOT:
- Assume it should be reopened
- Start working on it
- Move it to TODO automatically
```

### Parent-Child Status Synchronization

**Rule 6: Status changes in sub-issues propagate to parent issue**

This maintains accurate status of milestone issues.

#### Sub-Issue → TODO

```
Trigger: Any sub-issue moves to TODO
Action: Move parent issue to TODO (if not already)

Example:
Sub-Issue: "Implement JWT service" → TODO
Parent: "User Authentication System" → TODO

Reason: Work has started on the milestone
```

#### Sub-Issue → In Progress

```
Trigger: Any sub-issue moves to In Progress
Action: Move parent issue to In Progress (if not already)

Example:
Sub-Issue: "Create login endpoint" → In Progress
Parent: "User Authentication System" → In Progress

Reason: Active work is happening on the milestone
```

#### All Sub-Issues → Done

```
Trigger: ALL sub-issues completed and in Done status
Action: Move parent issue to Done

Example:
✓ Sub-Issue: "Design schema" → Done
✓ Sub-Issue: "Implement JWT" → Done
✓ Sub-Issue: "Create registration" → Done
✓ Sub-Issue: "Create login" → Done
✓ Sub-Issue: "Add hashing" → Done
✓ Sub-Issue: "Add reset" → Done
✓ Sub-Issue: "Add sessions" → Done
✓ Sub-Issue: "Write tests" → Done
→ Parent: "User Authentication System" → Done

Reason: Milestone is complete
```

**Important Notes:**
- Parent only moves to Done when **ALL** sub-issues are Done
- If any sub-issue is still In Progress, parent stays In Progress
- If parent has no sub-issues, manage its status independently

**Implementation:**
```
After moving sub-issue status:
1. Get parent issue ID
2. Check if parent exists
3. Apply appropriate status transition rule
4. Update parent status in Linear
5. Log the status change
```

## Task Execution Workflow

### High-Level Workflow Overview

```
┌─────────────────────────────────────────────────┐
│ 1. Check Linear TODO Status                    │
│    - Issues available? → Proceed               │
│    - No issues? → STOP and REPORT              │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ 2. Select Issue from TODO                      │
│    - Review requirements                        │
│    - Check acceptance criteria                  │
│    - Verify dependencies met                    │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ 3. Move Issue: TODO → In Progress              │
│    - Update Linear status                       │
│    - Update parent status if sub-issue          │
│    - Add comment if needed                      │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ 4. Execute Implementation                       │
│    - Follow git-workflow-SKILL.md               │
│    - Use appropriate coding standard skills     │
│    - Use specialized subagents                  │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ 5. Code Review & Testing                        │
│    - Run code-review-linear-task-creator        │
│    - Run bug-hunter-tester                      │
│    - Fix all issues found                       │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ 6. Complete Task                                │
│    - Merge to master (see git-workflow)         │
│    - Move Issue: In Progress → Done            │
│    - Update parent status if all sub-issues Done│
│    - Add completion comment                     │
└─────────────────────────────────────────────────┘
```

### Detailed Workflow Steps

#### Step 1: Check Linear for Available Work

```
Action: Query Linear for TODO issues

Linear API Call:
list_issues(status: "TODO", limit: 20)

Decision:
- Issues found → Select one and proceed
- No issues → STOP and report to user
```

#### Step 2: Select and Review Issue

```
Action: Choose issue and verify it's ready

Checks:
✓ Requirements are clear
✓ Acceptance criteria defined
✓ Dependencies met (if any)
✓ You have necessary context

If unclear:
- Ask user for clarification
- Request more details in Linear
- Don't proceed with incomplete information
```

#### Step 3: Update Status to In Progress

```
Action: Move issue and parent (if applicable)

Linear API Calls:
1. update_issue(issueId, status: "In Progress")
2. If sub-issue: update parent status to "In Progress"
3. Optional: add_comment(issueId, "Started work on [task]")

Verification:
- Linear status updated
- Parent status updated (if applicable)
- Team can see work has started
```

#### Step 4: Execute Implementation

**Reference other skills for actual implementation:**
- **git-workflow-SKILL.md** - Branch creation, commits, merging
- **cpp-coding-standards-SKILL.md** - C++ implementation
- **csharp-coding-standards-SKILL.md** - C# implementation

**Use specialized subagents:**
- `cpp-senior-engineer` - For C++ implementation
- `csharp-senior-engineer` - For C# implementation
- Project-specific subagents as needed

**Implementation checklist:**
```
[ ] Create feature branch (see git-workflow)
[ ] Implement changes following coding standards
[ ] Write unit tests first (TDD)
[ ] Commit changes properly (see git-workflow)
[ ] Verify code compiles without warnings
[ ] Ensure acceptance criteria are met
```

#### Step 5: Quality Validation

**Code Review:**
```
Subagent: code-review-linear-task-creator

Process:
1. Run code review subagent
2. Review all findings
3. Fix issues using appropriate subagents
4. Commit fixes to branch
5. Re-run review if significant changes
6. Proceed when review passes
```

**Testing:**
```
Subagent: bug-hunter-tester

Process:
1. Run testing validation subagent
2. Verify all unit tests pass
3. Check integration tests pass
4. Validate acceptance criteria
5. Fix any test failures
6. Commit fixes to branch
7. Re-run tests until all pass
```

**Quality gates must PASS:**
- ✅ Code review passed
- ✅ All tests passed
- ✅ Acceptance criteria met
- ✅ No regressions introduced

#### Step 6: Complete and Update Linear

```
Action: Finalize work and update Linear

Steps:
1. Merge to master (see git-workflow-SKILL.md)
2. Update Linear: In Progress → Done
3. If sub-issue: Check if all sub-issues Done → Update parent to Done
4. Add completion comment with summary

Linear API Calls:
update_issue(issueId, status: "Done")
add_comment(issueId, "Completed: [summary of work]")

If sub-issue:
- Check all sibling sub-issues status
- If all Done: update_issue(parentId, status: "Done")
```

**Completion comment template:**
```
Completed [issue title]

Changes made:
- [Key change 1]
- [Key change 2]
- [Key change 3]

Acceptance criteria met:
✓ [Criterion 1]
✓ [Criterion 2]
✓ [Criterion 3]

Branch: feature/ISSUE-123-description
Commits: [commit hashes]
```

## Quality Gates

### Mandatory Quality Gates

Every task must pass through these gates before completion:

#### Gate 1: Requirements Clarity
```
Checkpoint: Before starting work
Validator: Manual review
Requirements:
- Issue requirements are clear
- Acceptance criteria defined
- Dependencies identified
- Scope is understood
Status: PASS/FAIL
Action if FAIL: Ask user for clarification
```

#### Gate 2: Implementation Standards
```
Checkpoint: During implementation
Validator: Coding standards skills
Requirements:
- Follows C++/C# coding standards
- Proper architecture and design
- Documentation present
- Tests written
Status: PASS/FAIL
Action if FAIL: Refactor to meet standards
```

#### Gate 3: Code Review
```
Checkpoint: After implementation
Validator: code-review-linear-task-creator subagent
Requirements:
- No code quality issues
- Best practices followed
- No potential bugs
- Architecture appropriate
Status: PASS/FAIL
Action if FAIL: Fix issues and re-review
```

#### Gate 4: Testing Validation
```
Checkpoint: After code review passes
Validator: bug-hunter-tester subagent
Requirements:
- All unit tests pass
- Integration tests pass
- No regressions
- Acceptance criteria validated
Status: PASS/FAIL
Action if FAIL: Fix failures and retest
```

#### Gate 5: Linear Status Updated
```
Checkpoint: After merge to master
Validator: Manual verification
Requirements:
- Issue status updated to Done
- Parent status updated (if applicable)
- Completion comment added
- All sub-issues accounted for
Status: PASS/FAIL
Action if FAIL: Update Linear properly
```

**Only when ALL gates PASS → Task is complete**

## Error Handling

### No Issues in TODO

**Scenario:** Linear TODO status is empty

**Actions:**
```
1. STOP current work immediately
2. Report to user:
   "No issues available in TODO status.
    
    Options:
    - Move issues from Backlog to TODO
    - Create new issues
    - Review In Progress issues
    
    I cannot proceed without tasks in TODO."
3. Wait for user instructions
4. Do not start any work
```

### Linear MCP Unavailable

**Scenario:** linear-server MCP not responding

**Actions:**
```
1. STOP ALL work immediately
2. Report to user:
   "CRITICAL: linear-server MCP is unavailable.
    
    Cannot proceed with task management.
    Please check MCP configuration.
    
    Steps to resolve:
    - Verify linear-server is installed
    - Check MCP configuration in settings
    - Restart Claude Code if needed"
3. Do not proceed with any work
4. This is a BLOCKING issue
```

### Issue Requirements Unclear

**Scenario:** Selected issue lacks clear requirements

**Actions:**
```
1. Do not start implementation
2. Ask user:
   "Issue [ISSUE-ID] has unclear requirements.
    
    Missing:
    - [What's unclear]
    
    Can you provide:
    - [What you need]"
3. Wait for clarification
4. Update issue in Linear with new information
5. Then proceed
```

### Quality Gate Failure

**Scenario:** Code review or tests fail

**Actions:**
```
1. Do NOT merge to master
2. Review failures:
   - Understand each issue
   - Prioritize fixes
3. Fix issues using appropriate subagents
4. Commit fixes to branch
5. Re-run quality gate
6. Repeat until PASS
7. Only then proceed to merge
```

### Parent Issue Status Confusion

**Scenario:** Not sure when to update parent status

**Decision Tree:**
```
Is this a sub-issue?
├─ No → Manage status independently
└─ Yes → Follow parent sync rules
    ├─ Moving to TODO → Parent to TODO
    ├─ Moving to In Progress → Parent to In Progress
    └─ Moving to Done → Check siblings
        ├─ All siblings Done → Parent to Done
        └─ Any sibling not Done → Parent stays In Progress
```

## Integration with Other Skills

### Git Workflow Integration

This task management skill **references** git-workflow-SKILL.md for:
- Branch creation and naming
- Commit message formatting
- Merge procedures
- Repository state management

**Workflow:**
```
Task Management Skill: Orchestrates when to do git operations
Git Workflow Skill: Defines how to do git operations
```

### Coding Standards Integration

This skill **references** coding standards for:
- C++ implementation → cpp-coding-standards-SKILL.md
- C# implementation → csharp-coding-standards-SKILL.md

**Workflow:**
```
Task Management Skill: Ensures standards are followed
Coding Standards Skills: Define what standards to follow
```

### Subagent Integration

This skill **orchestrates** subagent usage:
- `cpp-senior-engineer` - C++ implementation
- `csharp-senior-engineer` - C# implementation  
- `code-review-linear-task-creator` - Code quality validation
- `bug-hunter-tester` - Testing validation

**Workflow:**
```
Task Management Skill: Calls subagents at appropriate workflow steps
Subagents: Execute specialized tasks
```

## Quick Reference

### Status Transition Rules
```
✅ Work from TODO only
❌ Never work from Cancelled
⚠️ Backlog only with explicit permission

Sub-issue → TODO ⟹ Parent → TODO
Sub-issue → In Progress ⟹ Parent → In Progress
All sub-issues → Done ⟹ Parent → Done
```

### Workflow Checklist
```
[ ] Check linear-server MCP available
[ ] Find issue in TODO status
[ ] Review requirements and acceptance criteria
[ ] Move issue to In Progress (update parent if sub-issue)
[ ] Implement using appropriate skills/subagents
[ ] Pass code review (code-review-linear-task-creator)
[ ] Pass testing (bug-hunter-tester)
[ ] Merge to master (see git-workflow-SKILL.md)
[ ] Move issue to Done (update parent if all sub-issues done)
[ ] Add completion comment
```

### Linear API Common Operations
```
# List TODO issues
list_issues(status: "TODO")

# Update issue status
update_issue(issueId, status: "In Progress")
update_issue(issueId, status: "Done")

# Add comment
add_comment(issueId, "comment text")

# Get issue details
get_issue(issueId)

# Create sub-issue
create_issue(title, description, parent: parentIssueId)
```

## Summary

### Key Principles

1. **Linear is mandatory** - No work without Linear tracking
2. **Autonomous issues** - Each issue is independently completable
3. **Status discipline** - Work from TODO, never from Cancelled
4. **Parent synchronization** - Sub-issue status propagates to parent
5. **Quality gates** - All gates must pass before completion
6. **Clear communication** - Report issues, ask for clarification

### Success Criteria

For successful task completion:
- ✅ Linear MCP functional and available
- ✅ Issue clearly defined with acceptance criteria
- ✅ Status properly maintained throughout workflow
- ✅ All quality gates passed
- ✅ Code merged to master (see git-workflow)
- ✅ Issue status updated to Done
- ✅ Parent status updated appropriately
- ✅ Completion documented

### Common Pitfalls to Avoid

- ❌ Working without Linear tracking
- ❌ Taking issues from Cancelled
- ❌ Starting work from Backlog without permission
- ❌ Skipping quality gates
- ❌ Not updating parent issue status
- ❌ Working when no issues in TODO
- ❌ Proceeding with unclear requirements
- ❌ Forgetting to add completion comments

**Remember:** Linear tracking is non-negotiable. Quality gates are mandatory, not optional.
