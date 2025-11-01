# Phase 5: Frontend Development (ASP.NET MVC) - Implementation Guide

**Branch**: `feature/phase-5-frontend-mvc`
**Status**: 📋 PLANNED
**Created**: 2025-11-01
**Estimated Duration**: 4-6 weeks (160-240 hours)

---

## Overview

Phase 5 implements the complete frontend user interface using ASP.NET MVC with Razor views. The frontend will consume the REST API built in Phases 1-4 and provide a comprehensive web interface for documentation management, project tracking, and Kanban board visualization.

**Key Features**:
- Complete MVC application with authentication UI
- Documentation management interface with markdown editor
- Project/Epic/Task tracking with advanced search
- Interactive Kanban board with drag-and-drop
- Responsive design using Bootstrap 5
- Client-side validation and Ajax integration
- Role-based UI elements

**Technology Stack**:
- ASP.NET Core 9.0 MVC
- Razor Views with Tag Helpers
- Bootstrap 5.3 for responsive design
- jQuery 3.7 for DOM manipulation
- JavaScript ES6+ for interactivity
- Marked.js for markdown rendering
- SortableJS for drag-and-drop

---

## Architecture Design

### Project Structure

```
DocsAndPlannings.Web/
├── Controllers/
│   ├── AccountController.cs         (Authentication UI)
│   ├── DashboardController.cs       (Main dashboard)
│   ├── DocumentsController.cs       (Documentation UI)
│   ├── ProjectsController.cs        (Project management UI)
│   ├── EpicsController.cs           (Epic management UI)
│   ├── WorkItemsController.cs       (Task/Bug UI)
│   ├── BoardController.cs           (Kanban board UI)
│   └── HomeController.cs            (Landing page)
├── Models/
│   ├── ViewModels/
│   │   ├── Account/                 (Login, Register, Profile)
│   │   ├── Documents/               (Document editor, viewer)
│   │   ├── Projects/                (Project forms, lists)
│   │   ├── Epics/                   (Epic forms, lists)
│   │   ├── WorkItems/               (Work item forms, search)
│   │   └── Board/                   (Kanban board view)
│   └── ErrorViewModel.cs
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml           (Main layout)
│   │   ├── _LoginPartial.cshtml     (Login/logout links)
│   │   ├── _NavigationPartial.cshtml (Navigation menu)
│   │   ├── Error.cshtml             (Error page)
│   │   └── Components/              (View components)
│   ├── Account/                     (Auth views)
│   ├── Dashboard/                   (Dashboard views)
│   ├── Documents/                   (Document views)
│   ├── Projects/                    (Project views)
│   ├── Epics/                       (Epic views)
│   ├── WorkItems/                   (Work item views)
│   ├── Board/                       (Kanban board views)
│   └── Home/
├── wwwroot/
│   ├── css/
│   │   ├── site.css                 (Custom styles)
│   │   ├── markdown-editor.css      (Markdown editor styles)
│   │   └── kanban-board.css         (Kanban board styles)
│   ├── js/
│   │   ├── site.js                  (Global scripts)
│   │   ├── api-client.js            (API helper functions)
│   │   ├── markdown-editor.js       (Markdown editor)
│   │   └── kanban-board.js          (Drag-and-drop logic)
│   └── lib/                         (Third-party libraries)
└── Services/
    └── ApiClient.cs                 (HttpClient wrapper for API)
```

### Frontend Architecture Pattern

**Pattern**: Model-View-Controller (MVC) with API Backend

```
┌─────────────────────────────────────────────────────┐
│                    Browser                           │
│  ┌─────────────┐    ┌──────────────┐               │
│  │ Razor Views │◄───│ JavaScript    │               │
│  │  (.cshtml)  │    │ (Ajax calls)  │               │
│  └──────┬──────┘    └───────┬──────┘               │
│         │                    │                       │
└─────────┼────────────────────┼───────────────────────┘
          │                    │
          │ Form Posts         │ Ajax (JSON)
          │                    │
┌─────────▼────────────────────▼───────────────────────┐
│              MVC Controllers                          │
│  ┌────────────────────────────────────────┐         │
│  │ ViewModels ◄─► Controllers ◄─► Models  │         │
│  └─────────────────┬──────────────────────┘         │
│                    │                                  │
│              ┌─────▼──────┐                          │
│              │ ApiClient  │ (HttpClient)             │
│              └─────┬──────┘                          │
└────────────────────┼───────────────────────────────┘
                     │ HTTP (REST API)
┌────────────────────▼───────────────────────────────┐
│           DocsAndPlannings.Api                      │
│  ┌──────────────────────────────────────┐          │
│  │  REST Controllers ◄─► Services        │          │
│  └──────────────────┬───────────────────┘          │
│                     │                                │
│              ┌──────▼───────┐                       │
│              │ EF Core      │                       │
│              └──────┬───────┘                       │
└─────────────────────┼──────────────────────────────┘
                      │
                ┌─────▼──────┐
                │  SQLite DB │
                └────────────┘
```

### API Client Service

The frontend will use an `ApiClient` service to centralize all HTTP communication with the backend API:

```csharp
public interface IApiClient
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<T?> PostAsync<T>(string endpoint, object data);
    Task<T?> PutAsync<T>(string endpoint, object data);
    Task<bool> DeleteAsync(string endpoint);
    Task<byte[]> GetFileAsync(string endpoint);
    Task<bool> UploadFileAsync(string endpoint, IFormFile file);
}
```

---

## Phase 5 Task Breakdown

### Sprint 5.1: Core UI & Layout (40 hours / 1 week)

#### Task 5.1.1: Create API Client Service
**Estimate**: 6 hours
**Priority**: Critical (blocks all other work)
**Dependencies**: None

**Description**:
Create a centralized HTTP client service to communicate with the REST API backend.

**Implementation Details**:
- Create `IApiClient` interface in `Services/IApiClient.cs`
- Implement `ApiClient` class with HttpClient wrapper
- Add authentication token management (JWT from cookies/session)
- Implement error handling and response deserialization
- Add retry logic for transient failures
- Configure in Program.cs with DI
- **PREREQUISITE - Configure CORS in API Project** (DocsAndPlannings.Api/Program.cs):
  ```csharp
  services.AddCors(options =>
  {
      options.AddPolicy("WebAppPolicy", builder =>
      {
          builder.WithOrigins("https://localhost:5001", "https://yourdomain.com")
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowCredentials(); // Required for cookie-based auth
      });
  });

  app.UseCors("WebAppPolicy");
  ```
  Note: Update origins list with actual Web app URLs (development and production)

**Acceptance Criteria**:
- [ ] ApiClient service implements GET, POST, PUT, DELETE methods
- [ ] Supports generic type deserialization
- [ ] Handles authentication headers automatically
- [ ] Returns null on 404, throws on other errors
- [ ] Supports file upload/download
- [ ] Registered as scoped service in DI
- [ ] CORS configured in API project
- [ ] Ajax calls from Web to API succeed without CORS errors

**Testing Requirements**:
- [ ] Unit tests for ApiClient methods
- [ ] Mock HttpClient for testing
- [ ] Test error handling scenarios

**Files**:
- `source/DocsAndPlannings.Web/Services/IApiClient.cs`
- `source/DocsAndPlannings.Web/Services/ApiClient.cs`
- `tests/DocsAndPlannings.Web.Tests/Services/ApiClientTests.cs`

---

#### Task 5.1.2: Enhance Shared Layout with Navigation
**Estimate**: 5 hours
**Priority**: High
**Dependencies**: None

**Description**:
Update the main layout to include proper navigation, authentication state, and responsive design.

**Implementation Details**:
- Update `Views/Shared/_Layout.cshtml` with Bootstrap 5 navigation
- Create `Views/Shared/_LoginPartial.cshtml` for auth state display
- Create `Views/Shared/_NavigationPartial.cshtml` with menu items
  - Dashboard
  - Documents
  - Projects
  - Boards
  - Admin (if admin role)
- Add user profile dropdown (logout, settings)
- Implement responsive mobile menu
- Add flash message container for success/error messages

**Acceptance Criteria**:
- [ ] Navigation bar shows all main sections
- [ ] Login/logout links show based on authentication state
- [ ] User name displayed when logged in
- [ ] Mobile-responsive hamburger menu works
- [ ] Active page highlighted in navigation
- [ ] Flash message container for alerts

**Testing Requirements**:
- [ ] Manual testing of responsive design (desktop, tablet, mobile)
- [ ] Test navigation links lead to correct pages
- [ ] Test authenticated vs unauthenticated states

**Files**:
- `source/DocsAndPlannings.Web/Views/Shared/_Layout.cshtml`
- `source/DocsAndPlannings.Web/Views/Shared/_LoginPartial.cshtml`
- `source/DocsAndPlannings.Web/Views/Shared/_NavigationPartial.cshtml`

---

#### Task 5.1.3: Create Account Controller and Views
**Estimate**: 8 hours
**Priority**: Critical (blocks authenticated features)
**Dependencies**: Task 5.1.1 (ApiClient)

**Description**:
Implement authentication UI for login, registration, and logout.

**Implementation Details**:
- Create `AccountController` with actions:
  - Login (GET/POST)
  - Register (GET/POST)
  - Logout (POST)
  - Profile (GET/POST) - user profile management
- Create ViewModels:
  - `LoginViewModel` (Email, Password, RememberMe)
  - `RegisterViewModel` (Username, Email, Password, ConfirmPassword)
  - `ProfileViewModel` (Username, Email, current info)
- Create Razor views:
  - `Login.cshtml` - login form with validation
  - `Register.cshtml` - registration form with validation
  - `Profile.cshtml` - user profile view/edit
- Implement JWT token storage with secure cookie configuration:
  - Access token lifetime: 1 hour
  - Refresh token lifetime: 24 hours
  - HttpOnly flag: true (prevent XSS access)
  - Secure flag: true (HTTPS only)
  - SameSite: Strict (CSRF protection)
  - Automatic token refresh 5 minutes before expiration
  - Store access and refresh tokens in separate secure cookies
- Add client-side validation with jQuery Validation
- Call API endpoints via ApiClient

**Acceptance Criteria**:
- [ ] Login form with email/password fields
- [ ] Registration form with username, email, password, confirm password
- [ ] Client-side validation for all forms
- [ ] Server-side validation with error display
- [ ] JWT token stored in secure cookie on successful login
- [ ] Logout clears authentication cookie
- [ ] Redirect to Dashboard after successful login
- [ ] User profile page shows current user info

**Testing Requirements**:
- [ ] Unit tests for AccountController actions
- [ ] Test successful login stores token
- [ ] Test failed login shows error
- [ ] Test registration validation
- [ ] Test logout clears authentication

**Files**:
- `source/DocsAndPlannings.Web/Controllers/AccountController.cs`
- `source/DocsAndPlannings.Web/Models/ViewModels/Account/LoginViewModel.cs`
- `source/DocsAndPlannings.Web/Models/ViewModels/Account/RegisterViewModel.cs`
- `source/DocsAndPlannings.Web/Models/ViewModels/Account/ProfileViewModel.cs`
- `source/DocsAndPlannings.Web/Views/Account/Login.cshtml`
- `source/DocsAndPlannings.Web/Views/Account/Register.cshtml`
- `source/DocsAndPlannings.Web/Views/Account/Profile.cshtml`
- `tests/DocsAndPlannings.Web.Tests/Controllers/AccountControllerTests.cs`

---

#### Task 5.1.4: Create Dashboard Controller and View
**Estimate**: 6 hours
**Priority**: High
**Dependencies**: Task 5.1.1 (ApiClient), Task 5.1.3 (Authentication)

**Description**:
Create the main dashboard landing page showing overview of recent activity.

**Implementation Details**:
- Create `DashboardController` with Index action
- Create `DashboardViewModel` containing:
  - Recent documents (last 5)
  - Recent projects (last 5)
  - Recent work items assigned to user (last 10)
  - Activity statistics (total docs, projects, work items)
- Fetch data from multiple API endpoints via ApiClient
- Create `Dashboard/Index.cshtml` view with:
  - Welcome message with username
  - Quick stats cards (documents, projects, work items)
  - Recent activity lists with links
  - Quick action buttons (New Document, New Project, etc.)
- Add [Authorize] attribute to require authentication

**Acceptance Criteria**:
- [ ] Dashboard requires authentication
- [ ] Shows recent documents with titles and dates
- [ ] Shows recent projects with keys and names
- [ ] Shows recent assigned work items with status
- [ ] Quick stats display correct counts
- [ ] Quick action buttons link to create forms
- [ ] Responsive layout on mobile/tablet

**Testing Requirements**:
- [ ] Unit tests for DashboardController
- [ ] Test data aggregation from multiple APIs
- [ ] Test with empty data (no recent items)
- [ ] Test authorization requirement

**Files**:
- `source/DocsAndPlannings.Web/Controllers/DashboardController.cs`
- `source/DocsAndPlannings.Web/Models/ViewModels/Dashboard/DashboardViewModel.cs`
- `source/DocsAndPlannings.Web/Views/Dashboard/Index.cshtml`
- `tests/DocsAndPlannings.Web.Tests/Controllers/DashboardControllerTests.cs`

---

#### Task 5.1.5: Create Error Handling Views
**Estimate**: 3 hours
**Priority**: Medium
**Dependencies**: None

**Description**:
Create user-friendly error pages for common HTTP errors.

**Implementation Details**:
- Update existing `Views/Shared/Error.cshtml` with better styling
- Create `Views/Shared/NotFound.cshtml` (404 page)
- Create `Views/Shared/Unauthorized.cshtml` (401/403 page)
- Create `Views/Shared/ServerError.cshtml` (500 page)
- Configure error handling in Program.cs:
  - Development: UseExceptionHandler with detailed page
  - Production: UseExceptionHandler with friendly page
  - UseStatusCodePagesWithReExecute for 404, 401, 403
- Add custom error controller if needed
- **Configure logging strategy**:
  ```csharp
  // In Program.cs
  services.AddLogging(builder =>
  {
      builder.AddConsole();
      builder.AddDebug();
      if (app.Environment.IsProduction())
      {
          // Use Serilog or NLog for file-based logging
          builder.AddFile("logs/webapp-{Date}.log");
          // Consider: Application Insights, Sentry for production monitoring
      }
  });

  // Centralized error handling with logging
  app.UseExceptionHandler(errorApp =>
  {
      errorApp.Run(async context =>
      {
          var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
          var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

          logger.LogError(exception,
              "Unhandled exception. User: {User}, Path: {Path}",
              context.User.Identity?.Name ?? "Anonymous",
              context.Request.Path);

          // Redirect to error page
          context.Response.Redirect("/Error");
      });
  });
  ```
- Configure log levels (Information, Warning, Error)
- Set log retention policy (30 days recommended)
- Add structured logging with user context

**Acceptance Criteria**:
- [ ] 404 page shows "Page Not Found" message with navigation
- [ ] 401/403 page shows "Unauthorized" message with login link
- [ ] 500 page shows "Server Error" message
- [ ] Error pages match site design and layout
- [ ] Development mode shows detailed error information
- [ ] Production mode shows friendly error messages
- [ ] Logging configured for console (dev) and file (production)
- [ ] All unhandled exceptions logged with context (user, path, stack trace)
- [ ] Log files rotate daily
- [ ] Log retention policy configured (30 days)

**Testing Requirements**:
- [ ] Manual test of each error page
- [ ] Test navigation from error pages
- [ ] Test in both Development and Production modes

**Files**:
- `source/DocsAndPlannings.Web/Views/Shared/Error.cshtml`
- `source/DocsAndPlannings.Web/Views/Shared/NotFound.cshtml`
- `source/DocsAndPlannings.Web/Views/Shared/Unauthorized.cshtml`
- `source/DocsAndPlannings.Web/Views/Shared/ServerError.cshtml`

---

#### Task 5.1.6: Add Custom CSS and JavaScript
**Estimate**: 4 hours
**Priority**: Medium
**Dependencies**: Task 5.1.2 (Layout)

**Description**:
Create custom CSS and JavaScript for site-wide styling and functionality.

**Implementation Details**:
- Create `wwwroot/css/site.css` with custom styles:
  - Custom color scheme and branding
  - Button styles
  - Form styles
  - Card styles
  - Utility classes
- Create `wwwroot/js/site.js` with common functions:
  - Flash message display helper
  - Form submission helpers
  - Loading spinner helper
  - Date formatting utilities
  - CSRF token management for Ajax requests
- Create `wwwroot/js/api-client.js` for client-side API calls:
  - Wrapper for fetch API
  - Error handling
  - Token management
  - CSRF token injection in headers
- Configure CSRF protection in Program.cs:
  ```csharp
  services.AddAntiforgery(options => {
      options.HeaderName = "X-CSRF-TOKEN";
      options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
  });
  ```
- Add CSRF meta tag in _Layout.cshtml:
  ```html
  @inject Microsoft.AspNetCore.Antiforgery.IAntiforgery Antiforgery
  <meta name="csrf-token" content="@Antiforgery.GetAndStoreTokens(Context).RequestToken" />
  ```
- Add CSRF token to all Ajax requests in site.js
- Configure Content Security Policy (CSP) and security headers:
  ```csharp
  app.Use(async (context, next) => {
      context.Response.Headers.Add("Content-Security-Policy",
          "default-src 'self'; " +
          "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
          "style-src 'self' 'unsafe-inline'; " +
          "img-src 'self' data: https:; " +
          "font-src 'self'; " +
          "connect-src 'self'; " +
          "frame-ancestors 'none'");
      context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
      context.Response.Headers.Add("X-Frame-Options", "DENY");
      context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
      await next();
  });
  ```
- Include libraries in _Layout.cshtml

**Acceptance Criteria**:
- [ ] Custom CSS applies consistent styling across site
- [ ] JavaScript utilities work without errors
- [ ] Flash messages display and auto-dismiss
- [ ] Loading indicators show during async operations
- [ ] All scripts include proper error handling
- [ ] Code follows JavaScript ES6+ standards
- [ ] CSRF protection configured and tested
- [ ] CSRF token automatically added to all Ajax requests
- [ ] CSP headers configured correctly
- [ ] Security headers present in all responses
- [ ] All forms include antiforgery tokens

**Testing Requirements**:
- [ ] Manual testing of all JavaScript functions
- [ ] Test in multiple browsers (Chrome, Firefox, Edge)
- [ ] Test responsive CSS on different screen sizes

**Files**:
- `source/DocsAndPlannings.Web/wwwroot/css/site.css`
- `source/DocsAndPlannings.Web/wwwroot/js/site.js`
- `source/DocsAndPlannings.Web/wwwroot/js/api-client.js`

---

#### Task 5.1.7: Configure Authentication Middleware
**Estimate**: 4 hours
**Priority**: Critical
**Dependencies**: Task 5.1.1 (ApiClient)

**Description**:
Configure JWT authentication in the MVC app to validate tokens from cookies.

**Implementation Details**:
- Update `Program.cs` to add JWT authentication with cookie configuration:
  ```csharp
  services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
      .AddCookie(options => {
          options.Cookie.HttpOnly = true;          // Prevent XSS access
          options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
          options.Cookie.SameSite = SameSiteMode.Strict; // CSRF protection
          options.ExpireTimeSpan = TimeSpan.FromHours(8);
          options.SlidingExpiration = true;        // Auto-refresh on activity
          options.LoginPath = "/Account/Login";
          options.AccessDeniedPath = "/Account/AccessDenied";
      });
  ```
- Configure JwtBearer authentication scheme
- Set token validation parameters (issuer, audience, key - must match API settings)
- Add authorization policies (Admin, User roles)
- Configure [Authorize] attribute behavior
- Add middleware to extract JWT from cookie and validate
- Implement automatic token refresh 5 minutes before expiration

**Acceptance Criteria**:
- [ ] JWT authentication configured correctly
- [ ] Tokens validated against same settings as API
- [ ] [Authorize] attribute protects controllers/actions
- [ ] Role-based authorization works (Admin, User)
- [ ] Unauthenticated users redirected to login
- [ ] Authentication state available in views

**Testing Requirements**:
- [ ] Test protected pages require authentication
- [ ] Test role-based authorization
- [ ] Test token expiration handling
- [ ] Test invalid token rejection

**Files**:
- `source/DocsAndPlannings.Web/Program.cs`

---

#### Task 5.1.8: Create Client-Side Validation Scripts
**Estimate**: 4 hours
**Priority**: Medium
**Dependencies**: None

**Description**:
Set up jQuery Validation for client-side form validation.

**Implementation Details**:
- Verify jQuery Validation libraries in wwwroot/lib
- Create `Views/Shared/_ValidationScriptsPartial.cshtml` with validation includes
- Configure unobtrusive validation
- Add custom validation rules:
  - Email format
  - Password strength
  - Date range validation
  - File size/type validation
- Create custom validation error styling in CSS
- Test with all forms

**Acceptance Criteria**:
- [ ] jQuery Validation libraries included
- [ ] Unobtrusive validation enabled
- [ ] Validation messages display inline with fields
- [ ] Custom validation rules work correctly
- [ ] Error styling matches site design
- [ ] Validation works on all forms

**Testing Requirements**:
- [ ] Manual testing of each validation rule
- [ ] Test form submission with valid/invalid data
- [ ] Test validation messages appearance

**Files**:
- `source/DocsAndPlannings.Web/Views/Shared/_ValidationScriptsPartial.cshtml`
- `source/DocsAndPlannings.Web/wwwroot/css/validation.css`

---

### Sprint 5.2: Documentation UI (50 hours / 1.5 weeks)

#### Task 5.2.1: Create Documents Controller Actions
**Estimate**: 8 hours
**Priority**: High
**Dependencies**: Sprint 5.1 (ApiClient, Authentication)

**Description**:
Implement MVC controller for document management operations.

**Implementation Details**:
- Create `DocumentsController` with actions:
  - Index (GET) - list all documents with search/filter
  - Details (GET) - view document with rendered markdown
  - Create (GET/POST) - create new document form
  - Edit (GET/POST) - edit existing document
  - Delete (POST) - delete document
  - History (GET) - view document version history
  - Restore (POST) - restore specific version
- Add authorization checks (author-only for edit/delete)
- Call Documents API endpoints via ApiClient
- Handle file attachments (screenshot upload)

**Acceptance Criteria**:
- [ ] All CRUD actions implemented
- [ ] Authorization enforced (author can edit/delete)
- [ ] Published documents viewable by all authenticated users
- [ ] Draft documents viewable only by author
- [ ] Version history accessible
- [ ] Error handling for API failures

**Testing Requirements**:
- [ ] Unit tests for all controller actions
- [ ] Test authorization rules
- [ ] Test with valid and invalid document IDs
- [ ] Test version history and restore

**Files**:
- `source/DocsAndPlannings.Web/Controllers/DocumentsController.cs`
- `tests/DocsAndPlannings.Web.Tests/Controllers/DocumentsControllerTests.cs`

---

#### Task 5.2.2: Create Document ViewModels
**Estimate**: 4 hours
**Priority**: High
**Dependencies**: Task 5.2.1

**Description**:
Create ViewModels for document operations.

**Implementation Details**:
- Create `DocumentListItemViewModel` for list display
- Create `DocumentDetailsViewModel` for viewing
- Create `CreateDocumentViewModel` for creation form
- Create `EditDocumentViewModel` for editing
- Create `DocumentVersionViewModel` for version history
- Add validation attributes for all models
- Map between API DTOs and ViewModels

**Acceptance Criteria**:
- [ ] All ViewModels have required properties
- [ ] Validation attributes applied correctly
- [ ] ViewModels match form requirements
- [ ] Display annotations for labels/formatting

**Testing Requirements**:
- [ ] Validation rules tested
- [ ] Mapping between DTOs and ViewModels tested

**Files**:
- `source/DocsAndPlannings.Web/Models/ViewModels/Documents/DocumentListItemViewModel.cs`
- `source/DocsAndPlannings.Web/Models/ViewModels/Documents/DocumentDetailsViewModel.cs`
- `source/DocsAndPlannings.Web/Models/ViewModels/Documents/CreateDocumentViewModel.cs`
- `source/DocsAndPlannings.Web/Models/ViewModels/Documents/EditDocumentViewModel.cs`
- `source/DocsAndPlannings.Web/Models/ViewModels/Documents/DocumentVersionViewModel.cs`

---

#### Task 5.2.3: Create Document List View
**Estimate**: 6 hours
**Priority**: High
**Dependencies**: Task 5.2.1, Task 5.2.2

**Description**:
Create the document list/index page with search and filtering.

**Implementation Details**:
- Create `Views/Documents/Index.cshtml`
- Display documents in card grid or table layout
- Implement search form (text search)
- Implement filters (tags, author, published status)
- Add pagination controls
- Show document title, summary, author, dates
- Add "New Document" button
- Add action links (View, Edit, Delete) per document
- Implement responsive layout

**Acceptance Criteria**:
- [ ] Documents displayed in organized layout
- [ ] Search form filters documents by text
- [ ] Tag filter works correctly
- [ ] Pagination navigates through results
- [ ] Edit/Delete links visible only for document author
- [ ] View link available for all published documents
- [ ] Responsive on mobile/tablet

**Testing Requirements**:
- [ ] Manual testing of search and filters
- [ ] Test pagination
- [ ] Test authorization visibility rules
- [ ] Test empty state (no documents)

**Files**:
- `source/DocsAndPlannings.Web/Views/Documents/Index.cshtml`

---

#### Task 5.2.4: Create Document Details View
**Estimate**: 6 hours
**Priority**: High
**Dependencies**: Task 5.2.1, Task 5.2.2

**Description**:
Create the document viewer page with rendered markdown.

**Implementation Details**:
- Create `Views/Documents/Details.cshtml`
- Display document metadata (title, author, dates, tags)
- Render markdown content as HTML using Marked.js
- Display attachments/screenshots with thumbnails
- Add navigation (back to list, edit, delete)
- Add version history link
- Implement document hierarchy breadcrumb (parent docs)
- Show related documents (children in hierarchy)
- Apply syntax highlighting for code blocks (Prism.js or Highlight.js)

**Acceptance Criteria**:
- [ ] Markdown rendered correctly to HTML
- [ ] Code blocks have syntax highlighting
- [ ] Images/screenshots displayed inline
- [ ] Breadcrumb shows document hierarchy
- [ ] Edit/Delete buttons visible only to author
- [ ] Version history link present
- [ ] Related documents listed

**Testing Requirements**:
- [ ] Test markdown rendering with various syntax
- [ ] Test code highlighting
- [ ] Test image display
- [ ] Test hierarchy breadcrumb navigation

**Files**:
- `source/DocsAndPlannings.Web/Views/Documents/Details.cshtml`
- `source/DocsAndPlannings.Web/wwwroot/js/markdown-renderer.js`

---

#### Task 5.2.5: Create Document Editor View with Markdown Support
**Estimate**: 12 hours
**Priority**: High
**Dependencies**: Task 5.2.1, Task 5.2.2

**Description**:
Create document create/edit forms with markdown editor and live preview.

**Implementation Details**:
- Create `Views/Documents/Create.cshtml` form
- Create `Views/Documents/Edit.cshtml` form
- Implement markdown editor with:
  - Syntax toolbar (bold, italic, headers, lists, code, links, images)
  - Split-pane layout (editor on left, preview on right)
  - Live preview of rendered markdown
  - Textarea with markdown content
- Add fields:
  - Title (required)
  - Content (markdown, required)
  - Tags (multiselect or autocomplete)
  - Parent document (dropdown for hierarchy)
  - Published status (checkbox)
- Add screenshot upload section:
  - File input for image upload
  - Preview uploaded images
  - Insert image markdown syntax into content
  - File size validation (10 MB max per Phase 2 spec)
  - File type validation (image formats only: PNG, JPG, GIF, WebP)
  - Progress bar for large uploads
  - Clear error messages if size/type invalid
- Configure file upload limits in Program.cs:
  ```csharp
  services.Configure<FormOptions>(options =>
  {
      options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
  });
  ```
- Add to web.config if using IIS:
  ```xml
  <system.webServer>
    <security>
      <requestFiltering>
        <requestLimits maxAllowedContentLength="10485760" /> <!-- 10 MB -->
      </requestFiltering>
    </security>
  </system.webServer>
  ```
- Add save/cancel buttons
- Implement autosave to localStorage (prevent data loss)
- Client-side validation

**Acceptance Criteria**:
- [ ] Markdown editor with toolbar buttons
- [ ] Live preview updates as user types
- [ ] Image upload works and inserts markdown
- [ ] File size limit enforced (10 MB max)
- [ ] File type validation (images only)
- [ ] Progress bar displayed during upload
- [ ] Clear error message if file too large
- [ ] Clear error message if invalid file type
- [ ] Tag selection/autocomplete functional
- [ ] Parent document hierarchy selection works
- [ ] Autosave prevents data loss
- [ ] Form validation prevents empty submission
- [ ] Cancel button returns to list without saving

**Testing Requirements**:
- [ ] Test markdown syntax with toolbar buttons
- [ ] Test live preview rendering
- [ ] Test image upload and insertion
- [ ] Test form validation
- [ ] Test autosave functionality

**Files**:
- `source/DocsAndPlannings.Web/Views/Documents/Create.cshtml`
- `source/DocsAndPlannings.Web/Views/Documents/Edit.cshtml`
- `source/DocsAndPlannings.Web/wwwroot/js/markdown-editor.js`
- `source/DocsAndPlannings.Web/wwwroot/css/markdown-editor.css`

---

#### Task 5.2.6: Create Version History View
**Estimate**: 6 hours
**Priority**: Medium
**Dependencies**: Task 5.2.1, Task 5.2.2

**Description**:
Create page to view document version history and restore previous versions.

**Implementation Details**:
- Create `Views/Documents/History.cshtml`
- Display list of all document versions
- Show version number, date, author for each version
- Add "View" link to see version content (modal or separate page)
- Add "Restore" button to restore old version
- Highlight current version
- Show diff view comparing versions (optional advanced feature)

**Acceptance Criteria**:
- [ ] Version history lists all versions chronologically
- [ ] Current version clearly marked
- [ ] View link shows version content
- [ ] Restore button creates new version from old content
- [ ] Only document author can restore versions
- [ ] Confirmation dialog before restore

**Testing Requirements**:
- [ ] Test with documents having multiple versions
- [ ] Test restore functionality
- [ ] Test authorization for restore
- [ ] Test version viewing

**Files**:
- `source/DocsAndPlannings.Web/Views/Documents/History.cshtml`
- `source/DocsAndPlannings.Web/Views/Documents/ViewVersion.cshtml`

---

#### Task 5.2.7: Add Tag Management UI
**Estimate**: 5 hours
**Priority**: Medium
**Dependencies**: Task 5.2.1

**Description**:
Create tag management interface for admin users.

**Implementation Details**:
- Create `TagsController` with actions (admin only):
  - Index (GET) - list all tags
  - Create (POST) - create new tag via Ajax
  - Edit (PUT) - edit tag via Ajax
  - Delete (DELETE) - delete tag via Ajax
- Create `Views/Tags/Index.cshtml` for admin tag management
- Implement inline editing of tags
- Add tag color picker for visual categorization
- Show document count per tag
- Restrict access to Admin role only

**Acceptance Criteria**:
- [ ] Admin can view all tags
- [ ] Admin can create new tags
- [ ] Admin can edit tag name/color
- [ ] Admin can delete unused tags
- [ ] Delete prevented if tag in use
- [ ] Non-admin users cannot access
- [ ] Document count displayed per tag

**Testing Requirements**:
- [ ] Test admin-only authorization
- [ ] Test CRUD operations for tags
- [ ] Test delete prevention for tags in use

**Files**:
- `source/DocsAndPlannings.Web/Controllers/TagsController.cs`
- `source/DocsAndPlannings.Web/Views/Tags/Index.cshtml`
- `tests/DocsAndPlannings.Web.Tests/Controllers/TagsControllerTests.cs`

---

#### Task 5.2.8: Integrate Markdown Libraries
**Estimate**: 3 hours
**Priority**: High
**Dependencies**: Task 5.2.4, Task 5.2.5

**Description**:
Add and configure JavaScript libraries for markdown rendering.

**Implementation Details**:
- Download and add to wwwroot/lib:
  - Marked.js (markdown to HTML)
  - Highlight.js or Prism.js (syntax highlighting)
- Create wrapper JavaScript for markdown rendering
- Configure syntax highlighting themes
- Add sanitization to prevent XSS in markdown
- Test with various markdown samples

**Acceptance Criteria**:
- [ ] Marked.js renders markdown correctly
- [ ] Code blocks have syntax highlighting
- [ ] XSS prevention in place (sanitize HTML)
- [ ] Support for tables, lists, headers, code, images
- [ ] Performance acceptable for large documents

**Testing Requirements**:
- [ ] Test various markdown syntax
- [ ] Test XSS prevention with malicious markdown
- [ ] Test code highlighting for multiple languages

**Files**:
- `source/DocsAndPlannings.Web/wwwroot/lib/marked/` (library)
- `source/DocsAndPlannings.Web/wwwroot/lib/highlight/` (library)
- `source/DocsAndPlannings.Web/wwwroot/js/markdown-renderer.js`

---

### Sprint 5.3: Planning UI (60 hours / 2 weeks)

#### Task 5.3.1: Create Projects Controller and Views
**Estimate**: 10 hours
**Priority**: High
**Dependencies**: Sprint 5.1 (ApiClient, Authentication)

**Description**:
Implement project management UI.

**Implementation Details**:
- Create `ProjectsController` with actions:
  - Index (GET) - list all projects
  - Details (GET) - project overview with epics/work items
  - Create (GET/POST) - create new project
  - Edit (GET/POST) - edit project
  - Delete (POST) - soft delete/archive project
  - Archive (POST) - archive project
  - Unarchive (POST) - unarchive project
- Create ViewModels:
  - `ProjectListItemViewModel`
  - `ProjectDetailsViewModel`
  - `CreateProjectViewModel`
  - `EditProjectViewModel`
- Create views:
  - `Index.cshtml` - project list with search/filter
  - `Details.cshtml` - project overview
  - `Create.cshtml` - create form
  - `Edit.cshtml` - edit form
- Authorization: only project owner can edit/delete

**Acceptance Criteria**:
- [ ] Project list shows all user's projects
- [ ] Search/filter by project key or name
- [ ] Create project form with key generation preview
- [ ] Project details show epics and work items summary
- [ ] Edit/delete only for project owner
- [ ] Archive/unarchive functionality
- [ ] Validation prevents duplicate project keys

**Testing Requirements**:
- [ ] Unit tests for controller actions
- [ ] Test authorization rules
- [ ] Test archive/unarchive
- [ ] Manual UI testing

**Files**:
- `source/DocsAndPlannings.Web/Controllers/ProjectsController.cs`
- `source/DocsAndPlannings.Web/Models/ViewModels/Projects/*.cs`
- `source/DocsAndPlannings.Web/Views/Projects/*.cshtml`
- `tests/DocsAndPlannings.Web.Tests/Controllers/ProjectsControllerTests.cs`

---

#### Task 5.3.2: Create Epics Controller and Views
**Estimate**: 10 hours
**Priority**: High
**Dependencies**: Task 5.3.1

**Description**:
Implement epic management UI.

**Implementation Details**:
- Create `EpicsController` with actions:
  - Index (GET) - list epics for project
  - Details (GET) - epic details with work items
  - Create (GET/POST) - create epic
  - Edit (GET/POST) - edit epic
  - Delete (POST) - delete epic
  - UpdateStatus (POST) - change epic status
- Create ViewModels for epics
- Create views with forms
- Display epic hierarchy within project
- Show work item count and status distribution

**Acceptance Criteria**:
- [ ] Epic list filtered by project
- [ ] Create epic within project context
- [ ] Epic details show associated work items
- [ ] Status update via dropdown
- [ ] Assignment to users
- [ ] Progress indicator based on work items

**Testing Requirements**:
- [ ] Unit tests for controller
- [ ] Test epic creation within project
- [ ] Test status transitions

**Files**:
- `source/DocsAndPlannings.Web/Controllers/EpicsController.cs`
- `source/DocsAndPlannings.Web/Models/ViewModels/Epics/*.cs`
- `source/DocsAndPlannings.Web/Views/Epics/*.cshtml`
- `tests/DocsAndPlannings.Web.Tests/Controllers/EpicsControllerTests.cs`

---

#### Task 5.3.3: Create Work Items Controller and Views
**Estimate**: 14 hours
**Priority**: High
**Dependencies**: Task 5.3.2

**Description**:
Implement task/bug/subtask management UI.

**Implementation Details**:
- Create `WorkItemsController` with actions:
  - Index (GET) - list work items with advanced search
  - Details (GET) - work item details with comments
  - Create (GET/POST) - create work item (Task, Bug, Subtask)
  - Edit (GET/POST) - edit work item
  - Delete (POST) - delete work item
  - UpdateStatus (POST) - change status
  - Assign (POST) - assign to user
  - AddComment (POST) - add comment (via Ajax)
- Create comprehensive ViewModels
- Create views with all fields:
  - Summary, Description (markdown), Type, Priority, Status
  - Assignee, Reporter, Due Date
  - Parent work item (for subtasks), Epic
- Implement advanced search form:
  - Filter by project, epic, status, assignee, type, priority
  - Text search in summary/description
  - Date range filter
- Display work item hierarchy (task → subtasks)

**Acceptance Criteria**:
- [ ] Work item list with advanced search/filter
- [ ] Create form supports Task, Bug, Subtask types
- [ ] Subtask creation requires parent task
- [ ] Work item details show all information
- [ ] Inline status updates
- [ ] Assignment dropdown with users
- [ ] Comments displayed chronologically
- [ ] Add comment via Ajax without page reload
- [ ] Markdown rendering in description
- [ ] Due date picker

**Testing Requirements**:
- [ ] Unit tests for all actions
- [ ] Test hierarchy rules (subtask requires parent)
- [ ] Test search/filter combinations
- [ ] Test comment creation

**Files**:
- `source/DocsAndPlannings.Web/Controllers/WorkItemsController.cs`
- `source/DocsAndPlannings.Web/Models/ViewModels/WorkItems/*.cs`
- `source/DocsAndPlannings.Web/Views/WorkItems/*.cshtml`
- `tests/DocsAndPlannings.Web.Tests/Controllers/WorkItemsControllerTests.cs`

---

#### Task 5.3.4: Create Status Management UI
**Estimate**: 5 hours
**Priority**: Medium
**Dependencies**: Sprint 5.1

**Description**:
Create admin UI for status management.

**Implementation Details**:
- Create `StatusesController` (admin only) with actions:
  - Index (GET) - list all statuses
  - Create (POST) - create custom status
  - Edit (PUT) - edit status
  - Delete (DELETE) - delete status
  - ConfigureTransitions (GET/POST) - configure allowed transitions
- Create views for status management
- Display default vs custom statuses
- Configure status colors for visual representation
- Set up transition rules (which statuses can transition to which)

**Acceptance Criteria**:
- [ ] Admin can view all statuses
- [ ] Admin can create custom statuses
- [ ] Admin can edit status name/color
- [ ] Admin can configure transition rules
- [ ] Delete prevented for statuses in use
- [ ] Default statuses protected from deletion

**Testing Requirements**:
- [ ] Test admin authorization
- [ ] Test CRUD operations
- [ ] Test transition configuration

**Files**:
- `source/DocsAndPlannings.Web/Controllers/StatusesController.cs`
- `source/DocsAndPlannings.Web/Views/Statuses/*.cshtml`
- `tests/DocsAndPlannings.Web.Tests/Controllers/StatusesControllerTests.cs`

---

#### Task 5.3.5: Implement Advanced Search Component
**Estimate**: 8 hours
**Priority**: High
**Dependencies**: Task 5.3.3

**Description**:
Create reusable advanced search component for work items.

**Implementation Details**:
- Create search view component or partial view
- Multi-field search form:
  - Text search (summary, description)
  - Project dropdown (multiselect)
  - Epic dropdown (multiselect)
  - Status dropdown (multiselect)
  - Assignee dropdown (multiselect)
  - Reporter dropdown (multiselect)
  - Work item type (Task, Bug, Subtask)
  - Priority (Low, Medium, High, Critical)
  - Date range (created, updated, due date)
- Implement as collapsible panel
- Save search filters to session/localStorage
- Export search results (CSV) - optional
- Results pagination

**Acceptance Criteria**:
- [ ] All search fields functional
- [ ] Multiselect dropdowns work correctly
- [ ] Search results update on form submission
- [ ] Filters combined with AND logic
- [ ] Reset button clears all filters
- [ ] Search state preserved on pagination
- [ ] Responsive on mobile

**Testing Requirements**:
- [ ] Test each filter independently
- [ ] Test combination of filters
- [ ] Test search with no results
- [ ] Test pagination with filters

**Files**:
- `source/DocsAndPlannings.Web/ViewComponents/WorkItemSearchComponent.cs`
- `source/DocsAndPlannings.Web/Views/Shared/Components/WorkItemSearch/Default.cshtml`
- `source/DocsAndPlannings.Web/wwwroot/js/work-item-search.js`

---

#### Task 5.3.6: Create Comments Component
**Estimate**: 6 hours
**Priority**: Medium
**Dependencies**: Task 5.3.3

**Description**:
Create reusable comments component for work items.

**Implementation Details**:
- Create comments view component
- Display comments list:
  - Author name and avatar (initials)
  - Comment text (markdown support)
  - Timestamp (relative: "5 minutes ago")
  - Edit/delete buttons (author only)
- Add comment form:
  - Textarea for comment text
  - Submit button
  - Ajax submission without page reload
- Inline edit functionality:
  - Click comment to edit
  - Save/cancel buttons
- Delete with confirmation dialog
- Auto-update comments (optional: polling or SignalR)

**Acceptance Criteria**:
- [ ] Comments displayed chronologically
- [ ] Add comment via Ajax
- [ ] Edit comment inline (author only)
- [ ] Delete comment with confirmation (author only)
- [ ] Markdown rendering in comments
- [ ] Relative timestamps
- [ ] Loading indicator during Ajax operations

**Testing Requirements**:
- [ ] Test add comment
- [ ] Test edit comment (authorization)
- [ ] Test delete comment (authorization)
- [ ] Test markdown rendering

**Files**:
- `source/DocsAndPlannings.Web/ViewComponents/CommentsComponent.cs`
- `source/DocsAndPlannings.Web/Views/Shared/Components/Comments/Default.cshtml`
- `source/DocsAndPlannings.Web/wwwroot/js/comments.js`

---

#### Task 5.3.7: Add User Selection Components
**Estimate**: 4 hours
**Priority**: Medium
**Dependencies**: Sprint 5.1

**Description**:
Create reusable user selection dropdown/autocomplete components.

**Implementation Details**:
- Create user dropdown component
- Fetch users from API
- Implement autocomplete search by name/email
- Display user avatar (initials) with name
- Support single or multiselect modes
- Use Select2 or similar library for enhanced dropdowns

**Acceptance Criteria**:
- [ ] User dropdown loads all users
- [ ] Autocomplete filters by name/email
- [ ] Selected users display correctly
- [ ] Works in both single and multiselect modes
- [ ] Responsive on mobile

**Testing Requirements**:
- [ ] Test user search/filter
- [ ] Test selection in forms
- [ ] Test with many users (performance)

**Files**:
- `source/DocsAndPlannings.Web/ViewComponents/UserSelectComponent.cs`
- `source/DocsAndPlannings.Web/Views/Shared/Components/UserSelect/Default.cshtml`
- `source/DocsAndPlannings.Web/wwwroot/js/user-select.js`

---

#### Task 5.3.8: Create Priority and Type Badge Components
**Estimate**: 3 hours
**Priority**: Low
**Dependencies**: None

**Description**:
Create reusable badge components for displaying work item metadata.

**Implementation Details**:
- Create priority badge component:
  - Critical (red), High (orange), Medium (yellow), Low (gray)
  - Icon + text
- Create work item type badge component:
  - Task (blue), Bug (red), Subtask (gray)
  - Icon + text
- Create status badge component:
  - Color-coded based on status
- Use consistent styling and icons

**Acceptance Criteria**:
- [ ] Priority badges color-coded correctly
- [ ] Type badges color-coded correctly
- [ ] Status badges use configured colors
- [ ] Icons display correctly
- [ ] Responsive and accessible

**Testing Requirements**:
- [ ] Manual testing of badge display
- [ ] Test in various contexts (lists, details)

**Files**:
- `source/DocsAndPlannings.Web/Views/Shared/_PriorityBadge.cshtml`
- `source/DocsAndPlannings.Web/Views/Shared/_TypeBadge.cshtml`
- `source/DocsAndPlannings.Web/Views/Shared/_StatusBadge.cshtml`
- `source/DocsAndPlannings.Web/wwwroot/css/badges.css`

---

### Sprint 5.4: Kanban Board UI (50 hours / 1.5 weeks)

#### Task 5.4.1: Create Board Controller and ViewModels
**Estimate**: 6 hours
**Priority**: High
**Dependencies**: Sprint 5.1 (ApiClient, Authentication)

**Description**:
Implement Kanban board controller for displaying project boards.

**Implementation Details**:
- Create `BoardController` with actions:
  - Index (GET) - redirect to first project board or project selection
  - View (GET) - display board for project ID
  - CreateBoard (POST) - initialize board for project
  - UpdateColumn (PUT) - update column configuration via Ajax
  - ReorderColumns (PUT) - reorder columns via Ajax
  - MoveWorkItem (PUT) - move work item between columns via Ajax
  - GetBoardData (GET) - get board data as JSON for Ajax refresh
- Create ViewModels:
  - `BoardViewModel` - full board with columns and work items
  - `BoardColumnViewModel` - column with work items
  - `BoardWorkItemViewModel` - work item card data
  - `MoveWorkItemViewModel` - for move operations
- Call Board API endpoints via ApiClient

**Acceptance Criteria**:
- [ ] Board view displays for valid project ID
- [ ] Board creation initializes columns from statuses
- [ ] Ajax endpoints return JSON for client-side operations
- [ ] Authorization checks project access
- [ ] Error handling for missing board

**Testing Requirements**:
- [ ] Unit tests for all controller actions
- [ ] Test authorization rules
- [ ] Test Ajax endpoint responses
- [ ] Test with project that has no board

**Files**:
- `source/DocsAndPlannings.Web/Controllers/BoardController.cs`
- `source/DocsAndPlannings.Web/Models/ViewModels/Board/*.cs`
- `tests/DocsAndPlannings.Web.Tests/Controllers/BoardControllerTests.cs`

---

#### Task 5.4.2: Create Kanban Board View Layout
**Estimate**: 8 hours
**Priority**: High
**Dependencies**: Task 5.4.1

**Description**:
Create the HTML/CSS layout for the Kanban board.

**Implementation Details**:
- Create `Views/Board/View.cshtml`
- Implement horizontal scrolling column layout
- Create board header:
  - Project name and key
  - Filter controls (epic, assignee, search)
  - Board settings button
  - Refresh button
- Create column structure:
  - Column header (status name, WIP count/limit)
  - Work item cards container
  - Add work item button per column
- Create work item card template:
  - Work item key and summary
  - Type and priority badges
  - Assignee avatar
  - Due date indicator
- Implement CSS grid or flexbox for responsive columns
- Add loading skeleton for initial load

**Acceptance Criteria**:
- [ ] Board displays horizontally scrollable columns
- [ ] Column headers show status name and work item count
- [ ] Work item cards display key info compactly
- [ ] Filter controls present in header
- [ ] Responsive on tablet (vertical scrolling for columns)
- [ ] Mobile shows single column at a time
- [ ] Loading state displays during data fetch

**Testing Requirements**:
- [ ] Manual testing of layout on desktop/tablet/mobile
- [ ] Test with varying numbers of columns (2-10)
- [ ] Test with varying work item counts per column
- [ ] Test empty board state

**Files**:
- `source/DocsAndPlannings.Web/Views/Board/View.cshtml`
- `source/DocsAndPlannings.Web/wwwroot/css/kanban-board.css`

---

#### Task 5.4.3: Implement Drag-and-Drop Functionality
**Estimate**: 12 hours
**Priority**: Critical
**Dependencies**: Task 5.4.2

**Description**:
Add drag-and-drop functionality to move work items between columns.

**Implementation Details**:
- Integrate SortableJS library for drag-and-drop
- Configure draggable work item cards
- Configure droppable columns
- Implement drag events:
  - onStart - highlight valid drop zones
  - onMove - validate transition (status rules)
  - onEnd - call API to update work item status
- Show visual feedback:
  - Dragging placeholder
  - Valid/invalid drop zones
  - Loading state during API call
- Handle API errors:
  - Invalid status transition - show error, revert card
  - Network error - show error, revert card
  - Success - keep card in new position, update UI
- Implement optimistic updates (move card immediately, revert if fails)
- Add touch support for mobile drag-and-drop

**Acceptance Criteria**:
- [ ] Work item cards draggable with mouse
- [ ] Cards can be dropped on valid columns
- [ ] Invalid transitions prevented with error message
- [ ] Optimistic UI updates (immediate move)
- [ ] Failed moves revert card position
- [ ] Visual feedback during drag (placeholder, highlight)
- [ ] Touch/mobile drag-and-drop works
- [ ] Smooth animations during drag

**Testing Requirements**:
- [ ] Test valid status transitions
- [ ] Test invalid status transitions (should fail gracefully)
- [ ] Test network error handling
- [ ] Test on touch devices
- [ ] Test rapid successive moves

**Files**:
- `source/DocsAndPlannings.Web/wwwroot/lib/sortable/` (library)
- `source/DocsAndPlannings.Web/wwwroot/js/kanban-board.js`
- `source/DocsAndPlannings.Web/wwwroot/css/kanban-board.css`

---

#### Task 5.4.4: Implement Board Filtering
**Estimate**: 6 hours
**Priority**: High
**Dependencies**: Task 5.4.2

**Description**:
Add filtering controls to Kanban board.

**Implementation Details**:
- Add filter controls to board header:
  - Epic dropdown (filter by epic)
  - Assignee dropdown (filter by assignee)
  - Search input (filter by work item key or summary)
  - "My Items Only" checkbox
  - "Clear Filters" button
- Implement client-side filtering logic:
  - Show/hide work item cards based on filters
  - Update work item counts in column headers
  - Preserve filters in URL query string
- Alternative: Server-side filtering via API (reload board data)
- Add visual indicator when filters active

**Acceptance Criteria**:
- [ ] Epic filter shows work items in selected epic only
- [ ] Assignee filter shows work items assigned to selected user
- [ ] Search filter matches work item key or summary text
- [ ] "My Items Only" shows only current user's assignments
- [ ] Multiple filters combine with AND logic
- [ ] Clear filters button resets all filters
- [ ] Column counts update based on filtered items
- [ ] Filters preserved in URL (shareable links)

**Testing Requirements**:
- [ ] Test each filter independently
- [ ] Test combination of filters
- [ ] Test URL preservation and sharing
- [ ] Test filter with empty results

**Files**:
- `source/DocsAndPlannings.Web/wwwroot/js/kanban-board-filter.js`

---

#### Task 5.4.5: Implement Quick Edit Modal
**Estimate**: 8 hours
**Priority**: Medium
**Dependencies**: Task 5.4.2

**Description**:
Create modal dialog for quick editing work items from board.

**Implementation Details**:
- Create modal dialog with work item fields:
  - Summary (editable)
  - Description (markdown textarea)
  - Priority (dropdown)
  - Assignee (user dropdown)
  - Due date (date picker)
  - Save/cancel buttons
  - "Open Full Details" link
- Trigger modal on work item card click
- Fetch work item details via Ajax
- Update work item via Ajax on save
- Update card UI after successful save
- Show loading state during operations

**Acceptance Criteria**:
- [ ] Click work item card opens quick edit modal
- [ ] Modal displays current work item data
- [ ] Fields are editable
- [ ] Save updates work item via API
- [ ] Card updates after save without page reload
- [ ] Cancel closes modal without changes
- [ ] "Open Full Details" navigates to work item details page
- [ ] Validation prevents invalid data

**Testing Requirements**:
- [ ] Test modal open/close
- [ ] Test field updates
- [ ] Test validation
- [ ] Test save success/failure

**Files**:
- `source/DocsAndPlannings.Web/Views/Board/_QuickEditModal.cshtml`
- `source/DocsAndPlannings.Web/wwwroot/js/quick-edit-modal.js`

---

#### Task 5.4.6: Implement Column Configuration
**Estimate**: 6 hours
**Priority**: Medium
**Dependencies**: Task 5.4.2

**Description**:
Add UI for configuring board columns.

**Implementation Details**:
- Add "Board Settings" button in board header
- Create settings modal with:
  - Column list with drag-to-reorder
  - WIP limit input per column
  - Column visibility toggle (collapse/expand)
  - Save/cancel buttons
- Implement column reordering via drag-and-drop
- Update column configuration via API
- Reflect changes in board immediately
- Show WIP limit warnings when exceeded (visual highlight)

**Acceptance Criteria**:
- [ ] Settings modal displays all columns
- [ ] Columns can be reordered via drag-and-drop
- [ ] WIP limits can be set per column
- [ ] Changes saved to API
- [ ] Board updates after settings save
- [ ] WIP limit exceeded shows visual warning
- [ ] Collapsed columns hide work items

**Testing Requirements**:
- [ ] Test column reordering
- [ ] Test WIP limit configuration
- [ ] Test column collapse/expand
- [ ] Test settings persistence

**Files**:
- `source/DocsAndPlannings.Web/Views/Board/_SettingsModal.cshtml`
- `source/DocsAndPlannings.Web/wwwroot/js/board-settings.js`

---

#### Task 5.4.7: Implement Board Auto-Refresh
**Estimate**: 4 hours
**Priority**: Low (Optional)
**Dependencies**: Task 5.4.1

**Description**:
Add automatic board refresh to show updates from other users.

**Implementation Details**:
- Implement polling mechanism (every 30 seconds)
- Fetch board data via Ajax
- Compare with current board state
- Update only changed cards (add, remove, move)
- Show notification when updates detected
- Pause auto-refresh during drag operation
- Add manual refresh button
- Option: Implement with SignalR for real-time updates (advanced)

**Acceptance Criteria**:
- [ ] Board refreshes automatically every 30 seconds
- [ ] Only changed items update (no full reload)
- [ ] No refresh during active drag operation
- [ ] Notification shows when updates applied
- [ ] Manual refresh button works immediately
- [ ] Performance acceptable with large boards

**Testing Requirements**:
- [ ] Test auto-refresh with multiple browser windows
- [ ] Test refresh during drag operation
- [ ] Test with concurrent updates

**Files**:
- `source/DocsAndPlannings.Web/wwwroot/js/board-auto-refresh.js`

---

#### Task 5.4.8: Add Board Loading and Error States
**Estimate**: 3 hours
**Priority**: Medium
**Dependencies**: Task 5.4.2

**Description**:
Implement loading skeletons and error state UI.

**Implementation Details**:
- Create loading skeleton for board:
  - Column header skeletons
  - Card skeletons (3-5 per column)
  - Animated pulse effect
- Create error state view:
  - Error message display
  - Retry button
  - "Back to Projects" button
- Create empty state for boards with no work items:
  - Message encouraging work item creation
  - "Create Work Item" button
- Handle various error scenarios:
  - Board not found for project
  - Network errors
  - API errors

**Acceptance Criteria**:
- [ ] Loading skeleton displays during initial load
- [ ] Error view displays on load failure
- [ ] Retry button attempts reload
- [ ] Empty state displays for boards with no work items
- [ ] All states visually polished

**Testing Requirements**:
- [ ] Test loading state (slow network simulation)
- [ ] Test error state (disconnect network)
- [ ] Test empty state (board with no work items)
- [ ] Test retry functionality

**Files**:
- `source/DocsAndPlannings.Web/Views/Board/_LoadingSkeleton.cshtml`
- `source/DocsAndPlannings.Web/Views/Board/_ErrorState.cshtml`
- `source/DocsAndPlannings.Web/Views/Board/_EmptyState.cshtml`

---

## Integration and Testing (Phase 5.5)

### Task 5.5.1: End-to-End Integration Testing
**Estimate**: 12 hours
**Priority**: High

**Description**:
Perform comprehensive integration testing of all frontend features with backend API.

**Testing Scenarios**:
1. User Registration and Login Flow
2. Document Creation, Editing, and Publishing
3. Project/Epic/Work Item Creation Flow
4. Work Item Search and Filtering
5. Kanban Board Drag-and-Drop
6. Comments and Activity Tracking
7. Authorization Rules (author-only, admin-only)
8. Error Handling and Edge Cases

---

### Task 5.5.2: Browser Compatibility Testing
**Estimate**: 6 hours
**Priority**: Medium

**Desktop Browsers (Required)**:
- Chrome: Last 2 versions (currently 120+)
- Firefox: Last 2 versions (currently 121+)
- Edge: Last 2 versions (currently 120+)
- Safari: Last 2 versions (currently 17+) - if macOS available

**Mobile Browsers (Required)**:
- iOS Safari: iOS 15+
- Chrome Mobile: Last 2 versions
- Samsung Internet: Last 2 versions

**Unsupported Browsers**:
- Internet Explorer 11 - No support
- Show browser update banner for very old browsers: "For best experience, please update your browser"
- Core functionality should still work where possible (graceful degradation)

**Test Devices and Resolutions**:
- Desktop (1920x1080, 1366x768, 1280x720)
- Tablet (iPad 1024x768, Android tablet 800x1280)
- Mobile (iPhone 390x844, Android 360x800)

**Browser Compatibility Checklist**:
- [ ] Layout renders correctly on all supported browsers
- [ ] JavaScript features work (no console errors)
- [ ] CSS styles applied correctly (no visual glitches)
- [ ] Form validation works
- [ ] Drag-and-drop works (desktop and touch)
- [ ] Markdown editor functional
- [ ] Ajax requests succeed
- [ ] Authentication flow works
- [ ] Responsive design adapts to screen sizes
- [ ] Touch gestures work on mobile browsers

**Testing Approach**:
- Use BrowserStack or similar service for cross-browser testing
- Test critical user flows on each browser
- Document any browser-specific issues and workarounds
- Add polyfills if needed for older browser support

---

### Task 5.5.3: Performance Optimization
**Estimate**: 8 hours
**Priority**: Medium

**Optimization Tasks**:
- Minimize and bundle CSS/JavaScript
- Enable response compression (gzip)
- Add caching headers for static assets
- Optimize images (compress, proper formats)
- Lazy load images on scroll
- Profile and optimize slow pages
- Reduce API call frequency (caching, debouncing)

---

### Task 5.5.4: Accessibility Audit
**Estimate**: 6 hours
**Priority**: Medium

**Accessibility Requirements**:
- Semantic HTML elements
- ARIA labels for interactive elements
- Keyboard navigation support
- Screen reader compatibility
- Color contrast compliance (WCAG AA)
- Focus indicators visible
- Form labels associated correctly

---

### Task 5.5.5: UI Polish and Refinement
**Estimate**: 8 hours
**Priority**: Low

**Polish Tasks**:
- Consistent spacing and alignment
- Smooth animations and transitions
- Loading indicators for all async operations
- Tooltips for icon buttons
- Consistent button and link styles
- Proper form validation messages
- Success/error message styling
- Favicon and page titles

---

## Dependencies and Prerequisites

### External Libraries Required

**CSS Frameworks**:
- Bootstrap 5.3.3 (already included in ASP.NET Core 9.0 template)

**JavaScript Libraries** (with specific versions):
- jQuery 3.7.1 (already included in ASP.NET Core template)
- jQuery Validation 1.19.5 (already included in ASP.NET Core template)
- Marked.js v11.2.0 (markdown rendering)
- Highlight.js v11.9.0 (code syntax highlighting)
- SortableJS v1.15.2 (drag-and-drop for Kanban board)
- Choices.js v10.2.0 (enhanced dropdowns - selected over Select2 for smaller size and modern API)
- date-fns v3.3.1 (date formatting - selected over Moment.js for tree-shakable, modern approach)

**Installation**:
```bash
# Option 1: Download libraries to wwwroot/lib/
# Download from CDN providers:
# - Marked.js: https://cdn.jsdelivr.net/npm/marked@11.2.0/
# - Highlight.js: https://cdn.jsdelivr.net/npm/highlight.js@11.9.0/
# - SortableJS: https://cdn.jsdelivr.net/npm/sortablejs@1.15.2/
# - Choices.js: https://cdn.jsdelivr.net/npm/choices.js@10.2.0/
# - date-fns: https://cdn.jsdelivr.net/npm/date-fns@3.3.1/

# Option 2: Use npm for library management (if desired)
# npm install marked@11.2.0 highlight.js@11.9.0 sortablejs@1.15.2 choices.js@10.2.0 date-fns@3.3.1

# Configure in _Layout.cshtml with proper script/link tags
```

**Library Decision Rationale**:
- **Choices.js over Select2**: Smaller bundle size (46KB vs 80KB), native-like UX, better mobile support
- **date-fns over Moment.js**: Tree-shakable (only bundle what you use), modern immutable API, smaller size
- **Highlight.js**: Wide language support, automatic language detection, smaller than Prism.js for common use cases

---

## API Endpoints Reference

The frontend will consume the following backend API endpoints:

### Authentication
- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/logout

### Users
- GET /api/users - list all users
- GET /api/users/{id} - get user by ID
- PUT /api/users/{id} - update user profile

### Documents
- GET /api/documents - list documents with search
- GET /api/documents/{id} - get document details
- POST /api/documents - create document
- PUT /api/documents/{id} - update document
- DELETE /api/documents/{id} - delete document
- GET /api/documents/{id}/versions - get version history
- POST /api/documents/{id}/versions/{versionId}/restore - restore version
- POST /api/documents/{id}/attachments - upload attachment
- GET /api/documents/{id}/attachments/{attachmentId} - download attachment
- DELETE /api/documents/{id}/attachments/{attachmentId} - delete attachment

### Tags
- GET /api/tags - list all tags
- POST /api/tags - create tag (admin)
- PUT /api/tags/{id} - update tag (admin)
- DELETE /api/tags/{id} - delete tag (admin)

### Projects
- GET /api/projects - list projects
- GET /api/projects/{id} - get project details
- POST /api/projects - create project
- PUT /api/projects/{id} - update project
- DELETE /api/projects/{id} - delete project
- POST /api/projects/{id}/archive - archive project
- POST /api/projects/{id}/unarchive - unarchive project

### Epics
- GET /api/epics - list epics (with project filter)
- GET /api/epics/{id} - get epic details
- POST /api/epics - create epic
- PUT /api/epics/{id} - update epic
- DELETE /api/epics/{id} - delete epic
- PUT /api/epics/{id}/status - update status
- PUT /api/epics/{id}/assign - assign to user

### Work Items
- GET /api/workitems - list work items with search
- GET /api/workitems/{id} - get work item details
- POST /api/workitems - create work item
- PUT /api/workitems/{id} - update work item
- DELETE /api/workitems/{id} - delete work item
- PUT /api/workitems/{id}/status - update status
- PUT /api/workitems/{id}/assign - assign to user

### Comments
- GET /api/workitems/{workItemId}/comments - list comments
- POST /api/workitems/{workItemId}/comments - create comment
- PUT /api/comments/{id} - update comment
- DELETE /api/comments/{id} - delete comment

### Statuses
- GET /api/statuses - list all statuses
- POST /api/statuses - create status (admin)
- PUT /api/statuses/{id} - update status (admin)
- DELETE /api/statuses/{id} - delete status (admin)
- POST /api/statuses/validate-transition - validate transition

### Boards
- GET /api/projects/{projectId}/board - get board
- POST /api/projects/{projectId}/board - create board
- PUT /api/projects/{projectId}/board - update board settings
- DELETE /api/projects/{projectId}/board - delete board
- GET /api/projects/{projectId}/board/view - get board view with work items
- PUT /api/projects/{projectId}/board/columns/{columnId} - update column
- PUT /api/projects/{projectId}/board/columns/reorder - reorder columns
- PUT /api/projects/{projectId}/board/workitems/{workItemId}/move - move work item

---

## Sprint Summary

### Sprint 5.1: Core UI & Layout (40 hours)
- API Client Service
- Layout and Navigation
- Authentication UI
- Dashboard
- Error Pages
- CSS/JavaScript Setup
- Client Validation

### Sprint 5.2: Documentation UI (50 hours)
- Documents Controller
- Document Views (List, Details, Create/Edit)
- Markdown Editor with Live Preview
- Version History
- Tag Management
- Markdown Libraries Integration

### Sprint 5.3: Planning UI (60 hours)
- Projects Controller and Views
- Epics Controller and Views
- Work Items Controller and Views (with advanced search)
- Status Management UI
- Comments Component
- User Selection Components
- Badge Components

### Sprint 5.4: Kanban Board UI (50 hours)
- Board Controller
- Board View Layout
- Drag-and-Drop Functionality
- Board Filtering
- Quick Edit Modal
- Column Configuration
- Auto-Refresh (optional)
- Loading and Error States

### Sprint 5.5: Integration & Polish (40 hours)
- End-to-End Testing
- Browser Compatibility
- Performance Optimization
- Accessibility Audit
- UI Polish

**Total Estimated Effort**: 240 hours (6 weeks at 40 hours/week)

---

## Risk Assessment

### High-Risk Items

1. **Drag-and-Drop Complexity** (Task 5.4.3)
   - Risk: Browser compatibility issues, mobile touch support
   - Mitigation: Use well-tested library (SortableJS), extensive testing

2. **Markdown Editor Performance** (Task 5.2.5)
   - Risk: Large documents may cause lag in live preview
   - Mitigation: Debounce preview updates, test with large documents

3. **API Integration Errors** (All tasks)
   - Risk: API changes or errors break frontend
   - Mitigation: Comprehensive error handling, API contract tests

### Medium-Risk Items

1. **Browser Compatibility** (All tasks)
   - Risk: Features work differently across browsers
   - Mitigation: Test on multiple browsers, use polyfills

2. **Authentication Token Management** (Task 5.1.3, 5.1.7)
   - Risk: Token expiration, security vulnerabilities
   - Mitigation: Secure cookie storage, token refresh logic

3. **Performance with Large Datasets** (Multiple tasks)
   - Risk: Slow page loads with many items
   - Mitigation: Pagination, lazy loading, virtualization

---

## Success Criteria

Phase 5 is complete when:

- [ ] All 8 controllers implemented with actions
- [ ] All views created and functional
- [ ] Authentication and authorization working correctly
- [ ] Document management fully functional (CRUD, markdown, versioning)
- [ ] Project/Epic/Work Item management fully functional
- [ ] Advanced search and filtering working
- [ ] Kanban board with drag-and-drop operational
- [ ] All forms have client-side validation
- [ ] Responsive design works on desktop, tablet, mobile
- [ ] Integration tests pass (end-to-end scenarios)
- [ ] Browser compatibility tested (Chrome, Firefox, Edge, Safari)
- [ ] Zero build warnings or errors
- [ ] Code review completed
- [ ] Documentation updated

---

## Next Steps After Phase 5

**Phase 6: Polish & Optimization**
- Performance benchmarking and optimization
- Security audit
- Load testing
- API documentation (Swagger)
- User guide and developer documentation
- Deployment guide

**Post-MVP Enhancements**:
- Real-time collaboration (SignalR)
- Email notifications
- Advanced reporting and analytics
- Export functionality (PDF, CSV)
- Google OAuth integration
- Webhooks for external integrations

---

**Last Updated**: 2025-11-01
**Status**: Ready for Development
