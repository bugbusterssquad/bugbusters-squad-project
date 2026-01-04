using System.Text.Json;
using ClubsApi.Data;
using ClubsApi.Models;

namespace ClubsApi.Services;

public class AuditLogService(AppDbContext db)
{
    public async Task LogAsync(int? actorUserId, string action, string entityType, int? entityId, object? meta = null)
    {
        var log = new AuditLog
        {
            ActorUserId = actorUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            MetaJson = meta == null ? null : JsonSerializer.Serialize(meta),
            CreatedAt = DateTime.UtcNow
        };

        db.AuditLogs.Add(log);
        await db.SaveChangesAsync();
    }
}
