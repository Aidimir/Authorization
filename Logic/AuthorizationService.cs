using System.Security.Authentication;
using Db.Models;
using Domain.Models;
using Microsoft.VisualBasic;

namespace Logic;

public interface IAuthorizationService
{
    public Task<UserAuth> RegisterUser(User user);
    public Task<UserAuth> Auth(UserCredentials userCredentials);
    public Task<bool> CheckIsEmailNotTaken(string email);
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
        if (userCredentials.RefreshToken is not null && await _tokenService.ValidateToken(userCredentials.RefreshToken))
            throw new AuthenticationException("Invalid refresh token");

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
}