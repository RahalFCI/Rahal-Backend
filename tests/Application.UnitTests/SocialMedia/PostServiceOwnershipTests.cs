using Application.UnitTests.Common;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using SocialMedia.Application.DTOs.Comments;
using SocialMedia.Application.DTOs.Posts;
using SocialMedia.Application.Interfaces;
using SocialMedia.Application.Services;
using SocialMedia.Domain.Entities;
using StackExchange.Redis;

namespace Application.UnitTests.SocialMedia;

public class PostServiceOwnershipTests
{
    private readonly ISocialMediaRepository<Post> _posts = Substitute.For<ISocialMediaRepository<Post>>();
    private readonly ISocialMediaRepository<Comment> _comments = Substitute.For<ISocialMediaRepository<Comment>>();
    private readonly ILikeRepository _likes = Substitute.For<ILikeRepository>();
    private readonly IFollowRepository _follows = Substitute.For<IFollowRepository>();
    private readonly IUserGateway _users = Substitute.For<IUserGateway>();
    private readonly IConnectionMultiplexer _redis = Substitute.For<IConnectionMultiplexer>();
    private readonly IPublishEndpoint _publisher = Substitute.For<IPublishEndpoint>();
    private readonly IObjectStorageService _storage = Substitute.For<IObjectStorageService>();

    [Fact]
    public async Task DeletePostAsync_ReturnsUnauthorized_WhenExplorerDoesNotOwnPost()
    {
        // Arrange: the post exists, but belongs to another explorer.
        var ownerId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var post = new Post { Id = Guid.NewGuid(), UserId = ownerId, Content = "owned post" };
        _posts.GetTable().Returns(AsyncQueryable.From(post));

        var service = CreateService();

        // Act: a non-owner tries to delete the post.
        var result = await service.DeletePostAsync(post.Id, callerId);

        // Assert: the service blocks the mutation before cache or database writes.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.Unauthorized);
        _posts.DidNotReceiveWithAnyArgs().Update(default!);
        await _posts.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
        _redis.DidNotReceiveWithAnyArgs().GetDatabase();
    }

    [Fact]
    public async Task CreatePostAsync_ReturnsValidationError_WhenContentAndMediaAreEmpty()
    {
        // Arrange: a post must have either text content or media.
        var service = CreateService();

        // Act: create an empty post.
        var result = await service.CreatePostAsync(new CreatePostRequest(), Guid.NewGuid());

        // Assert: no repository write is attempted.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.ValidationError);
        _posts.DidNotReceiveWithAnyArgs().Add(default!);
    }

    [Fact]
    public async Task CreatePostAsync_ReturnsValidationError_WhenMediaCountExceedsLimit()
    {
        // Arrange: the current service limit is three media items.
        var service = CreateService();

        // Act: create a post with too many media ids.
        var result = await service.CreatePostAsync(new CreatePostRequest
        {
            Content = "hello",
            MediaIds = new List<string> { "1", "2", "3", "4" }
        }, Guid.NewGuid());

        // Assert: validation stops before Redis pending-media checks.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.ValidationError);
        _redis.DidNotReceiveWithAnyArgs().GetDatabase();
        _posts.DidNotReceiveWithAnyArgs().Add(default!);
    }

    [Fact]
    public async Task CreatePostAsync_PersistsCachesAndPublishes_WhenTextPostIsValid()
    {
        // Arrange: text-only posts do not need pending-media validation.
        var userId = Guid.NewGuid();
        Post? addedPost = null;
        _posts.When(repository => repository.Add(Arg.Any<Post>()))
            .Do(call => addedPost = call.Arg<Post>());

        var db = SetupRedisDatabase();
        db.HashSetAsync(Arg.Any<RedisKey>(), Arg.Any<HashEntry[]>(), Arg.Any<CommandFlags>())
            .Returns(Task.FromResult(true));
        db.KeyExpireAsync(Arg.Any<RedisKey>(), Arg.Any<TimeSpan?>(), Arg.Any<CommandFlags>())
            .Returns(Task.FromResult(true));
        db.SortedSetLengthAsync(
                Arg.Any<RedisKey>(),
                Arg.Any<double>(),
                Arg.Any<double>(),
                Arg.Any<Exclude>(),
                Arg.Any<CommandFlags>())
            .Returns(Task.FromResult(1L));
        db.SortedSetAddAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<double>(), Arg.Any<When>(), Arg.Any<CommandFlags>())
            .Returns(Task.FromResult(true));
        db.SortedSetRemoveRangeByRankAsync(Arg.Any<RedisKey>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CommandFlags>())
            .Returns(Task.FromResult(0L));

        var service = CreateService();

        // Act: create a valid text post.
        var result = await service.CreatePostAsync(new CreatePostRequest { Content = "hello world" }, userId);

        // Assert: the post is persisted and returned to the caller.
        result.IsSuccess.Should().BeTrue();
        addedPost.Should().NotBeNull();
        addedPost!.UserId.Should().Be(userId);
        addedPost.Content.Should().Be("hello world");
        await _posts.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPostByIdAsync_ReturnsCachedPost_WithLikedFlagAndAuthorName()
    {
        // Arrange: a complete Redis post hash should avoid the DB read model query.
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var createdAt = new DateTimeOffset(new DateTime(2026, 7, 4, 12, 0, 0, DateTimeKind.Utc)).ToUnixTimeSeconds();

        var db = SetupRedisDatabase();
        db.HashGetAllAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
            .Returns(new[]
            {
                new HashEntry("Id", postId.ToString()),
                new HashEntry("Content", "cached content"),
                new HashEntry("AuthorId", authorId.ToString()),
                new HashEntry("LikesCount", 3),
                new HashEntry("CommentsCount", 2),
                new HashEntry("CreatedAt", createdAt),
                new HashEntry("IsPublic", "True"),
                new HashEntry("MediaUrls", "[]")
            });
        db.KeyExistsAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(true));
        db.SetContainsAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(true));
        _users.GetUserDisplayNamesAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string> { [authorId] = "Author Name" });

        var service = CreateService();

        // Act: fetch the post by id.
        var result = await service.GetPostByIdAsync(postId, viewerId);

        // Assert: cached post fields are mapped with the viewer's like state.
        result.IsSuccess.Should().BeTrue();
        result.Data.Id.Should().Be(postId);
        result.Data.AuthorName.Should().Be("Author Name");
        result.Data.IsLikedByThisUser.Should().BeTrue();
        _posts.DidNotReceiveWithAnyArgs().GetTable();
    }

    [Fact]
    public async Task GetPostByIdAsync_ReturnsNotFound_WhenCacheMissAndDbMiss()
    {
        // Arrange: neither Redis nor the DB has the post.
        var db = SetupRedisDatabase();
        db.HashGetAllAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(Array.Empty<HashEntry>());
        _posts.GetTable().Returns(AsyncQueryable.From<Post>());

        var service = CreateService();

        // Act: fetch a missing post.
        var result = await service.GetPostByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert: the caller gets a not-found response.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task DeletePostAsync_ReturnsNotFound_WhenPostIsAlreadyDeleted()
    {
        // Arrange: deleted posts cannot be deleted again.
        var post = new Post { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), IsDeleted = true };
        _posts.GetTable().Returns(AsyncQueryable.From(post));

        var service = CreateService();

        // Act: try to delete an already deleted post.
        var result = await service.DeletePostAsync(post.Id, post.UserId);

        // Assert: no cache invalidation or save happens.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.NotFound);
        _redis.DidNotReceiveWithAnyArgs().GetDatabase();
        await _posts.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task DeletePostAsync_AllowsAdmin_ToDeleteAnyPost()
    {
        // Arrange: admins are allowed to delete posts they do not own.
        var post = new Post { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Content = "admin deleted" };
        _posts.GetTable().Returns(AsyncQueryable.From(post));
        SetupRedisForPostDelete();

        var service = CreateService();

        // Act: an admin deletes another user's post.
        var result = await service.DeletePostAsync(post.Id, Guid.NewGuid(), isAdmin: true);

        // Assert: the post is soft-deleted and persisted.
        result.IsSuccess.Should().BeTrue();
        post.IsDeleted.Should().BeTrue();
        post.DeletedAt.Should().NotBeNull();
        _posts.Received(1).Update(post);
        await _posts.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LikePostAsync_ReturnsValidationError_WhenUserAlreadyLikedPostInCache()
    {
        // Arrange: Redis SADD returning false means the like already exists.
        var db = SetupRedisDatabase();
        db.KeyExistsAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(true));
        db.SetAddAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(false));

        var service = CreateService();

        // Act: like a post already present in the user's like set.
        var result = await service.LikePostAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert: duplicate likes are rejected before DB writes.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.ValidationError);
        _likes.DidNotReceiveWithAnyArgs().Add(default!);
    }

    [Fact]
    public async Task UnlikePostAsync_ReturnsValidationError_WhenUserLikeIsMissingFromCache()
    {
        // Arrange: Redis SREM returning false means the user had not liked the post.
        var db = SetupRedisDatabase();
        db.KeyExistsAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(true));
        db.SetRemoveAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(false));

        var service = CreateService();

        // Act: unlike a post that was not liked.
        var result = await service.UnlikePostAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert: no DB relationship lookup/removal happens.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.ValidationError);
        await _likes.DidNotReceiveWithAnyArgs().GetAsync(default, default, default);
    }

    [Fact]
    public async Task UnlikePostAsync_RemovesLike_WhenCacheAndDbRelationshipExist()
    {
        // Arrange: Redis removes the liked post and the DB like row exists.
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var like = new Like { PostId = postId, UserId = userId };
        _likes.GetAsync(userId, postId, Arg.Any<CancellationToken>()).Returns(like);

        var db = SetupRedisDatabase();
        db.KeyExistsAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(true));
        db.SetRemoveAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(true));
        db.SetLengthAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(1L));
        db.HashDecrementAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<long>(), Arg.Any<CommandFlags>())
            .Returns(Task.FromResult(0L));

        var service = CreateService();

        // Act: unlike the post.
        var result = await service.UnlikePostAsync(postId, userId);

        // Assert: the like relationship is removed from the repository.
        result.IsSuccess.Should().BeTrue();
        _likes.Received(1).Remove(like);
        await _likes.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EditCommentAsync_ReturnsUnauthorized_WhenExplorerDoesNotOwnComment()
    {
        // Arrange: the target comment belongs to another explorer.
        var comment = new Comment { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Content = "original" };
        _comments.GetByIdAsync(comment.Id, Arg.Any<CancellationToken>()).Returns(comment);

        var service = CreateService();

        // Act: a different explorer tries to edit it.
        var result = await service.EditCommentAsync(
            comment.Id,
            Guid.NewGuid(),
            new EditCommentRequest { Content = "changed" });

        // Assert: the comment is untouched.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.Unauthorized);
        comment.Content.Should().Be("original");
        _comments.DidNotReceiveWithAnyArgs().Update(default!);
        await _comments.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task EditCommentAsync_ReturnsNotFound_WhenCommentIsDeleted()
    {
        // Arrange: deleted comments are not editable.
        var comment = new Comment { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), IsDeleted = true };
        _comments.GetByIdAsync(comment.Id, Arg.Any<CancellationToken>()).Returns(comment);

        var service = CreateService();

        // Act: try to edit a deleted comment.
        var result = await service.EditCommentAsync(
            comment.Id,
            comment.UserId,
            new EditCommentRequest { Content = "new" });

        // Assert: the service returns not found and leaves the entity unchanged.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.NotFound);
        _comments.DidNotReceiveWithAnyArgs().Update(default!);
    }

    [Fact]
    public async Task EditCommentAsync_UpdatesContent_WhenExplorerOwnsComment()
    {
        // Arrange: the comment owner is the caller.
        var userId = Guid.NewGuid();
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PostId = Guid.NewGuid(),
            Content = "old"
        };
        _comments.GetByIdAsync(comment.Id, Arg.Any<CancellationToken>()).Returns(comment);

        var service = CreateService();

        // Act: the owner edits the comment.
        var result = await service.EditCommentAsync(
            comment.Id,
            userId,
            new EditCommentRequest { Content = "new" });

        // Assert: the edited content is saved and returned.
        result.IsSuccess.Should().BeTrue();
        comment.Content.Should().Be("new");
        comment.UpdatedAt.Should().NotBeNull();
        result.Data.Content.Should().Be("new");
        _comments.Received(1).Update(comment);
        await _comments.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteCommentAsync_ReturnsUnauthorized_WhenExplorerDoesNotOwnComment()
    {
        // Arrange: the comment exists, but belongs to someone else.
        var comment = new Comment { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PostId = Guid.NewGuid() };
        _comments.GetByIdAsync(comment.Id, Arg.Any<CancellationToken>()).Returns(comment);

        var service = CreateService();

        // Act: a non-owner tries to delete the comment.
        var result = await service.DeleteCommentAsync(comment.Id, Guid.NewGuid());

        // Assert: no soft-delete or cache update is performed.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.Unauthorized);
        comment.IsDeleted.Should().BeFalse();
        _comments.DidNotReceiveWithAnyArgs().Update(default!);
        await _comments.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
        _redis.DidNotReceiveWithAnyArgs().GetDatabase();
    }

    [Fact]
    public async Task DeleteCommentAsync_ReturnsNotFound_WhenCommentDoesNotExist()
    {
        // Arrange: the repository cannot find the comment.
        _comments.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Comment?)null);
        var service = CreateService();

        // Act: delete a missing comment.
        var result = await service.DeleteCommentAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert: the service exits before cache and persistence.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.NotFound);
        _redis.DidNotReceiveWithAnyArgs().GetDatabase();
        await _comments.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task DeleteCommentAsync_AllowsAdmin_ToDeleteAnyComment()
    {
        // Arrange: admins can moderate comments across users.
        var comment = new Comment { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PostId = Guid.NewGuid() };
        _comments.GetByIdAsync(comment.Id, Arg.Any<CancellationToken>()).Returns(comment);
        SetupRedisForCommentDelete();

        var service = CreateService();

        // Act: an admin deletes another user's comment.
        var result = await service.DeleteCommentAsync(comment.Id, Guid.NewGuid(), isAdmin: true);

        // Assert: the comment is soft-deleted and the write is persisted.
        result.IsSuccess.Should().BeTrue();
        comment.IsDeleted.Should().BeTrue();
        _comments.Received(1).Update(comment);
        await _comments.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCommentAsync_ReturnsNotFound_WhenPostIsMissing()
    {
        // Arrange: the post is not cached and cannot be found in the DB.
        var db = SetupRedisDatabase();
        db.KeyExistsAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(false));
        _posts.GetTable().Returns(AsyncQueryable.From<Post>());

        var service = CreateService();

        // Act: comment on a missing post.
        var result = await service.CreateCommentAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new CreateCommentRequest { Content = "hello" });

        // Assert: no comment is inserted.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.NotFound);
        _comments.DidNotReceiveWithAnyArgs().Add(default!);
    }

    [Fact]
    public async Task CreateCommentAsync_AddsRootComment_WhenPostCacheExists()
    {
        // Arrange: the post hash already exists, so no post hydration is required.
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        Comment? addedComment = null;
        _comments.When(repository => repository.Add(Arg.Any<Comment>()))
            .Do(call => addedComment = call.Arg<Comment>());

        var db = SetupRedisDatabase();
        db.KeyExistsAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(true));
        db.HashIncrementAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<long>(), Arg.Any<CommandFlags>())
            .Returns(Task.FromResult(1L));
        db.HashGetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<CommandFlags>())
            .Returns(authorId.ToString());

        var service = CreateService();

        // Act: create a root comment.
        var result = await service.CreateCommentAsync(
            postId,
            userId,
            new CreateCommentRequest { Content = "nice post" });

        // Assert: the new comment is staged and saved.
        result.IsSuccess.Should().BeTrue();
        addedComment.Should().NotBeNull();
        addedComment!.PostId.Should().Be(postId);
        addedComment.UserId.Should().Be(userId);
        addedComment.Content.Should().Be("nice post");
        await _comments.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetRootCommentsAsync_MapsDeletedCommentsWithoutUserLookup()
    {
        // Arrange: deleted comments with replies are shown as placeholders.
        var postId = Guid.NewGuid();
        var activeUserId = Guid.NewGuid();
        _comments.GetTable().Returns(AsyncQueryable.From(
            new Comment
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = activeUserId,
                Content = "active",
                CreatedAt = new DateTime(2026, 7, 4, 10, 0, 0, DateTimeKind.Utc)
            },
            new Comment
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = Guid.NewGuid(),
                Content = "deleted",
                IsDeleted = true,
                RepliesCount = 1,
                CreatedAt = new DateTime(2026, 7, 4, 9, 0, 0, DateTimeKind.Utc)
            }));
        _users.GetUserDisplayNamesAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string> { [activeUserId] = "Active User" });

        var service = CreateService();

        // Act: fetch root comments.
        var result = await service.GetRootCommentsAsync(postId, cursor: null);

        // Assert: active comments get names, deleted comments get placeholders.
        result.IsSuccess.Should().BeTrue();
        result.Data.Comments.Should().HaveCount(2);
        result.Data.Comments.Should().Contain(comment => comment.UserDisplayName == "Active User");
        result.Data.Comments.Should().Contain(comment => comment.Content == "[Deleted]");
    }

    [Fact]
    public async Task GetFeedPaginatedAsync_ReturnsEmptyPage_WhenUserHasNoFeedPosts()
    {
        // Arrange: cache miss hydration and DB fallback both find no posts.
        var db = SetupRedisDatabase();
        db.SortedSetLengthAsync(
                Arg.Any<RedisKey>(),
                Arg.Any<double>(),
                Arg.Any<double>(),
                Arg.Any<Exclude>(),
                Arg.Any<CommandFlags>())
            .Returns(Task.FromResult(0L));
        db.SortedSetRangeByScoreAsync(
                Arg.Any<RedisKey>(),
                Arg.Any<double>(),
                Arg.Any<double>(),
                Arg.Any<Exclude>(),
                Arg.Any<Order>(),
                Arg.Any<long>(),
                Arg.Any<long>(),
                Arg.Any<CommandFlags>())
            .Returns(Array.Empty<RedisValue>());
        _follows.GetFolloweeIdsByFollowerAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(new List<Guid>());
        _posts.GetTable().Returns(AsyncQueryable.From<Post>());

        var service = CreateService();

        // Act: request the feed.
        var result = await service.GetFeedPaginatedAsync(Guid.NewGuid());

        // Assert: the response is a successful empty page.
        result.IsSuccess.Should().BeTrue();
        result.Data.Posts.Should().BeEmpty();
        result.Data.NextCursor.Should().BeNull();
    }

    private PostService CreateService()
    {
        return new PostService(
            _posts,
            _comments,
            _likes,
            _follows,
            _users,
            _redis,
            _publisher,
            NullLogger<PostService>.Instance,
            _storage);
    }

    private void SetupRedisForPostDelete()
    {
        var db = Substitute.For<IDatabase>();
        var batch = Substitute.For<IBatch>();

        _redis.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(db);
        db.CreateBatch(Arg.Any<object?>()).Returns(batch);
        batch.KeyDeleteAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(true));
        batch.SortedSetRemoveAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(true));
    }

    private void SetupRedisForCommentDelete()
    {
        var db = Substitute.For<IDatabase>();

        _redis.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(db);
        db.KeyExistsAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(true));
        db.HashDecrementAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<long>(), Arg.Any<CommandFlags>())
            .Returns(Task.FromResult(0L));
    }

    private IDatabase SetupRedisDatabase()
    {
        var db = Substitute.For<IDatabase>();
        _redis.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(db);
        return db;
    }
}
