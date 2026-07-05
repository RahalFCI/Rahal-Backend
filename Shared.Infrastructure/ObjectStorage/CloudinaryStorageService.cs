using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
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

        public async Task DeleteMediaAsync(string mediaUrl, CancellationToken cancellationToken = default)
        {
            var (publicId, resourceType) = ParseMediaUrl(mediaUrl);
            if (string.IsNullOrWhiteSpace(publicId))
            {
                return;
            }

            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = resourceType
            };

            await _cloudinary.DestroyAsync(deletionParams);
        }

        private static (string PublicId, ResourceType ResourceType) ParseMediaUrl(string mediaUrl)
        {
            if (!Uri.TryCreate(mediaUrl, UriKind.Absolute, out var uri))
            {
                return (string.Empty, ResourceType.Image);
            }

            var segments = uri.AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length < 3 || !segments[1].Equals("upload", StringComparison.OrdinalIgnoreCase))
            {
                return (string.Empty, ResourceType.Image);
            }

            var resourceType = segments[0].Equals("video", StringComparison.OrdinalIgnoreCase)
                ? ResourceType.Video
                : ResourceType.Image;

            var publicIdSegments = segments
                .Skip(2)
                .Where(segment => !segment.StartsWith("v", StringComparison.OrdinalIgnoreCase) || !segment.Skip(1).All(char.IsDigit))
                .ToArray();

            var publicId = string.Join('/', publicIdSegments);
            var extensionIndex = publicId.LastIndexOf('.');
            if (extensionIndex > 0)
            {
                publicId = publicId[..extensionIndex];
            }

            return (publicId, resourceType);
        }
    }
}
