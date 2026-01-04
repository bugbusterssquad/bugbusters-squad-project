using ClubsApi.Data;
using ClubsApi.Models;
using ClubsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(AppDbContext db, AnalyticsService analyticsService) : ControllerBase
{
    public record EventListDto(
        int Id,
        int ClubId,
        string ClubName,
        string Title,
        string? Description,
        string? Location,
        DateTime StartAt,
        DateTime EndAt,
        int Capacity,
        string Status
    );

    public record EventDetailDto(
        int Id,
        int ClubId,
        string ClubName,
        string Title,
        string? Description,
        string? Location,
        DateTime StartAt,
        DateTime EndAt,
        int Capacity,
        string Status
    );

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventListDto>>> Get(
        [FromQuery] int? clubId,
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? end,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        var query = db.Events
            .Include(e => e.Club)
            .Where(e => e.Status == EventStatus.Published)
            .AsNoTracking();

        if (clubId.HasValue)
            query = query.Where(e => e.ClubId == clubId.Value);

        if (start.HasValue)
            query = query.Where(e => e.StartAt >= start.Value);

        if (end.HasValue)
            query = query.Where(e => e.StartAt <= end.Value);

        var total = await query.CountAsync();
        Response.Headers.Append("X-Total-Count", total.ToString());

        var events = await query
            .OrderBy(e => e.StartAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(events.Select(e => new EventListDto(
            e.Id,
            e.ClubId,
            e.Club?.Name ?? "",
            e.Title,
            e.Description,
            e.Location,
            e.StartAt,
            e.EndAt,
            e.Capacity,
            e.Status.ToString()
        )));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventDetailDto>> GetById(int id)
    {
        var ev = await db.Events
            .Include(e => e.Club)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev == null) return NotFound("Etkinlik bulunamadı.");

        if (ev.Status != EventStatus.Published)
            return NotFound("Etkinlik bulunamadı.");

        await analyticsService.LogEventAsync(HttpContext, "event_viewed", "event", ev.Id, new { eventId = ev.Id });

        return Ok(new EventDetailDto(
            ev.Id,
            ev.ClubId,
            ev.Club?.Name ?? "",
            ev.Title,
            ev.Description,
            ev.Location,
            ev.StartAt,
            ev.EndAt,
            ev.Capacity,
            ev.Status.ToString()
        ));
    }
}
