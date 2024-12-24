namespace Api.DTO.Responses;

public record UserAuthResponse
{
    public required string RefreshToken { get; set; }
    public required string AuthToken { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
}