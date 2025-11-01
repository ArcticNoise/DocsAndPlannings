using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Statuses;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Models;
using DocsAndPlannings.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Tests.Services;

public sealed class StatusServiceTests : IDisposable
{
    private readonly ApplicationDbContext m_Context;
    private readonly IStatusService m_StatusService;

    public StatusServiceTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        m_Context = new ApplicationDbContext(options);
        m_StatusService = new StatusService(m_Context);

        SeedTestData();
    }

    public void Dispose()
    {
        m_Context.Dispose();
    }

    private void SeedTestData()
    {
        Status status1 = new Status
        {
            Id = 1,
            Name = "TODO",
            Color = "#95a5a6",
            OrderIndex = 1,
            IsDefaultForNew = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        Status status2 = new Status
        {
            Id = 2,
            Name = "IN PROGRESS",
            Color = "#3498db",
            OrderIndex = 2,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        Status status3 = new Status
        {
            Id = 3,
            Name = "DONE",
            Color = "#2ecc71",
            OrderIndex = 3,
            IsCompletedStatus = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        Status inactiveStatus = new Status
        {
            Id = 4,
            Name = "ARCHIVED",
            Color = "#000000",
            OrderIndex = 99,
            CreatedAt = DateTime.UtcNow,
            IsActive = false
        };

        m_Context.Statuses.AddRange(status1, status2, status3, inactiveStatus);
        m_Context.SaveChanges();
    }

    [Fact]
    public async Task GetAllStatusesAsync_ReturnsActiveStatusesOrderedByIndex()
    {
        IReadOnlyList<StatusDto> result = await m_StatusService.GetAllStatusesAsync();

        Assert.NotNull(result);
        Assert.Equal(3, result.Count); // Only active statuses
        Assert.Equal("TODO", result[0].Name);
        Assert.Equal("IN PROGRESS", result[1].Name);
        Assert.Equal("DONE", result[2].Name);
    }

    [Fact]
    public async Task GetStatusByIdAsync_ExistingId_ReturnsStatus()
    {
        StatusDto? result = await m_StatusService.GetStatusByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("TODO", result.Name);
        Assert.Equal("#95a5a6", result.Color);
        Assert.True(result.IsDefaultForNew);
    }

    [Fact]
    public async Task GetStatusByIdAsync_NonExistentId_ReturnsNull()
    {
        StatusDto? result = await m_StatusService.GetStatusByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateStatusAsync_ValidRequest_CreatesStatus()
    {
        CreateStatusRequest request = new CreateStatusRequest
        {
            Name = "REVIEW",
            Color = "#ff9900",
            OrderIndex = 5,
            IsDefaultForNew = false,
            IsCompletedStatus = false,
            IsCancelledStatus = false
        };

        StatusDto result = await m_StatusService.CreateStatusAsync(request);

        Assert.NotNull(result);
        Assert.Equal("REVIEW", result.Name);
        Assert.Equal("#ff9900", result.Color);
        Assert.Equal(5, result.OrderIndex);
        Assert.True(result.IsActive);

        Status? dbStatus = await m_Context.Statuses.FindAsync(result.Id);
        Assert.NotNull(dbStatus);
        Assert.Equal("REVIEW", dbStatus.Name);
    }

    [Fact]
    public async Task CreateStatusAsync_NullRequest_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await m_StatusService.CreateStatusAsync(null!));
    }

    [Fact]
    public async Task CreateStatusAsync_DuplicateName_ThrowsBadRequestException()
    {
        CreateStatusRequest request = new CreateStatusRequest
        {
            Name = "TODO", // Duplicate
            Color = "#000000",
            OrderIndex = 10
        };

        BadRequestException ex = await Assert.ThrowsAsync<BadRequestException>(
            async () => await m_StatusService.CreateStatusAsync(request));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task UpdateStatusAsync_ValidRequest_UpdatesStatus()
    {
        UpdateStatusRequest request = new UpdateStatusRequest
        {
            Name = "TO DO (Updated)",
            Color = "#ffffff",
            OrderIndex = 10,
            IsDefaultForNew = false,
            IsCompletedStatus = false,
            IsCancelledStatus = false,
            IsActive = true
        };

        StatusDto result = await m_StatusService.UpdateStatusAsync(1, request);

        Assert.NotNull(result);
        Assert.Equal("TO DO (Updated)", result.Name);
        Assert.Equal("#ffffff", result.Color);
        Assert.Equal(10, result.OrderIndex);
        Assert.False(result.IsDefaultForNew);

        Status? dbStatus = await m_Context.Statuses.FindAsync(1);
        Assert.NotNull(dbStatus);
        Assert.Equal("TO DO (Updated)", dbStatus.Name);
    }

    [Fact]
    public async Task UpdateStatusAsync_NullRequest_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await m_StatusService.UpdateStatusAsync(1, null!));
    }

    [Fact]
    public async Task UpdateStatusAsync_NonExistentId_ThrowsNotFoundException()
    {
        UpdateStatusRequest request = new UpdateStatusRequest
        {
            Name = "TEST",
            Color = "#000000",
            OrderIndex = 1
        };

        NotFoundException ex = await Assert.ThrowsAsync<NotFoundException>(
            async () => await m_StatusService.UpdateStatusAsync(999, request));

        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task UpdateStatusAsync_DuplicateName_ThrowsBadRequestException()
    {
        UpdateStatusRequest request = new UpdateStatusRequest
        {
            Name = "IN PROGRESS", // Name of status 2
            Color = "#000000",
            OrderIndex = 1
        };

        BadRequestException ex = await Assert.ThrowsAsync<BadRequestException>(
            async () => await m_StatusService.UpdateStatusAsync(1, request));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task DeleteStatusAsync_UnusedStatus_DeletesSuccessfully()
    {
        await m_StatusService.DeleteStatusAsync(3);

        Status? dbStatus = await m_Context.Statuses.FindAsync(3);
        Assert.Null(dbStatus);
    }

    [Fact]
    public async Task DeleteStatusAsync_NonExistentId_ThrowsNotFoundException()
    {
        NotFoundException ex = await Assert.ThrowsAsync<NotFoundException>(
            async () => await m_StatusService.DeleteStatusAsync(999));

        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task DeleteStatusAsync_StatusInUse_ThrowsBadRequestException()
    {
        // Create a project that uses status 1
        Project project = new Project
        {
            Name = "Test Project",
            Key = "TEST",
            Description = "Test",
            OwnerId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        m_Context.Projects.Add(project);
        await m_Context.SaveChangesAsync();

        Epic epic = new Epic
        {
            ProjectId = project.Id,
            Key = "TEST-EPIC-1",
            Summary = "Test Epic",
            StatusId = 1, // Uses status 1
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        m_Context.Epics.Add(epic);
        await m_Context.SaveChangesAsync();

        BadRequestException ex = await Assert.ThrowsAsync<BadRequestException>(
            async () => await m_StatusService.DeleteStatusAsync(1));

        Assert.Contains("in use", ex.Message);
    }

    [Fact]
    public async Task ValidateTransitionAsync_SameStatus_ReturnsTrue()
    {
        bool result = await m_StatusService.ValidateTransitionAsync(1, 1);

        Assert.True(result);
    }

    [Fact]
    public async Task ValidateTransitionAsync_NoExplicitRule_ReturnsTrueByDefault()
    {
        bool result = await m_StatusService.ValidateTransitionAsync(1, 2);

        Assert.True(result); // Permissive by default
    }

    [Fact]
    public async Task ValidateTransitionAsync_AllowedTransition_ReturnsTrue()
    {
        StatusTransition transition = new StatusTransition
        {
            FromStatusId = 1,
            ToStatusId = 2,
            IsAllowed = true,
            CreatedAt = DateTime.UtcNow
        };
        m_Context.StatusTransitions.Add(transition);
        await m_Context.SaveChangesAsync();

        bool result = await m_StatusService.ValidateTransitionAsync(1, 2);

        Assert.True(result);
    }

    [Fact]
    public async Task ValidateTransitionAsync_DisallowedTransition_ReturnsFalse()
    {
        StatusTransition transition = new StatusTransition
        {
            FromStatusId = 3,
            ToStatusId = 1,
            IsAllowed = false,
            CreatedAt = DateTime.UtcNow
        };
        m_Context.StatusTransitions.Add(transition);
        await m_Context.SaveChangesAsync();

        bool result = await m_StatusService.ValidateTransitionAsync(3, 1);

        Assert.False(result);
    }

    [Fact]
    public async Task GetAllowedTransitionsAsync_ReturnsAllowedStatuses()
    {
        StatusTransition transition1 = new StatusTransition
        {
            FromStatusId = 1,
            ToStatusId = 2,
            IsAllowed = true,
            CreatedAt = DateTime.UtcNow
        };

        StatusTransition transition2 = new StatusTransition
        {
            FromStatusId = 1,
            ToStatusId = 3,
            IsAllowed = true,
            CreatedAt = DateTime.UtcNow
        };

        StatusTransition transition3 = new StatusTransition
        {
            FromStatusId = 1,
            ToStatusId = 4,
            IsAllowed = false, // Not allowed
            CreatedAt = DateTime.UtcNow
        };

        m_Context.StatusTransitions.AddRange(transition1, transition2, transition3);
        await m_Context.SaveChangesAsync();

        IReadOnlyList<StatusDto> result = await m_StatusService.GetAllowedTransitionsAsync(1);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Only allowed transitions
        Assert.Contains(result, s => s.Id == 2);
        Assert.Contains(result, s => s.Id == 3);
        Assert.DoesNotContain(result, s => s.Id == 4);
    }

    [Fact]
    public async Task GetAllowedTransitionsAsync_NoTransitions_ReturnsEmptyList()
    {
        IReadOnlyList<StatusDto> result = await m_StatusService.GetAllowedTransitionsAsync(1);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateStatusTransitionAsync_ValidRequest_CreatesTransition()
    {
        CreateStatusTransitionRequest request = new CreateStatusTransitionRequest
        {
            FromStatusId = 1,
            ToStatusId = 2,
            IsAllowed = true
        };

        StatusTransitionDto result = await m_StatusService.CreateStatusTransitionAsync(request);

        Assert.NotNull(result);
        Assert.Equal(1, result.FromStatusId);
        Assert.Equal("TODO", result.FromStatusName);
        Assert.Equal(2, result.ToStatusId);
        Assert.Equal("IN PROGRESS", result.ToStatusName);
        Assert.True(result.IsAllowed);

        StatusTransition? dbTransition = await m_Context.StatusTransitions.FindAsync(result.Id);
        Assert.NotNull(dbTransition);
    }

    [Fact]
    public async Task CreateStatusTransitionAsync_NullRequest_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await m_StatusService.CreateStatusTransitionAsync(null!));
    }

    [Fact]
    public async Task CreateStatusTransitionAsync_NonExistentFromStatus_ThrowsNotFoundException()
    {
        CreateStatusTransitionRequest request = new CreateStatusTransitionRequest
        {
            FromStatusId = 999,
            ToStatusId = 2,
            IsAllowed = true
        };

        NotFoundException ex = await Assert.ThrowsAsync<NotFoundException>(
            async () => await m_StatusService.CreateStatusTransitionAsync(request));

        Assert.Contains("Source status", ex.Message);
    }

    [Fact]
    public async Task CreateStatusTransitionAsync_NonExistentToStatus_ThrowsNotFoundException()
    {
        CreateStatusTransitionRequest request = new CreateStatusTransitionRequest
        {
            FromStatusId = 1,
            ToStatusId = 999,
            IsAllowed = true
        };

        NotFoundException ex = await Assert.ThrowsAsync<NotFoundException>(
            async () => await m_StatusService.CreateStatusTransitionAsync(request));

        Assert.Contains("Target status", ex.Message);
    }

    [Fact]
    public async Task CreateStatusTransitionAsync_DuplicateTransition_ThrowsBadRequestException()
    {
        StatusTransition existing = new StatusTransition
        {
            FromStatusId = 1,
            ToStatusId = 2,
            IsAllowed = true,
            CreatedAt = DateTime.UtcNow
        };
        m_Context.StatusTransitions.Add(existing);
        await m_Context.SaveChangesAsync();

        CreateStatusTransitionRequest request = new CreateStatusTransitionRequest
        {
            FromStatusId = 1,
            ToStatusId = 2,
            IsAllowed = false
        };

        BadRequestException ex = await Assert.ThrowsAsync<BadRequestException>(
            async () => await m_StatusService.CreateStatusTransitionAsync(request));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task CreateDefaultStatusesAsync_EmptyDatabase_CreatesDefaultStatuses()
    {
        // Clear existing data
        m_Context.Statuses.RemoveRange(m_Context.Statuses);
        await m_Context.SaveChangesAsync();

        await m_StatusService.CreateDefaultStatusesAsync();

        List<Status> statuses = await m_Context.Statuses.ToListAsync();

        Assert.Equal(5, statuses.Count);
        Assert.Contains(statuses, s => s.Name == "TODO" && s.IsDefaultForNew);
        Assert.Contains(statuses, s => s.Name == "IN PROGRESS");
        Assert.Contains(statuses, s => s.Name == "DONE" && s.IsCompletedStatus);
        Assert.Contains(statuses, s => s.Name == "CANCELLED" && s.IsCancelledStatus);
        Assert.Contains(statuses, s => s.Name == "BACKLOG");
    }

    [Fact]
    public async Task CreateDefaultStatusesAsync_StatusesExist_DoesNotCreateDuplicates()
    {
        int initialCount = await m_Context.Statuses.CountAsync();

        await m_StatusService.CreateDefaultStatusesAsync();

        int finalCount = await m_Context.Statuses.CountAsync();

        Assert.Equal(initialCount, finalCount); // No new statuses created
    }
}
