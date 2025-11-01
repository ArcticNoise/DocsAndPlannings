using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.Models;
using DocsAndPlannings.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Tests.Services;

public sealed class KeyGenerationServiceTests : IDisposable
{
    private readonly ApplicationDbContext m_Context;
    private readonly IKeyGenerationService m_KeyGenerationService;

    public KeyGenerationServiceTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        m_Context = new ApplicationDbContext(options);
        m_KeyGenerationService = new KeyGenerationService(m_Context);

        SeedTestData();
    }

    public void Dispose()
    {
        m_Context.Dispose();
    }

    private void SeedTestData()
    {
        Project project1 = new Project
        {
            Id = 1,
            Name = "Test Project",
            Key = "TEST",
            Description = "Test project",
            OwnerId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Project project2 = new Project
        {
            Id = 2,
            Name = "Another Project",
            Key = "PROJ",
            Description = "Another project",
            OwnerId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.Projects.AddRange(project1, project2);
        m_Context.SaveChanges();
    }

    [Fact]
    public async Task GenerateEpicKeyAsync_FirstEpic_GeneratesEpic1()
    {
        string result = await m_KeyGenerationService.GenerateEpicKeyAsync(1);

        Assert.Equal("TEST-EPIC-1", result);
    }

    [Fact]
    public async Task GenerateEpicKeyAsync_SubsequentEpic_IncrementsNumber()
    {
        Epic epic1 = new Epic
        {
            ProjectId = 1,
            Key = "TEST-EPIC-1",
            Summary = "Epic 1",
            StatusId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.Epics.Add(epic1);
        await m_Context.SaveChangesAsync();

        string result = await m_KeyGenerationService.GenerateEpicKeyAsync(1);

        Assert.Equal("TEST-EPIC-2", result);
    }

    [Fact]
    public async Task GenerateEpicKeyAsync_WithGapsInNumbering_UsesMaxPlusOne()
    {
        Epic epic1 = new Epic
        {
            ProjectId = 1,
            Key = "TEST-EPIC-1",
            Summary = "Epic 1",
            StatusId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Epic epic5 = new Epic
        {
            ProjectId = 1,
            Key = "TEST-EPIC-5",
            Summary = "Epic 5",
            StatusId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.Epics.AddRange(epic1, epic5);
        await m_Context.SaveChangesAsync();

        string result = await m_KeyGenerationService.GenerateEpicKeyAsync(1);

        Assert.Equal("TEST-EPIC-6", result);
    }

    [Fact]
    public async Task GenerateEpicKeyAsync_DifferentProject_GeneratesIndependentKey()
    {
        Epic epic1 = new Epic
        {
            ProjectId = 1,
            Key = "TEST-EPIC-1",
            Summary = "Epic 1",
            StatusId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Epic epic2 = new Epic
        {
            ProjectId = 1,
            Key = "TEST-EPIC-2",
            Summary = "Epic 2",
            StatusId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.Epics.AddRange(epic1, epic2);
        await m_Context.SaveChangesAsync();

        string result = await m_KeyGenerationService.GenerateEpicKeyAsync(2);

        Assert.Equal("PROJ-EPIC-1", result); // Independent counter for project 2
    }

    [Fact]
    public async Task GenerateEpicKeyAsync_NonExistentProject_ThrowsInvalidOperationException()
    {
        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await m_KeyGenerationService.GenerateEpicKeyAsync(999));

        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task GenerateWorkItemKeyAsync_FirstWorkItem_Generates1()
    {
        string result = await m_KeyGenerationService.GenerateWorkItemKeyAsync(1);

        Assert.Equal("TEST-1", result);
    }

    [Fact]
    public async Task GenerateWorkItemKeyAsync_SubsequentWorkItem_IncrementsNumber()
    {
        WorkItem workItem1 = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Work Item 1",
            StatusId = 1,
            ReporterId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.WorkItems.Add(workItem1);
        await m_Context.SaveChangesAsync();

        string result = await m_KeyGenerationService.GenerateWorkItemKeyAsync(1);

        Assert.Equal("TEST-2", result);
    }

    [Fact]
    public async Task GenerateWorkItemKeyAsync_WithGapsInNumbering_UsesMaxPlusOne()
    {
        WorkItem workItem1 = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Work Item 1",
            StatusId = 1,
            ReporterId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        WorkItem workItem10 = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-10",
            Type = WorkItemType.Bug,
            Summary = "Work Item 10",
            StatusId = 1,
            ReporterId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.WorkItems.AddRange(workItem1, workItem10);
        await m_Context.SaveChangesAsync();

        string result = await m_KeyGenerationService.GenerateWorkItemKeyAsync(1);

        Assert.Equal("TEST-11", result);
    }

    [Fact]
    public async Task GenerateWorkItemKeyAsync_DifferentProject_GeneratesIndependentKey()
    {
        WorkItem workItem1 = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Work Item 1",
            StatusId = 1,
            ReporterId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        WorkItem workItem2 = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-2",
            Type = WorkItemType.Task,
            Summary = "Work Item 2",
            StatusId = 1,
            ReporterId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.WorkItems.AddRange(workItem1, workItem2);
        await m_Context.SaveChangesAsync();

        string result = await m_KeyGenerationService.GenerateWorkItemKeyAsync(2);

        Assert.Equal("PROJ-1", result); // Independent counter for project 2
    }

    [Fact]
    public async Task GenerateWorkItemKeyAsync_NonExistentProject_ThrowsInvalidOperationException()
    {
        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await m_KeyGenerationService.GenerateWorkItemKeyAsync(999));

        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task GenerateEpicKeyAsync_WithNonNumericSuffix_IgnoresInvalidKeys()
    {
        Epic epic1 = new Epic
        {
            ProjectId = 1,
            Key = "TEST-EPIC-1",
            Summary = "Epic 1",
            StatusId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Epic epicInvalid = new Epic
        {
            ProjectId = 1,
            Key = "TEST-EPIC-ABC", // Invalid numeric suffix
            Summary = "Epic Invalid",
            StatusId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.Epics.AddRange(epic1, epicInvalid);
        await m_Context.SaveChangesAsync();

        string result = await m_KeyGenerationService.GenerateEpicKeyAsync(1);

        Assert.Equal("TEST-EPIC-2", result); // Ignores ABC, continues from 1
    }

    [Fact]
    public async Task GenerateWorkItemKeyAsync_WithNonNumericSuffix_IgnoresInvalidKeys()
    {
        WorkItem workItem1 = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Work Item 1",
            StatusId = 1,
            ReporterId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        WorkItem workItemInvalid = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-XYZ", // Invalid numeric suffix
            Type = WorkItemType.Task,
            Summary = "Work Item Invalid",
            StatusId = 1,
            ReporterId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.WorkItems.AddRange(workItem1, workItemInvalid);
        await m_Context.SaveChangesAsync();

        string result = await m_KeyGenerationService.GenerateWorkItemKeyAsync(1);

        Assert.Equal("TEST-2", result); // Ignores XYZ, continues from 1
    }
}
