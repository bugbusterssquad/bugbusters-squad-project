using System.Diagnostics;
using System.Security.Claims;

namespace ClubsApi.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        var requestId = context.TraceIdentifier;
        if (!context.Response.HasStarted)
        {
            context.Response.Headers.TryAdd("X-Request-Id", requestId);
        }

        var stopwatch = Stopwatch.StartNew();
        await next(context);
        stopwatch.Stop();

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var roles = context.User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToArray();

        logger.LogInformation("HTTP {Method} {Path} -> {StatusCode} ({ElapsedMs}ms) requestId={RequestId} userId={UserId} roles={Roles}",
            context.Request.Method,
            context.Request.Path.Value ?? string.Empty,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            requestId,
            userId ?? "anonymous",
            roles.Length == 0 ? "none" : string.Join(',', roles));
    }
}
