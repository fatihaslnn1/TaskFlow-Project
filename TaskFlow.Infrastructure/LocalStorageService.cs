using Microsoft.AspNetCore.Http;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Infrastructure.Services;

public class LocalStorageService : IStorageService
{
    public async Task<string> UploadFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0) return string.Empty;

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{fileName}";
    }
}