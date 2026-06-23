using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Shared.Application.DTOs;
using Shared.Application.Events.Posts;
using Shared.Domain.Enums;
using SocialMedia.Application.DTOs.Posts;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Application.Services
{
    public class PostService : IPostService
    {
        // Cloudinary base URL template
        private const string CloudinaryBaseUrl = "https://res.cloudinary.com";

        private static readonly TimeSpan PostCacheTtl = TimeSpan.FromDays(14);

        private readonly ISocialMediaRepository<Post> _postRepository;
        private readonly IConnectionMultiplexer _redis;
        private readonly IPublishEndpoint _publisher;
        private readonly ILogger<PostService> _logger;
        private readonly string _cloudName;

        public PostService(
            ISocialMediaRepository<Post> postRepository,
            IConnectionMultiplexer redis,
            IPublishEndpoint publisher,
            ILogger<PostService> logger,
            IConfiguration configuration)
        {
            _postRepository = postRepository;
            _redis          = redis;
            _publisher      = publisher;
            _logger         = logger;

            // Read the already-resolved cloud name from configuration
            // (Cloudinary:CloudName is bound and env-var-replaced in DependencyInjection.cs)
            _cloudName = configuration["Cloudinary:CloudName"]
                         ?? Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME")
                         ?? throw new InvalidOperationException("Cloudinary CloudName is not configured.");
        }

        public async Task<ApiResponse<PostResponse>> CreatePostAsync(
            CreatePostRequest request,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            // ── 1. Basic validation ────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(request.Content) && request.MediaIds.Count == 0)
            {
                _logger.LogWarning("CreatePost rejected: empty content and no media for user {UserId}", userId);
                return ApiResponse<PostResponse>.Failure(ErrorCode.ValidationError);
            }

            if (request.MediaIds.Count > 3)
            {
                _logger.LogWarning("CreatePost rejected: too many media items ({Count}) for user {UserId}", request.MediaIds.Count, userId);
                return ApiResponse<PostResponse>.Failure(ErrorCode.ValidationError);
            }

            // ── 2. Redis security check: verify all public_ids were pre-signed ─
            var db         = _redis.GetDatabase();
            var pendingKey = $"pending_media:{userId}";

            if (request.MediaIds.Count > 0)
            {
                var pendingSet = (await db.SetMembersAsync(pendingKey))
                    .Select(v => v.ToString())
                    .ToHashSet();

                var invalidIds = request.MediaIds
                    .Where(id => !pendingSet.Contains(id))
                    .ToList();

                if (invalidIds.Count > 0)
                {
                    _logger.LogWarning(
                        "CreatePost rejected: {Count} unrecognised media ID(s) for user {UserId}: {Ids}",
                        invalidIds.Count, userId, string.Join(", ", invalidIds));
                    return ApiResponse<PostResponse>.Failure(ErrorCode.Unauthorized);
                }
            }

            // ── 3. Build full Cloudinary HTTPS URLs from public_ids ────────────
            var mediaUrls = request.MediaIds
                .Select(publicId => BuildCloudinaryUrl(publicId))
                .ToList();

            // ── 4. Persist to PostgreSQL ───────────────────────────────────────
            var post = new Post
            {
                UserId    = userId,
                Content   = request.Content,
                IsPublic  = request.IsPublic,
                MediaUrls = mediaUrls,   // Full HTTPS URLs stored in DB, not raw public_ids
                CreatedAt = DateTime.UtcNow
            };

            _postRepository.Add(post);
            await _postRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Post {PostId} persisted for user {UserId}", post.Id, userId);

            // ── 5. Remove used public_ids from Redis (prevent reuse) ───────────
            if (request.MediaIds.Count > 0)
            {
                var redisValues = request.MediaIds.Select(id => (RedisValue)id).ToArray();
                await db.SetRemoveAsync(pendingKey, redisValues);
                _logger.LogDebug("Consumed {Count} media ID(s) from pending set for user {UserId}", redisValues.Length, userId);
            }

            // ── 6. Cache post in Redis (HSET, 14-day TTL) ─────────────────────
            try
            {
                await CachePostAsync(db, post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis cache write failed for post {PostId} — non-fatal, DB is source of truth", post.Id);
            }

            // ── 7. Publish PostCreatedEvent to RabbitMQ ────────────────────────
            try
            {
                await _publisher.Publish(
                    new PostCreatedEvent(post.Id, userId, post.CreatedAt),
                    cancellationToken);

                _logger.LogInformation("PostCreatedEvent published for post {PostId}", post.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish PostCreatedEvent for post {PostId} — non-fatal", post.Id);
            }

            // ── 8. Return 201 ──────────────────────────────────────────────────
            return ApiResponse<PostResponse>.Success(new PostResponse
            {
                Id        = post.Id,
                UserId    = post.UserId,
                Content   = post.Content,
                IsPublic  = post.IsPublic,
                MediaUrls = post.MediaUrls,
                CreatedAt = post.CreatedAt
            });
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>
        /// Converts a Cloudinary public_id to a full HTTPS delivery URL.
        /// Resource type is inferred from the public_id prefix stamped by MediaService:
        ///   "post_video_…" → /video/upload/   |   all others → /image/upload/
        /// </summary>
        private string BuildCloudinaryUrl(string publicId)
        {
            var resourceType = publicId.StartsWith("post_video_", StringComparison.OrdinalIgnoreCase)
                ? "video"
                : "image";

            return $"{CloudinaryBaseUrl}/{_cloudName}/{resourceType}/upload/{publicId}";
        }

        private static async Task CachePostAsync(IDatabase db, Post post)
        {
            var key = $"post:{post.Id}";

            await db.HashSetAsync(key, new HashEntry[]
            {
                new("UserId",        post.UserId.ToString()),
                new("Content",       post.Content),
                new("MediaUrls",     System.Text.Json.JsonSerializer.Serialize(post.MediaUrls)),
                new("IsPublic",      post.IsPublic.ToString()),
                new("LikesCount",    0),
                new("CommentsCount", 0),
                new("CreatedAt",     new DateTimeOffset(post.CreatedAt, TimeSpan.Zero).ToUnixTimeSeconds())
            });

            await db.KeyExpireAsync(key, PostCacheTtl);
        }
    }
}
