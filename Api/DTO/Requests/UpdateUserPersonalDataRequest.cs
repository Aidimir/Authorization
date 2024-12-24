using System.ComponentModel.DataAnnotations;
using Core.Validators;

namespace Api.DTO.Requests;

public record UpdateUserPersonalDataRequest
{
    [Required]
    [MinLength(6, ErrorMessage = "Username must be at least 6 characters long.")]
    [MaxLength(100, ErrorMessage = "Username must be no more than 100 characters long.")]
    public string Login { get; init; }

    [BirthDate(MinAge = 0, ErrorMessage = "Birth date can't be in the future")]
    public DateOnly? BirthDate { get; init; }

    [DataType(DataType.PhoneNumber)] public string? PhoneNumber { get; init; }
}