using Db.Models;
using Microsoft.EntityFrameworkCore;

namespace Db.Context;

public class TokenContext : DbContext
{
    public TokenContext(DbContextOptions<TokenContext> options) : base(options)
    {
    }

    private DbSet<TokenEntity?> _tokens { get; set; }

    public async Task<TokenEntity?> FindTokenAsync(Guid tokenId)
    {
        return await _tokens.FindAsync(tokenId);
    }

    public async Task AddTokenAsync(TokenEntity? token)
    {
        await _tokens.AddAsync(token);
        await SaveChangesAsync();
    }

    public async Task RemoveTokenAsync(string tokenId)
    {
        var token = await _tokens.FindAsync(tokenId);
        if (token != null)
            _tokens.Remove(token);
        await SaveChangesAsync();
    }

    public async Task RemoveAllUsersTokensAsync(string userId)
    {
        var userTokens = await _tokens.Where(t => t.UserId.ToString() == userId).ToListAsync();
        _tokens.RemoveRange(userTokens);
        await SaveChangesAsync();
    }

    public async Task RemoveAllUsersAuthTokensAsync(string userId)
    {
        var userTokens = await _tokens.Where(t => t.UserId.ToString() == userId && !t.IsRefreshToken).ToListAsync();
        _tokens.RemoveRange(userTokens);
        await SaveChangesAsync();
    }

    public async Task RemoveAllUsersRefreshTokensAsync(string userId)
    {
        var userTokens = await _tokens.Where(t => t.UserId.ToString() == userId && t.IsRefreshToken).ToListAsync();
        _tokens.RemoveRange(userTokens);
        await SaveChangesAsync();
    }
}