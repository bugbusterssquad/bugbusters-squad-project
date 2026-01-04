using ClubsApi.Data;
using ClubsApi.Models;
using ClubsApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace ClubsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db, JwtTokenService tokenService, AuditLogService auditLogService, AnalyticsService analyticsService) : ControllerBase
{
    public record RegisterDto(string Name, string Email, string Password);
    public record LoginDto(string Email, string Password);
    public record AuthUserDto(int Id, string Name, string Email, string Role);
    public record AuthResponse(string Token, AuthUserDto User);

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthUserDto>> Me()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
        if (user == null) return Unauthorized();

        return Ok(new AuthUserDto(user.Id, user.Name, user.Email, user.Role.ToString()));
    }

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        if (await db.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Bu email zaten kayıtlı.");

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Ad ve email zorunludur.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            return BadRequest("Şifre en az 8 karakter olmalıdır.");

        var newUser = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Student
        };

        db.Users.Add(newUser);
        await db.SaveChangesAsync();
        return Ok(new { message = "Kayıt başarılı!" });
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return Unauthorized("Email veya şifre hatalı.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Email veya şifre hatalı.");

        user.LastLoginAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await auditLogService.LogAsync(user.Id, "login", "user", user.Id, new
        {
            ip = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        await analyticsService.LogEventAsync(HttpContext, "user_active", "user", user.Id);

        var token = tokenService.CreateToken(user);
        var response = new AuthResponse(token, new AuthUserDto(user.Id, user.Name, user.Email, user.Role.ToString()));

        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        return Ok(new { message = "Çıkış başarılı." });
    }

    private int? GetUserId()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idValue, out var id) ? id : null;
    }
}
