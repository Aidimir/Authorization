using System.Security.Authentication;
using Db;
using Db.Models;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Logic;

public interface IUsersService
{
    public Task<User> CreateUserAsync(User user);
    public Task<User> CreateTelegramUserAsync(User user, TelegramUser telegramUser);
    public Task UpdateUserPersonalDataAsync(string username, PersonalData personalData);
    public Task RemoveUserAsync(string username);
    public Task<User?> FindUserAsync(string? id = null, string? email = null, string? username = null);
    public Task<UserEntity?> FindUserByTelegramIdAsync(string telegramId);

    public Task<(bool success, UserEntity? userEntity)> CheckUsersPasswordAsync(string login, string password);
    public Task<UserEntity> FindEntityByLoginAsync(string login);
}

public class UsersService : IUsersService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<UserEntity> _userManager;

    public UsersService(UserManager<UserEntity> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<User> CreateUserAsync(User user)
    {
        var newUser = new UserEntity(user)
        {
            Roles = await _roleManager.GetUserRolesByIdsAsync(user.RoleIds.Select(id => id.ToString()))
        };

        var result = await _userManager.CreateAsync(newUser);
        if (!result.Succeeded)
            throw new Exception(string.Join(",", result.Errors.Select(err => err.Description)));

        await _userManager.AddPasswordAsync(newUser, user.Password);
        return newUser.ToDomainEntity();
    }

    public async Task<User> CreateTelegramUserAsync(User user, TelegramUser telegramUser)
    {
        var newUser = new UserEntity(user)
        {
            Roles = await _roleManager.GetUserRolesByIdsAsync(user.RoleIds.Select(id => id.ToString())),
            TelegramId = telegramUser.TelegramId,
            TelegramFirstName = telegramUser.FirstName,
            TelegramLastName = telegramUser.LastName,
            TelegramUsername = telegramUser.Username,
            TelegramPhotoUrl = telegramUser.PhotoUrl
        };

        var result = await _userManager.CreateAsync(newUser);
        if (!result.Succeeded)
            throw new Exception(string.Join(",", result.Errors.Select(err => err.Description)));

        await _userManager.AddPasswordAsync(newUser, user.Password);
        return newUser.ToDomainEntity();
    }

    public async Task<UserEntity?> FindUserByTelegramIdAsync(string telegramId)
    {
        return await _userManager.Users
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId);
    }

    public async Task UpdateUserPersonalDataAsync(string username, PersonalData personalData)
    {
        var existedUser = await _userManager.FindByNameAsync(username);

        if (existedUser is null)
            throw new AuthenticationException("No user with such id");

        existedUser.UpdatePersonalData(personalData);

        await _userManager.UpdateAsync(existedUser);
    }

    public async Task RemoveUserAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
            throw new AuthenticationException("No such user");

        await _userManager.DeleteAsync(user);
    }

    public async Task<User?> FindUserAsync(string? id = null, string? email = null, string? username = null)
    {
        return (await FindUserEntity(id, email, username))?.ToDomainEntity();
    }

    public async Task<(bool success, UserEntity? userEntity)> CheckUsersPasswordAsync(string login,
        string? password)
    {
        var entity = await FindEntityByLoginAsync(login);

        var isSamePassword = await _userManager.CheckPasswordAsync(entity, password);
        return (isSamePassword, isSamePassword ? entity : null);
    }

    public async Task<UserEntity> FindEntityByLoginAsync(string login)
    {
        UserEntity? userEntiy;

        userEntiy = await _userManager.FindByEmailAsync(login) ?? await _userManager.FindByNameAsync(login);
        if (userEntiy is null)
            throw new AuthenticationException($"No user with such login: {login}");

        return userEntiy;
    }

    private async Task<UserEntity?> FindUserEntity(string? id = null, string? email = null, string? username = null)
    {
        UserEntity? entity;

        if (id is not null)
        {
            entity = await _userManager.FindByIdAsync(id) ?? throw new AuthenticationException("No user with such id");
            return entity;
        }

        if (email is not null)
        {
            entity = await _userManager.FindByEmailAsync(email);
            return entity;
        }

        if (username is not null)
        {
            entity = await _userManager.FindByNameAsync(username);
            return entity;
        }

        throw new ArgumentNullException(id, "All user search parameters are null");
    }
}