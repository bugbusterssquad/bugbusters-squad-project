namespace ClubsApi.Models;

public enum CommentStatus
{
    Visible,
    Hidden,
    Deleted
}

public class EventComment
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
    public string Body { get; set; } = string.Empty;
    public CommentStatus Status { get; set; } = CommentStatus.Visible;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Event? Event { get; set; }
    public User? User { get; set; }
}
