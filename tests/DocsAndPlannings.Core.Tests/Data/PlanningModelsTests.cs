using Microsoft.EntityFrameworkCore;
using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.Models;

namespace DocsAndPlannings.Core.Tests.Data;

public class PlanningModelsTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public PlanningModelsTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public void CanCreateProject()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = new Project
        {
            Key = "TEST",
            Name = "Test Project",
            Description = "A test project",
            OwnerId = owner.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        int result = _context.SaveChanges();

        Assert.Equal(1, result);
        Assert.True(project.Id > 0);
        Assert.True(project.IsActive);
        Assert.False(project.IsArchived);
    }

    [Fact]
    public void CanCreateEpicInProject()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = new Project
        {
            Key = "TEST",
            Name = "Test Project",
            OwnerId = owner.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Status status = CreateTestStatus("TODO", 1);

        _context.Projects.Add(project);
        _context.Statuses.Add(status);
        _context.SaveChanges();

        Epic epic = new Epic
        {
            ProjectId = project.Id,
            Key = "TEST-EPIC-1",
            Summary = "Test Epic",
            Description = "Epic description",
            StatusId = status.Id,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Epics.Add(epic);
        int result = _context.SaveChanges();

        Assert.Equal(1, result);
        Assert.True(epic.Id > 0);
        Assert.Equal(3, epic.Priority);
    }

    [Fact]
    public void CanCreateWorkItemWithDifferentTypes()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = new Project
        {
            Key = "TEST",
            Name = "Test Project",
            OwnerId = owner.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Status status = CreateTestStatus("TODO", 1);

        _context.Projects.Add(project);
        _context.Statuses.Add(status);
        _context.SaveChanges();

        WorkItem task = new WorkItem
        {
            ProjectId = project.Id,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Test Task",
            StatusId = status.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        WorkItem bug = new WorkItem
        {
            ProjectId = project.Id,
            Key = "TEST-2",
            Type = WorkItemType.Bug,
            Summary = "Test Bug",
            StatusId = status.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WorkItems.AddRange(task, bug);
        int result = _context.SaveChanges();

        Assert.Equal(2, result);
        Assert.Equal(WorkItemType.Task, task.Type);
        Assert.Equal(WorkItemType.Bug, bug.Type);
    }

    [Fact]
    public void CanCreateWorkItemSubtask()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = new Project
        {
            Key = "TEST",
            Name = "Test Project",
            OwnerId = owner.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Status status = CreateTestStatus("TODO", 1);

        _context.Projects.Add(project);
        _context.Statuses.Add(status);
        _context.SaveChanges();

        WorkItem parent = new WorkItem
        {
            ProjectId = project.Id,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Parent Task",
            StatusId = status.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WorkItems.Add(parent);
        _context.SaveChanges();

        WorkItem subtask = new WorkItem
        {
            ProjectId = project.Id,
            ParentWorkItemId = parent.Id,
            Key = "TEST-2",
            Type = WorkItemType.Subtask,
            Summary = "Subtask",
            StatusId = status.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WorkItems.Add(subtask);
        int result = _context.SaveChanges();

        Assert.Equal(1, result);
        Assert.Equal(parent.Id, subtask.ParentWorkItemId);
    }

    [Fact]
    public void CanAssignWorkItemToUser()
    {
        User owner = CreateTestUser("owner@example.com");
        User assignee = CreateTestUser("assignee@example.com");
        _context.Users.AddRange(owner, assignee);
        _context.SaveChanges();

        Project project = new Project
        {
            Key = "TEST",
            Name = "Test Project",
            OwnerId = owner.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Status status = CreateTestStatus("TODO", 1);

        _context.Projects.Add(project);
        _context.Statuses.Add(status);
        _context.SaveChanges();

        WorkItem workItem = new WorkItem
        {
            ProjectId = project.Id,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Assigned Task",
            StatusId = status.Id,
            AssigneeId = assignee.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WorkItems.Add(workItem);
        int result = _context.SaveChanges();

        Assert.Equal(1, result);
        Assert.Equal(assignee.Id, workItem.AssigneeId);
    }

    [Fact]
    public void CanAddCommentToWorkItem()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = new Project
        {
            Key = "TEST",
            Name = "Test Project",
            OwnerId = owner.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Status status = CreateTestStatus("TODO", 1);

        _context.Projects.Add(project);
        _context.Statuses.Add(status);
        _context.SaveChanges();

        WorkItem workItem = new WorkItem
        {
            ProjectId = project.Id,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Task with comment",
            StatusId = status.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WorkItems.Add(workItem);
        _context.SaveChanges();

        WorkItemComment comment = new WorkItemComment
        {
            WorkItemId = workItem.Id,
            AuthorId = owner.Id,
            Content = "This is a comment",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WorkItemComments.Add(comment);
        int result = _context.SaveChanges();

        Assert.Equal(1, result);
        Assert.False(comment.IsEdited);
    }

    [Fact]
    public void DeletingProjectCascadesDeleteEpicsAndWorkItems()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = new Project
        {
            Key = "TEST",
            Name = "Test Project",
            OwnerId = owner.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Status status = CreateTestStatus("TODO", 1);

        _context.Projects.Add(project);
        _context.Statuses.Add(status);
        _context.SaveChanges();

        Epic epic = new Epic
        {
            ProjectId = project.Id,
            Key = "TEST-EPIC-1",
            Summary = "Test Epic",
            StatusId = status.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        WorkItem workItem = new WorkItem
        {
            ProjectId = project.Id,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Test Task",
            StatusId = status.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Epics.Add(epic);
        _context.WorkItems.Add(workItem);
        _context.SaveChanges();

        int projectId = project.Id;

        _context.Projects.Remove(project);
        _context.SaveChanges();

        Epic? deletedEpic = _context.Epics.FirstOrDefault(e => e.ProjectId == projectId);
        WorkItem? deletedWorkItem = _context.WorkItems.FirstOrDefault(w => w.ProjectId == projectId);

        Assert.Null(deletedEpic);
        Assert.Null(deletedWorkItem);
    }

    private User CreateTestUser(string email)
    {
        return new User
        {
            Email = email,
            PasswordHash = "hash123",
            FirstName = "Test",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private Status CreateTestStatus(string name, int orderIndex)
    {
        return new Status
        {
            Name = name,
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
