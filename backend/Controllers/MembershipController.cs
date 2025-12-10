using ClubsApi.Data;
using ClubsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ClubsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembershipController(AppDbContext db) : ControllerBase
{
    public record ApplyDto(int UserId, int ClubId);

    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] ApplyDto dto)
    {
        var exists = await db.Memberships
            .AnyAsync(m => m.UserId == dto.UserId && m.ClubId == dto.ClubId);

        if (exists)
            return BadRequest("Zaten başvuru yapılmış.");

        var membership = new Membership
        {
            UserId = dto.UserId,
            ClubId = dto.ClubId,
            Status = "pending"
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync();

        return Ok(new { message = "Başvuru oluşturuldu." });
    }

    // --- ADMIN ONAY ---
    [HttpPost("approve/{id}")]
    public async Task<IActionResult> Approve(int id)
    {
        var membership = await db.Memberships.FindAsync(id);
        if (membership == null) return NotFound("Başvuru bulunamadı.");

        membership.Status = "approved";

        // QR üret
        var qrContent = $"USER:{membership.UserId}-CLUB:{membership.ClubId}-MEMBERSHIP:{membership.Id}";

        using var qrGen = new QRCodeGenerator();
        var qrData = qrGen.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new QRCode(qrData);

        using Bitmap bitmap = qrCode.GetGraphic(20);
        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);

        membership.QrCode = Convert.ToBase64String(stream.ToArray());

        await db.SaveChangesAsync();

        return Ok(new { message = "Başvuru onaylandı. QR oluşturuldu." });
    }

    // --- KULLANICI KART GÖRÜNTÜLEME ---
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserCard(int userId)
    {
        var membership = await db.Memberships
            .Where(m => m.UserId == userId && m.Status == "approved")
            .FirstOrDefaultAsync();

        if (membership == null)
            return NotFound("Onaylanmış üyelik bulunamadı.");

        return Ok(new
        {
            membership.Id,
            membership.ClubId,
            membership.Status,
            QrCodeBase64 = membership.QrCode
        });
    }
}
