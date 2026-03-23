using Microsoft.AspNetCore.Http;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface ICloudinaryService
    {
        Task<string?> UploadImageAsync(IFormFile file, string folderName);
        Task<string?> UploadRawAsync(IFormFile file, string folderName);
    }
}
