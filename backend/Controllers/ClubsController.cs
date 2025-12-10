using ClubsApi.Data;
using ClubsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClubsController(AppDbContext db) : ControllerBase
{
    public record ClubListDto(int Id, string Name);
    public record ClubDetailDto(int Id, string Name, string? Mission, string? Management, string? Contact);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClubListDto>>> Get()
    {
        var clubs = await db.Clubs.OrderBy(c => c.Id).ToListAsync();
        return Ok(clubs.Select(c => new ClubListDto(c.Id, c.Name)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClubDetailDto>> GetById(int id)
    {
        var club = await db.Clubs.FindAsync(id);
        if (club == null) return NotFound("Kulüp bulunamadı.");

        return Ok(new ClubDetailDto(
            club.Id,
            club.Name,
            club.Mission,
            club.Management,
            club.Contact
        ));
    }
}
