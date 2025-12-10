using ClubsApi.Data;
using ClubsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db) : ControllerBase
{
    public record RegisterDto(string Name, string Email, string Password);
    public record LoginDto(string Email, string Password);
    public record ResetDto(string Email, string NewPassword);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        if (await db.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Bu email zaten kayıtlı.");

        var newUser = new User
        {
            Name = request.Name,
            Email = request.Email,
            Password = request.Password
        };

        db.Users.Add(newUser);
        await db.SaveChangesAsync();
        return Ok(new { message = "Kayıt başarılı!" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var user = await db.Users.FirstOrDefaultAsync(
            u => u.Email == request.Email && u.Password == request.Password
        );

        if (user == null)
            return Unauthorized("Email veya şifre hatalı.");

        return Ok(new { id = user.Id, name = user.Name, user.Email });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetDto request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return NotFound("Kullanıcı bulunamadı.");

        user.Password = request.NewPassword;
        await db.SaveChangesAsync();

        return Ok(new { message = "Şifre başarıyla güncellendi." });
    }
}
