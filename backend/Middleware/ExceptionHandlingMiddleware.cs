using System.Security.Claims;

namespace ClubsApi.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var requestId = context.TraceIdentifier;
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            logger.LogError(ex, "Unhandled exception requestId={RequestId} userId={UserId} path={Path}",
                requestId,
                userId ?? "anonymous",
                context.Request.Path.Value ?? string.Empty);

            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Sunucu hatası.",
                requestId
            });
        }
    }
}
