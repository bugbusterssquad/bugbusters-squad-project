namespace ClubsApi.Models;

public enum DocumentStatus
{
    Pending,
    Approved,
    Rejected
}

public class SksEventDocument
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int ClubId { get; set; }
    public int UploadedByUserId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    public string? ReviewNote { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public Event? Event { get; set; }
    public Club? Club { get; set; }
    public User? UploadedByUser { get; set; }
}
