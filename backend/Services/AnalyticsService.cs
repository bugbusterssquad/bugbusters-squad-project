using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ClubsApi.Data;
using ClubsApi.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ClubsApi.Services;

public class AnalyticsService(AppDbContext db)
{
    public async Task LogEventAsync(HttpContext httpContext, string eventName, string entityType, int? entityId, object? meta = null)
    {
        var userId = GetUserId(httpContext);
        var anonId = userId == null ? BuildAnonId(httpContext) : null;

        var evt = new AnalyticsEvent
        {
            UserId = userId,
            AnonId = anonId,
            EventName = eventName,
            EntityType = entityType,
            EntityId = entityId,
            MetaJson = meta == null ? null : JsonSerializer.Serialize(meta),
            CreatedAt = DateTime.UtcNow
        };

        db.AnalyticsEvents.Add(evt);
        await db.SaveChangesAsync();
    }

    private static int? GetUserId(HttpContext httpContext)
    {
        var idValue = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idValue, out var id) ? id : null;
    }

    private static string BuildAnonId(HttpContext httpContext)
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        var raw = $"{ip}|{userAgent}";

        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash);
    }
}
