namespace ClubsApi.Models;

public enum MembershipStatus
{
    Pending,
    Approved,
    Rejected
}

public class ClubMembershipApplication
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ClubId { get; set; }
    public MembershipStatus Status { get; set; } = MembershipStatus.Pending;
    public string? Note { get; set; }
    public string? QrCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public User? User { get; set; }
    public Club? Club { get; set; }
}
