using ClubsApi.Data;
using ClubsApi.Models;
using ClubsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClubsController(AppDbContext db, AnalyticsService analyticsService) : ControllerBase
{
    public record ClubListDto(int Id, string Name, string? Description, string? Category, string? LogoUrl);
    public record ClubOptionDto(int Id, string Name);
    public record ClubDetailDto(
        int Id,
        string Name,
        string? Description,
        string? Category,
        string? Contact,
        string? LogoUrl,
        string Status,
        DateTime CreatedAt
    );

    [HttpGet("options")]
    public async Task<ActionResult<IEnumerable<ClubOptionDto>>> GetOptions()
    {
        var clubs = await db.Clubs
            .AsNoTracking()
            .Where(c => c.Status == ClubStatus.Active)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(clubs.Select(c => new ClubOptionDto(c.Id, c.Name)));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClubListDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 9)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 9;
        if (pageSize > 50) pageSize = 50;

        var query = db.Clubs.AsNoTracking().Where(c => c.Status == ClubStatus.Active).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search) || (c.Description != null && c.Description.Contains(search)));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(c => c.Category == category);

        var total = await query.CountAsync();
        Response.Headers.Append("X-Total-Count", total.ToString());

        var clubs = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return Ok(clubs.Select(c => new ClubListDto(c.Id, c.Name, c.Description, c.Category, c.LogoUrl)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClubDetailDto>> GetById(int id)
    {
        var club = await db.Clubs.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (club == null) return NotFound("Kulüp bulunamadı.");

        await analyticsService.LogEventAsync(HttpContext, "club_viewed", "club", club.Id, new { clubId = club.Id });

        return Ok(new ClubDetailDto(
            club.Id,
            club.Name,
            club.Description,
            club.Category,
            club.Contact,
            club.LogoUrl,
            club.Status.ToString(),
            club.CreatedAt
        ));
    }
}
