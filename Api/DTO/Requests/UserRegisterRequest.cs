using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core.Validators;
using Microsoft.AspNetCore.Mvc;

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
    [DefaultValue("1qaz2wsxaA@")]
    public string Password { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [DefaultValue("1qaz2wsxaA@")]
    public string PasswordConfirm { get; set; }

    [BirthDate(MinAge = 0, ErrorMessage = "Birth date can't be in the future")]
    // [DataType(DataType.DateTime)]
    [DefaultValue("03-10-2003")]
    public DateOnly BirthDate { get; set; }

    [DefaultValue("+79999999999")]
    [DataType(DataType.PhoneNumber)] public string? PhoneNumber { get; set; }
    
    [DefaultValue(null)]
    public IEnumerable<Guid>? RoleIds { get; set; }
}