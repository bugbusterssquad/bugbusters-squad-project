namespace ClubsApi.Models;

public enum SksApplicationStatus
{
    Pending,
    Approved,
    Rejected
}

public class SksClubApplication
{
    public int Id { get; set; }
    public int ClubId { get; set; }
    public int SubmittedByUserId { get; set; }
    public SksApplicationStatus Status { get; set; } = SksApplicationStatus.Pending;
    public string? ReviewNote { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public Club? Club { get; set; }
    public User? SubmittedByUser { get; set; }
}
