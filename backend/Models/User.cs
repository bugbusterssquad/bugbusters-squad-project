namespace ClubsApi.Models;

public enum UserRole
{
    Student,
    ClubAdmin,
    SksAdmin,
    SuperAdmin
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Student;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public StudentProfile? StudentProfile { get; set; }
    public ICollection<ClubMembershipApplication> ClubMembershipApplications { get; set; } = new List<ClubMembershipApplication>();
    public ICollection<ClubAdmin> ClubAdminRoles { get; set; } = new List<ClubAdmin>();
    public ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();
}
