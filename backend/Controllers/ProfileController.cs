using System.Security.Claims;
using ClubsApi.Data;
using ClubsApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace ClubsApi.Controllers;

[ApiController]
[Route("api/profile")]
public class ProfileController(AppDbContext db) : ControllerBase
{
    public record ProfileDto(
        int Id,
        string Name,
        string Email,
        string Role,
        string? Faculty,
        string? Department,
        string? Bio,
        string? AvatarUrl
    );

    public record UpdateProfileDto(
        string? Name,
        string? Faculty,
        string? Department,
        string? Bio,
        string? AvatarUrl
    );

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ProfileDto>> GetMe()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var user = await db.Users
            .Include(u => u.StudentProfile)
            .FirstOrDefaultAsync(u => u.Id == userId.Value);

        if (user == null) return Unauthorized();

        return Ok(new ProfileDto(
            user.Id,
            user.Name,
            user.Email,
            user.Role.ToString(),
            user.StudentProfile?.Faculty,
            user.StudentProfile?.Department,
            user.StudentProfile?.Bio,
            user.StudentProfile?.AvatarUrl
        ));
    }

    [HttpPatch("me")]
    [Authorize]
    [EnableRateLimiting("write")]
    public async Task<ActionResult<ProfileDto>> UpdateMe([FromBody] UpdateProfileDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var user = await db.Users
            .Include(u => u.StudentProfile)
            .FirstOrDefaultAsync(u => u.Id == userId.Value);

        if (user == null) return Unauthorized();

        if (!string.IsNullOrWhiteSpace(dto.Name))
            user.Name = dto.Name.Trim();

        if (user.StudentProfile == null)
        {
            user.StudentProfile = new StudentProfile
            {
                UserId = user.Id
            };
        }

        if (dto.Faculty != null) user.StudentProfile.Faculty = TrimOrNull(dto.Faculty, 150);
        if (dto.Department != null) user.StudentProfile.Department = TrimOrNull(dto.Department, 150);
        if (dto.Bio != null) user.StudentProfile.Bio = TrimOrNull(dto.Bio, 500);
        if (dto.AvatarUrl != null) user.StudentProfile.AvatarUrl = TrimOrNull(dto.AvatarUrl, 500);

        await db.SaveChangesAsync();

        return Ok(new ProfileDto(
            user.Id,
            user.Name,
            user.Email,
            user.Role.ToString(),
            user.StudentProfile.Faculty,
            user.StudentProfile.Department,
            user.StudentProfile.Bio,
            user.StudentProfile.AvatarUrl
        ));
    }

    private int? GetUserId()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idValue, out var id) ? id : null;
    }

    private static string? TrimOrNull(string value, int maxLen)
    {
        var trimmed = value.Trim();
        if (trimmed.Length == 0) return null;
        return trimmed.Length > maxLen ? trimmed.Substring(0, maxLen) : trimmed;
    }
}
