using Microsoft.AspNetCore.Http;

namespace backend_project.Services;

public interface IFileService
{
    Task<string> UploadFileAsync(IFormFile file, string folder);
    Task DeleteFileAsync(string fileUrl);
}
