using System.ComponentModel.DataAnnotations;

namespace Api.DTO.Requests;

public record UserAuthRequest
{
    [Required] [MaxLength(100)] public string Login { get; set; }

    public string? Password { get; set; }
    public string? RefreshToken { get; set; }
}