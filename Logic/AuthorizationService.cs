using System.Security.Authentication;
using Db.Models;
using Domain.Models;

namespace Logic;

public class TelegramUser
{
    public string TelegramId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public string PhotoUrl { get; set; }
}

public interface IAuthorizationService
{
    public Task<UserAuth> RegisterUser(User user);
    public Task<UserAuth> Auth(UserCredentials userCredentials);
    public Task<bool> CheckIsEmailNotTaken(string email);
    public Task<bool> ValidateToken(string token);
    
    // Telegram auth methods
    public Task<UserAuth> RegisterTelegramUser(TelegramUser telegramUser);
    public Task<UserAuth> AuthTelegramUser(string telegramId);
    public Task<bool> CheckIsTelegramIdNotTaken(string telegramId);
}

public class AuthorizationService : IAuthorizationService
{
    private readonly ITokenService _tokenService;
    private readonly IUsersService _usersService;


    public AuthorizationService(IUsersService usersService,
        ITokenService tokenService)
    {
        _usersService = usersService;
        _tokenService = tokenService;
    }

    public async Task<UserAuth> RegisterUser(User user)
    {
        await _usersService.CreateUserAsync(user);
        return await Auth(new UserCredentials(user.Login, user.Password, null));
    }

    public async Task<UserAuth> Auth(UserCredentials userCredentials)
    {
        if (userCredentials.RefreshToken is not null)
        {
            var validationResult = await _tokenService.ValidateToken(userCredentials.RefreshToken);
            if (validationResult.Success)
            {
                var userEntity = await _usersService.FindEntityByLoginAsync(userCredentials.Login);
                return await CreateResponse(userEntity);
            }

            throw new AuthenticationException("Invalid refresh token");
        }

        var checkUsersCredentials = await _usersService.CheckUsersPasswordAsync(userCredentials.Login,
            userCredentials.Password);

        if (checkUsersCredentials.success)
            return await CreateResponse(checkUsersCredentials.userEntity!);

        async Task<UserAuth> CreateResponse(UserEntity userEntity)
        {
            await _tokenService.InvalidateAllUserAuthTokens(userEntity.Id);
            var token = await _tokenService.GenerateToken(userEntity);
            var refreshToken = await _tokenService.GenerateToken(userEntity, true);


            return new UserAuth
            {
                RefreshToken = refreshToken,
                AuthToken = token,
                UserName = userEntity.UserName,
                Email = userEntity.Email
            };
        }

        throw new AuthenticationException("Wrong password or login");
    }

    public async Task<bool> CheckIsEmailNotTaken(string email)
    {
        var user = await _usersService.FindUserAsync(email: email);
        return user is null;
    }

    public async Task<bool> ValidateToken(string token)
    {
        var tokenValidationResult = await _tokenService.ValidateToken(token);
        if (!tokenValidationResult.Success)
            throw new AuthenticationException(tokenValidationResult.ErrorDescription);

        return true;
    }

    public async Task<UserAuth> RegisterTelegramUser(TelegramUser telegramUser)
    {
        // Generate unique username for telegram user
        var username = $"tg_{telegramUser.TelegramId}";
        
        var user = new User
        {
            Login = username,
            Email = null, // Telegram doesn't provide email
            Password = GenerateRandomPassword(), // Generate a random password
            PhoneNumber = null
        };

        await _usersService.CreateTelegramUserAsync(user, telegramUser);
        return await AuthTelegramUser(telegramUser.TelegramId);
    }
    public async Task<UserAuth> AuthTelegramUser(string telegramId)
    {
        var userEntity = await _usersService.FindUserByTelegramIdAsync(telegramId);
        if (userEntity == null)
            throw new AuthenticationException("Telegram user not found");

        return await CreateAuthResponse(userEntity);
    }

    public async Task<bool> CheckIsTelegramIdNotTaken(string telegramId)
    {
        var user = await _usersService.FindUserByTelegramIdAsync(telegramId);
        return user is null;
    }

    private async Task<UserAuth> CreateAuthResponse(UserEntity userEntity)
    {
        await _tokenService.InvalidateAllUserAuthTokens(userEntity.Id);
        var token = await _tokenService.GenerateToken(userEntity);
        var refreshToken = await _tokenService.GenerateToken(userEntity, true);

        return new UserAuth
        {
            RefreshToken = refreshToken,
            AuthToken = token,
            UserName = userEntity.UserName,
            Email = userEntity.Email
        };
    }

    private string GenerateRandomPassword()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 16);
    }
}