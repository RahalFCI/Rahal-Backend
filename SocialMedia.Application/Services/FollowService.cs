using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Events.Users;
using Shared.Application.Pagination;
using Shared.Domain.Enums;
using SocialMedia.Application.DTOs.Follows;
using SocialMedia.Application.DTOs.Users;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Entities;
using StackExchange.Redis;

namespace SocialMedia.Application.Services
{
    public class FollowService : IFollowService
    {
        private static readonly TimeSpan FollowingTtl = TimeSpan.FromDays(7);
        private static readonly TimeSpan UserProfileTtl = TimeSpan.FromDays(7);

        private readonly IFollowRepository _followRepository;
        private readonly IUserGateway _userGateway;
        private readonly IConnectionMultiplexer _redis;
        private readonly IPublishEndpoint _publisher;
        private readonly ILogger<FollowService> _logger;

        public FollowService(
            IFollowRepository followRepository,
            IUserGateway userGateway,
            IConnectionMultiplexer redis,
            IPublishEndpoint publisher,
            ILogger<FollowService> logger)
        {
            _followRepository = followRepository;
            _userGateway = userGateway;
            _redis = redis;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<ApiResponse<FollowResponse>> FollowAsync(
            Guid followerId,
            Guid followingId,
            CancellationToken cancellationToken = default)
        {
            if (followerId == Guid.Empty || followingId == Guid.Empty)
            {
                return ApiResponse<FollowResponse>.Failure(ErrorCode.ValidationError);
            }

            if (followerId == followingId)
            {
                _logger.LogWarning("User {FollowerId} tried to follow themselves", followerId);
                return ApiResponse<FollowResponse>.Failure(ErrorCode.ValidationError);
            }

            var usersById = await _userGateway.GetUsersByIdsAsync(
                new[] { followerId, followingId },
                cancellationToken);

            if (!usersById.ContainsKey(followerId) || !usersById.ContainsKey(followingId))
            {
                _logger.LogWarning(
                    "Follow failed because follower {FollowerId} or target user {FollowingId} does not exist",
                    followerId,
                    followingId);

                return ApiResponse<FollowResponse>.Failure(ErrorCode.NotFound);
            }

            var db = _redis.GetDatabase();
            var followingKey = $"Following:{followerId}";
            var followerProfileKey = $"UserProfile:{followerId}";
            var followingProfileKey = $"UserProfile:{followingId}";

            await HydrateFollowingAsync(db, followerId, followingKey, cancellationToken);

            var wasAdded = await db.SetAddAsync(followingKey, followingId.ToString());
            if (!wasAdded)
            {
                _logger.LogWarning("User {FollowerId} already follows user {FollowingId}", followerId, followingId);
                return ApiResponse<FollowResponse>.Failure(ErrorCode.ValidationError);
            }
            await db.KeyExpireAsync(followingKey, FollowingTtl);

            await HydrateUserProfileAsync(db, followerId, followerProfileKey, cancellationToken);
            await db.HashIncrementAsync(followerProfileKey, "FollowingCount", 1);

            await HydrateUserProfileAsync(db, followingId, followingProfileKey, cancellationToken);
            await db.HashIncrementAsync(followingProfileKey, "FollowersCount", 1);

            var timestamp = DateTime.UtcNow;
            var follow = new Follow
            {
                FollowerId = followerId,
                FolloweeId = followingId,
                CreatedAt = timestamp
            };

            _followRepository.Add(follow);

            try
            {
                await _followRepository.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await RollbackFollowCacheAsync(db, followingKey, followerProfileKey, followingProfileKey, followingId);
                _logger.LogError(ex, "Failed to persist follow relationship from {FollowerId} to {FollowingId}", followerId, followingId);
                return ApiResponse<FollowResponse>.Failure(ErrorCode.DatabaseError);
            }

            try
            {
                await _publisher.Publish(
                    new UserFollowedEvent(followerId, followingId, timestamp),
                    cancellationToken);

                _logger.LogInformation("UserFollowedEvent published for follower {FollowerId} and following {FollowingId}", followerId, followingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish UserFollowedEvent for follower {FollowerId} and following {FollowingId}", followerId, followingId);
            }

            return ApiResponse<FollowResponse>.Success(new FollowResponse
            {
                FollowerId = followerId,
                FollowingId = followingId,
                Timestamp = timestamp
            });
        }

        public async Task<ApiResponse<FollowResponse>> UnfollowAsync(
            Guid followerId,
            Guid followingId,
            CancellationToken cancellationToken = default)
        {
            if (followerId == Guid.Empty || followingId == Guid.Empty || followerId == followingId)
            {
                return ApiResponse<FollowResponse>.Failure(ErrorCode.ValidationError);
            }

            var db = _redis.GetDatabase();
            var followingKey = $"Following:{followerId}";
            var followerProfileKey = $"UserProfile:{followerId}";
            var followingProfileKey = $"UserProfile:{followingId}";

            await HydrateFollowingAsync(db, followerId, followingKey, cancellationToken);

            var wasRemoved = await db.SetRemoveAsync(followingKey, followingId.ToString());
            if (!wasRemoved)
            {
                _logger.LogWarning("User {FollowerId} tried to unfollow user {FollowingId} they do not follow", followerId, followingId);
                return ApiResponse<FollowResponse>.Failure(ErrorCode.ValidationError);
            }

            await HydrateUserProfileAsync(db, followerId, followerProfileKey, cancellationToken);
            await DecrementHashFieldWithoutNegativeAsync(db, followerProfileKey, "FollowingCount");

            await HydrateUserProfileAsync(db, followingId, followingProfileKey, cancellationToken);
            await DecrementHashFieldWithoutNegativeAsync(db, followingProfileKey, "FollowersCount");

            var follow = await _followRepository.GetAsync(followerId, followingId, cancellationToken);
            if (follow is not null)
            {
                _followRepository.Remove(follow);

                try
                {
                    await _followRepository.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    await RollbackUnfollowCacheAsync(db, followingKey, followerProfileKey, followingProfileKey, followingId);
                    _logger.LogError(ex, "Failed to remove follow relationship from {FollowerId} to {FollowingId}", followerId, followingId);
                    return ApiResponse<FollowResponse>.Failure(ErrorCode.DatabaseError);
                }
            }

            var timestamp = DateTime.UtcNow;

            try
            {
                await _publisher.Publish(
                    new UserUnfollowedEvent(followerId, followingId, timestamp),
                    cancellationToken);

                _logger.LogInformation("UserUnfollowedEvent published for follower {FollowerId} and following {FollowingId}", followerId, followingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish UserUnfollowedEvent for follower {FollowerId} and following {FollowingId}", followerId, followingId);
            }

            return ApiResponse<FollowResponse>.Success(new FollowResponse
            {
                FollowerId = followerId,
                FollowingId = followingId,
                Timestamp = timestamp
            });
        }

        public async Task<ApiResponse<PagedResult<SocialUserResponseDto>>> GetFollowersAsync(
            Guid userId,
            OffsetPaginationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
            {
                return ApiResponse<PagedResult<SocialUserResponseDto>>.Failure(ErrorCode.ValidationError);
            }

            NormalizePaginationRequest(request);

            var followerIdsPage = await _followRepository.GetFollowerIdsByFolloweePaginatedAsync(
                userId,
                request,
                cancellationToken);

            var users = await BuildSocialUsersPageAsync(followerIdsPage.Items.ToList(), cancellationToken);

            return ApiResponse<PagedResult<SocialUserResponseDto>>.Success(new PagedResult<SocialUserResponseDto>
            {
                Items = users,
                TotalCount = followerIdsPage.TotalCount,
                Page = followerIdsPage.Page,
                PageSize = followerIdsPage.PageSize
            });
        }

        public async Task<ApiResponse<PagedResult<SocialUserResponseDto>>> GetFolloweesAsync(
            Guid userId,
            OffsetPaginationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
            {
                return ApiResponse<PagedResult<SocialUserResponseDto>>.Failure(ErrorCode.ValidationError);
            }

            NormalizePaginationRequest(request);

            var followeeIdsPage = await _followRepository.GetFolloweeIdsByFollowerPaginatedAsync(
                userId,
                request,
                cancellationToken);

            var users = await BuildSocialUsersPageAsync(followeeIdsPage.Items.ToList(), cancellationToken);

            return ApiResponse<PagedResult<SocialUserResponseDto>>.Success(new PagedResult<SocialUserResponseDto>
            {
                Items = users,
                TotalCount = followeeIdsPage.TotalCount,
                Page = followeeIdsPage.Page,
                PageSize = followeeIdsPage.PageSize
            });
        }

        public async Task<ApiResponse<PagedResult<SocialUserResponseDto>>> GetSocialUsersAsync(
            OffsetPaginationRequest request,
            CancellationToken cancellationToken = default)
        {
            NormalizePaginationRequest(request);

            var usersPage = await _userGateway.GetUsersPaginatedAsync(request, cancellationToken);
            var users = usersPage.Items.ToList();
            var counters = await GetUserProfileCountersAsync(users.Select(user => user.Id).ToList(), cancellationToken);

            return ApiResponse<PagedResult<SocialUserResponseDto>>.Success(new PagedResult<SocialUserResponseDto>
            {
                Items = users.Select(user =>
                {
                    var counts = counters.GetValueOrDefault(user.Id);
                    return new SocialUserResponseDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        FollowersCount = counts.FollowersCount,
                        FollowingCount = counts.FollowingCount
                    };
                }).ToList(),
                TotalCount = usersPage.TotalCount,
                Page = usersPage.Page,
                PageSize = usersPage.PageSize
            });
        }

        public async Task<ApiResponse<SocialUserResponseDto>> GetSocialUserByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
            {
                return ApiResponse<SocialUserResponseDto>.Failure(ErrorCode.ValidationError);
            }

            var users = await BuildSocialUsersPageAsync(new[] { userId }, cancellationToken);
            var user = users.FirstOrDefault();
            if (user is null)
            {
                return ApiResponse<SocialUserResponseDto>.Failure(ErrorCode.NotFound);
            }

            return ApiResponse<SocialUserResponseDto>.Success(user);
        }

        private async Task<List<SocialUserResponseDto>> BuildSocialUsersPageAsync(
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken)
        {
            if (userIds.Count == 0)
            {
                return new List<SocialUserResponseDto>();
            }

            var usersById = await _userGateway.GetUsersByIdsAsync(userIds, cancellationToken);
            var counters = await GetUserProfileCountersAsync(userIds, cancellationToken);

            return userIds
                .Where(usersById.ContainsKey)
                .Select(userId =>
                {
                    var user = usersById[userId];
                    var counts = counters.GetValueOrDefault(userId);

                    return new SocialUserResponseDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        FollowersCount = counts.FollowersCount,
                        FollowingCount = counts.FollowingCount
                    };
                })
                .ToList();
        }

        private async Task<Dictionary<Guid, UserProfileCounters>> GetUserProfileCountersAsync(
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken)
        {
            var distinctUserIds = userIds.Distinct().ToList();
            if (distinctUserIds.Count == 0)
            {
                return new Dictionary<Guid, UserProfileCounters>();
            }

            var db = _redis.GetDatabase();
            var batch = db.CreateBatch();
            var profileTasks = distinctUserIds.ToDictionary(
                userId => userId,
                userId => batch.HashGetAllAsync($"UserProfile:{userId}"));

            batch.Execute();
            await Task.WhenAll(profileTasks.Values);

            var counters = new Dictionary<Guid, UserProfileCounters>();
            var missingUserIds = new List<Guid>();

            foreach (var (userId, task) in profileTasks)
            {
                if (TryMapUserProfileCounters(await task, out var profileCounters))
                {
                    counters[userId] = profileCounters;
                    continue;
                }

                missingUserIds.Add(userId);
            }

            if (missingUserIds.Count == 0)
            {
                return counters;
            }

            _logger.LogDebug(
                "Cache miss for {Count} UserProfile hashes; hydrating counters from DB",
                missingUserIds.Count);

            var followersCounts = await _followRepository.CountFollowersByUserIdsAsync(missingUserIds, cancellationToken);
            var followingCounts = await _followRepository.CountFollowingByUserIdsAsync(missingUserIds, cancellationToken);

            var hydrateBatch = db.CreateBatch();
            var hydrateTasks = new List<Task>(missingUserIds.Count * 2);

            foreach (var userId in missingUserIds)
            {
                var profileCounters = new UserProfileCounters(
                    followersCounts.GetValueOrDefault(userId),
                    followingCounts.GetValueOrDefault(userId));

                counters[userId] = profileCounters;

                var key = (RedisKey)$"UserProfile:{userId}";
                hydrateTasks.Add(hydrateBatch.HashSetAsync(key, new HashEntry[]
                {
                    new("FollowersCount", profileCounters.FollowersCount),
                    new("FollowingCount", profileCounters.FollowingCount)
                }));
                hydrateTasks.Add(hydrateBatch.KeyExpireAsync(key, UserProfileTtl));
            }

            hydrateBatch.Execute();
            await Task.WhenAll(hydrateTasks);

            return counters;
        }

        private static bool TryMapUserProfileCounters(
            HashEntry[] hashEntries,
            out UserProfileCounters counters)
        {
            counters = default;

            if (hashEntries.Length == 0)
            {
                return false;
            }

            var fields = hashEntries.ToDictionary(
                entry => entry.Name.ToString(),
                entry => entry.Value);

            if (!int.TryParse(fields.GetValueOrDefault("FollowersCount").ToString(), out var followersCount))
            {
                return false;
            }

            if (!int.TryParse(fields.GetValueOrDefault("FollowingCount").ToString(), out var followingCount))
            {
                return false;
            }

            counters = new UserProfileCounters(followersCount, followingCount);
            return true;
        }

        private static void NormalizePaginationRequest(OffsetPaginationRequest request)
        {
            request.Page = request.Page <= 0 ? 1 : request.Page;
            request.PageSize = request.PageSize <= 0 ? 10 : request.PageSize;
        }

        private async Task HydrateFollowingAsync(
            IDatabase db,
            Guid followerId,
            RedisKey followingKey,
            CancellationToken cancellationToken)
        {
            if (await db.KeyExistsAsync(followingKey))
            {
                return;
            }

            _logger.LogDebug("Cache miss for Following:{FollowerId}; hydrating from DB", followerId);

            var followeeIds = await _followRepository.GetFolloweeIdsByFollowerAsync(followerId, cancellationToken);
            if (followeeIds.Count > 0)
            {
                var redisValues = followeeIds.Select(id => (RedisValue)id.ToString()).ToArray();
                await db.SetAddAsync(followingKey, redisValues);
            }

            await db.KeyExpireAsync(followingKey, FollowingTtl);
        }

        private readonly record struct UserProfileCounters(int FollowersCount, int FollowingCount);

        private async Task HydrateUserProfileAsync(
            IDatabase db,
            Guid userId,
            RedisKey profileKey,
            CancellationToken cancellationToken)
        {
            if (await db.KeyExistsAsync(profileKey))
            {
                return;
            }

            _logger.LogDebug("Cache miss for UserProfile:{UserId}; hydrating counters from DB", userId);

            var followersCount = await _followRepository.CountFollowersAsync(userId, cancellationToken);
            var followingCount = await _followRepository.CountFollowingAsync(userId, cancellationToken);

            await db.HashSetAsync(profileKey, new HashEntry[]
            {
                new("FollowersCount", followersCount),
                new("FollowingCount", followingCount)
            });

            await db.KeyExpireAsync(profileKey, UserProfileTtl);
        }

        private static async Task DecrementHashFieldWithoutNegativeAsync(
            IDatabase db,
            RedisKey key,
            RedisValue field)
        {
            var count = await db.HashDecrementAsync(key, field, 1);
            if (count < 0)
            {
                await db.HashSetAsync(key, field, 0);
            }
        }

        private static async Task RollbackFollowCacheAsync(
            IDatabase db,
            RedisKey followingKey,
            RedisKey followerProfileKey,
            RedisKey followingProfileKey,
            Guid followingId)
        {
            await db.SetRemoveAsync(followingKey, followingId.ToString());

            var followingCount = await db.HashDecrementAsync(followerProfileKey, "FollowingCount", 1);
            if (followingCount < 0)
            {
                await db.HashSetAsync(followerProfileKey, "FollowingCount", 0);
            }

            var followersCount = await db.HashDecrementAsync(followingProfileKey, "FollowersCount", 1);
            if (followersCount < 0)
            {
                await db.HashSetAsync(followingProfileKey, "FollowersCount", 0);
            }
        }

        private static async Task RollbackUnfollowCacheAsync(
            IDatabase db,
            RedisKey followingKey,
            RedisKey followerProfileKey,
            RedisKey followingProfileKey,
            Guid followingId)
        {
            await db.SetAddAsync(followingKey, followingId.ToString());
            await db.HashIncrementAsync(followerProfileKey, "FollowingCount", 1);
            await db.HashIncrementAsync(followingProfileKey, "FollowersCount", 1);
        }
    }
}
