using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public required string Email { get; set; }

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public required string Password { get; set; }

    [Required]
    [MaxLength(100)]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(100)]
    public required string LastName { get; set; }
}
