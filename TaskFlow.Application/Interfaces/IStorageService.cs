using Microsoft.AspNetCore.Http;

namespace TaskFlow.Application.Interfaces;

public interface IStorageService
{
    Task<string> UploadFileAsync(IFormFile file);
}