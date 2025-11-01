# Docs and Plannings - Development Roadmap

## Project Overview
High-performance ASP.NET web application combining documentation management (Confluence-like) and project tracking (Jira-like).

## Technology Stack
- **Backend**: ASP.NET Core C# Web API
- **Frontend**: ASP.NET MVC with Razor Views
- **Database**: SQLite with Entity Framework Core
- **Architecture**: Dependency Injection (DI) and Inversion of Control (IoC)
- **API**: REST API

---

## Phase 1: Foundation & Infrastructure

### 1.1 Project Setup ✅ COMPLETED (2025-10-30)
- [x] Create solution structure (source/, tests/)
- [x] Set up ASP.NET Core Web API project
- [x] Set up ASP.NET MVC project
- [x] Configure SQLite database
- [x] Set up Entity Framework Core
- [x] Configure Dependency Injection (DI) container
- [x] Set up service registrations (repositories, services, etc.)
- [x] Implement IoC patterns for loose coupling
- [x] Configure project settings (nullable, implicit usings, warnings as errors)
- [x] Create .editorconfig
- [x] Set up unit test projects with DI support

### 1.2 Database Schema Design ✅ COMPLETED (2025-10-30)
- [x] Design user authentication tables
- [x] Design documentation tables
- [x] Design planning/tracking tables (Projects, Epics, Tasks, Bugs, Subtasks)
- [x] Design relationships and constraints
- [x] Create migration scripts

### 1.3 Authentication & Authorization ✅ COMPLETED (2025-10-30)
- [x] Implement user registration
- [x] Implement login/logout
- [x] Implement JWT token authentication
- [x] Create user management API endpoints
- [x] Add role-based access control (RBAC)
- [x] Unit tests for authentication

---

## Phase 2: Documentation Module

### 2.1 Core Documentation Features ✅ COMPLETED (2025-10-31)
- [x] Create document model (with markdown support)
- [x] Implement CRUD operations for documents
- [x] REST API endpoints for documents
- [x] Document versioning/history
- [x] Document hierarchy with circular reference prevention
- [x] Tag management system (admin-only)
- [x] Unit tests for document operations ✅ COMPLETED (2025-10-31)

### 2.2 Advanced Documentation Features ✅ COMPLETED (2025-10-31)
- [x] Screenshot upload and storage (filesystem-based)
- [x] Image embedding in markdown
- [x] Document search functionality
- [x] Document categorization/tagging
- [x] Access control for documents (author + published model)
- [x] Unit tests for advanced features ✅ COMPLETED (2025-10-31)

---

## Phase 3: Planning/Tracking Module ✅ COMPLETED (2025-10-31)

### 3.1 Core Planning Features ✅ COMPLETED (2025-10-31)
- [x] Project model and CRUD operations
- [x] Epic model and CRUD operations
- [x] Task/Bug model and CRUD operations
- [x] Subtask model and CRUD operations
- [x] Implement hierarchy (Project → Epic → Task/Bug → Subtask)
- [x] Unit tests for planning models ✅ COMPLETED (2025-11-01)

### 3.2 Status Management ✅ COMPLETED (2025-10-31)
- [x] Implement basic statuses (TODO, IN PROGRESS, DONE, CANCELLED, BACKLOG)
- [x] Custom status configuration per item type
- [x] Status transition validation
- [ ] Status history tracking (deferred to Phase 6)
- [x] Unit tests for status management ✅ COMPLETED (2025-11-01)

### 3.3 Item Management ✅ COMPLETED (2025-10-31)
- [x] Unique ID generation (project-based, e.g., PROJ-123)
- [x] Summary/title field
- [x] Description field (markdown support)
- [x] Assignee functionality
- [x] Priority field
- [x] Due date field
- [x] Comments/activity log
- [x] Unit tests for item management ✅ COMPLETED (2025-11-01)

### 3.4 REST API Endpoints ✅ COMPLETED (2025-10-31)
- [x] Projects API (CRUD)
- [x] Epics API (CRUD)
- [x] Tasks/Bugs API (CRUD)
- [x] Subtasks API (CRUD)
- [x] Status API (list, create custom)
- [x] Assignment API
- [x] Search/filter API

---

## Phase 4: Kanban Board ✅ COMPLETED (2025-11-01)

### 4.1 Kanban Features ✅ COMPLETED (2025-11-01)
- [x] Board model per project
- [x] Column configuration based on statuses
- [x] API for board data retrieval
- [x] Drag-and-drop status update endpoint
- [x] Board filtering/grouping
- [x] Unit tests for kanban operations
- [x] Integration tests for end-to-end workflows

**Phase 4 Deliverables**:
- 2 Model classes (Board, BoardColumn)
- 10 DTOs (board operations and views)
- 1 Service interface (IBoardService)
- 1 Service implementation (BoardService)
- 1 Controller (BoardsController with 8 REST endpoints)
- 1 Database migration
- **125 tests total** (15 model tests + 72 service tests + 28 controller tests + 5 integration tests + 13 bug-hunting tests)
- Build: 0 warnings, 0 errors
- Test Results: 325 total tests (324 passing, 1 skipped)

**API Endpoints** (8 total):
1. `POST /api/projects/{projectId}/board` - Create board
2. `GET /api/projects/{projectId}/board` - Get board
3. `PUT /api/projects/{projectId}/board` - Update board
4. `DELETE /api/projects/{projectId}/board` - Delete board
5. `GET /api/projects/{projectId}/board/view` - Get board view with work items
6. `PUT /api/projects/{projectId}/board/columns/{columnId}` - Update column configuration
7. `PUT /api/projects/{projectId}/board/columns/reorder` - Reorder columns
8. `PUT /api/projects/{projectId}/board/workitems/{workItemId}/move` - Move work item between statuses

**Features Implemented**:
- Project-specific board configurations
- Dynamic columns based on project statuses
- Board state retrieval with filtering (epic, assignee, search text)
- Work item movement with status transition validation
- Column customization (WIP limits, collapsed state, order)
- Optimistic concurrency control with RowVersion
- Comprehensive authorization (project owner only)
- Cascade delete (board deletion removes all columns)
- Integration with existing status management system

---

## Phase 5: Frontend Development (ASP.NET MVC)

### 5.1 Core UI & Layout
- [ ] Create shared layout (_Layout.cshtml)
- [ ] Navigation partial views
- [ ] Main dashboard controller and views
- [ ] Login/registration controllers and Razor views
- [ ] Responsive CSS/Bootstrap integration
- [ ] Client-side validation scripts
- [ ] Error handling views (404, 500, etc.)

### 5.2 Documentation UI (MVC)
- [ ] DocumentsController with action methods
- [ ] Document list view (Index.cshtml)
- [ ] Document editor view with markdown support
- [ ] Document viewer view
- [ ] Screenshot upload form and file handling
- [ ] ViewModels for document operations
- [ ] Ajax calls for API integration

### 5.3 Planning UI (MVC)
- [ ] ProjectsController, EpicsController, TasksController
- [ ] Project list view with filtering
- [ ] Epic/Task/Bug list views with search
- [ ] Item detail view (Details.cshtml, linkable via route)
- [ ] Create/Edit forms with model binding
- [ ] Assignee dropdown with user list
- [ ] ViewModels for planning items
- [ ] Partial views for reusable components

### 5.4 Kanban Board UI (MVC)
- [ ] KanbanController with board actions
- [ ] Kanban board view with status columns
- [ ] JavaScript for drag-and-drop functionality
- [ ] Ajax endpoints for status updates
- [ ] Column customization interface
- [ ] Quick edit modal dialogs
- [ ] Real-time board updates

---

## Phase 6: Polish & Optimization

### 6.1 Performance
- [ ] Database query optimization
- [ ] API response caching
- [ ] Lazy loading for large datasets
- [ ] Performance benchmarking

### 6.2 Testing & Quality
- [ ] Integration tests
- [ ] End-to-end tests
- [ ] Code coverage analysis
- [ ] Security audit
- [ ] Load testing

### 6.3 Documentation
- [ ] API documentation (Swagger/OpenAPI)
- [ ] User guide
- [ ] Developer documentation
- [ ] Deployment guide

---

## Future Enhancements (Post-MVP)
- Google OAuth integration
- Email notifications
- Webhooks
- Advanced reporting/analytics
- Export functionality (PDF, CSV)
- Real-time collaboration
- Mobile app
- Integration with external tools

---

## Current Status
**Phase**: Phase 4 - Kanban Board (COMPLETE WITH TESTS ✅)
**Current Task**: Ready for Phase 5 - Frontend Development (ASP.NET MVC)
**Last Updated**: 2025-11-01

**Recent Completion**: Phase 4 Kanban Board ✅ (2025-11-01)
- 125 new tests added for Phase 4 (15 model + 72 service + 28 controller + 5 integration + 13 bug-hunting)
- BoardService: 72 comprehensive tests covering all operations
- BoardsController: 28 tests covering all 8 REST endpoints
- 5 integration tests for end-to-end workflows
- 13 bug-hunting tests for edge cases and concurrent scenarios
- Code review completed with 0 critical issues
- Bug hunting completed - 0 bugs found
- Total test count: 325 tests (324 passing, 1 skipped)
- Build: 0 warnings, 0 errors

**Previous Completion**: Phase 3 Planning/Tracking Module ✅ (2025-10-31)

**Phase 3 - Planning/Tracking Module (33 endpoints total)**:

**Phase 3.1 - Status Management (7 endpoints)**:
- Status model with EF Core configuration
- StatusDto, CreateStatusRequest, UpdateStatusRequest DTOs
- IStatusService and StatusService implementation
  - CRUD operations for statuses
  - Status transition validation with configurable workflow
  - Default status creation (To Do, In Progress, Done, Cancelled, Backlog)
- StatusesController with 7 REST endpoints
  - GetAllStatuses, GetStatusById, CreateStatus, UpdateStatus, DeleteStatus
  - ValidateTransition, CreateDefaultStatuses

**Phase 3.2 - Project Management (7 endpoints)**:
- Project model with key generation
- 5 Project DTOs (create, update, detail, list item)
- IKeyGenerationService and KeyGenerationService implementation
  - Unique key generation (e.g., PROJ, PROJ-EPIC-1, PROJ-123)
- IProjectService and ProjectService implementation
  - Full CRUD operations with ownership validation
  - Project archiving/unarchiving functionality
  - Pagination support
- ProjectsController with 7 REST endpoints

**Phase 3.3 - Epic Management (9 endpoints)**:
- Epic model with work breakdown structure
- 4 Epic DTOs (create, update, detail, list item)
- IEpicService and EpicService implementation
  - Full CRUD with automatic status assignment
  - Epic assignment to users
  - Status transitions with validation
  - Work item count tracking
- EpicsController with 9 REST endpoints

**Phase 3.4 - Work Item Management (10 endpoints)**:
- WorkItem model with hierarchy support (Task, Bug, Subtask)
- WorkItemType enum (Task, Bug, Subtask)
- 5 Work Item DTOs (create, update, search, detail, list item)
- IWorkItemService and WorkItemService implementation
  - Full CRUD with hierarchy validation
  - Hierarchy rules (Task → Subtask only, no circular references)
  - Reporter tracking (who created the item)
  - Advanced search with filters (project, epic, status, assignee, reporter, type, priority, text)
  - Parent-child relationship management
  - Circular reference detection
- WorkItemsController with 10 REST endpoints

**Phase 3.5 - Comments System (5 endpoints)**:
- WorkItemComment model with author tracking
- 3 Comment DTOs (create, update, detail)
- ICommentService and CommentService implementation
  - Comments linked to work items
  - Author-only edit/delete authorization
  - Edit tracking (IsEdited flag)
- CommentsController with 5 REST endpoints

**Summary**:
- 33 API endpoints total (7 status + 7 project + 9 epic + 10 work item + 5 comment)
- 22 DTOs created (5 status + 5 project + 4 epic + 5 work item + 3 comment)
- 5 services implemented (Status, KeyGeneration, Project, Epic, WorkItem, Comment)
- 5 controllers created (Statuses, Projects, Epics, WorkItems, Comments)
- 4 custom exceptions (InvalidStatusTransition, InvalidHierarchy, CircularHierarchy, DuplicateKey)
- Database migration: Phase3PlanningModule
- Build: 0 warnings, 0 errors
- Tests: Deferred to Phase 6 for comprehensive test coverage

**Features Implemented**:
- Complete project-epic-workitem hierarchy with validation
- Automatic sequential key generation (PROJ-123 format)
- Status workflow with transition validation
- Work item types: Task, Bug, Subtask
- Comments with author tracking and edit history
- Reporter and Assignee tracking
- Priority and due date management
- Advanced search and filtering
- Circular reference prevention in hierarchy
- Default status seeding on application startup

**Previous Milestone**: Phase 2 Documentation Module ✅ (2025-10-31)

**Phase 2.1 - Core Documentation (7 endpoints)**:
- 7 Document DTOs (create, update, search, list, version)
- 3 Tag DTOs (create, update, response)
- DocumentService with full CRUD operations
  - Circular hierarchy prevention via graph traversal
  - Automatic versioning on content changes
  - Access control (author-only edit, published documents public)
- TagService with admin-only tag management
- DocumentsController with 7 endpoints
- TagsController with 4 endpoints (admin-only create/update/delete)

**Phase 2.2 - File Upload & Attachments (4 endpoints)**:
- DocumentAttachment model with EF Core configuration
- IFileStorageService and FileStorageService implementation
  - Local filesystem storage in screenshots/ directory
  - File validation (10 MB max, images only: JPEG, PNG, GIF, WebP, BMP)
- Extended DocumentService with 4 attachment methods
  - Automatic file cleanup on document deletion
- 4 file management endpoints in DocumentsController

**Summary**:
- 11 API endpoints total (7 document + 4 file management)
- 10 DTOs created
- 3 services implemented
- 2 controllers created
- Custom exceptions for error handling
- Build: 0 warnings, 0 errors
- Tests: 154 passed, 1 skipped ✅ COMPLETED (2025-10-31)
  - 91 new Phase 2 tests added (29 DocumentService, 13 TagService, 23 FileStorageService, 18 DocumentsController, 11 TagsController)
  - All Phase 2 features fully tested

**Previous Milestone**: Phase 1.3 Authentication & Authorization ✅ (2025-10-30)
- Complete authentication system with registration and login
- JWT token-based authentication with role support
- BCrypt password hashing (work factor 12)
- 6 service classes (PasswordHasher, JwtTokenService, AuthenticationService)
- 4 DTO classes for API contracts
- 2 API controllers (Auth, Users)
- JWT middleware configuration in Program.cs
- Role-based authorization with [Authorize] attributes
- 27 new comprehensive tests (all passing)
- 7 security-focused bug hunting tests
- Total: 64 tests (63 passing, 1 expected skip)
- Security analysis: SQL injection protected, timing attack resistant
