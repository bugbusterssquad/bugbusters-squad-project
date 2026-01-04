namespace ClubsApi.Models;

public enum RegistrationStatus
{
    Registered,
    Cancelled,
    Waitlist
}

public class EventRegistration
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Registered;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Event? Event { get; set; }
    public User? User { get; set; }
}
