namespace ClubsApi.Models;

public class StudentProfile
{
    public int UserId { get; set; }
    public string? Faculty { get; set; }
    public string? Department { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }

    public User? User { get; set; }
}
