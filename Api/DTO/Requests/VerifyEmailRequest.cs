using System.ComponentModel.DataAnnotations;

namespace Api.DTO.Requests;

public record VerifyEmailRequest
{
    [Required] public string Email { get; set; }

    [Required] public string Code { get; set; }
}