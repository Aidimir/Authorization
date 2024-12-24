using System.ComponentModel.DataAnnotations;
using Core.Validators;

namespace Api.DTO.Requests;

public record UserRegisterRequest
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; }

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Login { get; set; }

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    [RegularExpression(@"^.*(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*\(\)_\-+=]).*$",
        ErrorMessage = "Requirement is minimum 1 special char and 1 digit and 1 Uppercase letter")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string PasswordConfirm { get; set; }

    [BirthDate(MinAge = 0, ErrorMessage = "Birth date can't be in the future")]
    public DateOnly BirthDate { get; set; }

    [DataType(DataType.PhoneNumber)] public string? PhoneNumber { get; set; }

    public IEnumerable<Guid>? RoleIds { get; set; }
}