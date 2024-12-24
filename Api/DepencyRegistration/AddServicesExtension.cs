using System.Net;
using Api.Middlewares;
using Db.Models;
using Logic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Api.DepencyRegistration;

public static class AddServicesExtension
{
    public static void AddLogicServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(AutoMappingProfile));
        services.AddTransient<RoleManager<IdentityRole>>();
        services.AddTransient<UserManager<UserEntity>>();
        
        services.AddTransient<ITokenService, TokenService>();
        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<IUsersService, UsersService>();
        services.AddTransient<IAuthorizationService, AuthorizationService>();
        services.AddTransient<AuthorizeOrchestrationService>();
        
        services.TryAddScoped<HttpClient>();
        services.TryAddScoped<WebClient>();

        services.AddScoped<GlobalExceptionHandlerMiddleware>();
        // services.AddScoped<JwtValidationMiddleware>();
    }
}