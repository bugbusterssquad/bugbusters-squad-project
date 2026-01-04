using ClubsApi.Data;
using ClubsApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubsApi.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminStatsController(AppDbContext db) : ControllerBase
{
    public record ViewStatDto(int EntityId, string Name, int Total, int Unique);
    public record ViewStatsResponse(IEnumerable<ViewStatDto> Clubs, IEnumerable<ViewStatDto> Events);
    public record ClubStatsDto(int ClubId, string ClubName, int EventCount, int RegistrationCount);
    public record DauPointDto(string Date, int Count);
    public record AdminStatsResponse(
        IEnumerable<ClubStatsDto> Clubs,
        IEnumerable<DauPointDto> DauTrend,
        ViewStatsResponse Views,
        int TotalRegistrations
    );

    [HttpGet("view-stats")]
    [Authorize(Roles = "SuperAdmin,SksAdmin")]
    public async Task<ActionResult<ViewStatsResponse>> GetViewStats()
    {
        return Ok(await BuildViewStatsAsync());
    }

    [HttpGet("stats")]
    [Authorize(Roles = "SuperAdmin,SksAdmin")]
    public async Task<IActionResult> GetStats([FromQuery] string? format)
    {
        var clubs = await db.Clubs.AsNoTracking().ToListAsync();
        var events = await db.Events.AsNoTracking().ToListAsync();
        var registrations = await db.EventRegistrations.AsNoTracking().ToListAsync();

        var eventCounts = events
            .GroupBy(e => e.ClubId)
            .ToDictionary(g => g.Key, g => g.Count());

        var eventClubMap = events.ToDictionary(e => e.Id, e => e.ClubId);
        var registrationCounts = registrations
            .Where(r => r.Status != RegistrationStatus.Cancelled)
            .GroupBy(r => eventClubMap.TryGetValue(r.EventId, out var clubId) ? clubId : 0)
            .Where(g => g.Key != 0)
            .ToDictionary(g => g.Key, g => g.Count());

        var clubStats = clubs.Select(c => new ClubStatsDto(
            c.Id,
            c.Name,
            eventCounts.TryGetValue(c.Id, out var count) ? count : 0,
            registrationCounts.TryGetValue(c.Id, out var regCount) ? regCount : 0
        )).OrderByDescending(c => c.RegistrationCount).ToList();

        var totalRegistrations = registrations.Count(r => r.Status != RegistrationStatus.Cancelled);

        var startDate = DateTime.UtcNow.Date.AddDays(-13);
        var dauEvents = await db.AnalyticsEvents
            .AsNoTracking()
            .Where(e => e.EventName == "user_active" && e.CreatedAt >= startDate)
            .ToListAsync();

        var dauMap = dauEvents
            .GroupBy(e => e.CreatedAt.Date)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.UserId?.ToString() ?? e.AnonId ?? string.Empty).Distinct().Count()
            );

        var dauTrend = Enumerable.Range(0, 14)
            .Select(offset =>
            {
                var day = startDate.AddDays(offset);
                return new DauPointDto(day.ToString("yyyy-MM-dd"), dauMap.TryGetValue(day, out var count) ? count : 0);
            })
            .ToList();

        var viewStats = await BuildViewStatsAsync();

        var response = new AdminStatsResponse(clubStats, dauTrend, viewStats, totalRegistrations);

        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = BuildCsv(response);
            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "admin-stats.csv");
        }

        return Ok(response);
    }

    private async Task<ViewStatsResponse> BuildViewStatsAsync()
    {
        var analytics = await db.AnalyticsEvents
            .AsNoTracking()
            .Where(e => e.EventName == "club_viewed" || e.EventName == "event_viewed")
            .ToListAsync();

        var clubs = await db.Clubs.AsNoTracking().ToListAsync();
        var events = await db.Events.AsNoTracking().ToListAsync();

        var clubMap = clubs.ToDictionary(c => c.Id, c => c.Name);
        var eventMap = events.ToDictionary(e => e.Id, e => e.Title);

        var clubStats = analytics
            .Where(e => e.EventName == "club_viewed" && e.EntityId.HasValue)
            .GroupBy(e => e.EntityId!.Value)
            .Select(g => new ViewStatDto(
                g.Key,
                clubMap.TryGetValue(g.Key, out var name) ? name : "Unknown",
                g.Count(),
                g.Select(e => e.UserId?.ToString() ?? e.AnonId ?? string.Empty).Distinct().Count()
            ))
            .OrderByDescending(x => x.Total)
            .ToList();

        var eventStats = analytics
            .Where(e => e.EventName == "event_viewed" && e.EntityId.HasValue)
            .GroupBy(e => e.EntityId!.Value)
            .Select(g => new ViewStatDto(
                g.Key,
                eventMap.TryGetValue(g.Key, out var name) ? name : "Unknown",
                g.Count(),
                g.Select(e => e.UserId?.ToString() ?? e.AnonId ?? string.Empty).Distinct().Count()
            ))
            .OrderByDescending(x => x.Total)
            .ToList();

        return new ViewStatsResponse(clubStats, eventStats);
    }

    private static string BuildCsv(AdminStatsResponse response)
    {
        static string Escape(string value) => $"\"{value.Replace("\"", "\"\"")}\"";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("ClubId,ClubName,EventCount,RegistrationCount");
        foreach (var club in response.Clubs)
        {
            sb.AppendLine($"{club.ClubId},{Escape(club.ClubName)},{club.EventCount},{club.RegistrationCount}");
        }

        sb.AppendLine();
        sb.AppendLine("Date,DAU");
        foreach (var point in response.DauTrend)
        {
            sb.AppendLine($"{point.Date},{point.Count}");
        }

        sb.AppendLine();
        sb.AppendLine("EntityType,EntityId,Name,Total,Unique");
        foreach (var club in response.Views.Clubs)
        {
            sb.AppendLine($"club,{club.EntityId},{Escape(club.Name)},{club.Total},{club.Unique}");
        }
        foreach (var ev in response.Views.Events)
        {
            sb.AppendLine($"event,{ev.EntityId},{Escape(ev.Name)},{ev.Total},{ev.Unique}");
        }

        sb.AppendLine();
        sb.AppendLine($"TotalRegistrations,{response.TotalRegistrations}");
        return sb.ToString();
    }
}
