using System.Text.Json;
using FluentAssertions;
using NSubstitute;
using Notifications.Application.Extensions;
using Notifications.Application.Interfaces;
using Notifications.Application.Services;
using Notifications.Domain.Entities;
using Notifications.Domain.Enums;
using Shared.Domain.Enums;
using Users.Contracts.DTOs;
using Users.Contracts.Interfaces;

namespace Application.UnitTests.Notifications;

public class NotificationServiceTests
{
    private readonly INotificationRepository _notifications = Substitute.For<INotificationRepository>();
    private readonly IUsersPublicApi _usersPublicApi = Substitute.For<IUsersPublicApi>();

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsUnauthorized_WhenUserIdIsEmpty()
    {
        // Arrange: empty user ids are not valid authenticated callers.
        var service = CreateService();

        // Act: request an unread count without a user id.
        var result = await service.GetUnreadCountAsync(Guid.Empty);

        // Assert: the repository is not queried for invalid callers.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.Unauthorized);
        await _notifications.DidNotReceiveWithAnyArgs().GetUnreadCountAsync(default, default);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsRepositoryCount()
    {
        // Arrange: the repository owns the unread-count query.
        var userId = Guid.NewGuid();
        _notifications.GetUnreadCountAsync(userId, Arg.Any<CancellationToken>()).Returns(7);
        var service = CreateService();

        // Act: request the unread count.
        var result = await service.GetUnreadCountAsync(userId);

        // Assert: the count is mapped directly to the response DTO.
        result.IsSuccess.Should().BeTrue();
        result.Data.UnreadCount.Should().Be(7);
    }

    [Fact]
    public async Task GetUserNotificationsPaginatedAsync_ClampsLimit_AndBuildsMessagesWithActorNames()
    {
        // Arrange: notification rows keep normalized ids, while actor names are resolved for display text.
        var userId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var metadata = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>("""{"preview":"Nice trip"}""");

        _notifications.GetUserNotificationsAsync(userId, Arg.Any<DateTime>(), 100, Arg.Any<CancellationToken>())
            .Returns(new List<Notification>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ActorId = actorId,
                    Type = NotificationType.SocialPostComment.ToStoredValue(),
                    TargetId = Guid.NewGuid().ToString(),
                    Metadata = metadata,
                    CreatedAt = new DateTime(2026, 7, 4, 10, 0, 0, DateTimeKind.Utc)
                }
            });

        _usersPublicApi.GetUsersDisplayNamesAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { new UserPublicDto { Id = actorId, DisplayName = "Mona" } });

        var service = CreateService();

        // Act: ask for more than the allowed page size.
        var result = await service.GetUserNotificationsPaginatedAsync(userId, limit: 500);

        // Assert: the service caps the page size and returns a frontend-ready message.
        result.IsSuccess.Should().BeTrue();
        result.Data.Notifications.Should().ContainSingle()
            .Which.Message.Should().Be("Mona commented: Nice trip...");
        result.Data.NextCursor.Should().Be(new DateTime(2026, 7, 4, 10, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task MarkAsReadAsync_MarksAll_WhenNotificationIdIsMissing()
    {
        // Arrange: null notification id means "mark all" for this user.
        var userId = Guid.NewGuid();
        var service = CreateService();

        // Act: mark all notifications as read.
        var result = await service.MarkAsReadAsync(userId);

        // Assert: the bulk repository operation is used.
        result.IsSuccess.Should().BeTrue();
        await _notifications.Received(1).MarkAllAsReadAsync(userId, Arg.Any<CancellationToken>());
        await _notifications.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task MarkAsReadAsync_ReturnsNotFound_WhenNotificationBelongsToAnotherUser()
    {
        // Arrange: a user cannot mark another user's notification as read.
        var userId = Guid.NewGuid();
        _notifications.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new Notification { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), IsRead = false });

        var service = CreateService();

        // Act: try to mark a foreign notification.
        var result = await service.MarkAsReadAsync(userId, Guid.NewGuid());

        // Assert: no mutation is persisted.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.NotFound);
        await _notifications.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task MarkAsReadAsync_SavesOnlyWhenNotificationWasUnread()
    {
        // Arrange: an unread notification owned by the user should be updated.
        var userId = Guid.NewGuid();
        var notification = new Notification { Id = Guid.NewGuid(), UserId = userId, IsRead = false };
        _notifications.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>()).Returns(notification);

        var service = CreateService();

        // Act: mark the notification as read.
        var result = await service.MarkAsReadAsync(userId, notification.Id);

        // Assert: the entity is changed and saved once.
        result.IsSuccess.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
        await _notifications.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetFcmTokenAsync_TrimsAndUpsertsToken()
    {
        // Arrange: the API may pass whitespace around the token.
        var userId = Guid.NewGuid();
        var service = CreateService();

        // Act: save the FCM token.
        var result = await service.SetFcmTokenAsync(userId, "  token-123  ");

        // Assert: a clean token is stored.
        result.IsSuccess.Should().BeTrue();
        await _notifications.Received(1).UpsertFcmTokenAsync(userId, "token-123", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetFcmTokenAsync_ReturnsValidationError_WhenTokenIsBlank()
    {
        // Arrange: blank tokens should never overwrite a real device token.
        var service = CreateService();

        // Act: try to save a blank token.
        var result = await service.SetFcmTokenAsync(Guid.NewGuid(), "   ");

        // Assert: the repository is not called.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.ValidationError);
        await _notifications.DidNotReceiveWithAnyArgs().UpsertFcmTokenAsync(default, default!, default);
    }

    private NotificationService CreateService()
    {
        return new NotificationService(_notifications, _usersPublicApi);
    }
}
