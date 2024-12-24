namespace Domain.Models;

public record UserAuth
{
    public required string RefreshToken { get; set; }
    public required string AuthToken { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
}