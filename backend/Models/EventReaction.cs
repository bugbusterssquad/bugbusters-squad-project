namespace ClubsApi.Models;

public enum ReactionType
{
    Like
}

public class EventReaction
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
    public ReactionType Type { get; set; } = ReactionType.Like;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Event? Event { get; set; }
    public User? User { get; set; }
}
