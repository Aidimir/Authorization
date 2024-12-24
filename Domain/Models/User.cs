namespace Domain.Models;

public class User
{
    public required string Email { get; set; }
    public required string Login { get; set; }
    public required string Password { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? PhoneNumber { get; set; }

    public IEnumerable<Guid> RoleIds { get; set; } = new List<Guid>();
}