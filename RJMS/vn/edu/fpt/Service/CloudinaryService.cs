using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

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
                Folder = folderName,
                Type = "upload",
                AccessMode = "public"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                // Log error if needed
                return null;
            }

            return uploadResult.SecureUrl.ToString();
        }

        public async Task<string?> UploadRawAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0) return null;

            using var stream = file.OpenReadStream();
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folderName,
                UseFilename = true,
                UniqueFilename = true,
                Type = "upload",
                AccessMode = "public"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
                return null;

            return uploadResult.SecureUrl.ToString();
        }

        public async Task<string?> UploadPdfAsImageAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0) return null;

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folderName,
                UseFilename = true,
                UniqueFilename = true,
                Format = "pdf",
                Type = "upload",
                AccessMode = "public"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
            {
                return null;
            }

            return BuildPdfFirstPagePreviewUrl(uploadResult.SecureUrl?.ToString())
                ?? uploadResult.SecureUrl?.ToString();
        }

        public string? BuildSignedRawUrl(string? sourceUrl)
        {
            if (string.IsNullOrWhiteSpace(sourceUrl)) return sourceUrl;
            if (!Uri.TryCreate(sourceUrl, UriKind.Absolute, out var uri)) return sourceUrl;
            if (!uri.Host.Contains("res.cloudinary.com", StringComparison.OrdinalIgnoreCase)) return sourceUrl;

            var segments = uri.AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            // Expected path format:
            // /<cloud-name>/raw/<type>/v<version>/<public-id>
            var rawIndex = segments.FindIndex(s => s.Equals("raw", StringComparison.OrdinalIgnoreCase));
            if (rawIndex < 0 || rawIndex + 2 >= segments.Count) return sourceUrl;

            var deliveryType = segments[rawIndex + 1];
            var publicIdStart = rawIndex + 2;
            if (publicIdStart < segments.Count &&
                segments[publicIdStart].Length > 1 &&
                segments[publicIdStart][0] == 'v' &&
                segments[publicIdStart].Skip(1).All(char.IsDigit))
            {
                publicIdStart++;
            }

            if (publicIdStart >= segments.Count) return sourceUrl;

            var publicId = string.Join('/', segments.Skip(publicIdStart));
            return _cloudinary.Api.UrlImgUp
                .Secure(true)
                .ResourceType("raw")
                .Type(deliveryType)
                .Signed(true)
                .BuildUrl(publicId);
        }

        public string? BuildPdfFirstPagePreviewUrl(string? sourceUrl)
        {
            if (string.IsNullOrWhiteSpace(sourceUrl)) return null;
            if (!Uri.TryCreate(sourceUrl, UriKind.Absolute, out var uri)) return null;
            if (!uri.Host.Contains("res.cloudinary.com", StringComparison.OrdinalIgnoreCase)) return null;

            var segments = uri.AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            // Expected path format:
            // /<cloud-name>/image/<type>/v<version>/<public-id>.pdf
            var imageIndex = segments.FindIndex(s => s.Equals("image", StringComparison.OrdinalIgnoreCase));
            if (imageIndex < 0 || imageIndex + 2 >= segments.Count) return null;

            var type = segments[imageIndex + 1];
            if (!type.Equals("upload", StringComparison.OrdinalIgnoreCase)
                && !type.Equals("private", StringComparison.OrdinalIgnoreCase)
                && !type.Equals("authenticated", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var publicIdStart = imageIndex + 2;
            if (publicIdStart < segments.Count &&
                segments[publicIdStart].Length > 1 &&
                segments[publicIdStart][0] == 'v' &&
                segments[publicIdStart].Skip(1).All(char.IsDigit))
            {
                publicIdStart++;
            }

            if (publicIdStart >= segments.Count) return null;

            var publicIdSegments = segments.Skip(publicIdStart).ToList();
            var last = publicIdSegments[^1];
            if (!last.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) return null;

            publicIdSegments[^1] = last[..^4];
            var transformedPath = "/"
                + segments[0]
                + "/image/"
                + type
                + "/pg_1/"
                + string.Join('/', publicIdSegments)
                + ".jpg";

            return $"{uri.Scheme}://{uri.Host}{transformedPath}";
        }
    }
}
