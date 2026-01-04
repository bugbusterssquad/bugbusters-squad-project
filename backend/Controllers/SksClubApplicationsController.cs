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
[Route("api/sks/club-applications")]
public class SksClubApplicationsController(
    AppDbContext db,
    ClubAccessService clubAccessService,
    AuditLogService auditLogService,
    NotificationService notificationService) : ControllerBase
{
    public record SubmitClubApplicationDto(int ClubId);
    public record ReviewClubApplicationDto(string Status, string? ReviewNote);
    public record SksClubApplicationDto(
        int Id,
        int ClubId,
        string ClubName,
        int SubmittedByUserId,
        string SubmittedByName,
        string SubmittedByEmail,
        string Status,
        string? ReviewNote,
        DateTime CreatedAt,
        DateTime? ReviewedAt
    );

    [HttpPost]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Submit([FromBody] SubmitClubApplicationDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var club = await db.Clubs.FirstOrDefaultAsync(c => c.Id == dto.ClubId);
        if (club == null) return NotFound("Kulüp bulunamadı.");

        if (!await IsClubAdminAsync(userId.Value, dto.ClubId))
            return Forbid();

        var existing = await db.SksClubApplications
            .FirstOrDefaultAsync(a => a.ClubId == dto.ClubId);

        if (existing != null)
        {
            var validation = ApplicationRules.ValidateReapply(
                existing.Status,
                SksApplicationStatus.Pending,
                SksApplicationStatus.Approved,
                SksApplicationStatus.Rejected,
                existing.ReviewedAt,
                DateTime.UtcNow,
                30,
                "Bu kulüp için aktif başvuru mevcut.",
                "Reddedilen başvurudan sonra 30 gün beklemelisiniz.");
            if (validation != null) return BadRequest(validation);

            existing.Status = SksApplicationStatus.Pending;
            existing.ReviewNote = null;
            existing.ReviewedAt = null;
            existing.CreatedAt = DateTime.UtcNow;
            existing.SubmittedByUserId = userId.Value;

            await db.SaveChangesAsync();

            return Ok(new { message = "Başvuru yenilendi." });
        }

        var application = new SksClubApplication
        {
            ClubId = dto.ClubId,
            SubmittedByUserId = userId.Value,
            Status = SksApplicationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        db.SksClubApplications.Add(application);
        await db.SaveChangesAsync();

        return Ok(new { message = "Başvuru oluşturuldu." });
    }

    [HttpGet]
    [Authorize(Roles = "SksAdmin,SuperAdmin")]
    public async Task<ActionResult<IEnumerable<SksClubApplicationDto>>> GetAll([FromQuery] string? status)
    {
        var query = db.SksClubApplications
            .Include(a => a.Club)
            .Include(a => a.SubmittedByUser)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<SksApplicationStatus>(status, true, out var parsed))
        {
            query = query.Where(a => a.Status == parsed);
        }

        var applications = await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Ok(applications.Select(a => new SksClubApplicationDto(
            a.Id,
            a.ClubId,
            a.Club?.Name ?? string.Empty,
            a.SubmittedByUserId,
            a.SubmittedByUser?.Name ?? string.Empty,
            a.SubmittedByUser?.Email ?? string.Empty,
            a.Status.ToString(),
            a.ReviewNote,
            a.CreatedAt,
            a.ReviewedAt
        )));
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "SksAdmin,SuperAdmin")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Review(int id, [FromBody] ReviewClubApplicationDto dto)
    {
        var reviewerId = GetUserId();
        if (reviewerId == null) return Unauthorized();

        var application = await db.SksClubApplications
            .Include(a => a.Club)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null) return NotFound("Başvuru bulunamadı.");

        if (!Enum.TryParse<SksApplicationStatus>(dto.Status, true, out var newStatus))
            return BadRequest("Geçersiz durum.");

        var validation = ReviewRules.ValidateReviewTransition(
            application.Status,
            newStatus,
            SksApplicationStatus.Pending,
            SksApplicationStatus.Approved,
            SksApplicationStatus.Rejected,
            "Başvuru zaten sonuçlanmış.");
        if (validation != null) return BadRequest(validation);

        application.Status = newStatus;
        application.ReviewNote = dto.ReviewNote;
        application.ReviewedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        await auditLogService.LogAsync(reviewerId, newStatus == SksApplicationStatus.Approved ? "sks_approve_club" : "sks_reject_club",
            "sks_club_application",
            application.Id,
            new { application.ClubId, application.SubmittedByUserId });

        await notificationService.NotifyAsync(application.SubmittedByUserId, "sks_club_application", new
        {
            clubId = application.ClubId,
            clubName = application.Club?.Name,
            status = application.Status.ToString(),
            reviewNote = application.ReviewNote
        });

        return Ok(new { message = "Başvuru güncellendi." });
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
