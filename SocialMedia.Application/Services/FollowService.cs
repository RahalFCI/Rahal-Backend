using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Events.Users;
using Shared.Domain.Enums;
using SocialMedia.Application.DTOs.Follows;
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
        private readonly IConnectionMultiplexer _redis;
        private readonly IPublishEndpoint _publisher;
        private readonly ILogger<FollowService> _logger;

        public FollowService(
            IFollowRepository followRepository,
            IConnectionMultiplexer redis,
            IPublishEndpoint publisher,
            ILogger<FollowService> logger)
        {
            _followRepository = followRepository;
            _redis = redis;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<ApiResponse<FollowResponse>> FollowAsync(
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

            var wasAdded = await db.SetAddAsync(followingKey, followingId.ToString());
            if (!wasAdded)
            {
                _logger.LogWarning("User {FollowerId} already follows user {FollowingId}", followerId, followingId);
                return ApiResponse<FollowResponse>.Failure(ErrorCode.ValidationError);
            }

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
