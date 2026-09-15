using Microsoft.AspNetCore.Http;

namespace TaskFlow.Infrastructure.Services
{
    public class FileService
    {
        private readonly string[] _allowedExtensions = { ".png", ".jpg", ".jpeg", ".pdf", ".txt", ".log" };
        private readonly string[] _allowedMimeTypes = { 
            "image/png", "image/jpeg", "application/pdf", "text/plain", "text/log" 
        };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5 MB Limit

        public async Task<(bool IsValid, string Message, string FilePath, string FileName)> SaveFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (false, "Dosya seçilmedi.", string.Empty, string.Empty);

            if (file.Length > _maxFileSize)
                return (false, "Dosya boyutu çok büyük (Maksimum 5MB).", string.Empty, string.Empty);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return (false, "Desteklenmeyen dosya uzantısı.", string.Empty, string.Empty);

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return (true, "Başarılı", Path.Combine("Uploads", uniqueFileName), file.FileName);
        }
    }
}