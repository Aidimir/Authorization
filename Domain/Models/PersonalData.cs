namespace Domain.Models;

public class PersonalData
{
    public string Login { get; init; }

    public DateOnly? BirthDate { get; init; }

    public string? PhoneNumber { get; init; }
}