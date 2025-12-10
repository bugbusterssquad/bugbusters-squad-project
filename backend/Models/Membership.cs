namespace ClubsApi.Models;

public class Membership
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public int ClubId { get; set; }

    public string Status { get; set; } = "pending"; // pending, approved, rejected

    public string? QrCode { get; set; }

    public User? User { get; set; }
    public Club? Club { get; set; }
}
