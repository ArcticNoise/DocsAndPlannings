---
name: dev-plan-manager-SKILL
description: Task management, bug triage, priority management, and sprint planning. Use when breaking down features, tracking bugs, prioritizing work, coordinating development activities, or aligning daily work with strategic goals.
---

# Development Plan Management Skill

## When to Use This Skill

Use this skill whenever you need to manage tasks, track bugs, prioritize work, or coordinate development activities. This includes:

- Breaking down features into actionable tasks
- Triaging and prioritizing bugs
- Recommending what to work on next
- Tracking progress toward milestones
- Managing sprint planning and execution
- Coordinating work across team members
- Aligning daily work with strategic goals
- Managing technical debt alongside feature work

**Key Indicators**: User mentions "what should I work on next", "break this down into tasks", "prioritize these items", "track progress", "we have a bug", "plan the sprint", or asks about project status.

## Core Responsibilities

### 1. Task Decomposition & Creation

Break down features and epics into granular, implementable tasks that align with the project's development plan.

**Task Creation Principles**:

```
✅ Good Task:
Title: Implement TransformComponent with SIMD optimization
Description:
- Create TransformComponent struct with position, rotation, scale
- Implement SIMD-accelerated matrix operations
- Add unit tests for matrix calculations
- Validate memory layout for cache efficiency
Acceptance Criteria:
- [ ] Component stores transform data in contiguous memory
- [ ] SIMD operations 4x faster than scalar baseline
- [ ] Unit tests achieve 95%+ coverage
- [ ] No heap allocations during transform updates
Estimate: 8 hours
Labels: ECS, Performance, Component
Dependencies: None
```

```
❌ Poor Task:
Title: Transform stuff
Description: Make transforms work
Estimate: Unknown
```

**Task Decomposition Example**:

```
Epic: Player Movement System
│
├─ Feature: Input Handling
│  ├─ Task: Create InputComponent with keyboard/gamepad state
│  │  Estimate: 3 hours
│  │  Dependencies: None
│  │
│  ├─ Task: Implement InputSystem to poll and update components
│  │  Estimate: 4 hours
│  │  Dependencies: InputComponent
│  │
│  └─ Task: Add unit tests for input edge cases
│     Estimate: 2 hours
│     Dependencies: InputSystem
│
├─ Feature: Physics Integration
│  ├─ Task: Add VelocityComponent and ForceComponent
│  │  Estimate: 2 hours
│  │  Dependencies: None
│  │
│  ├─ Task: Create PhysicsSystem for force integration
│  │  Estimate: 6 hours
│  │  Dependencies: VelocityComponent, ForceComponent
│  │
│  └─ Task: Implement collision detection with native physics library
│     Estimate: 12 hours
│     Dependencies: PhysicsSystem, Native Physics P/Invoke wrapper
│
└─ Feature: Movement System
   ├─ Task: Create MovementSystem using input and physics
   │  Estimate: 4 hours
   │  Dependencies: InputSystem, PhysicsSystem
   │
   ├─ Task: Add movement constraints (max speed, acceleration)
   │  Estimate: 3 hours
   │  Dependencies: MovementSystem
   │
   └─ Task: Performance test with 10,000 entities
      Estimate: 4 hours
      Dependencies: MovementSystem complete
```

**Task Sizing Guidelines**:

- **Small** (1-4 hours): Single component, simple function, isolated test
- **Medium** (4-8 hours): System implementation, P/Invoke wrapper, integration
- **Large** (8-16 hours): Complex system, native library integration, optimization
- **Epic** (16+ hours): Break down further into smaller tasks

**Task Checklist**:
- [ ] Title is clear and action-oriented
- [ ] Description explains what and why
- [ ] Acceptance criteria are specific and testable
- [ ] Estimate is reasonable (2-16 hours ideal)
- [ ] Dependencies identified
- [ ] Labels/tags applied for filtering
- [ ] Assigned to appropriate person (or unassigned for pool)

### 2. Bug Management & Triage

Assess, prioritize, and track bugs in relation to the development plan.

**Bug Severity Classification**:

**Critical (P0)** - Fix immediately, stop other work:
- Production system completely down
- Data loss or corruption
- Security vulnerability actively exploited
- Memory corruption causing crashes
- Deadlock in main game loop

**Major (P1)** - Fix this sprint:
- Core feature completely broken
- Significant memory leak (megabytes per minute)
- Crash in common user scenario
- P/Invoke marshaling causing data corruption
- Performance regression >50%

**Minor (P2)** - Fix next sprint:
- Feature partially broken
- Edge case crash
- Minor memory leak (kilobytes over hours)
- UI glitch or visual artifact
- Incorrect error message

**Trivial (P3)** - Fix when convenient:
- Cosmetic issue with no functional impact
- Documentation error
- Code style inconsistency
- Optimization opportunity (no current problem)

**Bug Triage Process**:

```
Step 1: GATHER INFORMATION
- [ ] Reproduction steps documented
- [ ] Environment details (OS, architecture, build type)
- [ ] Expected vs actual behavior clear
- [ ] Impact scope assessed (how many users/scenarios)
- [ ] Frequency determined (always, sometimes, rare)

Step 2: CLASSIFY SEVERITY
- Critical: Production/data/security impact
- Major: Core functionality broken
- Minor: Partial functionality or edge case
- Trivial: No functional impact

Step 3: DETERMINE PRIORITY
Consider:
- User impact (how many users affected)
- Workaround availability (can users avoid it?)
- Business risk (financial, reputational, legal)
- Development plan impact (blocks milestone work?)

Step 4: ASSIGN & SCHEDULE
- Critical: Immediate hotfix, all hands
- Major: Current sprint, specific assignee
- Minor: Next sprint, add to backlog
- Trivial: Backlog, fix opportunistically

Step 5: TRACK & VERIFY
- [ ] Root cause identified
- [ ] Fix implemented and tested
- [ ] Regression tests added
- [ ] Verified in production/staging
```

**Bug Report Template**:

```markdown
**Title**: [Component/System]: [Brief description of issue]

**Severity**: [Critical / Major / Minor / Trivial]
**Priority**: [P0 / P1 / P2 / P3]
**Status**: [New / Investigating / In Progress / Fixed / Verified]

**Environment**:
- OS: Windows 10 x64
- Build: Debug / Release
- Commit: abc1234

**Description**:
[Clear explanation of what's wrong]

**Reproduction Steps**:
1. Start application with 1000 entities
2. Run MovementSystem for 60 seconds
3. Observe memory usage increase from 50MB to 500MB

**Expected Behavior**:
Memory usage should remain stable around 50-60MB

**Actual Behavior**:
Memory usage grows continuously by ~7-8MB per second

**Root Cause** (if known):
MovementSystem allocates temporary List<T> in tight loop without reuse

**Proposed Fix**:
Cache List<T> as system field and reuse with Clear() each frame

**Impact**:
- Affects: All users running game for >2 minutes
- Workaround: Restart application every 2 minutes
- Blocks: Performance milestone (requires <100MB baseline)

**Related Issues**:
- Similar leak in RenderSystem (#234)
- Memory profiling task (#189)
```

### 3. Priority Management

Maintain alignment between daily work and strategic development plan.

**Priority Decision Framework**:

```
┌─────────────────────────────────────────────────┐
│         CRITICAL PATH ANALYSIS                   │
├─────────────────────────────────────────────────┤
│ 1. Production emergencies (P0 bugs)             │
│ 2. Blocking dependencies (unblock teammates)    │
│ 3. Milestone-critical work (sprint goals)       │
│ 4. High-value features (ROI, user impact)       │
│ 5. Technical debt (impacts velocity)            │
│ 6. Nice-to-have improvements                    │
└─────────────────────────────────────────────────┘
```

**Priority Matrix** (Eisenhower):

```
              URGENT
                │
    Q1          │         Q2
  CRITICAL      │      PLANNED
   BUGS         │      FEATURES
  DO FIRST      │    SCHEDULE
                │
────────────────┼────────────────  IMPORTANT
                │
    Q3          │         Q4
 INTERRUPTS     │    TIME WASTERS
  DELEGATE      │     ELIMINATE
                │
            NOT URGENT
```

**Recommendations by Scenario**:

**Scenario 1: Feature complete, what's next?**
```
Analysis:
✓ User profile page complete (milestone M1)
✓ No blocking bugs
✓ 3 days until sprint end

Recommendation:
1. Pick next M1 task: "Add password change functionality" (4h)
2. Alternative: Start M2 prep work if M1 fully complete
3. Avoid: Starting large new feature with 3 days left

Rationale:
- Stay focused on current milestone M1
- Choose task completable before sprint end
- Build momentum toward milestone completion
```

**Scenario 2: Bug reported during feature development**
```
Analysis:
Current work: Implementing payment integration (8h remaining)
Bug: Login page crashes on mobile (Major, P1)
Sprint: 5 days remaining, payment is milestone-critical

Recommendation:
Option A: Continue payment, assign bug to teammate
Option B: Pause payment, fix bug (2h estimate), resume
Option C: Escalate for team priority decision

Factors:
+ Payment blocks milestone, high business value
+ Bug has workaround (desktop login works)
+ Bug affects significant users (~30%)
- Payment can't be tested without login fix

Decision: Option B - Fix login first (unblocks testing)
```

**Scenario 3: Competing high-priority items**
```
Items:
A. Implement multiplayer networking (Epic, 40h)
B. Fix memory leak in ECS queries (P1, 4h)
C. Optimize rendering for low-end devices (16h)
D. Add unit tests for new components (8h)

Analysis:
- B is quick win, improves stability
- D prevents future bugs, enables safe iteration
- A is large, could take entire sprint
- C is important but not blocking

Recommended Order:
1. B: Fix memory leak (stabilize foundation)
2. D: Add unit tests (quality gate)
3. C: Optimize rendering (performance milestone)
4. A: Start multiplayer (break into smaller tasks first)

Rationale:
- Fix critical issues before building new features
- Establish quality practices early
- Defer large epics until properly decomposed
```

**Priority Adjustment Triggers**:

Reassess priorities when:
- ⚠️ Production bug discovered (may need immediate attention)
- 🚫 Blocking dependency emerges (unblock teammates first)
- 📅 Milestone deadline approaches (focus on critical path)
- 👥 Team member becomes available/unavailable (reassign work)
- 💡 New information changes assumptions (strategic pivot)
- 🔥 External emergency (customer escalation, security incident)

### 4. Progress Tracking & Reporting

Monitor execution against the development plan and provide clear status updates.

**Sprint Dashboard Format**:

```markdown
# Sprint 23: Core ECS Implementation
**Duration**: Jan 15 - Jan 29 (2 weeks)
**Goal**: Complete component storage and query system

## Progress Overview
████████████░░░░░░░░ 60% Complete (Day 7/14)

**Velocity**: 32 points / 40 planned (On track)
**Burndown**: Healthy - tracking slightly ahead

## Milestone Status

### M1: Component Storage System ✓ COMPLETE
- [x] ComponentArray<T> implementation (8 pts)
- [x] Memory pool allocator (5 pts)
- [x] Unit tests for storage (3 pts)

### M2: Query System ⚠️ AT RISK
- [x] Query builder API (5 pts)
- [x] Component signature matching (3 pts)
- [ ] Query caching and compilation (8 pts) - In Progress
- [ ] Performance benchmarks (3 pts) - Blocked

**Risk**: Query caching taking longer than estimated (day 7, expected day 5)
**Mitigation**: Added 2nd developer to parallelize benchmark setup

### M3: System Scheduler 📋 NOT STARTED
- [ ] System registration (5 pts)
- [ ] Execution order dependencies (8 pts)
- [ ] Multi-threaded system execution (13 pts)

## Task Breakdown

**Completed (16 tasks)**: ████████████████ 100%
**In Progress (3 tasks)**: ████████░░░░░░░░ 50%
**Blocked (1 task)**: ░░░░░░░░░░░░░░░░ 0%
**Not Started (8 tasks)**: ░░░░░░░░░░░░░░░░ 0%

## Active Work

| Task | Assignee | Status | Remaining |
|------|----------|--------|-----------|
| Query caching | Alice | In Progress | 4h |
| P/Invoke wrapper | Bob | In Progress | 6h |
| Memory profiling | Carol | In Progress | 2h |

## Blockers 🚫

1. **Performance benchmarks blocked**
   - Waiting for: Query caching completion
   - Impact: Can't validate performance requirements
   - Action: Alice focused on unblocking this (ETA: EOD)

## Risks ⚠️

1. **Query caching complexity**
   - Risk: May slip into next sprint
   - Probability: Medium
   - Impact: Delays dependent benchmarks
   - Mitigation: Added reviewer, daily check-ins

2. **Multi-threaded execution scope**
   - Risk: 13 points may be underestimated
   - Probability: High
   - Impact: M3 incomplete this sprint
   - Mitigation: Break into smaller tasks, defer non-critical parts

## Wins 🎉

- Component storage 2x faster than baseline target
- Zero memory leaks detected in testing
- All P1 bugs from last sprint resolved

## Next Sprint Preview

**Planned Focus**: System scheduler and multi-threading
**Prerequisites**: M2 completion (query system)
**Estimated Capacity**: 40 points (team of 3)
```

**Daily Standup Format**:

```markdown
# Daily Standup - Jan 22

## Alice
**Yesterday**: Implemented query caching (70% complete)
**Today**: Finish query caching, start cache invalidation
**Blockers**: None

## Bob
**Yesterday**: Completed P/Invoke function declarations
**Today**: Add error handling and marshaling tests
**Blockers**: Waiting for native library build fix (Carol)

## Carol
**Yesterday**: Debugged native library build issues
**Today**: Fix build, review Bob's P/Invoke code
**Blockers**: None

## Summary
- ✅ On track for sprint goal
- ⚠️ Bob blocked by build issue (Carol resolving today)
- 🎯 Query system on target for EOW completion
```

**Burndown Chart** (Text Format):

```
Story Points Remaining

40 │ ●
   │  ╲
30 │   ●╲
   │    ╲●
20 │     ╲╲●
   │      ╲╲╲●  ← Actual
10 │       ╲╲╲╲● 
   │        ╲╲╲╲╲● (Ideal)
 0 │         ╲╲╲╲╲●
   └─────────────────────
   D1 D3 D5 D7 D9 D11 D13

Status: On track (actual slightly ahead of ideal)
```

**Key Metrics to Track**:

- **Velocity**: Points completed per sprint (average last 3 sprints)
- **Cycle Time**: Days from start to completion per task
- **Lead Time**: Days from creation to completion per task
- **Throughput**: Tasks completed per week
- **Blocked Time**: Percentage of tasks currently blocked
- **Bug Escape Rate**: Bugs found in QA/production vs caught in development
- **Test Coverage**: Percentage of code covered by tests
- **Technical Debt Ratio**: Debt work vs feature work time

### 5. Sprint Planning

Structure and plan sprint work aligned with development milestones.

**Sprint Planning Process**:

```
STEP 1: REVIEW LAST SPRINT (30 min)
├─ Velocity achieved vs planned
├─ What went well (celebrate wins)
├─ What went poorly (improve process)
└─ Incomplete work (carry over or deprioritize)

STEP 2: MILESTONE ALIGNMENT (15 min)
├─ Current milestone status
├─ Next milestone requirements
├─ Dependencies between milestones
└─ Adjust plan if needed

STEP 3: BACKLOG REFINEMENT (45 min)
├─ Review top backlog items
├─ Break down large tasks (>16h)
├─ Clarify acceptance criteria
├─ Estimate effort (story points or hours)
└─ Identify dependencies

STEP 4: CAPACITY PLANNING (15 min)
├─ Available team capacity (days × team members)
├─ Account for time off, meetings, support
├─ Reserve capacity for bugs (10-20%)
└─ Calculate velocity target

STEP 5: COMMITMENT (30 min)
├─ Select tasks for sprint
├─ Verify dependencies satisfied
├─ Assign tasks to team members
├─ Confirm sprint goal achievable
└─ Get team commitment

STEP 6: SPRINT GOAL (15 min)
└─ Define clear, measurable sprint goal
```

**Sprint Planning Template**:

```markdown
# Sprint 24 Planning
**Duration**: Jan 29 - Feb 12 (2 weeks)
**Team Capacity**: 80 hours (4 people × 10 days × 2 hours productive/day)
**Reserved for Bugs**: 16 hours (20%)
**Available for Features**: 64 hours

## Sprint Goal
Complete system scheduler with basic multi-threading support, enabling 
parallel execution of independent systems.

## Success Criteria
- [ ] Systems can be registered with dependencies
- [ ] Scheduler executes systems in correct order
- [ ] 2+ independent systems run in parallel
- [ ] Zero race conditions in test suite
- [ ] Documentation complete for system registration

## Committed Work

### High Priority (Must Complete)
1. System registration API (5 pts, 8h) - Alice
2. Dependency graph builder (8 pts, 12h) - Bob
3. Topological sort for execution order (5 pts, 8h) - Carol
4. Basic parallel execution (8 pts, 12h) - David

Subtotal: 26 pts, 40 hours

### Medium Priority (Should Complete)
5. Thread pool implementation (5 pts, 8h) - Alice
6. Race condition tests (3 pts, 4h) - Bob
7. Performance profiling (3 pts, 4h) - Carol

Subtotal: 11 pts, 16 hours

### Stretch Goals (Nice to Have)
8. Work stealing scheduler (13 pts, 20h)
9. CPU affinity optimization (8 pts, 12h)

Total Committed: 37 pts, 56 hours (leaves 8h buffer)

## Dependencies
- Query system must be complete (Sprint 23 carryover)
- Native thread library P/Invoke wrapper needed by Day 3

## Risks
1. Thread safety complexity may reveal unexpected issues
2. Performance targets may require optimization iteration
3. Dependency graph edge cases may emerge during testing

## Out of Scope
- GPU-accelerated systems (deferred to Sprint 26)
- Automatic parallelization (deferred to Sprint 27)
- Cross-system communication (addressed in Sprint 25)
```

## Task Management Best Practices

### Task Writing Guidelines

**Title Format**: `[Action Verb] [Component]: [Specific outcome]`

Examples:
- ✅ "Implement ComponentArray: Add dynamic resizing"
- ✅ "Fix MovementSystem: Prevent negative velocity"
- ✅ "Optimize EntityQuery: Cache compiled queries"
- ❌ "Components" (too vague)
- ❌ "Do movement stuff" (unclear action)

**Description Template**:

```markdown
**Context**: [Why this task exists, what problem it solves]

**Approach**: [High-level approach or design decisions]

**Implementation Details**:
- [Specific step 1]
- [Specific step 2]
- [Specific step 3]

**Acceptance Criteria**:
- [ ] Criterion 1 (specific and testable)
- [ ] Criterion 2 (specific and testable)
- [ ] Criterion 3 (specific and testable)

**Testing Requirements**:
- [ ] Unit tests for [specific scenario]
- [ ] Integration tests for [workflow]
- [ ] Performance tests showing [metric]

**Documentation**:
- [ ] XML comments for public API
- [ ] Update architecture docs if needed
- [ ] Add usage examples

**Definition of Done**:
- [ ] Code complete and reviewed
- [ ] Tests passing and coverage adequate
- [ ] Documentation updated
- [ ] No new warnings or errors
- [ ] Deployed to staging/tested
```

### Dependency Management

**Identify Dependencies**:
```
Task: Implement MovementSystem
Dependencies:
- Blocks: PlayerController (needs movement to function)
- Blocked by: TransformComponent (must exist first)
- Related: PhysicsSystem (integrates with movement)
```

**Dependency Visualization**:
```
   ┌─────────────────┐
   │ Transform Comp  │
   └────────┬────────┘
            │
            ▼
   ┌─────────────────┐      ┌──────────────┐
   │ Movement System │─────▶│ Physics Sys  │
   └────────┬────────┘      └──────────────┘
            │
            ▼
   ┌─────────────────┐
   │ Player Control  │
   └─────────────────┘
```

**Managing Blockers**:

1. **Identify blocker type**:
   - Technical: Missing library, broken build, unclear requirements
   - Resource: Waiting for teammate, external dependency
   - Decision: Requires architectural or product decision

2. **Document blocker**:
   - What is blocking the task?
   - Who can unblock it?
   - When is it expected to be resolved?
   - What's the workaround (if any)?

3. **Escalate appropriately**:
   - Day 1: Flag in standup, attempt self-resolution
   - Day 2: Direct communication with blocker owner
   - Day 3: Escalate to lead/manager for priority adjustment

### Work In Progress (WIP) Limits

**Recommended WIP Limits**:
- Individual developer: 2 tasks max (1 primary, 1 paused/blocked)
- Team: Members × 2 (prevents context switching)
- Sprint: Don't start new work if <50% complete

**Why WIP Limits Matter**:
- ✅ Reduces context switching overhead
- ✅ Encourages completing work before starting new
- ✅ Makes blockers immediately visible
- ✅ Improves flow and throughput
- ✅ Reveals capacity issues early

**When to Break WIP Limits**:
- Blocked on external dependency (can't make progress)
- Waiting for code review (start next while waiting)
- Production emergency (pause current work)

## Common Scenarios

### Scenario 1: Feature Breakdown

**User Request**: "We need a particle system for visual effects"

**Analysis**:
```
Epic: Particle System
Estimated Total: 60-80 hours (3-4 sprints for team of 2)

Complexity Factors:
- Requires native rendering library integration (C++)
- Needs custom memory allocator for pooling
- Performance critical (60 FPS with 10,000 particles)
- Complex math (physics simulation)
```

**Task Breakdown**:
```
Sprint 1: Foundation (20 hours)
├─ Task 1: Design particle component data structure (4h)
│  - Position, velocity, lifetime, color fields
│  - Ensure cache-friendly memory layout
│  - Unit tests for data integrity
│
├─ Task 2: Implement particle memory pool (8h)
│  - Fixed-size block allocator
│  - O(1) allocation/deallocation
│  - Prevent fragmentation
│  - Memory leak tests
│
└─ Task 3: Create basic particle emitter (8h)
   - Spawn rate control
   - Initial velocity/position randomization
   - Integration tests

Sprint 2: Simulation (24 hours)
├─ Task 4: Implement particle update system (8h)
│  - Physics integration (gravity, drag)
│  - Lifetime management
│  - SIMD optimization
│
├─ Task 5: Add native renderer P/Invoke (12h)
│  - Wrapper for native particle rendering
│  - Marshaling for bulk particle data
│  - Error handling and validation tests
│
└─ Task 6: Performance optimization (4h)
   - Profile particle update loop
   - Batch rendering calls
   - Achieve 60 FPS target

Sprint 3: Features (16 hours)
├─ Task 7: Add particle affectors (6h)
│  - Color over lifetime
│  - Size over lifetime
│  - Force fields
│
├─ Task 8: Create particle effect presets (4h)
│  - Fire, smoke, sparks, explosions
│  - Serialization format
│
└─ Task 9: Documentation and examples (6h)
   - API documentation
   - Usage examples
   - Performance guidelines

Total: 60 hours across 3 sprints
```

### Scenario 2: Bug Triage

**Bug Report**: "Game freezes after running for 5 minutes"

**Triage Process**:

```
STEP 1: GATHER INFORMATION
Reporter: QA tester
Frequency: Always (100% reproduction)
Environment: Release build, Windows 10
Reproduction:
1. Launch game
2. Play normally for ~5 minutes
3. Game becomes unresponsive

STEP 2: INITIAL INVESTIGATION (15 minutes)
- Attach debugger: Deadlock detected
- Thread analysis: Two threads waiting on same mutex
- Call stacks point to EntityManager and RenderSystem

STEP 3: SEVERITY ASSESSMENT
Category: Deadlock (Critical)
Impact: Complete game freeze, requires force close
User Impact: 100% of users after 5 minutes
Workaround: Restart game every 4 minutes (not viable)

STEP 4: PRIORITY DETERMINATION
Severity: Critical
Priority: P0 (Fix immediately)
Rationale: 
- Affects all users
- No viable workaround
- Blocks all testing
- Impacts launch deadline

STEP 5: ASSIGNMENT
Assigned to: Senior developer with threading expertise
Timeline: Immediate (stop other work)
Support: Junior dev to assist with testing

STEP 6: TRACKING
Status: In Progress
ETA: 4 hours (analysis + fix + testing)
Updates: Every hour

RESOLUTION:
Root Cause: Lock order inversion between EntityManager and RenderSystem
Fix: Standardize lock ordering, add lock hierarchy validation
Testing: 24-hour stress test with ThreadSanitizer
Prevention: Add locking order documentation, review all lock acquisitions
```

### Scenario 3: Priority Conflict Resolution

**Situation**: Two high-priority items, limited capacity

```
Item A: Implement multiplayer networking
- Business Value: High (enables online play)
- Complexity: High (8 weeks)
- Dependencies: Server infrastructure (in progress)
- Risk: High (new technology, external dependencies)

Item B: Optimize frame rate for low-end devices
- Business Value: High (40% of users on low-end devices)
- Complexity: Medium (3 weeks)
- Dependencies: Profiling tools (available)
- Risk: Medium (known problem, proven solutions)

Capacity: 1 senior developer available

Decision Framework:
1. Can we do both? No (11 weeks total, 8 weeks until launch)
2. Which is blocking? Networking blocks online features
3. Which has more risk? Networking has more unknowns
4. Which has faster ROI? Performance helps users immediately
5. Which is reversible? Performance optimization is reversible

Recommendation:
Priority Order:
1. Performance Optimization (B) - 3 weeks
   Rationale: Lower risk, faster delivery, immediate user benefit
   
2. Networking (A) - Start in week 4
   Rationale: Allows parallel server infrastructure completion
   Risk mitigation: 5 weeks for networking vs 8 estimated (buffer)

Alternative: Split work if second developer becomes available
- Dev 1: Performance (3 weeks)
- Dev 2: Networking (8 weeks in parallel)
```

## Communication Templates

### Status Update Email

```markdown
Subject: [Project] Sprint 24 Status - Jan 29

Hi team,

Quick update on Sprint 24 progress:

✅ COMPLETED
- System registration API (Alice)
- Dependency graph builder (Bob)

🔄 IN PROGRESS (On Track)
- Topological sort for execution order (Carol) - 80% complete
- Basic parallel execution (David) - 60% complete

⚠️ AT RISK
- Thread pool implementation delayed by 1 day
  Reason: Unexpected complexity in work stealing algorithm
  Mitigation: Simplify approach, focus on basic pooling first
  New ETA: Feb 2 (was Feb 1)

🚫 BLOCKED
- None

📊 METRICS
- Velocity: 15/20 points complete (75%)
- Burndown: Slightly behind (1 day)
- Bugs: 0 new, 2 resolved

🎯 NEXT 3 DAYS
- Complete topological sort (Carol)
- Finish basic parallel execution (David)
- Start thread pool (Alice)

Let me know if you have questions!
```

### Task Assignment

```markdown
Subject: New Task: Implement Query Caching

Hi Alice,

I'm assigning you a new task based on our Sprint 24 priorities:

**Task**: Implement Query Caching in EntityQuery
**Priority**: High (milestone critical)
**Estimate**: 8 hours
**Due**: Feb 3

**Context**:
Query compilation is happening every frame, causing 15% CPU overhead.
We need to cache compiled queries and reuse them.

**Acceptance Criteria**:
- [ ] Queries cached by component signature
- [ ] Cache invalidation on component add/remove
- [ ] Performance test shows <1% query overhead
- [ ] Thread-safe cache access

**Resources**:
- Design doc: docs/query-caching.md
- Reference impl: EntityFramework query cache
- Profiler data: profiling/query-overhead.trace

**Dependencies**:
- Requires: Query builder API (complete)
- Blocks: Performance benchmarks (high priority)

Let me know if you need clarification!
```

## Decision-Making Framework

### When to Say No

**Decline requests that**:
- Significantly derail current sprint goals
- Lack clear acceptance criteria or value justification
- Are better handled after current milestone
- Duplicate existing work
- Violate architectural principles
- Can't be completed with available resources

**How to decline**:
```markdown
"I understand [request] is important. However, accepting it now would:
1. Risk our Sprint 24 goal of [goal]
2. Delay [milestone] by [timeframe]
3. Reduce our velocity by [percentage]

Alternative approach:
- Add to backlog for Sprint 25 (Feb 12)
- Reassess priority in sprint planning
- Consider if smaller MVP version achieves same goal

Would you like to discuss this in today's standup?"
```

### When to Escalate

**Escalate decisions involving**:
- Significant architectural changes
- Timeline impacts >1 sprint
- Resource reallocation
- Scope changes to committed work
- Technical risks without clear mitigation
- Cross-team dependencies
- Conflicting stakeholder priorities

**Escalation Template**:
```markdown
Subject: Decision Needed: [Topic]

**Situation**:
[Clear description of the issue requiring decision]

**Options**:
1. Option A: [Description]
   Pros: [Benefits]
   Cons: [Drawbacks]
   Impact: [Timeline, resources, risk]

2. Option B: [Description]
   Pros: [Benefits]
   Cons: [Drawbacks]
   Impact: [Timeline, resources, risk]

**Recommendation**: Option [A/B]
**Rationale**: [Why this option is best]

**Urgency**: [Immediate / This Week / Next Sprint]
**Decision Needed By**: [Date]

**Impact of No Decision**:
[What happens if we don't decide]
```

## Quality Metrics

### Healthy Project Indicators

✅ **Good Signs**:
- Velocity stable within 10% across sprints
- <20% of work blocked at any time
- >80% of sprint commitments met
- Bug escape rate <10% (caught before QA)
- <5% emergency/unplanned work per sprint
- Test coverage >80% for new code
- Technical debt <20% of total work

⚠️ **Warning Signs**:
- Velocity decreasing sprint over sprint
- >30% of work blocked
- <60% of sprint commitments met
- Bug escape rate >20%
- >20% emergency/unplanned work
- Test coverage <60%
- Technical debt >40% of total work

🚨 **Critical Issues**:
- Velocity dropped >50% from baseline
- >50% of work blocked
- <40% of sprint commitments met
- Critical bugs found in production
- >40% emergency/unplanned work
- Test coverage <40%
- Technical debt >60% of total work

### Continuous Improvement

**Sprint Retrospective Questions**:

1. **What went well?**
   - Celebrate successes
   - Identify practices to continue

2. **What went poorly?**
   - Identify problems without blame
   - Focus on process, not people

3. **What should we change?**
   - Specific, actionable improvements
   - One or two changes per sprint (don't overwhelm)

4. **What did we learn?**
   - Technical insights
   - Process lessons
   - Team dynamics observations

**Action Items from Retro**:
- Must be specific and measurable
- Assign owner to each action
- Review in next retrospective
- Limit to 1-3 actions (focus)

## Final Reminders

### Core Principles

1. **Transparency**: Always provide honest status, even if it's bad news
2. **Alignment**: Every task should trace to a strategic goal
3. **Balance**: Mix quick wins with long-term investments
4. **Quality**: Never sacrifice quality for speed (it costs more later)
5. **People**: Optimize for human wellbeing, not just output
6. **Iteration**: Perfect is the enemy of done; ship and improve
7. **Communication**: Over-communicate status, especially problems

### You Are Successful When

- ✅ Team always knows what to work on next
- ✅ Priorities are clear and aligned with strategy
- ✅ Blockers are identified and resolved quickly
- ✅ Progress is visible and measurable
- ✅ Surprises are rare (risks identified early)
- ✅ Team velocity is predictable and sustainable
- ✅ Stakeholders are informed and confident

Your role is to be the central nervous system of the development workflow, ensuring every task serves the larger plan and the team always moves forward efficiently toward their goals.
