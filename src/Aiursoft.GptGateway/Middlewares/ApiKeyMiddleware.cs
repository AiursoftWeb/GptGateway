using Aiursoft.GptGateway.Models.Configuration;
using Microsoft.Extensions.Options;

namespace Aiursoft.GptGateway.Middlewares;

public class ApiKeyMiddleware(RequestDelegate next, IOptions<GptModelOptions> modelOptions, ILogger<ApiKeyMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var apiKey = modelOptions.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            await next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (!path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) &&
            !path.StartsWith("/v1", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].ToString();
        var providedKey = string.Empty;

        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            providedKey = authHeader["Bearer ".Length..].Trim();
        }

        if (string.Equals(apiKey, providedKey, StringComparison.Ordinal))
        {
            await next(context);
        }
        else
        {
            logger.LogWarning("Unauthorized access attempt with path: {Path}. Provided key: {ProvidedKey}", path, providedKey);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized. Please provide a valid API Key in the Authorization header as a Bearer token.");
        }
    }
}

public static class ApiKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyValidation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiKeyMiddleware>();
    }
}
