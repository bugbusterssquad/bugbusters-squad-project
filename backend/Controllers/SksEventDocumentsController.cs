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
public class SksEventDocumentsController(
    AppDbContext db,
    ClubAccessService clubAccessService,
    DocumentStorageService documentStorageService,
    NotificationService notificationService,
    AuditLogService auditLogService) : ControllerBase
{
    public record EventDocumentDto(
        int Id,
        int EventId,
        string EventTitle,
        int ClubId,
        string ClubName,
        string FileName,
        string Status,
        string? ReviewNote,
        DateTime CreatedAt,
        DateTime? ReviewedAt
    );

    public record ReviewEventDocumentDto(string Status, string? ReviewNote);

    [HttpPost("api/events/{eventId}/documents")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Upload(int eventId, IFormFile file)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var ev = await db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId);
        if (ev == null) return NotFound("Etkinlik bulunamadı.");

        if (!await IsClubAdminAsync(userId.Value, ev.ClubId))
            return Forbid();

        var (relativePath, originalName, contentType) = await documentStorageService.SaveEventDocumentAsync(eventId, file);

        var document = new SksEventDocument
        {
            EventId = ev.Id,
            ClubId = ev.ClubId,
            UploadedByUserId = userId.Value,
            FilePath = relativePath,
            OriginalFileName = originalName,
            ContentType = contentType,
            Status = DocumentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        db.SksEventDocuments.Add(document);
        await db.SaveChangesAsync();

        return Ok(new { message = "Belge yüklendi." });
    }

    [HttpGet("api/events/{eventId}/documents")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin,SksAdmin")]
    public async Task<ActionResult<IEnumerable<EventDocumentDto>>> GetEventDocuments(int eventId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var ev = await db.Events.Include(e => e.Club).AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId);
        if (ev == null) return NotFound("Etkinlik bulunamadı.");

        if (User.IsInRole(UserRole.SksAdmin.ToString()) || clubAccessService.IsSuperAdmin(User))
        {
            return Ok(await BuildDocumentDtos(db.SksEventDocuments.Where(d => d.EventId == eventId)));
        }

        if (!await IsClubAdminAsync(userId.Value, ev.ClubId))
            return Forbid();

        return Ok(await BuildDocumentDtos(db.SksEventDocuments.Where(d => d.EventId == eventId)));
    }

    [HttpGet("api/sks/event-documents")]
    [Authorize(Roles = "SksAdmin,SuperAdmin")]
    public async Task<ActionResult<IEnumerable<EventDocumentDto>>> GetAll([FromQuery] string? status)
    {
        var query = db.SksEventDocuments.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<DocumentStatus>(status, true, out var parsed))
            query = query.Where(d => d.Status == parsed);

        return Ok(await BuildDocumentDtos(query));
    }

    [HttpPatch("api/sks/event-documents/{id}")]
    [Authorize(Roles = "SksAdmin,SuperAdmin")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Review(int id, [FromBody] ReviewEventDocumentDto dto)
    {
        var reviewerId = GetUserId();
        if (reviewerId == null) return Unauthorized();

        var document = await db.SksEventDocuments
            .Include(d => d.Event)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null) return NotFound("Belge bulunamadı.");

        if (!Enum.TryParse<DocumentStatus>(dto.Status, true, out var newStatus))
            return BadRequest("Geçersiz durum.");

        var validation = ReviewRules.ValidateReviewTransition(
            document.Status,
            newStatus,
            DocumentStatus.Pending,
            DocumentStatus.Approved,
            DocumentStatus.Rejected,
            "Belge zaten sonuçlanmış.");
        if (validation != null) return BadRequest(validation);

        document.Status = newStatus;
        document.ReviewNote = dto.ReviewNote;
        document.ReviewedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        await auditLogService.LogAsync(reviewerId,
            newStatus == DocumentStatus.Approved ? "sks_approve_document" : "sks_reject_document",
            "sks_event_document",
            document.Id,
            new { document.EventId, document.ClubId });

        await notificationService.NotifyAsync(document.UploadedByUserId, "sks_event_document", new
        {
            eventId = document.EventId,
            eventTitle = document.Event?.Title,
            status = document.Status.ToString(),
            reviewNote = document.ReviewNote
        });

        return Ok(new { message = "Belge güncellendi." });
    }

    [HttpGet("api/event-documents/{id}/download")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin,SksAdmin")]
    public async Task<IActionResult> Download(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var document = await db.SksEventDocuments
            .Include(d => d.Event)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null) return NotFound();

        if (User.IsInRole(UserRole.SksAdmin.ToString()) || clubAccessService.IsSuperAdmin(User))
            return PhysicalFile(documentStorageService.GetAbsolutePath(document.FilePath), document.ContentType, document.OriginalFileName);

        if (document.Event == null)
            return Forbid();

        if (!await IsClubAdminAsync(userId.Value, document.Event.ClubId))
            return Forbid();

        return PhysicalFile(documentStorageService.GetAbsolutePath(document.FilePath), document.ContentType, document.OriginalFileName);
    }

    private async Task<IEnumerable<EventDocumentDto>> BuildDocumentDtos(IQueryable<SksEventDocument> query)
    {
        var documents = await query
            .Include(d => d.Event)
            .Include(d => d.Club)
            .AsNoTracking()
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return documents.Select(d => new EventDocumentDto(
            d.Id,
            d.EventId,
            d.Event?.Title ?? string.Empty,
            d.ClubId,
            d.Club?.Name ?? string.Empty,
            d.OriginalFileName,
            d.Status.ToString(),
            d.ReviewNote,
            d.CreatedAt,
            d.ReviewedAt
        ));
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
