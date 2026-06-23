using CloudinaryDotNet;
using Microsoft.Extensions.Options;
using Shared.Application.Interfaces;
using Shared.Infrastructure.Settings;

namespace Shared.Infrastructure.ObjectStorage
{
    /// <summary>
    /// Cloudinary implementation of <see cref="IObjectStorageService"/>.
    /// To switch to AWS S3 or DigitalOcean Spaces, implement IObjectStorageService
    /// in a new class and swap the registration in Shared.Infrastructure/DependencyInjection.cs.
    /// No other code needs to change.
    /// </summary>
    public class CloudinaryStorageService : IObjectStorageService
    {
        private readonly CloudinarySettings _settings;
        private readonly Cloudinary _cloudinary;

        public CloudinaryStorageService(IOptions<CloudinarySettings> options)
        {
            _settings = options.Value;

            var account = new Account(
                _settings.CloudName,
                _settings.ApiKey,
                _settings.ApiSecret);

            _cloudinary = new Cloudinary(account);
        }

        public (string Signature, long Timestamp, string ApiKey, string CloudName) GenerateUploadSignature(
            string publicId,
            string resourceType,
            long maxFileSize)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Parameters that will be signed
            var signParams = new SortedDictionary<string, object>
            {
                { "public_id",     publicId },
                { "timestamp",     timestamp }
            };

            var signature = _cloudinary.Api.SignParameters(signParams);

            return (signature, timestamp, _settings.ApiKey, _settings.CloudName);
        }

        public string BuildMediaUrl(string publicId)
        {
            var isVideo = publicId.StartsWith("post_video_", StringComparison.OrdinalIgnoreCase);
            var isGif   = publicId.StartsWith("post_gif_", StringComparison.OrdinalIgnoreCase);
            
            var resourceType = isVideo ? "video" : "image";
            var extension = isVideo ? ".mp4" : (isGif ? ".gif" : ".jpg");

            return $"https://res.cloudinary.com/{_settings.CloudName}/{resourceType}/upload/{publicId}{extension}";
        }
    }
}
