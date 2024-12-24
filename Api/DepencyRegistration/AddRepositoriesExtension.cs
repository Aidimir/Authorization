using Db.Context;
using Db.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TokenContext = Db.Context.TokenContext;

namespace Api.DepencyRegistration;

public static class AddRepositoriesExtension
{
    public static void AddRepositories(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContext<UserContext>(options => { options.UseNpgsql(connectionString); });
        services.AddDbContext<TokenContext>(options => { options.UseNpgsql(connectionString); });
        services.AddDbContext<EmailConfirmationContext>(options => { options.UseNpgsql(connectionString); });

        services.AddIdentity<UserEntity, IdentityRole>()
            .AddEntityFrameworkStores<UserContext>()
            .AddDefaultTokenProviders();

        using (var provider = services.BuildServiceProvider())
        {
            var userContext = provider.GetRequiredService<UserContext>();
            if (userContext.Database.GetPendingMigrations().Any())
                userContext.Database.Migrate();

            var tokenContext = provider.GetRequiredService<TokenContext>();
            if (tokenContext.Database.GetPendingMigrations().Any())
                tokenContext.Database.Migrate();
            
            var emailConfirmationContext = provider.GetRequiredService<EmailConfirmationContext>();
            if (emailConfirmationContext.Database.GetPendingMigrations().Any())
                emailConfirmationContext.Database.Migrate();
        }
    }
}