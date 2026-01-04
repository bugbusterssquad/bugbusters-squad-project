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
public class EventRegistrationsController(
    AppDbContext db,
    ClubAccessService clubAccessService,
    NotificationService notificationService,
    AuditLogService auditLogService) : ControllerBase
{
    public record RegistrationDto(int Id, int EventId, string Status, DateTime CreatedAt);
    public record AdminRegistrationDto(int Id, int UserId, string UserName, string UserEmail, string Status, DateTime CreatedAt);

    [HttpPost("api/events/{eventId}/registrations")]
    [Authorize(Roles = "Student,SuperAdmin")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Register(int eventId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var ev = await db.Events
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (ev == null || ev.Status != EventStatus.Published)
            return NotFound("Etkinlik bulunamadı.");

        if (ev.StartAt <= DateTime.UtcNow)
            return BadRequest("Etkinlik başlangıcı geçmiş olamaz.");

        var existing = ev.Registrations.FirstOrDefault(r => r.UserId == userId.Value);
        if (existing != null)
        {
            if (existing.Status == RegistrationStatus.Registered || existing.Status == RegistrationStatus.Waitlist)
                return BadRequest("Zaten kayıtlısınız.");

            var registeredCount = ev.Registrations.Count(r => r.Status == RegistrationStatus.Registered);
            existing.Status = RegistrationRules.DetermineStatus(ev.Capacity, registeredCount);
            existing.CreatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            await auditLogService.LogAsync(userId.Value, "register_event", "event_registration", existing.Id, new
            {
                eventId = ev.Id,
                status = existing.Status.ToString()
            });

            await notificationService.NotifyAsync(userId.Value, "event_registration", new
            {
                eventId = ev.Id,
                eventTitle = ev.Title,
                status = existing.Status.ToString()
            });

            return Ok(new { message = "Kayıt güncellendi.", status = existing.Status.ToString() });
        }

        var registeredCountNew = ev.Registrations.Count(r => r.Status == RegistrationStatus.Registered);
        var status = RegistrationRules.DetermineStatus(ev.Capacity, registeredCountNew);
        var registration = new EventRegistration
        {
            EventId = ev.Id,
            UserId = userId.Value,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

        db.EventRegistrations.Add(registration);
        await db.SaveChangesAsync();

        await auditLogService.LogAsync(userId.Value, "register_event", "event_registration", registration.Id, new
        {
            eventId = ev.Id,
            status = registration.Status.ToString()
        });

        await notificationService.NotifyAsync(userId.Value, "event_registration", new
        {
            eventId = ev.Id,
            eventTitle = ev.Title,
            status = registration.Status.ToString()
        });

        return Ok(new { message = "Kayıt alındı.", status = registration.Status.ToString() });
    }

    [HttpGet("api/events/{eventId}/registrations/me")]
    [Authorize]
    public async Task<ActionResult<RegistrationDto>> GetMyRegistration(int eventId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var reg = await db.EventRegistrations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId.Value);

        if (reg == null) return NotFound();

        return Ok(new RegistrationDto(reg.Id, reg.EventId, reg.Status.ToString(), reg.CreatedAt));
    }

    [HttpDelete("api/events/{eventId}/registrations/me")]
    [Authorize]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> CancelMyRegistration(int eventId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var reg = await db.EventRegistrations
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId.Value);

        if (reg == null) return NotFound();

        reg.Status = RegistrationStatus.Cancelled;
        await db.SaveChangesAsync();

        await auditLogService.LogAsync(userId.Value, "cancel_event_registration", "event_registration", reg.Id, new
        {
            eventId = reg.EventId
        });

        await PromoteWaitlistAsync(eventId);

        return Ok(new { message = "Kayıt iptal edildi." });
    }

    [HttpGet("api/events/{eventId}/registrations")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    public async Task<ActionResult<IEnumerable<AdminRegistrationDto>>> GetRegistrations(int eventId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var ev = await db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId);
        if (ev == null) return NotFound("Etkinlik bulunamadı.");

        if (!await IsClubAdminAsync(userId.Value, ev.ClubId))
            return Forbid();

        var regs = await db.EventRegistrations
            .Include(r => r.User)
            .Where(r => r.EventId == eventId)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return Ok(regs.Select(r => new AdminRegistrationDto(
            r.Id,
            r.UserId,
            r.User?.Name ?? string.Empty,
            r.User?.Email ?? string.Empty,
            r.Status.ToString(),
            r.CreatedAt
        )));
    }

    private async Task PromoteWaitlistAsync(int eventId)
    {
        var ev = await db.Events
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (ev == null) return;

        var registeredCount = ev.Registrations.Count(r => r.Status == RegistrationStatus.Registered);
        var available = RegistrationRules.GetAvailableSlots(ev.Capacity, registeredCount);
        if (available <= 0) return;

        var next = ev.Registrations
            .Where(r => r.Status == RegistrationStatus.Waitlist)
            .OrderBy(r => r.CreatedAt)
            .FirstOrDefault();

        if (next == null) return;

        next.Status = RegistrationStatus.Registered;
        await db.SaveChangesAsync();

        await notificationService.NotifyAsync(next.UserId, "event_waitlist_promoted", new
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
