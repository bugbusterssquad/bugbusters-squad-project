namespace ClubsApi.Models;

public enum ClubStatus
{
    Active,
    Passive
}

public class Club
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Contact { get; set; }
    public string? LogoUrl { get; set; }
    public ClubStatus Status { get; set; } = ClubStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<ClubAdmin> ClubAdmins { get; set; } = new List<ClubAdmin>();
    public ICollection<ClubMembershipApplication> ClubMembershipApplications { get; set; } = new List<ClubMembershipApplication>();
}
