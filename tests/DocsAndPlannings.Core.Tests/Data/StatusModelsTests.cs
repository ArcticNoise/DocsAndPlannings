using Microsoft.EntityFrameworkCore;
using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.Models;

namespace DocsAndPlannings.Core.Tests.Data;

public class StatusModelsTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public StatusModelsTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public void CanCreateStatus()
    {
        Status status = new Status
        {
            Name = "TODO",
            Color = "#CCCCCC",
            OrderIndex = 1,
            IsDefaultForNew = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Statuses.Add(status);
        int result = _context.SaveChanges();

        Assert.Equal(1, result);
        Assert.True(status.Id > 0);
        Assert.True(status.IsActive);
        Assert.False(status.IsCompletedStatus);
        Assert.False(status.IsCancelledStatus);
    }

    [Fact]
    public void CanCreateCompletedStatus()
    {
        Status status = new Status
        {
            Name = "DONE",
            OrderIndex = 4,
            IsCompletedStatus = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Statuses.Add(status);
        int result = _context.SaveChanges();

        Assert.Equal(1, result);
        Assert.True(status.IsCompletedStatus);
    }

    [Fact]
    public void CanCreateStatusTransition()
    {
        Status todo = new Status
        {
            Name = "TODO",
            OrderIndex = 1,
            CreatedAt = DateTime.UtcNow
        };

        Status inProgress = new Status
        {
            Name = "IN PROGRESS",
            OrderIndex = 2,
            CreatedAt = DateTime.UtcNow
        };

        _context.Statuses.AddRange(todo, inProgress);
        _context.SaveChanges();

        StatusTransition transition = new StatusTransition
        {
            FromStatusId = todo.Id,
            ToStatusId = inProgress.Id,
            IsAllowed = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.StatusTransitions.Add(transition);
        int result = _context.SaveChanges();

        Assert.Equal(1, result);
        Assert.True(transition.IsAllowed);
    }

    [Fact]
    public void CanQueryAllowedTransitions()
    {
        Status todo = new Status
        {
            Name = "TODO",
            OrderIndex = 1,
            CreatedAt = DateTime.UtcNow
        };

        Status inProgress = new Status
        {
            Name = "IN PROGRESS",
            OrderIndex = 2,
            CreatedAt = DateTime.UtcNow
        };

        Status done = new Status
        {
            Name = "DONE",
            OrderIndex = 3,
            CreatedAt = DateTime.UtcNow
        };

        _context.Statuses.AddRange(todo, inProgress, done);
        _context.SaveChanges();

        _context.StatusTransitions.AddRange(
            new StatusTransition { FromStatusId = todo.Id, ToStatusId = inProgress.Id, CreatedAt = DateTime.UtcNow },
            new StatusTransition { FromStatusId = inProgress.Id, ToStatusId = done.Id, CreatedAt = DateTime.UtcNow },
            new StatusTransition { FromStatusId = inProgress.Id, ToStatusId = todo.Id, IsAllowed = false, CreatedAt = DateTime.UtcNow }
        );
        _context.SaveChanges();

        Status? foundStatus = _context.Statuses
            .Include(s => s.TransitionsFrom)
            .FirstOrDefault(s => s.Name == "IN PROGRESS");

        Assert.NotNull(foundStatus);
        Assert.Equal(2, foundStatus.TransitionsFrom.Count);

        StatusTransition? allowedTransition = foundStatus.TransitionsFrom.FirstOrDefault(t => t.ToStatusId == done.Id);
        StatusTransition? notAllowedTransition = foundStatus.TransitionsFrom.FirstOrDefault(t => t.ToStatusId == todo.Id);

        Assert.NotNull(allowedTransition);
        Assert.True(allowedTransition.IsAllowed);

        Assert.NotNull(notAllowedTransition);
        Assert.False(notAllowedTransition.IsAllowed);
    }

    [Fact]
    public void CanQueryStatusesByOrder()
    {
        _context.Statuses.AddRange(
            new Status { Name = "DONE", OrderIndex = 3, CreatedAt = DateTime.UtcNow },
            new Status { Name = "TODO", OrderIndex = 1, CreatedAt = DateTime.UtcNow },
            new Status { Name = "IN PROGRESS", OrderIndex = 2, CreatedAt = DateTime.UtcNow }
        );
        _context.SaveChanges();

        List<Status> orderedStatuses = _context.Statuses.OrderBy(s => s.OrderIndex).ToList();

        Assert.Equal(3, orderedStatuses.Count);
        Assert.Equal("TODO", orderedStatuses[0].Name);
        Assert.Equal("IN PROGRESS", orderedStatuses[1].Name);
        Assert.Equal("DONE", orderedStatuses[2].Name);
    }

    [Fact]
    public void StatusNameMaxLengthIsConfiguredCorrectly()
    {
        Microsoft.EntityFrameworkCore.Metadata.IEntityType? model = _context.Model.FindEntityType(typeof(Status));
        Assert.NotNull(model);

        Microsoft.EntityFrameworkCore.Metadata.IProperty? nameProperty = model.FindProperty(nameof(Status.Name));
        Assert.NotNull(nameProperty);

        Assert.Equal(50, nameProperty.GetMaxLength());
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
