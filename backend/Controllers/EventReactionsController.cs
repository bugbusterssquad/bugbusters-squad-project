using System.Security.Claims;
using ClubsApi.Data;
using ClubsApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace ClubsApi.Controllers;

[ApiController]
public class EventReactionsController(AppDbContext db) : ControllerBase
{
    public record ReactionSummaryDto(int Total, bool Liked);

    [HttpGet("api/events/{eventId}/reactions")]
    public async Task<ActionResult<ReactionSummaryDto>> GetSummary(int eventId)
    {
        var userId = GetUserId();

        var total = await db.EventReactions
            .AsNoTracking()
            .CountAsync(r => r.EventId == eventId && r.Type == ReactionType.Like);

        var liked = false;
        if (userId.HasValue)
        {
            liked = await db.EventReactions
                .AsNoTracking()
                .AnyAsync(r => r.EventId == eventId && r.UserId == userId.Value && r.Type == ReactionType.Like);
        }

        return Ok(new ReactionSummaryDto(total, liked));
    }

    [HttpPost("api/events/{eventId}/reactions")]
    [Authorize]
    [EnableRateLimiting("write")]
    public async Task<ActionResult<ReactionSummaryDto>> ToggleLike(int eventId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var existing = await db.EventReactions
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId.Value && r.Type == ReactionType.Like);

        if (existing != null)
        {
            db.EventReactions.Remove(existing);
            await db.SaveChangesAsync();
        }
        else
        {
            var reaction = new EventReaction
            {
                EventId = eventId,
                UserId = userId.Value,
                Type = ReactionType.Like,
                CreatedAt = DateTime.UtcNow
            };
            db.EventReactions.Add(reaction);
            await db.SaveChangesAsync();
        }

        var total = await db.EventReactions
            .AsNoTracking()
            .CountAsync(r => r.EventId == eventId && r.Type == ReactionType.Like);

        var liked = existing == null;
        return Ok(new ReactionSummaryDto(total, liked));
    }

    private int? GetUserId()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idValue, out var id) ? id : null;
    }
}
