using Db.Models;
using Microsoft.EntityFrameworkCore;

namespace Db.Context;

public class EmailConfirmationContext : DbContext
{
    public EmailConfirmationContext(DbContextOptions<EmailConfirmationContext> options) : base(options)
    {
    }

    private DbSet<EmailConfirmationEntity?> _emailConfirmations { get; set; }

    public async Task<EmailConfirmationEntity?> FindConfirmationEntityAsync(string email)
    {
        return await _emailConfirmations.FindAsync(email);
    }

    public async Task UpdateConfirmationEntityAsync(EmailConfirmationEntity? confirmationEntity)
    {
        _emailConfirmations.Update(confirmationEntity);
        await SaveChangesAsync();
    }

    public async Task AddConfirmationEntiytyAsync(EmailConfirmationEntity? entity)
    {
        var foundConfirmationEntity = await FindConfirmationEntityAsync(entity.Email);
        if (foundConfirmationEntity is not null)
            throw new InvalidOperationException("Email confirmation is already in use.");

        await _emailConfirmations.AddAsync(entity);
        await SaveChangesAsync();
    }

    public async Task RemoveConfirmationEntityAsync(string email)
    {
        var entity = await _emailConfirmations.FindAsync(email);
        if (entity != null)
            _emailConfirmations.Remove(entity);

        await SaveChangesAsync();
    }
}