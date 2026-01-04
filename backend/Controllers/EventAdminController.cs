using System.Security.Claims;
using ClubsApi.Data;
using ClubsApi.Models;
using ClubsApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace ClubsApi.Controllers;

[ApiController]
public class EventAdminController(
    AppDbContext db,
    ClubAccessService clubAccessService,
    NotificationService notificationService,
    AuditLogService auditLogService) : ControllerBase
{
    public record EventAdminDto(
        int Id,
        int ClubId,
        string Title,
        string? Description,
        string? Location,
        DateTime StartAt,
        DateTime EndAt,
        int Capacity,
        string Status
    );

    public record CreateEventDto(
        string Title,
        string? Description,
        string? Location,
        DateTime StartAt,
        DateTime EndAt,
        int Capacity
    );

    public record UpdateEventDto(
        string Title,
        string? Description,
        string? Location,
        DateTime StartAt,
        DateTime EndAt,
        int Capacity,
        string Status
    );

    [HttpGet("api/admin/clubs/{clubId}/events")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    public async Task<ActionResult<IEnumerable<EventAdminDto>>> GetClubEvents(int clubId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (!await IsClubAdminAsync(userId.Value, clubId))
            return Forbid();

        var events = await db.Events
            .AsNoTracking()
            .Where(e => e.ClubId == clubId)
            .OrderByDescending(e => e.StartAt)
            .ToListAsync();

        return Ok(events.Select(e => new EventAdminDto(
            e.Id,
            e.ClubId,
            e.Title,
            e.Description,
            e.Location,
            e.StartAt,
            e.EndAt,
            e.Capacity,
            e.Status.ToString()
        )));
    }

    [HttpPost("api/clubs/{clubId}/events")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Create(int clubId, [FromBody] CreateEventDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (!await IsClubAdminAsync(userId.Value, clubId))
            return Forbid();

        var validation = EventRules.ValidateEvent(dto.StartAt, dto.EndAt, dto.Capacity);
        if (validation != null) return BadRequest(validation);

        var ev = new Event
        {
            ClubId = clubId,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Location = dto.Location?.Trim(),
            StartAt = dto.StartAt,
            EndAt = dto.EndAt,
            Capacity = dto.Capacity,
            Status = EventStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        db.Events.Add(ev);
        await db.SaveChangesAsync();

        await auditLogService.LogAsync(userId.Value, "create_event", "event", ev.Id, new
        {
            ev.ClubId,
            ev.Title,
            Status = ev.Status.ToString()
        });

        return Ok(new { message = "Etkinlik oluşturuldu.", id = ev.Id });
    }

    [HttpPatch("api/events/{id}")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEventDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var ev = await db.Events
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev == null) return NotFound("Etkinlik bulunamadı.");

        if (!await IsClubAdminAsync(userId.Value, ev.ClubId))
            return Forbid();

        if (!Enum.TryParse<EventStatus>(dto.Status, true, out var status))
            return BadRequest("Geçersiz durum.");

        var validation = EventRules.ValidateEvent(dto.StartAt, dto.EndAt, dto.Capacity);
        if (validation != null) return BadRequest(validation);

        var previousCapacity = ev.Capacity;
        var previousStatus = ev.Status;
        ev.Title = dto.Title.Trim();
        ev.Description = dto.Description?.Trim();
        ev.Location = dto.Location?.Trim();
        ev.StartAt = dto.StartAt;
        ev.EndAt = dto.EndAt;
        ev.Capacity = dto.Capacity;
        ev.Status = status;

        await db.SaveChangesAsync();

        await auditLogService.LogAsync(userId.Value, "update_event", "event", ev.Id, new
        {
            ev.ClubId,
            PreviousStatus = previousStatus.ToString(),
            Status = ev.Status.ToString(),
            PreviousCapacity = previousCapacity,
            Capacity = ev.Capacity
        });

        if (status == EventStatus.Cancelled)
        {
            var notifyIds = ev.Registrations
                .Where(r => r.Status == RegistrationStatus.Registered)
                .Select(r => r.UserId)
                .ToList();

            if (notifyIds.Count > 0)
            {
                await notificationService.NotifyManyAsync(notifyIds, "event_cancelled", new
                {
                    eventId = ev.Id,
                    eventTitle = ev.Title
                });
            }
        }
        else if (status == EventStatus.Published)
        {
            var notifyIds = ev.Registrations
                .Where(r => r.Status == RegistrationStatus.Registered)
                .Select(r => r.UserId)
                .ToList();

            if (notifyIds.Count > 0)
            {
                await notificationService.NotifyManyAsync(notifyIds, "event_updated", new
                {
                    eventId = ev.Id,
                    eventTitle = ev.Title
                });
            }
        }

        if (ev.Status != EventStatus.Cancelled && ev.Capacity > previousCapacity)
        {
            await PromoteWaitlistAsync(ev);
        }

        return Ok(new { message = "Etkinlik güncellendi." });
    }

    private async Task PromoteWaitlistAsync(Event ev)
    {
        var registeredCount = ev.Registrations.Count(r => r.Status == RegistrationStatus.Registered);
        var available = RegistrationRules.GetAvailableSlots(ev.Capacity, registeredCount);
        if (available <= 0) return;

        var waitlist = ev.Registrations
            .Where(r => r.Status == RegistrationStatus.Waitlist)
            .OrderBy(r => r.CreatedAt)
            .Take(available)
            .ToList();

        if (waitlist.Count == 0) return;

        foreach (var registration in waitlist)
        {
            registration.Status = RegistrationStatus.Registered;
        }

        await db.SaveChangesAsync();

        await notificationService.NotifyManyAsync(waitlist.Select(r => r.UserId), "event_waitlist_promoted", new
        {
            eventId = ev.Id,
            eventTitle = ev.Title
        });
    }

    private int? GetUserId()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idValue, out var id) ? id : null;
    }

    private Task<bool> IsClubAdminAsync(int userId, int clubId)
    {
        if (clubAccessService.IsSuperAdmin(User))
            return Task.FromResult(true);

        return clubAccessService.IsClubAdminAsync(userId, clubId);
    }
}
