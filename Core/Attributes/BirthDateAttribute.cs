using System.ComponentModel.DataAnnotations;

namespace Core.Validators;

public class BirthDateAttribute : ValidationAttribute
{
    public int MinAge { get; set; }

    public override bool IsValid(object? value)
    {
        if (value is null)
            return true;

        var val = value as DateOnly? ?? throw new ValidationException("Birth Date is invalid");

        if (val.AddYears(MinAge) <= DateOnly.FromDateTime(DateTime.Today))
            return true;

        return false;
    }
}