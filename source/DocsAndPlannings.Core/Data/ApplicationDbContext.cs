using DocsAndPlannings.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentVersion> DocumentVersions { get; set; }
    public DbSet<DocumentTag> DocumentTags { get; set; }
    public DbSet<DocumentTagMap> DocumentTagMaps { get; set; }
    public DbSet<DocumentAttachment> DocumentAttachments { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Epic> Epics { get; set; }
    public DbSet<WorkItem> WorkItems { get; set; }
    public DbSet<WorkItemComment> WorkItemComments { get; set; }
    public DbSet<Status> Statuses { get; set; }
    public DbSet<StatusTransition> StatusTransitions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUserEntities(modelBuilder);
        ConfigureDocumentEntities(modelBuilder);
        ConfigurePlanningEntities(modelBuilder);
        ConfigureStatusEntities(modelBuilder);
    }

    private void ConfigureUserEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.Property(e => e.AssignedAt).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureDocumentEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CurrentVersion).IsRequired().HasDefaultValue(1);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.IsPublished).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

            entity.HasOne(e => e.Author)
                .WithMany()
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ParentDocument)
                .WithMany(d => d.ChildDocuments)
                .HasForeignKey(e => e.ParentDocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DocumentVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VersionNumber).IsRequired();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.ChangeDescription).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasIndex(e => new { e.DocumentId, e.VersionNumber }).IsUnique();

            entity.HasOne(e => e.Document)
                .WithMany(d => d.Versions)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.ModifiedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DocumentTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        });

        modelBuilder.Entity<DocumentTagMap>(entity =>
        {
            entity.HasKey(e => new { e.DocumentId, e.TagId });
            entity.Property(e => e.AssignedAt).IsRequired();

            entity.HasOne(e => e.Document)
                .WithMany(d => d.Tags)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                .WithMany(t => t.Documents)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DocumentAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.StoredFileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FileSizeBytes).IsRequired();
            entity.Property(e => e.UploadedAt).IsRequired();

            entity.HasOne(e => e.Document)
                .WithMany(d => d.Attachments)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UploadedBy)
                .WithMany()
                .HasForeignKey(e => e.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigurePlanningEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(10);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.IsArchived).IsRequired().HasDefaultValue(false);

            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Epic>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Summary).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Priority).IsRequired().HasDefaultValue(3);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Project)
                .WithMany(p => p.Epics)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Assignee)
                .WithMany()
                .HasForeignKey(e => e.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Status)
                .WithMany(s => s.Epics)
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Summary).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Priority).IsRequired().HasDefaultValue(3);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Project)
                .WithMany(p => p.WorkItems)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Epic)
                .WithMany(ep => ep.WorkItems)
                .HasForeignKey(e => e.EpicId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ParentWorkItem)
                .WithMany(w => w.ChildWorkItems)
                .HasForeignKey(e => e.ParentWorkItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Assignee)
                .WithMany()
                .HasForeignKey(e => e.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Reporter)
                .WithMany()
                .HasForeignKey(e => e.ReporterId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Status)
                .WithMany(s => s.WorkItems)
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkItemComment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.IsEdited).IsRequired().HasDefaultValue(false);

            entity.HasOne(e => e.WorkItem)
                .WithMany(w => w.Comments)
                .HasForeignKey(e => e.WorkItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Author)
                .WithMany()
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureStatusEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.OrderIndex).IsRequired();
            entity.Property(e => e.IsDefaultForNew).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.IsCompletedStatus).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.IsCancelledStatus).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        });

        modelBuilder.Entity<StatusTransition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IsAllowed).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasIndex(e => new { e.FromStatusId, e.ToStatusId }).IsUnique();

            entity.HasOne(e => e.FromStatus)
                .WithMany(s => s.TransitionsFrom)
                .HasForeignKey(e => e.FromStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ToStatus)
                .WithMany(s => s.TransitionsTo)
                .HasForeignKey(e => e.ToStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
