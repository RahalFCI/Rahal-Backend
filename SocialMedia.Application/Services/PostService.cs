using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Shared.Application.DTOs;
using Shared.Application.Events.Posts;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using SocialMedia.Application.DTOs.Posts;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Application.Services
{
    public class PostService : IPostService
    {
        private static readonly TimeSpan PostCacheTtl = TimeSpan.FromDays(14);

        private static readonly TimeSpan UserLikesTtl = TimeSpan.FromDays(7);

        private readonly ISocialMediaRepository<Post> _postRepository;
        private readonly ISocialMediaRepository<Comment> _commentRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IUserGateway _userGateway;
        private readonly IConnectionMultiplexer _redis;
        private readonly IPublishEndpoint _publisher;
        private readonly ILogger<PostService> _logger;
        private readonly IObjectStorageService _storageService;

        public PostService(
            ISocialMediaRepository<Post> postRepository,
            ISocialMediaRepository<Comment> commentRepository,
            ILikeRepository likeRepository,
            IUserGateway userGateway,
            IConnectionMultiplexer redis,
            IPublishEndpoint publisher,
            ILogger<PostService> logger,
            IObjectStorageService storageService)
        {
            _postRepository    = postRepository;
            _commentRepository = commentRepository;
            _likeRepository    = likeRepository;
            _userGateway       = userGateway;
            _redis             = redis;
            _publisher         = publisher;
            _logger            = logger;
            _storageService    = storageService;
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
                .Select(publicId => _storageService.BuildMediaUrl(publicId))
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

        public async Task<ApiResponse<string>> LikePostAsync(
            Guid postId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var db           = _redis.GetDatabase();
            var userLikesKey = $"UserLikes:{userId}";
            var postCacheKey = $"Post:{postId}";

            // ── 1. User Like Validation (Redis Set) ───────────────────────────
            // Check for cache miss: if the UserLikes set doesn't exist in Redis,
            // hydrate it from the DB first.
            if (!await db.KeyExistsAsync(userLikesKey))
            {
                _logger.LogDebug("Cache miss for UserLikes:{UserId} — hydrating from DB", userId);
                var likedPostIds = await _likeRepository.GetPostIdsLikedByUserAsync(userId, cancellationToken);

                if (likedPostIds.Count > 0)
                {
                    var redisValues = likedPostIds.Select(id => (RedisValue)id.ToString()).ToArray();
                    await db.SetAddAsync(userLikesKey, redisValues);
                }

                // Set TTL whether the set is empty or not — so we don't query DB on every request
                await db.KeyExpireAsync(userLikesKey, UserLikesTtl);
            }

            // SADD returns 1 if the element was added (new like), 0 if it already existed.
            var wasAdded = await db.SetAddAsync(userLikesKey, postId.ToString());
            if (!wasAdded)
            {
                _logger.LogWarning("User {UserId} already liked post {PostId}", userId, postId);
                return ApiResponse<string>.Failure(ErrorCode.ValidationError);
            }

            // ── 2. Post Cache Hydration + Increment ───────────────────────────
            if (!await db.KeyExistsAsync(postCacheKey))
            {
                _logger.LogDebug("Cache miss for Post:{PostId} — hydrating from DB", postId);

                var post = await _postRepository.GetTable()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

                if (post is null)
                {
                    // Rollback the SADD since the post doesn't exist
                    await db.SetRemoveAsync(userLikesKey, postId.ToString());
                    _logger.LogWarning("Like rejected: post {PostId} not found", postId);
                    return ApiResponse<string>.Failure(ErrorCode.NotFound);
                }

                var likesCount = await _postRepository.GetTable()
                    .AsNoTracking()
                    .Where(p => p.Id == postId)
                    .SelectMany(p => p.Likes)
                    .CountAsync(cancellationToken);

                var commentsCount = await _commentRepository.GetTable()
                    .AsNoTracking()
                    .Where(c => c.PostId == postId && !c.IsDeleted)
                    .CountAsync(cancellationToken);

                await db.HashSetAsync(postCacheKey, new HashEntry[]
                {
                    new("UserId",        post.UserId.ToString()),
                    new("Content",       post.Content),
                    new("MediaUrls",     System.Text.Json.JsonSerializer.Serialize(post.MediaUrls)),
                    new("IsPublic",      post.IsPublic.ToString()),
                    new("LikesCount",    likesCount),
                    new("CommentsCount", commentsCount),
                    new("CreatedAt",     new DateTimeOffset(post.CreatedAt, TimeSpan.Zero).ToUnixTimeSeconds())
                });

                await db.KeyExpireAsync(postCacheKey, PostCacheTtl);
            }

            // Atomically increment LikesCount in the cache
            await db.HashIncrementAsync(postCacheKey, "LikesCount", 1);
            _logger.LogDebug("Incremented LikesCount for Post:{PostId}", postId);

            // ── 3. Persist Like to PostgreSQL ─────────────────────────────────
            var like = new Like
            {
                UserId    = userId,
                PostId    = postId,
                CreatedAt = DateTime.UtcNow
            };

            _likeRepository.Add(like);
            await _likeRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} liked post {PostId}", userId, postId);

            // ── 4. Publish PostLikedEvent to RabbitMQ ─────────────────────────
            try
            {
                await _publisher.Publish(
                    new PostLikedEvent(postId, userId, DateTime.UtcNow),
                    cancellationToken);

                _logger.LogInformation("PostLikedEvent published for post {PostId}", postId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish PostLikedEvent for post {PostId} — non-fatal", postId);
            }

            return ApiResponse<string>.Success("Post liked successfully");
        }
        public async Task<ApiResponse<string>> UnlikePostAsync(
            Guid postId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var db           = _redis.GetDatabase();
            var userLikesKey = $"UserLikes:{userId}";
            var postCacheKey = $"Post:{postId}";

            // ── 1. User Like Validation (Redis Set) ───────────────────────────
            if (!await db.KeyExistsAsync(userLikesKey))
            {
                _logger.LogDebug("Cache miss for UserLikes:{UserId} — hydrating from DB", userId);
                var likedPostIds = await _likeRepository.GetPostIdsLikedByUserAsync(userId, cancellationToken);

                if (likedPostIds.Count > 0)
                {
                    var redisValues = likedPostIds.Select(id => (RedisValue)id.ToString()).ToArray();
                    await db.SetAddAsync(userLikesKey, redisValues);
                }

                await db.KeyExpireAsync(userLikesKey, UserLikesTtl);
            }

            // SREM returns 1 if element was removed, 0 if it didn't exist
            var wasRemoved = await db.SetRemoveAsync(userLikesKey, postId.ToString());
            if (!wasRemoved)
            {
                _logger.LogWarning("User {UserId} tried to unlike post {PostId} they haven't liked", userId, postId);
                return ApiResponse<string>.Failure(ErrorCode.ValidationError);
            }

            // ── 2. Post Cache Hydration + Decrement ───────────────────────────
            if (!await db.KeyExistsAsync(postCacheKey))
            {
                _logger.LogDebug("Cache miss for Post:{PostId} — hydrating from DB", postId);

                var post = await _postRepository.GetTable()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

                if (post is null)
                {
                    // Rollback the SREM
                    await db.SetAddAsync(userLikesKey, postId.ToString());
                    _logger.LogWarning("Unlike rejected: post {PostId} not found", postId);
                    return ApiResponse<string>.Failure(ErrorCode.NotFound);
                }

                var likesCount = await _postRepository.GetTable()
                    .AsNoTracking()
                    .Where(p => p.Id == postId)
                    .SelectMany(p => p.Likes)
                    .CountAsync(cancellationToken);

                var commentsCount = await _commentRepository.GetTable()
                    .AsNoTracking()
                    .Where(c => c.PostId == postId && !c.IsDeleted)
                    .CountAsync(cancellationToken);

                await db.HashSetAsync(postCacheKey, new HashEntry[]
                {
                    new("UserId",        post.UserId.ToString()),
                    new("Content",       post.Content),
                    new("MediaUrls",     System.Text.Json.JsonSerializer.Serialize(post.MediaUrls)),
                    new("IsPublic",      post.IsPublic.ToString()),
                    new("LikesCount",    likesCount),
                    new("CommentsCount", commentsCount),
                    new("CreatedAt",     new DateTimeOffset(post.CreatedAt, TimeSpan.Zero).ToUnixTimeSeconds())
                });

                await db.KeyExpireAsync(postCacheKey, PostCacheTtl);
            }

            // Atomically decrement LikesCount in the cache
            await db.HashDecrementAsync(postCacheKey, "LikesCount", 1);
            _logger.LogDebug("Decremented LikesCount for Post:{PostId}", postId);

            // ── 3. Delete Like from PostgreSQL ────────────────────────────────
            var like = await _likeRepository.GetAsync(userId, postId, cancellationToken);
            if (like is not null)
            {
                _likeRepository.Remove(like);
                await _likeRepository.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("User {UserId} unliked post {PostId}", userId, postId);
            }

            return ApiResponse<string>.Success("Post unliked successfully");
        }
        public async Task<ApiResponse<SocialMedia.Application.DTOs.Comments.CommentResponse>> CreateCommentAsync(
            Guid postId,
            Guid userId,
            SocialMedia.Application.DTOs.Comments.CreateCommentRequest request,
            CancellationToken cancellationToken = default)
        {
            var db = _redis.GetDatabase();
            var postCacheKey = $"Post:{postId}";

            // ── 1. Post Cache Hydration + Increment ───────────────────────────
            if (!await db.KeyExistsAsync(postCacheKey))
            {
                _logger.LogDebug("Cache miss for Post:{PostId} — hydrating from DB", postId);

                var post = await _postRepository.GetTable()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

                if (post is null)
                {
                    _logger.LogWarning("Comment rejected: post {PostId} not found", postId);
                    return ApiResponse<SocialMedia.Application.DTOs.Comments.CommentResponse>.Failure(ErrorCode.NotFound);
                }

                var likesCount = await _postRepository.GetTable()
                    .AsNoTracking()
                    .Where(p => p.Id == postId)
                    .SelectMany(p => p.Likes)
                    .CountAsync(cancellationToken);

                var commentsCount = await _commentRepository.GetTable()
                    .AsNoTracking()
                    .Where(c => c.PostId == postId && !c.IsDeleted)
                    .CountAsync(cancellationToken);

                await db.HashSetAsync(postCacheKey, new HashEntry[]
                {
                    new("UserId",        post.UserId.ToString()),
                    new("Content",       post.Content),
                    new("MediaUrls",     System.Text.Json.JsonSerializer.Serialize(post.MediaUrls)),
                    new("IsPublic",      post.IsPublic.ToString()),
                    new("LikesCount",    likesCount),
                    new("CommentsCount", commentsCount),
                    new("CreatedAt",     new DateTimeOffset(post.CreatedAt, TimeSpan.Zero).ToUnixTimeSeconds())
                });

                await db.KeyExpireAsync(postCacheKey, PostCacheTtl);
            }

            // Atomically increment CommentsCount in the cache
            await db.HashIncrementAsync(postCacheKey, "CommentsCount", 1);
            _logger.LogDebug("Incremented CommentsCount for Post:{PostId}", postId);

            // ── 2. Persist Comment + RepliesCount update in a single transaction ────
            // EF Core's SaveChangesAsync wraps all tracked changes in one implicit
            // transaction. We stage both the new comment and the parent's counter
            // update as tracked changes, then flush them together atomically.

            var comment = new Comment
            {
                PostId          = postId,
                UserId          = userId,
                Content         = request.Content,
                ParentCommentId = request.ParentCommentId,
                CreatedAt       = DateTime.UtcNow
            };

            _commentRepository.Add(comment);

            // If this is a reply, fetch the parent and increment its counter.
            // Both changes are staged in the same DbContext and committed together.
            if (request.ParentCommentId.HasValue)
            {
                var parent = await _commentRepository.GetByIdAsync(
                    request.ParentCommentId.Value, cancellationToken);

                if (parent is not null)
                {
                    parent.RepliesCount++;
                    _commentRepository.Update(parent);
                    _logger.LogDebug("Staged RepliesCount increment for parent comment {ParentCommentId}", parent.Id);
                }
            }

            // Single SaveChangesAsync → single BEGIN/COMMIT wrapping both INSERT + UPDATE.
            await _commentRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} commented on post {PostId}", userId, postId);

            // ── 3. Map and Return Response ────────────────────────────────────
            var response = new SocialMedia.Application.DTOs.Comments.CommentResponse
            {
                Id              = comment.Id,
                PostId          = comment.PostId,
                UserId          = comment.UserId,
                ParentCommentId = comment.ParentCommentId,
                Content         = comment.Content,
                CreatedAt       = comment.CreatedAt
            };

            return ApiResponse<SocialMedia.Application.DTOs.Comments.CommentResponse>.Success(response);
        }

        public async Task<ApiResponse<SocialMedia.Application.DTOs.Comments.CommentResponse>> EditCommentAsync(
            Guid commentId,
            Guid userId,
            SocialMedia.Application.DTOs.Comments.EditCommentRequest request,
            CancellationToken cancellationToken = default)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId, cancellationToken);

            if (comment is null || comment.IsDeleted)
            {
                return ApiResponse<SocialMedia.Application.DTOs.Comments.CommentResponse>.Failure(ErrorCode.NotFound);
            }

            if (comment.UserId != userId)
            {
                return ApiResponse<SocialMedia.Application.DTOs.Comments.CommentResponse>.Failure(ErrorCode.Unauthorized);
            }

            comment.Content = request.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            _commentRepository.Update(comment);
            await _commentRepository.SaveChangesAsync(cancellationToken);

            return ApiResponse<SocialMedia.Application.DTOs.Comments.CommentResponse>.Success(
                new SocialMedia.Application.DTOs.Comments.CommentResponse
                {
                    Id = comment.Id,
                    PostId = comment.PostId,
                    UserId = comment.UserId,
                    UserDisplayName = null, // Not fetched during edit to save DB trip
                    ParentCommentId = comment.ParentCommentId,
                    Content = comment.Content,
                    RepliesCount = comment.RepliesCount,
                    CreatedAt = comment.CreatedAt
                });
        }

        public async Task<ApiResponse<string>> DeleteCommentAsync(
            Guid commentId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId, cancellationToken);

            // ── 1. Validation ──────────────────────────────────────────────────
            if (comment is null || comment.IsDeleted)
            {
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            if (comment.UserId != userId)
            {
                return ApiResponse<string>.Failure(ErrorCode.Unauthorized);
            }

            var postId = comment.PostId;
            var db = _redis.GetDatabase();
            var postCacheKey = $"Post:{postId}";

            // ── 2. Soft Delete in PostgreSQL ──────────────────────────────────
            // We stage both the soft-delete and the parent's RepliesCount decrement
            // as tracked changes, then flush them together atomically in one transaction.

            comment.IsDeleted = true;
            _commentRepository.Update(comment);

            if (comment.ParentCommentId.HasValue)
            {
                var parent = await _commentRepository.GetByIdAsync(
                    comment.ParentCommentId.Value, cancellationToken);

                if (parent is not null && parent.RepliesCount > 0)
                {
                    parent.RepliesCount--;
                    _commentRepository.Update(parent);
                    _logger.LogDebug("Staged RepliesCount decrement for parent comment {ParentCommentId}", parent.Id);
                }
            }

            await _commentRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} soft-deleted comment {CommentId}", userId, commentId);

            // ── 3. Post Cache Hydration + Decrement ───────────────────────────
            if (!await db.KeyExistsAsync(postCacheKey))
            {
                _logger.LogDebug("Cache miss for Post:{PostId} — hydrating from DB", postId);

                var post = await _postRepository.GetTable()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

                if (post is not null)
                {
                    var likesCount = await _postRepository.GetTable()
                        .AsNoTracking()
                        .Where(p => p.Id == postId)
                        .SelectMany(p => p.Likes)
                        .CountAsync(cancellationToken);

                    var commentsCount = await _commentRepository.GetTable()
                        .AsNoTracking()
                        .Where(c => c.PostId == postId && !c.IsDeleted)
                        .CountAsync(cancellationToken);

                    await db.HashSetAsync(postCacheKey, new HashEntry[]
                    {
                        new("UserId",        post.UserId.ToString()),
                        new("Content",       post.Content),
                        new("MediaUrls",     System.Text.Json.JsonSerializer.Serialize(post.MediaUrls)),
                        new("IsPublic",      post.IsPublic.ToString()),
                        new("LikesCount",    likesCount),
                        new("CommentsCount", commentsCount),
                        new("CreatedAt",     new DateTimeOffset(post.CreatedAt, TimeSpan.Zero).ToUnixTimeSeconds())
                    });

                    await db.KeyExpireAsync(postCacheKey, PostCacheTtl);
                }
            }

            // Decrement CommentsCount and ensure it doesn't go below 0
            var newCount = await db.HashDecrementAsync(postCacheKey, "CommentsCount", 1);
            if (newCount < 0)
            {
                await db.HashSetAsync(postCacheKey, "CommentsCount", 0);
            }

            _logger.LogDebug("Decremented CommentsCount for Post:{PostId} to {Count}", postId, Math.Max(0, newCount));

            return ApiResponse<string>.Success(string.Empty);
        }

        public async Task<ApiResponse<SocialMedia.Application.DTOs.Comments.CommentPagedResponse>> GetRootCommentsAsync(
            Guid postId,
            DateTime? cursor,
            int limit = 20,
            CancellationToken cancellationToken = default)
        {
            var query = _commentRepository.GetTable()
                .IgnoreQueryFilters()
                .Where(c => c.PostId == postId && c.ParentCommentId == null)
                .Where(c => c.IsDeleted == false || (c.IsDeleted == true && c.RepliesCount > 0));

            if (cursor.HasValue)
            {
                query = query.Where(c => c.CreatedAt < cursor.Value);
            }

            var comments = await query
                .OrderByDescending(c => c.CreatedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);

            var response = await MapCommentsAsync(comments, cancellationToken);
            var nextCursor = comments.Count > 0 ? comments.Last().CreatedAt : (DateTime?)null;

            return ApiResponse<SocialMedia.Application.DTOs.Comments.CommentPagedResponse>.Success(
                new SocialMedia.Application.DTOs.Comments.CommentPagedResponse
                {
                    Comments = response,
                    NextCursor = nextCursor
                });
        }

        public async Task<ApiResponse<SocialMedia.Application.DTOs.Comments.CommentPagedResponse>> GetCommentRepliesAsync(
            Guid commentId,
            DateTime? cursor,
            int limit = 20,
            CancellationToken cancellationToken = default)
        {
            var query = _commentRepository.GetTable()
                .IgnoreQueryFilters()
                .Where(c => c.ParentCommentId == commentId)
                .Where(c => c.IsDeleted == false || (c.IsDeleted == true && c.RepliesCount > 0));

            if (cursor.HasValue)
            {
                // Ascending chronological order: greater than cursor
                query = query.Where(c => c.CreatedAt > cursor.Value);
            }

            var comments = await query
                .OrderBy(c => c.CreatedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);

            var response = await MapCommentsAsync(comments, cancellationToken);
            var nextCursor = comments.Count > 0 ? comments.Last().CreatedAt : (DateTime?)null;

            return ApiResponse<SocialMedia.Application.DTOs.Comments.CommentPagedResponse>.Success(
                new SocialMedia.Application.DTOs.Comments.CommentPagedResponse
                {
                    Comments = response,
                    NextCursor = nextCursor
                });
        }

        private async Task<List<SocialMedia.Application.DTOs.Comments.CommentResponse>> MapCommentsAsync(
            List<Comment> comments,
            CancellationToken cancellationToken)
        {
            var activeUserIds = comments.Where(c => !c.IsDeleted).Select(c => c.UserId).Distinct().ToList();
            var userNames = await _userGateway.GetUserDisplayNamesAsync(activeUserIds, cancellationToken);

            return comments.Select(c => 
            {
                if (c.IsDeleted)
                {
                    return new SocialMedia.Application.DTOs.Comments.CommentResponse
                    {
                        Id = c.Id,
                        PostId = c.PostId,
                        ParentCommentId = c.ParentCommentId,
                        RepliesCount = c.RepliesCount,
                        CreatedAt = c.CreatedAt,
                        Content = "[Deleted]",
                        UserId = null,
                        UserDisplayName = "[Deleted User]"
                    };
                }

                return new SocialMedia.Application.DTOs.Comments.CommentResponse
                {
                    Id = c.Id,
                    PostId = c.PostId,
                    ParentCommentId = c.ParentCommentId,
                    RepliesCount = c.RepliesCount,
                    CreatedAt = c.CreatedAt,
                    Content = c.Content,
                    UserId = c.UserId,
                    UserDisplayName = userNames.TryGetValue(c.UserId, out var name) ? name : "Unknown User"
                };
            }).ToList();
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static async Task CachePostAsync(IDatabase db, Post post)
        {
            var key = $"Post:{post.Id}";

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
