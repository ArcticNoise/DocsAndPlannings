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

### 1.2 Database Schema Design
- [ ] Design user authentication tables
- [ ] Design documentation tables
- [ ] Design planning/tracking tables (Projects, Epics, Tasks, Bugs, Subtasks)
- [ ] Design relationships and constraints
- [ ] Create migration scripts

### 1.3 Authentication & Authorization
- [ ] Implement user registration
- [ ] Implement login/logout
- [ ] Implement JWT token authentication
- [ ] Create user management API endpoints
- [ ] Add role-based access control (RBAC)
- [ ] Unit tests for authentication

---

## Phase 2: Documentation Module

### 2.1 Core Documentation Features
- [ ] Create document model (with markdown support)
- [ ] Implement CRUD operations for documents
- [ ] REST API endpoints for documents
- [ ] Document versioning/history
- [ ] Unit tests for document operations

### 2.2 Advanced Documentation Features
- [ ] Screenshot upload and storage
- [ ] Image embedding in markdown
- [ ] Document search functionality
- [ ] Document categorization/tagging
- [ ] Access control for documents
- [ ] Unit tests for advanced features

---

## Phase 3: Planning/Tracking Module

### 3.1 Core Planning Features
- [ ] Project model and CRUD operations
- [ ] Epic model and CRUD operations
- [ ] Task/Bug model and CRUD operations
- [ ] Subtask model and CRUD operations
- [ ] Implement hierarchy (Project → Epic → Task/Bug → Subtask)
- [ ] Unit tests for planning models

### 3.2 Status Management
- [ ] Implement basic statuses (TODO, IN PROGRESS, DONE, CANCELLED, BACKLOG)
- [ ] Custom status configuration per item type
- [ ] Status transition validation
- [ ] Status history tracking
- [ ] Unit tests for status management

### 3.3 Item Management
- [ ] Unique ID generation (project-based, e.g., PROJ-123)
- [ ] Summary/title field
- [ ] Description field (markdown support)
- [ ] Assignee functionality
- [ ] Priority field
- [ ] Due date field
- [ ] Comments/activity log
- [ ] Unit tests for item management

### 3.4 REST API Endpoints
- [ ] Projects API (CRUD)
- [ ] Epics API (CRUD)
- [ ] Tasks/Bugs API (CRUD)
- [ ] Subtasks API (CRUD)
- [ ] Status API (list, create custom)
- [ ] Assignment API
- [ ] Search/filter API

---

## Phase 4: Kanban Board

### 4.1 Kanban Features
- [ ] Board model per project
- [ ] Column configuration based on statuses
- [ ] API for board data retrieval
- [ ] Drag-and-drop status update endpoint
- [ ] Board filtering/grouping
- [ ] Unit tests for kanban operations

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
**Phase**: Phase 1 - Foundation & Infrastructure
**Current Task**: 1.2 Database Schema Design
**Last Updated**: 2025-10-30

**Recent Completion**: Phase 1.1 Project Setup ✅
- Solution structure created with source/ and tests/ directories
- 3 projects: Core (library), Api (REST API), Web (MVC)
- 3 test projects with xUnit and EF Core InMemory
- SQLite + Entity Framework Core configured
- Dependency Injection properly configured
- All projects use nullable, implicit usings, TreatWarningsAsErrors
- Comprehensive .editorconfig created
- User model and ApplicationDbContext implemented
- 8 comprehensive tests (7 passing, 1 skipped)
- Bug hunting completed with all issues resolved
- Build: 0 warnings, 0 errors
