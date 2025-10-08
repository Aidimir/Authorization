using Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace Db.Models;

public class UserEntity : IdentityUser
{
    public UserEntity(User user)
    {
        Email = user.Email;
        UserName = user.Login;
        PhoneNumber = user.PhoneNumber;
        BirthDate = user.BirthDate;
    }

    public UserEntity()
    {
    }

    public DateOnly? BirthDate { get; set; }
    public required IEnumerable<IdentityRole> Roles { get; set; }
    
    public string TelegramId { get; set; }
    public string TelegramFirstName { get; set; }
    public string TelegramLastName { get; set; }
    public string TelegramUsername { get; set; }
    public string TelegramPhotoUrl { get; set; }

    public void UpdatePersonalData(PersonalData personalData)
    {
        UserName = personalData.Login;
        PhoneNumber = personalData.PhoneNumber;
        BirthDate = personalData.BirthDate;
    }

    public User ToDomainEntity()
    {
        return new User
        {
            Email = Email,
            Login = UserName,
            Password = null,
            BirthDate = BirthDate,
            PhoneNumber = PhoneNumber,
            RoleIds = Roles.Select(r => Guid.Parse(r.Id)).ToList()
        };
    }
}