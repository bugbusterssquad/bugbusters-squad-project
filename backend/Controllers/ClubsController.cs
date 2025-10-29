using ClubsApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClubsController(AppDbContext db) : ControllerBase
{
    public record ClubDto(string Name, string Message);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClubDto>>> Get()
    {
        var clubs = await db.Clubs.AsNoTracking()
                                  .OrderBy(c => c.Id)
                                  .ToListAsync();

        var result = clubs.Select(c => new ClubDto(
            c.Name,
            $"{c.Name}: yakında hizmetinizdeyiz"
        ));

        return Ok(result);
    }
}
