using Logic;
using Microsoft.AspNetCore.Authorization;

namespace Api.Middlewares;

public class JwtValidationMiddleware
{
    private readonly RequestDelegate _next;

    public JwtValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITokenService tokenService)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint != null && !endpoint.Metadata.OfType<AuthorizeAttribute>().Any())
        {
            await _next(context);
        }
        else
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                await _next(context);
                return;
            }

            try
            {
                var isValidToken = await tokenService.ValidateToken(token);
                if (!isValidToken.Value)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid token");
                    return;
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync($"Token validation failed: {ex.Message}");
            }
        }
    }
}