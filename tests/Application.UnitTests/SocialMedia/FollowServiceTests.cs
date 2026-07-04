using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shared.Domain.Enums;
using Shared.Application.Pagination;
using SocialMedia.Application.DTOs.Users;
using SocialMedia.Application.Interfaces;
using SocialMedia.Application.Services;
using SocialMedia.Domain.Entities;
using StackExchange.Redis;

namespace Application.UnitTests.SocialMedia;

public class FollowServiceTests
{
    private readonly IFollowRepository _follows = Substitute.For<IFollowRepository>();
    private readonly IUserGateway _users = Substitute.For<IUserGateway>();
    private readonly IConnectionMultiplexer _redis = Substitute.For<IConnectionMultiplexer>();
    private readonly IPublishEndpoint _publisher = Substitute.For<IPublishEndpoint>();

    [Fact]
    public async Task FollowAsync_ReturnsValidationError_WhenUserFollowsSelf()
    {
        // Arrange: following yourself is not a valid social relationship.
        var userId = Guid.NewGuid();
        var service = CreateService();

        // Act: try to follow the same user id.
        var result = await service.FollowAsync(userId, userId);

        // Assert: validation stops before user lookup, cache, database, or event publishing.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.ValidationError);
        await _users.DidNotReceiveWithAnyArgs().GetUsersByIdsAsync(default!, default);
        _redis.DidNotReceiveWithAnyArgs().GetDatabase();
        _follows.DidNotReceiveWithAnyArgs().Add(default!);
    }

    // UTest
    [Fact]
    public async Task FollowAsync_ReturnsNotFound_WhenTargetUserDoesNotExist()
    {
        // Arrange: the Users module can resolve the follower but not the follow target.
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();
        _users.GetUsersByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, UserGatewayUserDto>
            {
                [followerId] = new() { Id = followerId, Name = "Follower" }
            });

        var service = CreateService();

        // Act: try to follow a missing user.
        var result = await service.FollowAsync(followerId, followingId);

        // Assert: the service fails before mutating Redis or the follow table.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.NotFound);
        _redis.DidNotReceiveWithAnyArgs().GetDatabase();
        _follows.DidNotReceiveWithAnyArgs().Add(default!);
    }

    [Fact]
    public async Task UnfollowAsync_ReturnsValidationError_WhenFollowerAndTargetAreSame()
    {
        // Arrange: unfollow has the same self-relationship guard as follow.
        var userId = Guid.NewGuid();
        var service = CreateService();

        // Act: try to unfollow yourself.
        var result = await service.UnfollowAsync(userId, userId);

        // Assert: no cache or repository calls are made.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.ValidationError);
        _redis.DidNotReceiveWithAnyArgs().GetDatabase();
        await _follows.DidNotReceiveWithAnyArgs().GetAsync(default, default, default);
    }

    [Fact]
    public async Task FollowAsync_ReturnsValidationError_WhenRedisShowsRelationshipAlreadyExists()
    {
        // Arrange: Redis SADD returning false means the follower already follows the target.
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();
        _users.GetUsersByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, UserGatewayUserDto>
            {
                [followerId] = new() { Id = followerId, Name = "Follower" },
                [followingId] = new() { Id = followingId, Name = "Target" }
            });

        var db = SetupRedisDatabase();
        db.KeyExistsAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(true));
        db.SetAddAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(false));

        var service = CreateService();

        // Act: try to follow an already-followed user.
        var result = await service.FollowAsync(followerId, followingId);

        // Assert: the duplicate follow is rejected before persistence.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.ValidationError);
        _follows.DidNotReceiveWithAnyArgs().Add(default!);
        await _follows.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task FollowAsync_PersistsRelationship_WhenCacheMutationSucceeds()
    {
        // Arrange: all users exist and Redis accepts the new follow id.
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();
        Follow? addedFollow = null;

        _users.GetUsersByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, UserGatewayUserDto>
            {
                [followerId] = new() { Id = followerId, Name = "Follower" },
                [followingId] = new() { Id = followingId, Name = "Target" }
            });

        _follows.When(repository => repository.Add(Arg.Any<Follow>()))
            .Do(call => addedFollow = call.Arg<Follow>());

        var db = SetupRedisDatabase();
        db.KeyExistsAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(true));
        db.SetAddAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(true));
        db.KeyExpireAsync(Arg.Any<RedisKey>(), Arg.Any<TimeSpan?>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(true));
        db.HashIncrementAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<long>(), Arg.Any<CommandFlags>())
            .Returns(Task.FromResult(1L));

        var service = CreateService();

        // Act: follow the target user.
        var result = await service.FollowAsync(followerId, followingId);

        // Assert: the relationship is staged and saved.
        result.IsSuccess.Should().BeTrue();
        addedFollow.Should().NotBeNull();
        addedFollow!.FollowerId.Should().Be(followerId);
        addedFollow.FolloweeId.Should().Be(followingId);
        await _follows.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnfollowAsync_ReturnsValidationError_WhenRedisShowsUserWasNotFollowed()
    {
        // Arrange: Redis SREM returning false means no cached relationship existed.
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();
        var db = SetupRedisDatabase();
        db.KeyExistsAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(true));
        db.SetRemoveAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(false));

        var service = CreateService();

        // Act: try to unfollow a user that is not followed.
        var result = await service.UnfollowAsync(followerId, followingId);

        // Assert: the DB relationship is not touched.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.ValidationError);
        await _follows.DidNotReceiveWithAnyArgs().GetAsync(default, default, default);
    }

    [Fact]
    public async Task UnfollowAsync_RemovesRelationship_WhenCacheAndDbRelationshipExist()
    {
        // Arrange: Redis removes the follow id and the DB relationship exists.
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();
        var follow = new Follow { FollowerId = followerId, FolloweeId = followingId };
        _follows.GetAsync(followerId, followingId, Arg.Any<CancellationToken>()).Returns(follow);

        var db = SetupRedisDatabase();
        db.KeyExistsAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(true));
        db.SetRemoveAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(true));
        db.HashDecrementAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<long>(), Arg.Any<CommandFlags>())
            .Returns(Task.FromResult(0L));

        var service = CreateService();

        // Act: unfollow the target user.
        var result = await service.UnfollowAsync(followerId, followingId);

        // Assert: the DB relationship is removed and saved.
        result.IsSuccess.Should().BeTrue();
        _follows.Received(1).Remove(follow);
        await _follows.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetFollowersAsync_ReturnsValidationError_WhenUserIdIsEmpty()
    {
        // Arrange: follower lists require a real profile id.
        var service = CreateService();

        // Act: request followers for an empty id.
        var result = await service.GetFollowersAsync(Guid.Empty, new OffsetPaginationRequest());

        // Assert: no repository query is made.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.ValidationError);
        await _follows.DidNotReceiveWithAnyArgs().GetFollowerIdsByFolloweePaginatedAsync(default, default!, default);
    }

    [Fact]
    public async Task GetFollowersAsync_ReturnsEmptyPage_WhenRepositoryReturnsNoFollowerIds()
    {
        // Arrange: an empty id page should not trigger user or counter hydration.
        var userId = Guid.NewGuid();
        _follows.GetFollowerIdsByFolloweePaginatedAsync(userId, Arg.Any<OffsetPaginationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Guid>
            {
                Items = Array.Empty<Guid>(),
                TotalCount = 0,
                Page = 1,
                PageSize = 10
            });

        var service = CreateService();

        // Act: request followers.
        var result = await service.GetFollowersAsync(userId, new OffsetPaginationRequest { Page = -1, PageSize = -5 });

        // Assert: pagination is normalized and the response is empty.
        result.IsSuccess.Should().BeTrue();
        result.Data.Items.Should().BeEmpty();
        result.Data.Page.Should().Be(1);
        result.Data.PageSize.Should().Be(10);
        await _users.DidNotReceiveWithAnyArgs().GetUsersByIdsAsync(default!, default);
    }

    [Fact]
    public async Task GetFolloweesAsync_ReturnsEmptyPage_WhenRepositoryReturnsNoFolloweeIds()
    {
        // Arrange: an empty followee page is a successful, empty social list.
        var userId = Guid.NewGuid();
        _follows.GetFolloweeIdsByFollowerPaginatedAsync(userId, Arg.Any<OffsetPaginationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Guid>
            {
                Items = Array.Empty<Guid>(),
                TotalCount = 0,
                Page = 1,
                PageSize = 10
            });

        var service = CreateService();

        // Act: request followees.
        var result = await service.GetFolloweesAsync(userId, new OffsetPaginationRequest());

        // Assert: no user lookup is needed for an empty page.
        result.IsSuccess.Should().BeTrue();
        result.Data.Items.Should().BeEmpty();
        await _users.DidNotReceiveWithAnyArgs().GetUsersByIdsAsync(default!, default);
    }

    private FollowService CreateService()
    {
        return new FollowService(
            _follows,
            _users,
            _redis,
            _publisher,
            NullLogger<FollowService>.Instance);
    }

    private IDatabase SetupRedisDatabase()
    {
        var db = Substitute.For<IDatabase>();
        _redis.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(db);
        return db;
    }
}
