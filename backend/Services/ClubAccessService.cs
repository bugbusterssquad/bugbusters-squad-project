using ClubsApi.Data;
using ClubsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClubsApi.Services;

public class ClubAccessService(AppDbContext db)
{
    public Task<bool> IsClubAdminAsync(int userId, int clubId)
    {
        return db.ClubAdmins.AnyAsync(a => a.UserId == userId && a.ClubId == clubId);
    }

    public Task<List<int>> GetAdminClubIdsAsync(int userId)
    {
        return db.ClubAdmins
            .Where(a => a.UserId == userId)
            .Select(a => a.ClubId)
            .Distinct()
            .ToListAsync();
    }

    public bool IsSuperAdmin(System.Security.Claims.ClaimsPrincipal user)
    {
        return user.IsInRole(UserRole.SuperAdmin.ToString());
    }
}
