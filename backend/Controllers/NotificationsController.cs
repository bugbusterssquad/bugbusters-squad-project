using System.Security.Claims;
using System.Text.Json;
using ClubsApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubsApi.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController(AppDbContext db) : ControllerBase
{
    public record NotificationDto(int Id, string Type, JsonDocument Payload, DateTime CreatedAt, DateTime? ReadAt);

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetAll()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var notifications = await db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId.Value)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        var result = notifications.Select(n => new NotificationDto(
            n.Id,
            n.Type,
            JsonDocument.Parse(n.PayloadJson),
            n.CreatedAt,
            n.ReadAt
        ));

        return Ok(result);
    }

    [HttpPatch("{id}/read")]
    [Authorize]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId.Value);

        if (notification == null) return NotFound();

        if (notification.ReadAt == null)
        {
            notification.ReadAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        return Ok(new { message = "Bildirim okundu." });
    }

    private int? GetUserId()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idValue, out var id) ? id : null;
    }
}
