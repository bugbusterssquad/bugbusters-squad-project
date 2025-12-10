namespace ClubsApi.Models;

public class Club
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    public string? Mission { get; set; }
    public string? Management { get; set; }
    public string? Contact { get; set; }
}
