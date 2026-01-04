using System.Security.Claims;
using ClubsApi.Data;
using ClubsApi.Models;
using ClubsApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace ClubsApi.Controllers;

[ApiController]
public class ClubApplicationsController(AppDbContext db, AuditLogService auditLogService, NotificationService notificationService) : ControllerBase
{
    public record ApplyDto(string? Note);
    public record UpdateApplicationDto(string Status, string? Note);
    public record ApplicationDto(int Id, int ClubId, string ClubName, string Status, string? Note, string? QrCodeBase64, DateTime CreatedAt);
    public record ClubApplicationDto(int Id, int UserId, string UserName, string UserEmail, string Status, string? Note, DateTime CreatedAt);

    [HttpPost("api/clubs/{clubId}/applications")]
    [Authorize(Roles = "Student,SuperAdmin")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> ApplyToClub(int clubId, [FromBody] ApplyDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var clubExists = await db.Clubs.AnyAsync(c => c.Id == clubId);
        if (!clubExists) return NotFound("Kulüp bulunamadı.");

        var existing = await db.ClubMembershipApplications
            .FirstOrDefaultAsync(m => m.UserId == userId && m.ClubId == clubId);

        if (existing != null)
        {
            var validation = ApplicationRules.ValidateReapply(
                existing.Status,
                MembershipStatus.Pending,
                MembershipStatus.Approved,
                MembershipStatus.Rejected,
                existing.ReviewedAt,
                DateTime.UtcNow,
                30,
                "Zaten aktif bir başvurunuz var.",
                "Reddedilen başvurudan sonra 30 gün beklemelisiniz.");
            if (validation != null) return BadRequest(validation);

            existing.Status = MembershipStatus.Pending;
            existing.Note = dto.Note;
            existing.ReviewedAt = null;
            existing.QrCode = null;
            existing.CreatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            await auditLogService.LogAsync(userId, "reapply_membership", "club_membership_application", existing.Id, new
            {
                clubId
            });

            return Ok(new { message = "Başvuru yenilendi." });
        }

        var application = new ClubMembershipApplication
        {
            UserId = userId.Value,
            ClubId = clubId,
            Status = MembershipStatus.Pending,
            Note = dto.Note,
            CreatedAt = DateTime.UtcNow
        };

        db.ClubMembershipApplications.Add(application);
        await db.SaveChangesAsync();

        await auditLogService.LogAsync(userId, "apply_membership", "club_membership_application", application.Id, new
        {
            clubId
        });

        return Ok(new { message = "Başvuru oluşturuldu." });
    }

    [HttpGet("api/club-applications/me")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ApplicationDto>>> GetMyApplications()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var apps = await db.ClubMembershipApplications
            .Include(a => a.Club)
            .Where(a => a.UserId == userId.Value)
            .OrderByDescending(a => a.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return Ok(apps.Select(a => new ApplicationDto(
            a.Id,
            a.ClubId,
            a.Club?.Name ?? string.Empty,
            a.Status.ToString(),
            a.Note,
            a.QrCode,
            a.CreatedAt
        )));
    }

    [HttpGet("api/clubs/{clubId}/applications")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    public async Task<ActionResult<IEnumerable<ClubApplicationDto>>> GetClubApplications(int clubId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (!await IsClubAdminAsync(userId.Value, clubId) && !IsSuperAdmin())
            return Forbid();

        var apps = await db.ClubMembershipApplications
            .Include(a => a.User)
            .Where(a => a.ClubId == clubId)
            .OrderByDescending(a => a.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return Ok(apps.Select(a => new ClubApplicationDto(
            a.Id,
            a.UserId,
            a.User?.Name ?? string.Empty,
            a.User?.Email ?? string.Empty,
            a.Status.ToString(),
            a.Note,
            a.CreatedAt
        )));
    }

    [HttpPatch("api/club-applications/{id}")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> UpdateApplication(int id, [FromBody] UpdateApplicationDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var application = await db.ClubMembershipApplications
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null) return NotFound("Başvuru bulunamadı.");

        if (!await IsClubAdminAsync(userId.Value, application.ClubId) && !IsSuperAdmin())
            return Forbid();

        if (!Enum.TryParse<MembershipStatus>(dto.Status, true, out var newStatus))
            return BadRequest("Geçersiz durum.");

        var validation = ReviewRules.ValidateReviewTransition(
            application.Status,
            newStatus,
            MembershipStatus.Pending,
            MembershipStatus.Approved,
            MembershipStatus.Rejected,
            "Başvuru zaten sonuçlanmış.");
        if (validation != null) return BadRequest(validation);

        application.Status = newStatus;
        application.Note = dto.Note;
        application.ReviewedAt = DateTime.UtcNow;

        if (newStatus == MembershipStatus.Approved)
        {
            var qrContent = $"USER:{application.UserId}-CLUB:{application.ClubId}-APP:{application.Id}";

            using var qrGen = new QRCodeGenerator();
            var qrData = qrGen.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            var qrBytes = qrCode.GetGraphic(20);
            application.QrCode = Convert.ToBase64String(qrBytes);
        }

        await db.SaveChangesAsync();

        await auditLogService.LogAsync(userId,
            newStatus == MembershipStatus.Approved ? "approve_membership" : "reject_membership",
            "club_membership_application",
            application.Id,
            new { application.ClubId, application.UserId });

        await notificationService.NotifyAsync(application.UserId, "membership_status", new
        {
            clubId = application.ClubId,
            status = application.Status.ToString(),
            note = application.Note
        });

        return Ok(new { message = "Başvuru güncellendi." });
    }

    private int? GetUserId()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idValue, out var id) ? id : null;
    }

    private bool IsSuperAdmin() => User.IsInRole(UserRole.SuperAdmin.ToString());

    private Task<bool> IsClubAdminAsync(int userId, int clubId)
    {
        if (User.IsInRole(UserRole.SuperAdmin.ToString()))
            return Task.FromResult(true);

        return db.ClubAdmins.AnyAsync(a => a.UserId == userId && a.ClubId == clubId);
    }
}
