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
public class AnnouncementsController(
    AppDbContext db,
    ClubAccessService clubAccessService,
    AuditLogService auditLogService) : ControllerBase
{
    public record AnnouncementDto(int Id, int ClubId, string Title, string Content, string Status, DateTime CreatedAt);
    public record CreateAnnouncementDto(string Title, string Content, string Status);
    public record UpdateAnnouncementDto(string Title, string Content, string Status);

    [HttpGet("api/clubs/{clubId}/announcements")]
    public async Task<ActionResult<IEnumerable<AnnouncementDto>>> GetPublished(int clubId)
    {
        var announcements = await db.Announcements
            .AsNoTracking()
            .Where(a => a.ClubId == clubId && a.Status == AnnouncementStatus.Published)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Ok(announcements.Select(a => new AnnouncementDto(
            a.Id,
            a.ClubId,
            a.Title,
            a.Content,
            a.Status.ToString(),
            a.CreatedAt
        )));
    }

    [HttpGet("api/admin/clubs/{clubId}/announcements")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    public async Task<ActionResult<IEnumerable<AnnouncementDto>>> GetAdminList(int clubId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (!await IsClubAdminAsync(userId.Value, clubId))
            return Forbid();

        var announcements = await db.Announcements
            .AsNoTracking()
            .Where(a => a.ClubId == clubId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Ok(announcements.Select(a => new AnnouncementDto(
            a.Id,
            a.ClubId,
            a.Title,
            a.Content,
            a.Status.ToString(),
            a.CreatedAt
        )));
    }

    [HttpPost("api/clubs/{clubId}/announcements")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Create(int clubId, [FromBody] CreateAnnouncementDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (!await IsClubAdminAsync(userId.Value, clubId))
            return Forbid();

        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest("Başlık ve içerik zorunludur.");

        if (!Enum.TryParse<AnnouncementStatus>(dto.Status, true, out var status))
            status = AnnouncementStatus.Published;

        var announcement = new Announcement
        {
            ClubId = clubId,
            Title = dto.Title.Trim(),
            Content = dto.Content.Trim(),
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

        db.Announcements.Add(announcement);
        await db.SaveChangesAsync();

        await auditLogService.LogAsync(userId.Value, "create_announcement", "announcement", announcement.Id, new
        {
            announcement.ClubId,
            announcement.Title,
            Status = announcement.Status.ToString()
        });

        return Ok(new { message = "Duyuru oluşturuldu." });
    }

    [HttpPatch("api/announcements/{id}")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAnnouncementDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var announcement = await db.Announcements.FirstOrDefaultAsync(a => a.Id == id);
        if (announcement == null) return NotFound("Duyuru bulunamadı.");

        if (!await IsClubAdminAsync(userId.Value, announcement.ClubId))
            return Forbid();

        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest("Başlık ve içerik zorunludur.");

        if (!Enum.TryParse<AnnouncementStatus>(dto.Status, true, out var status))
            return BadRequest("Geçersiz durum.");

        announcement.Title = dto.Title.Trim();
        announcement.Content = dto.Content.Trim();
        announcement.Status = status;
        announcement.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        await auditLogService.LogAsync(userId.Value, "update_announcement", "announcement", announcement.Id, new
        {
            announcement.ClubId,
            announcement.Title,
            Status = announcement.Status.ToString()
        });

        return Ok(new { message = "Duyuru güncellendi." });
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
