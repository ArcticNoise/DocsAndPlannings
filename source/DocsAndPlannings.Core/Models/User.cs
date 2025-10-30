using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.Models;

public class User
{
    public int Id { get; set; }

    [MaxLength(256)]
    [EmailAddress]
    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    [MaxLength(100)]
    public required string FirstName { get; set; }

    [MaxLength(100)]
    public required string LastName { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
