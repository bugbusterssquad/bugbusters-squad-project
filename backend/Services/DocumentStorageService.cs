using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ClubsApi.Services;

public class DocumentStorageService(IWebHostEnvironment env)
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/png",
        "image/jpeg"
    };

    public async Task<(string RelativePath, string FileName, string ContentType)> SaveEventDocumentAsync(int eventId, IFormFile file)
    {
        if (file.Length == 0)
            throw new InvalidOperationException("Dosya boş olamaz.");

        if (file.Length > MaxFileSizeBytes)
            throw new InvalidOperationException("Dosya boyutu 10MB'ı geçemez.");

        if (!AllowedContentTypes.Contains(file.ContentType))
            throw new InvalidOperationException("Sadece PDF veya PNG/JPEG dosyalarına izin verilir.");

        var uploadsRoot = Path.Combine(env.ContentRootPath, "Storage", "event-documents", eventId.ToString());
        Directory.CreateDirectory(uploadsRoot);

        var extension = Path.GetExtension(file.FileName);
        var safeFileName = Path.GetFileName(file.FileName);
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var storedPath = Path.Combine(uploadsRoot, storedName);

        await using var stream = File.Create(storedPath);
        await file.CopyToAsync(stream);

        var relativePath = Path.Combine("Storage", "event-documents", eventId.ToString(), storedName)
            .Replace(Path.DirectorySeparatorChar, '/');

        return (relativePath, safeFileName, file.ContentType);
    }

    public string GetAbsolutePath(string relativePath)
    {
        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(env.ContentRootPath, normalized);
    }
}
