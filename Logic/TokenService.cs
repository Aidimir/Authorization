using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using Db.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using static System.Guid;
using TokenContext = Db.Context.TokenContext;

namespace Logic;

public interface ITokenService
{
    public Task<string> GenerateToken(UserEntity userEntity, bool isRefresh = false);
    public Task<bool> ValidateToken(string token);
    public Task InvalidateAllUserAuthTokens(string userId);
    public Task InvalidateAllUserRefreshTokens(string userId);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly TokenContext _tokenStore;

    public TokenService(IConfiguration configuration, TokenContext tokenStore)
    {
        _configuration = configuration;
        _tokenStore = tokenStore;
    }

    public async Task<bool> ValidateToken(string token)
    {
        var validator = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidIssuer = _configuration["JwtSettings:Issuer"],
            ValidAudience = _configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true
        };

        var jwtToken = validator.ReadJwtToken(token);
        var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

        if (string.IsNullOrEmpty(jti)) throw new AuthenticationException("token has no id");

        var isTokenValid = validator.CanReadToken(token) &&
                           (await validator.ValidateTokenAsync(token, validationParameters)).IsValid;
        if (!isTokenValid)
            throw new AuthenticationException("Invalid token");

        var tokenEntity = await _tokenStore.FindTokenAsync(Parse(jti));
        if (tokenEntity is null)
            throw new AuthenticationException("Token is invalidated");

        if (tokenEntity.ExpirationDate < DateTime.Now)
            throw new AuthenticationException("Token has expired");

        return true;
    }

    public async Task<string> GenerateToken(UserEntity userEntity, bool isRefresh = false)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var configString =
            _configuration[isRefresh ? "JwtSettings:RefreshTokenLifecycle" : "JwtSettings:AccessTokenLifecycle"];

        if (!int.TryParse(configString, out var expiresAfter))
            throw new ArgumentException(nameof(expiresAfter));
        var expiresAt = DateTime.UtcNow.AddSeconds(expiresAfter);

        var generateClaims = GenerateClaims(userEntity);

        var token = new JwtSecurityToken(
            _configuration["JwtSettings:Issuer"],
            _configuration["JwtSettings:Audience"],
            expires: expiresAt,
            claims: generateClaims.claims,
            signingCredentials: credentials
        );


        await _tokenStore.AddTokenAsync(new TokenEntity
        {
            Id = Parse(generateClaims.tokenJti), UserId = Parse(userEntity.Id), IsRefreshToken = isRefresh,
            ExpirationDate = expiresAt
        });

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task InvalidateAllUserAuthTokens(string userId)
    {
        await _tokenStore.RemoveAllUsersAuthTokensAsync(userId);
    }

    public async Task InvalidateAllUserRefreshTokens(string userId)
    {
        await _tokenStore.RemoveAllUsersRefreshTokensAsync(userId);
    }

    private (IEnumerable<Claim> claims, string tokenJti) GenerateClaims(UserEntity userEntity)
    {
        var tokenJti = NewGuid().ToString();

        var result = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Name, userEntity.UserName),
            new(JwtRegisteredClaimNames.Email, userEntity.Email),
            new(JwtRegisteredClaimNames.NameId, userEntity.Id),
            new(JwtRegisteredClaimNames.Jti, tokenJti)
        };
        result.AddRange(userEntity.Roles?.Select(role => new Claim(ClaimTypes.Role, role.Id)) ?? Array.Empty<Claim>());

        return (result, tokenJti);
    }
}