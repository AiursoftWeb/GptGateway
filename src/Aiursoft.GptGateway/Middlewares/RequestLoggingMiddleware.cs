using System.Diagnostics;
using System.Text;
using Aiursoft.GptGateway.Data;
using Aiursoft.GptGateway.Models;

namespace Aiursoft.GptGateway.Middlewares;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, RequestLogContext logContext, ClickhouseDbContext clickhouseDbContext)
    {
        var sw = Stopwatch.StartNew();
        var request = context.Request;

        request.EnableBuffering();

        var method = request.Method;
        var path = request.Path + request.QueryString;

        logContext.Log.IP = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        logContext.Log.Method = method;
        logContext.Log.Path = path;
        logContext.Log.UserAgent = request.Headers["User-Agent"].ToString();
        logContext.Log.TraceId = context.TraceIdentifier;

        var body = string.Empty;
        if (request.ContentLength > 0)
        {
            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        logger.LogInformation("→ HTTP {Method} {Path}  Body: {Body}", method, path, body);
        
        await next(context);

        logContext.Log.StatusCode = context.Response.StatusCode;
        if (logContext.Log.Duration == 0)
        {
            logContext.Log.Duration = sw.Elapsed.TotalMilliseconds;
        }

        if (clickhouseDbContext.Enabled)
        {
            clickhouseDbContext.RequestLogs.Add(logContext.Log);
            await clickhouseDbContext.SaveChangesAsync();
        }
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
