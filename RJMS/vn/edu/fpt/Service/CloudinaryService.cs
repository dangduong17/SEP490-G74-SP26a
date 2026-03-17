using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var section = configuration.GetSection("Cloudinary");
            var account = new Account(
                section["CloudName"],
                section["ApiKey"],
                section["ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string?> UploadImageAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0) return null;

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folderName
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                // Log error if needed
                return null;
            }

            return uploadResult.SecureUrl.ToString();
        }
    }
}
