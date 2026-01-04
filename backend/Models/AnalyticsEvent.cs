namespace ClubsApi.Models;

public class AnalyticsEvent
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? AnonId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? MetaJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
