namespace DocsAndPlannings.Core.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
}
