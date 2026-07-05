using FluentValidation;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using SocialMedia.Application.DTOs.Media;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Enums;

namespace SocialMedia.Application.Services
{
    public class MediaService : IMediaService
    {
        // File size limits
        private const long ImageMaxBytes = 5L * 1024 * 1024;    // 5 MB
        private const long GifMaxBytes   = 5L * 1024 * 1024;    // 5 MB
        private const long VideoMaxBytes = 500L * 1024 * 1024;  // 500 MB

        private static readonly TimeSpan PendingMediaTtl = TimeSpan.FromHours(1);

        private readonly IObjectStorageService _storageService;
        private readonly IConnectionMultiplexer _redis;
        private readonly IValidator<GenerateUploadSignaturesRequest> _validator;
        private readonly ILogger<MediaService> _logger;

        public MediaService(
            IObjectStorageService storageService,
            IConnectionMultiplexer redis,
            IValidator<GenerateUploadSignaturesRequest> validator,
            ILogger<MediaService> logger)
        {
            _storageService = storageService;
            _redis = redis;
            _validator = validator;
            _logger = logger;
        }

        public async Task<ApiResponse<GenerateUploadSignaturesResponse>> GenerateUploadSignaturesAsync(
            GenerateUploadSignaturesRequest request,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("GenerateUploadSignatures validation failed for user {UserId}: {Errors}",
                    userId, validationResult.ToString());
                return ApiResponse<GenerateUploadSignaturesResponse>.Failure(ErrorCode.ValidationError);
            }

            var signatures = new List<MediaUploadSignatureItem>(request.Items.Count);
            var publicIds  = new List<string>(request.Items.Count);

            foreach (var item in request.Items)
            {
                var publicId = BuildPublicId(userId, item.FileType);

                // Map the domain-specific MediaType to the storage-agnostic primitives
                // that IObjectStorageService (in Shared) understands.
                var (resourceType, maxBytes) = MapMediaType(item.FileType);

                var (signature, timestamp, apiKey, cloudName) =
                    _storageService.GenerateUploadSignature(publicId, resourceType, maxBytes);

                signatures.Add(new MediaUploadSignatureItem
                {
                    PublicId   = publicId,
                    Signature  = signature,
                    Timestamp  = timestamp,
                    ApiKey     = apiKey,
                    CloudName  = cloudName
                });

                publicIds.Add(publicId);
            }

            await RegisterPendingMediaAsync(userId, publicIds);

            _logger.LogInformation("Generated {Count} upload signature(s) for user {UserId}", signatures.Count, userId);

            return ApiResponse<GenerateUploadSignaturesResponse>.Success(
                new GenerateUploadSignaturesResponse { Signatures = signatures });
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private static string BuildPublicId(Guid userId, MediaType mediaType)
        {
            var prefix = mediaType switch
            {
                MediaType.Image => "image",
                MediaType.Gif   => "gif",
                MediaType.Video => "video",
                _               => "media"
            };
            return $"post_{prefix}_{userId}_{Guid.NewGuid():N}";
        }

        /// <summary>
        /// Maps a SocialMedia-domain MediaType to the provider-agnostic
        /// (resourceType string, maxFileSize bytes) that IObjectStorageService accepts.
        /// </summary>
        private static (string ResourceType, long MaxBytes) MapMediaType(MediaType mediaType) =>
            mediaType switch
            {
                MediaType.Video => ("video", VideoMaxBytes),
                MediaType.Gif   => ("image", GifMaxBytes),    // Cloudinary treats GIFs as images
                _               => ("image", ImageMaxBytes)
            };

        private async Task RegisterPendingMediaAsync(Guid userId, List<string> publicIds)
        {
            var db       = _redis.GetDatabase();
            var redisKey = $"pending_media:{userId}";

            var values = publicIds.Select(id => (RedisValue)id).ToArray();
            await db.SetAddAsync(redisKey, values);
            await db.KeyExpireAsync(redisKey, PendingMediaTtl);

            _logger.LogDebug("Registered {Count} pending media IDs in Redis for user {UserId} (TTL 1h)",
                publicIds.Count, userId);
        }
    }
}
