namespace ClubsApi.Models;

public enum ClubAdminRole
{
    Owner,
    Admin
}

public class ClubAdmin
{
    public int Id { get; set; }
    public int ClubId { get; set; }
    public int UserId { get; set; }
    public ClubAdminRole RoleInClub { get; set; } = ClubAdminRole.Admin;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Club? Club { get; set; }
    public User? User { get; set; }
}
