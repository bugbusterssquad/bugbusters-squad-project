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
public class EventCommentsController(
    AppDbContext db,
    ClubAccessService clubAccessService,
    AuditLogService auditLogService) : ControllerBase
{
    public record CommentDto(int Id, int UserId, string UserName, string Body, DateTime CreatedAt, string Status);
    public record CreateCommentDto(string Body);

    [HttpGet("api/events/{eventId}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetByEvent(int eventId)
    {
        var comments = await db.EventComments
            .Include(c => c.User)
            .Where(c => c.EventId == eventId && c.Status == CommentStatus.Visible)
            .OrderBy(c => c.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return Ok(comments.Select(c => new CommentDto(
            c.Id,
            c.UserId,
            c.User?.Name ?? string.Empty,
            c.Body,
            c.CreatedAt,
            c.Status.ToString()
        )));
    }

    [HttpPost("api/events/{eventId}/comments")]
    [Authorize(Roles = "Student,ClubAdmin,SuperAdmin")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Create(int eventId, [FromBody] CreateCommentDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var ev = await db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId);
        if (ev == null || ev.Status != EventStatus.Published)
            return NotFound("Etkinlik bulunamadı.");

        var body = dto.Body?.Trim();
        if (string.IsNullOrWhiteSpace(body))
            return BadRequest("Yorum boş olamaz.");

        if (body.Length > 1000)
            return BadRequest("Yorum 1000 karakteri geçemez.");

        if (CommentModerationService.ContainsBlockedContent(body))
            return BadRequest("Yorum içeriği uygun değil.");

        var comment = new EventComment
        {
            EventId = eventId,
            UserId = userId.Value,
            Body = body,
            Status = CommentStatus.Visible,
            CreatedAt = DateTime.UtcNow
        };

        db.EventComments.Add(comment);
        await db.SaveChangesAsync();

        await auditLogService.LogAsync(userId.Value, "create_comment", "event_comment", comment.Id, new
        {
            eventId,
            comment.Status
        });

        return Ok(new { message = "Yorum eklendi." });
    }

    [HttpPatch("api/comments/{id}/hide")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Hide(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var comment = await db.EventComments
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null) return NotFound();

        if (clubAccessService.IsSuperAdmin(User))
        {
            comment.Status = CommentStatus.Hidden;
            comment.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            await auditLogService.LogAsync(userId.Value, "hide_comment", "event_comment", comment.Id, new { comment.EventId });
            return Ok(new { message = "Yorum gizlendi." });
        }

        if (comment.Event == null)
            return Forbid();

        if (!await clubAccessService.IsClubAdminAsync(userId.Value, comment.Event.ClubId))
            return Forbid();

        comment.Status = CommentStatus.Hidden;
        comment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await auditLogService.LogAsync(userId.Value, "hide_comment", "event_comment", comment.Id, new { comment.EventId });

        return Ok(new { message = "Yorum gizlendi." });
    }

    [HttpDelete("api/comments/{id}")]
    [Authorize]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var comment = await db.EventComments.FirstOrDefaultAsync(c => c.Id == id);
        if (comment == null) return NotFound();

        if (comment.UserId != userId.Value && !clubAccessService.IsSuperAdmin(User))
            return Forbid();

        comment.Status = CommentStatus.Deleted;
        comment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await auditLogService.LogAsync(userId.Value, "delete_comment", "event_comment", comment.Id, new { comment.EventId });

        return Ok(new { message = "Yorum silindi." });
    }

    private int? GetUserId()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idValue, out var id) ? id : null;
    }
}
