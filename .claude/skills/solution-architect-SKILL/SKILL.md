---
name: solution-architect-SKILL
description: Research, analyze, and create comprehensive implementation guidelines for complex technical problems. Use for architectural decisions, technology selection, system design, evaluating trade-offs, and creating detailed implementation roadmaps.
---

# Solution Architect Skill

## When to Use This Skill

Use this skill when you need to research, analyze, and create comprehensive implementation guidelines for complex technical problems. This includes:

- Implementing new features requiring architectural decisions
- Solving complex technical problems with multiple approaches
- Making technology selection decisions
- Designing system architecture or major components
- Planning significant refactoring efforts
- Establishing technical patterns and standards
- Evaluating trade-offs between different solutions
- Creating detailed implementation roadmaps

**Key Indicators**: User asks "what's the best way to...", "how should I implement...", "design a solution for...", "research approaches for...", or needs guidance on complex technical decisions.

## Core Architect Responsibilities

### 1. Information Gathering

Begin by thoroughly understanding the problem through strategic questioning.

**Essential Questions Framework**:

```
PROBLEM DEFINITION
├─ What is the core problem or goal?
├─ What triggered this need?
├─ What does success look like?
└─ What are the must-have vs nice-to-have requirements?

TECHNICAL CONTEXT
├─ What is the current technology stack?
│  ├─ Languages and versions (.NET 8, C++17)
│  ├─ Frameworks (ECS, ASP.NET, React)
│  ├─ Infrastructure (cloud, on-premise)
│  └─ Development tools (IDE, CI/CD)
├─ What is the existing architecture?
│  ├─ Monolith, microservices, serverless?
│  ├─ Database (SQL, NoSQL, both?)
│  └─ Integration points
└─ What are the technical constraints?
   ├─ Must use existing infrastructure?
   ├─ Language/framework restrictions?
   └─ Legacy system integration requirements?

REQUIREMENTS
├─ Functional Requirements
│  ├─ Core features needed
│  ├─ User workflows
│  └─ Business logic rules
├─ Non-Functional Requirements
│  ├─ Performance (latency, throughput)
│  ├─ Scalability (users, data volume)
│  ├─ Reliability (uptime, error rates)
│  ├─ Security (auth, encryption, compliance)
│  └─ Maintainability (code quality, testability)
└─ Success Criteria
   ├─ Measurable metrics
   ├─ Acceptance criteria
   └─ Definition of done

CONSTRAINTS
├─ Budget (development, infrastructure, licenses)
├─ Timeline (deadlines, phases)
├─ Team (size, expertise, availability)
├─ Regulatory (GDPR, HIPAA, industry-specific)
└─ Business (compatibility, migration, support)

CONTEXT
├─ Who are the users?
├─ What is the scale (current and projected)?
├─ What is the business criticality?
└─ What are the integration touchpoints?
```

**Information Gathering Template**:

```markdown
## Project Context

**Problem Statement**: [1-2 sentences describing core issue]

**Goals**:
- Primary: [Main objective]
- Secondary: [Additional benefits]

**Current State**:
- Technology: [Stack description]
- Architecture: [Current design]
- Pain Points: [What's not working]

**Requirements**:

**Functional**:
- [ ] Feature 1
- [ ] Feature 2
- [ ] Feature 3

**Non-Functional**:
- Performance: [Targets]
- Scale: [Current/expected users, data]
- Security: [Requirements]
- Availability: [Uptime needs]

**Constraints**:
- Budget: [Amount or "limited"]
- Timeline: [Deadline]
- Team: [Size, skills]
- Technical: [Must use X, can't use Y]
- Business: [Must integrate with Z]

**Success Criteria**:
- [ ] Criterion 1: [Measurable]
- [ ] Criterion 2: [Measurable]
- [ ] Criterion 3: [Measurable]

**Open Questions**:
1. [Question needing clarification]
2. [Another question]
```

### 2. Comprehensive Research

Investigate multiple solution approaches systematically.

**Research Methodology**:

```
STEP 1: IDENTIFY SOLUTION SPACE
├─ Brainstorm 5-7 potential approaches
├─ Include both conventional and novel solutions
├─ Consider build vs buy options
└─ Range from simple to complex

STEP 2: RESEARCH EACH APPROACH
For each approach:
├─ How does it work? (architecture, components)
├─ What are the benefits? (advantages, strengths)
├─ What are the drawbacks? (limitations, weaknesses)
├─ What's the complexity? (implementation, maintenance)
├─ What's the cost? (development, infrastructure, licenses)
├─ What's the maturity? (proven, emerging, experimental)
├─ Community & support? (docs, examples, help available)
└─ Who else uses it? (case studies, success stories)

STEP 3: DEEP DIVE ON TOP CANDIDATES
For top 2-3 approaches:
├─ Study official documentation
├─ Review architecture patterns
├─ Analyze performance characteristics
├─ Check compatibility with stack
├─ Review security considerations
├─ Assess learning curve
└─ Evaluate long-term viability
```

**Research Documentation Template**:

```markdown
## Solution Research

### Approach 1: [Name]

**Overview**: [1-paragraph description]

**Architecture**:
```
[High-level architecture diagram in text/ASCII]
```

**Components**:
- Component A: [Purpose]
- Component B: [Purpose]
- Component C: [Purpose]

**Implementation Complexity**: [Low/Medium/High]
**Learning Curve**: [Low/Medium/High]
**Maturity**: [Experimental/Emerging/Proven]

**Pros**:
- ✅ Advantage 1
- ✅ Advantage 2
- ✅ Advantage 3

**Cons**:
- ❌ Limitation 1
- ❌ Limitation 2
- ❌ Limitation 3

**Technology Stack**:
- [Library/Framework 1] (v1.2.3)
- [Library/Framework 2] (v4.5.6)

**Cost Estimate**:
- Development: [X hours/weeks]
- Infrastructure: [$/month]
- Licenses: [$X one-time or $Y/month]

**Community & Support**:
- Documentation: [Excellent/Good/Fair/Poor]
- Community Size: [Large/Medium/Small]
- Active Development: [Yes/No]
- Commercial Support: [Available/Not available]

**Case Studies**:
- Company A: [How they used it, results]
- Company B: [How they used it, results]

**References**:
- [Official documentation URL]
- [Tutorial URL]
- [Case study URL]

---

[Repeat for Approach 2, Approach 3, etc.]
```

**Example Research - Real-Time Notifications**:

```markdown
## Approach 1: WebSockets with SignalR (.NET)

**Overview**: Use ASP.NET Core SignalR for real-time bidirectional communication over WebSockets, with automatic fallback to long-polling for older browsers.

**Architecture**:
```
Browser (SignalR Client)
    ↕ WebSocket (wss://)
SignalR Hub (ASP.NET Core)
    ↕
Redis Backplane (for scale-out)
    ↕
Multiple Server Instances
```

**Components**:
- **SignalR Hub**: Server-side endpoint for client connections
- **SignalR Client**: JavaScript/TypeScript library for browsers
- **Redis Backplane**: Pub/sub for message distribution across servers
- **Connection Manager**: Tracks active connections

**Implementation Complexity**: Low-Medium
**Learning Curve**: Low (familiar .NET patterns)
**Maturity**: Proven (Microsoft-supported, 7+ years production use)

**Pros**:
- ✅ Native .NET integration (same stack as existing API)
- ✅ Automatic reconnection handling built-in
- ✅ Scales horizontally with Redis backplane
- ✅ Strong typing with C# DTOs
- ✅ Excellent documentation and examples
- ✅ Free and open-source

**Cons**:
- ❌ Requires persistent connections (may hit connection limits)
- ❌ Redis required for multi-server (additional infrastructure)
- ❌ WebSocket firewall issues in some enterprise networks
- ❌ Not ideal for very high-throughput scenarios (>100K connections)

**Technology Stack**:
- ASP.NET Core SignalR (v8.0)
- Redis (v7.0) for backplane
- JavaScript SignalR Client (v8.0)

**Cost Estimate**:
- Development: 2 weeks (hub + client integration)
- Infrastructure: $50/month (Redis hosting on Azure)
- Licenses: Free (MIT license)

**Community & Support**:
- Documentation: Excellent (Microsoft docs)
- Community: Large (.NET community)
- Active Development: Yes (Microsoft-maintained)
- Commercial Support: Available (Microsoft)

**Case Studies**:
- **Stack Overflow**: Uses SignalR for real-time notifications (10M+ users)
- **JetBrains**: Real-time collaboration in Rider IDE
- **Microsoft Teams**: Built on SignalR for chat

**References**:
- https://docs.microsoft.com/en-us/aspnet/core/signalr/
- https://learn.microsoft.com/en-us/aspnet/core/tutorials/signalr

**Performance Characteristics**:
- Latency: 10-50ms typical
- Throughput: 10K connections per server
- Message rate: 1K messages/sec per server

---

## Approach 2: Server-Sent Events (SSE)

**Overview**: Use HTTP Server-Sent Events for one-way server-to-client streaming, simpler than WebSockets but unidirectional.

**Architecture**:
```
Browser (EventSource API)
    ← SSE (text/event-stream)
SSE Endpoint (ASP.NET Core)
    ↕
Message Queue (RabbitMQ/Kafka)
    ↑
Application Events
```

**Implementation Complexity**: Low
**Learning Curve**: Very Low (simple HTTP)
**Maturity**: Proven (W3C standard, 10+ years)

**Pros**:
- ✅ Simpler than WebSockets (one-way, HTTP-based)
- ✅ Automatic reconnection (built into EventSource)
- ✅ Works through proxies/firewalls (standard HTTP)
- ✅ No special server infrastructure needed
- ✅ Easy to implement and debug

**Cons**:
- ❌ One-way only (server to client)
- ❌ Client-to-server requires separate HTTP requests
- ❌ Browser connection limits (6 per domain)
- ❌ Not supported in older IE (Edge is fine)
- ❌ Text-only (JSON encoding required)

**Technology Stack**:
- ASP.NET Core (v8.0) custom middleware
- EventSource API (browser native)
- Optional: RabbitMQ for event distribution

**Cost Estimate**:
- Development: 1 week
- Infrastructure: $0-30/month (optional message queue)
- Licenses: Free

**Best For**: Read-heavy scenarios (notifications, dashboards, live feeds)
**Not Ideal For**: Bidirectional communication (chat, collaboration)

---

## Approach 3: Polling with HTTP/2

**Overview**: Client polls server at intervals using HTTP/2 multiplexing for efficiency. Simple fallback approach.

**Implementation Complexity**: Very Low
**Learning Curve**: Very Low
**Maturity**: Proven

**Pros**:
- ✅ Simplest implementation (standard HTTP GET)
- ✅ No persistent connections (lower server load)
- ✅ Works everywhere (maximum compatibility)
- ✅ Easy to debug and monitor
- ✅ Stateless (scales easily)

**Cons**:
- ❌ Higher latency (poll interval delay)
- ❌ Unnecessary requests when no updates (wasteful)
- ❌ Not truly "real-time" (1-5 second delay typical)
- ❌ Higher bandwidth usage than push-based

**Best For**: Low-frequency updates, simple requirements, maximum compatibility
**Not Ideal For**: True real-time (<100ms latency), high-frequency updates

---

## Approach 4: Firebase Cloud Messaging (FCM)

**Overview**: Use Google's push notification service for mobile and web push notifications.

**Pros**:
- ✅ Free for unlimited notifications
- ✅ Handles connection management
- ✅ Works even when app closed (mobile)
- ✅ Highly scalable (Google infrastructure)

**Cons**:
- ❌ Third-party dependency (Google)
- ❌ Requires user permission (browser prompt)
- ❌ Limited to notifications (not for data sync)
- ❌ Privacy considerations (data through Google)

**Best For**: Push notifications to mobile apps, browser notifications
**Not Ideal For**: Real-time data synchronization, in-app messaging
```

### 3. Solution Evaluation

Apply rigorous analysis to select the best approach.

**Evaluation Framework**:

```markdown
## Solution Comparison Matrix

| Criteria | Weight | Approach 1 | Approach 2 | Approach 3 |
|----------|--------|------------|------------|------------|
| **Functional Fit** | 25% | 9/10 | 7/10 | 6/10 |
| **Performance** | 20% | 8/10 | 7/10 | 5/10 |
| **Complexity** | 15% | 7/10 | 9/10 | 10/10 |
| **Cost** | 15% | 8/10 | 9/10 | 10/10 |
| **Scalability** | 10% | 9/10 | 6/10 | 8/10 |
| **Maintainability** | 10% | 8/10 | 8/10 | 9/10 |
| **Risk** | 5% | 9/10 | 8/10 | 9/10 |
| **Weighted Score** | - | **8.0** | **7.8** | **7.5** |

**Scoring Guide**:
- 10: Excellent - exceeds requirements
- 8: Good - meets requirements well
- 6: Acceptable - meets minimum requirements
- 4: Poor - doesn't meet some requirements
- 2: Inadequate - significant gaps

---

## Trade-Off Analysis

### Approach 1 (SignalR) vs Approach 2 (SSE)

**SignalR Wins When**:
- ✅ Need bidirectional communication (chat, collaboration)
- ✅ Want strong typing and .NET integration
- ✅ Need to scale to high connection counts
- ✅ Low-latency is critical (<50ms)

**SSE Wins When**:
- ✅ Only need server-to-client push (notifications, feeds)
- ✅ Want simplicity (less moving parts)
- ✅ Need maximum firewall compatibility
- ✅ Updates are infrequent (every few seconds acceptable)

**Decision Factors**:
1. **Bidirectional required?** → SignalR
2. **One-way sufficient?** → SSE
3. **Scale >10K concurrent?** → SignalR with Redis
4. **Budget constrained?** → SSE (no backplane needed)

---

## Recommendation

**Selected Approach**: Approach 1 (SignalR)

**Rationale**:
1. **Best Functional Fit** (9/10): Supports bidirectional communication for chat feature planned in roadmap
2. **Proven Scalability** (9/10): Battle-tested at scale (Stack Overflow, Teams)
3. **Stack Alignment**: Native .NET integration, same technology family
4. **Future-Proof**: Supports future features (typing indicators, presence)
5. **Risk Mitigation**: Microsoft-supported, excellent documentation

**Trade-Offs Accepted**:
- Redis infrastructure cost ($50/mo) - acceptable given budget
- Persistent connections - manageable at target scale (5K users)

**Alternative for Consideration**:
- Start with SSE if bidirectional not immediately needed
- Migrate to SignalR when chat feature starts (3-6 months)
- This delays Redis cost and reduces initial complexity

**Final Recommendation**: 
Implement SignalR now to avoid future migration, complexity acceptable given team .NET expertise.
```

### 4. Implementation Guidelines Creation

Produce comprehensive, actionable implementation guides.

**Guideline Structure**:

```markdown
# Implementation Guide: [Solution Name]

## Executive Summary

**Solution**: [Name and brief description]
**Approach**: [High-level strategy]
**Timeline**: [Estimated duration]
**Team**: [Required roles and effort]

**Key Decisions**:
- Decision 1: [Choice made and why]
- Decision 2: [Choice made and why]
- Decision 3: [Choice made and why]

---

## Architecture Overview

### System Architecture

```
[Detailed architecture diagram in text/ASCII]

Components:
- Component A: [Purpose and responsibilities]
- Component B: [Purpose and responsibilities]
- Component C: [Purpose and responsibilities]

Data Flow:
1. [Step-by-step data flow]
2. [Through the system]
3. [To final destination]
```

### Technology Stack

| Layer | Technology | Version | Purpose |
|-------|------------|---------|---------|
| Frontend | React | 18.2 | UI components |
| Backend | ASP.NET Core | 8.0 | API and SignalR Hub |
| Real-time | SignalR | 8.0 | WebSocket communication |
| Cache | Redis | 7.0 | Backplane for scale-out |
| Database | PostgreSQL | 15 | Persistent storage |
| Hosting | Azure App Service | - | Application hosting |

### Design Patterns

- **Hub Pattern**: Centralized message routing (SignalR Hub)
- **Pub/Sub**: Redis for message distribution across servers
- **Repository Pattern**: Data access abstraction
- **DTO Pattern**: Strongly-typed message contracts

---

## Implementation Phases

### Phase 1: Foundation (Week 1)

**Goal**: Set up infrastructure and basic SignalR integration

**Tasks**:
1. **Set up Redis** (4h)
   - Provision Redis instance (Azure Cache for Redis or local Docker)
   - Configure connection string
   - Test connectivity

2. **Create SignalR Hub** (4h)
   - Add SignalR NuGet packages
   - Create NotificationHub class
   - Configure SignalR in Program.cs
   - Add Redis backplane

3. **Client Integration** (8h)
   - Install @microsoft/signalr npm package
   - Create SignalR service wrapper
   - Implement connection management
   - Add reconnection logic

**Deliverables**:
- [ ] Redis running and accessible
- [ ] SignalR Hub responding to connections
- [ ] Client successfully connects
- [ ] Basic "hello world" message works

**Testing**:
- Unit tests for hub methods
- Integration test for connection lifecycle
- Load test: 100 simultaneous connections

---

### Phase 2: Core Features (Week 2)

**Goal**: Implement notification delivery system

**Tasks**:
1. **Define Message Contracts** (2h)
   ```csharp
   public class NotificationMessage {
       public string Type { get; set; }
       public string Title { get; set; }
       public string Body { get; set; }
       public DateTime Timestamp { get; set; }
       public Dictionary<string, object> Data { get; set; }
   }
   ```

2. **Implement Hub Methods** (6h)
   ```csharp
   public class NotificationHub : Hub {
       public async Task SubscribeToUserNotifications(string userId) {
           await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
       }
       
       public async Task SendNotificationToUser(string userId, NotificationMessage message) {
           await Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", message);
       }
   }
   ```

3. **Client Notification Handler** (6h)
   ```typescript
   class NotificationService {
       private connection: HubConnection;
       
       async connect() {
           this.connection = new HubConnectionBuilder()
               .withUrl("/notifications")
               .withAutomaticReconnect()
               .build();
           
           this.connection.on("ReceiveNotification", (message) => {
               this.handleNotification(message);
           });
           
           await this.connection.start();
       }
       
       private handleNotification(message: NotificationMessage) {
           // Display toast, update UI, etc.
       }
   }
   ```

4. **Persistence Layer** (6h)
   - Create Notification entity
   - Implement repository
   - Store notifications in database
   - Query unread notifications

**Deliverables**:
- [ ] Strongly-typed message contracts
- [ ] Hub methods for send/receive
- [ ] Client displays notifications
- [ ] Notifications persisted to database

---

### Phase 3: Advanced Features (Week 3)

[Continue with additional phases...]

---

## Detailed Implementation Steps

### Step 1: Set Up Project Structure

**Create New Projects**:
```bash
# Backend
dotnet new webapi -n MyApp.Api
cd MyApp.Api
dotnet add package Microsoft.AspNetCore.SignalR
dotnet add package StackExchange.Redis

# Create Hub
mkdir Hubs
touch Hubs/NotificationHub.cs
```

**Project Structure**:
```
MyApp.Api/
├── Controllers/
│   └── NotificationsController.cs
├── Hubs/
│   └── NotificationHub.cs
├── Models/
│   └── NotificationMessage.cs
├── Services/
│   ├── INotificationService.cs
│   └── NotificationService.cs
└── Program.cs
```

### Step 2: Configure SignalR

**Program.cs Configuration**:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSignalR()
    .AddStackExchangeRedis(connectionString, options => {
        options.Configuration.ChannelPrefix = "MyApp";
    });

builder.Services.AddSingleton<INotificationService, NotificationService>();

var app = builder.Build();

// Configure middleware
app.MapHub<NotificationHub>("/notifications");

app.Run();
```

**appsettings.json**:
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "SignalR": {
    "MaximumReceiveMessageSize": 32768,
    "KeepAliveInterval": "00:00:15",
    "ClientTimeoutInterval": "00:00:30"
  }
}
```

### Step 3: Implement NotificationHub

```csharp
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

[Authorize]  // Require authentication
public class NotificationHub : Hub {
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationHub> _logger;
    
    public NotificationHub(
        INotificationService notificationService,
        ILogger<NotificationHub> logger) {
        _notificationService = notificationService;
        _logger = logger;
    }
    
    public override async Task OnConnectedAsync() {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null) {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", 
                userId, Context.ConnectionId);
            
            // Send unread notifications
            var unread = await _notificationService.GetUnreadNotificationsAsync(userId);
            await Clients.Caller.SendAsync("UnreadNotifications", unread);
        }
        
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception) {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null) {
            _logger.LogInformation("User {UserId} disconnected", userId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task MarkAsRead(string notificationId) {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null) {
            await _notificationService.MarkAsReadAsync(userId, notificationId);
        }
    }
}
```

### Step 4: Implement Client Service

**TypeScript Client (React)**:
```typescript
import * as signalR from '@microsoft/signalr';

export interface NotificationMessage {
    id: string;
    type: string;
    title: string;
    body: string;
    timestamp: Date;
    data?: Record<string, any>;
}

export class NotificationService {
    private connection: signalR.HubConnection;
    private listeners: ((notification: NotificationMessage) => void)[] = [];
    
    constructor(hubUrl: string, accessToken: string) {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {
                accessTokenFactory: () => accessToken
            })
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: (retryContext) => {
                    // Exponential backoff: 0, 2, 10, 30 seconds
                    if (retryContext.previousRetryCount === 0) return 0;
                    if (retryContext.previousRetryCount === 1) return 2000;
                    if (retryContext.previousRetryCount === 2) return 10000;
                    return 30000;
                }
            })
            .configureLogging(signalR.LogLevel.Information)
            .build();
        
        this.setupHandlers();
    }
    
    private setupHandlers() {
        this.connection.on('ReceiveNotification', (notification: NotificationMessage) => {
            this.listeners.forEach(listener => listener(notification));
        });
        
        this.connection.onreconnecting((error) => {
            console.warn('Connection lost, reconnecting...', error);
        });
        
        this.connection.onreconnected((connectionId) => {
            console.info('Reconnected with ID:', connectionId);
        });
        
        this.connection.onclose((error) => {
            console.error('Connection closed:', error);
        });
    }
    
    async start(): Promise<void> {
        try {
            await this.connection.start();
            console.info('SignalR connected');
        } catch (error) {
            console.error('Failed to connect:', error);
            throw error;
        }
    }
    
    async stop(): Promise<void> {
        await this.connection.stop();
    }
    
    subscribe(listener: (notification: NotificationMessage) => void): () => void {
        this.listeners.push(listener);
        // Return unsubscribe function
        return () => {
            const index = this.listeners.indexOf(listener);
            if (index > -1) {
                this.listeners.splice(index, 1);
            }
        };
    }
    
    async markAsRead(notificationId: string): Promise<void> {
        await this.connection.invoke('MarkAsRead', notificationId);
    }
}
```

**React Hook**:
```typescript
import { useEffect, useState } from 'react';
import { NotificationService, NotificationMessage } from './NotificationService';

export function useNotifications(hubUrl: string, accessToken: string) {
    const [notifications, setNotifications] = useState<NotificationMessage[]>([]);
    const [isConnected, setIsConnected] = useState(false);
    const [service] = useState(() => new NotificationService(hubUrl, accessToken));
    
    useEffect(() => {
        const unsubscribe = service.subscribe((notification) => {
            setNotifications(prev => [notification, ...prev]);
        });
        
        service.start()
            .then(() => setIsConnected(true))
            .catch((error) => console.error('Connection failed:', error));
        
        return () => {
            unsubscribe();
            service.stop();
        };
    }, [service]);
    
    const markAsRead = async (notificationId: string) => {
        await service.markAsRead(notificationId);
        setNotifications(prev => 
            prev.map(n => n.id === notificationId ? {...n, read: true} : n)
        );
    };
    
    return { notifications, isConnected, markAsRead };
}

// Usage in component
function NotificationPanel() {
    const { notifications, isConnected, markAsRead } = useNotifications(
        '/notifications',
        authToken
    );
    
    return (
        <div>
            {!isConnected && <div>Connecting...</div>}
            {notifications.map(n => (
                <NotificationItem 
                    key={n.id} 
                    notification={n}
                    onMarkRead={() => markAsRead(n.id)}
                />
            ))}
        </div>
    );
}
```

---

## Configuration Guide

### Production Configuration

**appsettings.Production.json**:
```json
{
  "ConnectionStrings": {
    "Redis": "${REDIS_CONNECTION_STRING}"
  },
  "SignalR": {
    "MaximumReceiveMessageSize": 32768,
    "KeepAliveInterval": "00:00:15",
    "ClientTimeoutInterval": "00:00:30",
    "MaximumParallelInvocationsPerClient": 1
  },
  "Logging": {
    "LogLevel": {
      "Microsoft.AspNetCore.SignalR": "Warning",
      "Microsoft.AspNetCore.Http.Connections": "Warning"
    }
  }
}
```

### Scaling Configuration

**Azure App Service**:
- Enable "Always On" to prevent cold starts
- Scale out to 2+ instances for high availability
- Use Azure Cache for Redis (Standard C1 minimum)

**Redis Configuration**:
```
# redis.conf for production
maxmemory-policy allkeys-lru
timeout 0
tcp-keepalive 300
```

---

## Testing Strategy

### Unit Tests

```csharp
public class NotificationHubTests {
    [Fact]
    public async Task OnConnectedAsync_AddsUserToGroup() {
        // Arrange
        var hub = new NotificationHub(mockService, mockLogger);
        var context = CreateMockContext(userId: "123");
        hub.Context = context;
        
        // Act
        await hub.OnConnectedAsync();
        
        // Assert
        mockGroups.Verify(g => g.AddToGroupAsync(
            It.IsAny<string>(), 
            "user-123", 
            default), Times.Once);
    }
}
```

### Integration Tests

```csharp
[Collection("SignalR")]
public class NotificationIntegrationTests : IAsyncLifetime {
    private readonly WebApplicationFactory<Program> _factory;
    private HubConnection _connection;
    
    public async Task InitializeAsync() {
        _connection = new HubConnectionBuilder()
            .WithUrl($"{_factory.Server.BaseAddress}notifications")
            .Build();
        await _connection.StartAsync();
    }
    
    [Fact]
    public async Task CanSendAndReceiveNotification() {
        // Arrange
        NotificationMessage received = null;
        _connection.On<NotificationMessage>("ReceiveNotification", msg => {
            received = msg;
        });
        
        // Act
        await _connection.InvokeAsync("SendNotification", new NotificationMessage {
            Type = "test",
            Title = "Test",
            Body = "Test notification"
        });
        
        // Assert
        await Task.Delay(100);  // Allow time for delivery
        Assert.NotNull(received);
        Assert.Equal("test", received.Type);
    }
}
```

### Load Tests

```csharp
// Using NBomber for load testing
var scenario = Scenario.Create("signalr_notifications", async context => {
    var connection = new HubConnectionBuilder()
        .WithUrl("https://myapp.com/notifications")
        .Build();
    
    await connection.StartAsync();
    
    var response = await connection.InvokeAsync<bool>(
        "SendNotification", 
        testMessage
    );
    
    return response 
        ? Response.Ok() 
        : Response.Fail();
});

var simulation = Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromMinutes(5));

NBomberRunner
    .RegisterScenarios(scenario)
    .WithWorkerPlugins(new PingPlugin())
    .LoadSimulation(simulation)
    .Run();
```

**Load Test Targets**:
- 1,000 concurrent connections
- 100 messages/second throughput
- <100ms p95 latency
- <1% error rate

---

## Deployment Plan

### CI/CD Pipeline

**GitHub Actions Workflow**:
```yaml
name: Deploy SignalR App

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore -c Release
      
      - name: Test
        run: dotnet test --no-build -c Release
      
      - name: Publish
        run: dotnet publish -c Release -o ./publish
      
      - name: Deploy to Azure
        uses: azure/webapps-deploy@v2
        with:
          app-name: myapp-signalr
          publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
          package: ./publish
```

### Deployment Steps

1. **Pre-Deployment**:
   ```bash
   # Run tests
   dotnet test
   
   # Build release
   dotnet publish -c Release
   
   # Verify configuration
   # Check Redis connectivity
   # Review scaling settings
   ```

2. **Deployment**:
   ```bash
   # Deploy to staging
   az webapp deployment source config-zip \
     --resource-group myapp-rg \
     --name myapp-staging \
     --src ./publish.zip
   
   # Smoke test staging
   curl https://myapp-staging.azurewebsites.net/health
   
   # Deploy to production (blue-green)
   az webapp deployment slot swap \
     --resource-group myapp-rg \
     --name myapp \
     --slot staging \
     --target-slot production
   ```

3. **Post-Deployment**:
   ```bash
   # Verify health
   # Monitor metrics
   # Check error logs
   # Test notifications
   ```

### Rollback Plan

```bash
# If issues detected, swap back
az webapp deployment slot swap \
  --resource-group myapp-rg \
  --name myapp \
  --slot production \
  --target-slot staging

# Or rollback to previous version
az webapp deployment source show \
  --resource-group myapp-rg \
  --name myapp

az webapp deployment source config-zip \
  --resource-group myapp-rg \
  --name myapp \
  --src ./previous-version.zip
```

---

## Monitoring & Observability

### Metrics to Track

**Connection Metrics**:
- Active connections count
- Connection rate (connects/sec)
- Disconnection rate
- Average connection duration
- Reconnection attempts

**Message Metrics**:
- Messages sent/received per second
- Message delivery latency (p50, p95, p99)
- Failed message deliveries
- Message queue depth

**Resource Metrics**:
- CPU usage
- Memory usage
- Redis memory usage
- Network bandwidth

### Application Insights Setup

```csharp
builder.Services.AddApplicationInsightsTelemetry(options => {
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});

// Custom metrics
public class NotificationHub : Hub {
    private readonly TelemetryClient _telemetry;
    
    public override async Task OnConnectedAsync() {
        _telemetry.TrackEvent("SignalR_Connected", new Dictionary<string, string> {
            ["UserId"] = Context.User?.Identity?.Name,
            ["ConnectionId"] = Context.ConnectionId
        });
        
        _telemetry.GetMetric("SignalR_ActiveConnections").TrackValue(1);
        
        await base.OnConnectedAsync();
    }
}
```

### Logging Strategy

```csharp
builder.Services.AddLogging(logging => {
    logging.AddConsole();
    logging.AddDebug();
    logging.AddApplicationInsights();
    
    // SignalR specific logging
    logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Information);
    logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Warning);
});

// Structured logging
_logger.LogInformation(
    "Notification sent: {NotificationId} to user {UserId} at {Timestamp}",
    notification.Id,
    userId,
    DateTime.UtcNow
);
```

### Alerting Rules

**Azure Monitor Alerts**:
- Connection failures >5% → Critical
- Average latency >500ms → Warning
- Redis memory >80% → Warning
- Error rate >1% → Critical
- Active connections >8,000 (scale trigger) → Info

---

## Security Considerations

### Authentication & Authorization

```csharp
// Require authentication
[Authorize]
public class NotificationHub : Hub { }

// Claim-based authorization
[Authorize(Policy = "NotificationAccess")]
public class NotificationHub : Hub { }

// Configure authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            // ... JWT validation settings
        };
        
        // Allow SignalR to use JWT from query string
        options.Events = new JwtBearerEvents {
            OnMessageReceived = context => {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                
                if (!string.IsNullOrEmpty(accessToken) && 
                    path.StartsWithSegments("/notifications")) {
                    context.Token = accessToken;
                }
                
                return Task.CompletedTask;
            }
        };
    });
```

### Input Validation

```csharp
public async Task SendNotification(NotificationMessage message) {
    // Validate input
    if (string.IsNullOrWhiteSpace(message.Title)) {
        throw new HubException("Title is required");
    }
    
    if (message.Title.Length > 100) {
        throw new HubException("Title too long");
    }
    
    // Sanitize HTML
    message.Body = HtmlSanitizer.Sanitize(message.Body);
    
    // Rate limiting
    var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (await _rateLimiter.IsRateLimitExceeded(userId)) {
        throw new HubException("Rate limit exceeded");
    }
    
    await Clients.User(userId).SendAsync("ReceiveNotification", message);
}
```

### Data Protection

```csharp
// Enable HTTPS only
builder.Services.AddSignalR(options => {
    options.EnableDetailedErrors = false;  // Don't leak info in production
    options.MaximumReceiveMessageSize = 32 * 1024;  // 32KB limit
});

// CORS configuration
builder.Services.AddCors(options => {
    options.AddPolicy("SignalRCors", policy => {
        policy.WithOrigins("https://myapp.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

app.UseCors("SignalRCors");
```

---

## Performance Optimization

### Connection Pooling

```csharp
// Redis connection pooling
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => {
    var configuration = ConfigurationOptions.Parse(connectionString);
    configuration.AbortOnConnectFail = false;
    configuration.ConnectTimeout = 5000;
    configuration.SyncTimeout = 5000;
    return ConnectionMultiplexer.Connect(configuration);
});
```

### Message Batching

```csharp
// Batch multiple notifications
public class NotificationService {
    private readonly Channel<NotificationMessage> _queue;
    
    public async Task QueueNotification(string userId, NotificationMessage message) {
        await _queue.Writer.WriteAsync(message);
    }
    
    private async Task ProcessQueue() {
        var batch = new List<NotificationMessage>();
        
        await foreach (var message in _queue.Reader.ReadAllAsync()) {
            batch.Add(message);
            
            if (batch.Count >= 10 || /* timeout */) {
                await SendBatch(batch);
                batch.Clear();
            }
        }
    }
}
```

### Caching Strategy

```csharp
// Cache user preferences
private readonly IMemoryCache _cache;

public async Task<UserPreferences> GetUserPreferences(string userId) {
    return await _cache.GetOrCreateAsync($"prefs:{userId}", async entry => {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
        return await _database.GetUserPreferencesAsync(userId);
    });
}
```

---

## Maintenance Guidelines

### Regular Tasks

**Daily**:
- Monitor error logs
- Check Redis memory usage
- Review connection metrics
- Verify alert system

**Weekly**:
- Review performance trends
- Analyze slow operations
- Check for memory leaks
- Update dependencies (security patches)

**Monthly**:
- Review and optimize queries
- Update NuGet packages
- Review and update monitoring thresholds
- Capacity planning review

### Troubleshooting Guide

**Issue: High Connection Failures**
```
Symptoms: Connection error rate >5%
Possible Causes:
1. Redis connectivity issues
2. Server overload
3. Network problems
4. Authentication failures

Investigation:
- Check Redis status: redis-cli ping
- Check server CPU/memory
- Review authentication logs
- Test from different networks

Resolution:
- Scale Redis tier
- Add more app instances
- Fix authentication config
- Check firewall rules
```

**Issue: High Latency**
```
Symptoms: Message delivery >500ms
Possible Causes:
1. Redis backplane slow
2. Too many subscribers per group
3. Inefficient hub methods
4. Database queries in hub

Investigation:
- Profile hub methods
- Check Redis latency
- Review group sizes
- Analyze database queries

Resolution:
- Optimize Redis configuration
- Restructure groups
- Move heavy work to background
- Add caching layer
```

---

## Documentation Requirements

### API Documentation

Use XML comments for public APIs:
```csharp
/// <summary>
/// Hub for real-time notification delivery
/// </summary>
public class NotificationHub : Hub {
    /// <summary>
    /// Marks a notification as read
    /// </summary>
    /// <param name="notificationId">The notification ID</param>
    /// <returns>Task representing the asynchronous operation</returns>
    /// <exception cref="HubException">Thrown when notification not found or access denied</exception>
    public async Task MarkAsRead(string notificationId) {
        // Implementation
    }
}
```

### Architecture Documentation

Maintain docs/architecture.md:
- System overview diagram
- Component responsibilities
- Data flow diagrams
- Scaling strategy
- Disaster recovery plan

### Runbook

Create ops/runbook.md:
- Deployment procedures
- Rollback procedures
- Common issues and solutions
- Monitoring dashboard links
- On-call escalation process

---

## Success Criteria

**Functional Requirements** ✓
- [ ] Real-time notification delivery (<100ms)
- [ ] Automatic reconnection on disconnect
- [ ] Persistent notifications (survive disconnect)
- [ ] Read/unread tracking
- [ ] User presence indicators (optional v2)

**Non-Functional Requirements** ✓
- [ ] Support 5,000 concurrent connections
- [ ] 99.9% uptime SLA
- [ ] <100ms p95 latency
- [ ] Horizontal scalability proven
- [ ] Security audit passed

**Quality Gates** ✓
- [ ] Unit test coverage >80%
- [ ] Integration tests passing
- [ ] Load test targets met
- [ ] Security scan passed
- [ ] Performance benchmarks met

---

## Conclusion

This implementation guide provides a complete path from setup through production deployment for a SignalR-based real-time notification system. Follow phases sequentially, validate each milestone, and maintain documentation throughout.

**Next Steps**:
1. Review and approve this guide
2. Set up development environment (Phase 1, Week 1)
3. Schedule sprint planning
4. Begin implementation

**Questions or Concerns**:
[Space for team to add notes during review]
```

## Best Practices

### Make Guidelines Actionable

❌ **Vague**: "Set up SignalR"
✅ **Specific**: "Run `dotnet add package Microsoft.AspNetCore.SignalR`, create `Hubs/NotificationHub.cs`, configure in `Program.cs` line 23"

### Provide Code Examples

Every architectural decision should have accompanying code:
- Configuration snippets
- Implementation templates
- Test examples
- Deployment scripts

### Explain the "Why"

Don't just say what to do, explain why:
```
❌ "Use Redis backplane"
✅ "Use Redis backplane to enable horizontal scaling - without it, messages only reach connections on the same server instance"
```

### Include Error Scenarios

```
Common Issues:

**Redis Connection Fails**
Error: "StackExchange.Redis.RedisConnectionException"
Cause: Redis not accessible or wrong connection string
Solution: 
1. Verify Redis is running: `redis-cli ping`
2. Check connection string in appsettings.json
3. Ensure firewall allows port 6379
```

## When to Escalate

Create an "Options Analysis" instead of a recommendation when:
- Multiple equally valid approaches exist
- Trade-offs depend on unstated preferences
- Decision requires business context you lack
- Technical constraints are ambiguous

**Present options with**:
- Clear pros/cons for each
- Decision factors (what matters)
- Recommendation if one approach is clearly better
- Defer decision to stakeholders if unclear

## Self-Verification Checklist

Before delivering implementation guidelines:

- [ ] Problem thoroughly understood (all questions answered)
- [ ] Multiple approaches researched (3-5 alternatives)
- [ ] Trade-offs clearly explained
- [ ] Recommendation well-justified
- [ ] Implementation guide is complete (all phases covered)
- [ ] Code examples are accurate and tested
- [ ] Configuration provided for all environments
- [ ] Testing strategy defined
- [ ] Deployment plan included
- [ ] Monitoring and observability addressed
- [ ] Security considerations covered
- [ ] Maintenance guidelines provided
- [ ] Success criteria measurable
- [ ] Common issues and solutions documented

## Final Reminders

### Core Principles

1. **Thoroughness**: Research deeply, document completely
2. **Clarity**: Write for the implementer, not just the architect
3. **Practicality**: Every step should be implementable
4. **Justification**: Explain decisions with rationale
5. **Completeness**: Cover full lifecycle (dev → prod → maintenance)

### You Are Successful When

- ✅ Development team can start implementing immediately
- ✅ All technical decisions are justified
- ✅ Implementation path is clear and unambiguous
- ✅ Edge cases and errors are anticipated
- ✅ Testing and deployment are planned
- ✅ Long-term maintenance is considered
- ✅ Success criteria are measurable

Your role is to bridge the gap between "what to build" and "how to build it", providing the development team with everything they need to execute successfully. A great implementation guide should inspire confidence and provide a clear path forward.
