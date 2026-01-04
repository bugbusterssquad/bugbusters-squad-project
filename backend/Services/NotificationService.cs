using System.Text.Json;
using ClubsApi.Data;
using ClubsApi.Models;
using System.Net.Http.Json;

namespace ClubsApi.Services;

public class NotificationService(AppDbContext db, IHttpClientFactory httpClientFactory, ILogger<NotificationService> logger)
{
    private readonly string? _webhookUrl = Environment.GetEnvironmentVariable("NOTIFICATION_WEBHOOK_URL");

    public async Task NotifyAsync(int userId, string type, object payload)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            PayloadJson = JsonSerializer.Serialize(payload),
            CreatedAt = DateTime.UtcNow
        };

        db.Notifications.Add(notification);
        await db.SaveChangesAsync();

        await DispatchWebhookAsync(new[] { userId }, type, notification.PayloadJson);
    }

    public async Task NotifyManyAsync(IEnumerable<int> userIds, string type, object payload)
    {
        var createdAt = DateTime.UtcNow;
        var json = JsonSerializer.Serialize(payload);
        var notifications = userIds.Distinct().Select(userId => new Notification
        {
            UserId = userId,
            Type = type,
            PayloadJson = json,
            CreatedAt = createdAt
        });

        db.Notifications.AddRange(notifications);
        await db.SaveChangesAsync();

        await DispatchWebhookAsync(userIds, type, json);
    }

    private async Task DispatchWebhookAsync(IEnumerable<int> userIds, string type, string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(_webhookUrl))
            return;

        try
        {
            var client = httpClientFactory.CreateClient();
            var payload = JsonSerializer.Deserialize<JsonElement>(payloadJson);
            await client.PostAsJsonAsync(_webhookUrl, new
            {
                userIds = userIds.Distinct().ToArray(),
                type,
                payload
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Notification webhook delivery failed.");
        }
    }
}
