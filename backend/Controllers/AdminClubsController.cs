using System.Security.Claims;
using ClubsApi.Data;
using ClubsApi.Models;
using ClubsApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubsApi.Controllers;

[ApiController]
[Route("api/admin/clubs")]
public class AdminClubsController(AppDbContext db, ClubAccessService clubAccessService) : ControllerBase
{
    public record AdminClubDto(int Id, string Name);

    [HttpGet("mine")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    public async Task<ActionResult<IEnumerable<AdminClubDto>>> GetMyClubs()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (clubAccessService.IsSuperAdmin(User))
        {
            var all = await db.Clubs
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new AdminClubDto(c.Id, c.Name))
                .ToListAsync();
            return Ok(all);
        }

        var clubIds = await clubAccessService.GetAdminClubIdsAsync(userId.Value);
        var clubs = await db.Clubs
            .AsNoTracking()
            .Where(c => clubIds.Contains(c.Id))
            .OrderBy(c => c.Name)
            .Select(c => new AdminClubDto(c.Id, c.Name))
            .ToListAsync();

        return Ok(clubs);
    }

    private int? GetUserId()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idValue, out var id) ? id : null;
    }
}
