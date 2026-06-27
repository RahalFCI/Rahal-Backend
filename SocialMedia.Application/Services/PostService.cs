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
using System.Text.Json;

namespace SocialMedia.Application.Services
{
    public class PostService : IPostService
    {
        private const int MaxFeedSize = 500;

        private static readonly TimeSpan PostCacheTtl = TimeSpan.FromDays(7);

        private static readonly TimeSpan FeedCacheTtl = TimeSpan.FromDays(7);

        private static readonly TimeSpan UserLikesTtl = TimeSpan.FromDays(7);

        private const string EmptyUserLikesSentinel = "__EmptyUserLikes__";

        private readonly ISocialMediaRepository<Post> _postRepository;
        private readonly ISocialMediaRepository<Comment> _commentRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IFollowRepository _followRepository;
        private readonly IUserGateway _userGateway;
        private readonly IConnectionMultiplexer _redis;
        private readonly IPublishEndpoint _publisher;
        private readonly ILogger<PostService> _logger;
        private readonly IObjectStorageService _storageService;

        public PostService(
            ISocialMediaRepository<Post> postRepository,
            ISocialMediaRepository<Comment> commentRepository,
            ILikeRepository likeRepository,
            IFollowRepository followRepository,
            IUserGateway userGateway,
            IConnectionMultiplexer redis,
            IPublishEndpoint publisher,
            ILogger<PostService> logger,
            IObjectStorageService storageService)
        {
            _postRepository    = postRepository;
            _commentRepository = commentRepository;
            _likeRepository    = likeRepository;
            _followRepository  = followRepository;
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

            try
            {
                await AddPostToAuthorFeedAsync(db, userId, post.Id, post.CreatedAt, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis feed write failed for author {UserId} and post {PostId} — non-fatal, DB is source of truth", userId, post.Id);
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

        public async Task<ApiResponse<PostResponseDto>> GetPostByIdAsync(
            Guid postId,
            Guid viewerUserId,
            CancellationToken cancellationToken = default)
        {
            var db = _redis.GetDatabase();
            var postCacheKey = $"Post:{postId}";
            var cachedPost = TryMapPostHash(postId, await db.HashGetAllAsync(postCacheKey));

            if (cachedPost is not null)
            {
                var isLiked = await IsPostLikedByUserAsync(db, viewerUserId, postId, cancellationToken);
                return ApiResponse<PostResponseDto>.Success(MapPostResponse(cachedPost, isLiked));
            }

            var post = await GetPostReadModelQuery()
                .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

            if (post is null)
            {
                return ApiResponse<PostResponseDto>.Failure(ErrorCode.NotFound);
            }

            await CachePostAsync(db, post);

            var isPostLiked = await IsPostLikedByUserAsync(db, viewerUserId, postId, cancellationToken);

            return ApiResponse<PostResponseDto>.Success(MapPostResponse(post, isPostLiked));
        }

        public async Task<ApiResponse<string>> DeletePostAsync(
            Guid postId,
            Guid userId,
            bool isAdmin = false,
            CancellationToken cancellationToken = default)
        {
            if (postId == Guid.Empty || userId == Guid.Empty)
            {
                return ApiResponse<string>.Failure(ErrorCode.ValidationError);
            }

            var post = await _postRepository.GetTable()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

            if (post is null || post.IsDeleted)
            {
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            if (!isAdmin && post.UserId != userId)
            {
                return ApiResponse<string>.Failure(ErrorCode.Unauthorized);
            }

            post.IsDeleted = true;
            post.DeletedAt = DateTime.UtcNow;
            post.UpdatedAt = DateTime.UtcNow;

            _postRepository.Update(post);
            await _postRepository.SaveChangesAsync(cancellationToken);

            var db = _redis.GetDatabase();
            try
            {
                var batch = db.CreateBatch();
                var postCacheDeleteTask = batch.KeyDeleteAsync($"Post:{post.Id}");
                var authorFeedRemoveTask = batch.SortedSetRemoveAsync($"Feed:{post.UserId}", post.Id.ToString());

                batch.Execute();
                await Task.WhenAll(postCacheDeleteTask, authorFeedRemoveTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete Post:{PostId} cache or author feed entry after post deletion - non-fatal, DB is source of truth", post.Id);
            }

            try
            {
                await _publisher.Publish(
                    new PostDeletedEvent(post.Id, post.UserId, DateTime.UtcNow),
                    cancellationToken);

                _logger.LogInformation("PostDeletedEvent published for post {PostId}", post.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish PostDeletedEvent for post {PostId} - non-fatal", post.Id);
            }

            return ApiResponse<string>.Success("Post deleted successfully");
        }

        public async Task<ApiResponse<FeedPagedResponse>> GetFeedPaginatedAsync(
            Guid userId,
            long? cursor = null,
            int limit = 20,
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
            {
                return ApiResponse<FeedPagedResponse>.Failure(ErrorCode.ValidationError);
            }

            if (limit <= 0)
            {
                limit = 20;
            }

            var db = _redis.GetDatabase();
            var feedKey = $"Feed:{userId}";
            var cursorTimestamp = cursor ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (await db.SortedSetLengthAsync(feedKey) == 0)
            {
                await HydrateFeedAsync(db, userId, feedKey, cancellationToken);
            }

            var redisPostIds = (await db.SortedSetRangeByScoreAsync(
                    feedKey,
                    double.NegativeInfinity,
                    cursorTimestamp - 1,
                    Exclude.None,
                    Order.Descending,
                    skip: 0,
                    take: limit))
                .Select(value => Guid.TryParse(value.ToString(), out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .ToList();

            var combinedPostIds = new List<Guid>(redisPostIds);
            var seenPostIds = redisPostIds.ToHashSet();

            if (redisPostIds.Count < limit)
            {
                var missingCount = limit - redisPostIds.Count;
                var fallbackPostIds = await GetFeedPostIdsFromDbAsync(
                    userId,
                    cursorTimestamp,
                    missingCount,
                    seenPostIds,
                    cancellationToken);

                foreach (var postId in fallbackPostIds)
                {
                    if (seenPostIds.Add(postId))
                    {
                        combinedPostIds.Add(postId);
                    }
                }
            }

            if (combinedPostIds.Count == 0)
            {
                return ApiResponse<FeedPagedResponse>.Success(new FeedPagedResponse());
            }

            var posts = await GetPostsByIdsWithCacheAsync(db, combinedPostIds, cancellationToken);
            var likedPostIds = await GetLikedPostIdsFromCacheAsync(
                db,
                userId,
                posts.Select(post => post.Id).ToList(),
                cancellationToken);

            var responsePosts = posts
                .OrderByDescending(post => post.CreatedAt)
                .Select(post => MapPostResponse(post, likedPostIds.Contains(post.Id)))
                .ToList();

            return ApiResponse<FeedPagedResponse>.Success(new FeedPagedResponse
            {
                Posts = responsePosts,
                NextCursor = responsePosts.Count == 0
                    ? null
                    : new DateTimeOffset(responsePosts[^1].CreatedAt, TimeSpan.Zero).ToUnixTimeSeconds()
            });
        }

        // this Endpoint/Service doesn't rely on caching, just db indexes
        public async Task<ApiResponse<FeedPagedResponse>> GetUserPostsPaginatedAsync(
            Guid userId,
            Guid viewerUserId,
            long? cursor = null,
            int limit = 20,
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
            {
                return ApiResponse<FeedPagedResponse>.Failure(ErrorCode.ValidationError);
            }

            if (limit <= 0)
            {
                limit = 20;
            }

            var cursorTimestamp = cursor ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var cursorDate = DateTimeOffset.FromUnixTimeSeconds(cursorTimestamp).UtcDateTime;

            var posts = await _postRepository.GetTable()
                .AsNoTracking()
                .Where(post => post.UserId == userId)
                .Where(post => post.CreatedAt < cursorDate)
                .OrderByDescending(post => post.CreatedAt)
                .Take(limit)
                .Select(post => new CachedPost
                {
                    Id = post.Id,
                    Content = post.Content,
                    AuthorId = post.UserId,
                    LikesCount = post.Likes.Count(),
                    CommentsCount = post.Comments.Count(comment => !comment.IsDeleted),
                    IsLikedByThisUser = viewerUserId != Guid.Empty
                        && post.Likes.Any(like => like.UserId == viewerUserId),
                    CreatedAt = post.CreatedAt,
                    IsPublic = post.IsPublic,
                    MediaUrls = post.MediaUrls
                })
                .ToListAsync(cancellationToken);

            var responsePosts = posts
                .Select(post => MapPostResponse(post, post.IsLikedByThisUser))
                .ToList();

            return ApiResponse<FeedPagedResponse>.Success(new FeedPagedResponse
            {
                Posts = responsePosts,
                NextCursor = responsePosts.Count == 0
                    ? null
                    : new DateTimeOffset(responsePosts[^1].CreatedAt, TimeSpan.Zero).ToUnixTimeSeconds()
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
            await EnsureUserLikesCacheHydratedAsync(db, userId, userLikesKey, cancellationToken);

            // SADD returns 1 if the element was added (new like), 0 if it already existed.
            var wasAdded = await db.SetAddAsync(userLikesKey, postId.ToString());
            if (!wasAdded)
            {
                _logger.LogWarning("User {UserId} already liked post {PostId}", userId, postId);
                return ApiResponse<string>.Failure(ErrorCode.ValidationError);
            }

            await db.SetRemoveAsync(userLikesKey, EmptyUserLikesSentinel);

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
                    if (await db.SetLengthAsync(userLikesKey) == 0)
                    {
                        await db.SetAddAsync(userLikesKey, EmptyUserLikesSentinel);
                        await db.KeyExpireAsync(userLikesKey, UserLikesTtl);
                    }

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

                await CachePostAsync(db, new CachedPost
                {
                    Id = post.Id,
                    Content = post.Content,
                    AuthorId = post.UserId,
                    LikesCount = likesCount,
                    CommentsCount = commentsCount,
                    CreatedAt = post.CreatedAt,
                    IsPublic = post.IsPublic,
                    MediaUrls = post.MediaUrls
                });
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
            await EnsureUserLikesCacheHydratedAsync(db, userId, userLikesKey, cancellationToken);

            // SREM returns 1 if element was removed, 0 if it didn't exist
            var wasRemoved = await db.SetRemoveAsync(userLikesKey, postId.ToString());
            if (!wasRemoved)
            {
                _logger.LogWarning("User {UserId} tried to unlike post {PostId} they haven't liked", userId, postId);
                return ApiResponse<string>.Failure(ErrorCode.ValidationError);
            }

            var like = await _likeRepository.GetAsync(userId, postId, cancellationToken);
            if (like is null)
            {
                _logger.LogWarning("Unlike rejected: like relationship from user {UserId} to post {PostId} was not found in DB", userId, postId);
                return ApiResponse<string>.Failure(ErrorCode.ValidationError);
            }

            if (await db.SetLengthAsync(userLikesKey) == 0)
            {
                await db.SetAddAsync(userLikesKey, EmptyUserLikesSentinel);
                await db.KeyExpireAsync(userLikesKey, UserLikesTtl);
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

                await CachePostAsync(db, new CachedPost
                {
                    Id = post.Id,
                    Content = post.Content,
                    AuthorId = post.UserId,
                    LikesCount = likesCount,
                    CommentsCount = commentsCount,
                    CreatedAt = post.CreatedAt,
                    IsPublic = post.IsPublic,
                    MediaUrls = post.MediaUrls
                });
            }

            // Atomically decrement LikesCount in the cache
            await db.HashDecrementAsync(postCacheKey, "LikesCount", 1);
            _logger.LogDebug("Decremented LikesCount for Post:{PostId}", postId);

            // ── 3. Delete Like from PostgreSQL ────────────────────────────────
            _likeRepository.Remove(like);
            await _likeRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("User {UserId} unliked post {PostId}", userId, postId);

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

                await CachePostAsync(db, new CachedPost
                {
                    Id = post.Id,
                    Content = post.Content,
                    AuthorId = post.UserId,
                    LikesCount = likesCount,
                    CommentsCount = commentsCount,
                    CreatedAt = post.CreatedAt,
                    IsPublic = post.IsPublic,
                    MediaUrls = post.MediaUrls
                });
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
            bool isAdmin = false,
            CancellationToken cancellationToken = default)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId, cancellationToken);

            // ── 1. Validation ──────────────────────────────────────────────────
            if (comment is null || comment.IsDeleted)
            {
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            if (!isAdmin && comment.UserId != userId)
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

                    await CachePostAsync(db, new CachedPost
                    {
                        Id = post.Id,
                        Content = post.Content,
                        AuthorId = post.UserId,
                        LikesCount = likesCount,
                        CommentsCount = commentsCount,
                        CreatedAt = post.CreatedAt,
                        IsPublic = post.IsPublic,
                        MediaUrls = post.MediaUrls
                    });
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

        private IQueryable<CachedPost> GetPostReadModelQuery()
        {
            return _postRepository.GetTable()
                .AsNoTracking()
                .Select(post => new CachedPost
                {
                    Id = post.Id,
                    Content = post.Content,
                    AuthorId = post.UserId,
                    LikesCount = post.Likes.Count(),
                    CommentsCount = post.Comments.Count(comment => !comment.IsDeleted),
                    CreatedAt = post.CreatedAt,
                    IsPublic = post.IsPublic,
                    MediaUrls = post.MediaUrls
                });
        }

        private async Task HydrateFeedAsync(
            IDatabase db,
            Guid userId,
            RedisKey feedKey,
            CancellationToken cancellationToken)
        {
            var followeeIds = await _followRepository.GetFolloweeIdsByFollowerAsync(userId, cancellationToken);
            var feedAuthorIds = followeeIds
                .Append(userId)
                .Distinct()
                .ToList();

            var feedEntries = await _postRepository.GetTable()
                .AsNoTracking()
                .Where(post => feedAuthorIds.Contains(post.UserId))
                .OrderByDescending(post => post.CreatedAt)
                .Take(MaxFeedSize)
                .Select(post => new
                {
                    post.Id,
                    post.CreatedAt
                })
                .ToListAsync(cancellationToken);

            if (feedEntries.Count == 0)
            {
                return;
            }

            var redisEntries = feedEntries
                .Select(post => new SortedSetEntry(
                    post.Id.ToString(),
                    new DateTimeOffset(post.CreatedAt, TimeSpan.Zero).ToUnixTimeSeconds()))
                .ToArray();

            await db.SortedSetAddAsync(feedKey, redisEntries);
            await db.SortedSetRemoveRangeByRankAsync(feedKey, 0, -501);
            await db.KeyExpireAsync(feedKey, FeedCacheTtl);
        }

        private async Task AddPostToAuthorFeedAsync(
            IDatabase db,
            Guid authorId,
            Guid postId,
            DateTime createdAt,
            CancellationToken cancellationToken)
        {
            var feedKey = (RedisKey)$"Feed:{authorId}";
            if (await db.SortedSetLengthAsync(feedKey) == 0)
            {
                await HydrateFeedAsync(db, authorId, feedKey, cancellationToken);
            }

            var score = new DateTimeOffset(createdAt, TimeSpan.Zero).ToUnixTimeSeconds();
            await db.SortedSetAddAsync(feedKey, postId.ToString(), score);
            await db.SortedSetRemoveRangeByRankAsync(feedKey, 0, -501);
            await db.KeyExpireAsync(feedKey, FeedCacheTtl);
        }

        private async Task<List<Guid>> GetFeedPostIdsFromDbAsync(
            Guid userId,
            long cursorTimestamp,
            int limit,
            HashSet<Guid> excludedPostIds,
            CancellationToken cancellationToken)
        {
            var followeeIds = await _followRepository.GetFolloweeIdsByFollowerAsync(userId, cancellationToken);
            var feedAuthorIds = followeeIds
                .Append(userId)
                .Distinct()
                .ToList();

            var cursorDate = DateTimeOffset.FromUnixTimeSeconds(cursorTimestamp).UtcDateTime;

            return await _postRepository.GetTable()
                .AsNoTracking()
                .Where(post => feedAuthorIds.Contains(post.UserId))
                .Where(post => post.CreatedAt < cursorDate)
                .Where(post => !excludedPostIds.Contains(post.Id))
                .OrderByDescending(post => post.CreatedAt)
                .Take(limit)
                .Select(post => post.Id)
                .ToListAsync(cancellationToken);
        }

        private async Task<List<CachedPost>> GetPostsByIdsWithCacheAsync(
            IDatabase db,
            IReadOnlyCollection<Guid> postIds,
            CancellationToken cancellationToken)
        {
            var distinctPostIds = postIds.Distinct().ToList();
            var batch = db.CreateBatch();
            var cacheTasks = distinctPostIds.ToDictionary(
                postId => postId,
                postId => batch.HashGetAllAsync($"Post:{postId}"));

            batch.Execute();
            await Task.WhenAll(cacheTasks.Values);

            var posts = new List<CachedPost>();
            var missingPostIds = new List<Guid>();

            foreach (var (postId, task) in cacheTasks)
            {
                var cachedPost = TryMapPostHash(postId, await task);
                if (cachedPost is null)
                {
                    missingPostIds.Add(postId);
                    continue;
                }

                posts.Add(cachedPost);
            }

            if (missingPostIds.Count == 0)
            {
                return posts;
            }

            var hydratedPosts = await GetPostReadModelQuery()
                .Where(post => missingPostIds.Contains(post.Id))
                .ToListAsync(cancellationToken);

            if (hydratedPosts.Count > 0)
            {
                await CachePostsAsync(db, hydratedPosts);
                posts.AddRange(hydratedPosts);
            }

            return posts;
        }

        private async Task<bool> IsPostLikedByUserAsync(
            IDatabase db,
            Guid userId,
            Guid postId,
            CancellationToken cancellationToken)
        {
            if (userId == Guid.Empty)
            {
                return false;
            }

            var userLikesKey = (RedisKey)$"UserLikes:{userId}";
            await EnsureUserLikesCacheHydratedAsync(db, userId, userLikesKey, cancellationToken);

            return await db.SetContainsAsync(userLikesKey, postId.ToString());
        }

        private async Task<HashSet<Guid>> GetLikedPostIdsFromCacheAsync(
            IDatabase db,
            Guid userId,
            IReadOnlyCollection<Guid> postIds,
            CancellationToken cancellationToken)
        {
            if (userId == Guid.Empty || postIds.Count == 0)
            {
                return new HashSet<Guid>();
            }

            var distinctPostIds = postIds.Distinct().ToList();
            var userLikesKey = (RedisKey)$"UserLikes:{userId}";
            await EnsureUserLikesCacheHydratedAsync(db, userId, userLikesKey, cancellationToken);

            var batch = db.CreateBatch();
            var containsTasks = distinctPostIds.ToDictionary(
                postId => postId,
                postId => batch.SetContainsAsync(userLikesKey, postId.ToString()));

            batch.Execute();
            await Task.WhenAll(containsTasks.Values);

            return containsTasks
                .Where(entry => entry.Value.Result)
                .Select(entry => entry.Key)
                .ToHashSet();
        }

        private async Task EnsureUserLikesCacheHydratedAsync(
            IDatabase db,
            Guid userId,
            RedisKey userLikesKey,
            CancellationToken cancellationToken)
        {
            if (userId == Guid.Empty || await db.KeyExistsAsync(userLikesKey))
            {
                return;
            }

            _logger.LogDebug("Cache miss for UserLikes:{UserId} — hydrating from DB", userId);
            var likedPostIds = await _likeRepository.GetPostIdsLikedByUserAsync(userId, cancellationToken);

            var redisValues = likedPostIds.Count == 0
                ? new[] { (RedisValue)EmptyUserLikesSentinel }
                : likedPostIds.Select(id => (RedisValue)id.ToString()).ToArray();

            await db.SetAddAsync(userLikesKey, redisValues);
            await db.KeyExpireAsync(userLikesKey, UserLikesTtl);
        }

        private static PostResponseDto MapPostResponse(CachedPost post, bool isLikedByThisUser = false)
        {
            return new PostResponseDto
            {
                Id = post.Id,
                Content = post.Content,
                AuthorId = post.AuthorId,
                LikesCount = post.LikesCount,
                CommentsCount = post.CommentsCount,
                IsLikedByThisUser = isLikedByThisUser,
                CreatedAt = post.CreatedAt,
                IsPublic = post.IsPublic,
                MediaUrls = post.MediaUrls
            };
        }

        private static CachedPost? TryMapPostHash(Guid postId, HashEntry[] hashEntries)
        {
            if (hashEntries.Length == 0)
            {
                return null;
            }

            var fields = hashEntries.ToDictionary(
                entry => entry.Name.ToString(),
                entry => entry.Value);

            var authorValue = fields.TryGetValue("AuthorId", out var authorId)
                ? authorId
                : fields.GetValueOrDefault("UserId");

            if (!Guid.TryParse(authorValue.ToString(), out var parsedAuthorId))
            {
                return null;
            }

            var idValue = fields.GetValueOrDefault("Id");
            var parsedId = Guid.TryParse(idValue.ToString(), out var hashPostId)
                ? hashPostId
                : postId;

            if (!long.TryParse(fields.GetValueOrDefault("CreatedAt").ToString(), out var createdAtTimestamp))
            {
                return null;
            }

            _ = int.TryParse(fields.GetValueOrDefault("LikesCount").ToString(), out var likesCount);
            _ = int.TryParse(fields.GetValueOrDefault("CommentsCount").ToString(), out var commentsCount);
            _ = bool.TryParse(fields.GetValueOrDefault("IsPublic").ToString(), out var isPublic);

            var mediaUrls = new List<string>();
            var mediaUrlsValue = fields.GetValueOrDefault("MediaUrls").ToString();
            if (!string.IsNullOrWhiteSpace(mediaUrlsValue))
            {
                mediaUrls = JsonSerializer.Deserialize<List<string>>(mediaUrlsValue) ?? new List<string>();
            }

            return new CachedPost
            {
                Id = parsedId,
                Content = fields.GetValueOrDefault("Content").ToString(),
                AuthorId = parsedAuthorId,
                LikesCount = likesCount,
                CommentsCount = commentsCount,
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(createdAtTimestamp).UtcDateTime,
                IsPublic = isPublic,
                MediaUrls = mediaUrls
            };
        }

        private static async Task CachePostsAsync(IDatabase db, IReadOnlyCollection<CachedPost> posts)
        {
            var batch = db.CreateBatch();
            var tasks = new List<Task>(posts.Count * 2);

            foreach (var post in posts)
            {
                var key = (RedisKey)$"Post:{post.Id}";
                tasks.Add(batch.HashSetAsync(key, ToRedisHashEntries(post)));
                tasks.Add(batch.KeyExpireAsync(key, PostCacheTtl));
            }

            batch.Execute();
            await Task.WhenAll(tasks);
        }

        private static Task CachePostAsync(IDatabase db, CachedPost post)
        {
            var key = $"Post:{post.Id}";

            return Task.WhenAll(
                db.HashSetAsync(key, ToRedisHashEntries(post)),
                db.KeyExpireAsync(key, PostCacheTtl));
        }

        private static async Task CachePostAsync(IDatabase db, Post post)
        {
            var key = $"Post:{post.Id}";
            var cachedPost = new CachedPost
            {
                Id = post.Id,
                Content = post.Content,
                AuthorId = post.UserId,
                LikesCount = 0,
                CommentsCount = 0,
                CreatedAt = post.CreatedAt,
                IsPublic = post.IsPublic,
                MediaUrls = post.MediaUrls
            };

            await db.HashSetAsync(key, ToRedisHashEntries(cachedPost));
            await db.KeyExpireAsync(key, PostCacheTtl);
        }

        private static HashEntry[] ToRedisHashEntries(CachedPost post)
        {
            return new HashEntry[]
            {
                new("Id", post.Id.ToString()),
                new("Content", post.Content),
                new("AuthorId", post.AuthorId.ToString()),
                new("UserId", post.AuthorId.ToString()),
                new("LikesCount", post.LikesCount),
                new("CommentsCount", post.CommentsCount),
                new("CreatedAt", new DateTimeOffset(post.CreatedAt, TimeSpan.Zero).ToUnixTimeSeconds()),
                new("IsPublic", post.IsPublic.ToString()),
                new("MediaUrls", JsonSerializer.Serialize(post.MediaUrls))
            };
        }

        private sealed class CachedPost
        {
            public Guid Id { get; set; }
            public string Content { get; set; } = string.Empty;
            public Guid AuthorId { get; set; }
            public int LikesCount { get; set; }
            public int CommentsCount { get; set; }
            public bool IsLikedByThisUser { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsPublic { get; set; } = true;
            public List<string> MediaUrls { get; set; } = new();
        }
    }
}
