---
name: git-workflow-SKILL
description: Complete Git workflow including branch naming, commit message standards, merge procedures, and repository state management. Use for branch creation, commits, pull requests, and maintaining clean version control following project conventions.
---

# Git Workflow Skill

## Overview
This skill defines the complete Git workflow including branch naming conventions, commit message standards, merge procedures, and repository state management. Follow these practices for clean, traceable version control.

## Branch Management

### Branch Naming Conventions

**Standard format:**
```
{type}/{issue-id}-{brief-description}
```

**Branch types:**
- `feature/` - New features or enhancements
- `bugfix/` - Bug fixes
- `hotfix/` - Critical production fixes
- `refactor/` - Code refactoring
- `docs/` - Documentation updates
- `test/` - Test additions or fixes

**Naming rules:**
- Include issue ID from task tracker (e.g., Linear, Jira)
- Use kebab-case for description
- Keep description brief but meaningful (3-5 words)
- Lowercase only
- No special characters except hyphens

**Examples:**
```bash
✅ GOOD
feature/AUTH-123-implement-jwt-service
bugfix/API-456-fix-login-timeout
hotfix/SEC-789-patch-vulnerability
refactor/DB-234-optimize-queries
docs/DOC-567-update-api-guide
test/TEST-890-add-integration-tests

❌ BAD
feature/jwt_service              # Missing issue ID
Feature/AUTH-123-JWT             # Uppercase
feature/AUTH-123-implement-the-new-jwt-authentication-service  # Too long
feature/auth123-jwt              # No separator between ID and description
```

### Creating Branches

**Always branch from the latest main/master:**
```bash
# Update main branch
git checkout master
git pull origin master

# Create and checkout new branch
git checkout -b feature/AUTH-123-implement-jwt-service
```

**Verify branch state:**
```bash
# Check current branch
git branch

# Verify branch is clean
git status
# Should show: "nothing to commit, working tree clean"
```

### Branch Lifecycle

1. **Create branch** from main/master
2. **Work on branch** - all commits go here
3. **Keep branch updated** - periodically merge/rebase from main
4. **Complete work** - ensure all commits are made
5. **Merge to main** - integrate changes
6. **Delete branch** (optional) - cleanup after merge

## Commit Management

### Commit Message Format

**Standard structure:**
```
{type}({scope}): {description}

{body}

{footer}
```

**Components:**

**Type** (required) - What kind of change:
- `feat` - New feature
- `fix` - Bug fix
- `refactor` - Code refactoring (no functionality change)
- `test` - Adding or updating tests
- `docs` - Documentation changes
- `style` - Formatting, missing semicolons, etc. (no code change)
- `chore` - Maintenance tasks, dependency updates
- `perf` - Performance improvements
- `build` - Build system changes
- `ci` - CI/CD configuration changes

**Scope** (optional) - Area affected:
- Module name, component, or subsystem
- Examples: `auth`, `api`, `database`, `ui`, `tests`

**Description** (required):
- Brief summary of the change
- Use imperative mood: "add" not "added" or "adds"
- Lowercase first letter
- No period at the end
- Maximum 72 characters

**Body** (optional):
- Detailed explanation of changes
- Use bullet points for multiple changes
- Explain "why" not just "what"
- Wrap at 72 characters

**Footer** (optional):
- Reference issue tracker IDs
- Breaking changes
- Use keywords: `Closes`, `Fixes`, `Resolves`, `Refs`

### Commit Message Examples

**Simple commit:**
```
feat(auth): add JWT token generation
```

**With body:**
```
feat(auth): add JWT token generation

- Implement token signing with RS256
- Add token expiration handling
- Include user claims in payload
```

**With issue reference:**
```
fix(api): resolve timeout on large file uploads

Increased buffer size and added chunked upload support
to handle files larger than 100MB.

Fixes AUTH-456
```

**Multiple issues:**
```
refactor(database): optimize query performance

- Add indexes on frequently queried columns
- Rewrite N+1 queries to use joins
- Cache repeated lookups

Closes DB-234, DB-235
```

**Breaking change:**
```
feat(api): update authentication endpoint structure

BREAKING CHANGE: The /auth/login endpoint now requires
email instead of username. Update all client applications.

Closes AUTH-789
```

### Commit Best Practices

**1. Atomic commits** - One logical change per commit:
```bash
✅ GOOD - Separate commits
git commit -m "feat(auth): add user registration"
git commit -m "test(auth): add registration tests"
git commit -m "docs(auth): update API documentation"

❌ BAD - Everything in one commit
git commit -m "add registration, tests, and docs"
```

**2. Commit frequently:**
- Commit after completing each logical unit of work
- Don't wait until end of day
- Makes code review easier
- Easier to revert if needed

**3. Review before committing:**
```bash
# Review what will be committed
git diff --staged

# Or use interactive staging
git add -p
```

**4. Write clear messages:**
- Future you will thank present you
- Helps team understand changes
- Makes git log useful for tracking

## Staging and Committing

### Selective Staging

**Stage specific files:**
```bash
# Stage single file
git add src/Authentication.cpp

# Stage multiple files
git add src/Authentication.cpp src/Authentication.h

# Stage all files in directory
git add src/auth/

# Stage all modified files
git add -u

# Stage everything (use with caution)
git add .
```

**Interactive staging:**
```bash
# Choose which changes to stage
git add -p

# Options:
# y - stage this hunk
# n - don't stage this hunk
# s - split into smaller hunks
# e - manually edit the hunk
# q - quit
```

### Unstaging Changes

```bash
# Unstage specific file
git reset HEAD src/Authentication.cpp

# Unstage all changes
git reset HEAD
```

### Amending Commits

**Fix the last commit:**
```bash
# Amend message only
git commit --amend

# Add forgotten changes to last commit
git add forgotten_file.cpp
git commit --amend --no-edit

# ⚠️ WARNING: Only amend commits that haven't been pushed!
```

## Merging Strategy

### Pre-Merge Checklist

Before merging to main/master, verify:
- [ ] All changes committed to feature branch
- [ ] All tests passing
- [ ] Code review completed and approved
- [ ] No uncommitted changes (`git status` clean)
- [ ] Branch is up-to-date with main/master

### Standard Merge Process

**Step 1: Update main branch**
```bash
git checkout master
git pull origin master
```

**Step 2: Merge feature branch**
```bash
git merge feature/AUTH-123-implement-jwt-service
```

**Step 3: Handle merge conflicts (if any)**
```bash
# Conflicts will be marked in files
# Edit files to resolve conflicts
# Look for conflict markers:
# <<<<<<< HEAD
# ======= 
# >>>>>>> feature/AUTH-123

# After resolving conflicts
git add resolved_file.cpp
git commit
# Git will create a merge commit
```

**Step 4: Verify merge**
```bash
# Run tests
make test  # or your test command

# Verify build
make       # or your build command

# Check final state
git log --oneline -5
```

**Step 5: Push to remote**
```bash
git push origin master
```

### Alternative: Rebase Workflow

**When to use rebase:**
- Keep linear history
- Clean up commits before merging
- Integrate upstream changes

```bash
# Update feature branch with latest main
git checkout feature/AUTH-123-implement-jwt-service
git rebase master

# If conflicts occur
# Resolve conflicts in each file
git add resolved_file.cpp
git rebase --continue

# If rebase gets too complicated
git rebase --abort  # Start over

# After successful rebase
git checkout master
git merge feature/AUTH-123-implement-jwt-service  # Fast-forward merge
git push origin master
```

**⚠️ Rebase Warning:**
- Never rebase commits that have been pushed to shared branches
- Only rebase local feature branches
- Rebase rewrites history

### Pull Request / Merge Request Workflow

**If using platform-based merges (GitHub, GitLab, etc.):**

**Step 1: Push feature branch**
```bash
git push origin feature/AUTH-123-implement-jwt-service
```

**Step 2: Create PR/MR on platform**
- Go to repository on GitHub/GitLab/Bitbucket
- Create Pull Request / Merge Request
- Fill in description (reference issue)
- Request reviewers

**Step 3: Wait for CI/CD and reviews**
- Automated tests run
- Code review by team members
- Address feedback in new commits
- Push updates to same branch

**Step 4: Merge via platform**
- Click "Merge" button when approved
- Choose merge strategy (merge commit, squash, rebase)
- Delete branch after merge (optional)

**Step 5: Update local repository**
```bash
git checkout master
git pull origin master
```

## Repository State Management

### Clean Working Directory

**Goal: Always maintain clean repository state**

**Check status:**
```bash
git status

# Ideal output:
# On branch feature/AUTH-123-implement-jwt-service
# nothing to commit, working tree clean
```

### Handling Unwanted Files

**Add to .gitignore:**
```bash
# Common items to ignore:

# Build artifacts
/bin/
/obj/
/build/
/out/

# IDE files
.vs/
.vscode/
.idea/
*.suo
*.user

# Temporary files
*.tmp
*.log
*.swp
*~

# Dependencies
node_modules/
packages/
vendor/

# OS files
.DS_Store
Thumbs.db
```

**Update .gitignore:**
```bash
# Add patterns to .gitignore
echo "*.log" >> .gitignore

# Commit .gitignore changes
git add .gitignore
git commit -m "chore: update gitignore for log files"

# Verify files are now ignored
git status  # Should not show ignored files
```

**Remove already-tracked files:**
```bash
# If file was committed before adding to .gitignore
git rm --cached unwanted_file.log
git commit -m "chore: remove log file from tracking"
```

### Stashing Changes

**Save work in progress:**
```bash
# Stash current changes
git stash

# Stash with message
git stash save "WIP: implementing validation"

# List stashes
git stash list

# Apply most recent stash
git stash pop

# Apply specific stash
git stash apply stash@{1}

# Delete stash
git stash drop stash@{0}
```

## Verification Commands

### Pre-Commit Verification

```bash
# Check what will be committed
git diff --cached

# Check status
git status

# Review recent commits
git log --oneline -5

# Check which branch you're on
git branch
```

### Pre-Merge Verification

```bash
# Check for uncommitted changes
git status

# View all changes from master
git diff master...HEAD

# Check commits that will be merged
git log master..HEAD

# Verify branch is up-to-date
git fetch origin
git status  # Should show "up-to-date"
```

### Post-Merge Verification

```bash
# Verify merge was successful
git log --oneline --graph -10

# Check current branch
git branch

# Verify no uncommitted changes
git status

# Verify tests still pass
make test  # or your test command
```

## Common Git Workflows

### Starting New Work

```bash
# 1. Update main
git checkout master
git pull origin master

# 2. Create feature branch
git checkout -b feature/AUTH-123-new-feature

# 3. Verify clean state
git status

# 4. Start working...
```

### Completing Work

```bash
# 1. Verify all changes committed
git status  # Should be clean

# 2. Update from main
git fetch origin
git merge origin/master  # or rebase

# 3. Resolve any conflicts

# 4. Final verification
make test

# 5. Merge to main
git checkout master
git merge feature/AUTH-123-new-feature

# 6. Push
git push origin master
```

### Fixing Mistakes

**Uncommit last commit (keep changes):**
```bash
git reset --soft HEAD~1
```

**Uncommit and discard changes:**
```bash
git reset --hard HEAD~1
# ⚠️ WARNING: This deletes changes permanently!
```

**Undo changes in working directory:**
```bash
# Single file
git checkout -- src/file.cpp

# All files
git checkout .
```

**Create fixup commit:**
```bash
# Make fixes
git add fixed_files.cpp
git commit --fixup HEAD
```

## Best Practices Summary

### Branch Management
- ✅ Always branch from latest main/master
- ✅ Use descriptive branch names with issue IDs
- ✅ Keep branches short-lived
- ✅ Delete branches after merging

### Commits
- ✅ Make atomic commits (one logical change)
- ✅ Write clear, descriptive commit messages
- ✅ Use conventional commit format
- ✅ Commit frequently
- ✅ Reference issue tracker IDs

### Merging
- ✅ Update main before merging
- ✅ Verify tests pass before merging
- ✅ Resolve conflicts carefully
- ✅ Verify merge success
- ✅ Push to remote after merge

### Repository State
- ✅ Keep working directory clean
- ✅ Update .gitignore for unwanted files
- ✅ Verify no uncommitted changes before merge
- ✅ Use `git status` frequently

### Common Pitfalls to Avoid
- ❌ Committing to main/master directly
- ❌ Large commits with unrelated changes
- ❌ Vague commit messages
- ❌ Merging with uncommitted changes
- ❌ Forgetting to reference issues
- ❌ Not testing before merge
- ❌ Ignoring merge conflicts
- ❌ Committing generated/build files

## Quick Reference

### Branch Naming
```
feature/{issue-id}-{description}
bugfix/{issue-id}-{description}
hotfix/{issue-id}-{description}
```

### Commit Message
```
type(scope): description

Detailed changes

Closes ISSUE-123
```

### Essential Commands
```bash
git checkout -b feature/ISSUE-123-name  # Create branch
git add file.cpp                         # Stage file
git commit -m "type: message"            # Commit
git checkout master                      # Switch to main
git pull origin master                   # Update main
git merge feature/ISSUE-123-name        # Merge
git push origin master                   # Push changes
git status                               # Check state
```

## Integration with Task Management

This Git workflow integrates with task management systems by:
- Including issue IDs in branch names
- Referencing issues in commit messages
- Using keywords (Closes, Fixes) to auto-update issues
- Maintaining traceability from code to requirements

For complete task management workflow, see **task-management-linear-SKILL.md**.
